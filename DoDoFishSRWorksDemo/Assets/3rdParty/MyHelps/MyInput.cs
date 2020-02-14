using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class MyInput
{
    public static bool isJoystickController(uint deviceIndex)
    {
        if (!OpenVR.System.IsTrackedDeviceConnected(deviceIndex))
            return false;
        //enum EVRControllerAxisType
        //  {
        //   k_eControllerAxis_None = 0,
        //   k_eControllerAxis_TrackPad = 1,
        //   k_eControllerAxis_Joystick = 2,
        //   k_eControllerAxis_Trigger = 3, // Analog trigger data is in the X axis
        //  };
        var error = default(ETrackedPropertyError);
        int rrr = OpenVR.System.GetInt32TrackedDeviceProperty(deviceIndex, ETrackedDeviceProperty.Prop_Axis0Type_Int32, ref error);
        if (error != ETrackedPropertyError.TrackedProp_Success)
            Debug.Log("[_isJoystickController] " + rrr);
        else if (rrr == 2)
            return true;

        error = default(ETrackedPropertyError);
        rrr = OpenVR.System.GetInt32TrackedDeviceProperty(deviceIndex, ETrackedDeviceProperty.Prop_Axis1Type_Int32, ref error);
        if (error != ETrackedPropertyError.TrackedProp_Success)
            Debug.Log("[_isJoystickController] " + rrr);
        else if (rrr == 2)
            return true;

        error = default(ETrackedPropertyError);
        rrr = OpenVR.System.GetInt32TrackedDeviceProperty(deviceIndex, ETrackedDeviceProperty.Prop_Axis2Type_Int32, ref error);
        if (error != ETrackedPropertyError.TrackedProp_Success)
            Debug.Log("[_isJoystickController] " + rrr);
        else if (rrr == 2)
            return true;

        error = default(ETrackedPropertyError);
        rrr = OpenVR.System.GetInt32TrackedDeviceProperty(deviceIndex, ETrackedDeviceProperty.Prop_Axis3Type_Int32, ref error);
        if (error != ETrackedPropertyError.TrackedProp_Success)
            Debug.Log("[_isJoystickController] " + rrr);
        else if (rrr == 2)
            return true;

        error = default(ETrackedPropertyError);
        rrr = OpenVR.System.GetInt32TrackedDeviceProperty(deviceIndex, ETrackedDeviceProperty.Prop_Axis4Type_Int32, ref error);
        if (error != ETrackedPropertyError.TrackedProp_Success)
            Debug.Log("[_isJoystickController] " + rrr);
        else if (rrr == 2)
            return true;
        return false;
    }

    public static bool IsCosmos
    {
        get
        {
#if UNITY_2017_2_OR_NEWER
            string modelName = UnityEngine.XR.XRDevice.model;
#else // !UNITY_2017_2_OR_NEWER
            string modelName = UnityEngine.VR.VRDevice.model;
#endif // UNITY_2017_2_OR_NEWER
            return (modelName.ToLower().Contains("cosmos"));
        }
    }

    public float SwipeSpeed;
    public bool PadLPress, PadRPress, PadUpPress, PadDnPress, /*PadCPress,*/  _padPressDown,
        PadSwipeL, PadSwipeR, PadSwipeU, PadSwipeD, PadKeyUp/*, PadTouch*/;
    public bool /*padFirstPressUp,*/ PadUpTouch, PadDnTouch, PadLTouch, PadRTouch;


    HandRole _handRole;
    public MyInput(HandRole handRole)
    {
        _handRole = handRole;
    }

    public bool IsJoystick { get { return isJoystickController(ViveRole.GetDeviceIndex(_handRole)); } }
  
    public bool PadPressDown
    {
        get
        {
            bool isJoystick = isJoystickController(ViveRole.GetDeviceIndex(_handRole));
            if (isJoystick)
            {
                //因為joystick太難壓下，所以往上推快速放開當作PAD press
                //這邊是往上到底推一陣子，當作jotstick press = PAD press
                return (PadUpPress || PadDnPress || PadLPress || PadRPress);
            }
            else
                return _padPressDown;
        }
    }

    bool isJoystick;
    public bool PadTouch
    {
        get
        {
            //isJoystick = isJoystickController(ViveRole.GetDeviceIndex(_handRole));
            if (isJoystick)
            {
                const float padTouchThreshold = 0.1f;
                Vector2 v = joystickAxis;//ViveInput.GetPadAxis(HandRole.RightHand);
                if (v.sqrMagnitude > padTouchThreshold)
                    return true;
                return false;
            }
            else
                return ViveInput.GetPress(_handRole, ControllerButton.PadTouch);
        }
    }

    float? _padTouchTimeCount;
    public bool PadTouchDuration(float duration)
    {
        if (!PadTouch)
        {
            _padTouchTimeCount = null;
            return false;
        }

        if (_padTouchTimeCount == null)
            _padTouchTimeCount = Time.unscaledTime;

        if (_padTouchTimeCount.HasValue)
        {
            float offset = Time.unscaledTime - _padTouchTimeCount.Value;
            if (offset > duration)
            {
                _padTouchTimeCount = null;
                return true;
            }
        }
        return false;
    }

    float _joystickUpTimeCount, _joystickDnTimeCount, _joystickLTimeCount, _joystickRTimeCount,
        _prevAxisY, _startAxisY, _startAxisYTime;
    bool _teleportPointerVisible;
    Vector2 joystickAxis;
    void _updateJoystickPadPress()
    {
        const float axisThreshold = 0.5f, timeThreshold = 0.2f;
        joystickAxis = ViveInput.GetPadAxis(_handRole);
        if (joystickAxis.y > axisThreshold)
            _joystickUpTimeCount -= Time.unscaledDeltaTime;
        else
            _joystickUpTimeCount = timeThreshold;

        if (joystickAxis.y < -axisThreshold)
            _joystickDnTimeCount -= Time.unscaledDeltaTime;
        else
            _joystickDnTimeCount = timeThreshold;

        if (joystickAxis.x > axisThreshold)
            _joystickRTimeCount -= Time.unscaledDeltaTime;
        else
            _joystickRTimeCount = timeThreshold;

        if (joystickAxis.x < -axisThreshold)
            _joystickLTimeCount -= Time.unscaledDeltaTime;
        else
            _joystickLTimeCount = timeThreshold;

        PadUpPress = (_joystickUpTimeCount < 0);
        PadDnPress = (_joystickDnTimeCount < 0);
        PadLPress = (_joystickLTimeCount < 0);
        PadRPress = (_joystickRTimeCount < 0);

        if (PadUpPress)
            _teleportPointerVisible = true;

        //參考用，沒法慢慢放開取消:
        //if (isJoystickController(ViveRole.GetDeviceIndex(HandRole.LeftHand)))
        //{
        //    if (TeleportPointerVisible)
        //    {
        //        float axisY = ViveInput.GetAxis(HandRole.LeftHand, ControllerAxis.PadY);
        //        if (Mathf.Abs(axisY) < 0.1f)
        //        {
        //            Teleport(targetPos);
        //        }
        //    }
        //}

        float offsetY = _prevAxisY - joystickAxis.y;
        _prevAxisY = joystickAxis.y;

        //修正可以慢慢放開取消teleport的方式
        //modify teleport joystick can slowly move to Y0, to cancel Teleport.
        if (_teleportPointerVisible)
        {
            if (offsetY > 0) //大於0表示往下慢慢放開
            {
                if (_startAxisYTime < 0)
                    _startAxisYTime = Time.time;
            }
            else
            {
                _startAxisY = joystickAxis.y;
                _startAxisYTime = -1;
            }

            if (Mathf.Abs(joystickAxis.y) < 0.1f && _startAxisYTime > 0)
            {
                //往下放開速率又超過就代表放開jotstick
                float velocity = (_startAxisY - joystickAxis.y) / (Time.time - _startAxisYTime);
                if (velocity > 3)
                {
                    //DoTeleport(targetPos);
                    PadKeyUp = true;//因為joystick太難壓下，所以往上推到底一陣子，之後快速放開當作PAD press之後放開
                    _teleportPointerVisible = false;
                }
            }
        }

        if (!PadTouch)
            _teleportPointerVisible = false;
    }

    public bool IsFirstGrip
    {
        get { return ViveInput.GetPressDown(_handRole, ControllerButton.Grip); }
    }

    public bool IsFirstTrigger
    {
        get { return ViveInput.GetPressDown(_handRole, ControllerButton.Trigger); }
    }

    public bool IsFirstBumper
    {
        get { return ViveInput.GetPressDown(_handRole, ControllerButton.Bumper); }
    }

    public bool IsKeepBumper
    {
        get { return ViveInput.GetPress(_handRole, ControllerButton.Bumper); }
    }

    public bool IsFirstCosmosAKey
    {
        get { return ViveInput.GetPressDown(_handRole, ControllerButton.AKey); }
    }

    public bool IsFirstCosmosBKey
    {
        get { return ViveInput.GetPressDown(_handRole, ControllerButton.BKey); }
    }

    public bool IsKeepTrigger
    {
        get { return ViveInput.GetPress(_handRole, ControllerButton.Trigger); }
    }

    public void Update()
    {
        isJoystick = isJoystickController(ViveRole.GetDeviceIndex(_handRole));

        /*PadIsFirstPressUp = */
        PadUpTouch = PadDnTouch = PadLTouch = PadRTouch = /*PadCPress =*/
    PadUpPress = PadDnPress = PadLPress = PadRPress = PadKeyUp =
    PadSwipeL = PadSwipeR = PadSwipeU = PadSwipeD = false;

        Vector2 padTouchAxis = (isJoystick) ? ViveInput.GetPadAxis(_handRole) : ViveInput.GetPadTouchAxis(_handRole);
        const float PadThreshold = 0.7f;
        //Debug.Log(padTouchAxis);
        if (padTouchAxis.x > PadThreshold)
            PadLTouch = true;
        else if (padTouchAxis.x < -PadThreshold)
            PadRTouch = true;

        if (padTouchAxis.y > PadThreshold)
            PadUpTouch = true;
        else if (padTouchAxis.y < -PadThreshold)
            PadDnTouch = true;

        if (isJoystick)
        {
            _updateJoystickPadPress();
        }
        else
        {
            //check PadKeyUp
            Vector2 padPressAxis = ViveInput.GetPadPressAxis(_handRole);
            if (_padPressDown)
            {
                _padPressDown = false;
                if (padPressAxis.x == 0 && padPressAxis.y == 0)
                {
                    //padFirstPressUp = true;
                    PadKeyUp = true;
                }
            }

            //check PadPressLeft, PadPressRight, PadPressUp, PadPressDown
            if (padPressAxis.x != 0 || padPressAxis.y != 0)
            {
                _padPressDown = true;
                if (padPressAxis.x > PadThreshold)
                    PadLPress = true;
                else if (padPressAxis.x < -PadThreshold)
                    PadRPress = true;

                if (padPressAxis.y > PadThreshold)
                {
                    if (!isJoystick)
                        PadUpPress = true;
                }
                else if (padPressAxis.y < -PadThreshold)
                    PadDnPress = true;

                //if (padPressAxis.x < PadThreshold &&
                //    padPressAxis.x > -PadThreshold &&
                //    padPressAxis.y < PadThreshold &&
                //    padPressAxis.y > -PadThreshold)
                //{
                //    PadCPress = true;
                //}
            }

            //if (padRightFirstPressUp)
            //    PadIsFirstPressUp = true;
            //padRightFirstPressUp = false;
            // Debug.LogWarning("padRightPress : " + padRightPress + " : " + padRightPressAxis);

        }

        //detect swipe
        _swipe();
    }

    const float swipeThreshold = 0.65f;
    bool _isSwiping;
    Vector2 swipeStartPos, swipeEndPos;
    bool joyStickCanSwipeStart;
    void _swipe()
    {
        if (isJoystick)
        {
            if (Mathf.Abs(joystickAxis.x) < 0.1f)
                joyStickCanSwipeStart = true;
        }
        else
            joyStickCanSwipeStart = true;

        if (!joyStickCanSwipeStart)
            return;

        bool isSwipeStart = (isJoystick) ? (Mathf.Abs(joystickAxis.x) < 0.9f) : PadTouch;
        if (isSwipeStart)
        {
            if (!_isSwiping)
            {
                _isSwiping = true;
                if (isJoystick)
                {
                    swipeStartPos = joystickAxis;
                }
                else
                {
                    swipeStartPos = ViveInput.GetPadTouchAxis(_handRole);
                }
                //  Debug.LogWarning("Swipe Start");
            }
            else
            {
                if (isJoystick)
                {
                    swipeEndPos = joystickAxis;
                }
                else
                {
                    swipeEndPos = ViveInput.GetPadTouchAxis(_handRole);
                }
            }
        }
        else
        {
            if (swipeEndPos.sqrMagnitude > 0)
            {
                Vector2 delta = swipeEndPos - swipeStartPos;
                swipeEndPos = Vector2.zero;
                _isSwiping = false;
                joyStickCanSwipeStart = false;
                // Debug.LogWarning("Swipe End");

                if (delta.x * delta.x > delta.y * delta.y)
                {
                    if (Mathf.Abs(delta.x) > swipeThreshold * ((isJoystick) ? 0.5f : 1))
                    {
                        SwipeSpeed = delta.x;
                        if (delta.x > 0)
                        {
                            PadSwipeR = true;
                            Debug.LogWarning("Swipe To Right");
                        }
                        else
                        {
                            PadSwipeL = true;
                            Debug.LogWarning("Swipe To Left");
                        }
                    }
                }
                else
                {
                    if (Mathf.Abs(delta.y) > swipeThreshold * 0.7f * ((isJoystick) ? 0.5f : 1))//swipe up/down more sensitive
                    {
                        SwipeSpeed = delta.y;
                        if (delta.y > 0)
                        {
                            PadSwipeU = true;
                            Debug.Log("Swipe To Up");
                        }
                        else
                        {
                            PadSwipeD = true;
                            Debug.Log("Swipe To Down");
                        }
                    }
                }
            }
        }
    }
}
