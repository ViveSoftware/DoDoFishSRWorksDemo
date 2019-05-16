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
 * @file    "State.cs"
 * @desc    The abstract class for a state.
 * @author	shengjie luo
 * @history	2016/01/30
 *
 */

using UnityEngine;
using System.Collections;

public abstract class State : MonoBehaviour
{
    protected StateManager stateManager;
    public int stateId = -1;

    public void Init(StateManager stateManager, int stateId)
    {
        this.stateManager = stateManager;
        this.stateId = stateId;
    }

    public abstract void StateEnter();
    public abstract void StateExit();
    public abstract void StateUpdate();
    public virtual bool OnTriggerStateChange(int fromStateId, int toStateID)
    {
        return false;
    }

    public virtual void ControllerFunctionDefine()
    {

    }

}
