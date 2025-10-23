using System.Collections.Generic;
using UnityEngine;
using Overwolf.CFCore.UnityUI.Popups;
using System.IO;
using Overwolf.CFCore.Base.Api.Models;
using Overwolf.CFCore.UnityUI.Data;
using Overwolf.CFCore.Base.Api.Models.Requests;
using UnityEngine.UI;
using Overwolf.CFCore.Base.Services.Http.Models;

namespace Overwolf.CFCore.UnityUI.SampleGame {
  public class SampleGameUploadMod : MonoBehaviour {
    private UploadCreationOptions options;
    private Mod currentMod;

    public Texture newerThumbnail;
    public GameObject UIGameObject;
    public GameObject PopupPrefab;
    private UploadModPopup uploadModPopupScript;
    private GameObject popupGameObject;

#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] InputField FileNameInputField;
#pragma warning restore 0649

    public void CreateNewMod() {
      CreateNewModOptions();
      OpenPopup();
    }

    public void UpdateModText() {
      CreateUpdateModTextsOptions(currentMod);
      OpenPopup();
    }

    public void UpdateWholeMod() {
      CreateUpdateModFilesOptions(currentMod);
      OpenPopup();
    }

    private void OpenPopup() {
      if (popupGameObject != null) {
        popupGameObject.SetActive(true);
        return;
      }

      popupGameObject = Instantiate(PopupPrefab,
                                        UIGameObject.transform);
      uploadModPopupScript = popupGameObject.GetComponentInChildren<UploadModPopup>();

      uploadModPopupScript.Initialize(options);
      SubscribeToEvents();
    }

    private void SubscribeToEvents() {
      if (options.isEditMod) {
        uploadModPopupScript.ModUpdatedEvent += OnModUploaded;
      }
      else {
        uploadModPopupScript.ModCreatedEvent += OnModUploaded;
      }

      if (options.isUploadNewFile) {
        uploadModPopupScript.FileProgressEvent += OnFileProgress;
        uploadModPopupScript.FileUploadedEvent += OnFileUploaded;
        uploadModPopupScript.FileUploadFailedEvent += OnFileUploadError;
      }
    }

    private void UnsubscribeToEvents() {
      if (uploadModPopupScript == null)
        return;
      if (options.isEditMod) {
        uploadModPopupScript.ModUpdatedEvent -= OnModUploaded;
      } else {
        uploadModPopupScript.ModCreatedEvent -= OnModUploaded;
      }

      if (options.isUploadNewFile) {
        uploadModPopupScript.FileProgressEvent -= OnFileProgress;
        uploadModPopupScript.FileUploadedEvent -= OnFileUploaded;
        uploadModPopupScript.FileUploadFailedEvent -= OnFileUploadError;
      }
    }

    private void Finished() {
      options = null;
      DestroyImmediate(uploadModPopupScript.gameObject);
    }

    private void CreateNewModOptions() {
      options = new UploadCreationOptions();
      string file = GetFile();
      if (file!=null) {
        options.localFilePath = file;
        options.isUploadNewFile = true;
      }

      options.isEditMod = false;
      options.isUploadNewThumbnailFlag = true;
      options.uploadFileDto = CreateNewModFileDto();
    }

    private void CreateUpdateModTextsOptions(Mod mod) {
      options = new UploadCreationOptions();
      options.mod = mod;
      options.isUploadNewFile = false;
      options.isEditMod = true;
      options.isUploadNewThumbnailFlag = false;
      options.thumbnailTexture = newerThumbnail;
    }

    private void CreateUpdateModFilesOptions(Mod mod) {
      options = new UploadCreationOptions();
      options.mod = mod;
      string file = GetFile();
      if (file != null) {
        options.localFilePath = file;
        options.isUploadNewFile = true;
      }

      options.isEditMod = true;
      options.isUploadNewThumbnailFlag = true;
      options.thumbnailTexture = newerThumbnail;
      options.uploadFileDto = CreateUpdateModFileDto();
    }

    private UploadModFileRequestDto CreateNewModFileDto() {
      UploadModFileRequestDto dto = new UploadModFileRequestDto();
      dto.Changelog = "This is a new file";
      dto.ChangelogType = Base.Api.Models.Enums.ChangelogMarkupType.Text;
      dto.DisplayName = "First file upload";
      dto.Filename = FileNameInputField.text;
      dto.GameVersionIds = new List<int>();
      dto.Note = "Note: will it work?";
      dto.ReleaseType = Base.Api.Models.Enums.FileReleaseType.Beta;
      return dto;
    }

    private UploadModFileRequestDto CreateUpdateModFileDto() {
      UploadModFileRequestDto dto = new UploadModFileRequestDto();
      dto.Changelog = "This is an updated file";
      dto.ChangelogType = Base.Api.Models.Enums.ChangelogMarkupType.Text;
      dto.DisplayName = "Second file upload";
      dto.Filename = FileNameInputField.text;
      dto.GameVersionIds = new List<int>();
      dto.Note = "Note: it works!";
      dto.ReleaseType = Base.Api.Models.Enums.FileReleaseType.Release;
      return dto;
    }

    private void OnModUploaded(Mod mod) {
      currentMod = mod;
      if(!options.isUploadNewFile) {
        UnsubscribeToEvents();
        Finished();
      }
      Debug.Log("mod uploaded mod id: " + currentMod.Id);
    }

    private void OnFileUploaded(UploadedFileInfo uploadedFileInfo) {
      UnsubscribeToEvents();
      Finished();
      Debug.Log("file uploaded: " + uploadedFileInfo.Filename);
    }

    private void OnFileUploadError(string error) {
      UnsubscribeToEvents();
      Finished();
      Debug.LogError("file upload error: " + error);
    }

    private void OnFileProgress(HttpProgress progress) {
      Debug.Log(string.Format("file progress : {0}% , Transfered bytes : {1}" ,
                               progress.Progress , progress.TransferredBytes));
    }

    private string GetFile() {
      Debug.Log(FileNameInputField);
      if (FileNameInputField != null) {
         string localFilePath=Path.Combine(Application.persistentDataPath,
                                   FileNameInputField.text);
        Debug.Log(localFilePath);
        return localFilePath;
      }
      return null;
    }
  }
}

