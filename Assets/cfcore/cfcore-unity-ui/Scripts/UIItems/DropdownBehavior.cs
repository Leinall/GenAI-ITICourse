using UnityEngine;
using UnityEngine.EventSystems;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class DropdownBehavior : MonoBehaviour, ICancelHandler {
#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] GameObject Icon_Closed;
    [SerializeField] GameObject Icon_Opened;

#pragma warning restore 0649

    void Update() {
      GameObject obj = GameObject.Find("Dropdown List");
      if (obj != null && obj.transform.parent.Equals(transform)) {
        Icon_Opened.SetActive(true);
        Icon_Closed.SetActive(false);
      } else {
        Icon_Opened.SetActive(false);
        Icon_Closed.SetActive(true);
      }
    }

    // -------------------------------------------------------------------------
    public void OnCancel(BaseEventData eventData) {
      if (Icon_Closed.activeSelf) {
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject,
                                       null,
                                       ExecuteEvents.cancelHandler);
      }
    }
  }
}
