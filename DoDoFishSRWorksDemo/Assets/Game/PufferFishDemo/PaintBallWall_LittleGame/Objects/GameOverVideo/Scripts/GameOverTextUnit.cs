using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverTextUnit : MonoBehaviour
{
    public enum mode
    {
        character,
        floating,
        number
    }
    public string characters;
    public int scores;
    public float floating;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private mode playMode = mode.character;
    private TextMeshPro textMesh;
    // Use this for initialization
    void Start ()
    {
        textMesh = GetComponent<TextMeshPro>();
        textMesh.text = "";
    }

    public void PlayTextAnim()
    {
        switch (playMode)
        {
            case mode.character:
                StartCoroutine(PlayCharacterCoroutine());
                break;
            case mode.number:
                StartCoroutine(PlayNumberCoroutine());
                break;
            case mode.floating:
                StartCoroutine(PlayFloatingCoroutine());
                break;
        }
    }

    public void OnResetEvent()
    {
        textMesh.text = "";
    }

    private IEnumerator PlayCharacterCoroutine()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            textMesh.text += characters[i];
            yield return new WaitForSeconds(duration / (float)characters.Length);
        }
    }

    private IEnumerator PlayFloatingCoroutine()
    {
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float value = Mathf.Lerp(0, floating, timer / duration);
            textMesh.text = value.ToString("F2");
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator PlayNumberCoroutine()
    {
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float value = Mathf.Lerp(0, scores, timer / duration);
            textMesh.text = ((int)value).ToString();
            yield return new WaitForEndOfFrame();
        }
    }
}
