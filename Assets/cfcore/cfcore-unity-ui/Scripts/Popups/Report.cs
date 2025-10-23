using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.Base.Api.Models;
using UnityEngine.EventSystems;

namespace Overwolf.CFCore.UnityUI.Popups {

  public class Report : Popup {
#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] TMP_Dropdown Dropdown_ReportReason;
    [SerializeField] TMP_InputField InputField_Description;
    [SerializeField] Button Btn_Submit;
    [SerializeField] Button Btn_Cancel;
#pragma warning restore 0649

    private uint modId;

    // -------------------------------------------------------------------------
    void GetReportReasons() {
     if ( READ_APIManager.Instance.ReportReasons == null
        || READ_APIManager.Instance.ReportReasons.Count == 0) {
        READ_APIManager.Instance.GetReportReasons(
          (reasons) => InitializeReasons(reasons),
          ()=> { });
      }
     else {
        InitializeReasons(READ_APIManager.Instance.ReportReasons);
      }
    }

    // -------------------------------------------------------------------------
    private void InitializeReasons(List<ReportReason> reasons) {
      Dropdown_ReportReason.ClearOptions();

      List<string> sortOptions = new List<string>();
      sortOptions.Add("---");
      foreach (var reason in reasons) {
        sortOptions.Add(reason.Name);
      }

      Dropdown_ReportReason.AddOptions(sortOptions);
    }

    // -------------------------------------------------------------------------
    public void Initialize(uint modId ) {
      this.modId = modId;
      GetReportReasons();
    }

    // -------------------------------------------------------------------------
    public void CheckValidationOfSubmission() {
      var submitGui = Btn_Submit.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

      if (Dropdown_ReportReason.value == 0 || string.IsNullOrEmpty(InputField_Description.text)) {
        Btn_Submit.interactable = false;
        submitGui.color = new Color(1, 1, 1, 0.5f);
      } else {
        Btn_Submit.interactable = true;
        submitGui.color = Color.white;
      }

      ToggleSubmitButtonNavigation(Btn_Submit.interactable);
    }

    // -------------------------------------------------------------------------
    public void SubmitReport() {
      READ_APIManager.Instance.SubmitReport(
        READ_APIManager.Instance.ReportReasons[Dropdown_ReportReason.value - 1],
        InputField_Description.text,
        modId,
        () => {
          ToastManager.Instance.ActivateNormalToast("Report submitted!");
          ClosePopup();
        },
        () => { Debug.LogError("Failed Submit report"); });

    }

    // -------------------------------------------------------------------------
    protected override void SelectComponentOnView(string selectableName = null) {
      Dropdown_ReportReason.Select();
    }

    // -------------------------------------------------------------------------
    public override void OnCancel(BaseEventData eventData) {
      if (InputField_Description.isFocused) {
        Btn_Cancel.Select();
        return;
      }

      base.OnCancel(eventData);
    }

    // -------------------------------------------------------------------------
    private void ToggleSubmitButtonNavigation(bool enable) {
      var nav = Btn_Cancel.navigation;
      nav.selectOnRight = enable ? Btn_Submit : null;
      Btn_Cancel.navigation = nav;
    }
  }
}
