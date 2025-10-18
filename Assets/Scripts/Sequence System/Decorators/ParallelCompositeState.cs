using System.Collections.Generic;
using UnityEngine;

public class ParallelCompositeState : StateBase
{
    [SerializeField] private List<StateBase> subStates;

    private int _finishedStatedCount;

    public override void StartStep()
    {
        foreach (var subState in subStates)
        {
            subState.OnStepCompleted += OnSubStateCompleted;
            subState.StartStep();
        }
    }

    public override void EndStep()
    {
        foreach (var subState in subStates)
        {
            subState.OnStepCompleted -= OnSubStateCompleted;
        }
        OnStepCompleted.Invoke();
    }

    private void OnSubStateCompleted()
    {
        _finishedStatedCount++;

        if (_finishedStatedCount >= subStates.Count)
        {
            EndStep();
        }
    }
}
