using UnityEngine;

public class ChooseAgent : StateBase
{

    public Transform[] InterogationPoints;
    public override void StartStep()
    {

    }

    public override void EndStep()
    {

    }

    public void CameraMoveToAPoint(int charID)
    {
        Camera.main.transform.position = InterogationPoints[charID].transform.position;
    }


}
