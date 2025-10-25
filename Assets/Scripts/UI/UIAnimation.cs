using DG.Tweening;
using RTLTMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class UIAnimation : MonoBehaviour
{
    // public RTLTextMeshPro testArabicText;
    UserText_Tag _userText;
    IntroPanel_Tag _introPanel;
    YallaBeena_Tag _yallaBeena;
    Button _yallaBeenaButton;

    public string fullText = " بك";
    private RTLTextMeshPro _rtlText;
    private void OnEnable()
    {
        DefineObjects();
    }

    private void DefineObjects()
    {
        _userText = FindFirstObjectByType<UserText_Tag>();
        _rtlText = _userText.GetComponent<RTLTextMeshPro>();

        _yallaBeena = FindFirstObjectByType<YallaBeena_Tag>();
        _yallaBeenaButton = _yallaBeena.GetComponent<Button>();

        _introPanel = FindFirstObjectByType<IntroPanel_Tag>();
    }
    void Start()
    {
        StartCoroutine(ShowText());

        _yallaBeenaButton.onClick.AddListener(ChooseCharacterAnimation);
    }
    void Update()
    {

    }

    IEnumerator ShowText()
    {
        string fullText = "المحقق كونان بتاعناااا فيه جريمه كده عايزينك تحقق فيها تشوف مين ال كل الجبنه و سمم الكلاب جاهز تبدء  يلا بيناااااااااااااا";
        _rtlText.text = "";

        for (int i = 1; i <= fullText.Length; i++)
        {
            _rtlText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(0.05F);
        }
        _yallaBeenaButton.interactable = true;
    }




    [UnityEngine.ContextMenu("Play animation")]
    public void ChooseCharacterAnimation()
    {
        _introPanel.gameObject.SetActive(false);

        FindFirstObjectByType<Heessa_Tag>().transform.DOMoveX(240f, 1)
            .OnComplete(() =>
            {
                FindFirstObjectByType<Adnan_Tag>().transform.DOMoveX(720f, 1)
                    .OnComplete(() =>
                    {
                        FindFirstObjectByType<Hosneya_Tag>().transform.DOMoveX(1200f, 1)
                            .OnComplete(() =>
                            {
                                FindFirstObjectByType<Killer_Tag>().transform.DOMoveX(1680f, 1);
                            });
                    });
            });
    }
}
