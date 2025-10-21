using ArabicSupport;
using UnityEngine;
using UnityEngine.UI;

public class ArabicFixe : MonoBehaviour
{
    public Text textUI;

    void Start()
    {
        string fixedText = ArabicFixer.Fix(textUI.text, true, true);
        textUI.text = fixedText;
    }
}
