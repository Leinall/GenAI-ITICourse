using Overwolf.CFCore.UnityUI.UIItems;
using UnityEngine;

namespace Overwolf.CFCore.CFCContext {
  public static class CFCContextConstants {
    public static readonly Context.Key<GameObject> ActiveMenu = new(nameof(ActiveMenu));

    public static readonly Context.Key<bool> IsModalViewOpen = new(nameof(IsModalViewOpen));
    public static readonly Context.Key<ModTileScript> LastSelectedModTile = new(nameof(LastSelectedModTile));
    public static readonly Context.Key<int> LastPaginationPage = new(nameof(LastPaginationPage));
    public static readonly Context.Key<bool> IsGamepadControl = new(nameof(IsGamepadControl));
    public static readonly Context.Key<bool> IsUINavigationMode = new(nameof(IsUINavigationMode));
    public static readonly Context.Key<int> LastSelectedClass = new(nameof(LastSelectedClass));

    public static readonly Context.Key<GameObject> CurrentFirstModInPage = new(nameof(CurrentFirstModInPage));
  }
}
