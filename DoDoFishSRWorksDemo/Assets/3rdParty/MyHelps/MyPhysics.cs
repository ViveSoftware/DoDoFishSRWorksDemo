using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPhysics : MonoBehaviour
{
    public bool UseGravity;
    public bool FreezeAll;
    public CollisionDetectionMode forceCollision = CollisionDetectionMode.ContinuousDynamic;

    [Header("Racket-------------------------------------------")]
    public bool IsRacketRigidbody;
    public GameObject RacketFollowHand;

    [Header("Ball-------------------------------------------")]
    public bool IsBallRigidbody;

    private void Awake()
    {
        if (IsRacketRigidbody)
        {
            SetRacketRigidbody(gameObject, RacketFollowHand);
        }
        else if (IsBallRigidbody)
        {
            SetBallRigidbody(gameObject);
        }
    }

    /// <summary>
    /// Add a rigidbody for Racket, very hight speed wave racket and the ball not through racket(球拍).
    /// and use _rb.MovePosition to follow hand.
    /// </summary>
    public class RacketRigidbody : MonoBehaviour
    {
        GameObject _followObj;
        Rigidbody _rb;

        /// <summary>
        /// Add a rigidbody for Racket, very hight speed wave racket and the ball not through racket(球拍).
        /// </summary>
        public void Init(GameObject followHandObj)
        {
            _followObj = followHandObj;
            _rb = this.gameObject.GetComponent<Rigidbody>();
            if (_rb == null)
                _rb = this.gameObject.AddComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            //_rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void Update()
        {
            _rb.MovePosition(_followObj.transform.position);
        }
    }

    public static void SetRacketRigidbody(GameObject racketObj, GameObject followHandObj)
    {
        RacketRigidbody racket = racketObj.AddComponent<RacketRigidbody>();
        racket.Init(followHandObj);

        MyPhysics myPhys = racketObj.GetComponent<MyPhysics>();
        // if (racketObj.GetComponent<Rigidbody>() != null)
        // {
        racketObj.GetComponent<Rigidbody>().useGravity = myPhys.UseGravity;
        racketObj.GetComponent<Rigidbody>().constraints = (myPhys.FreezeAll) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        racketObj.GetComponent<Rigidbody>().collisionDetectionMode = myPhys.forceCollision;
        // }
    }

    public static void SetBallRigidbody(GameObject ballObj)
    {
        Rigidbody rb = ballObj.GetComponent<Rigidbody>();
        if (rb == null)
            rb = ballObj.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        //rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        MyPhysics myPhys = ballObj.GetComponent<MyPhysics>();
        // if (ballObj.GetComponent<Rigidbody>() != null)
        // {
        ballObj.GetComponent<Rigidbody>().useGravity = myPhys.UseGravity;
        ballObj.GetComponent<Rigidbody>().constraints = (myPhys.FreezeAll) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        ballObj.GetComponent<Rigidbody>().collisionDetectionMode = myPhys.forceCollision;
        // }
    }
}
