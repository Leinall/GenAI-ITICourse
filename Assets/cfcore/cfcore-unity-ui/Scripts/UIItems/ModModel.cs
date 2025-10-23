using Overwolf.CFCore.Base.Api.Models.Enums;
using Overwolf.CFCore.Base.Library.Models;
using Overwolf.CFCore.UnityUI.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Overwolf.CFCore.Base.Api.Models {
  public class ModModel {
    private bool LikeButtonLock;
    public Mod modData;
    public bool isCorrupted;
    public bool isUpdateRequired;
    private string description;
    public Texture thumbnailTexture;

    uint _modLikes;
    public uint ModLikeCountNumber { get { return _modLikes; } }

    public string Description { get => description; }

    public ModModel(Mod mod) {
      modData = mod;
      _modLikes = mod.ThumbsUpCount ?? 0;
    }

    public void Install( Action OnSuccess, Action<LibraryProgress> OnProgress, Action OnFailure) {

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
    }

    public void GenerateDescription(Action<string> onSuccess) {
      if (description == null)
        READ_APIManager.Instance.GetModDescriptions(modData.Id,
          (desc) => { description = desc; onSuccess(desc); }
          , () => { });
      else
        onSuccess(description);
    }

    public void Like(bool isLiked, Action OnSuccess) {
      if (LikeButtonLock) {
        return;
      }

      if (READ_APIManager.Instance.IsSignedIn) {
        LikeButtonLock = true;
        ThumbsUpDirection direction = isLiked ? ThumbsUpDirection.Down :
                                                ThumbsUpDirection.Up;
        READ_APIManager.Instance.ThumbsUpMod(modData.Id, direction, () => {
          LikeButtonLock = false;

          if (!isLiked) { // like
            if (!READ_APIManager.Instance.CurrentPlayerLikedModIdList.Contains(modData.Id)) {
              READ_APIManager.Instance.CurrentPlayerLikedModIdList.Add(modData.Id);
              modData.ThumbsUpCount++;
              _modLikes++;
            }
          } else { // unlike
            if (READ_APIManager.Instance.CurrentPlayerLikedModIdList.Contains(modData.Id)) {
              READ_APIManager.Instance.CurrentPlayerLikedModIdList.Remove(modData.Id);
              modData.ThumbsUpCount--;
              _modLikes--;
              Debug.Log($"removing mod {modData.Id} and now it contains { READ_APIManager.Instance.CurrentPlayerLikedModIdList.Count} items");
            }
          }
          OnSuccess();
        });
      } else { //not signed in

        UIManager.Instance.SignIn();
      }
    }
  }
}