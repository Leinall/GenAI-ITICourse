using UnityEngine;
using TMPro;
using Overwolf.CFCore.UnityUI.Utils;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class ReadMore : Popup {
#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] TextMeshProUGUI Txt_ModName;
    [SerializeField] TextMeshProUGUI Txt_Description;
    [SerializeField] TextUtilsLoadExternalSprite loadExternalSpriteUtilities;

    private const string kCloseButtonName = "Btn_Close";
#pragma warning restore 0649

    public virtual void Setup(string modName, string description) {
      Txt_ModName.text = modName;
      loadExternalSpriteUtilities.ProccessText(description);
    }

    protected override void SelectComponentOnView(string selectableName = null) {
      base.SelectComponentOnView(kCloseButtonName);
    }
  }
}
