using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.Popups {
  public class ToastErrorNotification:MonoBehaviour  {

#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] RawImage Icon;
    [SerializeField] TMP_Text Txt_Notification;
    [SerializeField] float WaitTime = 20f;
    [SerializeField] float DestructionTime = 1f;
#pragma warning restore 0649

    const string kDisappearTrigger = "disappear";


    void Start() {
      StartCoroutine(DisableToast());
    }

    public virtual void SetupErrorToast(string notification,
                Texture IconTexture = null) {
      if (!string.IsNullOrEmpty(notification)) {
        Txt_Notification.text = notification;
      }
      if (IconTexture != null)
        Icon.texture = IconTexture;
    }

    IEnumerator DisableToast() {
      yield return new WaitForSeconds(WaitTime);

      if (gameObject != null)
        GetComponent<Animator>().SetTrigger(kDisappearTrigger);

      yield return new WaitForSeconds(DestructionTime);
      if (gameObject != null)
        Destroy(gameObject);
    }

    public void ForceDisable(bool isImidiate) {
      if (isImidiate) {
        gameObject.SetActive(false);
      } else {
        StartCoroutine(ForceDisableToast());
      }
    }

    IEnumerator ForceDisableToast() {
      GetComponent<Animator>().SetTrigger(kDisappearTrigger);
      yield return new WaitForSeconds(DestructionTime);
      if (gameObject != null)
        Destroy(gameObject);
    }
  }
}