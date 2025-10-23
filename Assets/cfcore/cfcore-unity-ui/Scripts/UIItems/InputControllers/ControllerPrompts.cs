using System.Collections.Generic;
using Overwolf.CFCore.UnityUI.Gamepad;
using static Overwolf.CFCore.UnityUI.Gamepad.InputController;
using static Overwolf.CFCore.UnityUI.InputController.GamepadPromptsScript;
using PromptsDictionary = System.Collections.ObjectModel.ReadOnlyDictionary<Overwolf.CFCore.UnityUI.Gamepad.InputController.NavigationMode, System.Collections.Generic.List<Overwolf.CFCore.UnityUI.InputController.GamepadPromptsScript.GamepadButtonPromptContent>>;

namespace Overwolf.CFCore.UnityUI.InputController {
  public static class ControllerPrompts {
    // TODO(sabraham): Prompts are dynamic and change according to screen and
    // state, e.g. detailed view while installing
    public static readonly PromptsDictionary Map = new(
      new Dictionary<NavigationMode, List<GamepadButtonPromptContent>>() {
      {
        NavigationMode.Library,
        new List<GamepadButtonPromptContent>() {
          new GamepadButtonPromptContent() { Type = GamepadButton.South },
          new GamepadButtonPromptContent() { Type = GamepadButton.West },
          new GamepadButtonPromptContent() { Type = GamepadButton.Menu },
          new GamepadButtonPromptContent() { Type = GamepadButton.Shoulders }
        }
      },
      {
        NavigationMode.ModDetailed,
        new List<GamepadButtonPromptContent>() {
          new GamepadButtonPromptContent() { Type = GamepadButton.South },
          new GamepadButtonPromptContent() { Type = GamepadButton.West },
          new GamepadButtonPromptContent() { Type = GamepadButton.Menu },
          //new GamepadButtonPromptContent() { Type = GamepadPrompt.Shoulders },
          new GamepadButtonPromptContent() { Type = GamepadButton.East }
        }
      },
      {
        NavigationMode.Menu,
        new List<GamepadButtonPromptContent>() {
          new GamepadButtonPromptContent() { Type = GamepadButton.South },
          new GamepadButtonPromptContent() { Type = GamepadButton.Shoulders }
        }
      }
      });
  }
}