using System.Collections.Generic;
using UnityEngine;
using System;
using Overwolf.CFCore.UnityUI.Utils;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class ScreenshotPopup : MonoBehaviour {
    public List<GameObject> UIElements;
    public Texture screenshotTexture;
    private IScreenShot screenShotUtil;
    private Action OnPopupCloseAction;

    public void Initialize(Action OnClose, IScreenShot screenShotImplementation) {
      OnPopupCloseAction = OnClose;
      screenShotUtil = screenShotImplementation;
      screenShotUtil.ScreenCaptureSuccess += OnScreenshotSuccess;
      screenShotUtil.ScreenCaptureFail += OnScreenshotFailed;
    }

    public void TakeScreenshot() {
      screenShotUtil.TakeScreenshot();
      SetActiveUIElements(false);
    }

    private void SetActiveUIElements(bool active) {
      foreach (GameObject gameObject in UIElements) {
        gameObject.SetActive(active);
      }
    }

    private void OnScreenshotSuccess(Texture texture) {
      screenshotTexture = texture;
      ClosePopup();
    }

    private void OnScreenshotFailed(string error) {
      SetActiveUIElements(true);
      Debug.LogError(error);
    }

    public void ClosePopup() {
      screenShotUtil.ScreenCaptureSuccess -= OnScreenshotSuccess;
      screenShotUtil.ScreenCaptureFail -= OnScreenshotFailed;
      OnPopupCloseAction();
    }
  }
}