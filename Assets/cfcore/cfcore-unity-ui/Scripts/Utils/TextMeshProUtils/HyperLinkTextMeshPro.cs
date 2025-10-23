using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Overwolf.CFCore.UnityUI.Utils {
	public class HyperLinkTextMeshPro : MonoBehaviour, IPointerClickHandler {

		private TMP_Text text = default;

		private void Start() {
			text = GetComponent<TMP_Text>();
		}

		// Callback for handling clicks.
		public void OnPointerClick(PointerEventData eventData) {
			var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
			if (linkIndex >= 0) {
				var linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();

				Application.OpenURL(linkId);
			}
		}

	}
}