using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ContentController : MonoBehaviour
{
    public LineMeshTreeVisualizer lineMeshTreeVisualizer;
    public LoadingScreen loadingScreen;
    Thread _thread;
    private bool _workDone = false;
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

        while (NodesDataFileCreator.threadRunning & !_workDone)
        {
            yield return null;
        }

        lineMeshTreeVisualizer.CreateTreeMeshes(NodesDataFileCreator.nodes);
        _workDone = true;
        loadingScreen.m_SceneReadyToActivate = _workDone;
    }
    
    
    void OnDisable()
    {
        if (NodesDataFileCreator.threadRunning)
        {
            NodesDataFileCreator.threadRunning = false;
            _thread.Join();
        }
        
    }
}
