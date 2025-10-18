using System.Collections.Generic;
/// <summary>
/// This is a normal state but control additional substates
/// </summary>
public class ControlStateBase : StateBase
{
    public List<SubStateBase> subStates = new List<SubStateBase>();

    public override void StartStep()
    {
    }
    public override void EndStep()
    {
    }
}
