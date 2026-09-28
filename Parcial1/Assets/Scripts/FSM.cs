using System.Collections.Generic;
using Unity.VisualScripting;

public class FSM
{
    Dictionary<string, IState> _states = new Dictionary<string, IState>();
    public IState _currentState;

    public void AddState(string name, IState state)
    {
        if (_states.ContainsKey(name)) return;

        _states.Add(name, state);
    }

    public void Execute()
    {
        if (_currentState != null)
            _currentState.OnUpdate();
    }

    public void ChangeState(string name)
    {
        if (!_states.ContainsKey(name)) return;

        if (_currentState == _states[name]) return;

        if (_currentState != null)
            _currentState.OnExit();

        _currentState = _states[name];

        _currentState.OnEnter();
    }
}