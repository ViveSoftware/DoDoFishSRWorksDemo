using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageController : MonoBehaviour
{
    public Material GateMRMat, GateThicknessMat;
    public MeshRenderer[] GetThicknessMesh;

    public float LiftMachineSpeed = 5f;
    public float GarageOpenTime = 3f;
    public Transform Platform;

    public bool IsReady { get { return isAct & isOpening; } }

    private Animator animator;
    private float liftMachineTimer = 0f, garageTimer = 0f;
    private bool isAct = false, isOpening = false, openDone, closeDone = true;
    public bool IsCloseDone() { return closeDone; }
    public bool IsOpenDone() { return openDone; }

    private IEnumerator OpenCoroutine = null;
    private IEnumerator CloseCoroutine = null;
    private const float soundVolume = 0.5f;
    CarAudio carAudio;
    LiftMachineAudio liftMachineAudio;
    Transform racingCar;
    float origGarageOpenTime;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        MyHelpNode.FindTransform(transform, "racingCar", out racingCar);
        carAudio = racingCar.GetComponent<CarAudio>();

        liftMachineAudio = GetComponent<LiftMachineAudio>();
        origGarageOpenTime = GarageOpenTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            transform.localPosition += Vector3.forward * 0.01f;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            transform.localPosition -= Vector3.forward * 0.01f;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GarageSwitch();
        }
        if (Input.GetKey(KeyCode.X))
        {
            LiftMachineRaiseUp();
        }
        if (Input.GetKey(KeyCode.C))
        {
            LiftMachineFallDown();
        }
        if (Input.GetKey(KeyCode.V))
        {
            GarageTurnLeft();
        }
        if (Input.GetKey(KeyCode.B))
        {
            GarageTurnRight();
        }
    }

    public float GarageUpEndFade, GarageUpEndTime, GarageUppingFade, GarageUppingTime, GarageUpStart2Time, GarageUpStart22Time;
    public float GarageDownEndFade, GarageDownEndTime, GarageDownEndFade2, GarageDownEndTime2, GarageDowningFade, GarageDowningTime;
    public bool GarageSwitch()
    {
        if (isOpening && !openDone)
            return true;
        if (!isOpening && !closeDone)
            return true;

        if (!isOpening)
        {
            openDone = false;
            isOpening = true;
            GarageOpen();

            liftMachineAudio.PlaySound("GarageUpStart", soundVolume, 0.0f, 0);
            liftMachineAudio.PlaySound("GarageUpStart2", soundVolume * 0.5f, 0.1f, GarageUpStart2Time, 1.2f);//ground open
            liftMachineAudio.PlaySound("GarageUpStart2", soundVolume, 0.0f, GarageUpStart22Time);
            liftMachineAudio.PlaySound("GarageUpping", soundVolume, GarageUppingFade, GarageUppingTime);
            liftMachineAudio.PlaySound("GarageUpEnd", soundVolume, GarageUpEndFade, GarageUpEndTime);
        }
        else
        {   
            isOpening = false;
            GarageClose();

            liftMachineAudio.PlaySound("GarageUpping", soundVolume, 0, 0);
            liftMachineAudio.PlaySound("GarageUpEnd", soundVolume, GarageDowningFade, GarageDowningTime);
            liftMachineAudio.PlaySound("GarageUpStart2", soundVolume, GarageDownEndFade, GarageDownEndTime);
            liftMachineAudio.PlaySound("GarageUpStart2", soundVolume * 0.5f, GarageDownEndFade2, GarageDownEndTime2, 1.2f);//ground close
        }
        return isOpening;
    }

    const float GateCoverOpenRatio = 0.13f;
    public void GarageCloseAgain()
    {
        //Mathf.Lerp(GateClosePosition, GateOpenPosition, garageTimer / GarageOpenTime)
        GateClosePosition = 0;
        GateOpenPosition = GateCoverOpenRatio;
        closeDone = false;
        isOpening = false;
        GarageOpenTime = 1;
        garageTimer = GarageOpenTime;
        GarageClose();
    }

    public void GarageOpenSmall()
    {
        GateClosePosition = 0;
        GateOpenPosition = GateCoverOpenRatio;
        openDone = false;
        isOpening = true;
        GarageOpenTime = 1;
        garageTimer = 0;
        GarageOpen();
    }

    public void GarageTurnLeft()
    {
        if (!isAct) return;
        Vector3 rot = Platform.localEulerAngles;
        float rotZ = rot.z - Time.deltaTime * 30f;
        Platform.localEulerAngles = new Vector3(rot.x, rot.y, rotZ);

        if (!isLiftMachineSoundPlaying)
        {
            isLiftMachineSoundPlaying = true;
            LiftMachineSoundAlreadyStop = false;
            liftMachineAudio.PlaySoundLoop("GarageUpping", soundVolume);
        }
    }

    public void GarageTurnRight()
    {
        if (!isAct) return;
        Vector3 rot = Platform.localEulerAngles;
        float rotZ = rot.z + Time.deltaTime * 30f;
        Platform.localEulerAngles = new Vector3(rot.x, rot.y, rotZ);

        if (!isLiftMachineSoundPlaying)
        {
            isLiftMachineSoundPlaying = true;
            LiftMachineSoundAlreadyStop = false;
            liftMachineAudio.PlaySoundLoop("GarageUpping", soundVolume);
        }
    }

    //Transform recParent;
    //Vector3 recLocalPos;
    //Quaternion recLocalRot;
    public void GarageOpen()
    {
        if (CloseCoroutine != null)
        {
            StopCoroutine(CloseCoroutine);
        }
        OpenCoroutine = GarageOpenCoroutine();
        StartCoroutine(OpenCoroutine);

        //change parent
        //recParent = transform.parent;
        //recLocalPos = transform.localPosition;
        //recLocalRot = transform.localRotation;
        //transform.parent = null;
        closeDone = true;//open from close
    }

    public void GarageClose()
    {
        closeDone = false;
        if (OpenCoroutine != null)
        {
            StopCoroutine(OpenCoroutine);
        }
        CloseCoroutine = GarageCloseCoroutine();
        StartCoroutine(CloseCoroutine);
        isAct = false;

        CarDoorClose();
        openDone = true;
    }

    bool isLiftMachineSoundPlaying, LiftMachineSoundAlreadyStop;
    public void LiftMachineStop()
    {
        if (isLiftMachineSoundPlaying)
        {
            isLiftMachineSoundPlaying = false;

            if (!LiftMachineSoundAlreadyStop)
            {
                LiftMachineSoundAlreadyStop = true;
                liftMachineAudio.PlaySound("GarageUpEnd", soundVolume);
            }
        }
    }

    public void LiftMachineRaiseUp()
    {
        if (!isAct) return;
        liftMachineTimer += Time.deltaTime;
        liftMachineTimer = (liftMachineTimer * LiftMachineSpeed > 0.9999f) ? 0.9999f / LiftMachineSpeed : liftMachineTimer;
        float normalizeTime = liftMachineTimer * LiftMachineSpeed;
        animator.Play("PillarRaise", 1, Mathf.Lerp(0, 1, normalizeTime));

        LiftMachineSoundPlaying(normalizeTime);
    }

    public void LiftMachineFallDown()
    {
        if (!isAct) return;
        liftMachineTimer -= Time.deltaTime;
        liftMachineTimer = (liftMachineTimer < 0) ? 0 : liftMachineTimer;
        float normalizeTime = liftMachineTimer * LiftMachineSpeed;
        animator.Play("PillarRaise", 1, Mathf.Lerp(0, 1, normalizeTime));

        LiftMachineSoundPlaying(normalizeTime);
    }

    void LiftMachineSoundPlaying(float normalizeTime)
    {
        if (!isLiftMachineSoundPlaying)
        {
            Debug.Log("normalizeTime : " + normalizeTime);
            isLiftMachineSoundPlaying = true;
            LiftMachineSoundAlreadyStop = false;
            if (0 >= normalizeTime || normalizeTime >= 0.99f)
                liftMachineAudio.PlaySound("GarageUpEnd", soundVolume);
            else
                liftMachineAudio.PlaySoundLoop("GarageUpping", soundVolume);
        }
        else
        {
            if ((0 >= normalizeTime || normalizeTime > 0.99f) && !LiftMachineSoundAlreadyStop)
            {
                LiftMachineSoundAlreadyStop = true;
                liftMachineAudio.PlaySound("GarageUpEnd", soundVolume);
            }
        }
    }

    public float GateClosePosition = 0, GateOpenPosition = 1;
    private IEnumerator GarageOpenCoroutine()
    {
        foreach (MeshRenderer gate in GetThicknessMesh)
            gate.material = GateThicknessMat;

        while (garageTimer < GarageOpenTime)
        {
            animator.Play("GarageOpen", 0, Mathf.Lerp(GateClosePosition, GateOpenPosition, garageTimer / GarageOpenTime));
            garageTimer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        garageTimer = GarageOpenTime;
        animator.Play("GarageOpen", 0, Mathf.Lerp(GateClosePosition, GateOpenPosition, garageTimer / GarageOpenTime));
        isAct = true;
        openDone = true;
        GarageOpenTime = origGarageOpenTime;
        //carAudio.PlaySound("EngineStart");
        //CarDoorOpen();
    }

    private IEnumerator GarageCloseCoroutine()
    {
        while (liftMachineTimer > 0)
        {
            liftMachineTimer -= Time.deltaTime;
            liftMachineTimer = (liftMachineTimer < 0) ? 0 : liftMachineTimer;
            animator.Play("PillarRaise", 1, Mathf.Lerp(0, 1, liftMachineTimer * LiftMachineSpeed));
            yield return new WaitForEndOfFrame();
        }

        Vector3 rot = Platform.localEulerAngles;
        float rotZ = ((rot.z % 360) < 0) ? rot.z % 360 + 360 : rot.z % 360;
        Platform.localEulerAngles = new Vector3(rot.x, rot.y, rotZ);

        float timer = 0, speed = 45f;
        float Zdegree = rotZ;
        while (Zdegree != 0)
        {
            timer += Time.deltaTime;

            Zdegree = (rotZ > 180) ? rotZ + timer * speed : rotZ - timer * speed;
            if (Zdegree > 360 || Zdegree < 0)
            {
                Zdegree = 0;
            }

            Platform.localEulerAngles = new Vector3(rot.x, rot.y, Zdegree);
            yield return new WaitForEndOfFrame();
        }

        while (garageTimer > 0)
        {
            animator.Play("GarageOpen", 0, Mathf.Lerp(GateClosePosition, GateOpenPosition, garageTimer / GarageOpenTime));
            garageTimer -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        garageTimer = 0;
        animator.Play("GarageOpen", 0, Mathf.Lerp(GateClosePosition, GateOpenPosition, garageTimer / GarageOpenTime));
        closeDone = true;
        GarageOpenTime = origGarageOpenTime;
        //change parent
        //transform.parent = recParent;
        //transform.localPosition = recLocalPos;
        //transform.localRotation = recLocalRot;

        foreach (MeshRenderer gate in GetThicknessMesh)
            gate.material = GateMRMat;
    }

    public void SetGateCoverOpenPose()
    {
        animator = GetComponent<Animator>();
        animator.Play("GarageOpen", 0, GateCoverOpenRatio);
    }

    //public void CarDoorOpen()
    //{
    //   
    //    Animator carAnimator = racingCar.GetComponent<Animator>();
    //    carAnimator.enabled = true;
    //    carAnimator.Play("open");
    //}

    void CarDoorClose()
    {
        if (racingCar == null)
            MyHelpNode.FindTransform(transform, "racingCar", out racingCar);
        Animator carAnimator = racingCar.GetComponent<Animator>();
        carAnimator.enabled = true;
        carAnimator.Play("close");

        carAudio.PlaySound("CarDoorClose", soundVolume, 0, 1.0f);
    }

    public void CarDoorSwitch()
    {
        if (racingCar == null)
            MyHelpNode.FindTransform(transform, "racingCar", out racingCar);
        Animator carAnimator = racingCar.GetComponent<Animator>();
        carAnimator.enabled = true;
        bool isopen = carAnimator.GetBool("open");
        isopen = !isopen;
        carAnimator.SetBool("open", isopen);
        carAudio.PlaySound(
            (isopen) ? "CarDoorOpen" : "CarDoorClose", soundVolume, 0,
            (isopen) ? 0.3f : 1.0f);
    }
}
