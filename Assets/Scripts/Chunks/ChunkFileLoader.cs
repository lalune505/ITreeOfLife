using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class ChunkFileLoader : MonoBehaviour
{
    public string chunkDirectoryName = "chunks";
    private string chunkDicrectory;
    public bool enableSaving = false;
    
    public void Awake()
    {
        chunkDicrectory = Application.persistentDataPath + "/" + chunkDirectoryName + "/";

        if (!Directory.Exists(chunkDicrectory))
            Directory.CreateDirectory(chunkDicrectory);
    }
    
    public Chunk LoadChunkAt(Vector3 chunkPos)
    { 
        var filename = chunkPos.ToString().Md5Sum();
        
        var fullPath = chunkDicrectory + filename + ".json";
        if (!File.Exists(fullPath)) return null;
        
        var fileContents = File.ReadAllText(fullPath);

        var chunk = JsonConvert.DeserializeObject<Chunk>(fileContents);

        if (chunk.verticies.Count == 0)
        {
            File.Delete(fullPath);
            return null; //This chunk is corrupt, ignore it..
        }

        Debug.Log("Loaded from " + fullPath);

        return chunk;
    }

    public void SaveChunk(Chunk chunk)
    {
        if (!enableSaving)
            return;
        
        if (chunk.verticies.Count == 0)
            return; //Don't save an empty chunk..
        
        var chunkPos = chunk.position;
        
        var filename = chunkPos.ToString().Md5Sum();
        
        var fullPath = chunkDicrectory + filename + ".json";

        var json = JsonConvert.SerializeObject(chunk);
        
        File.WriteAllText(fullPath, json);
    }
}
