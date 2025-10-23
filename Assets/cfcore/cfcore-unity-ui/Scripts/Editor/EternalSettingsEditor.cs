using Overwolf.CFCore.UnityUI.Themes;
using UnityEditor;
using UnityEngine;

namespace Overwolf.CFCore.UnityUI.EternalAppSettings {
  [CustomEditor(typeof(EternalSettings))]
  public class EternalSettingsEditor : Editor {

    public override void OnInspectorGUI() {

      base.OnInspectorGUI();

      EternalSettings myTarget = (EternalSettings)target;
      EditorGUILayout.Space();

      GUIStyle labelStyle = new GUIStyle();
      labelStyle.alignment = TextAnchor.MiddleCenter;
      labelStyle.fontStyle = FontStyle.Bold;
      labelStyle.normal.textColor = Color.white;

      EditorGUILayout.Space();
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.LabelField("Theme controller", labelStyle);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      if (GUILayout.Button("Overwrite UI with theme settings")) {
        SetupAllThemesToPrefabs();
      }
      EditorGUILayout.Space();


      if (GUILayout.Button("Reset UI Colors")) {
        ResetColors();
      }

      if (GUILayout.Button("Reset UI Shapes")) {
        ResetShapes();
      }
    }

    private void SetupAllThemesToPrefabs() {
      EternalSettings myTarget = (EternalSettings)target;
      string[] PrefabGuids = AssetDatabase.FindAssets("t:prefab");
      foreach (string guid in PrefabGuids) {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Debug.Log("go name is "+ go.name);
        if (myTarget.ColorTheme != null) {
          EternalColorSetter[] colorSetters = go.GetComponentsInChildren<EternalColorSetter>(true);
          foreach (var setter in colorSetters) {
            setter.SetupColor(myTarget.ColorTheme);
          }
        }

        if (myTarget.ShapeTheme != null) {
          EternalShapeSetter[] shapeSetters = go.GetComponentsInChildren<EternalShapeSetter>(true);
          foreach (var setter in shapeSetters) {
            if (setter.Shape == EternalUIShapable.LoadingBar) {
              Debug.Log("shape is " + setter.Shape.ToString());
            }
            setter.SetupShape(myTarget.ShapeTheme);
            Debug.Log( "shape is " +setter.Shape.ToString());
          }
        }
      }

      myTarget.ColorTheme = null;
      myTarget.ShapeTheme = null;
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
    }

    private void ResetColors() {
      EternalThemeColorScriptableObject defaultTheme = GetDefaultColors();
      if (defaultTheme == null) {
        Debug.LogWarning("Couldn't find the default color scriptable object.");
        return;
      }

      EternalSettings myTarget = (EternalSettings)target;
      string[] PrefabGuids = AssetDatabase.FindAssets("t:prefab");
      foreach (string guid in PrefabGuids) {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        EternalColorSetter[] colorSetters = go.GetComponentsInChildren<EternalColorSetter>(true);
        foreach (var setter in colorSetters) {
          setter.SetupColor(defaultTheme);
        }
      }

      myTarget.ColorTheme = null;
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      EditorApplication.RepaintHierarchyWindow();
    }

    private void ResetShapes() {
      EternalThemeShapeScriptableObject defaultTheme= GetDefaultShapes();
      if (defaultTheme  == null) {
        Debug.LogWarning("Couldn't find the default shape scriptable object.");
        return;
      }

      EternalSettings myTarget = (EternalSettings)target;
      string[] PrefabGuids = AssetDatabase.FindAssets("t:prefab");
      foreach (string guid in PrefabGuids) {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        EternalShapeSetter[] colorSetters = go.GetComponentsInChildren<EternalShapeSetter>(true);
        foreach (var setter in colorSetters) {
          setter.SetupShape(defaultTheme);
        }
      }

      myTarget.ShapeTheme = null;
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      EditorApplication.RepaintHierarchyWindow();

    }

    private EternalThemeShapeScriptableObject GetDefaultShapes() {
      string[] ThemesGuids = AssetDatabase.FindAssets("t:EternalThemeShapeScriptableObject");
      Debug.Log(ThemesGuids.Length);
      foreach (string guid in ThemesGuids) {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        EternalThemeShapeScriptableObject theme = AssetDatabase.LoadAssetAtPath<EternalThemeShapeScriptableObject>(path);
        if (theme.Name == "Default") {
          Debug.Log(theme.Name);
          return theme;
        }
      }
        return null;
    }

    private EternalThemeColorScriptableObject GetDefaultColors() {
      string[] ThemesGuids = AssetDatabase.FindAssets("t:EternalThemeColorScriptableObject");
      Debug.Log(ThemesGuids.Length);
      foreach (string guid in ThemesGuids) {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        EternalThemeColorScriptableObject theme = AssetDatabase.LoadAssetAtPath<EternalThemeColorScriptableObject>(path);
        if (theme.Name == "Default") {
          Debug.Log(theme.Name);
          return theme;
        }
      }
      return null;
    }
  }
}