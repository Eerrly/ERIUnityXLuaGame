using System.Collections.Generic;

[System.Serializable]
public class ManifestItem
{
    public uint hash;
    public List<uint> dependencies;
    public uint offset;
    public int size;
    /// <summary>
    /// 以文件夹打包
    /// </summary>
    public bool directories = false;
    public string packageResourcePath;
    public string md5;

    [System.NonSerialized]
    public bool packageResource = true;
    [System.NonSerialized] 
    public ManifestItem packageItem = null;
}

[System.Serializable]
public class ManifestConfig
{
    public List<ManifestItem> items;
}

public class Manifest
{
    private static List<uint> defaultValue = new List<uint>();
    public Dictionary<uint, ManifestItem> ManifestDict { get; set; }

    public List<uint> GetDependencies(uint hash)
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

    public List<uint> GetDependencies(uint hash, out uint offset)
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
