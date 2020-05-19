using System.Collections;
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

        while (!NodesDataFileCreator.filesDone)
        {
            yield return null;
        }
     
        _thread = new Thread(lineMeshTreeVisualizer.SetNodeViews);
        _thread.Start();

        while (!lineMeshTreeVisualizer.workDone)
        {
            yield return null;
        }
        
        lineMeshTreeVisualizer.CreateMesh(LineMeshTree.NodeViews);
        
        loadingScreen.m_SceneReadyToActivate = true;
    }
    
    
    void OnDisable()
    {
        if (!NodesDataFileCreator.filesDone || !lineMeshTreeVisualizer.workDone)
        {
            NodesDataFileCreator.filesDone = true;
            lineMeshTreeVisualizer.workDone = true;
            _thread.Join();
        }
        
    }
}
