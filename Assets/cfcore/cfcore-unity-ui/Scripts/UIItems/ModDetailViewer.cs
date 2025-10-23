using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.UnityUI.Popups;
using Assets.Scripts.Utils;
using Overwolf.CFCore.UnityUI.Utils;
using System.Linq;
using UnityEngine.EventSystems;
using System;
using Overwolf.CFCore.CFCContext;
using Overwolf.CFCore.UnityUI.Themes;
using Overwolf.CFCore.UnityUI.InputController;
using Overwolf.CFCore.UnityUI.Gamepad;
using static Overwolf.CFCore.UnityUI.Gamepad.InputController;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class ModDetailViewer : MonoBehaviour, ICancelHandler {
    // TODO(sabraham): see how to properly inherit from Popup and get multiple
    // popup opening behavior fixed (opening uninstall popup from context menu
    // when in ModDetailViewer as popup
    private static ModDetailViewer _instance;

    public static ModDetailViewer Instance {
      get {
        if (_instance == null)
          _instance = new ModDetailViewer();
        return _instance;
      }
    }

    private GameObject origin;
    private GamepadPromptsScript gamepadPrompts;

    public event Action<GameObject> OnInstallButtonClicked;
    public event Action<GameObject> OnUninstallButtonClicked;
    public event Action<GameObject> OnLikeButtonClicked;
    public event Action<GameObject> OnReportButtonClicked;
    public event Action OnCloseButtonClicked;
    public event Action OnCancelButtonClick;
#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] GameObject Btn_Menu;
    [SerializeField] GameObject Btn_Back;
    [SerializeField] GameObject ScreenshotDotPrefab;
    [SerializeField] GameObject ModDescriptionPrefab;
    [SerializeField] GameObject ScaledImgPrefab;
    [SerializeField] GameObject CategoryItemPrefab;
    [SerializeField] RawImage Mod_Logo;
    [SerializeField] TextMeshProUGUI Txt_ModName;
    [SerializeField] TextMeshProUGUI Txt_CreatorName;
    [SerializeField] TextMeshProUGUI Txt_TotalInstalls;
    [SerializeField] TextMeshProUGUI Txt_DateCreated;
    [SerializeField] TextMeshProUGUI Txt_FileSize;
    [SerializeField] TextMeshProUGUI Txt_LikesAmount;

    [SerializeField] GameObject Btn_Install;
    [SerializeField] TMP_Text   TXT_installing;
    [SerializeField] GameObject Item_Installing;
    [SerializeField] GameObject Item_Installed;
    [SerializeField] GameObject Btn_Uninstall;
    [SerializeField] GameObject Btn_Update;
    [SerializeField] GameObject Btn_LikeInTop;

    [SerializeField] GameObject Icon_Liked;
    [SerializeField] GameObject Icon_Unliked;
    [SerializeField] GameObject Btn_ReportMod;


    [SerializeField] GameObject Item_MenuOptions;
    [SerializeField] TextMeshProUGUI Txt_LikeInMenuOptions;
    [SerializeField] GameObject Btn_InstallInMenuOptions;
    [SerializeField] GameObject Btn_UninstallInMenuOptions;
    [SerializeField] GameObject Btn_UpdateInMenu;
    [SerializeField] GameObject Btn_LikeInMenu;
    [SerializeField] GameObject Btn_ReportModInMenu;

    [SerializeField] Transform Transform_Dots;
    [SerializeField] GameObject Btn_Prev;
    [SerializeField] GameObject Btn_Next;
    [SerializeField] GameObject Btn_ScaleImg;
    [SerializeField] Transform Transform_Categories;

    [SerializeField] RawImage ScreenshotImg;
    [SerializeField] TextMeshProUGUI Txt_Description;
    [SerializeField] GameObject Btn_ReadMore;

    [SerializeField] GameObject ModFailedGameObject;
    [SerializeField] GameObject ModRejectedGameObject;
    [SerializeField] GameObject ModPendingGameObject;
    [SerializeField] GameObject ModCorruptedGameObject;
    [SerializeField] GameObject TopRightButtonsGameObject;
#pragma warning restore 0649

    int currentScreenshotIndex = 0;
    Mod modData;
    ModModel detailedViewModel;
    private bool isSignInDisabled;

    void Awake() {
      _instance = this;
      gamepadPrompts = FindObjectOfType<GamepadPromptsScript>(true);
    }

    private void Start() {
      var theme =EternalUITheme.Instance;
      if (theme != null) {

        if (theme.ColorTheme != null) {
          var setters = GetComponentsInChildren<EternalColorSetter>(true);
          foreach (EternalColorSetter setter in setters) {
            setter.SetupColor(theme.ColorTheme);
          }
        }

        if (theme.ShapeTheme != null) {
          var setters = GetComponentsInChildren<EternalShapeSetter>(true);
          foreach (EternalShapeSetter setter in setters) {
            setter.SetupShape(theme.ShapeTheme);
          }
        }
      }

      SelectComponentOnView();
    }

    private void OnDestroy() {
      OnCloseButtonClicked?.Invoke();
      gamepadPrompts.SetPrompts(ControllerPrompts.Map[NavigationMode.Library]);
    }

    public void OnCancel(BaseEventData eventData) {
      Back();
    }

    public virtual void SetupLikes(bool isThumbsUp, string likesAmount) {

      if (isSignInDisabled) {
        Icon_Liked.SetActive(false);
        Icon_Unliked.SetActive(false);
        Btn_LikeInMenu.SetActive(false);
        Txt_LikeInMenuOptions.gameObject.SetActive(false);
        Txt_LikesAmount.gameObject.SetActive(false);
        Btn_LikeInTop.SetActive(false);
      } else {
        Icon_Unliked.SetActive(!isThumbsUp);
        Icon_Liked.SetActive(isThumbsUp);

        Txt_LikeInMenuOptions.text = isThumbsUp ? "Unlike" : "Like";
        Txt_LikesAmount.text = likesAmount;
      }
    }

    private void SetupInstallButton(bool isInstalled) {
      Btn_UninstallInMenuOptions.SetActive(isInstalled);
      Item_Installing.SetActive(false);

      if (isInstalled) {
        if (detailedViewModel.isUpdateRequired) {
          Btn_Install.SetActive(false);
          var likeBtn = Btn_LikeInTop.GetComponent<Selectable>();
          var nav = likeBtn.navigation;
          nav.selectOnRight = Btn_Update.GetComponent<Selectable>();
          likeBtn.navigation = nav;
        }

        Btn_InstallInMenuOptions.SetActive(false);
        Item_Installed.SetActive(!detailedViewModel.isUpdateRequired);
        Btn_Update.SetActive(detailedViewModel.isUpdateRequired);
        Btn_UpdateInMenu.SetActive(detailedViewModel.isUpdateRequired);
      } else {
        Btn_Update.SetActive(false);
        Btn_UpdateInMenu.SetActive(false);
        Btn_Install.SetActive(true);
        Item_Installed.SetActive(false);
      }
    }

    #region public methods
    public virtual void SetUpView(GameObject origin, ModModel model , bool isInstalled, bool isApprovedMod=true) {
      gamepadPrompts.SetPrompts(ControllerPrompts.Map[NavigationMode.ModDetailed]);
      gamepadPrompts.SetPromptActive(GamepadButton.West, !isInstalled);

      this.origin = origin;
      detailedViewModel = model;
      detailedViewModel.GenerateDescription((desc)=> {
        var Utils = GetComponentInChildren<TextUtilsLoadExternalSprite>();
        if (Utils!=null) {
          Utils.ProccessText(desc);
        } else {
          Txt_Description.text = desc;
        }
        UpdateReadMoreButtonVisibility();
      });

      isSignInDisabled = READ_APIManager.Settings != null && READ_APIManager.Settings.HideSignIn;

      if (isSignInDisabled) {
        Btn_ReportModInMenu.SetActive(false);
        Btn_ReportMod.SetActive(false);
      }

      currentScreenshotIndex = 0;
      modData = model.modData;

      SetupMenuComponents(isApprovedMod, isInstalled);
      // We call this a second time after menu is setup because the install
      // button which is our priority for selection may be available to select
      SelectComponentOnView();

      if (modData.Logo!= null)
      StartCoroutine(Load_Img(modData.Logo.Url, Mod_Logo));
      Txt_ModName.text = modData.Name;
      if (modData.Authors != null)
        Txt_CreatorName.text = "by " + modData.Authors[0].Name;
      if (modData.DownloadCount < 1000)
        Txt_TotalInstalls.text = modData.DownloadCount.ToString();
      else if (modData.DownloadCount < 999999)
        Txt_TotalInstalls.text = (modData.DownloadCount / 1000).ToString("F1") + "K";
      else
        Txt_TotalInstalls.text = (modData.DownloadCount / 1000000).ToString("F1") + "M";

      Txt_DateCreated.text = modData.DateCreated.ToShortDateString();

      if (modData.LatestFiles != null)
        for (int i = 0; i < modData.LatestFiles.Count; i++) {
        if (modData.LatestFiles[i].Id == modData.MainFileId) {
          Txt_FileSize.text = (modData.LatestFiles[i].FileLength / (float)1000000).ToString("F2") + " MB";
          break;
        }
      }

      if (modData.Categories != null && modData.Categories.Count > 0)
        List_Categories(modData.Categories );
      else {
        READ_APIManager.Instance.GetMod(modData.Id, (mod) => {
          modData = mod;
          if (modData.Categories != null && modData.Categories.Count>0)
            List_Categories(modData.Categories);
        },
        (e) => { });
      }

      //Screenshot dots set up
      for (int i = 0; i < Transform_Dots.childCount; i++)
        Destroy(Transform_Dots.GetChild(i).gameObject);

      if (modData.Screenshots != null && modData.Screenshots.Count > 0) {
        if (modData.Screenshots.Count == 1) {
          Btn_Prev.SetActive(false);
          Btn_Next.SetActive(false);
        } else {
          Btn_Prev.SetActive(true);
          Btn_Next.SetActive(true);
        }
        Btn_ScaleImg.SetActive(true);

        if (modData.Screenshots.Count != 1) {
          for (int i = 0; i < modData.Screenshots.Count; i++) {
            GameObject dot = Instantiate(ScreenshotDotPrefab, Transform_Dots) as GameObject;
          }
        }

        LoadScreenShot(currentScreenshotIndex);
      } else {
        Btn_Prev.SetActive(false);
        Btn_Next.SetActive(false);
        Btn_ScaleImg.SetActive(false);
      }

      Context.Instance.SetContext(CFCContextConstants.IsModalViewOpen, true);
    }

    //--------------------------------------------------------------------------
    private void SetupMenuComponents(bool isApprovedMod, bool isInstalled) {
      if (!isApprovedMod) {
        DisableMenuComponents();
        return;
      }

      Context.Instance.SetContext(CFCContextConstants.ActiveMenu, Item_MenuOptions);
      SetupInstallButton(isInstalled);
    }

    /// <summary>
    /// Select the install button if available, otherwise back button.
    /// </summary>
    private void SelectComponentOnView() {
      var selectableComponents = transform.GetComponentsInChildren<Selectable>();

      var componentPriority = new string[] {
        nameof(Btn_Install), nameof(Btn_Update), "Btn_Menu"
      };

      foreach (var componentName in componentPriority) {
        var component = selectableComponents.FirstOrDefault(
          item => item.name == componentName);
        if (component != null) {
          component.Select();
          return;
        }
      }

      Debug.LogWarning("Could not find a component to select");
    }

    public virtual void SetUpView(GameObject origin, ModModel model, bool isInstalled, CreationUtils.CombinedCreationStatus status) {
      this.origin = origin;
      detailedViewModel = model;
      detailedViewModel.GenerateDescription((desc) => {
        Txt_Description.text = desc;
        UpdateReadMoreButtonVisibility();
      });

      var isApproved = false;
      switch (status) {
        case CreationUtils.CombinedCreationStatus.Updating:
        case CreationUtils.CombinedCreationStatus.Pending:
          ModPendingGameObject.SetActive(true);
          break;
        case CreationUtils.CombinedCreationStatus.Rejected:
          ModRejectedGameObject.SetActive(true);
          break;
        case CreationUtils.CombinedCreationStatus.Failed:
          ModFailedGameObject.SetActive(true);
          break;
        case CreationUtils.CombinedCreationStatus.Approved:
          isApproved = true;
          break;
      }

      SetUpView(origin, detailedViewModel, isInstalled, isApproved);
    }

    private void DisableMenuComponents() {
      TopRightButtonsGameObject.SetActive(false);
      Context.Instance.SetContext(CFCContextConstants.ActiveMenu, null);
    }

    public void SetupCorruptedMod() {
      Item_Installed.SetActive(false);
      Btn_Uninstall.SetActive(true);
      ModCorruptedGameObject.SetActive(true);
    }

    public void SetupUpdateRequired(bool isUpdateRequred) {
      Item_Installed.SetActive(false);
      Btn_InstallInMenuOptions.SetActive(false);
      Btn_Update.SetActive(isUpdateRequred);
      Btn_UpdateInMenu.SetActive(isUpdateRequred);
    }

    public virtual void Install() {
      TXT_installing.text = "Installing";
      Btn_Update.SetActive(false);
      Btn_UpdateInMenu.SetActive(false);
      OnInstallButtonClicked?.Invoke(Btn_Menu);
    }

    public void CancelInstall() {
      OnCancelButtonClick?.Invoke();
    }

    public void Uninstall() {
      // HACK(sabraham): Since this invokes Uninstall in ModTileScript, we need
      // to send this so that when the uninstall popup closes, it can find a
      // selectable child in its origin
      OnUninstallButtonClicked?.Invoke(Btn_Menu);
    }

    public void Like() {
      OnLikeButtonClicked?.Invoke(Btn_LikeInTop);
    }

    public void Report() {
      OnReportButtonClicked?.Invoke(Btn_ReportMod);
    }

    public void PrevScreenshot() {
      if (modData.Screenshots==null || modData.Screenshots.Count == 0)
        return;

      currentScreenshotIndex = currentScreenshotIndex - 1 < 0 ?
        modData.Screenshots.Count - 1 :
        currentScreenshotIndex - 1;

      LoadScreenShot(currentScreenshotIndex);
    }

    public void NextScreenshot() {
      if (modData.Screenshots == null || modData.Screenshots.Count == 0)
        return;

      currentScreenshotIndex = currentScreenshotIndex + 1 > modData.Screenshots.Count - 1 ? 0 : currentScreenshotIndex + 1;

      LoadScreenShot(currentScreenshotIndex);
    }

    public void ReadMoreDescription() {
      GameObject ReadMoreItem = Instantiate(ModDescriptionPrefab, UIManager.Instance.transform) as GameObject;
      ReadMoreItem.GetComponent<ReadMore>().Setup(modData.Name, Txt_Description.text);
    }

    public void ScaledScreenshot() {
      GameObject scaledScreen = Instantiate(ScaledImgPrefab, UIManager.Instance.transform) as GameObject;
      var screenShot = scaledScreen.GetComponent<ScaledScreenshot>();
      screenShot.Setup(currentScreenshotIndex, modData);
      screenShot.SetPopupOrigin(Btn_Menu);
    }

    public void Back() {
      Destroy(gameObject);
      Context.Instance.SetContext(CFCContextConstants.IsModalViewOpen, false);
      if (origin == null) {
        Debug.LogWarning("No origin mod tile found");
        return;
      }

      origin.GetComponent<Selectable>().Select();
    }
    #endregion

    #region private methods
   public virtual void List_Categories(List<Category> categories) {
      List<Category> normalCategories = new List<Category>();

      //Sort the categories
      for (int i = 0; i < categories.Count; i++) {
        normalCategories.Add(categories[i]);
      }

      GameObject classItem = Instantiate(CategoryItemPrefab, Transform_Categories) as GameObject;

      Category thisClass = READ_APIManager.Instance.ClassList.Find((c) => c.Id == normalCategories[0].ClassId);
      classItem.GetComponentInChildren<TextMeshProUGUI>().text = thisClass.Name;
      StartCoroutine(Load_Img(thisClass.IconUrl, classItem.GetComponentInChildren<RawImage>()));


      classItem.transform.SetSiblingIndex(Transform_Categories.childCount - 2);

      for (int i = 0; i < normalCategories.Count; i++) {
        GameObject category = Instantiate(CategoryItemPrefab, Transform_Categories) as GameObject;
        category.GetComponentInChildren<TextMeshProUGUI>().text = normalCategories[i].Name;
        if (normalCategories[i].IconUrl!= null)
        StartCoroutine(Load_Img(normalCategories[i].IconUrl, category.GetComponentInChildren<RawImage>()));
      }
    }

    void LoadScreenShot(int index) {
      StartCoroutine(Load_Img(modData.Screenshots[index].Url, ScreenshotImg));

      for (int i = 0; i < Transform_Dots.childCount; i++) {
        if (index == i)
          Transform_Dots.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
        else
          Transform_Dots.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
      }
    }
    #endregion

    IEnumerator Load_Img(string url, RawImage targetImg) {
      targetImg.GetComponent<Animator>().ResetTrigger("fadeout");
      targetImg.GetComponent<Animator>().ResetTrigger("fadein");

      targetImg.GetComponent<Animator>().SetTrigger("fadeout");

      UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
      yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
      if (request.result == UnityWebRequest.Result.ProtocolError ||
          request.result == UnityWebRequest.Result.ConnectionError) {
#else
      if (request.isHttpError || request.isNetworkError) {
#endif
        Debug.LogError("Thumbnail loading failed!\n" + request.error);
      } else {
        yield return new WaitForSeconds(0.25f);

        targetImg.GetComponent<Animator>().SetTrigger("fadein");
        targetImg.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
      }
      request.Dispose();
    }

    private void UpdateReadMoreButtonVisibility() {
      float preferredHeight = LayoutUtility.GetPreferredHeight(Txt_Description.rectTransform);
      float originHeight = Txt_Description.rectTransform.rect.height;
      if (preferredHeight > originHeight)
        Btn_ReadMore.SetActive(true);
      else
        Btn_ReadMore.SetActive(false);
    }


    #region API Callbacks
    public void Installed() {
      Btn_Install.SetActive(false);
      Item_Installed.SetActive(true);
      Item_Installing.SetActive(false);
      Btn_InstallInMenuOptions.SetActive(false);
      Btn_UninstallInMenuOptions.SetActive(true);
      gamepadPrompts.SetPromptActive(GamepadButton.West, true);
    }

    public void Installing() {
      Btn_Install.SetActive(false);
      Item_Installed.SetActive(false);
      Item_Installing.SetActive(true);
      gamepadPrompts.SetPromptActive(GamepadButton.West, false);
    }

    public virtual void Unzipping() {
      TXT_installing.text = "Unzipping";
    }

    public void InstallFailed() {
      Btn_Install.SetActive(true);
      Item_Installed.SetActive(false);
      Item_Installing.SetActive(false);
    }

    public void UnInstalled() {
      Btn_Install.SetActive(true);
      Item_Installed.SetActive(false);
      Item_Installing.SetActive(false);
      Btn_InstallInMenuOptions.SetActive(true);
      Btn_UninstallInMenuOptions.SetActive(false);
      Btn_Uninstall.SetActive(false);
      ModCorruptedGameObject.SetActive(false);
    }

    public void UninstallFailed() {
      Btn_Install.SetActive(false);
      Item_Installed.SetActive(true);
      Item_Installing.SetActive(false);
    }
    #endregion
  }
}
