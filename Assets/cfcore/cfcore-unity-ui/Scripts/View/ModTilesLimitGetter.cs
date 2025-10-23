using System.Collections.Generic;

namespace Overwolf.CFCore.UnityUI.View {
  public static class ModTilesLimitGetter {

    private static List<RatioToTiles> BrowseTilesLimits = new List<RatioToTiles>() {
    new RatioToTiles(){ ratio= 3.5f, tiles = 30 },
    new RatioToTiles(){ ratio= 3.31f, tiles = 28 },
    new RatioToTiles(){ ratio= 3.06f, tiles = 26 },
    new RatioToTiles(){ ratio= 2.86f, tiles = 24 },
    new RatioToTiles(){ ratio= 2.63f, tiles = 22 },
    new RatioToTiles(){ ratio= 2.41f, tiles = 20 },
    new RatioToTiles(){ ratio= 2.19f, tiles = 18 },
    new RatioToTiles(){ ratio= 1.98f, tiles = 16 },
    new RatioToTiles(){ ratio= 1.76f, tiles = 14 },
    new RatioToTiles(){ ratio= 1.55f, tiles = 12 },
    new RatioToTiles(){ ratio= 0f, tiles = 10 },
  };

    public static int GetModTileLimit(float aspectRatio, ModTileType type = 0) {
      switch (type) {
        case ModTileType.BrowseTile:
          return ReturnLimit(BrowseTilesLimits, aspectRatio);
      }

      return 0;
    }

    private static int ReturnLimit(List<RatioToTiles> list, float ratio) {
      foreach (var rtt in list) {
        if (ratio > rtt.ratio)
          return rtt.tiles;
      }

      return 0;
    }

    private struct RatioToTiles {
      public float ratio;
      public int tiles;
    }
  }

  public enum ModTileType {
    BrowseTile = 0,
  }

}
