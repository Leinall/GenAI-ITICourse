using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using Overwolf.CFCore.Services.Common;
using System.Threading.Tasks;
using Overwolf.CFCore.Base.Services.Http;
using Overwolf.CFCore.Unity.Services.Http.Models;
using Overwolf.CFCore.Base.Services.FileSystem;
using Overwolf.CFCore.Base.Services.Http.Models;
using System.Threading;

namespace Overwolf.CFCore.Unity.Services.Http {
  // ---------------------------------------------------------------------------
  // Derive from |MonoBehaviour| in order to use |StartCoroutine| for tracking
  // transfer progress.
  class HttpFileServiceUnity : MonoBehaviour, IHttpFileService {
    private const float kTrackingIntervalInSeconds = 0.2f;
    private string kAbortResponseText = "Request aborted";
    private readonly Dictionary<uint, FileTransferTracking> _transferTracking;

    private IFileSystemService _fileSystemService;

    private uint _requestSequence = 0;

    // -------------------------------------------------------------------------
    // In order to instantiate an instance of a |MonoBehaviour| derived class.
    public static HttpFileServiceUnity Create(IFileSystemService fileService) {
      var instance = UnityGameObjectFactory.Create<HttpFileServiceUnity>();
      instance.Initialize(fileService);
      return instance;
    }

    // -------------------------------------------------------------------------
    public HttpFileServiceUnity() {
      _transferTracking = new Dictionary<uint, FileTransferTracking>();
    }

    // -------------------------------------------------------------------------
    // Ctor used for testing
    public HttpFileServiceUnity(IFileSystemService fileSystemService) : base() {
      Initialize(fileSystemService);
    }

    // -------------------------------------------------------------------------
#pragma warning disable CS1998
    public async Task<uint> DownloadFile(Uri url,
                                         FileInfo outputFile,
                                         Dictionary<string, string> headers,
                                         Action<HttpProgress> onProgress,
                                         Action<HttpResponse> onComplete,
                                         CancellationToken? token = null) {
      var webRequest = UnityWebRequest.Get(url);
      SetRequestHeaders(webRequest, headers);

      var downloadHandlerFile = new DownloadHandlerFile(outputFile.FullName);
      downloadHandlerFile.removeFileOnAbort = true;
      webRequest.downloadHandler = downloadHandlerFile;

      uint fileId = ++_requestSequence;
      var fileTransferTracking = new FileTransferTracking() {
        WebRequest = webRequest,
        TransferredBytes = 0,
        TimeStamp = Time.unscaledTime,
        onProgress = onProgress,
        onComplete = onComplete
      };

      _transferTracking[fileId] = fileTransferTracking;

      StartCoroutine(TrackProgress(webRequest, fileTransferTracking));

      var asyncOp = webRequest.SendWebRequest();
      asyncOp.completed += (op) => HandleWebRequestCompleted(fileId);

      return fileId;
    }

    // -------------------------------------------------------------------------
    public async Task<uint> PostFileWebForm(
      Uri url,
      UploadFileWebFormParams uploadFileParameters,
      Action<HttpProgress> onProgress,
      Action<HttpResponse> onComplete,
      CancellationToken? token = null) {

      WWWForm form = CreateForm(uploadFileParameters);
      UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
      return PerformWebFormRequest(webRequest,
                                   uploadFileParameters,
                                   onProgress,
                                   onComplete);
    }

    // -------------------------------------------------------------------------
    public async Task<uint> PutFileWebForm(
      Uri url,
      UploadFileWebFormParams uploadFileParameters,
      Action<HttpProgress> onProgress,
      Action<HttpResponse> onComplete,
      CancellationToken? token = null) {

      WWWForm form = CreateForm(uploadFileParameters);
      UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

      // NOTE(twolf): This is the simplest way I found to create a PUT request
      // with form data.
      webRequest.method = "PUT";

      return PerformWebFormRequest(webRequest,
                                   uploadFileParameters,
                                   onProgress,
                                   onComplete);
    }

    // -------------------------------------------------------------------------
    public void Abort(uint fileId) {
      if (!_transferTracking.TryGetValue(fileId,
                                         out FileTransferTracking track)) {
        return;
      }

      try {
        track.WebRequest.Abort();
      } catch (Exception) {
        // TODO(twolf): _logger.error
      }
    }

    // -------------------------------------------------------------------------
    private void Initialize(IFileSystemService fileSystemService) {
      _fileSystemService = fileSystemService;
    }

    // -------------------------------------------------------------------------
    private void SetRequestHeaders(UnityWebRequest request,
                                   Dictionary<string, string> headers) {
      foreach (var item in headers) {
        request.SetRequestHeader(item.Key, item.Value);
      }
    }

