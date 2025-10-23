using Overwolf.CFCore.Base;
using Overwolf.CFCore.Base.Api.Common;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.Base.Api.Models.Filters;
using Overwolf.CFCore.Base.Common;
using Overwolf.CFCore.Base.Library.Models;
using Overwolf.CFCore.Base.Models;
using Overwolf.CFCore.Unity;
using Overwolf.CFCore.Base.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Overwolf.CFCore.Base.Api.Models.Requests;
using Overwolf.CFCore.Base.Api.Models.Users;
using Overwolf.CFCore.Base.Creation.Models;
using Overwolf.CFCore.UnityUI.Popups;

  namespace Overwolf.CFCore.UnityUI.Managers {

  //TODO: Create a custom editor with settings creation button
  public class READ_APIManager : MonoBehaviour {

    #region Static Members
    private static READ_APIManager _instance;

    public static READ_APIManager Instance {
      get {
        if (_instance == null)
          Debug.LogError("Error READ_APIManager wasn't initiated. please make sure you have an API Manager in the scene and that you run Init() ");
        return _instance;
      }
    }

    private static Me _currentUser;
    public static Me CurrentUser {
      get {
        return _currentUser;
      }
    }

    private static EternalAppSettings.EternalSettings settings;
    public static EternalAppSettings.EternalSettings Settings {
      get {
        return settings;
      }
    }

  #endregion Static Members

    #region Private Members

    private ICFCore _cfcoreService;

    public static uint GameId {
      get {
        return Instance.kGameId;
      }
    }

    [SerializeField] uint kGameId;
    [SerializeField] string kApiKey;

    private bool isReady;
    private bool isSigned;
    private bool descriptionFlag;

    #endregion Private Members

    #region Public Members

    public uint kMaxConcurrentInstallations = 3;
    public bool IsReadyStart { get { return isReady; } }
    public bool IsSignedIn { get { return isSigned; } }

    [NonSerialized]
    public List<uint> CurrentPlayerLikedModIdList = new List<uint>();

    public List<InstalledMod> InstalledModList = new List<InstalledMod>();
    public List<Category> ClassList = new List<Category>();
    public List<InstalledMod> CurrentInstallingModList = new List<InstalledMod>();
    public List<CreationItem> CurrentPlayerCreationsList = new List<CreationItem>();
    public List<ReportReason> ReportReasons = new List<ReportReason>();

    #endregion  Public Members

    #region Events

    public delegate void CodeSent(bool isResend);
    public static event CodeSent CodeSentEvent;

    public delegate void LoginSuccess();
    public static event LoginSuccess LoginSuccessEvent;

    public delegate void LoginFailed(bool isCodeExpired);
    public static event LoginFailed LoginFailedEvent;

    public delegate void LogoutSuccess();
    public static event LogoutSuccess LogoutSuccessEvent;

    public delegate void ErrorDelegate(CFCoreError cfCoreError,Action RetryAction);
    public static event ErrorDelegate ErrorEvent;

    public delegate void CurrentUserDelegate(Me currentUser);
    public static event CurrentUserDelegate CurrentUserChangedEvent;

    #endregion Events

    private void Awake() {
      _instance = this;
      settings = Resources.Load<EternalAppSettings.EternalSettings>(EternalAppSettings.EternalSettings.FilePath);

      if (settings != null && settings.InitializeAPIAtStartup) {
        Init();
      }
    }

   public void Init() {
      GlobalLoader.Instance?.SetActive(true);

      string language = EternalAppSettings.LanguageEnum.en.ToString();

      if (settings == null) {
        Debug.LogWarning("Setting File doesn't exist. Please create a new setting from the menu : " +
          "   Eternal -> Edit Settings");
      } else{
        if (settings.GameId == 0 || string.IsNullOrEmpty(settings.APIKey)) {
          Debug.LogWarning($"The settings file exist at {EternalAppSettings.EternalSettings.FilePath} but the data isn't filled");
        } else {
          kGameId = settings.GameId;
          kApiKey = settings.APIKey;
          language = settings.EulaLanguage.ToString();
          descriptionFlag = settings.UseModDescription;
        }
      }

      _cfcoreService = BootstrapUnity.Create(new Settings() {
        GameId = kGameId,
        ApiKey = kApiKey,
        ModsDirectory = GetModsDirectory(),
        UserDataDirectory = GetUserDataDirectory(),
        MaxConcurrentInstallations = kMaxConcurrentInstallations,
        MaxConcurrentUploads = 1,
        Language = language,
        ModsDirectoryMode = Settings != null ?
          Settings.DirectoryMode :
          Base.Models.Enums.ModsDirectoryMode.CFCoreStructure,

      }) ;

      _cfcoreService.Initialize(() => {
        ServiceInitialized();
        if (IsAuthenticated()) {
          isSigned = true;
          LoginSuccessEvent?.Invoke();
          _currentUser = _cfcoreService.Authentication.AuthenticatedUser;
          CurrentUserChangedEvent?.Invoke(_currentUser);
        }
      }
        , ServiceInitFailed);
    }

    #region CallBacks
    private void ServiceInitialized() {
      //Get Classes and Installed Mods List at Startup
      Request_GetClassesAndInstalledModsAtStartUp();
    }

    private void ServiceInitFailed(string error) {
      GlobalLoader.Instance?.SetActive(false);
      Debug.LogError($"Failed to initialize: {error}");
      CFCoreError cfError = new CFCoreError(CFCoreErrorCodes.FailedToInitialize);
      HandleError(cfError);
    }

    private string GetModsDirectory() {
#if UNITY_EDITOR
      return Path.Combine(Directory.GetCurrentDirectory(), "cfcore/mods");
#else
      return EternalDirSettingsHelper.ExtractEnvironmentPath(
        settings.ModsDirectory);
#endif
    }

    private string GetUserDataDirectory() {
#if UNITY_EDITOR
      return Path.Combine(Directory.GetCurrentDirectory(), "cfcore/user_data");
#else
      return EternalDirSettingsHelper.ExtractEnvironmentPath(
        settings.UserDataDirectory);
#endif
    }
    #endregion

    #region Authentication
    public void Request_SendCode(string email,Action<CFCoreError> onFailure, bool isResend = false) {
      _cfcoreService.Authentication.SendSecurityCode(
        email,
        () => {
          Debug.Log("Successfully sent security code to " + email);
          if (CodeSentEvent != null) {
            CodeSentEvent(isResend);
          }
        },
        (error) => {
          HandleError(error);
          onFailure(error);
        });
    }
    public void Request_Logout() {
      _cfcoreService.Authentication.Logout(
        () => {
          Debug.Log("Logout Success!");
          isSigned = false;
          LogoutSuccessEvent?.Invoke();
          _currentUser = null;
        });
    }

    public void Request_GenerateToken(string email, long securityCode,Action<CFCoreError> onFailure) {
      _cfcoreService.Authentication.GenerateAuthToken(
        email,
        securityCode,
         OnGenerateAuthTokenSuccess,
        (error) => {
          OnGenerateAuthTokenFailed(error);
          onFailure(error);
        });
    }

    public void Request_GenerateToken(
                                     AuthExternalProviders authExternalProvider,
                                     string token,
                                     DateTime eulaAcceptanceTime) {

      _cfcoreService.Authentication.GenerateAuthTokenByExternalProvider(
        authExternalProvider,
        token,
        new AuthExternalAdditionalInfo() {
          EulaAcceptTime = eulaAcceptanceTime
        },
        OnGenerateAuthTokenSuccess,
        (error) => OnGenerateAuthTokenFailed(error));
    }

    #endregion

    #region Read APIs
    public void Request_GetClassesAndInstalledModsAtStartUp() {
      GlobalLoader.Instance?.SetActive(true);
      //Sync with Server
      _cfcoreService.Library.SyncWithServer(() => {
        //Get Installed Mods
        _cfcoreService.Library.GetInstalledMods((response) => {

          InstalledModList.Clear();
          for (int i = 0; i < response.Count; i++) {
            InstalledModList.Add(response[i]);

          }

          //Get all available classes
          _cfcoreService.Api.GetCategories(
              new GetCategoriesFilter(),
              (categoryResponse) => {
                ClassList.Clear();
                for (int i = 0; i < categoryResponse.Data.Count; i++) {
                  if (categoryResponse.Data[i].IsClass == true)
                    ClassList.Add(categoryResponse.Data[i]);
                }
                GlobalLoader.Instance?.SetActive(false);
                isReady = true;
                ClassList.Sort((Category classA, Category classB) =>
                 {return classA.DisplayIndex > classB.DisplayIndex ? 1 : -1; });
              },
              (error) => {
                CFCoreError cfError = new CFCoreError(error);
                GlobalLoader.Instance?.SetActive(false);
                HandleError(cfError, "Failed to get classes list", Request_GetClassesAndInstalledModsAtStartUp);
              });
        }, (cfError) => {
          GlobalLoader.Instance?.SetActive(false);
          HandleError(cfError, "Failed to get installed mods", Request_GetClassesAndInstalledModsAtStartUp);
      });
      }, (cfError) => {
        GlobalLoader.Instance?.SetActive(false);
        HandleError(cfError,"Eternal", Request_GetClassesAndInstalledModsAtStartUp);
      });
    }

    public void Request_GetInstalledMods() {
      _cfcoreService.Library.SyncWithServer(() => {
        Debug.Log("Successfully synchronized with server");
        _cfcoreService.Library.GetInstalledMods((response) => {
          Debug.Log($"Successfully got installed mods {response.Count}");
          InstalledModList.Clear();
          for (int i = 0; i < response.Count; i++) {
            InstalledModList.Add(response[i]);
          }

        }, (cfError) => {
          HandleError(cfError, "failed to get installed mods:");
        });
      }, (cfError) => {
        HandleError(cfError);
      });
    }

    public void Request_GetCategoriesOfClass(GetCategoriesFilter getCategoriesFilter,
                                             Action<List<Category>> onSuccess) {
      GlobalLoader.Instance?.SetActive(true);
      _cfcoreService.Api.GetCategories(
          getCategoriesFilter,
          (response) => {
            if (response != null)
              onSuccess(response.Data);
            GlobalLoader.Instance?.SetActive(false);
          },
          (error) => {
            GlobalLoader.Instance?.SetActive(false);
            CFCoreError cfError = new CFCoreError(error);
            HandleError(cfError,"Eternal",()=> Request_GetCategoriesOfClass( getCategoriesFilter,
                                              onSuccess));
          });
    }

    public void RequestMyCreations(Action<List<CreationItem>> OnSuccess,
                                  Action<CFCoreError> onFailure) {
      GlobalLoader.Instance?.SetActive(true);
      CurrentPlayerCreationsList = _cfcoreService.Creation.GetCreations();
      if (CurrentPlayerCreationsList == null) {
        CurrentPlayerCreationsList = new List<CreationItem>();
      }
      foreach (CreationItem cr in CurrentPlayerCreationsList) {
        Debug.Log(cr.Mod.Id);
      }

      RequestMyMods((mods) => {
        foreach (Mod mod in mods) {
          if (CurrentPlayerCreationsList.Find((creation) => creation.Mod.Id == mod.Id) == null) {
            CurrentPlayerCreationsList.Add(new CreationItem() { Mod = mod, Revision = null });
          }
        }
        GlobalLoader.Instance?.SetActive(false);
        OnSuccess(CurrentPlayerCreationsList);
      },
        (error) => {
          onFailure(error);
          HandleError(error, "Request My Creation", () => RequestMyCreations(OnSuccess,
                                   onFailure));
        });
    }


 public void SearchMods(SearchModsFilter searchFilter,
      ApiRequestPagination pagination,Action<ApiResponseWithPagination<List<Mod>>> onSuccess,
       Action<CFCoreError> onFailure) {
      GlobalLoader.Instance?.SetActive(true);
      _cfcoreService.Api.SearchMods(
          searchFilter,
          pagination,
          (response) => {
            var gameVersions = response.Data;
            onSuccess(response);
            GlobalLoader.Instance?.SetActive(false);
          },
          (error) => {
            GlobalLoader.Instance?.SetActive(false);
            CFCoreError cfError = new CFCoreError(error);

            HandleError(cfError,"Eternal",
              ()=> SearchMods( searchFilter, pagination,onSuccess, onFailure));
            onFailure(cfError);
          });
    }

    public void GetFileUrl(uint modId, uint fileId,
                           Action<string> onSuccess,
                           Action<ApiResponseError> onFailure) {
      _cfcoreService.Api.GetModFileURL(modId, fileId, (response) => {
        onSuccess(response.Data);
      },
        (errorResponse) => {
          onFailure(errorResponse);
          CFCoreError error = new CFCoreError(errorResponse);
          HandleError(error, "Installation");
        });
    }

    public async void InstallMod(Mod mod,
                           Texture modIconTexture,
                           Action onSuccess,
                           Action<LibraryProgress> onProgress,
                           Action onFailure) {

      bool isOutOfDate= false;
      InstalledMod iMod = InstalledModList.Find(
              (m) => m.Details.Id.Equals(mod.Id));

      if (iMod != null) {
        isOutOfDate = iMod.Status == Base.Library.Models.Enums.InstalledModStatus.OutOfDate;
      }

      InstalledMod installingMod = await _cfcoreService.Library.Install(
             mod,
            (installedMod) => {
              UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log($"Done installing: {installedMod.InstalledFile.FileName}");
                CurrentInstallingModList.Remove(
                  CurrentInstallingModList.Find(imod => imod.Details.Id == installedMod.Details.Id));
                Request_GetInstalledMods();
                ToastManager.Instance.InstalledToast(mod.Name, modIconTexture,isOutOfDate);
                onSuccess();
              });
            },
            (libraryProgress) => {
              UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log("progress is : " + libraryProgress.State);
                onProgress(libraryProgress);
              });
            },
             (error, installedMod) => {
               UnityMainThreadDispatcher.Instance().Enqueue(() => {
                 HandleError(error, "Installation");
                 CurrentInstallingModList.Remove(CurrentInstallingModList.Find(imod => imod.Details.Id == installedMod.Details.Id));
                 onFailure();
               });
             });

      CurrentInstallingModList.Add(installingMod);
    }

    public void UninstallMod(Mod ModData, Action onSuccess, Action onFailure) {
      _cfcoreService.Library.Uninstall(
          ModData.Id,
          (installedMod) => {
            Debug.Log($"Done uninstalling: {installedMod.Details.Name}");
            InstalledModList.Remove(InstalledModList.Find(imod => imod.Details.Id == ModData.Id ));
            Request_GetInstalledMods();
            onSuccess();
          },
          (error) => {
            HandleError(error, "Uninstallation");
            onFailure();
          });
    }

   public void CancelInstallation(Mod mod , Action OnSuccess) {
      InstalledMod installedMod = CurrentInstallingModList.Find(imod => imod.Details.Id == mod.Id);
      if (installedMod == null) {
        OnSuccess();
        return;
      }

      _cfcoreService.Library.CancelInstallation(
        installedMod,
        OnSuccess,
        (error) => HandleError(error, "Cancel Installation"));
    }

    public void ThumbsUpMod(uint modID, ThumbsUpDirection isThumbsUp,
                            Action OnVoteSuccess) {
      _cfcoreService.Api.UpdateThumbsUp(
        modID,
        isThumbsUp,
        OnVoteSuccess,
        (error) => {
          CFCoreError cFCoreError = new CFCoreError(error);
          HandleError(cFCoreError, "ThumbUpMod");
        });
    }

    public void GetUserThumbUpMods(Action OnSuccess) {
      _cfcoreService.Api.Users.GetMyThumbsUp(
        (thumbsUpList) => {
          CurrentPlayerLikedModIdList = new List<uint>(thumbsUpList.Data);
          Debug.Log($"CurrentPlayerLikedModIdList updated with {CurrentPlayerLikedModIdList.Count} items ");
          OnSuccess();
        },
        (error) => {
          CFCoreError cFCoreError = new CFCoreError(error);
          HandleError(cFCoreError, "Get User ThumbUpMods"); });
    }

    #region Creations

    public void CreateMod(CreateModRequestDto modParams,
                          byte[] localAvatarFile, // can be null
                          Action<Mod> onSuccess,
                          Action<CFCoreError> onFailure) {
      _cfcoreService.Creation.CreateMod (modParams, localAvatarFile, null,
        (mod) => {
          onSuccess(mod);
        },
        (error) => {
          HandleError(error);
          onFailure(error);
        })
      ;
    }

  public void UpdateMod(Mod mod,
                        UpdateModRequestDto modParams,
                        byte[] localAvatarFile, // can be null
                        Action<Mod> onSuccess,
                        Action<CFCoreError> onFailure) {

      _cfcoreService.Creation.UpdateMod(mod, modParams, localAvatarFile, null,
        (m) => {
          onSuccess(m);
        }, (error) => {
          HandleError(error);
          onFailure(error);
        })
      ;
    }

    public void CancelUpload(CreationItem creation,
                             Action onSuccess,
                             Action<CFCoreError> onFailure) {
      _cfcoreService.Creation.CancelUploadRevision(
        creation,
        () => {
          onSuccess();
        }, (error) => {
          HandleError(error);
          onFailure(error);
        });
      onSuccess();
    }

    public void DeleteMod(Mod mod,
                          Action onSuccess,
                          Action<CFCoreError> onFailure) {

      _cfcoreService.Creation.DeleteMod(mod,
                                        onSuccess,
                                        (error) => {
                                          HandleError(error);
                                          onFailure(error);
                                        });
    }

    private void RequestMyMods(Action<List<Mod>> onSuccess,
                             Action<CFCoreError> onFailure) {
      _cfcoreService.Api.Users.GetMyMods((mods) => {
        onSuccess(mods.Data);
      }, (er) => { onFailure(new CFCoreError(er)); });
    }
    public void UploadNewFile(Mod mod, FileInfo file,
      UploadModFileRequestDto dto,
      Action<UploadedFileInfo> OnUploadSuccess,
      Action<Base.Services.Http.Models.HttpProgress> onProgress,
      Action<CFCoreError> onFailure) {
      _cfcoreService.Creation.UploadCreation
        (mod, file, dto, (fileInfo) => {
          OnUploadSuccess(fileInfo);
        }, (progress) => {
          onProgress(progress);
        }, (error) => {
          onFailure(error);
        });
    }

    #endregion Creations

    public void GetMe(Action<Me> onSuccess) {
      _cfcoreService.Api.Users.GetMe((me) => {
        onSuccess(me.Data);
      }, (error) => {
        CFCoreError cFCoreError = new CFCoreError(error);
        HandleError(cFCoreError, "Get Me"); });
    }

    public void GetMod(uint modId, Action<Mod> onSuccess,
                       Action<ApiResponseError> onFailure) {
      _cfcoreService.Api.GetMod(modId, (mod) => {
        onSuccess(mod.Data);
      },
        (error) => {
          CFCoreError cFCoreError = new CFCoreError(error);
          HandleError(cFCoreError, "Get Mod");
          onFailure(error);
        }
      );
    }

    public void SubmitReport(ReportReason reportReason, string description,
                             uint modId, Action onSuccess, Action onFailure) {
      SubmitReportDTO submitReportDTO = new SubmitReportDTO() {
        reportReasonId = reportReason.Id,
        reportText = description
      };

      _cfcoreService.Api.SubmitReport(
        submitReportDTO,
        modId,
        onSuccess,
        (error) => {
          CFCoreError cFCoreError = new CFCoreError(error);
          HandleError(cFCoreError);
          onFailure();
        });
    }

    public void GetReportReasons(Action<List<ReportReason>> onSuccess, Action onFailure) {
      _cfcoreService.Api.GetReportsReasons(
        (reasons) => {
         ReportReasons = reasons.Data;
         onSuccess(ReportReasons);
        },
        (error) => {
          CFCoreError cFCoreError = new CFCoreError(error);
          HandleError(cFCoreError);
          onFailure();
        });
    }

    public void GetTermsOfService(Action<Base.Api.Models.Terms.Terms> onSuccess,
      Action onFailure) {
      _cfcoreService.Api.GetTerms((terms) => onSuccess(terms.Data) ,
        (error) => {
          CFCoreError cFCoreError = new CFCoreError(error);
          HandleError(cFCoreError);
          onFailure();
        });
    }

    public void GetModDescriptions(uint ModId, Action<string> onSuccess,
      Action onFailure) {

      if (descriptionFlag) {
        _cfcoreService.Api.GetModDescription(ModId,
          DescriptionResponseType.UnityRichText,
          (description) => onSuccess(description.Data),
           (error) => {
             CFCoreError cFCoreError = new CFCoreError(error);
             HandleError(cFCoreError);
             onFailure();
           });
      } else {
        _cfcoreService.Api.GetMod(ModId, (mod) => onSuccess(mod.Data.Summary), (error) => {
          CFCoreError cFCoreError = new CFCoreError(error);
          HandleError(cFCoreError);
          onFailure();
        });
      }
    }

    #endregion Read APIs

    public bool IsAuthenticated() {
      return _cfcoreService.Authentication.IsAuthenticated;
    }

    #region Error Handling

    private void HandleError(CFCoreError error, string errorPrefix = "Eternal",Action retryAction=null) {
      if (error.ErrorCode == CFCoreErrorCodes.InstallCancelled ||
        (error.ApiError != null && error.ApiError.Cancelled)) {
        Debug.Log("The request was canceled");
        return;
      }

      if (error.Description != null)
        Debug.LogError($"{errorPrefix} error: {error.Description}");
      else
        Debug.LogError($"{errorPrefix} error: {error.ApiError.Description}");
      ErrorEvent?.Invoke(error,retryAction);
    }
    #endregion Error Handling

    private void OnGenerateAuthTokenSuccess() {
      Debug.Log("Login Success!");
      isSigned = true;
      LoginSuccessEvent?.Invoke();
      GetMe((me) => {
        _currentUser = me;
        CurrentUserChangedEvent?.Invoke(me);
      });
    }

    private void OnGenerateAuthTokenFailed(CFCoreError error) {
      Debug.LogError(error.ApiError.Description);
      if (LoginFailedEvent != null) {
        LoginFailedEvent(error.ApiError.ResourceExpired);
      }
    }

  }
}
