using Demo;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    RobotSound robotSound;
    Rigidbody rb;
    Coroutine addForceCoroutine;
    public float addForceTime = 0.2f;
    public bool resetForce;
    public float minRatio = 0.2f;
    public float flyForceMin = 10, flyForceMax = 20;
    public float stallHigh = 0.5f;
    public float stallRange = 1f;
    public float moveForce = 2;
    public float turnForce = 1;
    float shieldEffectTime = 0.8f;
    bool canAddforce;
    Animator ani;
    Transform discoLightRoot;
    Transform headSpotlight;

    void Start()
    {
        robotSound = GetComponentInChildren<RobotSound>();
        ani = GetComponentInChildren<Animator>();

        discoLightRoot = transform.Find("DiscoLightRoot");
        discoLightRoot.DORotate(Vector3.up * 360, 3, RotateMode.LocalAxisAdd).SetLoops(-1).SetEase(Ease.Linear);
        discoLightRoot.transform.parent = null;//Move to root , since discoLight will causes rigidbody broken.

        MyHelpNode.FindTransform(transform, "RobotHeadSpotlight", out headSpotlight);
        rb = GetComponent<Rigidbody>();
    }

    public void SwitchDiscoLight()
    {
        discoLightRoot.gameObject.SetActive(!discoLightRoot.gameObject.activeSelf);
        headSpotlight.gameObject.SetActive(!discoLightRoot.gameObject.activeSelf);
    }

    void Update()
    {
        discoLightRoot.transform.position = transform.position;
        //discoLightRoot.transform.rotation = transform.rotation;
        if (resetForce)
        {
            resetForce = false;
            transform.position = Vector3.zero;
            rb.velocity = Vector3.zero;
        }

        if (transform.position.y < stallHigh)
        {
            canAddforce = true;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveForwrad();
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveBack();
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            TurnLeft();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            TurnRight();
        }
    }

    private void OnEnable()
    {
        addForceCoroutine = StartCoroutine(_addForceCoroutine());

        if (headSpotlight != null)
            headSpotlight.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        StopCoroutine(addForceCoroutine);
        addForceCoroutine = null;

        discoLightRoot.gameObject.SetActive(false);
    }

    IEnumerator _addForceCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(addForceTime);
            if (canAddforce)
            {
                canAddforce = false;
                float ratio = stallHigh - transform.position.y;
                ratio /= stallRange;
                ratio = Mathf.Max(minRatio, ratio);
                rb.AddForce(Vector3.up * Random.Range(flyForceMin, flyForceMax) * ratio, ForceMode.Force);
            }
        }
    }

    public void ShowUp(Vector3 camPos, Vector3 camForward)
    {
        robotSound = GetComponentInChildren<RobotSound>();
        ani = GetComponentInChildren<Animator>();
        ani.SetTrigger("ShowUp");
        Vector3 pos = camPos + camForward * 2;
        pos.y += 1;
        transform.position = pos;
        //transform.LookAt(camPos, Vector3.up);
        transform.up = Vector3.up;
        robotSound.PlaySound("ShowUpSound");
    }

    bool isMovingAnimation;
    public void MoveForwrad(float ratio = 1)
    {
        if (!isMovingAnimation)
        {
            isMovingAnimation = true;
            ani.SetTrigger("Move");
            robotSound.PlaySoundLoop("FlyingSound", 1);
        }
        Vector3 force = Vector3.forward * moveForce * ratio * Time.deltaTime;
        rb.AddRelativeForce(force, ForceMode.Force);
    }

    public void MoveBack()
    {
        if (!isMovingAnimation)
        {
            isMovingAnimation = true;
            //ani.SetTrigger("Move");
            robotSound.PlaySoundLoop("FlyingSound", 1);
        }
        Vector3 force = -Vector3.forward * moveForce * Time.deltaTime;
        rb.AddRelativeForce(force, ForceMode.Force);
    }

    public void TurnLeft()
    {
        Vector3 force = Vector3.up * turnForce * Time.deltaTime;
        rb.AddRelativeTorque(force, ForceMode.Force);
    }

    public void TurnRight()
    {
        Vector3 force = -Vector3.up * turnForce * Time.deltaTime;
        rb.AddRelativeTorque(force, ForceMode.Force);
    }

    public void Stop()
    {
        isMovingAnimation = false;
        ani.SetTrigger("Idle");
        robotSound.StopSound();

        //  rb.DOLookAt(transform.forward, 0.5f, AxisConstraint.None, Vector3.up);
    }
    public GameObject shieldEffectPrefab;
    GameObject shieldEffectObj;
    private void OnCollisionStay(Collision collision)
    {
        if (shieldEffectObj != null)
            return;
        shieldEffectObj = Instantiate(shieldEffectPrefab);
        Transform body;
        MyHelpNode.FindTransform(transform, "ball_robot", out body);
        shieldEffectObj.transform.parent = body;
        shieldEffectObj.transform.localPosition = Vector3.zero;
        shieldEffectObj.transform.localScale = Vector3.one * Random.Range(0.7f, 0.8f);
        shieldEffectObj.transform.right = (collision.contacts[0].point - shieldEffectObj.transform.position).normalized;

        shieldEffectObj.GetComponentInChildren<Renderer>().material.renderQueue = 3010;

        ARRender.Instance.AddUnityrenderWithDepth(shieldEffectObj.transform);
        StartCoroutine(destroyshieldEffectObj(shieldEffectObj));
    }

    IEnumerator destroyshieldEffectObj(GameObject obj)
    {
        yield return new WaitForSeconds(shieldEffectTime);
        ARRender.Instance.RemoveUnityrenderWithDepth(shieldEffectObj.transform);
        Destroy(obj);
    }

    public Transform GetDiscoLightRoot()
    {
        return discoLightRoot;
    }
}
