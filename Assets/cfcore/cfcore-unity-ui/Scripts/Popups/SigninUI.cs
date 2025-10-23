using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.Base.Common;
using UnityEngine.EventSystems;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class SigninUI : Popup {
    public const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
        + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] GameObject Panel_EmailInput;
    [SerializeField] TMP_InputField InputField_Email;
    [SerializeField] GameObject Alert_InvalidEmail;
    [SerializeField] GameObject AlertImg_InvalidEmail;
    [SerializeField] GameObject Btn_Continue;
    [SerializeField] GameObject Panel_CodeVerification;
    [SerializeField] TextMeshProUGUI Txt_EmailSent;
    [SerializeField] TMP_InputField InputField_Code1;
    [SerializeField] TMP_InputField InputField_Code2;
    [SerializeField] TMP_InputField InputField_Code3;
    [SerializeField] TMP_InputField InputField_Code4;
    [SerializeField] TMP_InputField InputField_Code5;
    [SerializeField] TMP_InputField InputField_Code6;
    [SerializeField] Sprite NormalInputSprite;
    [SerializeField] Sprite ErrorInputSprite;
    [SerializeField] Sprite SelectedInputSprite;

    [SerializeField] List<Image> InputImageList;
    [SerializeField] GameObject Alert_WrongCode;
    [SerializeField] GameObject Btn_Verify;
    [SerializeField] GameObject Notification_ResentCode;

    [SerializeField] GameObject Panel_CodeExpired;

    [SerializeField] GameObject Panel_PrivacyPolicy;

    [SerializeField] GameObject SignInErrorGameObject;
