using Overwolf.CFCore.UnityUI.InputController;
using Overwolf.CFCore.UnityUI.Managers;
using Overwolf.CFCore.UnityUI.UIItems;
using Overwolf.CFCore.CFCContext;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using Keyboard = UnityEngine.InputSystem.Keyboard;

namespace Overwolf.CFCore.UnityUI.Gamepad {
  public class InputController : MonoBehaviour {
    private CFCoreGamepadController gamepadController;
    [SerializeField]
    private EventSystem eventSystem;

    public List<Selectable> selectableMods;
    public List<Selectable> selectableTabs;
    public List<Selectable> selectableClasses;

    private List<Selectable> currentListOfSelectables;

    private NavigationMode currNavState;

    // -------------------------------------------------------------------------
    private void Start() {
      gamepadController = new CFCoreGamepadController();
      ActivateGamePad(UnityEngine.InputSystem.Gamepad.all.Count > 0);
      Initialize();
    }

    public void Test() {
      ChangeInputState(NavigationMode.Library);
    }

    // -------------------------------------------------------------------------
    private void Update() {
      if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
        ActivateGamePad(false);
      }

      if (Keyboard.current != null &&
          Keyboard.current.anyKey.wasPressedThisFrame) {
        ActivateGamePad(false);
      }

      var gamePad = UnityEngine.InputSystem.Gamepad.current;
      if (gamePad != null && gamePad.CheckStateIsAtDefaultIgnoringNoise()) {
        ActivateGamePad(true);
      }
    }

    // -------------------------------------------------------------------------
    private void OnEnable() {
      InputSystem.onDeviceChange += OnInputDeviceChange;
      InputSystem.onActionChange += OnInputActionChange;
    }

    // -------------------------------------------------------------------------
    private void OnDisable() {
      InputSystem.onDeviceChange -= OnInputDeviceChange;
      InputSystem.onActionChange -= OnInputActionChange;
    }

    // -------------------------------------------------------------------------
    private void OnDestroy() {
      UnsubscribeFromControllerEvents();
    }

    // -------------------------------------------------------------------------
    public void ActivateGamePad(bool active) {
      var gamepadPrompts = FindObjectOfType<GamepadPromptsScript>(true);

      if (active) {
        gamepadController.Enable();
        gamepadPrompts.gameObject.SetActive(true);
        SetGamepadTypePrompts(gamepadPrompts);
      } else {
        gamepadController.Disable();
        gamepadPrompts.gameObject.SetActive(false);
      }

      UpdateGamepadMode(active);
    }

    // -------------------------------------------------------------------------
    public void ChangeInputState(NavigationMode state) {
      currNavState = state;
    }

    // -------------------------------------------------------------------------
    private void Initialize() {
      SubscribeToControllerEvents();
    }

    // -------------------------------------------------------------------------
    private void UpdateGamepadMode(bool enabled) {
      Context.Instance.SetContext(
        CFCContextConstants.IsGamepadControl, enabled);
    }

    // -------------------------------------------------------------------------
    private void OnInputDeviceChange(InputDevice device,
                                     InputDeviceChange change) {
      if (device is not UnityEngine.InputSystem.Gamepad) {
        return;
      }

      if (change == InputDeviceChange.Removed) {
        ActivateGamePad(false);
      }

      if (change == InputDeviceChange.Added) {
        ActivateGamePad(true);
      }
    }

    // -------------------------------------------------------------------------
    private void OnInputActionChange(object arg1, InputActionChange arg2) {
      if (arg2 != InputActionChange.ActionPerformed) {
        return;
      }

      var action = arg1 as InputAction;
      if (action.activeControl.device is not UnityEngine.InputSystem.Gamepad) {
        return;
      }

      if (action.activeControl.IsPressed()) {
        ActivateGamePad(true);
      }
    }

    // -------------------------------------------------------------------------
    private void SetGamepadTypePrompts(GamepadPromptsScript gamepadPrompts) {
      var currDeviceType = UnityEngine.InputSystem.Gamepad.current?.GetType();
      if (currDeviceType == null) {
        Debug.LogWarning(
          "Tried to change gamepad prompts without active gamepad");
        return;
      }

      if (currDeviceType.IsSubclassOf(typeof(DualShockGamepad))) {
        gamepadPrompts.SetGamepadType(GamepadType.PlayStation);
      } else if (currDeviceType.IsSubclassOf(typeof(XInputController))) {
        gamepadPrompts.SetGamepadType(GamepadType.Xbox);
      }
    }


