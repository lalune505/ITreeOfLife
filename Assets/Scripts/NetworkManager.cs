using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using UniRx.Async;
using Debug = UnityEngine.Debug;

public class NetworkManager
{
    private const string TaxDumpURL = "ftp://ftp.ncbi.nlm.nih.gov/pub/taxonomy/taxdump.tar.gz";
    
    private const string SearchApiURL = "https://eol.org/api/search/1.0.json";
    private const string PagesApiURL = "https://eol.org/api/pages/1.0/";
    private const string PingURL = "https://eol.org/api/ping.json";
    
        
    private static async UniTask GetDataFile(string url, string path)
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

    public static async UniTask<Texture2D> GetNodeImage(string nodeSciName)
    {
        var searchJson = await RequestJObject($"{SearchApiURL}?q={nodeSciName}&page=1");

        if (!searchJson["results"].HasValues)
        {
            //Debug.Log(nodeSciName + " no results");
            return null;
        }
        var pageId = searchJson["results"][0]["id"].Value<string>();

        var pageJson = await RequestJObject($"{PagesApiURL}{pageId}.json?details=false&images_per_page=1");

        var token = pageJson["taxonConcept"]["dataObjects"];
        
        if (token == null || !token.HasValues)
        {
            //Debug.Log(nodeSciName + " no data objects");
            return null;
        }
        var imageLink = token[0]["eolMediaURL"].Value<string>();

        imageLink = imageLink.Insert(imageLink.IndexOf(".jpg", StringComparison.Ordinal), ".130x130");

        var nodeImage = await GetTexture(imageLink);
        
        return nodeImage;
    }

    private static async UniTask<JObject> RequestJObject(string urlMethod)
    {
        using (var webRequest = UnityWebRequest.Get(urlMethod))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            await webRequest.SendWebRequest();
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                //Debug.LogError($"Request {webRequest.url} error: {webRequest.error}");
                return null;
            }
            string rawJson = Encoding.Default.GetString(webRequest.downloadHandler.data);
            
            return JObject.Parse(rawJson);
        }
    }

    private static async UniTask<Texture2D> GetTexture(string urlMethod)
    {
        using (var webRequest = UnityWebRequestTexture.GetTexture(urlMethod))
        {
            webRequest.downloadHandler = new DownloadHandlerTexture();
            await webRequest.SendWebRequest();
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                //Debug.LogError($"Request {webRequest.url} error: {webRequest.error}");
                return null;
            }

            return DownloadHandlerTexture.GetContent(webRequest);
        }
    }

    public static async UniTask GetTaxDumpFile(string path)
    {
        await GetDataFile(TaxDumpURL,path);
    }

    public static async void CheckConnection()
    {
        var request = await RequestJObject(PingURL);
        if(request != null) 
            Debug.Log(request["response"]["message"]);
    }

}
