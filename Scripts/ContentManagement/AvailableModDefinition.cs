using System;
using HnSF;

[System.Serializable]
public class AvailableModDefinition
{
    [NonSerialized] public bool canUnload = true;
    public string guid;
    public string identifier;
    public string author;
    public string name;
    public string version;
    public ModOnlineRequirement OnlineRequirement;
    public int loader = (int)KnownModLoaderTypes.UMOD;
    public string path;
    public bool requiresReload;
    [NonSerialized] public ModLifecycleState currentLifecycle;
    [NonSerialized] public LoadedModDefinition loadedDefinition;

    public Guid GetGuid()
    {
        return new Guid(guid);
    }
}
