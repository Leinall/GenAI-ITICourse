using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.Base.Api.Models.Filters;
using Overwolf.CFCore.Base.Api.Models.Enums;
using UnityEngine.UI;
using System;
using Overwolf.CFCore.Base.Api.Models.Requests;
using System.Collections;
using UnityEngine.Networking;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.UnityUI.Utils;
using Overwolf.CFCore.UnityUI.Data;
using Overwolf.CFCore.Base.Services.Http.Models;
using System.IO;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class UploadModPopup : MonoBehaviour {

    #region Events declaration
    public delegate void ModSentDelegate(Mod mod);
    public event ModSentDelegate ModCreatedEvent;
    public event ModSentDelegate ModUpdatedEvent;

    public delegate void FileUploadedDelegate(UploadedFileInfo uploadedFileInfo);
    public event FileUploadedDelegate FileUploadedEvent;

    public delegate void FileUploadingProgressDelegate(HttpProgress progress);
    public event FileUploadingProgressDelegate FileProgressEvent;

    public delegate void ModUploadFailedDelegate(string error);
    public event ModUploadFailedDelegate ModCreationFailedEvent;
    public event ModUploadFailedDelegate FileUploadFailedEvent;
    #endregion Events declaration

    #region Constants
    private Color errorTextColor = new Color(0.7568628f, 0.1803922f, 0.1803922f);
    private Color normalTextColor = new Color(0.8980392f, 0.8980392f, 0.8980392f);
    private string modUploadStartedToastMessage = "Upload started and added to \"My Creations\"";
    private string modUploadSuccessToastMessage = "Successfully uploaded to CurseForge and \"My creations\"";
    private string modUploadFailedToastMessage = "Upload failed. See \"My creations\" to retry. ";
    private string kErrorCreatingScreenshot = "Thumbnail loading failed!\n";
    private const int kModNameLengthLimit = 16;
    private const int kModSummaryLengthLimit = 110;
    #endregion Constants

    #region UI elements declarations
#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] private GameObject PopupTakeScreenshotPrefab;
    [SerializeField] TextMeshProUGUI Txt_NameHeadline;
    [SerializeField] TextMeshProUGUI Txt_SummaryHeadline;
    [SerializeField] TextMeshProUGUI Txt_ScreenshotHeadline;
    [SerializeField] TMP_InputField Txt_ModName;
    [SerializeField] TextMeshProUGUI Txt_ModNameCharCounter;
    [SerializeField] TMP_InputField Txt_ModSummary;
    [SerializeField] TextMeshProUGUI Txt_ModSummaryCharCounter;
    [SerializeField] RawImage Img_Screenshot;
    [SerializeField] Button BTN_Screenshot;
    [SerializeField] TextMeshProUGUI Txt_ClassHeadline;
    [SerializeField] TMP_Dropdown Drp_Class;
    [SerializeField] TextMeshProUGUI Txt_CategoryHeadline;
    [SerializeField] TMP_Dropdown Drp_Category;
    [SerializeField] Button BTN_UploadMod;
    [SerializeField] GameObject NameErrorGameObject;
    [SerializeField] GameObject SummaryErrorGameObject;
    [SerializeField] GameObject ClassErrorGameObject;
    [SerializeField] GameObject CategoryErrorGameObject;
    [SerializeField] GameObject ThumbnailErrorGameObject;
    [SerializeField] GameObject PopupSignInPrefab;
#pragma warning restore 0649

    #endregion UI elements declarations

    #region Data declarations
    private List<Category> Classes;
    private Dictionary<uint, List<Category>> categoryDictionary;
    private Category selectedClass;
    private Category selectedCategory;
    private GameObject screenShotPopupGameObject;
    private byte[] modLogo;
    private Mod currentMod;
    private UploadCreationOptions options;
    #endregion Data declarations

    #region Populate data Methods

    /// <summary>
    /// Initialize the popup with the data from options
    /// </summary>
    /// <param name="options"></param>
    public void Initialize(UploadCreationOptions options) {
      this.options = options;
      currentMod = options.mod;
      Setup();
      READ_APIManager.Instance.IsAuthenticated();
    }

    public virtual void Setup() {
      Classes = new List<Category>();
      categoryDictionary = new Dictionary<uint, List<Category>>();
      categoryDictionary.Add(0, new List<Category>());
      if (currentMod != null) {
        SetupMod(currentMod);
      }

      if (options.thumbnailTexture != null) {
        var cropedTexture = CropTexture((Texture2D)options.thumbnailTexture);
        modLogo = cropedTexture.EncodeToPNG();
        Img_Screenshot.texture = cropedTexture;
      } else {
        if (Img_Screenshot.texture != null) {
          var cropedTexture = CropTexture((Texture2D)Img_Screenshot.texture);
          Img_Screenshot.texture = cropedTexture;
        }
      }

      SetupCategories();
      Txt_ModName.characterLimit = kModNameLengthLimit;
      Txt_ModSummary.characterLimit = kModSummaryLengthLimit;
      if (!options.isUploadNewThumbnailFlag) {
        BTN_Screenshot.gameObject.SetActive(false);
      }
    }

    public void Update() {
      UpdateCharLimitText(Txt_ModNameCharCounter,
                         Txt_ModName.text.Length,
                          kModNameLengthLimit);
      UpdateCharLimitText(Txt_ModSummaryCharCounter,
                          Txt_ModSummary.text.Length,
                          kModSummaryLengthLimit);
    }

    private void SetupCategories() {
      // get all the classes
      READ_APIManager.Instance.Request_GetCategoriesOfClass(
         new GetCategoriesFilter(), (categoryList) => {
           foreach (Category category in categoryList) {
             if (category.IsClass != null && category.IsClass.Value) {
               Classes.Add(category);
               categoryDictionary.Add(category.Id, new List<Category>());
             }
           }
           // create a dictionary between the class and its categories
           foreach (Category category in categoryList) {
             if ((category.IsClass == null || !category.IsClass.Value) && // not a class
                 (category.ClassId != null)) {  // has a parent class
               if (!categoryDictionary.ContainsKey(category.ClassId.Value)) {
                 Debug.LogError(string.Format("Error : Discrepancy  in database." +
                   " Class {0} doesn't exist although {1} has it as a class",
                   category.ClassId, category.Id));
               } else {
                 categoryDictionary[category.ClassId.Value].Add(category);
               }
             }
           }
           SetupDropDowns();
         });
    }

    private void SetupDropDowns() {
      Drp_Class.ClearOptions();
      List<string> options = new List<string> {
        "Select class"
      };
      foreach (var categoryClass in Classes) {
        options.Add(categoryClass.Name);
      }

      Drp_Class.AddOptions(options);
      if (currentMod != null ) {
        for (int i = 0; i < Classes.Count; i++) {
          if (currentMod.ClassId == Classes[i].Id) {
            selectedClass = Classes[i];
            Drp_Class.SetValueWithoutNotify(i + 1);
          }
        }
      } else {
        selectedClass = null;
        Drp_Class.SetValueWithoutNotify(0);
      }

      if (Classes.Count == 1) {
        Drp_Class.Hide();
      }

      Drp_Class.RefreshShownValue();
      UpdateCategoriesDropdown();

      if (currentMod != null) {
        for (int i = 0; i < categoryDictionary[selectedClass.Id].Count; i++) {
          if (currentMod.PrimaryCategoryId == categoryDictionary[selectedClass.Id][i].Id) {
            selectedCategory = categoryDictionary[selectedClass.Id][i];
            Drp_Category.SetValueWithoutNotify(i + 1);
            Drp_Category.RefreshShownValue();
          }
        }
      }
    }

    private void UpdateCategoriesDropdown() {
      Drp_Category.ClearOptions();
      List<string> categoryNames = new List<string> {
        "Select category"
      };
      if (selectedClass == null) {
        Drp_Category.AddOptions(categoryNames);
        Drp_Category.SetValueWithoutNotify(0);
        Drp_Category.RefreshShownValue();
        return;
      }
      foreach (Category cat in categoryDictionary[selectedClass.Id]) {
        categoryNames.Add(cat.Name);
      }
      Drp_Category.AddOptions(categoryNames);
      Drp_Category.RefreshShownValue();
    }

    public void ClassDropDownValueChanged(TMP_Dropdown change) {

      if (change.value == 0) {
        selectedClass = null;
      } else {
        selectedClass = Classes[change.value - 1];
      }
      UpdateCategoriesDropdown();
    }

    public void CategoryDropDownValueChanged(TMP_Dropdown change) {
      if (change.value == 0) {
        selectedCategory = null;
      } else {
        selectedCategory = categoryDictionary[selectedClass.Id][change.value - 1];
      }
    }

    private void SetupMod(Mod mod) {
      Txt_ModName.text = mod.Name;
      Txt_ModSummary.text = mod.Summary;

      if(options.thumbnailTexture) {
        var cropedTexture = CropTexture((Texture2D)options.thumbnailTexture);
        if (Img_Screenshot != null) {
          Img_Screenshot.texture = cropedTexture;
        }
        modLogo = cropedTexture.EncodeToPNG();
      }
      else if (mod.Logo != null
        && mod.Logo.Url != null) {
        StartCoroutine(Load_Screenshot(mod.Logo.Url));
      }
    }
    #endregion Populate data Methods

    #region Screenshot
    IEnumerator Load_Screenshot(string ScreenshotURL) {
      yield return new WaitUntil(() => { return System.IO.File.Exists(ScreenshotURL); });
      UnityWebRequest request = UnityWebRequestTexture.GetTexture(ScreenshotURL);
      yield return request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
      if (request.result != UnityWebRequest.Result.ProtocolError &&
          request.result != UnityWebRequest.Result.ConnectionError) {
#else
      if (!request.isHttpError && !request.isNetworkError) {
#endif
        yield return new WaitForSeconds(0.25f);
        var cropedTexture = CropTexture((Texture2D)((DownloadHandlerTexture)request.downloadHandler).texture);
        Img_Screenshot.texture = cropedTexture;
        modLogo = cropedTexture.EncodeToPNG();
      } else {
        Debug.LogError(kErrorCreatingScreenshot + request.error);
      }
      request.Dispose();
    }

    // Called from UI
    public void TakeScreenshotButtonClick() {
      gameObject.SetActive(false);
      screenShotPopupGameObject = Instantiate(PopupTakeScreenshotPrefab,
        transform.parent) as GameObject;
      ScreenshotPopup screenShot = screenShotPopupGameObject
        .GetComponentInChildren<ScreenshotPopup>();
      screenShot.Initialize(() =>
        OnScreenshotCompleted(screenShot.screenshotTexture),
        new ScreenShotImpl(READ_APIManager.Instance));
    }

    private void OnScreenshotCompleted(Texture texture) {
      gameObject.SetActive(true);
      if (texture != null) {
        Texture2D cropedTexture = CropTexture((Texture2D)texture);
        Img_Screenshot.texture = cropedTexture;
        modLogo = (cropedTexture.EncodeToPNG());
      }
      GameObject.Destroy(screenShotPopupGameObject);
    }
    #endregion Screenshot

    #region Unity Buttons Callbacks

    // Called from UI
    public void SaveMod() {
      if (!IsReadyToSend()) {
        return;
      }

      if (!READ_APIManager.Instance.IsSignedIn) {
        SignIn();
        return;
      }

      gameObject.SetActive(false);

      ToastManager.Instance.CreateModToast(Txt_ModName.text,
            modUploadStartedToastMessage, Img_Screenshot.texture);
      var updateDto = GenerateDto<UpdateModRequestDto>();
      if (options.isEditMod) {
        READ_APIManager.Instance.UpdateMod(currentMod, updateDto, modLogo,
          (mod) => {
            ToastManager.Instance.CreateModToast(Txt_ModName.text,
            modUploadSuccessToastMessage, Img_Screenshot.texture);
            ModUpdatedEvent?.Invoke(mod);
            OnModUploadSuccess();
          },
          (error) => {
            ToastManager.Instance.CreateModToast(Txt_ModName.text,
            modUploadFailedToastMessage, Img_Screenshot.texture, true);
            Debug.LogError(error.Description);
            ModCreationFailedEvent?.Invoke(error.Description);
          });
      } else {
        var createDto = GenerateDto<CreateModRequestDto>();
        READ_APIManager.Instance.CreateMod(createDto, modLogo,
          (mod) => {
            currentMod = mod;
            ModCreatedEvent?.Invoke(mod);
            ToastManager.Instance.CreateModToast(Txt_ModName.text,
             modUploadSuccessToastMessage, Img_Screenshot.texture);
            OnModUploadSuccess();
          },
             (error) => {
               BTN_UploadMod.interactable = true;
               ToastManager.Instance.CreateModToast(Txt_ModName.text,
               modUploadFailedToastMessage, Img_Screenshot.texture, true);
               if (error.Description!=null)
               Debug.LogError(error.Description);
               else if (error.ApiError!=null && error.ApiError.Description!=null) {
                 Debug.LogError(error.ApiError.Description);
               }
               ModCreationFailedEvent?.Invoke(error.Description);
             });
      }
    }
    #endregion Unity Buttons Callbacks

    public void SignIn() {
     Instantiate(PopupSignInPrefab, transform.parent);
    }

    #region Upload Logic
    private void OnModUploadSuccess() {
      if (options.isUploadNewFile)
        UploadNewFiles();
      else
        GameObject.Destroy(gameObject);
    }

    private void UploadNewFiles() {
      FileInfo file = new FileInfo(options.localFilePath);

      if (options.localFilePath != null) {
        READ_APIManager.Instance.UploadNewFile(
          currentMod,
          file,
          options.uploadFileDto,
          (uploadedFileInfo) => {
            FileUploadedEvent?.Invoke(uploadedFileInfo);
          }, (httpProgress) => {
            FileProgressEvent?.Invoke(httpProgress);
          }, (error) => {
            FileUploadFailedEvent?.Invoke(error.Description);
          });
      }
    }

    private bool IsReadyToSend() {
      bool isReady = true; ;
      if (Txt_ModName.text.Length == 0) {
        Txt_NameHeadline.color = errorTextColor;
        NameErrorGameObject.SetActive(true);
        isReady = false;
      } else {
        Txt_NameHeadline.color = normalTextColor;
        NameErrorGameObject.SetActive(false);
      }

      if (Txt_ModSummary.text.Length == 0) {
        Txt_SummaryHeadline.color = errorTextColor;
        SummaryErrorGameObject.SetActive(true);
        isReady = false;
      } else {
        Txt_SummaryHeadline.color = normalTextColor;
        SummaryErrorGameObject.SetActive(false);
      }

      if (modLogo == null) {
        Txt_ScreenshotHeadline.color = errorTextColor;
        ThumbnailErrorGameObject.SetActive(true);
        isReady = false;
      } else {
        Txt_ScreenshotHeadline.color = normalTextColor;
        ThumbnailErrorGameObject.SetActive(false);
      }

      if (Drp_Class.value==0) {
        isReady = false;
        Txt_ClassHeadline.color = errorTextColor;
        ClassErrorGameObject.SetActive(true);
      }
      else {
        Txt_ClassHeadline.color = normalTextColor;
        ClassErrorGameObject.SetActive(false);
      }

      if (Drp_Category.value==0) {
        isReady = false;
        Txt_CategoryHeadline.color = errorTextColor;
        CategoryErrorGameObject.SetActive(true);
      }
      else {
        Txt_CategoryHeadline.color = normalTextColor;
        CategoryErrorGameObject.SetActive(false);
      }

      return isReady;
    }

    private void UpdateCharLimitText(TMP_Text charCounterText,
                                   int currentLength,
                                   int maxLength) {
      charCounterText.text = String.Format("{0}/{1}", currentLength, maxLength);
      if (currentLength == maxLength) {
        charCounterText.color = errorTextColor;
      } else {
        charCounterText.color = normalTextColor;
      }
    }

    /// <summary>
    /// Crops the texture so it will be up to max size
    /// </summary>
    /// <param name="textureToCrop"></param>
    /// <returns></returns>
    private Texture2D CropTexture(Texture2D textureToCrop) {
      const int maxSize = 400;

      int width = Mathf.Min(textureToCrop.width, maxSize);
      int height = Mathf.Min(textureToCrop.height, maxSize);
      Texture2D answer = new Texture2D(width, height);

      int startPosX = Mathf.Max(0, (textureToCrop.width - width) / 2);
      int startPosY = Mathf.Max(0, (textureToCrop.height - height) / 2);
      Color[] c = textureToCrop.GetPixels(startPosX, startPosY, width, height);
      answer.SetPixels(c);
      answer.Apply();
      return answer;
    }

    private T GenerateDto<T>() where T : ModRequestDto, new() {
      var requestDto = new T {
        ClassId = selectedClass.Id,
        PrimaryCategoryId = selectedCategory.Id,
        Description = Txt_ModSummary.text,
        Summary = Txt_ModSummary.text,
        Name = Txt_ModName.text,
        IsExperimental = false,
        DescriptionType = MarkupType.WysiwygHtml,
        GameCategoryIds = new List<uint>()
      };
      return requestDto;
    }
    #endregion Private Logic
  }
}
