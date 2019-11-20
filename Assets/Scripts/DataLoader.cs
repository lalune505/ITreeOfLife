using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DataLoader : MonoBehaviour
{
    private string directoryPath = "/Users/zhenyaprivet/Desktop/taxdump";
    private string taxDumpFiles = "/Users/zhenyaprivet/Desktop/taxdump/files";
    private const string TaxDumpFileName = "taxdump.tar.gz";
    private const string NodesFileName = "nodes.dmp";

    // Start is called before the first frame update
    void Start()
    {
       // GetTaxDumpFiles();
    }

    private async void GetTaxDumpFiles()
    {
        await NetworkManager.GetTaxDumpFile(Path.Combine(directoryPath,TaxDumpFileName));
        
        Tar.ExtractTarGz(Path.Combine(directoryPath,TaxDumpFileName),taxDumpFiles);
        
    }

    private Dictionary<long, object> GetNodesDataFromFile(string filePath)
    {
        Dictionary<long, object> nodesData = new Dictionary<long, object>();
        try
        {
            foreach (string line in File.ReadLines(filePath))
            {
                
            }
            
        }
        catch(Exception e ) 
        {
            Debug.Log( $"The process failed with error: {e}");
           
        }
        
        return nodesData;
    }
    
    
}
