using UnityEngine;
using Overwolf.CFCore.Base.Api.Models.Requests;
using Overwolf.CFCore.Base.Api.Models;

namespace Overwolf.CFCore.UnityUI.Data {
  public class UploadCreationOptions {

    /// <summary>
    /// Is this option for edit mod or create a new one.
    /// </summary>
    public bool isEditMod;

    /// <summary>
    ///  If isEditMod = true, this is the mod we want to modify.
    /// </summary>
    public Mod mod;

    /// <summary>
    /// Is there a need to get new thumbnail.
    /// If false, will use the thumbnailTexture.
    /// </summary>
    public bool isUploadNewThumbnailFlag;

    /// <summary>
    /// A predefined texture for the mod. Will always be used unless 
    /// Can be null
    /// </summary>
    public Texture thumbnailTexture;

    /// <summary>
    /// Is there a new file to upload.
    /// </summary>
    public bool isUploadNewFile;

    /// <summary>
    /// The byte array that represent the file we should upload.
    /// Can only be null if isUploadNewFile is false.
    /// </summary>
    public string localFilePath;

    /// <summary>
    /// The DTO that contains the info of the file to upload.
    /// Can only be null if isUploadNewFile is false
    /// </summary>
    public UploadModFileRequestDto uploadFileDto;
  }
}
