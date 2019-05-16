using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public List<PaintVR.PBSPaintableObject> cancelComboObj;

    [SerializeField] private PaintBallScore scoreBoard;
    [SerializeField] private PaintBallCombo comboBoard;

    public int curScore { get { return scoreBoard._curScore; } }
    public float curHitRate
    {
        get
        {
            if (totalShot == 0)
            {
                return 0.0f;
            }
            else
            {
                return (float)hitShot / (float)totalShot * 100f;
            }
        }
    }

    private int _curCombo;
    private int totalShot, hitShot;

    private static ScoreManager scoreManager;
    public static ScoreManager instance
    {
        get
        {
            if (scoreManager == null)
            {
                scoreManager = FindObjectOfType(typeof(ScoreManager)) as ScoreManager;
                if (scoreManager == null)
                {
                    Debug.LogWarning("[UndoManager::Singleton] There should be one active gameobject attached UndoManager in scene");
                }
            }
            return scoreManager;
        }
    }

    private void Start()
    {
        _curCombo = 0;
        cancelComboObj = new List<PaintVR.PBSPaintableObject>();
    }

    private void OnDestroy()
    {
        foreach (var obj in cancelComboObj)
        {
            obj.RemoveOnPaintingListener(CancelCombo);
        }
    }

    public void AddScore(int value)
    {
        scoreBoard.AddScore(value * ((_curCombo / 5) + 1));
        AddCombo(1);
    }

    public void AddCombo(int value)
    {
        _curCombo++;
        comboBoard.AddCombo(value);
        ShotEvent(true);
    }

    public void CancelCombo()
    {
        _curCombo -= 2;
        _curCombo = (_curCombo < 0) ? 0 : _curCombo;
        comboBoard.UpdateCombo(_curCombo);
        ShotEvent(false);
    }

    public void ShotEvent(bool valid)
    {
        totalShot++;
        if (valid) hitShot++;
    }

    public void AddCancelComboObj(PaintVR.PBSPaintableObject obj)
    {
        obj.AddOnPaintingListener(CancelCombo);
        cancelComboObj.Add(obj);
    }

    public void HideScoreBoard()
    {
        scoreBoard.ShowUI(false);
        comboBoard.ShowUI(false);
    }

    public void EnableScoreBoard()
    {
        scoreBoard.ShowUI(true);
        comboBoard.ShowUI(false);
    }

    public void OnResetEvent()
    {
        scoreBoard.UpdateScore(0);
        comboBoard.UpdateCombo(0);
        _curCombo = 0;
        totalShot = 0;
        hitShot = 0;
        EnableScoreBoard();
    }
}
