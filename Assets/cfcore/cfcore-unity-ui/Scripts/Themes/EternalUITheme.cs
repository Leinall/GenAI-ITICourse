using UnityEngine;

namespace Overwolf.CFCore.UnityUI.Themes {

  /// <summary>
  /// Used for changing the general looks of the UI.
  /// Contains three parts :
  ///  Color : changes the colors of the UI
  ///  Shape : changes the shape of buttuns and other ui elements
  ///  Scale : changes the scale of the UI
  /// </summary>
  public class EternalUITheme : MonoBehaviour {

    public static EternalUITheme Instance;

    private EternalAppSettings.EternalSettings settingsAsset;
    private EternalThemeColorScriptableObject colorTheme;
    private EternalThemeShapeScriptableObject shapeTheme;

    public EternalThemeShapeScriptableObject ShapeTheme
      { get => shapeTheme; private set => shapeTheme = value; }

    public EternalThemeColorScriptableObject ColorTheme
      { get => colorTheme; private set => colorTheme = value; }

    #region Unity and events

    // This region adds and remove listeners for changes of the scriptable objects
    // in order to catch the changes of the theme and see them in runtime.

    private void Awake() {
      Instance = this;
      settingsAsset = Resources.Load<EternalAppSettings.EternalSettings>(EternalAppSettings.EternalSettings.FilePath);
      if (settingsAsset != null) {
        colorTheme = settingsAsset.ColorTheme;
        ShapeTheme = settingsAsset.ShapeTheme;
      }
    }

    private void OnDestroy() {
      if (ShapeTheme!= null && ShapeTheme.OnValueChange!= null)
      ShapeTheme.OnValueChange.RemoveListener(UpdateShapes);
      if (colorTheme != null && colorTheme.OnValueChange != null)
        colorTheme.OnValueChange.RemoveListener(UpdateColors);
    }

    public void ApplyColorTheme() {
      colorTheme.OnValueChange.AddListener(UpdateColors);
      UpdateColors();
    }
    public void ApplyShapeTheme() {
      ShapeTheme.OnValueChange.AddListener(UpdateShapes);
      UpdateShapes();
    }

    /// <summary>
    /// Updates the images of all the game objects which contain a shape setter component.
    /// </summary>
    public void UpdateShapes() {
      var setters = GetComponentsInChildren<EternalShapeSetter>(true);
      foreach (EternalShapeSetter setter in setters) {
        setter.SetupShape(ShapeTheme);
      }
    }

    /// <summary>
    /// Updates the colors of all the game objects which contain a color setter component.
    /// </summary>
    private void UpdateColors() {
      var setters = GetComponentsInChildren<EternalColorSetter>(true);
      foreach (EternalColorSetter setter in setters) {
        setter.SetupColor(this.ColorTheme);
      }
    }

    #endregion Unity and events

    #region Scale
    /// <summary>
    /// Rescale all the game objects which contain a scaler component.
    /// </summary>
    /// <returns>Returns the number of mods that a page can show</returns>
    public uint ApplyScale() {
      if (settingsAsset == null) {
        return ModItemsThreshold(1);
      }
      var setters = GetComponentsInChildren<EternalUIScaler>(true);
      foreach (EternalUIScaler setter in setters) {
        setter.ResizeRectTransform(settingsAsset.UIScale);
      }
      return ModItemsThreshold(settingsAsset.UIScale);
    }

    private uint ModItemsThreshold(float scaleMultiplier) {
      if (scaleMultiplier >= 0.99f)
        return 14;
      return 12;
    }

    #endregion Scale
  }
}
