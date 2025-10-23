namespace Overwolf.CFCore.Actions {
  public interface ICFCAction {
    event CFCActionStart onActionStarted;
    event CFCActionComplete onActionComplete;

    void StartAction();
  }

  public delegate void CFCActionStart();

  public delegate bool CFCActionComplete();
}