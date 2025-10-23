using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Overwolf.CFCore.Base.Services.Http;
using Overwolf.CFCore.Base.Services.Http.Models;

namespace Overwolf.CFCore.Unity.Services.Http {
  // ---------------------------------------------------------------------------
  class HttpServiceUnity : IHttpService {
    // -------------------------------------------------------------------------
    public void Get(Uri url,
                    Dictionary<string, string> headers,
                    Action<HttpResponse> onComplete) {
      UnityWebRequest request = UnityWebRequest.Get(url);

      SetRequestHeaders(request, headers);

      var asyncOp = request.SendWebRequest();

      asyncOp.completed += (op) => HandleWebRequestCompleted(
        op as UnityWebRequestAsyncOperation,
        onComplete);
    }

    // -------------------------------------------------------------------------
    public void Post(Uri url,
                     Dictionary<string, string> headers,
                     byte[] data,
                     Action<HttpResponse> onComplete) {

      PerformUploadMethod(UnityWebRequest.kHttpVerbPOST,
                          url,
                          headers,
                          data,
                          onComplete);
    }

    // -------------------------------------------------------------------------
    public void Put(Uri url,
                    Dictionary<string, string> headers,
                    byte[] data,
                    Action<HttpResponse> onComplete) {

      PerformUploadMethod(UnityWebRequest.kHttpVerbPUT,
                          url,
                          headers,
                          data,
                          onComplete);
    }
    // -------------------------------------------------------------------------
    public void Delete(Uri url,
                       Dictionary<string, string> headers,
                       Action<HttpResponse> onComplete) {

      UnityWebRequest request = UnityWebRequest.Delete(url);
      SetRequestHeaders(request, headers);

      var asyncOp = request.SendWebRequest();

      asyncOp.completed += (op) => HandleWebRequestCompleted(
        op as UnityWebRequestAsyncOperation,
        onComplete);
    }

    // -------------------------------------------------------------------------
    private void SetRequestHeaders(UnityWebRequest request,
                                   Dictionary<string, string> headers) {
      foreach (var item in headers) {
        request.SetRequestHeader(item.Key, item.Value);
      }
    }

    // -------------------------------------------------------------------------
    private void PerformUploadMethod(string method,
                                     Uri url,
                                     Dictionary<string, string> headers,
                                     byte[] data,
                                     Action<HttpResponse> onComplete) {
      UnityWebRequest request = new UnityWebRequest(url, method);

      SetRequestHeaders(request, headers);

      request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
      request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

      var asyncOp = request.SendWebRequest();

      asyncOp.completed += (op) => HandleWebRequestCompleted(
        op as UnityWebRequestAsyncOperation,
        onComplete);
    }

    // -------------------------------------------------------------------------
    private void HandleWebRequestCompleted(UnityWebRequestAsyncOperation op,
                                           Action<HttpResponse> onComplete) {
      if (op == null) {
        onComplete(new HttpResponse() {
          NonHttpError = true,
          ExtraInfo = "Missing async operation"
        });

        return;
      }

      var unityWebRequest = op.webRequest;
      var body = unityWebRequest.downloadHandler == null ?
        null : unityWebRequest.downloadHandler.text;

      var response = new HttpResponse() {
        NonHttpError = false,
        StatusCode = unityWebRequest.responseCode,
        Body = body
      };

      onComplete(response);
    }
  }
}
