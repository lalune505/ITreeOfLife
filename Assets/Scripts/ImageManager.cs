using System;
using System.Collections;

using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;


public class ImageManager : MonoBehaviour
{
    private string _imagesDirPath = "";

    void Start()
    {
        _imagesDirPath = Path.Combine(Application.persistentDataPath, "Images");
        
        if (!Directory.Exists(_imagesDirPath))
            Directory.CreateDirectory(_imagesDirPath);
        
        NetworkManager.CheckConnection();
        
    }

    public async void GetPage(string nodeName, NodesLabelController.NodeLabel nodeLabel)
    {
        var cacheFilePath = Path.Combine(_imagesDirPath, $"{nodeName}.texture");
        nodeLabel.HideImage();
        Texture2D texture = null;
        if (File.Exists(cacheFilePath))
        {
            var data = File.ReadAllBytes(cacheFilePath);

            texture = new Texture2D(1, 1);
            texture.LoadImage(data, true);

        }
        else
        {
            return;
        }/*else
        {
            texture = await NetworkManager.GetNodeImage(nodeName);
            if (texture)
            {
                CacheTexture(nodeName,texture.EncodeToJPG());
            }
            else
            {
                return;
            }
        }*/

        if (nodeLabel.textLabel.text != nodeName) return;
        
        nodeLabel.image.texture = texture;
        nodeLabel.ShowImage();
    }

    private void CacheTexture(string fileName, byte[] data)
    {
        var cacheFilePath = Path.Combine(_imagesDirPath, $"{fileName}.texture");

        File.WriteAllBytes(cacheFilePath, data);
    }
}
