
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    
    private readonly List<Mesh> _meshes = new List<Mesh>();
    //private int _meshCount = 0;
    public void Start()
    {
       // StartCoroutine(TreeChunkGenerator());

       CreateMeshFromChunk(chunkFileLoader.LoadChunkAt(Vector3.zero));
    }

    private IEnumerator TreeChunkGenerator()
    {
        new Thread(NodesDataFileCreator.SetNodes1Data).Start();

        while (!NodesDataFileCreator.filesDone)
        {
            yield return null;
        }

        new Thread(() => { LineMeshTree.CreateTreeChunkPoints(NodesDataFileCreator.nodes[nodeId], treeDepth, R); }).Start();

        while (!LineMeshTree.workDone)
        {
            yield return null;
        }
        
        TreeChunk chunk = new TreeChunk( Vector3.zero,nodeId,treeDepth, R);
        
        chunk.Recalculate(LineMeshTree.GetNodeViews());
        
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
        foreach (var item in chunk.meshData)
        {
            CreateObject(item);
        }
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
