using System.Collections.Generic;

[System.Serializable]
public class ManifestItem
{
    public uint hash;
    public uint[] dependencies;
    public uint offset;
    public int size;
    public bool directories = false;
    public string extension;
    public string packageResourcePath;
    public bool packageResource = false;
    public bool isPatching = false;
    public string md5;

    [System.NonSerialized] 
    public ManifestItem packageItem = null;
}

[System.Serializable]
public class ManifestConfig
{
    public ManifestItem[] items;
}

public class Manifest
{
    private static uint[] defaultValue = new uint[] { };
    public Dictionary<uint, ManifestItem> ManifestDict { get; set; }

    public uint[] GetDependencies(uint hash)
    {
        if(ManifestDict != null)
        {
            ManifestItem item;
            if(ManifestDict.TryGetValue(hash, out item))
            {
                return item.dependencies;
            }
        }
        return defaultValue;
    }

    public bool Exist(uint hash)
    {
        if (ManifestDict != null)
        {
            return ManifestDict.ContainsKey(hash);
        }
        return false;
    }

    public ManifestItem GetItem(uint hash)
    {
        if(ManifestDict != null)
        {
            ManifestItem item;
            if (ManifestDict.TryGetValue(hash, out item))
            {
                return item;
            }
        }
        return null;
    }

    public uint[] GetDependencies(uint hash, out uint offset)
    {
        offset = 0;
        if (ManifestDict != null)
        {
            ManifestItem item;
            if (ManifestDict.TryGetValue(hash, out item))
            {
                offset = item.offset;
                return item.dependencies;
            }
        }
        return defaultValue;
    }

}
