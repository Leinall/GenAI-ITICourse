using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField] protected bool autoStart;
    [SerializeField] public List<StateBase> steps = new List<StateBase>();
    protected StateBase currentStep;
    protected virtual void Start()
    {
        if (autoStart)
            StartNextStep(0);
    }
    protected virtual void StartNextStep(int index)
    {
        if (index < steps.Count)
        {
            currentStep = steps[index];
            currentStep.OnStepCompleted += this.OnStepCompleted;
            currentStep.enabled = true; //enable script, initially disabled
            currentStep.StartStep();
        }
    }

    protected virtual void OnStepCompleted()
    {
        currentStep.OnStepCompleted -= this.OnStepCompleted;
        currentStep.enabled = false;
        StartNextStep(steps.IndexOf(currentStep) + 1);
    }

    public virtual void StartSpecificStep(int index)
    {
        if (currentStep != null)
        {
            currentStep.OnStepCompleted -= this.OnStepCompleted;
            currentStep.enabled = false;
        }
        StartNextStep(index);
    }
}