    // -------------------------------------------------------------------------
    private WWWForm CreateForm(
      UploadFileWebFormParams uploadFileParameters) {

      WWWForm form = new WWWForm();

      var additionalFields = uploadFileParameters.AdditionalFormFields;
      foreach (var field in additionalFields) {
        if (field.Value == null) {
          continue;
        }
        form.AddField(field.Key, field.Value);
      }

      var fileData = uploadFileParameters.File.Data;
      var fieldName = uploadFileParameters.FormFieldNameFile;
      form.AddBinaryData(fieldName, fileData);

      return form;
    }

    // -------------------------------------------------------------------------
    private uint PerformWebFormRequest(UnityWebRequest webRequest,
                                       UploadFileWebFormParams parameters,
                                       Action<HttpProgress> onProgress,
                                       Action<HttpResponse> onComplete) {
      SetRequestHeaders(webRequest, parameters.Headers);

      uint fileId = ++_requestSequence;
      var fileTransferTracking = new FileTransferTracking() {
        WebRequest = webRequest,
        TransferredBytes = 0,
        TimeStamp = Time.unscaledTime,
        onProgress = onProgress,
        onComplete = onComplete
      };

      _transferTracking[fileId] = fileTransferTracking;

      StartCoroutine(TrackProgress(webRequest, fileTransferTracking));

      var asyncOp = webRequest.SendWebRequest();
      asyncOp.completed += (op) => HandleWebRequestCompleted(fileId);
      return fileId;
    }

    // -------------------------------------------------------------------------
    private void HandleWebRequestCompleted(uint fileId) {
      if (!_transferTracking.TryGetValue(fileId,
                                         out FileTransferTracking tracker)) {
        return;
      }

      _transferTracking.Remove(fileId);
    }

    // -------------------------------------------------------------------------
    private IEnumerator TrackProgress(UnityWebRequest request,
                                      FileTransferTracking tracker) {
      var progress = new HttpProgress();

      while (!request.isDone) {
        uint prevProgress = progress.Progress;
        FillProgress(request, tracker, progress);

        if (prevProgress != progress.Progress) {
          tracker.onProgress(progress);
        }

        yield return new WaitForSeconds(kTrackingIntervalInSeconds);
      }

#if UNITY_2020_1_OR_NEWER
      if (request.result != UnityWebRequest.Result.ProtocolError &&
          request.result != UnityWebRequest.Result.ConnectionError) {
#else
      if (!request.isHttpError && !request.isNetworkError) {
#endif
        FillProgress(request, tracker, progress);
        tracker.onProgress(progress);
      }

      // TODO(twolf): Error when transfer bytes didn't reach total bytes
      string body = null;
      try {
        body = ExtractBody(tracker);
      } catch (Exception e) {
        Debug.Log(e.Message);
      }

      var response = new HttpResponse() {
        Cancelled = (request.error == kAbortResponseText),
        NonHttpError = false,
        StatusCode = request.responseCode,
        Body = body
      };

      tracker.onComplete(response);
    }

    // -------------------------------------------------------------------------
    private void FillProgress(UnityWebRequest request,
                              FileTransferTracking tracker,
                              HttpProgress progress) {
      float transferredProgress = ExtractTransferredProgress(request);
      if (transferredProgress <= 0) {
        return;
      }

      float curTimestamp = Time.unscaledTime;

      ulong transferredBytes = ExtractTransferredBytes(request);

      ulong deltaBytes = transferredBytes - tracker.TransferredBytes;
      float deltaTime = curTimestamp - tracker.TimeStamp;

      tracker.TransferredBytes = transferredBytes;
      tracker.TimeStamp = curTimestamp;

      progress.Progress = (uint)Math.Round(transferredProgress * 100);
      progress.TransferredBytes = transferredBytes;
      progress.BytesPerSecond = (ulong)(deltaBytes / deltaTime);
    }

    // -------------------------------------------------------------------------
    private float ExtractTransferredProgress(UnityWebRequest request) {
      if (request.downloadProgress > 0) {
        return request.downloadProgress;
      }

      if (request.uploadProgress > 0) {
        return request.uploadProgress;
      }

      return 0;
    }

    // -------------------------------------------------------------------------
    private ulong ExtractTransferredBytes(UnityWebRequest request) {
      if (request.downloadedBytes > 0) {
        return request.downloadedBytes;
      }

      if (request.uploadedBytes > 0) {
        return request.uploadedBytes;
      }

      return 0;
    }

    // -------------------------------------------------------------------------
    private static string ExtractBody(FileTransferTracking tracker) {
      if (tracker.WebRequest.downloadHandler == null)
        return null;

      if (tracker.WebRequest.downloadHandler is DownloadHandlerBuffer)
        return tracker.WebRequest.downloadHandler.text;

      return $"ResponseText not available for {tracker.WebRequest.downloadHandler.GetType().Name}";
    }
  }
}
