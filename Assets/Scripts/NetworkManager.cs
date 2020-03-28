using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Async;

public class NetworkManager
{
    private const string TaxDumpURL = "ftp://ftp.ncbi.nlm.nih.gov/pub/taxonomy/taxdump.tar.gz";
    
    private const string SearchApiURL = "https://eol.org/api/search/1.0.json";
    private const string PagesApiURL = "https://eol.org/api/pages/1.0/";
    private const string PingURL = "";
    
        
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
    
    public static async UniTask<Texture> GetNodeImage(string nodeSciName)
    {
        var searchJson = await Request<JObject>($"{SearchApiURL}?q={nodeSciName}&page=1");

        var pageId = searchJson["results"][0]["id"].Value<string>();

        var pageJson = await Request<JObject>($"{PagesApiURL}{pageId}.json?details=false&images_per_page=1");

        var imageLink = pageJson["taxonConcept"]["dataObjects"][0]["mediaURL"].Value<string>();

        var nodeImage = await GetTexture<Texture>(imageLink);
        
        return nodeImage;
    }

    private static async UniTask<JObject> Request<T>(string urlMethod)
    {
        using (var webRequest = UnityWebRequest.Get(urlMethod))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            await webRequest.SendWebRequest();
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.LogError($"Request {webRequest.url} error: {webRequest.error}");
            }
            string rawJson = Encoding.Default.GetString(webRequest.downloadHandler.data);
            
            return JObject.Parse(rawJson);
        }
    }
    private static async UniTask<Texture> GetTexture<T>(string urlMethod)
    {
        using (var webRequest = UnityWebRequestTexture.GetTexture(urlMethod))
        {
            webRequest.downloadHandler = new DownloadHandlerTexture();
            await webRequest.SendWebRequest();
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.LogError($"Request {webRequest.url} error: {webRequest.error}");
            }

            return DownloadHandlerTexture.GetContent(webRequest);
        }
    }

    public static async UniTask GetTaxDumpFile(string path)
    {
        await Request(TaxDumpURL,path);
    }

}
