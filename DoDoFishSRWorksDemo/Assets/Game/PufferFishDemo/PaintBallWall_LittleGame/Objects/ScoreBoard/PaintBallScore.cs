using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PaintBallScore : MonoBehaviour
{
    public int _curScore = 0;
    [SerializeField] private PaintBallScoreUnit[] units;


    // Use this for initialization
    void Start()
    {

    }

    public void ShowUI(bool show)
    {
        foreach (var unit in units)
        {
            unit.render.enabled = show; 
        }
    }
	
    public void AddScore(int value)
    {
        for (int i = 0; i < units.Length; i++)
        {
            int preValue = (_curScore / (int)Mathf.Pow(10, i)) % 10;
            int newValue = ((_curScore + value) / (int)Mathf.Pow(10, i)) % 10;

            if (preValue != newValue)
            {
                units[i].UpdateText(newValue);
            }
        }

        _curScore += value;
    }

    public void UpdateScore(int value)
    {
        for (int i = 0; i < units.Length; i++)
        {
            units[i].UpdateText(0);
        }

        _curScore = value;
    }
}
