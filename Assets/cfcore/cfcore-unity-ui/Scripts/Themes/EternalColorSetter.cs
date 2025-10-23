using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Overwolf.CFCore.UnityUI.UIItems;

namespace Overwolf.CFCore.UnityUI.Themes {
  /// <summary>
  /// Changes the color of a colorable element on the gameObject.
  /// To use it, simply put this component on any GameObject that has a color or color block ,
  /// and choose 
  /// </summary>
  public class EternalColorSetter : MonoBehaviour {
    public List<EternalUIColorable> EternalUIColorables;
    private EternalThemeColorScriptableObject colorTheme;

    public void SetupColor(EternalThemeColorScriptableObject theme, bool isDestroy = true) {
      colorTheme = theme;
      foreach (EternalUIColorable colorableElement in EternalUIColorables)
        switch (colorableElement) {
          case EternalUIColorable.ModItemButton:
          case EternalUIColorable.ModItemUpdateButton:
          case EternalUIColorable.DropdownFont:
            SetSelectable(GetColorBlock(colorableElement));
            break;
          case EternalUIColorable.SidebarButton:
            SetSideButtons(GetColorBlock(colorableElement));
            break;
          default:
            SetColor(GetColor(colorableElement));
            break;
        }
      if (!isDestroy)
        Destroy(this);
    }

    private void SetColor(Color color) {
      Graphic graphic = GetComponent<Graphic>();
      if (graphic != null) {
        graphic.color = color;
      }
    }

    private void SetSelectable(ColorBlock block) {
      Selectable selectable = GetComponent<Selectable>();
      if (selectable != null) {
        selectable.colors = block;
      }
    }

    private void SetSideButtons(ColorBlock block) {
      var sideButton = GetComponent<Sidebar_HoverActiveEffect>();
      if (sideButton != null) {
        sideButton.Color_Default_BG = block.normalColor;
        sideButton.Color_Hover_BG = block.highlightedColor;
        sideButton.Color_Active_BG = block.selectedColor;
        sideButton.Color_Hover_Content = block.disabledColor;
        sideButton.Color_Default_Content = block.disabledColor;
        sideButton.Color_Active_Content = block.pressedColor;
      }
    }

    public ColorBlock GetColorBlock(EternalUIColorable colorable) {
      ColorBlock colorBlock = new ColorBlock();
      switch (colorable) {
        case EternalUIColorable.ModItemButton:
          colorBlock.colorMultiplier = 1;
          colorBlock.normalColor = colorTheme.ModItemButton.normal;
          colorBlock.highlightedColor = colorTheme.ModItemButton.hover;
          colorBlock.selectedColor = colorTheme.ModItemButton.hover;
          colorBlock.disabledColor = colorTheme.ModItemButton.normal;
          colorBlock.pressedColor = colorTheme.ModItemButton.normal;
          break;
        case EternalUIColorable.ModItemUpdateButton:
          colorBlock.colorMultiplier = 1;
          colorBlock.normalColor = colorTheme.ModItemUpdateButton.normal;
          colorBlock.highlightedColor = colorTheme.ModItemUpdateButton.hover;
          colorBlock.selectedColor = colorTheme.ModItemUpdateButton.hover;
          colorBlock.disabledColor = colorTheme.ModItemUpdateButton.normal;
          colorBlock.pressedColor = colorTheme.ModItemUpdateButton.normal;
          break;
        case EternalUIColorable.SidebarButton:
          colorBlock.colorMultiplier = 1;
          colorBlock.normalColor = colorTheme.SidebarButton.DefaultBG;
          colorBlock.highlightedColor = colorTheme.SidebarButton.HoverBG;
          colorBlock.selectedColor = colorTheme.SidebarButton.ActiveBG;
          colorBlock.disabledColor = colorTheme.SidebarButton.InactiveChoiceFont;
          colorBlock.pressedColor = colorTheme.SidebarButton.ActiveChoiceFont;
          break;
        case EternalUIColorable.DropdownFont:
          colorBlock.colorMultiplier = 1;
          colorBlock.normalColor = colorTheme.DropdownFont.normal;
          colorBlock.highlightedColor = colorTheme.DropdownFont.hover;
          colorBlock.selectedColor = colorTheme.DropdownFont.hover;
          colorBlock.disabledColor = colorTheme.DropdownFont.normal;
          colorBlock.pressedColor = colorTheme.DropdownFont.normal;
          break;
        default:
          Debug.LogError("Color setter couldn't find the correct colorable");
          break;

      }
      if (colorable != EternalUIColorable.SidebarButton) {
        colorBlock.disabledColor = new Color(colorBlock.disabledColor.r,
          colorBlock.disabledColor.g, colorBlock.disabledColor.b, 0.3f);
      }
      return colorBlock;
    }

    public Color GetColor(EternalUIColorable colorable) {
      Color color = new Color();

      switch (colorable) {
        case EternalUIColorable.Background:
          color = colorTheme.Background;
          break;
        case EternalUIColorable.Font:
          color = colorTheme.Font;
          break;
        case EternalUIColorable.FontGreyed:
          color = colorTheme.FontGreyed;
          break;
        case EternalUIColorable.ModItem:
          color = colorTheme.ModItem;
          break;
        case EternalUIColorable.ModItemOutline:
          color = colorTheme.ModItemOutline;
          break;
        case EternalUIColorable.Sidebar:
          color = colorTheme.Sidebar;
          break;
        case EternalUIColorable.DropdownBackground:
          color = colorTheme.DropdownColor;
          break;
        case EternalUIColorable.PaginationButton:
          color = colorTheme.PaginationButtonColor;
          break;
        case EternalUIColorable.SearchBackground:
          color = colorTheme.SearchBackground;
          break;
        case EternalUIColorable.ModDetailedInfoBackground:
          color = colorTheme.ModDetailedInfoBackground;
          break;
        case EternalUIColorable.MenuBackground:
          color = colorTheme.MenuBackground;
          break;
        case EternalUIColorable.PopupBackground:
          color = colorTheme.PopupBackground;
          break;
        case EternalUIColorable.Underline:
          color = colorTheme.Underline;
          break;
        case EternalUIColorable.Toast:
          color = colorTheme.Toast;
          break;
        case EternalUIColorable.InstallToast:
          color = colorTheme.InstallToast;
          break;
        case EternalUIColorable.ClassBackground:
          color = colorTheme.ClassBackground;
          break;
        case EternalUIColorable.LoaderBackground:
          color = colorTheme.LoaderBackground;
          break;
        case EternalUIColorable.InstallingBarFill:
          color = colorTheme.InstallingBarFill;
          break;
        case EternalUIColorable.ThumbsUp:
          color = colorTheme.ThumbsUp;
          break;
        case EternalUIColorable.PopupColor:
          color = colorTheme.PopupColor;
          break;
        case EternalUIColorable.SearchStroke:
          color = colorTheme.SearchStroke;
          break;

        default:
          Debug.LogError("Color setter couldn't find the correct colorable");
          break;
      }
      return color;
    }

  }
}