    #region ModBrowsing

    // ---------------------------------------------------------------------------
    private void SubscribeToControllerEvents() {
      gamepadController.Basic.Cancel.performed += OnCancelPerformed;
      gamepadController.Basic.CustomWest.performed += OnWestPerformed;
      gamepadController.Basic.CustomNorth.performed += OnNorthPerformed;

      gamepadController.ModManipulation.Menu.started += OnMenuButtonPressed;

      gamepadController.ModBrowsing.PaginationNavigation.started += OnPagination;
      gamepadController.ModBrowsing.ClassNavigation.started += OnClassChangeButtonPressed;
    }

    // ---------------------------------------------------------------------------
    private void UnsubscribeFromControllerEvents() {
      if (gamepadController == null) {
        return;
      }

      gamepadController.Basic.Cancel.performed -= OnCancelPerformed;
      gamepadController.Basic.CustomWest.performed -= OnWestPerformed;
      gamepadController.Basic.CustomNorth.performed -= OnNorthPerformed;

      gamepadController.ModManipulation.Menu.started -= OnMenuButtonPressed;

      gamepadController.ModBrowsing.PaginationNavigation.started -= OnPagination;
      gamepadController.ModBrowsing.ClassNavigation.started -= OnClassChangeButtonPressed;
    }

    // ---------------------------------------------------------------------------
    private void OnWestPerformed(CallbackContext context) {
      var selectedGameObject = GetSelectedGameObject();
      if (selectedGameObject == null) {
        return;
      }

      if (selectedGameObject.TryGetComponent<ModTileScript>(out var mod)) {
        if (!mod.isInstalled) {
          mod.Install();
        }
      }
    }

    // ---------------------------------------------------------------------------
    private void OnNorthPerformed(CallbackContext context) {

    }

    // ---------------------------------------------------------------------------
    private void OnCancelPerformed(CallbackContext context) {
      if (!eventSystem.currentSelectedGameObject) {
        Debug.LogWarning("Cancel performed without a selected game object");
        return;
      }

      ExecuteEvents.ExecuteHierarchy(eventSystem.currentSelectedGameObject,
                                     null,
                                     ExecuteEvents.cancelHandler);
    }

    // ---------------------------------------------------------------------------
    private void OnPagination(CallbackContext obj) {
      Debug.Log(obj);
      UIManager.Instance.Paginate(next: obj.action.ReadValue<float>() > 0);
    }


    #endregion ModBrowsing

    // -------------------------------------------------------------------------
    private void OnClassChangeButtonPressed(CallbackContext ctx) {
      var block = Context.Instance.GetContext(
        CFCContextConstants.IsModalViewOpen);
      if (block) {
        return;
      }

      int numberOfClasses = UIManager.Instance.ClassItemsGameObjects.Count;
      if (numberOfClasses <= 1)
        return;

      int currentClass = Context.Instance.GetContext(
        CFCContextConstants.LastSelectedClass);

      if (ctx.ReadValue<float>() > 0) {
        currentClass++;
        if (currentClass == numberOfClasses)
          currentClass = 0;
      } else {
        currentClass--;
        if (currentClass < 0)
          currentClass = numberOfClasses - 1;
      }

      UIManager.Instance.ClassItemsGameObjects[currentClass]
        .GetComponent<Button>().OnSubmit(null);
      Context.Instance.SetContext(
        CFCContextConstants.LastSelectedClass, currentClass);
    }

    // -------------------------------------------------------------------------
    private void OnMenuButtonPressed(CallbackContext ctx) {
      var menu = Context.Instance.GetContext(CFCContextConstants.ActiveMenu);
      if (menu == null) {
        return;
      }

      menu.SetActive(!menu.activeSelf);

      if (menu.activeSelf) {
        menu.transform.GetComponentInChildren<Selectable>().Select();
      } else {
        menu.transform.GetComponentInParent<Selectable>().Select();
      }
    }

    // -------------------------------------------------------------------------
    private GameObject GetSelectedGameObject() {
      if (eventSystem == null) {
        return null;
      }

      var gameObject = eventSystem.currentSelectedGameObject;
      return gameObject;
    }

    // -------------------------------------------------------------------------
    public enum NavigationMode {
      /// <summary>
      /// Main screen of every section (Browse, Installed, My Creations)
      /// </summary>
      Library,
      Popup, // scroll, next/prev(in detailed view ) and more?!
      Menu, //  basic
      ModDetailed
    }
  }
}