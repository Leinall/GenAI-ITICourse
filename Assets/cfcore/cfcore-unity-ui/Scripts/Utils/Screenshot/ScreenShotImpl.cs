using UnityEngine;

using System.Collections;

namespace Overwolf.CFCore.UnityUI.Utils {
  public class ScreenShotImpl : IScreenShot {
    public event ScreenCapturedSuccess ScreenCaptureSuccess;
    public event ScreenCapturedFailed ScreenCaptureFail;
    private MonoBehaviour monoscript;

    public ScreenShotImpl(MonoBehaviour mono) {
      monoscript = mono;
      if (monoscript == null) {
        ScreenCaptureFail?.Invoke("Cannot take screenshot, monoscript is null");
      }
    }
    public void TakeScreenshot() {
      monoscript.StartCoroutine(WaitForTheEndFrame());
    }

    private IEnumerator WaitForTheEndFrame() {
      yield return new WaitForEndOfFrame();
      var texture = ScreenCapture.CaptureScreenshotAsTexture();
      ScreenCaptureSuccess?.Invoke(texture);
    }
  }
}
