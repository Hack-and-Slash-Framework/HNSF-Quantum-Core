using System;

[System.Serializable]
public struct ModAssetSoftReference : IEquatable<ModAssetSoftReference>
{
    public string mod;
    public string assetID;
    public bool isFolder;

    public ModAssetSoftReference(string mod, string assetID, bool isFolder)
    {
        this.mod = mod;
        this.assetID = assetID;
        this.isFolder = isFolder;
    }
    
    public ModAssetSoftReference(string assetRef)
    {
        isFolder = false;
        if (string.IsNullOrEmpty(assetRef))
        {
            mod = string.Empty;
            assetID = string.Empty;
            return;
        }
        var splitAssetRef = assetRef.Split(':');
        this.mod = splitAssetRef[0];
        this.assetID = splitAssetRef[1];
    }
    
    public ModAssetSoftReference(IFighterDefinition definition)
    {
        mod = definition.modDefinition.modAsset.ModID;
        assetID = definition.ID;
        isFolder = false;
    }
    
    public override string ToString()
    {
        return (string.IsNullOrEmpty(mod) || string.IsNullOrEmpty(assetID)) ? string.Empty : $"{mod}:{assetID}";
    }

    public bool Equals(ModAssetSoftReference other)
    {
        return mod == other.mod && assetID == other.assetID && isFolder == other.isFolder;
    }

    public override bool Equals(object obj)
    {
        return obj is ModAssetSoftReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(mod, assetID, isFolder);
    }

    public static bool operator ==(ModAssetSoftReference assetA, ModAssetSoftReference assetB)
    {
        return (assetA.assetID == assetB.assetID) && (assetA.mod == assetB.mod) && (assetA.isFolder == assetB.isFolder);
    }
    
    public static bool operator !=(ModAssetSoftReference assetA, ModAssetSoftReference assetB)
    {
        return !(assetA == assetB);
    }
}
