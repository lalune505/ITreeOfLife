using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public delegate void DataHandler(NodesData data);
    public static event DataHandler OnDataLoaded;

    public int rootId;

    private void Awake()
    {
      // NodesDataFileCreator.SetNodesNames();
      // NodesDataFileCreator.SetNodesData();
       
       //OnDataLoaded?.Invoke(NodesDataFileCreator.nodes);
    }

    private void Start()
    {
       LoadAssetBundle("nodes", rootId.ToString());
    }

    void LoadAssetBundle(string assetBundleName,string id)
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);
        
         AssetBundle.LoadFromFileAsync(filePath).AsAsyncOperationObservable ()
             .Subscribe (xs => {if (xs.assetBundle != null) {
                     LoadAssetFromBundle(xs.assetBundle, assetBundleName + id.ToString() ); } 
             }).AddTo (this);
        
    }

    void LoadAssetFromBundle(AssetBundle assetBundle,string assetName)
    {
       assetBundle.LoadAssetAsync<NodesData>(assetName).AsAsyncOperationObservable ()
           .Subscribe(x => {
                if (x.asset != null)
                {
                    NodesData data = x.asset as NodesData;
                    OnDataLoaded?.Invoke(data);
                    assetBundle.Unload(true);
                }
           }).AddTo (this);
    }
    
}
