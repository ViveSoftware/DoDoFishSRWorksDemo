/**
 *
 * HTC Corporation Proprietary Rights Acknowledgment
 * Copyright (c) 2016 HTC Corporation
 * All Rights Reserved.
 *
 * The information contained in this work is the exclusive property of HTC Corporation
 * ("HTC").  Only the user who is legally authorized by HTC ("Authorized User") has
 * right to employ this work within the scope of this statement.  Nevertheless, the
 * Authorized User shall not use this work for any purpose other than the purpose
 * agreed by HTC.  Any and all addition or modification to this work shall be
 * unconditionally granted back to HTC and such addition or modification shall be
 * solely owned by HTC.  No right is granted under this statement, including but not
 * limited to, distribution, reproduction, and transmission, except as otherwise
 * provided in this statement.  Any other usage of this work shall be subject to the
 * further written consent of HTC.
 *
 * @file    "VRBreakoutStateManager.cs"
 * @desc    The class that manages game states.
 * @author  shengjie luo
 * @history	2016/01/29
 *
 */

using UnityEngine;
using System.Collections;

public abstract class StateManager : MonoBehaviour
{
    // Current state.
    protected State curState = null;
    // Next State Id
    private int nextStateId = -1;
    // Last State Id
    private int lastStateId = -1;
    // All states.
    protected State[] states = null;
    // The GameObject that contains all states.
    protected GameObject statesObj = null;

    public abstract void InitStates();

    public State getCurState()
    {
        return curState;
    }

    public int GetCurStateId()
    {
        if (curState != null)
            return curState.stateId;
        else
            return -1;
    }

    public int GetNextStateId()
    {
        return nextStateId;
    }

    public int GetLastStateId()
    {
        return lastStateId;
    }

    public bool TriggerStateChange(int fromStateId, int toStateId)
    {
        if (curState == null)
        {
            return false;
        }

        return curState.OnTriggerStateChange(fromStateId, toStateId);
    }

    public void SetState(int stateId)
    {
        if (stateId < 0 || stateId >= states.Length)
        {
            Debug.LogError("[StateManager.SetStates] stateId: " + stateId + " out of array index");
            return;
        }

        if (states[stateId] == null)
        {
            Debug.LogError("[StateManager.SetStates] the new state is null");
            return;
        }

        if (curState != null)
        {
            Debug.Log(curState.GetType().ToString() + " Exit");
            nextStateId = stateId;
            curState.StateExit();
        }
        lastStateId = GetCurStateId();
        curState = states[stateId];
        Debug.Log(curState.GetType().ToString() + " Enter");
        nextStateId = -1;
        curState.StateEnter();
    }

    void Update()
    {
        if (curState != null)
        {
            curState.StateUpdate();
        }
    }
}
