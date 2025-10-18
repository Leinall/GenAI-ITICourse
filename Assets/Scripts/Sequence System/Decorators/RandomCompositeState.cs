using System.Collections.Generic;
using UnityEngine;

public class RandomCompositeState : StateBase
{
    [SerializeField] private List<StateBase> states;

    private StateBase _chosenState;
    public override void StartStep()
    {
        int randomIndex = Random.Range(0, states.Count);
        states[randomIndex].OnStepCompleted += OnRandomStateCompleted;
        states[randomIndex].StartStep();
        _chosenState = states[randomIndex];
    }

    public override void EndStep()
    {
        _chosenState.OnStepCompleted -= OnRandomStateCompleted;
        OnStepCompleted?.Invoke();
    }

    private void OnRandomStateCompleted()
    {
        EndStep();
    }
}
