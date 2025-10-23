using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SmartCanvas : MonoBehaviour
{
    private float minVoume = -80f;
    private float maxVoume = -40f;
    private float minVoume_1 = 0f;
    private float maxVoume_1 = 1f;
    [SerializeField] [Range(0,5)] float lerpDuration;
    private float lerpStartTime;

    public AudioSource VoiceOver;
    [HideInInspector]
    public AudioSource audioSource;
    public bool _UseMic = true;
    //public AudioSource _voiceOver;
    public AudioMixerGroup _MicrophoneMixer, _Master, _Voice;
    //public AudioMixer audioMixer;
    //public string _MicrophoneMixerGroup = "MicVloume";
    //public string _VoiceOverGroup = "MicVloume";
    //Text To Choose 
    public TMP_Text _text_1;
    public TMP_Text _text_2;
    public Button b1;
    public Button b2;
    //Multiple Text
    public TMP_Text TextDisplay;
    //public GameObject TextPrefab;
    //public Transform contentTransform;
    public string _text = "";
    public AudioClip firstLine;
    public string _text_One = "";
    public string _text_Two = "";
    //Microphone
    public RectTransform lineRectTransform;
    public float lineScaleFactor = 2f;
    public float maxLineScale = 100f;
    float lineScale;

    private bool isMicrophoneInitialized = false;
    //Typing Speed
    float typingSpeed = 0.04f;
    //Highlighted Text
    public Color _highlightColor = Color.red;
    public float _higlightSpeed = 0.04f;
    public float _higlightTimer = 0f;
    public int CurrentIndex=0;
    public string originalText; 
    private bool isHighlighted = false;

    public GameObject Apply;
    public GameObject Choose;
    public GameObject Timer;
    public Color highlightColor = Color.red;
    //TimerBar
    //public GameObject Bar;
    public Image bar;
    public float time = 20.0f;
    //public float current = 0.0f;
    public AudioClip FirstVoiceOver;
    private float amplituide;
    private void Awake()
    {
        
    }

    void Start()
    {
        originalText = TextDisplay.text;
        TextDisplay.text = "";
    }

    void Update()
    {
        visually(TextDisplay.text);
    }
  
    public void StopLoading(float _time)
    {
        StopCoroutine(LoadingTimer(time));
    }

    public void callLoadingTimer(float _time)
    {
        StartCoroutine(LoadingTimer(_time));
    }
    private IEnumerator LoadingTimer(float _time)
    {
        float current = 0.0f;
        while (current < _time)
        {
            current += Time.deltaTime;
            float progress = current / _time;
            bar.fillAmount = progress;
            yield return null;
        }
    }
    
    public void Setup()
    {
        
        if (Microphone.devices.Length > 0)
        {
            audioSource.outputAudioMixerGroup = _MicrophoneMixer;
            audioSource.clip = Microphone.Start(null, true, 10, AudioSettings.outputSampleRate);
            audioSource.loop = true;


            while (!(Microphone.GetPosition(null) > 0)) { }

            audioSource.Play();
            isMicrophoneInitialized = true;
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
        
    }

    public void visually(string txt)
    {
        
        if (isMicrophoneInitialized)
        {
            float[] audioData = new float[256];
            audioSource.GetOutputData(audioData, 0);
            float audioLevel = 0f;

            foreach (float sample in audioData)
            {
                audioLevel += Mathf.Abs(sample);
            }
            audioLevel /= audioData.Length;
            amplituide = audioLevel * 100;
            lineScale = Mathf.Clamp(audioLevel * lineScaleFactor, 0f, maxLineScale);
            lineRectTransform.localScale = new Vector3(lineScale, 1f, 1f);
           // Debug.Log($"Your voice level is : {amplituide}");
            if (amplituide >= 1f)
            {
                float volume = Mathf.Lerp(minVoume, maxVoume, lerpDuration);
                _Voice.audioMixer.SetFloat("VoiceVolume", -80f);
                //StartCoroutine(LinebyLineWithColor(txt));
                
            }
            if (amplituide <= 0.1f)
            {
                float volume = Mathf.Lerp(minVoume_1, maxVoume_1, lerpDuration);
                _Voice.audioMixer.SetFloat("VoiceVolume", volume);
                //StopHighlight();
            }
        }         
    }

    public void StartHighlight(string final)
    {
     
        isHighlighted = true;
        CurrentIndex = 0;
        TextDisplay.text = "";
       // StartCoroutine(HighlightText(final));
    }

    public void StopHighlight()
    {
       
        TextDisplay.text = originalText;
        TextDisplay.color = Color.white; 
    }

    public void CallLineByLine(string final)
    {
        StartCoroutine(LinebyLine(final));
    }

    public IEnumerator LinebyLine(string final_text)
    {
        TextDisplay.text = "";
        foreach (char letter in final_text.ToCharArray())
        {
            
            TextDisplay.text += letter;
            TextDisplay.color += Color.red;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    public void CallLineByLineWithHighlighted(string final)
    {
        StartCoroutine(LinebyLineWithColor(final));
    }

    public IEnumerator LinebyLineWithColor(string final_text)
    {

        //Todo
        //pointer=0
        //while(true)
        //{
        //0- Stop condition : if(pointer>text.length-1)
        //1- check for amplituide 
        //2- if > threshold {Highlight}
        //3- Highlight 
        //-- DisplayText[pointer]
        //-- ColringText[pointer]
        //-- pointer++
        //-- yield wait 2 seconds 
        //}
        int pointer = 0;
        while (true)
        {
            if (pointer > final_text.Length -1)
            {
                Debug.Log("Doneeeeeeeeeeeee");
            }
            if (amplituide <= 1f)
            {
                Debug.Log("Gendyyyyyyyyyyyyyyyy");
                TextDisplay.text += final_text[pointer];
                TextDisplay.text += highlightColor[pointer];
                pointer++;
                yield return new WaitForSeconds(2f);
            }
        }
    }

   

    public void OnTextItemSelected()
    {
        TextDisplay.text = _text_One;
        TextDisplay.text = _text_Two;
    }
    public void SelectOne(string first)
    {
        _text_1.text = first;
    }
    public void Selecttwo(string second)
    {
        _text_2.text = second;
        Debug.Log(second);
    }

    public void Hide()
    {
        Apply.SetActive(true);
        Choose.SetActive(false);
        Timer.SetActive(false);
    }

    public void PlayVoiceOver(AudioClip clip)
    {
        //_Voice.audioMixer.SetFloat("VoiceVolume", 2);
        //VoiceOver.outputAudioMixerGroup = _Voice;
        VoiceOver.clip = clip;
        VoiceOver.Play();
        VoiceOver.loop = true;
       // Debug.Log($" THE voice over is :{VoiceOver.name}");
    }
    public void DefaultCanvas(AudioClip clip , string DefaultText)
    {
        TextDisplay.text = DefaultText;
        Setup();
        visually(DefaultText);
        CallLineByLine(DefaultText);
        
        Hide();
        PlayVoiceOver(clip);
    }

    public void PlayVoiceOverDefault(AudioClip clip)
    {
        StartCoroutine(PlayAudio(clip));
    }

    private IEnumerator PlayAudio(AudioClip clip)
    {
        _Voice.audioMixer.SetFloat("VoiceVolume", 2);
        VoiceOver.outputAudioMixerGroup = _Voice;
        VoiceOver.clip = clip;
        VoiceOver.Play();
        //VoiceOver.loop = true;
        yield return new WaitForSeconds(1f);
    }
    void OnDestroy()
    {
        if (isMicrophoneInitialized)
        {
            Microphone.End(null);
        }
    }

    

}



