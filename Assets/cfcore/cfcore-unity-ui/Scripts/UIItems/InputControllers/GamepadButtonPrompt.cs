using Overwolf.CFCore.UnityUI.Gamepad;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.InputController {
  public class GamepadButtonPrompt : MonoBehaviour {
    [SerializeField]
    private Sprite XboxSprite;
    [SerializeField]
    private Sprite PlayStationSprite;
    [SerializeField]
    private GameObject ButtonSprite;
    [SerializeField]
    private GameObject ButtonText;
    [SerializeField]
    private GamepadButton Button;

    private const string kPromptBasePath =
      "Assets/cfcore-unity-ui/UI/icons/gamepad/";

    // -------------------------------------------------------------------------
    public void SetText(string text) {
      ButtonText.GetComponent<TextMeshProUGUI>().text = text;
    }

    // -------------------------------------------------------------------------
    public void SetButtonType(GamepadType type) {
      var image = ButtonSprite.GetComponent<Image>();

      switch (type) {
        case GamepadType.PlayStation: image.sprite = PlayStationSprite; break;
        case GamepadType.Xbox: image.sprite = XboxSprite; break;
        default: return;
      }

      //var sp = AssetDatabase.LoadAssetAtPath<Sprite>(GetSpritePath(type));
      //ButtonSprite.GetComponent<Image>().sprite = sp;
    }

    // -------------------------------------------------------------------------
    private string GetSpritePath(GamepadType type) {
      if (Button == GamepadButton.Menu) {
        return $"{kPromptBasePath}{Button.ToString().ToLower()}.png";
      }

      return kPromptBasePath +
        $"{type.ToString().ToLower()}/" +
        $"{Button.ToString().ToLower()}.png";
    }
  }
}