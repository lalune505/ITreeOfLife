﻿using System;
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

    private async void GetTaxDumpFiles()
    {
        await NetworkManager.GetTaxDumpFile(Path.Combine(DirectoryPath,TaxDumpFileName));
        
        Tar.ExtractTarGz(Path.Combine(DirectoryPath,TaxDumpFileName),TaxDumpFiles);
        
    }
    public static Dictionary<int, Node> GetNodesData()
    {
        Dictionary<int, Node> nodesData = new Dictionary<int, Node>();
        try
        {
            foreach (string line in File.ReadLines(Path.Combine(TaxDumpFiles, NodesFileName)))
            {
                var dad = Parse(line.Split('|')[1].Replace("\t", ""));
                var son = Parse(line.Split('|')[0].Replace("\t", ""));
                if (!nodesData.ContainsKey(dad))
                {
                    nodesData[dad] = new Node {id = dad};
                }
                if (!nodesData.ContainsKey(son))
                {
                    nodesData[son] = new Node {id = son};
                }
                
                nodesData[dad].childrenNodes.Add(nodesData[son]);
            }
            
        }
        catch(Exception e ) 
        {
            Debug.Log( $"The process failed with error: {e}");
        }
        
        return nodesData;
    }
    
    
}
