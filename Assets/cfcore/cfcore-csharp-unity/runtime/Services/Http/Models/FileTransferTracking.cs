using Overwolf.CFCore.Base.Services.Http.Models;
using System;
using UnityEngine.Networking;

namespace Overwolf.CFCore.Unity.Services.Http.Models {
  // ---------------------------------------------------------------------------
  internal class FileTransferTracking {
    public ulong TransferredBytes;
    public float TimeStamp;
    public UnityWebRequest WebRequest;

    public Action<HttpProgress> onProgress;
    public Action<HttpResponse> onComplete;
  }
}
