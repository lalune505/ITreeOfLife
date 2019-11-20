using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Async;

public class NetworkManager
{
    private const string TaxDumpURL = "ftp://ftp.ncbi.nlm.nih.gov/pub/taxonomy/taxdump.tar.gz";
    private static async UniTask Request(string url, string path)
    {
        using (var webRequest = UnityWebRequest.Get(url))
        {
            webRequest.downloadHandler = new DownloadHandlerFile(path);
            Debug.Log("Start Loading File");
            await webRequest.SendWebRequest().ConfigureAwait(new Progress<float>(x => Debug.Log(x)));
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.LogError($"Request {webRequest.url} error: {webRequest.error}");
                return;
            }
            Debug.Log("File successfully downloaded and saved to " + path);
            
        }
    }

    public static async UniTask GetTaxDumpFile(string path)
    {
        await Request(TaxDumpURL,path);
    }

}
