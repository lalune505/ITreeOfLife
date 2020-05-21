
using System.Collections.Generic;
using UnityEngine;

public class LineMeshTreeVisualizer : MonoBehaviour
{
    public Camera cam;
    public int nodeId;
    public Material pointMaterial;
    public float R;
    public int treeDepth;
    public GameObject allTreeStart;

    [HideInInspector]
    public bool workDone = false;
    
    private readonly List<Mesh> _meshes = new List<Mesh>();
    //private int _meshCount = 0;
    public void Start()
    {
       // LineMeshTree.SetNodeViews(NodesDataFileCreator.nodes, nodeId, treeDepth, R);
        //workDone = true;
        
        NodesDataFileCreator.SetNodes1Data();
    }
    private void Update()
    {
        foreach (var mesh in _meshes)
        {
            Graphics.DrawMesh(mesh,allTreeStart.transform.position,allTreeStart.transform.rotation,pointMaterial,0 , cam);
        }
    }

    public void CreateMesh(IEnumerable<NodeView> nodeViews)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        var index = 0;
        var index1 = 0;
        
        foreach (var nodeView in nodeViews)
        {
            vertices.Add(nodeView.pos);
            foreach (var childrenNode in nodeView.childrenNodes)
            {
                if (vertices.Count > 65000)
                {
                    CreateObject(vertices, indices,allTreeStart.gameObject);
                    vertices.Clear();
                    indices.Clear();
                    vertices.Add(nodeView.pos);
                    index = 0;
                    index1 = 0;
                }
                vertices.Add(childrenNode.pos);
                index++;
                indices.Add(index1);
                indices.Add(index);
            }
            index1 = index + 1;
            index = index1;
        }
        CreateObject(vertices, indices,allTreeStart.gameObject);
        vertices.Clear();
        indices.Clear();
    }
    
    private void CreateObject(List<Vector3> meshVertices, List<int> meshTris, GameObject parentObj)
    {
         Mesh mesh = new Mesh();
         mesh.vertices = meshVertices.ToArray();
         mesh.SetIndices(meshTris.ToArray(),MeshTopology.Lines, 0, true);
        
         _meshes.Add(mesh);
         /*GameObject obj = new GameObject("TreeMesh" + _meshCount);
         obj.AddComponent<MeshFilter>().mesh = mesh;
         obj.AddComponent<MeshRenderer>().material = pointMaterial;
        
         obj.transform.SetParent(parentObj.transform, true);
 
         _meshCount++;*/
    }

}
