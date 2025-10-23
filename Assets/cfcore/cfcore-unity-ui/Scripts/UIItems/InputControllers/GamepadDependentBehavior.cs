using Overwolf.CFCore.CFCContext;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Overwolf.CFCore.UnityUI.InputController {
  [Serializable]
  public class GamepadObject {
    public GameObject GameObject;
    public bool IsVisibleWithGamepad;
  }

  public class GamepadDependentBehavior : MonoBehaviour {
    public List<GamepadObject> GamepadDependentObjects;

    // ---------------------------------------------------------------------------
    private void Awake() {
      if (GamepadDependentObjects == null ||
          GamepadDependentObjects.Count == 0) {
        return;
      }

      Context.Instance.AddListener(
        CFCContextConstants.IsGamepadControl, OnGamepadChange);
    }

    // ---------------------------------------------------------------------------
    private void OnDestroy() {
      Context.Instance.RemoveListener(
        CFCContextConstants.IsGamepadControl, OnGamepadChange);
    }

    // ---------------------------------------------------------------------------
    private void Start() {
      var isGamepadActive =
        Context.Instance.GetContext(CFCContextConstants.IsGamepadControl);
      ChangeObjectVisibility(isGamepadActive);
    }

    // ---------------------------------------------------------------------------
    private void OnGamepadChange(bool active) {
      ChangeObjectVisibility(active);
    }

    // ---------------------------------------------------------------------------
    private void ChangeObjectVisibility(bool isGamepadActive) {
      foreach (var component in GamepadDependentObjects) {
        var shouldBeVisible =
          component.IsVisibleWithGamepad ? isGamepadActive : !isGamepadActive;

        component.GameObject.SetActive(shouldBeVisible);
      }
    }
  }
}