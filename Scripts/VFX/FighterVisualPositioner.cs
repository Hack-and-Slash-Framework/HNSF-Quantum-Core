using System.Collections.Generic;
using Quantum;
using UnityEngine;

public class FighterVisualPositioner : MonoBehaviour
{
    [System.Serializable]
    public class TagBoneDefinition
    {
        public string name;
        public AssetRef<Tag> tag;
        public GameObject bone;
    }

    public Dictionary<AssetRef<Tag>, GameObject> boneMap = new();
    
    public Transform[] visualPositions;

    public TagBoneDefinition[] tagToBones;

    public GameObject GetBone(AssetRef<Tag> tag)
    {
        if(boneMap.Count == 0) BuildBoneMap();
        return boneMap.GetValueOrDefault(tag);
    }

    public void BuildBoneMap()
    {
        boneMap.Clear();
        foreach (var tb in tagToBones)
        {
            boneMap.Add(tb.tag, tb.bone);
        }
    }
    
    public Transform GetClosestVisualTransform(Vector3 position)
    {
        if (visualPositions.Length == 0) return null;

        int closestTransform = 0;
        float closestPosition = float.MaxValue;
        
        for (int i = 0; i < visualPositions.Length; i++)
        {
            var dist = Vector3.Distance(position, visualPositions[i].position);
            if (!(dist < closestPosition)) continue;
            closestTransform = i;
            closestPosition = dist;
        }
        
        return visualPositions[closestTransform];
    }
    
    public Vector3 GetClosestVisualPosition(Vector3 position)
    {
        if (visualPositions.Length == 0) return transform.position;

        int closestTransform = 0;
        float closestPosition = float.MaxValue;
        
        for (int i = 0; i < visualPositions.Length; i++)
        {
            var dist = Vector3.Distance(position, visualPositions[i].position);
            if (!(dist < closestPosition)) continue;
            closestTransform = i;
            closestPosition = dist;
        }
        
        return visualPositions[closestTransform].position;
    }
    
    public Vector3 GetClosestVisualPositionNoXZ(Vector3 position)
    {
        if (visualPositions.Length == 0)
        {
            var trSelf = transform.position;
            trSelf.z = 0;
            return trSelf;
        }

        int closestTransform = 0;
        float closestPosition = float.MaxValue;
        
        for (int i = 0; i < visualPositions.Length; i++)
        {
            var dist = Vector3.Distance(position, visualPositions[i].position);
            if (!(dist < closestPosition)) continue;
            closestTransform = i;
            closestPosition = dist;
        }

        var tr = visualPositions[closestTransform].position;
        tr.x = transform.position.x;
        tr.z = transform.position.z;
        return tr;
    }
    
    public Vector3 GetClosestVisualPositionByYNoXZ(Vector3 position)
    {
        if (visualPositions.Length == 0)
        {
            var trSelf = transform.position;
            trSelf.z = 0;
            return trSelf;
        }

        int closestTransform = 0;
        float closestPosition = float.MaxValue;
        
        for (int i = 0; i < visualPositions.Length; i++)
        {
            var dist = Mathf.Abs(position.y - visualPositions[i].position.y);
            if (!(dist < closestPosition)) continue;
            closestTransform = i;
            closestPosition = dist;
        }

        var tr = visualPositions[closestTransform].position;
        tr.z = transform.position.z;
        tr.x = transform.position.x;
        return tr;
    }
}
