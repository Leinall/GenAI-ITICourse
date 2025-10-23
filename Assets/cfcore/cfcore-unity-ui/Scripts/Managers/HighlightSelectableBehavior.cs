using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Overwolf.CFCore.UnityUI.UIItems {


  public class HighlightSelectableBehavior : MonoBehaviour  ,ISelectHandler, IDeselectHandler {

    [SerializeField]
    private GameObject SelectedBorder;

    public void OnDeselect(BaseEventData eventData) {
      SelectedBorder.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData) {
      SelectedBorder.SetActive(true);
    }

  }
}