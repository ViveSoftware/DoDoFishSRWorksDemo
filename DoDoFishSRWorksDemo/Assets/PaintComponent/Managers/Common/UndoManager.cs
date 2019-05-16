using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public class UndoManager : MonoBehaviour
    {
        public class ActionRecord
        {
            public string paintObjectID = "";
            public RenderTexture texMap = null;
            public int actionId = 0;

            public void UpdateRecord(string paintId, RenderTexture rt)
            {
                paintObjectID = paintId;
                Graphics.Blit(rt, texMap);
                actionId = 0;
            }

            public void UpdateRecord(ActionRecord record)
            {
                paintObjectID = record.paintObjectID;
                Graphics.Blit(record.texMap, texMap);
                actionId = record.actionId;
            }
        }

        public Dictionary<string, PBSPaintableObject> PaintableObjs
        {
            get { return paintableObjs; }
        }

        private const int NumRecords = 6;
        private const int DefaultTexSize = 2048;

        // Private Serialized Class Data.
        // ---------------------------------------------------------------
        [SerializeField] private MeshRenderer[] debugQuads = null;

        // Private Class Data.
        // ---------------------------------------------------------------
        private static UndoManager undoManager = null;
        private ActionRecord[] recordList = null;
        private ActionRecord currentAction = new ActionRecord();
        private Dictionary<string, PBSPaintableObject> paintableObjs = null;
        private int currentPointer = -1;

        // C# Properties for Public Access.
        // ---------------------------------------------------------------
        public static UndoManager instance
        {
            get
            {
                if (undoManager == null)
                {
                    undoManager = FindObjectOfType(typeof(UndoManager)) as UndoManager;
                    if (undoManager == null)
                    {
                        Debug.LogError("[UndoManager::Singleton] There should be one active gameobject attached UndoManager in scene");
                    }
                }
                return undoManager;
            }
        }

        // Unity Fixed Methods.
        // ---------------------------------------------------------------
        void Awake()
        {
            recordList = new ActionRecord[NumRecords];
            for (int index = 0 ; index < NumRecords ; ++index)
            {
                recordList[index] = new ActionRecord();
                recordList[index].texMap = new RenderTexture(DefaultTexSize, DefaultTexSize, 0, RenderTextureFormat.ARGB32);
            }

            if (debugQuads != null)
            {
                for (int index = 0 ; index < debugQuads.Length ; ++index)
                {
                    debugQuads[index].material.SetTexture("_MainTex", recordList[index].texMap);
                }
            }

            currentAction.texMap = new RenderTexture(DefaultTexSize, DefaultTexSize, 0, RenderTextureFormat.ARGB32);
        }

        void OnDestroy()
        {
            for (int index = 0 ; index < NumRecords ; ++index)
            {
                recordList[index] = null;
            }
            recordList = null;
        }

        // Public Class Methods.
        // ---------------------------------------------------------------
        public bool IsPaintOnDifferentObject(string id)
        {
            return (currentPointer < 0) || (id != recordList[currentPointer].paintObjectID);
        }

        public void InsertCurrentRecord(string paintId, RenderTexture rt)
        {
            currentAction.UpdateRecord(paintId, rt);

            currentPointer++;
            if (currentPointer < NumRecords)
            {
                // Still has space to insert.
                recordList[currentPointer].UpdateRecord(currentAction);
                // Debug.Log("[UndoManager::InsertRecord] Insert action record at pointer " + currentPointer);
            }
            else
            {
                currentPointer = NumRecords - 1;
                // No space available, move all records.
                for (int index = 1 ; index < NumRecords ; ++index)
                {
                    recordList[index - 1].UpdateRecord(recordList[index]);
                }
                recordList[currentPointer].UpdateRecord(currentAction);
                // Debug.Log("[UndoManager::InsertRecord] Insert action record at pointer " + currentPointer);
            }
        }

        public bool PerformUndo()
        {
            if (currentPointer <= 0)
            {
                return false;
            }
                
            int pointer = Mathf.Max(0, currentPointer - 1);
            // Debug.Log("[UndoManager::InsertRecord] Load record at pointer " + currentPointer);
            string id = recordList[pointer].paintObjectID;
            if (paintableObjs.ContainsKey(id))
            {
                currentPointer = pointer;
                PaintableObject entry = paintableObjs[id];
                if (entry == null)
                {
                    return false;
                }
                Graphics.Blit(recordList[currentPointer].texMap, entry.PaintTextureData.AlbedoTexture);
                return true;
            }
            return false;
        }

        public bool PerformRedo()
        {
            if (currentPointer >= NumRecords - 1)
            {
                return false;
            }

            int pointer = Mathf.Min(currentPointer + 1, NumRecords - 1);
            // Debug.Log("[UndoManager::InsertRecord] Load record at pointer " + currentPointer);
            string id = recordList[pointer].paintObjectID;
            if (paintableObjs.ContainsKey(id))
            {
                currentPointer = pointer;
                PaintableObject entry = paintableObjs[id];
                if (entry == null)
                {
                    return false;
                }
                Graphics.Blit(recordList[currentPointer].texMap, entry.PaintTextureData.AlbedoTexture);
                return true;
            }
            return false;
        }

        public void ClearActionRecords()
        {
            currentPointer = -1;
        }

        public void RegisterPaintableObject(PBSPaintableObject paintableObject)
        {
            if (paintableObjs == null)
            {
                paintableObjs = new Dictionary<string, PBSPaintableObject>();
            }
            paintableObjs.Add(paintableObject.paintObjectID, paintableObject);
        }

        public void UnRegisterPaintableObject(PBSPaintableObject paintableObject)
        {
            paintableObjs.Remove(paintableObject.paintObjectID);
        }
    }
}