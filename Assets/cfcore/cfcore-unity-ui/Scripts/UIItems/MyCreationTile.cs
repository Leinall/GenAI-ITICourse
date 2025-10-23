using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Overwolf.CFCore.Base.Api.Models.Enums;
using System;
using UnityEngine.Networking;
using Overwolf.CFCore.UnityUI.Managers;
using UnityEngine.EventSystems;
using Overwolf.CFCore.UnityUI.Popups;
using Overwolf.CFCore.Base.Creation.Models;
using Overwolf.CFCore.Base.Creation.Models.Enums;
using Overwolf.CFCore.UnityUI.View;
using System.IO;
using Assets.Scripts.Utils;
using Overwolf.CFCore.Base.Api.Models;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class MyCreationTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] RawImage Thumbnail;
    [SerializeField] TextMeshProUGUI ModName;
    [SerializeField] TextMeshProUGUI DateCreated;
    [SerializeField] TextMeshProUGUI DateModified;
    [SerializeField] TextMeshProUGUI LikesNumber;
    [SerializeField] TextMeshProUGUI InstallsNumber;

    [SerializeField] GameObject StatusWithFrameGameObject;
    [SerializeField] Image StatusFrameImage;
    [SerializeField] TextMeshProUGUI StatusText;

    [SerializeField] GameObject StatusWithPercentageGameObject;
    [SerializeField] Image ProgressBar;
    [SerializeField] TextMeshProUGUI ProgressBarPercentageText;

    [SerializeField] GameObject ModDetailViewPrefab;
    [SerializeField] GameObject ActionButtonGameObject;
    [SerializeField] TextMeshProUGUI ActionButtonText;

    [SerializeField] GameObject MenuOptions;
    [SerializeField] GameObject OptionRetry;
    [SerializeField] GameObject OptionsView;
    [SerializeField] GameObject OptionsGoToWebsite;
    [SerializeField] GameObject DeleteConfirmationPopup;
    [SerializeField] GameObject PopupReportPrefab;
    [SerializeField] GameObject UninstallConfirmPrefab;
