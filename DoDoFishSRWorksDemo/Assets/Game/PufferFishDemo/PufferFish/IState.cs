using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    string Name();

    /// <summary>
    /// oldState is previous state
    /// </summary>    
    void EnterState(IState oldState, StatePatternBase statePatternBase);

    void UpdateState();

    /// <summary>
    /// nextState is new incomong state
    /// </summary>
    void LeaveState();
}