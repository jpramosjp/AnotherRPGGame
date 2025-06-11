using UnityEngine;
using System.IO;

public class ShootPhoto : MonoBehaviour
{
    public Camera iconCamera;
    public int resolution = 512;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            RenderTexture rt = new RenderTexture(resolution, resolution, 24);
            iconCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            iconCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            iconCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            byte[] bytes = screenShot.EncodeToPNG();
            string filename = Path.Combine(Application.dataPath, "icon.png");
            File.WriteAllBytes(filename, bytes);

            Debug.Log("Ícone salvo em: " + filename);
        }
       
    }
}
