using System.Collections.Generic;

public class AssetBundleToolsConfig
{
    public List<AssetBundleToolsConfigItem> Items = new List<AssetBundleToolsConfigItem>();

    [System.Serializable]
    public class AssetBundleToolsConfigItem
    {
        public uint hash;
        public List<uint> dependencies;
        public uint offset;
        public int size;
        public bool directories;
        public string extension;
        public string packageResourcePath;
        public bool isPatching;
        public string md5;
    }

}
