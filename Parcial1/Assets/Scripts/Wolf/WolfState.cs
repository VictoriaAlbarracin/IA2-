using System;
using System.Collections.Generic;

public class WolfState<TInput>
{
    public string StateName { get; private set; }

    public event Action<TInput> OnEnter = delegate { };
    public event Action OnUpdate = delegate { };
    public event Action<TInput> OnExit = delegate { };

    private Dictionary<TInput, WolfState<TInput>> _transitions = new Dictionary<TInput, WolfState<TInput>>();

    public WolfState(string name)
    {
        StateName = name;
    }

    public void AddTransition(TInput input, WolfState<TInput> targetState)
    {
        if (!_transitions.ContainsKey(input))
            _transitions.Add(input, targetState);
    }

    public bool TryGetTransition(TInput input, out WolfState<TInput> nextState)
    {
        return _transitions.TryGetValue(input, out nextState);
    }

    public void Enter(TInput input) => OnEnter(input);
    public void Update() => OnUpdate();
    public void Exit(TInput input) => OnExit(input);
}

public class WolfStateMachine<TInput>
{
    public WolfState<TInput> CurrentState { get; private set; }

    public WolfStateMachine(WolfState<TInput> initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter(default);
    }

    public void TriggerInput(TInput input)
    {
        if (CurrentState.TryGetTransition(input, out WolfState<TInput> nextState))
        {
            CurrentState.Exit(input);
            CurrentState = nextState;
            CurrentState.Enter(input);
        }
    }

    public void Tick()
    {
        CurrentState?.Update();
    }
}