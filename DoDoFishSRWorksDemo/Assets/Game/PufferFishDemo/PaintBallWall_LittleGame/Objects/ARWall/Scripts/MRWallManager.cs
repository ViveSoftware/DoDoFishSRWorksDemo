using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRWallManager : MonoBehaviour
{
    public enum Size
    {
        _3X2,
        _7X4
    }
    [SerializeField] private Size size = Size._3X2;
    public int row = 4, col = 7;
    public PlayerHeartController heartController;
    [SerializeField] private GameObject ARWall;
    [SerializeField] private GameObject WallPrefab;
    [SerializeField] private GameObject InnerBox;
    [SerializeField] private GameObject OuterBox;
    [SerializeField] private GameObject GameOverVideo;
    [SerializeField] private Camera GameOverCamera;

    [SerializeField] private PaintVR.GBufferGenerator generator;
    [SerializeField] private PaintVR.PBSPaintableObject wall;
    [SerializeField] private MRWallPaintingClear cleaner;

    public GameObject GameStartButton;
    public bool gameStartFlag = false;
    public float fishSpeed = 5f;
    private PaintVR.PBSPaintableObject startButton;

    private MRWallUnit model;
    private PaintVR.PBSPaintableObjectRoot PBSRoot = null;
    private PaintVR.PBSPaintableObject[] paintObjs = null;

    public bool initStatus { get { return init; } }
    private bool init = false;

    private List<MRWallUnit> units = null;
    private string activeUnit = "ertdfg";
    private string pareToUnit = "1234567qwertyuasdfghjzxcvbnm";

    private static MRWallManager manager = null;
    public static MRWallManager instance
    {
        get
        {
            if (manager == null)
            {
                manager = FindObjectOfType(typeof(MRWallManager)) as MRWallManager;
                if (manager == null)
                {
                    Debug.LogError("[UndoManager::Singleton] There should be one active gameobject attached UndoManager in scene");
                }
            }
            return manager;
        }
    }

    public void ChangeFishSpeed(float speed)
    {
        fishSpeed = speed;
        foreach (var unit in units)
        {
            unit.fishSpeed = speed;
        }
    }

    public bool CheckUnitStatus()
    {
        bool isPlaying = false;
        foreach (var unit in units)
        {
            if (unit.IsPlaying)
            {
                isPlaying = true;
                break;
            }
        }
        return isPlaying;
    }

    public void PlayGameOverVideo()
    {
        StartCoroutine(PlayGameOverVideoCoroutine());
    }

    private IEnumerator PlayGameOverVideoCoroutine()
    {
        ScoreManager.instance.HideScoreBoard();
        GameOverVideoPlayer.instance.StartPlay();

        for (int j = 0; j < row; j++)
        {
            for (int i = 0; i < col; i++)
            {
                units[j * col + i].PlayVideoMode();
            }
            yield return new WaitForSeconds(0.5f);
        }

    }

    void Start()
    {
        InitGbuff();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.anyKeyDown)
        {
            CheckKeyDown();
        }*/
        if (Input.GetKeyUp(KeyCode.P))
        {
            foreach (var unit in units)
            {
                unit.UpdateWorldPosMatrix();
            }
        }
    }

    public void UpdateWorldPosMatrix()
    {
        foreach (var unit in units)
        {
            unit.UpdateWorldPosMatrix();
        }
    }

    public void CheckKeyDown(string sr)
    {
        for (int i = 0; i < sr.Length; i++)
        {
            int inx = pareToUnit.IndexOf(sr[i]);
            if (inx == -1) continue;

            int actInx = Mathf.FloorToInt((float)inx / (float)pareToUnit.Length * (float)activeUnit.Length);
            //Debug.Log("[Inx:" + inx + "]" + "[ActInx:" + actInx + "]");
            inx = pareToUnit.IndexOf(activeUnit[actInx]);

            MRWallUnit unit = units[inx];
            if (unit.IsPlaying) continue;
            unit.RotateStart();
        }
    }

    public void CheckKeyDown()
    {
        string sr = Input.inputString;
        for (int i = 0; i < sr.Length; i++)
        {
            int inx = pareToUnit.IndexOf(sr[i]);
            if (inx == -1) continue;

            MRWallUnit unit = units[inx];
            if (unit.IsPlaying) continue;
            unit.RotateStart();
        }
    }

    public void TiggerStartEvent()
    {
        gameStartFlag = true;
    }

    private IEnumerator InitCoroutine()
    {
        GameObject tp = Instantiate(WallPrefab);
        model = tp.GetComponent<MRWallUnit>();
        paintObjs = model.paintObjs;

        startButton = GameStartButton.GetComponent<PaintVR.PBSPaintableObject>();
        startButton.AddOnPaintingListener(TiggerStartEvent);
        generator.targetObject = GameStartButton;
        generator.GBufferGeneratorInit();

        yield return new WaitForSeconds(0.1f);

        generator.targetObject = wall.gameObject;
        generator.GBufferGeneratorInit();

        yield return new WaitForSeconds(0.1f);

        foreach (var obj in paintObjs)
        {
            generator.targetObject = obj.gameObject;
            generator.GBufferGeneratorInit();

            yield return new WaitForSeconds(0.1f);
        }

        PBSRoot = model.coverMesh.gameObject.AddComponent<PaintVR.PBSPaintableObjectRoot>();
        PBSRoot.registerUndoMgr = false;

        Mesh mesh = model.coverMesh.mesh;
        Vector3[] vertexs = new Vector3[4];
        vertexs[0] = new Vector3(-col / 2.0f, -row / 2.0f, 0.0f);
        vertexs[1] = new Vector3(+col / 2.0f, +row / 2.0f, 0.0f);
        vertexs[2] = new Vector3(+col / 2.0f, -row / 2.0f, 0.0f);
        vertexs[3] = new Vector3(-col / 2.0f, +row / 2.0f, 0.0f);

        mesh.vertices = vertexs;
        model.coverMesh.mesh = mesh;

        generator.targetObject = model.coverMesh.gameObject;
        generator.GBufferGeneratorInit();

        cleaner.srcTexture = PBSRoot.PaintTextureData.AlbedoTexture;

        yield return new WaitForSeconds(0.1f);

        InitMRWall();

        yield return new WaitForSeconds(0.1f);

        model.coverMesh.GetComponent<Renderer>().enabled = false;
        model.coverMesh.GetComponent<Collider>().enabled = false;
        model.coverMesh.transform.parent = ARWall.transform;
        Destroy(model.gameObject);

        yield return new WaitForSeconds(0.1f);
        init = true;
    }

    private void InitGbuff()
    {
        StartCoroutine(InitCoroutine());
    }

    private void InitMRWall()
    {
        units = new List<MRWallUnit>();

        float initPosZ = (float)col / 2 - 0.5f;
        float initPosY = (float)row / 2 - 0.5f;
        for (int j = 0; j < row; j++)
        {
            for (int i = 0; i < col; i++)
            {
                GameObject wall = Instantiate(WallPrefab);
                wall.name = "unit_" + (j * col + i);
                wall.transform.parent = ARWall.transform;
                wall.transform.localPosition = new Vector3(0, initPosY - j, initPosZ - i);
                wall.transform.localEulerAngles = Vector3.zero;
                wall.transform.localScale = Vector3.one;

                MRWallUnit unit = wall.GetComponent<MRWallUnit>();
                for (int k = 0; k < unit.paintObjs.Length; k++)
                {
                    unit.paintObjs[k].PositionMap = model.paintObjs[k].PositionMap;
                    unit.paintObjs[k].NormalMap = model.paintObjs[k].NormalMap;
                }

                PaintVR.PBSPaintableMiniature subComponent = unit.coverMesh.gameObject.AddComponent<PaintVR.PBSPaintableMiniature>();
                subComponent.registerUndoMgr = false;
                subComponent.OriginObj = PBSRoot;

                Vector2 UVst = new Vector2((float)i / (float)col, (float)(row - j - 1) / (float)row);
                Vector2 UVed = new Vector2((float)(i + 1) / (float)col, (float)(row - j) / (float)row);
                unit.ChangeCoverMeshUV(UVst, UVed);

                Vector3 VertexSt = new Vector3(-col / 2.0f + i, -row / 2.0f + (row - j - 1), 0);
                Vector3 VertexEd = new Vector3(-col / 2.0f + (i + 1), -row / 2.0f + (row - j), 0);
                unit.ChangeCoverMeshVertexs(VertexSt, VertexEd);

                unit.fishSpeed = fishSpeed;
                unit.mgr = this;
                unit.UpdateWorldPosMatrix();
                units.Add(unit);

                if (ScoreManager.instance != null)
                {
                    ScoreManager.instance.AddCancelComboObj(unit.paintObjs[1]);
                    ScoreManager.instance.AddCancelComboObj(subComponent);
                }
            }
        }

        if (ScoreManager.instance != null)
            ScoreManager.instance.AddCancelComboObj(wall.GetComponent<PaintVR.PBSPaintableObject>());

        SetAcitveUnit();
    }

    public void ResetMRWall()
    {
        UpdateWorldPosMatrix();
        foreach (var unit in units)
        {
            unit.ResetUint();
            unit.paintObjs[0].PaintableReset();
        }
        PBSRoot.PaintableReset();
        wall.GetComponent<PaintVR.PBSPaintableObject>().PaintableReset();
        startButton.PaintableReset();
        gameStartFlag = false;
    }

    private void SetAcitveUnit()
    {
        switch (size)
        {
            case Size._3X2:
                activeUnit = "ertdfg";

                RenderTexture gameOverVideo = new RenderTexture(1500, 1000, 24, RenderTextureFormat.ARGB32);

                for (int j = 0; j < units.Count; j++)
                {
                    units[j].gameObject.SetActive(false);
                }
                for (int i = 0; i < activeUnit.Length; i++)
                {
                    int inx = pareToUnit.IndexOf(activeUnit[i]);
                    units[inx].gameObject.SetActive(true);
                    units[inx].displayMesh.GetComponent<Renderer>().material.SetTexture("_MainTex", gameOverVideo);
                }

                InnerBox.transform.localScale = new Vector3(1, 1.0f / 4.0f * 2.0f, 1.0f / 7.0f * 3.0f);
                OuterBox.transform.localScale = new Vector3(1, 1.0f / 4.0f * 2.0f, 1.0f / 7.0f * 3.0f);
                GameOverCamera.orthographicSize = 12;
                GameOverCamera.targetTexture = gameOverVideo;
                GameOverVideo.transform.localScale = new Vector3(1, 1.2f, 1);
                break;
            case Size._7X4:
                activeUnit = "1234567qwertyuasdfghjzxcvbnm";
                break;
        }
    }
}
