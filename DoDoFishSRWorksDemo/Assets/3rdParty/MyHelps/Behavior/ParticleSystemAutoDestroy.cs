using UnityEngine;
using System.Collections;

/// <summary>
/// 控制此game object底下的particle全部撥完後會自動destroy
/// http://answers.unity3d.com/questions/219609/auto-destroying-particle-system.html
/// </summary>
public class ParticleSystemAutoDestroy : MonoBehaviour
{
    private ParticleSystem[] pss;

    [Tooltip("是否在結束後將particle destroy(false為使用DeActive)")]
    public bool UseDestroy = true;

    public void Start()
    {
        pss = GetComponentsInChildren<ParticleSystem>();
    }

    public void Update()
    {
        bool canDestroy = true;
        foreach (ParticleSystem ps in pss)
        {
            if (ps.IsAlive())
            {
                canDestroy = false;
                break;
            }
        }

        if (canDestroy)
        {
            if (UseDestroy)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}