using UnityEditor;
using UnityEngine;

namespace Overwolf.CFCore.UnityUI.EternalAppSettings {
  public class EternalSettingsMenuItem  {
    const string kDirectoryName = "Eternal";

    [MenuItem("Eternal/Edit Settings")]
    public static void EditSettings() {
      var settings = GetAsset();
      EditorGUIUtility.PingObject(settings);
      Selection.activeObject = settings;
    }

    static EternalSettings GetAsset() {
      var settingsAsset = Resources.Load<EternalSettings>(EternalSettings.FilePath);

      // if it doesnt exist we create one
      if (settingsAsset == null) {
        settingsAsset = ScriptableObject.CreateInstance<EternalSettings>();
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) {
          AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder($@"Assets/Resources/{kDirectoryName}")) {
          AssetDatabase.CreateFolder("Assets/Resources", kDirectoryName);
        }

        AssetDatabase.CreateAsset(
            settingsAsset, $@"Assets/Resources/{EternalSettings.FilePath}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      }
      return settingsAsset;
    }
  }
}