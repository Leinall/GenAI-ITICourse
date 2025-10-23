using UnityEditor;
using TMPro;

namespace Overwolf.CFCore.UnityUI.EternalAppSettings {
  [InitializeOnLoad]
  public class EternalUIEditor {

    private static bool autoImport = true;
    static EternalUIEditor() {
      if (!autoImport)
        return;

      string[] settings = AssetDatabase.FindAssets("t:TMP_Settings");
      if (settings.Length == 0)
        TMP_PackageUtilities.ImportProjectResourcesMenu();
    }
  }
}