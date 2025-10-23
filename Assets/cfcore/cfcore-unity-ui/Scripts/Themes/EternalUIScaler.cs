using UnityEngine;
namespace Overwolf.CFCore.UnityUI.Themes {

  /// <summary>
  /// Scales the UI according to multiplier.
  /// Works only with stretched images for now.
  /// </summary>
  public class EternalUIScaler : MonoBehaviour {

    /// <summary>
    /// Should scale the width of the object
    /// </summary>
    [SerializeField]
    private bool changeWidth=false;

    /// <summary>
    /// Should scale the Height of the object
    /// </summary>
    [SerializeField]
    private bool changeHeight=false;

    /// <summary>
    /// Should move the object to the right to be in the same relative position on X axis
    /// </summary>
    [SerializeField]
    private bool moveUIOnX=false;

    /// <summary>
    /// The initial size of the UI for transforming from percent to pixels
    /// </summary>
    private  float UIWidth = 1920;
    private  float UIHeight = 1080;

    /// <summary>
    /// Resize the rect transform of the object
    /// </summary>
    /// <param name="scaleMultiplier">Multiplier of the size of the UI.
    ///              For example 0.8 means that the UI size will be 80% </param>
    public void ResizeRectTransform(float scaleMultiplier) {
      Vector2 startingOfsetMin;
      Vector2 startingOfsetMax;
      RectTransform thisRectTransform;
      Rect mainThemeRect = EternalUITheme.Instance.GetComponent<RectTransform>().rect;
      UIWidth = mainThemeRect.width;
      UIHeight = mainThemeRect.height;

      float adjustedMultiplier = (1f - scaleMultiplier) / 2;
      thisRectTransform = GetComponent<RectTransform>();
      startingOfsetMin = thisRectTransform.offsetMin;
      startingOfsetMax = thisRectTransform.offsetMax;
      float deltaX = changeWidth ? UIWidth * adjustedMultiplier : 0;
      float deltaY = changeHeight ? UIHeight * adjustedMultiplier : 0;

      thisRectTransform.offsetMin = new Vector2(startingOfsetMin.x + deltaX,
        startingOfsetMin.y + deltaY / 2); // TODO: Remove this when we will get the new UI visuals
      thisRectTransform.offsetMax = new Vector2(startingOfsetMax.x - deltaX,
        startingOfsetMax.y - deltaY);

      thisRectTransform.localPosition = new Vector3( // TODO:this adjusments for the previus todo
        thisRectTransform.localPosition.x,
          thisRectTransform.localPosition.y + deltaY / 4,
          thisRectTransform.localPosition.z);

      if (moveUIOnX) {
        thisRectTransform.localPosition = new Vector3(thisRectTransform.localPosition.x
            + UIWidth * adjustedMultiplier,
            thisRectTransform.localPosition.y,
            thisRectTransform.localPosition.z);
      }
    }

  }
}
