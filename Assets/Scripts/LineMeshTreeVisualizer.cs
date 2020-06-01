
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx.Async;
using UnityEngine;

public class LineMeshTreeVisualizer : MonoBehaviour
{
    public Camera cam;
    public int nodeId;
    public Material pointMaterial;
    public float R;
    public int treeDepth;
    public GameObject allTreeStart;
    public ChunkFileLoader chunkFileLoader;
    public LoadingScreen loadingScreen;
    
    private readonly List<Mesh> _meshes = new List<Mesh>();
    //private int _meshCount = 0;
    public void Start()
    { 
        StartCoroutine(TreeChunkGenerator());

        //CreateMeshFromChunk(chunkFileLoader.LoadChunkAt(Vector3.zero));
    }

    private IEnumerator TreeChunkGenerator()
    {
        new Thread(NodesDataFileCreator.SetNodesNamesAndData).Start();

        while (!NodesDataFileCreator.filesDone)
        {
            yield return null;
        }
        new Thread(() => { LineMeshTree.CreateTreeChunkPoints(NodesDataFileCreator.nodes[nodeId], treeDepth, R); }).Start();

        while (!LineMeshTree.workDone)
        {
            yield return null;
        }

        loadingScreen.m_SceneReadyToActivate = true;
        
        TreeChunk chunk = new TreeChunk(Vector3.zero,nodeId,treeDepth, R);
        chunk.Recalculate( LineMeshTree.GetNodeViews());

        chunkFileLoader.SaveChunk(chunk);
        
        CreateMeshFromChunk(chunk);

    }
    
    
    private void Update()
    {
        foreach (var mesh in _meshes)
        {
            Graphics.DrawMesh(mesh,allTreeStart.transform.position,allTreeStart.transform.rotation,pointMaterial,0 , cam);
        }
    }

    public void CreateMeshFromChunk(TreeChunk chunk)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        var index = 0;
        var index1 = 0;

        var s = 0;
        foreach (var size in chunk.sizes)
        {
            vertices.Add(chunk.nodes[s]);
            for (var i = s + 1; i < s + size + 1; i++)
            {
                if (vertices.Count > 65000)
                {
                    CreateObject(new MeshData(vertices.ToArray(), indices.ToArray()));
                    vertices.Clear();
                    indices.Clear();
                    vertices.Add(chunk.nodes[s]);
                    index = 0;
                    index1 = 0;
                }

                vertices.Add(chunk.nodes[i]);
                index++;
                indices.Add(index1);
                indices.Add(index);
            }

            s += size + 1;
            index1 = index + 1;
            index = index1;
        }
        
        CreateObject(new MeshData(vertices.ToArray(), indices.ToArray()));
        
        vertices.Clear();
        indices.Clear();
    }

    private void CreateObject(MeshData m)
    {
         Mesh mesh = new Mesh();
         mesh.vertices = m.vertices;
         mesh.SetIndices(m.tris,MeshTopology.Lines, 0, true);
        
         _meshes.Add(mesh);
         /*GameObject obj = new GameObject("TreeMesh" + _meshCount);
         obj.AddComponent<MeshFilter>().mesh = mesh;
         obj.AddComponent<MeshRenderer>().material = pointMaterial;
        
         obj.transform.SetParent(parentObj.transform, true);
 
         _meshCount++;*/
    }

    
}
