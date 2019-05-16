using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public enum mode
    {
        record,
        read
    }
    public mode curMode = mode.record;
    public string levelName = "level_1";
    public bool levelFinish { get { return commandInx == commands.Count; } }

    private string filePath;    

    [SerializeField] private bool isRecording = false;
    [SerializeField] private bool recordInit = false;
    private bool isPlaying = false;
    private float curTime;
    private List<CommandInfo> commands = null;
    private int commandInx = 0;

    private static CommandManager manager = null;
    public static CommandManager instance
    {
        get
        {
            if (manager == null)
            {
                manager = FindObjectOfType(typeof(CommandManager)) as CommandManager;
                if (manager == null)
                {
                    Debug.LogError("[CommandManager::Singleton] There should be one active gameobject attached UndoManager in scene");
                }
            }
            return manager;
        }
    }

    // Use this for initialization
    void Start ()
    {
        filePath = Application.dataPath + "/StreamingAssets/" + levelName + ".xml";
        if (curMode == mode.read)
        {
            LoadLevelInfo(filePath);
            curTime = Time.timeSinceLevelLoad;
        }
    }

    private void Update()
    {
        switch (curMode)
        {
            case mode.record:
                RecordKeyBoardCommand();
                break;
            case mode.read:
                ReadKeyBoardCommand();
                break;
        }
    }

    public void StartLevel()
    {
        isPlaying = true;
        commandInx = 0;
        curTime = Time.timeSinceLevelLoad;
    }

    public void EndLevel()
    {
        isPlaying = false;
    }

    private void ReadKeyBoardCommand()
    {
        if (!(commandInx < commands.Count) || !isPlaying)
        {
            return;
        }
        while ((Time.timeSinceLevelLoad - curTime) > -(commands[commandInx].time + 2))
        {
            MRWallManager.instance.CheckKeyDown(commands[commandInx].command);
            commandInx++;

            if (commandInx == commands.Count)
            {
                isPlaying = false;
                break;
            }
        }
    }

    private void RecordKeyBoardCommand()
    {
        if (isRecording)
        {
            if (Input.anyKeyDown && recordInit)
            {
                CommandInfo info = new CommandInfo()
                {
                    time = curTime - Time.timeSinceLevelLoad,
                    command = Input.inputString
                };
                commands.Add(info);
            }
            else if (!recordInit)
            {
                recordInit = true;
                commands = new List<CommandInfo>();
                curTime = Time.timeSinceLevelLoad;
            }
        }
        else if(!isRecording && recordInit)
        {
            StreamWriter stream = new StreamWriter(filePath);
            var bformatter = new System.Xml.Serialization.XmlSerializer(typeof(List<CommandInfo>));
            bformatter.Serialize(stream, commands);
            stream.Close();
        }
    }

    public void LoadLevelInfo(string filePath)
    {
        if (File.Exists(filePath))
        {
            commands = new List<CommandInfo>();
            FileStream stream = new FileStream(filePath, FileMode.Open);
            var bformatter = new System.Xml.Serialization.XmlSerializer(typeof(List<CommandInfo>));
            commands = (List<CommandInfo>)bformatter.Deserialize(stream);
            stream.Close();
        }
        else
        {
            Debug.Log("Can't read level file");
        }
    }

    public class CommandInfo
    {
        public float time;
        public string command;
    }
}
