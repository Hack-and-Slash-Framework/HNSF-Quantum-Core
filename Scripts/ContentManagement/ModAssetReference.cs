using System;

[System.Serializable]
public class ModAssetReference
{
    [NonSerialized] public AvailableModDefinition modDefinition;
    public string assetID;
}
