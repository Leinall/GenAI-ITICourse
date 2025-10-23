using Overwolf.CFCore.Base;
using Overwolf.CFCore.Base.Api.Common;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.Base.Api.Models.Enums;
using Overwolf.CFCore.Base.Api.Models.Filters;
using Overwolf.CFCore.Base.Models;
using System.IO;
using UnityEngine;

namespace Overwolf.CFCore.Unity.Tests {
  public class SampleTest : MonoBehaviour {
    private ICFCore _cfcoreService;

    private const int kGameId = 432;
    private const string kApiKey = "";
    private const int kMaxConcurrentInstallations = 3;

    // -------------------------------------------------------------------------
    void Start() {
      _cfcoreService = BootstrapUnity.Create(new Settings() {
        GameId = kGameId,
        ApiKey = kApiKey,
        ModsDirectory = GetModsDirectory(),
        UserDataDirectory = GetUserDataDirectory(),
        MaxConcurrentInstallations = kMaxConcurrentInstallations
      });

      Debug.Log("Before Init...");

      _cfcoreService.Initialize(RunSamples, (error) => {
        Debug.LogError($"Failed to initialize: {error}");
      });

      Debug.Log("After Init...");
    }

    // -------------------------------------------------------------------------
    private string GetModsDirectory() {
#if UNITY_EDITOR
      return Path.Combine(Directory.GetCurrentDirectory(), "cfcore/mods");
#else
  return Path.Combine(Application.dataPath, "mods");
#endif
    }

    // -------------------------------------------------------------------------
    private string GetUserDataDirectory() {
#if UNITY_EDITOR
      return Path.Combine(Directory.GetCurrentDirectory(), "cfcore/user_data");
#else
  return Path.Combine(Application.persistentDataPath, "cfcore/userData");
#endif
    }

    // -------------------------------------------------------------------------
    private void RunSamples() {
      _cfcoreService.Library.SyncWithServer(() => {
        Debug.Log("Successfully synchronized with server");

        _cfcoreService.Library.GetInstalledMods((installedmods) => {
          Debug.Log($"Successfully got installed mods {installedmods.Count}");

          _cfcoreService.Api.SearchMods(
            new SearchModsFilter() {
              SearchFilter = "Hello World Public",
              SortField = ModsSearchSortField.TotalDownloads,
              SortOrder = SortOrder.desc
            },
            new ApiRequestPagination(),
            (response) => {

              var gameVersions = response.Data;
              Debug.Log("Received versions info: " + gameVersions.Count);
            }, (error) => {
              Debug.LogError(error.Description);
            });


        }, (err) => {
          Debug.LogError($"failed to get installed mods: {err.Description}");
        });
      }, (err) => {
        Debug.LogError(err.Description);
      });
      // _cfcoreService.Api.GetMod(551661, (_) => { }, (_) => { });

      //_cfcoreService.Api.CreateFile(
      //  551661,
      //  new FileInfo(@"D:\unity-projects\cfcore2\cfcore\mods\1\tmp\3358_3554627.zip"),
      //  new UploadModFileRequestDto() {
      //    GameVersionIds = new List<int>() { -1 },
      //    Filename = "test123.zip",
      //    //ChangelogType = ChangelogMarkupType.Text,
      //    //Changelog = "This is the changelog",
      //    ReleaseType = FileReleaseType.Release
      //  },
      //  (response) => {
      //    Debug.Log($"FileId: {response.Data.FileId}");
      //  },
      //  (progress) => Debug.Log(progress.Progress),
      //  (error) => Debug.LogError(error.Description));

      //_cfcoreService.Api.CreateMod(new CreateModRequestDto() {
      //  ClassId = 5133,
      //  Name = "Unique OW Test 3",
      //  Summary = "Some summary here",
      //  Description = "blah blah",
      //  DescriptionType = MarkupType.PlainText,
      //  PrimaryGameCategoryId = 5134,
      //  IsExperimental = true
      //},
      //(response) => Debug.Log(response.Data.Id.ToString()),
      //(error) => Debug.LogError(error.Description));

      //_cfcoreService.Api.UpdateMod(
      //  551926,
      //  new UpdateModRequestDto() {
      //    ClassId = 5133,
      //    Name = "Unique OW Test 3",
      //    Summary = "Some summary here2",
      //    Description = "blah blah sheep",
      //    DescriptionType = MarkupType.PlainText,
      //    PrimaryGameCategoryId = 5134,
      //    IsExperimental = true
      //  },
      //(response) => Debug.Log(response.Data.Id.ToString()),
      //(error) => Debug.LogError(error.Description));

      //_cfcoreService.Library.SyncWithServer(() => { }, (err) => { });
      //_cfcoreService.Api.GetMod(
      //  3358,
      //  (response) => DownloadMod(response.Data),
      //  (error) => Debug.LogError(error.Description));

      //_cfcoreService.CFCoreApi.GetUserMods(
      //  (mods) => {
      //    int b = 5;
      //  },
      //  (error) => {
      //    int b = 5;
      //  });

      //_cfcoreService.CFCoreAuthentication.SendSecurityCode(
      //  "email@email.com",
      //  () => Debug.Log("Successfully sent security code"),
      //  (error) => Debug.LogError(error.Description));

      //_cfcoreService.CFCoreAuthentication.GenerateAuthToken(
      //  "email@email.com",
      //  905892,
      //  () => Debug.Log("ya"),
      //  (error) => { });

      //_cfcoreService.Library.SyncWithServer(() => {
      //  Debug.Log("Successfully synchronized with server");

      //  _cfcoreService.Library.GetInstalledMods((installedMods) => {
      //    int len = installedMods.Count;
      //  }, (err) => {
      //    Debug.LogError($"Failed to get installed mods: {err.Description}");
      //  });
      //}, (err) => {
      //  if (err.ApiError != null) {
      //    Debug.LogError("Failed to synchronize with server - Api issue");
      //  } else {
      //    Debug.LogError($"Failed to synchronize with server: {err.Description}");
      //  }
      //});

      //_cfcoreService.CFCoreApi.GetGame((response) => {
      //  Game game = response.Data;
      //  Debug.Log("Received game info: " + game.Name);
      //}, (error) => {
      //  Debug.LogError(error.Description);
      //});

      //_cfcoreService.CFCoreApi.GetModDescription(3358, (response) => {
      //  Debug.Log($"Got description: {response}");
      //}, (error) => {
      //});

    }

    private void DownloadMod(Mod mod) {
      _cfcoreService.Library.Install(
        mod,
        (installedMod) => Debug.Log($"Done installing: {installedMod.InstalledFile.FileName}"),
        (progress) => Debug.Log($"Progress: {progress.State.ToString()} ({progress.DataTransfer.Progress})"),
        (error, instaledMod) => Debug.LogError($"Installation error: {error.Description}"));
    }
  }
}