using UnityEngine;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class GlobalLoader : MonoBehaviour {
    private static GlobalLoader _instance;

    /// <summary>
    /// The progress of the stuff that we are waiting for.
    /// Between 0-1
    /// </summary>
    private float _progress;

    /// <summary>
    /// If instance is null , that means we dont have a loader
    /// </summary>
    public static GlobalLoader Instance {
      get {
        return _instance;
      }
    }

    private void Awake() {
      _instance = this;
    }

    public void SetActive(bool isActive) {
      if (isActive) {
        _progress = 0;
        if (gameObject != null)
          gameObject.SetActive(true);
      } else {
        if (gameObject != null)
          gameObject.SetActive(false);
      }
    }

    public void UpdateProgress(float progress) {
      _progress = progress;
      // Update visual (_progress);
    }
  }
}
