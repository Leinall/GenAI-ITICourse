using Overwolf.CFCore.Base.Common;
using Overwolf.CFCore.UnityUI.Popups;
using Overwolf.CFCore.UnityUI.Themes;

using UnityEngine;


namespace Overwolf.CFCore.UnityUI.Managers {
  public class ToastManager : MonoBehaviour {

#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] GameObject PopupToastPrefab;
    [SerializeField] GameObject PopupInstalledToastPrefab;
    [SerializeField] GameObject PopupErrorToastPrefab;
#pragma warning restore 0649

    EternalUITheme theme ;

    // Start is called before the first frame update

    private static ToastManager _instance;

    public static ToastManager Instance {
      get {
        return _instance;
      }
    }
    private void Awake() {
      _instance = this;
    }

    private void Start() {
      theme = EternalUITheme.Instance;
    }

    public void ActivateNormalToast(string notification) {
      GameObject toast =
        Instantiate(PopupToastPrefab, transform) as GameObject;
      toast.GetComponent<Toast>().SetupToast(notification);
      ApplyTheme(toast);
    }

    public void InstalledToast(string installedModName, Texture modIconTexture,
      bool isUpdate = false) {
      GameObject toast =
        Instantiate(PopupInstalledToastPrefab, transform) as GameObject;
      string notification = isUpdate
        ? "Mod has been updated"
        : "Installed and added to \"Installed mods\"!";
      toast.GetComponent<Toast>().SetupToast(notification, installedModName,
                                             modIconTexture);
      ApplyTheme(toast);
    }

    public void CreateModToast(string modName, string notification,
                               Texture modIcon, bool isWarning = false) {
      GameObject toast =
       Instantiate(PopupInstalledToastPrefab, transform) as GameObject;
      toast.GetComponent<Toast>().SetupToast(notification, modName, modIcon,
                                             isWarning);
      ApplyTheme(toast);
    }

    public void CreateErrorToast(string notification, Texture iconTexture = null) {
      ForceDisableAllErrorToasts(isImidiate: false);
      GameObject toast = Instantiate(PopupErrorToastPrefab, transform);
      toast.GetComponent<ToastErrorNotification>().SetupErrorToast(notification, iconTexture);
    }

    public void ForceDisableAllErrorToasts(bool isImidiate) {
      var toasts = FindObjectsOfType<ToastErrorNotification>();
      for (int i = 0; i < toasts.Length; i++) {
        toasts[i].ForceDisable(isImidiate);
      }
    }

    private void ApplyTheme(GameObject toastGO) {
        if (theme != null) {

          if (theme.ColorTheme != null) {
            var setters = toastGO.GetComponentsInChildren<EternalColorSetter>(true);
            foreach (EternalColorSetter setter in setters) {
              setter.SetupColor(theme.ColorTheme);
            }
          }

          if (theme.ShapeTheme != null) {
            var setters = toastGO.GetComponentsInChildren<EternalShapeSetter>(true);
            foreach (EternalShapeSetter setter in setters) {
              setter.SetupShape(theme.ShapeTheme);
            }
          }
        }
      }
  }
}