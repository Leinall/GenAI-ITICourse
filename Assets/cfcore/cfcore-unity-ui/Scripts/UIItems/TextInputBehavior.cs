using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class TextInputBehavior : MonoBehaviour, ICancelHandler {
    [SerializeField]
    private Selectable objectToSelect;

    public void OnCancel(BaseEventData eventData) {
      if (objectToSelect == null) {
        return;
      }

      objectToSelect.Select();
    }
  }
}