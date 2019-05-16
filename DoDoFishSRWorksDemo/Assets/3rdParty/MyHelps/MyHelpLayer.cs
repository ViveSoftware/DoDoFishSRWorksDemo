using UnityEngine;
using System.Collections;

public static class MyHelpLayer
{
    public static bool IsMaskContainLayer(LayerMask layerMask, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogError("[MyHelpLayer] [IsContainLayer] project doesn't has layer : " + layerName);
            return false;
        }
        return (layerMask.value & 1 << layer) != 0;
    }

    public static LayerMask RemoveMaskLayer(LayerMask layerMask, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogError("[MyHelpLayer] [RemoveLayer] project doesn't has layer : " + layerName);
            return layerMask;
        }
        return RemoveMaskLayer(layerMask, layer);
    }

    public static LayerMask RemoveMaskLayer(LayerMask layerMask, int layer)
    {
        layerMask.value &= ~(1 << layer);
        return layerMask;
    }

    public static LayerMask InsertMaskLayer(LayerMask layerMask, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogError("[MyHelpLayer] [SetLayer] project doesn't has layer : " + layerName);
            return layerMask;
        }
        return InsertMaskLayer(layerMask, layer);
    }

    public static LayerMask InsertMaskLayer(LayerMask layerMask, int layer)
    {
        layerMask.value |= (1 << layer);
        return layerMask;
    }

    public static void SetSceneLayer(Transform root, int layer, bool onlySetDefaultLayer = false)
    {
        if (onlySetDefaultLayer && root.gameObject.layer == LayerMask.NameToLayer("Default"))
            root.gameObject.layer = layer;
        else if (!onlySetDefaultLayer)
            root.gameObject.layer = layer;

        for (int a = 0; a < root.childCount; a++)
        {
            SetSceneLayer(root.GetChild(a), layer, onlySetDefaultLayer);
        }
    }

    public static void ReplaceSceneLayer(Transform root, int origLayer, int destLayer)
    {
        if (root.gameObject.layer == origLayer)
            root.gameObject.layer = destLayer;

        for (int a = 0; a < root.childCount; a++)
        {
            ReplaceSceneLayer(root.GetChild(a), origLayer, destLayer);
        }
    }

    //public static void RemoveSceneLayer(Transform root, int layer)
    //{
    //    root.gameObject.layer &= ~layer;

    //    for (int a = 0; a < root.childCount; a++)
    //    {
    //        RemoveSceneLayer(root.GetChild(a), layer);
    //    }
    //}
}