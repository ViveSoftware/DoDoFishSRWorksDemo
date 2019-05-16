using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHeartController : MonoBehaviour
{
    public int CurrentAliveHeart { get { return curAliveHeart; } }
    private int curAliveHeart = 0;
    private Queue<PlayerHeart> rentingQueue;

    [SerializeField] private PlayerHeart[] units;
    [SerializeField] private Transform leftRoot;
    [SerializeField] private Transform rightRoot;
    private AudioSource[] audioSources;
    [SerializeField] private AudioClip[] HeartClip;

    public float curWidth { get { return _width; } }
    public float curHeight { get { return _height; } }
    private float _width = 0.649f, _height = 0.2f;

    // Use this for initialization
    private void Start ()
    {
        audioSources = GetComponents<AudioSource>();
        rentingQueue = new Queue<PlayerHeart>();

        foreach (var unit in units)
        {
            unit.AddOnBrokenListener((OnHeartBroken));
            ReturnHeart(unit);
        }
    }

    private void OnDisable()
    {
        foreach (var unit in units)
        {
            unit.RemoveOnBrokenListener(OnHeartBroken);
        }
    }

    public void ShowHeartWithoutCollison()
    {
        foreach (var unit in units)
        {
            unit.ShowHeartWithoutCollison();
        }
    }

    public void OnHideEvent()
    {
        foreach (var unit in units)
        {
            unit.HidePlayerHeart();
        }
    }

    public void OnResetEvent()
    {
        rentingQueue.Clear();
        foreach (var unit in units)
        {
            unit.ResetPlayerHeart();
            ReturnHeart(unit);
        }
        curAliveHeart = units.Length;
    }

    public void OnHeartBroken()
    {
        curAliveHeart--;

        if (!audioSources[0].isPlaying)
        {
            audioSources[0].clip = HeartClip[Random.Range(0, HeartClip.Length)];
            audioSources[0].volume = Mathf.Lerp(0.5f, 1, (8 - curAliveHeart) / 8.0f);
            audioSources[0].Play();
        }
        else if (!audioSources[1].isPlaying)
        {
            audioSources[1].clip = HeartClip[Random.Range(0, HeartClip.Length)];
            audioSources[1].volume = Mathf.Lerp(0.5f, 1, (8 - curAliveHeart) / 8.0f);
            audioSources[1].Play();
        }
    }

    public void ReturnHeart(PlayerHeart heart)
    {
        if(heart.curState == PlayerHeart.state.idel)
            rentingQueue.Enqueue(heart);
    }

    public PlayerHeart GetPathTarget()
    {
        if (rentingQueue.Count > 0)
        {
            return rentingQueue.Dequeue();
        }
        else
        {
            return null;
        }
    }

    public void ChangeHeartLineInterval(float height, float width)
    {
        leftRoot.localPosition = new Vector3(0, 0, -width);
        rightRoot.localPosition = new Vector3(0, 0, width);

        for (int i = 0; i < 4; i++)
        {
            units[i].transform.localPosition = Vector3.up * (height * (1.5f - i));
            units[4 + i].transform.localPosition = Vector3.up * (height * (1.5f - i));
        }

        _height = height;
        _width = width;
    }
}
