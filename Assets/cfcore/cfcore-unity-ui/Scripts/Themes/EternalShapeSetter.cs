using UnityEngine;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.Themes {
  public class EternalShapeSetter : MonoBehaviour {
    public EternalUIShapable Shape;
    private Image image;

    public void SetupShape(EternalThemeShapeScriptableObject ShapeSO) {

      if (ShapeSO == null) return;

      if (image == null) {
        image = GetComponent<Image>();
        if (image == null) return;
      }

      switch (Shape) {
        case EternalUIShapable.Button:
          image.sprite = ShapeSO.Button;
          break;
        case EternalUIShapable.Dropdown:
          image.sprite = ShapeSO.Dropdown;
          break;
        case EternalUIShapable.PaginationButton:
          image.sprite = ShapeSO.PaginationButton;
          break;
        case EternalUIShapable.MainPage:
          image.sprite = ShapeSO.MainPage;
          break;
        case EternalUIShapable.SearchField:
          image.sprite = ShapeSO.SearchField;
          break;
        case EternalUIShapable.Sidebar:
          image.sprite = ShapeSO.Sidebar;
          break;
        case EternalUIShapable.SidebarButton:
          image.sprite = ShapeSO.SidebarButton;
          break;
        case EternalUIShapable.ModItem:
          image.sprite = ShapeSO.ModTile;
          break;
        case EternalUIShapable.LoadingBar:
          image.sprite = ShapeSO.LoadingBar;
          break;
        case EternalUIShapable.InstalledModsEmptyView:
          image.sprite = ShapeSO.InstalledModsEmptyView;
          break;
        case EternalUIShapable.TileStroke:
          image.sprite = ShapeSO.ModTileStroke;
          break;
        case EternalUIShapable.ThumbsUpFull:
          image.sprite = ShapeSO.ThumbsUpFull;
          break;
        case EternalUIShapable.ThumbsUpEmpty:
          image.sprite = ShapeSO.ThumbsUpEmpty;
          break;
        case EternalUIShapable.SearchFieldStroke:
          image.sprite = ShapeSO.SearchStroke;
          break;
      }
    }
  }
}
