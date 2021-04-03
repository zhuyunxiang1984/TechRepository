using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaFramework;

public class TakeSnap : MonoBehaviour
{
    private Texture2D source;
    // Start is called before the first frame update
    void Start()
    {
        source = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24,false);
        source.filterMode = FilterMode.Bilinear;
    }

    // Update is called once per frame
    void OnDestroy()
    {
        Object.Destroy(source);
        source = null;
    }

    public void TakeScreenshot(System.Action OnComplete = null)
    {
        StartCoroutine(TakeSS(OnComplete));
    }

    IEnumerator TakeSS(System.Action OnComplete = null)
    {
        yield return new WaitForEndOfFrame();
        // 读取屏幕像素信息并存储为纹理数据，  
        source.ReadPixels(new Rect(0,0,Screen.width, Screen.height), 0, 0);  
        source.Apply();
        // 通知Lua层，截屏完成
        Util.CallMethod("WarGame", "ScreeShotTaked");
        OnComplete?.Invoke();
    }

    public Texture2D GetScreenshot()
    {
        return source;
    }
}
