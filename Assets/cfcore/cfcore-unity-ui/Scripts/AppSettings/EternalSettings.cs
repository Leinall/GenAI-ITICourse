using Overwolf.CFCore.Base.Models.Enums;
using Overwolf.CFCore.UnityUI.Themes;
using UnityEngine;

namespace Overwolf.CFCore.UnityUI.EternalAppSettings {
  /// <summary>
  /// Scriptable objects that contains the settings for the game.
  /// GameId and API key are required for the API to run properly ,
  /// The other fields are optional and are used to change the theme and scaling of the UI.
  /// </summary>
  public class EternalSettings : ScriptableObject {

    /// <summary>
    /// The path under Assets/Resources that the settings is stored.
    /// EternalSettings will be created here in default if it is missing.
    /// </summary>
    public static string FilePath = "Eternal/Settings";

    public bool InitializeAPIAtStartup = false;

    public uint GameId;
    public string APIKey;

    public bool HideSignIn = false;
    public bool HideMyCreation = false;

    /// <summary>
    /// %USER_DIR%/[game name]/mods (e.g. MyDocuments/game/mods)
    /// See: |kSpecialFolderUserDirectory|, |kSpecialFolderUserSettingsDirectory|
    /// </summary>
    public string ModsDirectory;
    /// <summary>
    /// %USER_SETTINGS_DIR%/[game name]/cfcore/user_data (e.g. appdata/local)
    /// See: |kSpecialFolderUserDirectory|, |kSpecialFolderUserSettingsDirectory|
    /// </summary>
    public string UserDataDirectory;

    public LanguageEnum EulaLanguage = LanguageEnum.en;
    public ModsDirectoryMode DirectoryMode = ModsDirectoryMode.CFCoreStructure;
    public bool UseModDescription = false;

    #region Theme Scriptable objects

    public EternalThemeColorScriptableObject ColorTheme;
    public EternalThemeShapeScriptableObject ShapeTheme;

    #endregion Theme Scriptable objects

    #region Scale data

    [SerializeField]
    private UIScalesEnum Scale = UIScalesEnum.Fullscreen;

    /// <summary>
    /// Enum that controlls the ammount of perecents that the UI takes of the screen
    /// </summary>
    public enum UIScalesEnum {
      Fullscreen = 100,
      Medium = 89,
    }

    [HideInInspector]
    public float UIScale {
      get { return getScale(); }
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>Changes from scale enum to float</returns>
    private float getScale() {
      return ((float)Scale)/ 100f;
    }

    #endregion Scale data

  }
}