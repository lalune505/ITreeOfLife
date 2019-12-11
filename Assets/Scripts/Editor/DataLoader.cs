using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using static System.Int32;

public class DataLoader
{
    private const string DirectoryPath = "/Users/zhenyaprivet/Desktop/taxdump";
    private const string TaxDumpFiles = "/Users/zhenyaprivet/Desktop/taxdump/files";
    private const string TaxDumpFileName = "taxdump.tar.gz";
    private const string NodesFileName = "nodes.dmp"; 
    private const string SCRIPTABLE_OBJECTS_DESTIONATION_PATH = "Assets/Resources/ScriptableObjects"; 
    private static Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    private async void GetTaxDumpFiles()
    {
        await NetworkManager.GetTaxDumpFile(Path.Combine(DirectoryPath,TaxDumpFileName));
        
        Tar.ExtractTarGz(Path.Combine(DirectoryPath,TaxDumpFileName),TaxDumpFiles);
        
    }
    public static void SetNodesData()
    {
        try
        {
            foreach (string line in File.ReadLines(Path.Combine(TaxDumpFiles, NodesFileName)))
            {
                var dad = Parse(line.Split('|')[1].Replace("\t", ""));
                var son = Parse(line.Split('|')[0].Replace("\t", ""));
                var sonRank = line.Split('|')[2].Replace("\t", "");
                if (!nodes.ContainsKey(dad))
                {
                    nodes[dad] = new Node {id = dad};
                }
                if (!nodes.ContainsKey(son))
                {
                    nodes[son] = new Node {id = son, rank = sonRank};
                }
                else
                {
                    if (string.IsNullOrEmpty(nodes[son].rank))
                    {
                        nodes[son].rank = sonRank;
                    }
                }
                nodes[dad].childrenNodes.Add(nodes[son]);
            }

        }
        catch(Exception e) 
        {
            Debug.Log( $"The process failed with error: {e}");
        }
    }

   private static void FillNodesDataById(int nodeId, IDictionary<int, Node> dict)
   {
       foreach (var child in nodes[nodeId].childrenNodes)
       {
           dict[child.id] = child;
           FillNodesDataById(child.id, dict);
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

        FillNodesDataById(rootNodeId,  gm.IntNodeDictionary);

        Debug.Log(gm.IntNodeDictionary.Count);
        
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