#pragma warning restore 0649

   public new void Start() {
      base.Start();
      READ_APIManager.CodeSentEvent += READ_APIManager_CodeSentEvent;
      READ_APIManager.LoginSuccessEvent += READ_APIManager_LoginSuccessEvent;
      READ_APIManager.LoginFailedEvent += READ_APIManager_LoginFaildEvent;
    }

    private void OnDestroy() {
      READ_APIManager.CodeSentEvent -= READ_APIManager_CodeSentEvent;
      READ_APIManager.LoginSuccessEvent -= READ_APIManager_LoginSuccessEvent;
      READ_APIManager.LoginFailedEvent -= READ_APIManager_LoginFaildEvent;
    }

    public static bool validateEmail(string email) {
      if (email != null)
        return Regex.IsMatch(email, MatchEmailPattern);
      else
        return false;
    }

    #region Email Input & Send Request for verification code
    public void EmailInputField_Updated(string str) {
      Alert_InvalidEmail.SetActive(false);
      AlertImg_InvalidEmail.SetActive(false);

      if (string.IsNullOrEmpty(str)) {
        Btn_Continue.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.5f);
      } else {
        Btn_Continue.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
      }
    }

    public void EmailInputField_EndEdit(string email) {
      if (validateEmail(email)) {
        Alert_InvalidEmail.SetActive(false);
        AlertImg_InvalidEmail.SetActive(false);
      } else {
        Alert_InvalidEmail.SetActive(true);
        AlertImg_InvalidEmail.SetActive(true);
      }
    }

    public void SendRequestForCode() {
      SignInErrorGameObject.SetActive(false);
      if (!validateEmail(InputField_Email.text))
        return;

      READ_APIManager.Instance.Request_SendCode(InputField_Email.text, OnAPIFailure);
    }

    private void READ_APIManager_CodeSentEvent(bool isResend) {
      SignInErrorGameObject.SetActive(false);
      if (isResend) {
        Notification_ResentCode.SetActive(true);
        StartCoroutine(HideNotification_ResendCode());
      } else {
        Panel_EmailInput.SetActive(false);

        Panel_CodeVerification.SetActive(true);
        Txt_EmailSent.text = InputField_Email.text;
        InputField_Code1.SetTextWithoutNotify("");
        InputField_Code2.SetTextWithoutNotify("");
        InputField_Code3.SetTextWithoutNotify("");
        InputField_Code4.SetTextWithoutNotify("");
        InputField_Code5.SetTextWithoutNotify("");
        InputField_Code6.SetTextWithoutNotify("");
        InputField_Code1.ActivateInputField();

        Alert_WrongCode.SetActive(false);

        Btn_Verify.GetComponent<Button>().interactable = false;
        Btn_Verify.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.5f);

        Notification_ResentCode.SetActive(false);
      }
    }

    public virtual void OnAPIFailure(CFCoreError error) {
      ToastManager.Instance.ForceDisableAllErrorToasts(isImidiate: true);
      SignInErrorGameObject.SetActive(true);

      if (Application.internetReachability == NetworkReachability.NotReachable) {
          error.Description = "An error has occurred.Please check your connection and try again";
      }

      SignInErrorGameObject.GetComponentInChildren<TMP_Text>().text = error.Description;
    }

    IEnumerator HideNotification_ResendCode() {
      yield return new WaitForSeconds(5f);
      Notification_ResentCode.SetActive(false);
    }
    #endregion

    #region Code Verification
    public void Code1_Input(string codeStr) {
      Alert_WrongCode.SetActive(false);
      InputField_Code2.SetTextWithoutNotify("");
      InputField_Code3.SetTextWithoutNotify("");
      InputField_Code4.SetTextWithoutNotify("");
      InputField_Code5.SetTextWithoutNotify("");
      InputField_Code6.SetTextWithoutNotify("");

      if (codeStr.Length == 6) {
        InputField_Code1.text = codeStr.Substring(0, 1);
        InputField_Code2.text = codeStr.Substring(1, 1);
        InputField_Code3.text = codeStr.Substring(2, 1);
        InputField_Code4.text = codeStr.Substring(3, 1);
        InputField_Code5.text = codeStr.Substring(4, 1);
        InputField_Code6.text = codeStr.Substring(5, 1);
      } else {
        if (IsMissingCode()) {
          if (codeStr.Length == 1)
            InputField_Code2.ActivateInputField();
        }
      }
    }

    public void Code1_EndEdit() {
      if (InputField_Code1.text.Length >= 1) {
        InputField_Code1.text = InputField_Code1.text.Substring(0, 1);
      }
    }

    public void Code2_Input(string codeStr) {
      if (IsMissingCode()) {
        if (codeStr.Length == 1)
          InputField_Code3.ActivateInputField();
        else if (codeStr.Length == 0)
          InputField_Code1.ActivateInputField();
      }
    }
    public void Code3_Input(string codeStr) {
      if (IsMissingCode()) {
        if (codeStr.Length == 1)
          InputField_Code4.ActivateInputField();
        else if (codeStr.Length == 0)
          InputField_Code2.ActivateInputField();
      }
    }
    public void Code4_Input(string codeStr) {
      if (IsMissingCode()) {
        if (codeStr.Length == 1)
          InputField_Code5.ActivateInputField();
        else if (codeStr.Length == 0)
          InputField_Code3.ActivateInputField();
      }
    }
    public void Code5_Input(string codeStr) {
      if (IsMissingCode()) {
        if (codeStr.Length == 1)
          InputField_Code6.ActivateInputField();
        else if (codeStr.Length == 0)
          InputField_Code4.ActivateInputField();
      }
    }
    public void Code6_Input(string codeStr) {
      if (codeStr.Length == 0)
        InputField_Code5.ActivateInputField();
      IsMissingCode();
    }

    bool IsMissingCode() {
      if (InputField_Code1.text.Length == 1 &&
          InputField_Code2.text.Length == 1 &&
          InputField_Code3.text.Length == 1 &&
          InputField_Code4.text.Length == 1 &&
          InputField_Code5.text.Length == 1 &&
          InputField_Code6.text.Length == 1) {
        Btn_Verify.GetComponent<Button>().interactable = true;
        Btn_Verify.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
        return false;
      } else {
        Btn_Verify.GetComponent<Button>().interactable = false;
        Btn_Verify.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.5f);
        return true;
      }
    }

    public void RadioToggleSelectedCodeImages(int imageNumber) {
      foreach (Image image in InputImageList) {
        image.sprite = NormalInputSprite;
      }
      if (imageNumber >= 0)
        InputImageList[imageNumber].sprite = SelectedInputSprite;
    }

    private void OnCodeError() {
      foreach (Image image in InputImageList) {
        image.sprite = ErrorInputSprite;
      }
    }

    public override void OnCancel(BaseEventData eventData) {
      if (Panel_PrivacyPolicy.activeSelf) {
        Panel_PrivacyPolicy.SetActive(false);
        return;
      }

      if (InputField_Email.isFocused) {
        Btn_Continue.GetComponent<Selectable>().Select();
        return;
      }

      base.OnCancel(eventData);
    }

    public void SendRequestForVerifyCode() {
      SignInErrorGameObject.SetActive(false);
      long.TryParse(InputField_Code1.text, out long code1);
      long.TryParse(InputField_Code2.text, out long code2);
      long.TryParse(InputField_Code3.text, out long code3);
      long.TryParse(InputField_Code4.text, out long code4);
      long.TryParse(InputField_Code5.text, out long code5);
      long.TryParse(InputField_Code6.text, out long code6);

      long code = code1 * 100000 + code2 * 10000 + code3 * 1000 + code4 * 100 + code5 * 10 + code6;

      READ_APIManager.Instance.Request_GenerateToken(InputField_Email.text, code, OnAPIFailure);
      Debug.Log(InputField_Email.text + " " + code);
    }

    public void ResendRequestForCode() {
      READ_APIManager.Instance.Request_SendCode(InputField_Email.text, OnAPIFailure, true);
    }

    public void BackToEmailInput() {
      Panel_CodeVerification.SetActive(false);
      Panel_EmailInput.SetActive(true);
    }

    protected override void SelectComponentOnView(string selectableName = null) {
      InputField_Email.Select();
    }

    private void READ_APIManager_LoginSuccessEvent() {
      Destroy(gameObject);
    }

    private void READ_APIManager_LoginFaildEvent(bool isCodeExpired) {
      OnCodeError();
      // TODO(twolf): We need to support other errors - like 50x errors
      if (isCodeExpired) {
        Panel_CodeVerification.SetActive(false);
        Panel_CodeExpired.SetActive(true);
      } else {
        Alert_WrongCode.SetActive(true);
      }
    }
    #endregion
  }
}
