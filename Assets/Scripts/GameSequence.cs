using UnityEngine;

public class GameSequence : StateManager
{

    public Transform Camera;

    public void GoToWayPoint(Transform transform)
    {
        Camera.transform.parent = transform;
        Camera.transform.localPosition = Vector3.zero;
    }
}
