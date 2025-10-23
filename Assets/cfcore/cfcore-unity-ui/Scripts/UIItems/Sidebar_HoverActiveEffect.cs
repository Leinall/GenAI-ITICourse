using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Overwolf.CFCore.UnityUI.UIItems {
  public class Sidebar_HoverActiveEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
#pragma warning disable 0649 // allocated in Prefab
    [SerializeField] Image Img_Background;

    [SerializeField] Image Img_Icon;
    [SerializeField] TextMeshProUGUI Txt_Content;
#pragma warning restore 0649

    public Color Color_Default_BG;
    public Color Color_Default_Content;

    public Color Color_Hover_BG;
    public Color Color_Hover_Content;

    public Color Color_Active_BG;
    public Color Color_Active_Content;

    bool isActive = false;

    //Hover
    public void OnPointerEnter(PointerEventData eventData) {
      if (!isActive) {
        if (Img_Background != null)
          Img_Background.color = Color_Hover_BG;
        if (Img_Icon != null)
          Img_Icon.color = Color_Hover_Content;
        if (Txt_Content != null)
          Txt_Content.color = Color_Hover_Content;
      }
    }

    public void OnPointerExit(PointerEventData eventData) {
      if (!isActive) {
        if (Img_Background != null)
          Img_Background.color = Color_Default_BG;
        if (Img_Icon != null)
          Img_Icon.color = Color_Default_Content;
        if (Txt_Content != null)
          Txt_Content.color = Color_Default_Content;
      }
    }

    //Active&Default
    public void SetActive(bool active) {
      if (active) {
        isActive = true;
        if (Img_Background != null)
          Img_Background.color = Color_Active_BG;
        if (Img_Icon != null)
          Img_Icon.color = Color_Active_Content;
        if (Txt_Content != null)
          Txt_Content.color = Color_Active_Content;
      } else {
        isActive = false;
        if (Img_Background != null)
          Img_Background.color = Color_Default_BG;
        if (Img_Icon != null)
          Img_Icon.color = Color_Default_Content;
        if (Txt_Content != null)
          Txt_Content.color = Color_Default_Content;
      }
    }

  }
}
