using System;
using UnityEngine;
using UnityEngine.Events;

namespace Overwolf.CFCore.UnityUI.Themes {

  [CreateAssetMenu(fileName = "ThemeColorObject", menuName = "Eternal/Theme Color")]
  public class EternalThemeColorScriptableObject : ScriptableObject {
    public string Name;

    [Tooltip("The color of the main page background")]
    public Color Background = new Color(0.02745098f, 0.02745098f, 0.02745098f, 1);

    [Tooltip("The color of the background of the bar on the side of the main page.")]
    public Color Sidebar = new Color(0.1490196f, 0.1490196f, 0.1490196f, 1f);

    [Tooltip("The color of the mods background that is displayed " +
      "in the browse mods and installed mods tabs.")]
    public Color ModItem =  new Color(0.1019608f, 0.1019608f, 0.1019608f, 1);

    [Tooltip("The color of the default font.")]
    public Color Font = new Color(0.8980392f, 0.8980392f, 0.8980392f,1);

    [Tooltip("The color of the greyed out font.")]
    public Color FontGreyed = new Color(0.6f, 0.6f, 0.6f, 1);

    [Tooltip("The color of the mods outline that is displayed " +
          "in the browse mods and installed mods tabs.")]
    public Color ModItemOutline = new Color(1, 1, 1, 1);

    [Tooltip("The color of the elelments in drop down.")]
    public Color DropdownColor = new Color(0.2f, 0.2f, 0.2f, 1);

    [Tooltip("The color of left/right buttons that move through mod pages.")]
    public Color PaginationButtonColor = new Color(0.2f, 0.2f, 0.2f, 1);

    [Tooltip("The color of the background in mod detailed view.")]
    public Color ModDetailedInfoBackground = new Color(0.4339623f, 0.4339623f, 0.4339623f, 0.4666667f);

    [Tooltip("The color of the search field background.")]
    public Color SearchBackground = new Color(0.1019608f, 0.1019608f, 0.1019608f, 1);

    [Tooltip("The color of the menu elelments that are shown when a mod right clicked.")]
    public Color MenuBackground = new Color(0.2f, 0.2f, 0.2f, 1);

    [Tooltip("The color surrounding area of a popup.")]
    public Color PopupBackground = new Color(0.05098039f, 0.05098039f, 0.05098039f, 0.9019608f);

    [Tooltip("The color of the popup.")]
    public Color PopupColor = new Color(0.1019608f, 0.1019608f, 0.1019608f, 0.7098039f);

    [Tooltip("The color of the underline line.")]
    public Color Underline = new Color(0.945098f, 0.3921569f, 0.2117647f, 1);

    [Tooltip("The color of the default toast.")]
    public Color Toast = new Color(0.145098f, 0.3333333f, 0.6156863f, 1);

    [Tooltip("The color of the install toast.")]
    public Color InstallToast = new Color(0.1490196f, 0.1490196f, 0.1490196f, 1);

    [Tooltip("The color of the background of the loading overlay.")]
    public Color LoaderBackground = new Color(0.02745098f, 0.02745098f, 0.02745098f, 0.39f);

    [Tooltip("The color of the background of the class top bar.")]
    public Color ClassBackground   = new Color(1, 1, 1, 0.003921569f);

    [Tooltip("The color of the filling of the install bar.")]
    public Color InstallingBarFill = new Color(0.945098f, 0.3921569f, 0.2117647f, 1);

    [Tooltip("The color of the thumbs up icon.")]
    public Color ThumbsUp = new Color(0.945098f, 0.3921569f, 0.2117647f, 1);

    [Tooltip("The color of the stroke around the search field in sidebar.")]
    public Color SearchStroke = Color.white;

    [Tooltip("The colors of the mod normal buttons.")]
    public BlockTheme ModItemButton = new BlockTheme() {
      normal = new Color(0.945098f, 0.3921569f, 0.2117647f, 1),
      hover = new Color(1, 0.4705882f, 0.3019608f, 1)
    };

    [Tooltip("The colors of the mod update buttons.")]
    public BlockTheme ModItemUpdateButton = new BlockTheme() {
      normal = new Color(0.145098f, 0.3333333f, 0.6156863f, 1),
      hover = new Color(0.2078431f, 0.4627451f, 0.8431373f, 1)
    };

    [Tooltip("The colors of the fonts of the dropdown menues.")]
    public BlockTheme DropdownFont = new BlockTheme() {
      normal = new Color(0.8f, 0.8f, 0.8f, 1),
      hover = new Color(0.89f, 0.89f, 0.89f, 1)
    };

    [Tooltip("The colors of the buttons and their fonts in the side menu.")]
    public SidebarBlockTheme SidebarButton = new SidebarBlockTheme() {
      DefaultBG = new Color (0.1490196f, 0.1490196f, 0.1490196f,1),
      HoverBG = new Color(0.2f, 0.2f, 0.2f, 0.5f),
      ActiveBG = new Color(0.2f, 0.2f, 0.2f, 1),
      ActiveChoiceFont = new Color(0.8980392f, 0.8980392f, 0.8980392f, 1),
      InactiveChoiceFont = new Color(0.8f, 0.8f, 0.8f, 1),
    };

    public UnityEvent OnValueChange = new UnityEvent();

    [Serializable]
    public struct BlockTheme {
      public Color normal;
      public Color hover;
    }

    [Serializable]
    public struct SidebarBlockTheme {
      public Color DefaultBG;
      public Color HoverBG;
      public Color ActiveBG;
      public Color InactiveChoiceFont;
      public Color ActiveChoiceFont;
    }

    public void OnValidate() {
      OnValueChange?.Invoke();
    }
  }
}
