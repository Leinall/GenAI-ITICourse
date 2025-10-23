using Overwolf.CFCore.Base.Library.Models;

namespace Overwolf.CFCore.Actions
{
  public class CFCInstallAction : ICFCAction
  {
    //int id = 0;

    public event InstallationProgress OnInstallationProgress;
    public event CFCActionStart onActionStarted;
    public event CFCActionComplete onActionComplete;

    public void StartAction()
    {
      throw new System.NotImplementedException();
    }

    /*
    public void StartAction() {

      Mod modData;
      modData = (Mod)CFCContext.Instance.ContextDictionary["ModData"];

      if (modData.LatestFiles != null && modData.LatestFiles.Count > 0 && modData.LatestFiles[0].DownloadUrl != null) {
        READ_APIManager.Instance.InstallMod(
              modData,
              thumbnailTexture,
              OnSuccess,
              (progress) => OnProgress(progress),
              OnFailure
              );
      } else {
        if (modData.LatestFiles == null || modData.LatestFiles[0] == null) {
          modData.LatestFiles = new System.Collections.Generic.List<File>();
          modData.LatestFiles.Add(modData.LatestFile);
          modData.LatestFiles[0].Hashes = modData.LatestFile.Hashes;
        }

        if (modData.LatestFiles[0].DownloadUrl == null) {
          READ_APIManager.Instance.GetFileUrl(modData.Id, modData.LatestFile.Id,
            (url) => {
              modData.LatestFile.DownloadUrl = url;
              modData.MainFileId = modData.LatestFiles[0].Id;
              READ_APIManager.Instance.InstallMod(
               modData,
               thumbnailTexture,
               OnSuccess,
               (progress) => OnProgress(progress),
               OnFailure
               );
            },
            (error) => {
              Debug.LogError("Install failed : Download url is empty.Does your API key have" +
                " permissions for downloading this mod?");
              OnFailure();
            }
            );
        } else {
          READ_APIManager.Instance.InstallMod(
              modData,
              thumbnailTexture,
              OnSuccess,
              (progress) => OnProgress(progress),
              OnFailure
              );
        }

    }
      }*/
  }

  public delegate void InstallationProgress(LibraryProgress libraryProgress);
}