using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Overwolf.CFCore.Base.Api.Models;
using UnityEngine.Networking;
using Overwolf.CFCore.UnityUI.UIItems;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class ScaledScreenshot : Popup {

#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] RawImage Img_Screenshot;
    [SerializeField] TextMeshProUGUI Txt_ScreenshotCount;

    [SerializeField] TextMeshProUGUI Txt_Description;
    [SerializeField] GameObject Btn_Prev;
    [SerializeField] GameObject Btn_Next;
    [SerializeField] GameObject Btn_Close;
#pragma warning restore 0649

    int currentScreenshotIndex;
    Mod modData;

    public void Setup(int screenshotIndex, Mod mod) {
      modData = mod;

      StartCoroutine(Load_Img(modData.Screenshots[screenshotIndex].Url, Img_Screenshot));
      Txt_ScreenshotCount.text = screenshotIndex + 1 + "/" + modData.Screenshots.Count;
      Txt_Description.text = modData.Screenshots[screenshotIndex].Description;


      if (modData.Screenshots.Count > 0) {
        if (modData.Screenshots.Count == 1) {
          Btn_Prev.SetActive(false);
          Btn_Next.SetActive(false);
        } else {
          Btn_Prev.SetActive(true);
          Btn_Next.SetActive(true);
        }
      } else {
        Btn_Prev.SetActive(false);
        Btn_Next.SetActive(false);
      }
    }

    public void PrevScreenshot() {
      ModDetailViewer.Instance.PrevScreenshot();

      currentScreenshotIndex = currentScreenshotIndex - 1 < 0 ? modData.Screenshots.Count - 1 : currentScreenshotIndex - 1;

      StartCoroutine(Load_Img(modData.Screenshots[currentScreenshotIndex].Url, Img_Screenshot));
      Txt_ScreenshotCount.text = currentScreenshotIndex + 1 + "/" + modData.Screenshots.Count;
      Txt_Description.text = modData.Screenshots[currentScreenshotIndex].Description;
    }

    public void NextScreenshot() {
      ModDetailViewer.Instance.NextScreenshot();

      currentScreenshotIndex = currentScreenshotIndex + 1 > modData.Screenshots.Count - 1 ? 0 : currentScreenshotIndex + 1;

      StartCoroutine(Load_Img(modData.Screenshots[currentScreenshotIndex].Url, Img_Screenshot));
      Txt_ScreenshotCount.text = currentScreenshotIndex + 1 + "/" + modData.Screenshots.Count;
      Txt_Description.text = modData.Screenshots[currentScreenshotIndex].Description;
    }

    protected override void SelectComponentOnView(string selectableName = null) {
      base.SelectComponentOnView(nameof(Btn_Close));
    }

    IEnumerator Load_Img(string url, RawImage targetImg) {
      targetImg.GetComponent<Animator>().ResetTrigger("fadeout");
      targetImg.GetComponent<Animator>().ResetTrigger("fadein");

      targetImg.GetComponent<Animator>().SetTrigger("fadeout");

      UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
      yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
      if (request.result == UnityWebRequest.Result.ProtocolError ||
          request.result == UnityWebRequest.Result.ConnectionError) {
#else
      if (request.isHttpError || request.isNetworkError) {
#endif
        Debug.LogError("Thumbnail loading failed!\n" + request.error);
      } else {
        yield return new WaitForSeconds(0.25f);

        targetImg.GetComponent<Animator>().SetTrigger("fadein");
        targetImg.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
      }
      request.Dispose();
    }
  }
}
