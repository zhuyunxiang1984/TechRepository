using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaussianBlur : MonoBehaviour
{
    //模糊半径
    public float BlurRadius = 1.0f;
    //降分辨率
    public int downSample = 2;
    //迭代次数
    public int iteration = 2;

    public Material _Material;

    private RenderTexture m_ScreenShot = null;

    void OnEnable()
    {
        //StartCoroutine(TakeScreenShot());
        TakeScreenShot();
    }
    void OnDisable(){
        if(m_ScreenShot){
            Shader.SetGlobalTexture("_ScreenShot", null);
            RenderTexture.ReleaseTemporary(m_ScreenShot);
            m_ScreenShot = null;
        }
    }

    void TakeScreenShot(){
        if (_Material)
        {
            //yield return new WaitForEndOfFrame();
            GameObject camera = GameObject.Find("MainCamera");
            TakeSnap takeSnap = camera.GetComponent<TakeSnap>();
            Texture2D source = takeSnap.GetScreenshot();

            // Texture2D source = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24,false);
            // source.filterMode = FilterMode.Bilinear;
        
            // // 读取屏幕像素信息并存储为纹理数据，  
            // source.ReadPixels(new Rect(0,0,Screen.width, Screen.height), 0, 0);  
            // source.Apply();

            
            
            //申请RenderTexture，RT的分辨率按照downSample降低
            RenderTexture rt1 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample);
            RenderTexture rt2 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample);
 
            //直接将原图拷贝到降分辨率的RT上
            Graphics.Blit(source, rt1);
 
            //进行迭代高斯模糊
            for(int i = 0; i < iteration; i++)
            {
                //第一次高斯模糊，设置offsets，竖向模糊
                _Material.SetVector("_offsets", new Vector4(0, BlurRadius, 0, 0));
                Graphics.Blit(rt1, rt2, _Material);
                //第二次高斯模糊，设置offsets，横向模糊
                _Material.SetVector("_offsets", new Vector4(BlurRadius, 0, 0, 0));
                Graphics.Blit(rt2, rt1, _Material);
            }

            if(m_ScreenShot == null){
                m_ScreenShot = RenderTexture.GetTemporary(Screen.width, Screen.height);
            }
 
            //将结果输出
            Graphics.Blit(rt1, m_ScreenShot);

            Material mat = GetComponent<Image>().material;
            mat.SetTexture("_ScreenShot", m_ScreenShot);
            
            

            //mat.SetTextureOffset("_ScreenShot", new Vector2(rect));

            //Shader.SetGlobalTexture("_ScreenShot", m_ScreenShot);
 
            //释放申请的两块RenderBuffer内容
            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }
    }
}
