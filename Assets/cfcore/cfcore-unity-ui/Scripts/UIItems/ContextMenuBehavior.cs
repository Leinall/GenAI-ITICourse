using Overwolf.CFCore.CFCContext;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class ContextMenuBehavior : MonoBehaviour, ICancelHandler {
    // -------------------------------------------------------------------------
    private void Start() {
      var buttons = GetComponentsInChildren<Button>(true);
      foreach (var button in buttons) {
        button.onClick.AddListener(() => gameObject.SetActive(false));
      }
    }

    // -------------------------------------------------------------------------
    void Update() {
      if (!gameObject.activeSelf) {
        return;
      }

      if (Input.GetMouseButtonUp(0)) {
        gameObject.SetActive(false);
      }
    }

    // -------------------------------------------------------------------------
    private void OnEnable() {
      if (!Context.Instance.GetContext(CFCContextConstants.IsGamepadControl)) {
        return;
      }

      transform.GetComponentInChildren<Selectable>().Select();
    }

    // -------------------------------------------------------------------------
    public void OnCancel(BaseEventData eventData) {
      gameObject.SetActive(false);
      transform.GetComponentInParent<Selectable>().Select();
    }
  }
}
