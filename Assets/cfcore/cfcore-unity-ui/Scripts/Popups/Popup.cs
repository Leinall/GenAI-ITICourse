using Overwolf.CFCore.CFCContext;
using Overwolf.CFCore.UnityUI.Themes;
using Overwolf.CFCore.UnityUI.UIItems;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class Popup : MonoBehaviour, IPointerClickHandler, ICancelHandler {
    private GameObject origin;

    public void Start() {

      var theme = EternalUITheme.Instance;
      if (theme != null) {
        if (theme.ColorTheme != null) {
          var setters = GetComponentsInChildren<EternalColorSetter>(true);
          foreach (EternalColorSetter setter in setters) {
            setter.SetupColor(theme.ColorTheme);
          }
        }

        if (theme.ShapeTheme != null) {
          var setters = GetComponentsInChildren<EternalShapeSetter>(true);
          foreach (EternalShapeSetter setter in setters) {
            setter.SetupShape(theme.ShapeTheme);
          }
        }
      }

      SelectComponentOnView();
      Context.Instance.SetContext(CFCContextConstants.IsModalViewOpen, true);
    }

    // -------------------------------------------------------------------------
    public void ClosePopup() {
      Destroy(gameObject);
      Context.Instance.SetContext(CFCContextConstants.IsModalViewOpen, false);

      if (origin == null) {
        Debug.LogWarning("No popup origin found");
        return;
      }

      origin.GetComponent<Selectable>().Select();
    }

    // -------------------------------------------------------------------------
    public void OnPointerClick(PointerEventData eventData) {
      if (gameObject.Equals(eventData.pointerCurrentRaycast.gameObject))
        ClosePopup();
    }

    // -------------------------------------------------------------------------
    public virtual void OnCancel(BaseEventData eventData) {
      ClosePopup();
    }
    
    // -------------------------------------------------------------------------
    public void SetPopupOrigin(GameObject gameObject) {
      var contextMenu = gameObject.GetComponentInParent<ContextMenuBehavior>();
      if (contextMenu != null) {
        gameObject = contextMenu.GetComponentInParent<Selectable>().gameObject;
      }

      origin = gameObject;
    }

    // -------------------------------------------------------------------------
    protected virtual void SelectComponentOnView(string selectableName = null) {
      if (!String.IsNullOrEmpty(selectableName)) {
        var specificSelectable = transform.GetComponentsInChildren<Selectable>()
          .FirstOrDefault(component => component.name == selectableName);

        if (specificSelectable != null) {
          specificSelectable.Select();
          return;
        }

        Debug.LogWarning($"Failed to find specific selectable {selectableName}");
      }

      var selectableComponents = transform.GetComponentsInChildren<Selectable>();
      if (selectableComponents?.Length == 0) {
        Debug.LogWarning("Could not find a component to select");
      }

      selectableComponents.First().Select();
    }
  }
}
