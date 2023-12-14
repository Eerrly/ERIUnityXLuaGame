using System.Collections.Generic;
using UnityEngine.Serialization;

public class BuildToolsConfig
{

    public bool enablePatching = false;

    public bool useAssetBundle = false;

    public List<BuildToolsConfigItem> itemList = new List<BuildToolsConfigItem>();

    [System.Serializable]
    public class BuildToolsConfigItem
    {
        public string root;
        public bool directories;
        public int searchOption;
        public string filter;
    }

}
