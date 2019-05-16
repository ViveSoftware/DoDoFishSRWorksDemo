using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDoorOpen : MonoBehaviour
{
    public Transform racingCar;
    public CarAudio carAudio;
    private void OnCollisionEnter(Collision collision)
    {
        CarDoorSwitch();
    }

    void CarDoorSwitch()
    {
        if (racingCar == null)
            MyHelpNode.FindTransform(transform, "racingCar", out racingCar);
        Animator carAnimator = racingCar.GetComponent<Animator>();
        carAnimator.enabled = true;
        bool isopen = carAnimator.GetBool("open");
        isopen = !isopen;
        carAnimator.SetBool("open", isopen);
        carAudio.PlaySound(
            (isopen) ? "CarDoorOpen" : "CarDoorClose", 1, 0,
            (isopen) ? 0.3f : 1.0f);
    }
}
