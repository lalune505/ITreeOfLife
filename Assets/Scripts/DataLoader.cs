using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static System.Int32;

public class DataLoader
{
    private const string DirectoryPath = "/Users/zhenyaprivet/Desktop/taxdump";
    private const string TaxDumpFiles = "/Users/zhenyaprivet/Desktop/taxdump/files";
    private const string TaxDumpFileName = "taxdump.tar.gz";
    private const string NodesFileName = "nodes.dmp"; 
    private  static Dictionary<int, Node> nodesData = new Dictionary<int, Node>();
    private async void GetTaxDumpFiles()
    {
        await NetworkManager.GetTaxDumpFile(Path.Combine(DirectoryPath,TaxDumpFileName));
        
        Tar.ExtractTarGz(Path.Combine(DirectoryPath,TaxDumpFileName),TaxDumpFiles);
        
    }
    public static void InitNodesData()
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
        catch(Exception e ) 
        {
            Debug.Log( $"The process failed with error: {e}");
        }

    }

    public static Dictionary<int, Node> GetNodesData()
    {
        return nodesData;
    }
    
}
