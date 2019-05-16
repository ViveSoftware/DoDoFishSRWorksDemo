using Demo;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CoordinateData
{
    public Vector3 Pos;
    public Vector3 Rot;
    public Vector3 Scl;
}

public class ConfigData
{
    public CoordinateData MRWall;
    public CoordinateData Heart;
    public CoordinateData trackerOfMRWall;
    public CoordinateData trackerOfHealth;

    public float intervalOfHealthWidth, intervalOfHealthHeight;
    public float fishSpeed;
}

public class RoomSettingManager : MonoBehaviour
{
    [SerializeField] private Transform MRWall;
    [SerializeField] private PlayerHeartController Heart;
    [SerializeField] private Transform trackerOfMRWall;
    [SerializeField] private Transform trackerOfHealth;
    [SerializeField] private GameObject canvas;

    public string fileName = "RoomSetting";
    private string filePath;
    private ConfigData data;
    public bool StartSetting
    {
        get
        {
            return startSetting;
        }
        set
        {
            startSetting = value;
            stageHint.SetActive(value);
            canvas.SetActive(value);
        }
    }
    private bool startSetting;
    public GameObject stageHint;

    private static RoomSettingManager manager = null;
    public static RoomSettingManager instance
    {
        get
        {
            if (manager == null)
            {
                manager = FindObjectOfType(typeof(RoomSettingManager)) as RoomSettingManager;
                if (manager == null)
                {
                    Debug.LogError("[RoomSettingManager::Singleton] There should be one active gameobject attached UndoManager in scene");
                }
            }
            return manager;
        }
    }

    // Use this for initialization
    void Start()
    {
        filePath = Application.dataPath + "/StreamingAssets/" + fileName + ".xml";
    }

    // Update is called once per frame
    void Update()
    {
        if (startSetting)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                float value = MRWall.transform.localScale.x + Time.deltaTime * 0.5f;
                MRWall.transform.localScale = Vector3.one * value;
            }
            if (Input.GetKey(KeyCode.W))
            {
                float value = MRWall.transform.localScale.x - Time.deltaTime * 0.5f;
                MRWall.transform.localScale = Vector3.one * value;
            }
            if (Input.GetKey(KeyCode.A))
            {
                float value = Heart.curHeight + Time.deltaTime * 0.5f;
                Heart.ChangeHeartLineInterval(value, Heart.curWidth);
            }
            if (Input.GetKey(KeyCode.S))
            {
                float value = Heart.curHeight - Time.deltaTime * 0.5f;
                Heart.ChangeHeartLineInterval(value, Heart.curWidth);
            }
            if (Input.GetKey(KeyCode.Z))
            {
                float value = Heart.curWidth + Time.deltaTime * 0.5f;
                Heart.ChangeHeartLineInterval(Heart.curHeight, value);
            }
            if (Input.GetKey(KeyCode.X))
            {
                float value = Heart.curWidth - Time.deltaTime * 0.5f;
                Heart.ChangeHeartLineInterval(Heart.curHeight, value);
            }
            if (Input.GetKeyDown(KeyCode.U) && GameObject.FindObjectOfType<ARRender>() == null)
            {
                LoadConfigFile();
            }
            if (Input.GetKeyDown(KeyCode.I) && GameObject.FindObjectOfType<ARRender>() == null)
            {
                SaveConfigFile();
            }
        }
    }

    private void LoadConfigFile()
    {
        if (!CheckConfigFile()) return;

        MRWall.transform.localPosition = data.MRWall.Pos;
        MRWall.transform.localEulerAngles = data.MRWall.Rot;
        MRWall.transform.localScale = data.MRWall.Scl;

        Heart.transform.localPosition = data.Heart.Pos;
        Heart.transform.localEulerAngles = data.Heart.Rot;
        Heart.transform.localScale = data.Heart.Scl;

        Heart.ChangeHeartLineInterval(data.intervalOfHealthHeight, data.intervalOfHealthWidth);

        trackerOfMRWall.position = data.trackerOfMRWall.Pos;
        trackerOfMRWall.eulerAngles = data.trackerOfMRWall.Rot;
        trackerOfHealth.position = data.trackerOfHealth.Pos;
        trackerOfHealth.eulerAngles = data.trackerOfHealth.Rot;

        MRWallManager.instance.ChangeFishSpeed(data.fishSpeed);
        MRWallManager.instance.UpdateWorldPosMatrix();
    }

    private void SaveConfigFile()
    {
        CoordinateData MRWallCoordinate = new CoordinateData()
        {
            Pos = MRWall.localPosition,
            Rot = MRWall.localEulerAngles,
            Scl = MRWall.localScale
        };

        CoordinateData HeartCoordinate = new CoordinateData()
        {
            Pos = Heart.transform.localPosition,
            Rot = Heart.transform.localEulerAngles,
            Scl = Heart.transform.localScale
        };

        CoordinateData trackerMRWallCoordinate = new CoordinateData()
        {
            Pos = trackerOfMRWall.localPosition,
            Rot = trackerOfMRWall.localEulerAngles,
            Scl = trackerOfMRWall.localScale
        };

        CoordinateData trackerHealthCoordinate = new CoordinateData()
        {
            Pos = trackerOfHealth.localPosition,
            Rot = trackerOfHealth.localEulerAngles,
            Scl = trackerOfHealth.localScale
        };

        data = new ConfigData()
        {
            MRWall = MRWallCoordinate,
            Heart = HeartCoordinate,
            trackerOfHealth = trackerHealthCoordinate,
            trackerOfMRWall = trackerMRWallCoordinate,
            intervalOfHealthHeight = Heart.curHeight,
            intervalOfHealthWidth = Heart.curWidth,
            fishSpeed = MRWallManager.instance.fishSpeed
        };

        StreamWriter stream = new StreamWriter(filePath);
        var bformatter = new System.Xml.Serialization.XmlSerializer(typeof(ConfigData));
        bformatter.Serialize(stream, data);
        stream.Close();
    }

    private bool CheckConfigFile()
    {
        if (File.Exists(filePath))
        {
            data = new ConfigData();
            FileStream stream = new FileStream(filePath, FileMode.Open);
            var bformatter = new System.Xml.Serialization.XmlSerializer(typeof(ConfigData));
            data = (ConfigData)bformatter.Deserialize(stream);
            stream.Close();
            return true;
        }
        else
        {
            Debug.Log("Can't read room setting file");
            return false;
        }
    }
}
