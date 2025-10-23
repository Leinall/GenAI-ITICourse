
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.UnityUI.View;
using Overwolf.CFCore.Base.Api.Models;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class DeleteConfirm : Popup {
    private Mod modToDelete;
    private const string kDeleteButtonName = "Btn_Delete";

    // -------------------------------------------------------------------------
    public void Initialize(Mod mod) {
      modToDelete = mod;
    }

    // -------------------------------------------------------------------------
    public void DeleteConfirmed() {
      READ_APIManager.Instance.DeleteMod(modToDelete,
                                         OnDelete,
                                         (er) => OnDelete());
    }

    // -------------------------------------------------------------------------
    protected override void SelectComponentOnView(string selectableName = null) {
      base.SelectComponentOnView(kDeleteButtonName);
    }

    // -------------------------------------------------------------------------
    private void OnDelete() {
      MyCreationsView.Instance.DeleteMod(modToDelete);
      ClosePopup();
    }
  } 
}
