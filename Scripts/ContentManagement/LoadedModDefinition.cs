using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LoadedModDefinition
{
    [NonSerialized] public AvailableModDefinition information;
    public BaseModInfoAsset modAsset;

    public bool HasValidModAsset()
    {
        return modAsset != null;
    }
}
