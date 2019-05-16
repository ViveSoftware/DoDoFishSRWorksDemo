using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBallCombo : MonoBehaviour
{
	private int _curCombo = 0;
    [SerializeField] private PaintBallScoreUnit text;
    [SerializeField] private PaintBallScoreUnit[] units;

    public void ShowUI(bool show)
    {
        foreach (var unit in units)
        {
            unit.render.enabled = show;
        }
        text.render.enabled = show;
    }

    public void AddCombo(int value)
    {
        for (int i = 0; i < units.Length; i++)
        {
            int preValue = (_curCombo / (int)Mathf.Pow(10, i)) % 10;
            int newValue = ((_curCombo + value) / (int)Mathf.Pow(10, i)) % 10;

            if (preValue != newValue)
            {
                units[i].UpdateText(newValue);
            }
        }

        _curCombo += value;
    }

    public void UpdateCombo(int value)
    {
        for (int i = 0; i < units.Length; i++)
        {
            int preValue = (_curCombo / (int)Mathf.Pow(10, i)) % 10;
            int newValue = ((value) / (int)Mathf.Pow(10, i)) % 10;

            if (preValue != newValue)
            {
                units[i].UpdateText(newValue);
            }
        }

        _curCombo = value;
    }
}
