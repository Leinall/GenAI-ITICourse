using UnityEngine;
using UnityEngine.Events;

public class StateBase : MonoBehaviour
{
    public UnityAction OnStepCompleted;

    public virtual void StartStep()
    {

    }

    public virtual void EndStep()
    {

    }
}

