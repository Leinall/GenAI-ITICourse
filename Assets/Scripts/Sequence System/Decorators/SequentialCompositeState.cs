using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class responsible for excuting substates in sequential.
/// </summary>
public class SequentialCompositeState : StateBase
{
    [SerializeField] protected List<StateBase> subStates;

    protected int _finishedStatedCount;
    private StateBase _currentState;

    public override void StartStep()
    {
        subStates[0].OnStepCompleted += OnSubStateCompleted;
        _currentState = subStates[0];
        subStates[0].StartStep();
    }

    public override void EndStep()
    {
        OnStepCompleted?.Invoke();
    }

    protected virtual void OnSubStateCompleted()
    {
        _currentState.OnStepCompleted -= OnSubStateCompleted;

        _finishedStatedCount++;
        if(_finishedStatedCount >= subStates.Count)
        {
            //finished all steps
            EndStep();
        }
        else
        {
            //start next step
            subStates[_finishedStatedCount].OnStepCompleted += OnSubStateCompleted;
            _currentState = subStates[_finishedStatedCount];
            subStates[_finishedStatedCount].StartStep();
        }
    }
}
