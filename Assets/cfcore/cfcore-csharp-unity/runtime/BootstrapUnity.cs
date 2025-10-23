using System.IO;
using UnityEngine;
using Overwolf.CFCore.Base;
using Overwolf.CFCore.Base.Models;
using Overwolf.CFCore.Base.Services;
using Overwolf.CFCore.Base.Services.FileSystem;
using Overwolf.CFCore.Base.Services.Http;
using Overwolf.CFCore.Unity.Services.Http;
using Overwolf.CFCore.Shared.Services;
using Overwolf.CFCore.Base.Services.UserContextService;
using Overwolf.CFCore.Base.Api.Services;
using Overwolf.CFCore.Base.Api;
using Overwolf.CFCore.Base.Library.Services;
using Overwolf.CFCore.Base.Library;
using Overwolf.CFCore.Base.Authentication;
using Overwolf.CFCore.Base.Models.Enums;
using Overwolf.CFCore.Base.Services.Compression;
using Overwolf.CFCore.Base.Library.Services.DirectoryModeOperations;
using Overwolf.CFCore.Base.Services.Hashing;
using Overwolf.CFCore.Base.Creation;
using Overwolf.CFCore.Base.LocalCreation;
using Overwolf.CFCore.Base.Services.ConfigStorageService;
using Overwolf.CFCore.Base.LocalCreation.Services;

namespace Overwolf.CFCore.Unity {
  // ---------------------------------------------------------------------------
  public static class BootstrapUnity {
    private static readonly string kUserAgentPrefix = "cfcore.unity";

    // -------------------------------------------------------------------------
    // TODO(twolf): Use Extenject for dependency injection
    public static ICFCore Create(Settings settings) {
      ISettingsService settingsService = new SettingsService(
        settings,
        CreateInternalSettings(settings));

      IFileSystemService fileSystemService =
                        new ThreadSafeFileService(new AsyncFileSystemService());
      IHttpService httpService = new HttpServiceUnity();
      IJsonService jsonService = new JsonService();
      IHashingService hashingService = new HashingService();
      IFingerprintService fingerprintService = new FingerprintService(
        fileSystemService, hashingService);
      IZipService zipService = new SharpZipService(fileSystemService);
      IHttpFileService httpFileService = HttpFileServiceUnity.Create(
        fileSystemService);

      IUserContextService userContextService = new UserContextService(
        settingsService,
        jsonService,
        fileSystemService);

      IApiHelperService apiHelperService = new ApiHelperService(
        settingsService,
        httpService,
        userContextService,
        jsonService);

      IApiUsers apiUsersService = new ApiUsersImpl(settingsService,
                                                   apiHelperService);

      ICFCoreApi cfcoreApi = new CFCoreApiImpl(settingsService,
                                         apiHelperService,
                                         apiUsersService);

      IInstalledModsService installedModsService = new InstalledModsService(
        settingsService,
        cfcoreApi,
        fileSystemService,
        jsonService);

      IModsDirectoryInfoService modsDirInfoService = 
        new ModsDirectoryInfoService(settingsService);

      var dirModeOperations = CreateDirectoryModeOperationsImpl(
        modsDirInfoService,
        settingsService,
        fileSystemService);

      IModsInstallationService modsInstallationService =
        new ModsInstallationService(modsDirInfoService,
                                    settingsService,
                                    httpFileService,
                                    fileSystemService,
                                    apiHelperService,
                                    hashingService,
                                    zipService,
                                    dirModeOperations);

      ILocalCreationValidationsService localCreationValidationService =
        new LocalCreationValidationsService(settingsService);

      CFCoreAuthenticationImpl auth = new CFCoreAuthenticationImpl(
        cfcoreApi,
        userContextService);

      CFCoreCreationImpl creationManager =
        new CFCoreCreationImpl(settingsService,
        fileSystemService,
        httpFileService,
        jsonService,
        apiHelperService);

      CFCoreLocalCreationAsyncImpl localCreations =
        new CFCoreLocalCreationAsyncImpl(
          filename => new JsonConfigStorageService(filename,
                                                   settingsService,
                                                   fileSystemService,
                                                   jsonService),
          settingsService,
          modsDirInfoService,
          localCreationValidationService,
          fileSystemService,
          jsonService,
          creationManager,
          hashingService,
          fingerprintService,
          zipService,
          httpFileService,
          apiHelperService,
          auth,
          cfcoreApi);

      CFCoreLibraryImpl library = new CFCoreLibraryImpl(
        installedModsService,
        modsInstallationService,
        localCreations);

      return new CFCoreImpl(cfcoreApi, library, library,
                            auth, auth, creationManager, creationManager,
                            localCreations, localCreations);
    }

    // -------------------------------------------------------------------------
    private static InternalSettings CreateInternalSettings(Settings settings) {
      var assemblyVersion = typeof(ICFCore).Assembly.GetName().Version;

      return new InternalSettings() {
        ApiBaseUrl = "https://api.curseforge.com",
        UserAgent = $"{kUserAgentPrefix} ({assemblyVersion})",
        Platform = Application.platform.ToString(),
      };
    }

    // -------------------------------------------------------------------------
    private static string GetRelativeModsPath(Settings settings) {
      if (settings.ModsDirectoryMode != ModsDirectoryMode.CFCoreStructure) {
        return "";
      }

      // We add the game id to the path
      return settings.GameId.ToString();
    }

    // -------------------------------------------------------------------------
    private static DirectoryInfo CalcLocalModsDirectory(Settings settings) {
      string modsDirectory = Path.Combine(settings.ModsDirectory,
                                          GetRelativeModsPath(settings));
      return new DirectoryInfo(modsDirectory);
    }

    // -------------------------------------------------------------------------
    private static IDirectoryModeOperations CreateDirectoryModeOperationsImpl(
      IModsDirectoryInfoService modsDirectoryInfoService,
      ISettingsService settingsService,
      IFileSystemService fileSystemService) {

      var settings = settingsService.Settings;

      switch (settings.ModsDirectoryMode) {
        case ModsDirectoryMode.CFCoreStructure:
        case ModsDirectoryMode.CFCoreStructureWithoutGameId:
          return new DirectoryModeOperationsCFCStructure(modsDirectoryInfoService,
                                                         fileSystemService);
        case ModsDirectoryMode.Flat:
          return new DirectoryModeOperationsFlat(modsDirectoryInfoService,
                                                 fileSystemService);
      }

      return null;
    }
  }
}
