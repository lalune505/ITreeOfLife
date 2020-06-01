using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using static System.Int32;

public class NodesDataFileCreator
{
    private const string DirectoryPath = "/Users/zhenyaprivet/Desktop/taxdump";
    private const string TaxDumpFiles = "/Users/zhenyaprivet/Desktop/taxdump/files";
    private const string TaxDumpFileName = "taxdump.tar.gz";
    private const string NodesFileName = "nodes.dmp";
    private const string NamesFileName = "names.dmp";
    private const string SCRIPTABLE_OBJECTS_DESTIONATION_PATH = "Assets/Resources/ScriptableObjects"; 
    public static Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    public static Dictionary<int, NodeName> names = new Dictionary<int, NodeName>();

    public static bool filesDone = false;
    private async void GetTaxDumpFiles()
    {
        await NetworkManager.GetTaxDumpFile(Path.Combine(DirectoryPath,TaxDumpFileName));
        
        Tar.ExtractTarGz(Path.Combine(DirectoryPath,TaxDumpFileName),TaxDumpFiles);
        
    }

    public static void SetNodesNamesAndData()
    {
        //SetNodesNames();
        var json = File.ReadAllText(@"/Users/zhenyaprivet/Desktop/taxdump/names.json");
        
        names = JsonConvert.DeserializeObject<Dictionary<int, NodeName>>(json);

        SetNodesData();
        filesDone = true;

    }
    public static void SetNodesData()
    {
        try
        {
            foreach (string line in File.ReadLines(Path.Combine(TaxDumpFiles, NodesFileName)))
            {
                var dad = Parse(line.Split('|')[1].Replace("\t", ""));
                var son = Parse(line.Split('|')[0].Replace("\t", ""));
                if (!nodes.ContainsKey(dad))
                {
                    nodes[dad] = new Node {id = dad, sciName = names[dad].sciName};
                }
                if (!nodes.ContainsKey(son))
                {
                    nodes[son] = new Node {id = son, sciName = names[son].sciName};
                }
                nodes[dad].childrenNodes.Add(nodes[son]);
            }

        }
        catch(Exception e) 
        {
            Debug.Log( $"The process failed with error: {e}");
        }
    }
    
    public static void SetNodes1Data()
    {
        try
        {
            foreach (string line in File.ReadLines(Path.Combine(TaxDumpFiles, NodesFileName)))
            {
                var dad = Parse(line.Split('|')[1].Replace("\t", ""));
                var son = Parse(line.Split('|')[0].Replace("\t", ""));
                if (!nodes.ContainsKey(dad))
                {
                    nodes[dad] = new Node {id = dad};
                }
                if (!nodes.ContainsKey(son))
                {
                    nodes[son] = new Node {id = son};
                }
                nodes[dad].childrenNodes.Add(nodes[son]);
            }

        }
        catch(Exception e) 
        {
            Debug.Log( $"The process failed with error: {e}");
        }

        filesDone = true;
    }
    
    public static void SetNodesNames()
    {
        try
        {
            foreach (string line in File.ReadLines(Path.Combine(TaxDumpFiles, NamesFileName)))
            {
               var taxId = Parse(line.Split('|')[0].Replace("\t", ""));
               var nameVal = line.Split('|')[1].Replace("\t", "");
               var nameType = line.Split('|')[3].Replace("\t", "");

               if (!names.ContainsKey(taxId))
               {
                   names[taxId] = new NodeName();
               }
               if (nameType == "scientific name")
               {
                   names[taxId].sciName = nameVal;
               }
            }

        }
        catch(Exception e) 
        {
            Debug.Log( $"The process failed with error: {e}");
        }
    }

    public static void CreateNodesScriptableObject(int rootNodeId)
    {
        EnsureDirectoryExists(SCRIPTABLE_OBJECTS_DESTIONATION_PATH);
 
        string fileName = Path.GetFileNameWithoutExtension(NodesFileName);

        string destinationPath = GetDestinationPath(fileName,rootNodeId);
      
        NodesData gm = AssetDatabase.LoadAssetAtPath<NodesData>(destinationPath);
        
        if (gm == null)
        {
            gm = ScriptableObject.CreateInstance<NodesData>();
            AssetDatabase.CreateAsset(gm, destinationPath);
        }

        gm.IntNodeDictionary = nodes;

        EditorUtility.SetDirty(gm);

        AssetDatabase.Refresh();

        AssetDatabase.SaveAssets();

    }
    
    public static void CreateSceneDataScriptableObject(int id, List<NodeView> nodeViews)
    {
        EnsureDirectoryExists(SCRIPTABLE_OBJECTS_DESTIONATION_PATH);

        string fileName = "SceneData";

        string destinationPath = GetDestinationPath(fileName,id);
      
        SceneData gm = AssetDatabase.LoadAssetAtPath<SceneData>(destinationPath);
        
        if (gm == null)
        {
            gm = ScriptableObject.CreateInstance<SceneData>();
            AssetDatabase.CreateAsset(gm, destinationPath);
        }

        gm.nodeViews = nodeViews;

        EditorUtility.SetDirty(gm);

        AssetDatabase.Refresh();

        AssetDatabase.SaveAssets();

    }
    private static string GetDestinationPath(string fileName,int id)
    {
        return SCRIPTABLE_OBJECTS_DESTIONATION_PATH + "/" + fileName + id.ToString() + ".asset";
    }
    
    private static void EnsureDirectoryExists(string directory )
    {
        if( !Directory.Exists( directory ) )
            Directory.CreateDirectory( directory );
    }
    
}
