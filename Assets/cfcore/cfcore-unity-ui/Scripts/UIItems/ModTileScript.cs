using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.Base.Library.Models;
using Overwolf.CFCore.Base.Library.Models.Enums;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.UnityUI.Popups;
using Overwolf.CFCore.UnityUI.Themes;
using Overwolf.CFCore.UnityUI.InputController;
using Overwolf.CFCore.UnityUI.Gamepad;
using Overwolf.CFCore.CFCContext;
using System;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class ModTileScript : MonoBehaviour, IPointerEnterHandler,
                                              IPointerExitHandler,
                                              ISelectHandler,
                                              IDeselectHandler {
    public Mod modData;
    public bool isInstalled { get; set; }
    public bool isLiked { get; set; }

    public RawImage ThumbnailImg;

    public TextMeshProUGUI Txt_ModName;
    public TextMeshProUGUI Txt_AutherName;
    public TextMeshProUGUI Txt_LikeCount;
    public TextMeshProUGUI Txt_ModSize;
    public TextMeshProUGUI Txt_Installed;

    public GameObject Btn_Install;
    public GameObject Icon_UnLiked;
    public GameObject Icon_Liked;

    public GameObject MenuOptions;
    public GameObject Btn_InstallInMenuOptions;
    public GameObject Btn_UninstallInMenuOptions;
    public GameObject Btn_UpdateMenuOption;
    public GameObject Btn_LikeMenuOptions;
    public GameObject LineMenuOptions;
    public GameObject Btn_ReportMenuOption;
    public GameObject Btn_Cancel;
    private Button button_install_component;

    public TextMeshProUGUI Txt_LikeInMenuOptions;

    public GameObject CoverItem_Installing;
    public TMP_Text CoverItem_Installing_Text;
    public Slider Slider_Installing;

    public GameObject Line_Border;
    public GameObject Icon_UpdateRequired;
    public GameObject Icon_Invalid;
    public GameObject Banner_Invalid;

    public bool isMouseOver = false;

    public ModDetailViewer modDetailViewer = null;

    private ModModel modModel;

    private GamepadPromptsScript gamepadPrompts;

    private bool isInvalid;
    private bool isUpdateRequired=false;
    private bool isSignInDisabled;

    private string kCoverItem_Installing_install_Text = "Installing...";
    private string kCoverItem_Installing_unzip_Text = "Unzipping...";

#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] GameObject ModDetailViewPrefab;
    [SerializeField] GameObject UninstallConfirmPrefab;
    [SerializeField] GameObject PopupReportPrefab;
#pragma warning restore 0649

    private void Awake() {
      gamepadPrompts = FindObjectOfType<GamepadPromptsScript>(true);
    }

    public void OnPointerEnter(PointerEventData eventData) {
      isMouseOver = true;
      Line_Border.SetActive(true);
      if (isInvalid) {
        Icon_Invalid.SetActive(false);
        Banner_Invalid.SetActive(true);
      }
    }

    public void OnPointerExit(PointerEventData eventData) {
      isMouseOver = false;
      Line_Border.SetActive(false);
      if (isInvalid) {
        Banner_Invalid.SetActive(false);
        Icon_Invalid.SetActive(true);
      }
    }

    public void OnSelect(BaseEventData eventData) {
      Line_Border.SetActive(true);
      if (isInvalid) {
        Icon_Invalid.SetActive(false);
        Banner_Invalid.SetActive(true);
      }

      Context.Instance.SetContext(CFCContextConstants.ActiveMenu, MenuOptions);
      gamepadPrompts.SetPromptActive(GamepadButton.West, !isInstalled);
    }

    public void OnDeselect(BaseEventData eventData) {
      Line_Border.SetActive(false);
      if (isInvalid) {
        Banner_Invalid.SetActive(false);
        Icon_Invalid.SetActive(true);
      }

      gamepadPrompts.SetPromptActive(GamepadButton.West, false);
    }

    private void OnDestroy() {
      UnsubscribeToModDetailedViewEvents();
      Context.Instance.SetContext(CFCContextConstants.ActiveMenu, null);
    }

    private void Start() {

      button_install_component = Btn_Install.GetComponent<Button>();
      var theme = EternalUITheme.Instance;
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
    }

   public virtual void Update() {
#if ENABLE_LEGACY_INPUT_MANAGER
    // Old input backends are enabled.

      if (Input.GetMouseButtonUp(1)) {
        if (isMouseOver) {
          OpenMenuOptions(true);
        } else {
          OpenMenuOptions(false);
        }
      } else if (Input.GetMouseButtonUp(0)) {
        OpenMenuOptions(false);
      }
#endif
      Btn_Install.SetActive(!isInstalled);
      Txt_Installed.gameObject.SetActive(isInstalled);
      Btn_InstallInMenuOptions.SetActive(!isInstalled);
      Btn_UninstallInMenuOptions.SetActive(isInstalled);

      if (isSignInDisabled) {
        Icon_Liked.SetActive(false);
        Icon_UnLiked.SetActive(false);
        return;
      }

      Icon_UnLiked.SetActive(!isLiked);
      Icon_Liked.SetActive(isLiked);
      if (isLiked)
        Txt_LikeInMenuOptions.text = "Unlike";
      else
        Txt_LikeInMenuOptions.text = "Like";
    }

    private void OpenMenuOptions(bool isOpened) {
      MenuOptions.SetActive(isOpened);
    }

    public virtual void SetupModItem(Mod data, bool installed) {
      isSignInDisabled = READ_APIManager.Settings != null && READ_APIManager.Settings.HideSignIn;
      modData = data;
      isInstalled = installed;
      modModel = new ModModel(modData);
      Txt_ModName.text = modData.Name;
      if (modData.Authors.Count > 0)
        Txt_AutherName.text = modData.Authors[0].Name;
      else
        Txt_AutherName.text = "";

      Txt_ModSize.text = ShortenNumberFormat(modData.DownloadCount);
      UpdateStatus();

      if (data.Logo != null)
        StartCoroutine(Load_ThumbnailImg(data.Logo.ThumbnailUrl));

      if (isSignInDisabled) {
        Icon_Liked.SetActive(false);
        Icon_UnLiked.SetActive(false);
        Txt_LikeCount.gameObject.SetActive(false);
        Btn_LikeMenuOptions.SetActive(false);
        Btn_ReportMenuOption.SetActive(false);
        LineMenuOptions.SetActive(false);
      }
    }

    /// <summary>
    /// Update the visuals according to the latest data
    /// </summary>
    public virtual void UpdateStatus() {
      isLiked =
        READ_APIManager.Instance.CurrentPlayerLikedModIdList.Contains(modData.Id);
      Txt_LikeCount.text = ShortenNumberFormat(modModel.ModLikeCountNumber);
    }

    IEnumerator Load_ThumbnailImg(string thumbnailURL) {
      ThumbnailImg.GetComponent<Animator>().ResetTrigger("fadeout");
      ThumbnailImg.GetComponent<Animator>().ResetTrigger("fadein");


      UnityWebRequest request = UnityWebRequestTexture.GetTexture(thumbnailURL);
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

        ThumbnailImg.GetComponent<Animator>().SetTrigger("fadein");
        ThumbnailImg.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        modModel.thumbnailTexture = ThumbnailImg.texture;
      }
      request.Dispose();
    }

    public void ClickMod() {
      Transform parentTransform = UIManager.Instance.Transform_BrowsePanel;
      if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.BrowseMods)
        parentTransform = UIManager.Instance.Transform_BrowsePanel;
      else if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.InstalledMods)
        parentTransform = UIManager.Instance.Transform_InstalledPanel;

      GameObject ModDetailView = Instantiate(ModDetailViewPrefab, parentTransform);
      modDetailViewer = ModDetailView.GetComponent<ModDetailViewer>();
      modDetailViewer.SetUpView(gameObject, modModel, isInstalled);
      modDetailViewer.SetupLikes(isLiked, ShortenNumberFormat(modModel.ModLikeCountNumber));
      if (modModel.isCorrupted) {
        modDetailViewer.SetupCorruptedMod();
      }
      SubscribeToModDetailedViewEvents();
    }

    private void SubscribeToModDetailedViewEvents() {
      if (modDetailViewer == null) return;
      modDetailViewer.OnCloseButtonClicked += UnsubscribeToModDetailedViewEvents;
      modDetailViewer.OnInstallButtonClicked += Install;
      modDetailViewer.OnLikeButtonClicked += Like;
      modDetailViewer.OnReportButtonClicked += Report;
      modDetailViewer.OnUninstallButtonClicked += Uninstall;
      modDetailViewer.OnCancelButtonClick += CancelInstall;

    }

    private void UnsubscribeToModDetailedViewEvents() {
      if (modDetailViewer == null) return;
      modDetailViewer.OnCloseButtonClicked -= UnsubscribeToModDetailedViewEvents;
      modDetailViewer.OnInstallButtonClicked -= Install;
      modDetailViewer.OnLikeButtonClicked -= Like;
      modDetailViewer.OnReportButtonClicked -= Report;
      modDetailViewer.OnUninstallButtonClicked -= Uninstall;
      modDetailViewer.OnCancelButtonClick -= CancelInstall;
    }

    #region Menu Options
    public virtual void Install(GameObject origin = null) {
      CoverItem_Installing.SetActive(true);
      CoverItem_Installing_Text.text = kCoverItem_Installing_install_Text;
      Btn_Cancel.SetActive(true);
      Btn_Cancel.GetComponent<Button>().interactable = true;
      Slider_Installing.value = 0;
      if (modDetailViewer != null)
        modDetailViewer.Installing();
      modModel.Install(
        Installed, Installing, InstallFailed);
      // Select the tile since this action may originate from the context menu
      // and leave us without a selected object
      if (origin != null) {
        origin.GetComponent<Selectable>().Select();
      } else {
        GetComponent<Selectable>().Select();
      }
    }

    public void Uninstall(GameObject origin = null) {
      GameObject uninstallPopup = Instantiate(UninstallConfirmPrefab, UIManager.Instance.transform) as GameObject;
      var popup = uninstallPopup.GetComponent<UninstallConfirm>();
      popup.SetupPopup(modData, UnInstalled, UninstallFailed);
      popup.SetPopupOrigin(origin != null ? origin : gameObject);
    }

    public void View() {
      ClickMod();
    }

    public void Like(GameObject origin = null) {
      modModel.Like(isLiked, () => {
        isLiked = !isLiked;
        Txt_LikeCount.text = ShortenNumberFormat(modModel.ModLikeCountNumber);

        if (modDetailViewer != null) {
          modDetailViewer.SetupLikes(isLiked, ShortenNumberFormat(modModel.ModLikeCountNumber));
        }
      });

      // Select the tile since this action may originate from the context menu
      // and leave us without a selected object
      var parent = origin != null ? origin : gameObject;
      parent.GetComponent<Selectable>().Select();
    }

    public void Report(GameObject origin = null) {
      if (READ_APIManager.Instance.IsSignedIn) {
        GameObject ReportPopup = Instantiate(PopupReportPrefab, UIManager.Instance.transform) as GameObject;
        Report report = ReportPopup.GetComponentInChildren<Report>();
        if (report != null) {
          report.Initialize(modData.Id);
          report.SetPopupOrigin(origin != null ? origin : gameObject);
        }
      } else {
        UIManager.Instance.SignIn();
      }
    }

    public void CancelInstall() {
      READ_APIManager.Instance.CancelInstallation(modData,
        () => {
          Btn_Cancel.SetActive(false);
          button_install_component.interactable = true;
          CoverItem_Installing.SetActive(false);
          SetUpdateRequired(isUpdateRequired);
        });
    }

    public void SetUpdateRequired(bool isRequired = true) {
      Icon_UpdateRequired.SetActive(isRequired);
      Btn_UpdateMenuOption.SetActive(isRequired);
      isUpdateRequired = isRequired;
      modModel.isUpdateRequired = isRequired;
    }

    public void SetInvalid(bool isInvalid = true) {
      Icon_Invalid.SetActive(isInvalid);
      this.isInvalid = isInvalid;
      modModel.isCorrupted = isInvalid;
    }
    #endregion

    #region API Callbacks
    public void Installed() {
      isInstalled = true;
      Btn_Cancel.SetActive(false);
      CoverItem_Installing.SetActive(false);
      SetInvalid(false);
      isUpdateRequired = false;
      SetUpdateRequired(false);

      if (modDetailViewer != null)
        modDetailViewer.Installed();
    }

    public virtual void Installing(LibraryProgress progress) {
        button_install_component.interactable = false;
        Slider_Installing.value = (float)progress.DataTransfer.Progress / 100;

        if (progress.State == LibraryProgressState.Unzipping) {
          CoverItem_Installing_Text.text = kCoverItem_Installing_unzip_Text;
          Debug.Log("unziping " + progress.DataTransfer.Progress);
          if (modDetailViewer != null)
            modDetailViewer.Unzipping();
        }

        if (progress.State == LibraryProgressState.Copying) {
          Btn_Cancel.GetComponent<Button>().interactable = false;
          Debug.Log("copying " + progress.DataTransfer.Progress);
        }
    }

    public void InstallFailed() {
       if (gameObject == null)
         return;
      CoverItem_Installing.SetActive(false);
      Btn_Cancel.SetActive(false);
      button_install_component.interactable = true;
      SetUpdateRequired(isUpdateRequired);

      if (modDetailViewer != null)
        modDetailViewer.InstallFailed();
    }

    public void UnInstalled() {
      SetInvalid(false);
      SetUpdateRequired(false);
      isInstalled = false;
      button_install_component.interactable = true;
      if (modDetailViewer != null)
        modDetailViewer.UnInstalled();

      ToastManager.Instance.ActivateNormalToast($"{modData.Name} uninstalled!");

      if (NavigationManager.Instance.SelectedNavigationSection == NavigationSection.InstalledMods) {
        Destroy(gameObject);
        UIManager.Instance.List_ClassesInInstalledMods();
      }
    }

    public void UninstallFailed() {

    }
    #endregion

    #region Helper Functions

    /// <summary>
    /// Changes a double number to "short" format
    /// 1,000->1K 1,000,000->1M
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    string ShortenNumberFormat(double numberToShorten) {
      if (numberToShorten < 1000)
        return numberToShorten.ToString();
      else if (numberToShorten < 999999)
        return (numberToShorten / 1000).ToString("F1") + "K";
      else
        return (numberToShorten / 1000000).ToString("F1") + "M";
    }



    #endregion Helper Functions
  }
}
