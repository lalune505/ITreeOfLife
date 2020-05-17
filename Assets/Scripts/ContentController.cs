using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ContentController : MonoBehaviour
{
    public LineMeshTreeVisualizer lineMeshTreeVisualizer;
    public LoadingScreen loadingScreen;
    Thread _thread;
    
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(CreateMeshFromNodesData());
    }

    IEnumerator CreateMeshFromNodesData()
    {
        _thread = new Thread(NodesDataFileCreator.SetNodesNamesAndData);
        _thread.Start();

        NodesDataFileCreator.threadRunning = true;

        while (NodesDataFileCreator.threadRunning)
        {
            yield return null;
        }
     
        _thread = new Thread(lineMeshTreeVisualizer.SetNodeViews);
        _thread.Start();
        lineMeshTreeVisualizer.threadRunning = true;
        
        while (lineMeshTreeVisualizer.threadRunning)
        {
            yield return null;
        }
        
        lineMeshTreeVisualizer.CreateMesh(LineMeshTree.NodeViews);
        
        loadingScreen.m_SceneReadyToActivate = true;
    }
    
    
    void OnDisable()
    {
        if (NodesDataFileCreator.threadRunning || lineMeshTreeVisualizer.threadRunning)
        {
            NodesDataFileCreator.threadRunning = false;
            lineMeshTreeVisualizer.threadRunning = false;
            _thread.Join();
        }
        
    }
}
