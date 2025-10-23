using System;

namespace Overwolf.CFCore.UnityUI.EternalAppSettings {
  public static class EternalDirSettingsHelper {
    private const string kUserDirToken = "%USER_DIR%";
    private const string kUserSettingsDirToken = "%USER_SETTINGS_DIR%";

    private static readonly string userDirPath =
      Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private static readonly string userSettingsDirPath =
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    // ---------------------------------------------------------------------------
    public static string ExtractEnvironmentPath(string path) {
      if (String.IsNullOrWhiteSpace(path)) {
        return String.Empty;
      }

      switch (path) {
        case string s when s.StartsWith(kUserDirToken):
          return s.Replace(kUserDirToken, userDirPath);
        case string s when s.StartsWith(kUserSettingsDirToken):
          return s.Replace(kUserSettingsDirToken, userSettingsDirPath);
        default:
          return path;
      }
    }
  }
}