using System.Collections.Generic;

public class SpriteAtlasToolsConfig
{

    public string spriteAtlasSaveDirPath = Setting.EditorSpriteAtlasPath;

    public int packingTextureWidthLimit = 1024;

    public int packingTextureHeightLimit = 1024;

    public List<SpriteAtlasToolsConfigItem> itemList = new List<SpriteAtlasToolsConfigItem>();

    [System.Serializable]
    public class SpriteAtlasToolsConfigItem
    {
        public string textureDirPath;

        public bool isSubDirSplit = false;
    }

}