#pragma warning restore 0649
    [SerializeField] private Color NormalStatusColor = new Color(0.145f, 0.615f, 0.247f);//green
    [SerializeField] private Color WarningStatusColor = new Color(0.98f, 0.737f, 0.235f);//orange
    [SerializeField] private Color ErrorStatusColor = new Color(0.756f, 0.18f, 0.18f);//red

    public GameObject Line_Border;
    public bool isMouseOver = false;

    private CreationUtils.CombinedCreationStatus currentCreationStatus ;

    private const string kThumbnailErrorFormat = "Profile texture loading failed!\n{0}";
    private const string kSiteUrlFormat = "https://console.curseforge.com/?#/games/{0}/moderation/projects";
    private bool isRetryable;
    private CreationItem CurrentCreation;
    private Action OnActionButtonPress;

    ModDetailViewer modDetailViewer = null;
    ModModel modModel;
    public virtual void Initialize(CreationItem creation) {
      CurrentCreation = creation;
      ModName.text = creation.Mod.Name;

      FormatDateTime(creation.Mod.DateCreated, DateCreated);
      FormatDateTime(creation.Mod.DateModified, DateModified);

      LikesNumber.text = creation.Mod.ThumbsUpCount != null
                         ? ShortenNumberFormat(creation.Mod.ThumbsUpCount ?? 0) : "0";
      InstallsNumber.text = ShortenNumberFormat(creation.Mod.DownloadCount);

      currentCreationStatus = CreationUtils.GetCombinedCreationStatus(creation);
        SetupModStatus(currentCreationStatus);

      if (currentCreationStatus == CreationUtils.CombinedCreationStatus.Updating) {
        OnProgressUpdate(creation.Revision.UploadProgress);
      }
        modModel = new ModModel(creation.Mod);
        StartCoroutine(LoadThumbnail());
    }

    public void Update() {
      if (Input.GetMouseButtonUp(1)) {
        if (isMouseOver)
          MenuOptions.SetActive(true);
        else
          MenuOptions.SetActive(false);
      } else if (Input.GetMouseButtonUp(0)) {
        MenuOptions.SetActive(false);
      }
    }

    void OnDestroy() {
      UnsubscribeToModDetailedViewEvents();
    }

    public void OnPointerEnter(PointerEventData eventData) {
      isMouseOver = true;
      Line_Border.SetActive(true);
      SetupMenuOptions();
    }

    public void OnPointerExit(PointerEventData eventData) {
      isMouseOver = false;
      Line_Border.SetActive(false);
    }

   public virtual void SetupModStatus(CreationUtils.CombinedCreationStatus status) {
      ToggleProgressBar(isProgressbarActive: false);
      ActionButtonGameObject.SetActive(false);
      isRetryable = false;

      switch (status) {
        case CreationUtils.CombinedCreationStatus.Updating:
          ToggleProgressBar(isProgressbarActive: true);
          ActionButtonText.text = "Cancel";
          OnActionButtonPress = Cancel;
          ActionButtonGameObject.SetActive(true);
          break;
        case CreationUtils.CombinedCreationStatus.Pending:
          StatusText.text = "Pending";
          StatusFrameImage.color = WarningStatusColor;
          break;
        case CreationUtils.CombinedCreationStatus.Canceled:
          isRetryable = true;
          StatusText.text = "Canceled";
          StatusFrameImage.color = ErrorStatusColor;
          OnActionButtonPress = Retry;
          ActionButtonGameObject.SetActive(true);
          ActionButtonText.text = "Re-Upload";
          break;
          case CreationUtils.CombinedCreationStatus.Failed:
          isRetryable = true;
          StatusText.text = "Failed";
          StatusFrameImage.color = ErrorStatusColor;
          OnActionButtonPress = Retry;
          ActionButtonGameObject.SetActive(true);
          ActionButtonText.text = "Retry";
          break;
        case CreationUtils.CombinedCreationStatus.Approved:
          StatusText.text = "Approved";
          StatusFrameImage.color = NormalStatusColor;
          ActionButtonGameObject.SetActive(false);
          break;
        case CreationUtils.CombinedCreationStatus.Rejected:
          isRetryable = true;
          StatusText.text = "Rejected";
          StatusFrameImage.color = ErrorStatusColor;
          break;
      }
    }

    private void SetupMenuOptions() {
      OptionRetry.SetActive(isRetryable);
      OptionsGoToWebsite.SetActive(!isRetryable);
      OptionsView.SetActive(!isRetryable);
    }

    private void FormatDateTime(DateTime dateTime, TextMeshProUGUI outputText) {
      if (dateTime == DateTime.MinValue) {
        outputText.text = "";
        return;
      }
      outputText.text = dateTime.ToShortDateString();
    }

    private IEnumerator LoadThumbnail() {
      if (CurrentCreation.Mod == null ||
        CurrentCreation.Mod.Logo == null ||
        CurrentCreation.Mod.Logo.Url == null ||
        CurrentCreation.Mod.Logo.Url.Length == 0) {
        yield break;
      }

      UnityWebRequest request =
        UnityWebRequestTexture.GetTexture(CurrentCreation.Mod.Logo.Url);
      yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
      if (request.result == UnityWebRequest.Result.ProtocolError ||
          request.result == UnityWebRequest.Result.ConnectionError) {
#else
      if (request.isHttpError || request.isNetworkError) {
#endif
        Debug.LogError(string.Format(kThumbnailErrorFormat, request.error));
      } else {
        Thumbnail.texture =
                      ((DownloadHandlerTexture)request.downloadHandler).texture;
      }
      request.Dispose();
    }

    private void ToggleProgressBar(bool isProgressbarActive) {
      StatusWithFrameGameObject.SetActive(!isProgressbarActive);
      StatusWithPercentageGameObject.SetActive(isProgressbarActive);
    }

    private void OnProgressUpdate(uint progress) {
      if (ProgressBarPercentageText != null)
        ProgressBarPercentageText.text =
          string.Format("{0}%", (progress).ToString());
      if (ProgressBar != null)
        ProgressBar.fillAmount = ((float)progress) / 100;
    }

    #region Menu Actions

    public void Cancel() {
      READ_APIManager.Instance.CancelUpload(CurrentCreation,
                                            OnCancel,
                                            (error) => {
                                              Debug.LogError(error.Description);
                                              OnCancel();
                                            }
                                            );
    }

    public virtual void Retry() {
      if (CurrentCreation.Revision == null ||
          CurrentCreation.Revision.FilePath.Length == 0) {
        // error
        return;
      }

      ToggleProgressBar(isProgressbarActive: true);
      CurrentCreation.Revision.Status = RevisionStatus.Updating;
      SetupModStatus(CreationUtils.CombinedCreationStatus.Updating);
      FileInfo file = new FileInfo(CurrentCreation.Revision.FilePath);

      READ_APIManager.Instance.UploadNewFile(CurrentCreation.Mod,
        file,
        CurrentCreation.Revision.UploadRequestDto,
        (fileinfo) => {
          OnSuccess();
        }, (prog) => OnProgressUpdate(prog.Progress),
        (error) => {
          OnFailed();
        });

      OnActionButtonPress = Cancel;
      ActionButtonGameObject.SetActive(true);
      ActionButtonText.text = "Cancel";
    }

    public void View() {
      GameObject ModDetailView = Instantiate(ModDetailViewPrefab, MyCreationsView.Instance.transform) as GameObject;
      bool isInstalled = READ_APIManager.Instance.InstalledModList.Find(
              (m) => m.Details.Id.Equals(CurrentCreation.Mod.Id))!=null;
      modDetailViewer = ModDetailView.GetComponent<ModDetailViewer>();
      modDetailViewer.SetUpView(gameObject, modModel, isInstalled, currentCreationStatus);
      modDetailViewer.SetupLikes(READ_APIManager.Instance.CurrentPlayerLikedModIdList.Contains(CurrentCreation.Mod.Id)
      , ShortenNumberFormat(modModel.ModLikeCountNumber));
      SubscribeToModDetailedViewEvents();
    }

    private void SubscribeToModDetailedViewEvents() {
      if (modDetailViewer == null) return;
      modDetailViewer.OnCloseButtonClicked += UnsubscribeToModDetailedViewEvents;
      modDetailViewer.OnInstallButtonClicked += Install;
      modDetailViewer.OnLikeButtonClicked += Like;
      modDetailViewer.OnReportButtonClicked += Report;
      modDetailViewer.OnUninstallButtonClicked += Uninstall;

    }

    private void UnsubscribeToModDetailedViewEvents() {
      if (modDetailViewer == null) return;
      modDetailViewer.OnCloseButtonClicked -= UnsubscribeToModDetailedViewEvents;
      modDetailViewer.OnInstallButtonClicked -= Install;
      modDetailViewer.OnLikeButtonClicked -= Like;
      modDetailViewer.OnReportButtonClicked -= Report;
      modDetailViewer.OnUninstallButtonClicked -= Uninstall;
    }

    public void Install(GameObject origin = null) {
      modModel.Install(
        modDetailViewer.Installed, (prog) => { }, () => { });
      if (origin != null) {
        origin.GetComponent<Selectable>().Select();
      }
    }

    public void Like(GameObject origin = null) {
     bool isLiked =
       READ_APIManager.Instance.CurrentPlayerLikedModIdList.Contains(CurrentCreation.Mod.Id);
      modModel.Like(isLiked, () => {
        isLiked = !isLiked;
        LikesNumber.text = ShortenNumberFormat(modModel.ModLikeCountNumber);

        if (modDetailViewer != null) {
          modDetailViewer.SetupLikes(isLiked, ShortenNumberFormat(modModel.ModLikeCountNumber));
        }
      });
    }

    public void Report(GameObject origin = null) {
      if (READ_APIManager.Instance.IsSignedIn) {
        GameObject ReportPopup = Instantiate(PopupReportPrefab, UIManager.Instance.transform) as GameObject;
        Report report = ReportPopup.GetComponentInChildren<Report>();
        if (report != null) {
          report.Initialize(modModel.modData.Id);
          report.SetPopupOrigin(origin != null ? origin : gameObject);
        }
      } else {
        UIManager.Instance.SignIn();
      }
    }

    public void Uninstall(GameObject origin = null) {
      GameObject uninstallPopup = Instantiate(UninstallConfirmPrefab, UIManager.Instance.transform) as GameObject;
      var popup = uninstallPopup.GetComponent<UninstallConfirm>();
      popup.SetPopupOrigin(origin ?? gameObject);
      popup.GetComponent<UninstallConfirm>().SetupPopup(CurrentCreation.Mod,
        () => {
          if (modDetailViewer != null)
            modDetailViewer.UnInstalled();
          ToastManager.Instance.ActivateNormalToast($"{CurrentCreation.Mod.Name} uninstalled!");
        },
        () => { });
    }

    public void GoToWebsite() {
      READ_APIManager.Instance.GetMod(CurrentCreation.Mod.Id,
        (mod) => {
          Application.OpenURL(mod.Links.WebsiteUrl);
        }, (e) => { }) ;
    }

    public void Delete() {
      GameObject deletePopup = Instantiate(DeleteConfirmationPopup, UIManager.Instance.transform);
      deletePopup.GetComponent<DeleteConfirm>().Initialize(CurrentCreation.Mod);
    }

    #endregion Menu Actions

    public void ActionButtonPress() {
      OnActionButtonPress?.Invoke();
    }

    private void OnCancel() {
      SetupModStatus(CreationUtils.CombinedCreationStatus.Canceled);
    }

    private void OnFailed() {
      CurrentCreation.Revision.Status = RevisionStatus.Failed;
      SetupModStatus(CreationUtils.CombinedCreationStatus.Failed);
    }

    private void OnSuccess() {
      CurrentCreation.Mod.Status = ModStatus.UnderReview;
      SetupModStatus(CreationUtils.CombinedCreationStatus.Pending);
    }

    string ShortenNumberFormat(double numberToShorten) {
      if (numberToShorten < 1000)
        return numberToShorten.ToString();
      else if (numberToShorten < 999999)
        return (numberToShorten / 1000).ToString("F1") + "K";
      else
        return (numberToShorten / 1000000).ToString("F1") + "M";
    }
  }
}