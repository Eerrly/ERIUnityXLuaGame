using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameApp : MonoBehaviour
{
    void Start()
    {
        Load();
    }

    async void Load()
    {
        AssetBundle prefabBundle = await UnityResourceLoader.Instance.LoadAssetBundleAsyncTask(Application.streamingAssetsPath + "/prefabcanvas");
        Object obj = await UnityResourceLoader.Instance.LoadObjectAsyncTask<Object>(prefabBundle, "PrefabCanvas");
        var go = Instantiate(obj) as GameObject;
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        LoadIcons(go);
    }

    void LoadIcons(GameObject go)
    {
        var pImgIcon1 = go.transform.GetChild(0).GetComponent<Image>();
        AssetBundle spriteBundle = UnityResourceLoader.Instance.LoadAssetBundle(Application.streamingAssetsPath + "/game");
        Sprite sprite1 = UnityResourceLoader.Instance.LoadObject<Sprite>(spriteBundle, "icon_1");
        pImgIcon1.sprite = sprite1;
        var pImgIcon2 = go.transform.GetChild(1).GetComponent<Image>();
        Sprite sprite2 = UnityResourceLoader.Instance.LoadObject<Sprite>(spriteBundle, "icon_2");
        pImgIcon2.sprite = sprite2;
    }

}
