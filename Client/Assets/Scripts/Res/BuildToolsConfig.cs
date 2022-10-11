using System.Collections.Generic;

public class BuildToolsConfig
{

    public bool enablePatching = false;

    public bool useAssetBundle = false;

    public List<BuildToolsConfigItem> itemList = new List<BuildToolsConfigItem>();

    public class BuildToolsConfigItem
    {
        public string root;
        public bool directories;
        public int searchoption;
        public string extension;
        public string filter;
    }

}
