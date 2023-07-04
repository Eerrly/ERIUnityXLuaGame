using UnityEngine;

public partial class Constant
{
    /// <summary>
    /// 加载资源任务的最大数量
    /// </summary>
    public static int MaxLoadingTaskCount => 32;

    /// <summary>
    /// UI缓存的最大数量
    /// </summary>
    public static int CacheUICount => 5;

    /// <summary>
    /// 图集配置文件
    /// </summary>
    public static string ATLAS_CONFIG_NAME => "sprite_atlas_tools_config.json";

    /// <summary>
    /// 客户端配置文件
    /// </summary>
    public static string CLIENT_CONFIG_NAME => "client.json";

    /// <summary>
    /// AssetBundle描述文件
    /// </summary>
    public static string ASSETBUNDLES_CONFIG_NAME => "asset_bundles.json";

    /// <summary>
    /// 图集文件后缀
    /// </summary>
    public static string ATLASSPRITE_EXTENSION => ".spriteatlas";

    /// <summary>
    /// 客户端版本号
    /// </summary>
    public static string VERSION_TXT_NAME => "version.txt";
}
