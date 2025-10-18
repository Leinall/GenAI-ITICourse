/// <summary>
/// Substates are controlled by a state
/// The idea of implementing substates is adding more behaviour to the state may related to the context of the state
/// e.g(Log In Module) controlState handles the loading data and every substate handles more scenarios (Forget password, Verification, Sign up, etc)
/// It's your call to implement the way that controlState handles substates (Sequenial, parallel, action based, more options based on a certain input)
/// </summary>
public class SubStateBase : StateBase
{
}
