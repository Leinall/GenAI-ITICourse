using UnityEngine;
using UnityEngine.Events;

namespace Overwolf.CFCore.UnityUI.Themes {
  [CreateAssetMenu(fileName = "ThemeShapeObject", menuName = "Eternal/Theme Shape")]
  public class EternalThemeShapeScriptableObject : ScriptableObject {

    public string Name;

    [Tooltip("The shape of the buttons")]
    public Sprite Button;

    [Tooltip("The shape of the dropdown")]
    public Sprite Dropdown;

    [Tooltip("The shape of left/right buttons that move through pages")]
    public Sprite PaginationButton;

    [Tooltip("The shape of the background of the main page")]
    public Sprite MainPage;

    [Tooltip("The shape of the search field")]
    public Sprite SearchField;

    [Tooltip("The shape of the sidebar")]
    public Sprite Sidebar;

    [Tooltip("The shape of the buttons in the sidebar")]
    public Sprite SidebarButton;

    [Tooltip("The shape of the mod tile that is displayed " +
      "in the search and installed sections")]
    public Sprite ModTile;

    [Tooltip("The filling of the loading bar that is animated when the game is loaded")]
    public Sprite LoadingBar;

    [Tooltip("The shape of the area that displays gamepad control prompts")]
    public Sprite GamepadPromptsBar;

    [Tooltip("The sprite that is shown when the installed mods section doesn't have any mods installed")]
    public Sprite InstalledModsEmptyView;

    [Tooltip("The shape of the highlighted stroke that around the mod tiles")]
    public Sprite ModTileStroke;

    [Tooltip("The shape indicates that the user liked the mod")]
    public Sprite ThumbsUpFull;

    [Tooltip("The shape indicates that the user didn't liked the mod")]
    public Sprite ThumbsUpEmpty;

    [Tooltip("The shape of the stroke around search field")]
    public Sprite SearchStroke;

    public UnityEvent OnValueChange = new UnityEvent();

    public void OnValidate() {
      OnValueChange?.Invoke();
    }
  }
}
