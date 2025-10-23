using UnityEngine;

namespace Overwolf.CFCore.UnityUI.Utils {
  public delegate void ScreenCapturedSuccess(Texture texture);
  public delegate void ScreenCapturedFailed(string errorMessage);
  public interface IScreenShot {
    /// <summary>
    /// Returns the path of the screenshot
    /// </summary>
    /// <returns></returns>
    event ScreenCapturedSuccess ScreenCaptureSuccess;

    /// <summary>
    /// Returns fail Message
    /// </summary>
    event ScreenCapturedFailed ScreenCaptureFail;

    void TakeScreenshot();
  }
}  
