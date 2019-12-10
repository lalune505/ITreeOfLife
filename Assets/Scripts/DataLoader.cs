using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static System.Int32;

public class DataLoader
{
    private const string DirectoryPath = "/Users/zhenyaprivet/Desktop/taxdump";
    private const string TaxDumpFiles = "/Users/zhenyaprivet/Desktop/taxdump/files";
    private const string TaxDumpFileName = "taxdump.tar.gz";
    private const string NodesFileName = "nodes.dmp"; 
    private const string SCRIPTABLE_OBJECTS_DESTIONATION_PATH = "Assets/Resources/ScriptableObjects"; 
   // private static Dictionary<int, Node> nodesData = new Dictionary<int, Node>();
    private async void GetTaxDumpFiles()
    {
        await NetworkManager.GetTaxDumpFile(Path.Combine(DirectoryPath,TaxDumpFileName));
        
        Tar.ExtractTarGz(Path.Combine(DirectoryPath,TaxDumpFileName),TaxDumpFiles);
        
    }
    public static void InitNodesData(IDictionary <int, Node> nodesData)
    {
        try
        {
            foreach (string line in File.ReadLines(Path.Combine(TaxDumpFiles, NodesFileName)))
            {
                var dad = Parse(line.Split('|')[1].Replace("\t", ""));
                var son = Parse(line.Split('|')[0].Replace("\t", ""));
                var sonRank = line.Split('|')[2].Replace("\t", "");
                if (!nodesData.ContainsKey(dad))
                {
                    nodesData[dad] = new Node {id = dad};
                }
                if (!nodesData.ContainsKey(son))
                {
                    nodesData[son] = new Node {id = son, rank = sonRank};
                }
                else
                {
                    if (string.IsNullOrEmpty(nodesData[son].rank))
                    {
                        nodesData[son].rank = sonRank;
                    }
                }
                nodesData[dad].childrenNodes.Add(nodesData[son]);
            }

        }
        catch(Exception e) 
        {
            Debug.Log( $"The process failed with error: {e}");
        }
    }

   /* public static Node GetNode(int nodeId)
    {
        return nodesData[nodeId];
    }*/
    
    private static void CreateScriptableObjectFromFile(string filePath)
    {
        EnsureDirectoryExists(SCRIPTABLE_OBJECTS_DESTIONATION_PATH);
 
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        string destinationPath = GetDestinationPath(fileName);
      
        NodesData gm = AssetDatabase.LoadAssetAtPath<NodesData>(destinationPath);
        
        if (gm == null)
        {
            gm = ScriptableObject.CreateInstance<NodesData>();
            AssetDatabase.CreateAsset(gm, destinationPath);
        }
        
        InitNodesData(gm.IntNodeDictionary);
        //gm.IntNodeDictionary = nodesData;
        EditorUtility.SetDirty(gm);

        AssetDatabase.Refresh();

        AssetDatabase.SaveAssets();

    }

    public static void CreateNodesDataFile()
    {
        CreateScriptableObjectFromFile(NodesFileName);
    }
    
    private static string GetDestinationPath(string fileName)
    {
        return SCRIPTABLE_OBJECTS_DESTIONATION_PATH + "/" + fileName + ".asset";
    }
    
    private static void EnsureDirectoryExists(string directory )
    {
        if( !Directory.Exists( directory ) )
            Directory.CreateDirectory( directory );
    }
    
}
