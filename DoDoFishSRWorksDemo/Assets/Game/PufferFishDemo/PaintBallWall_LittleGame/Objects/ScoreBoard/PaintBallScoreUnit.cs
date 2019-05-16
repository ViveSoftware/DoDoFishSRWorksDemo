using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PaintBallScoreUnit : MonoBehaviour
{
    public float minScale = 20, maxScale = 25;
    public float duration = 0.5f;
    public float ratio = 0.7f;

    public Renderer render;
    private IEnumerator TextAnimIEnumerator;
    private TextMeshPro textMesh;
    private float timer;
    // Use this for initialization

    void Start ()
    {
        textMesh = GetComponent<TextMeshPro>();
        render = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	public void UpdateText (int score)
    {
        textMesh.text = score.ToString();
        timer = 0;
        if (TextAnimIEnumerator == null)
        {
            TextAnimIEnumerator = TextAnimCoroutine();
            StartCoroutine(TextAnimIEnumerator);
        }
    }

    private IEnumerator TextAnimCoroutine()
    {
        float halfDuration = duration * ratio;
        while (timer < duration)
        {
            if (timer < halfDuration)
            {
                textMesh.fontSize = Mathf.Lerp(minScale, maxScale, timer / halfDuration);
            }
            else
            {
                textMesh.fontSize = Mathf.Lerp(maxScale, minScale, (timer - halfDuration) / (duration - halfDuration));
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        TextAnimIEnumerator = null;
    }
}
