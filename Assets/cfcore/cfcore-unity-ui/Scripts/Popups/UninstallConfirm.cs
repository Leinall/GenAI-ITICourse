using UnityEngine;
using TMPro;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.Base.Api.Models;
using UnityEngine.UI;
using System.Linq;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class UninstallConfirm : Popup {

#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] TextMeshProUGUI Txt_Title;
    [SerializeField] TextMeshProUGUI Txt_Confirm;
#pragma warning restore 0649

    Mod uninstalledMod;
    System.Action onSuccess, onFailure;
    private const string kUninstallButtonName = "Btn_Uninstall";

    // -------------------------------------------------------------------------
    public virtual void SetupPopup( Mod mod, System.Action OnSuccess, System.Action OnFailure) {
      Txt_Title.text = "Uninstall " + mod.Name;
      Txt_Confirm.text = "Are you sure you want to uninstall " + mod.Name + "?";
      onSuccess = OnSuccess;
      onFailure = OnFailure;
      uninstalledMod = mod;
    }

    // -------------------------------------------------------------------------
    public void ConfirmUninstall() {
      READ_APIManager.Instance.UninstallMod(uninstalledMod, onSuccess, onFailure);
      ClosePopup();
    }

    // -------------------------------------------------------------------------
    protected override void SelectComponentOnView(string selectableName = null) {
      base.SelectComponentOnView(kUninstallButtonName);
    }
  }
}
