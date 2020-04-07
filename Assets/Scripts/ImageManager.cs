using System;
using System.Collections;

using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;


public class ImageManager : MonoBehaviour
{
    public string name;
    public RawImage nodeImage;
    private string imagesDirPath = "";
  
    void Start()
    {
        imagesDirPath = Path.Combine(Application.persistentDataPath, "Images");
        
        if (!Directory.Exists(imagesDirPath))
            Directory.CreateDirectory(imagesDirPath);
        
        NetworkManager.CheckConnection();
        GetPage();
    }

    private async void GetPage()
    {
        var texture = GetTextureFromCache(name);

        if (texture == null)
        {
            texture = await NetworkManager.GetNodeImage(name);

            if (texture == null)
            {
                return;
            }

            CacheTexture(name,texture.EncodeToJPG());
        }
        
        nodeImage.texture = texture;
        nodeImage.SetNativeSize();

        nodeImage.enabled = true;
    }
    
    private Texture2D GetTextureFromCache(string fileName)
    {
        var cacheFilePath = Path.Combine(imagesDirPath, $"{fileName}.texture");

        Texture2D texture = null;

        if (File.Exists(cacheFilePath))
        {
            var data = File.ReadAllBytes(cacheFilePath);

            texture = new Texture2D(1, 1);
            texture.LoadImage(data, true);
        }

        return texture;
    }
    
    private void CacheTexture(string fileName, byte[] data)
    {
        var cacheFilePath = Path.Combine(imagesDirPath, $"{fileName}.texture");

        File.WriteAllBytes(cacheFilePath, data);
    }

    private void OnDestroy()
    {
       // if (Directory.Exists(imagesDirPath)) { Directory.Delete(imagesDirPath, true); }
    }
}
