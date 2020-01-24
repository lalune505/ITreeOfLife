using System;
using System.Collections;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public delegate void DataHandler(NodesData data);
    public static event DataHandler OnDataLoaded;
    private AssetBundle assetBundle;
    private NodesData data;

    private void Start()
    {
        StartCoroutine(CreateTree());
    }

    IEnumerator LoadAssetBundle(string assetBundleName)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);
        
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        assetBundle = assetBundleCreateRequest.assetBundle;
        data = assetBundle.LoadAsset<NodesData>(assetBundleName + "131567");

        assetBundle.Unload(true);
    }
    
    private IEnumerator CreateTree()
    {
        yield return StartCoroutine(LoadAssetBundle("nodes"));
        OnDataLoaded?.Invoke(data);
    }

    private void OnDestroy()
    {
        StopCoroutine(CreateTree());
    }
}
