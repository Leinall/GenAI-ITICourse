using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class Toast : MonoBehaviour {
    [SerializeField] protected RawImage Icon;
    [SerializeField] protected GameObject WarningImageGameObject;
    [SerializeField] protected TextMeshProUGUI Txt_ModName;
    [SerializeField] protected TextMeshProUGUI Txt_Notification;
    [SerializeField] protected float WaitTime = 12f;
    [SerializeField] protected float DestructionTime = 1f;

    const string kDisappearTrigger = "disappear";

    void Start() {
      Toast[] toasts = FindObjectsOfType<Toast>();
      for (int i = 0; i < toasts.Length; i++) {
        if (toasts[i].Equals(this))
          continue;
        toasts[i].ForceDisable();
      }

      StartCoroutine(DisableToast());
    }

    public virtual void SetupToast(string notification, string modName = "",
                Texture modIconTex = null, bool isWarning=false) {
      if (Txt_ModName != null)
        Txt_ModName.text = modName;
      if (Icon != null)
        Icon.texture = modIconTex;
      if (WarningImageGameObject!=null && !isWarning) {
        WarningImageGameObject.SetActive(false);
      }

      Txt_Notification.text = notification;

      if (string.IsNullOrEmpty(modName)) {
        float width = LayoutUtility.GetPreferredWidth(Txt_Notification.GetComponent<RectTransform>());
        GetComponent<RectTransform>().sizeDelta = new Vector2(width + 32f, 40);
      } else {
        float width_notification = LayoutUtility.GetPreferredWidth(Txt_Notification.GetComponent<RectTransform>());
        float width_modName = LayoutUtility.GetPreferredWidth(Txt_ModName.GetComponent<RectTransform>());

        float width = width_notification > width_modName ? width_notification : width_modName;

        GetComponent<RectTransform>().sizeDelta = new Vector2(width + 96f, 73);
      }
    }

    public void ForceDisable() {
      StartCoroutine(ForceDisableToast());
    }

    IEnumerator DisableToast() {
      yield return new WaitForSeconds(WaitTime);

      if (gameObject != null)
        GetComponent<Animator>().SetTrigger(kDisappearTrigger);

      yield return new WaitForSeconds(DestructionTime);
      if (gameObject != null)
        Destroy(gameObject);
    }

    IEnumerator ForceDisableToast() {
      GetComponent<Animator>().SetTrigger(kDisappearTrigger);
      yield return new WaitForSeconds(DestructionTime);
      if (gameObject != null)
        Destroy(gameObject);
    }
  }
}
