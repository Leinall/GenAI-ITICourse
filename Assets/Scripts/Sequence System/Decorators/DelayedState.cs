using UnityEngine;

public class DelayedState : StateBase
{
    [SerializeField] private StateBase state;
    [SerializeField] private float delayTime;


    public override void StartStep()
    {
        Invoke(nameof(StartDelayedState), delayTime);
    }

    public override void EndStep()
    {
        state.OnStepCompleted -= () => EndStep();
        OnStepCompleted?.Invoke();
    }

    private void StartDelayedState()
    {
        state.OnStepCompleted += () => EndStep();
        state.StartStep();
    }
}
