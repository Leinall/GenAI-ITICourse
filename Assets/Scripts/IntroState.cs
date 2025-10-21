using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroState : StateBase
{
    // close all the panels
    public GameObject IntroPanel;
    public GameObject DisclamerPanel;
    public GameObject ChoosePanel;
    public GameObject EvaluationPanel;
    public GameObject FeedbackPanel;

    public Button nextBtn;
    public VideoPlayer videoPlayer;

    private void Start()
    {
        DisclamerPanel.SetActive(false);
        ChoosePanel.SetActive(false);
        EvaluationPanel.SetActive(false);
        FeedbackPanel.SetActive(false);
    }

    public override void StartStep()
    {

        print("step1");
        nextBtn.interactable = false;

        videoPlayer.Play();
        // Subscribe to the event
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        print("Video finished playing");
        nextBtn.interactable = true;
    }

    public override void EndStep()
    {
        DisclamerPanel.SetActive(true);
    }
}
