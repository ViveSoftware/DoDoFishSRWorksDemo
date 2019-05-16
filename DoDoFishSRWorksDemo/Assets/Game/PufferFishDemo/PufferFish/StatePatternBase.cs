using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;

abstract public class StatePatternBase
{
    protected IState _currentState;
    protected IState[] _stateList;
    protected abstract void OnCreateState(out IState[] pleaseCreateThisList);
    //protected abstract void OnUpdateBase();
    public abstract void DebugLog(string text, int level = 0);

    public IState GetCurrentState()
    {
        return _currentState;
    }

    public void Restart()
    {
        try
        {
            OnCreateState(out _stateList);
        }
        catch (Exception e)
        {
            DebugLog("error error error : OnCreateState()[" + _currentState.Name() + "] : " + e.StackTrace + " , Message : " + e.Message, 2);
        }
    }

    public virtual void Update()
    {
        //DebugLog("[StatePatternBase] update : " + _currentState.Name());

        if (_currentState == null)
            DebugLog("[StatePatternBase] _currentState == null , maybe you are not calling Restart()");

        //try
        //{
        if (_currentState != null)
            _currentState.UpdateState();
        //OnUpdateBase();
        //}
        //catch (Exception e)
        //{
        //    DebugLog("error error error : UpdateState()[" + _currentState.Name() + "] : " + e.StackTrace + " , Message : " + e.Message, 2);
        //}
    }

    public void SwitchState(object newState)
    {
        int stateID = (int)newState;
        DebugLog((newState == null) ? "newState==null": ("newState not null : " + stateID));
        DebugLog("[StatePatternBase][SwitchState] :" + ((_currentState == null) ? "none" : _currentState.Name()) + " -> : " + _stateList[stateID].Name());
        if (stateID >= _stateList.Length)
            Debug.LogError("[SwitchState] : stateID >= _stateList.Length");
        if (_currentState != null)
            _currentState.LeaveState();
        IState oldState = _currentState;
        _currentState = _stateList[(int)newState];
        _currentState.EnterState(oldState, this);
    }
}

