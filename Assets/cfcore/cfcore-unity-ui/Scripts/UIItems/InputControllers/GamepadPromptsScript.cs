using Overwolf.CFCore.UnityUI.Gamepad;
using System.Collections.Generic;
using UnityEngine;

namespace Overwolf.CFCore.UnityUI.InputController {
  public class GamepadPromptsScript : MonoBehaviour {
    [SerializeField]
    private GameObject southButtonPrompt;
    [SerializeField]
    private GameObject northButtonPrompt;
    [SerializeField]
    private GameObject westButtonPrompt;
    [SerializeField]
    private GameObject eastButtonPrompt;
    [SerializeField]
    private GameObject optionsButtonPrompt;
    [SerializeField]
    private GameObject shoulderButtonsPrompt;
    [SerializeField]
    private GameObject triggerButtonsPrompt;

    public class GamepadButtonPromptContent {
      public GamepadButton Type;
      public string Text;
    }

    private void Start() {
    }

    // ---------------------------------------------------------------------------
    public void SetPrompts(List<GamepadButtonPromptContent> prompts) {
      DisableAllPrompts();

      foreach (GamepadButtonPromptContent prompt in prompts) {
        try {
          var buttonPrompt = GetMappedPrompt(prompt.Type);

          buttonPrompt.SetActive(true);
          if (!string.IsNullOrWhiteSpace(prompt.Text)) {
            buttonPrompt.GetComponent<GamepadButtonPrompt>().SetText(prompt.Text);
          }
        } catch { }
      }
    }

    // ---------------------------------------------------------------------------
    public void SetGamepadType(GamepadType type) {
      var children = GetComponentsInChildren<GamepadButtonPrompt>(true);
      foreach (var child in children) {
        child.SetButtonType(type);
      }
    }

    // ---------------------------------------------------------------------------
    public void SetPromptActive(GamepadButton promptType, bool active) {
      var buttonPrompt = GetMappedPrompt(promptType);
      buttonPrompt.SetActive(active);
    }

    // ---------------------------------------------------------------------------
    private GameObject GetMappedPrompt(GamepadButton button) {
      switch (button) {
        case GamepadButton.North: return northButtonPrompt;
        case GamepadButton.South: return southButtonPrompt;
        case GamepadButton.West: return westButtonPrompt;
        case GamepadButton.East: return eastButtonPrompt;
        case GamepadButton.Menu: return optionsButtonPrompt;
        case GamepadButton.Triggers: return triggerButtonsPrompt;
        case GamepadButton.Shoulders: return shoulderButtonsPrompt;
        default: throw new System.Exception("Unsupported prompt mapping");
      }
    }

    // ---------------------------------------------------------------------------
    private void DisableAllPrompts() {
      southButtonPrompt.SetActive(false);
      westButtonPrompt.SetActive(false);
      eastButtonPrompt.SetActive(false);
      northButtonPrompt.SetActive(false);
      optionsButtonPrompt.SetActive(false);
      shoulderButtonsPrompt.SetActive(false);
      triggerButtonsPrompt.SetActive(false);
    }
  }
}