using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using Unity.Mathematics;
using UnityEngine;

public class LineMeshTreeVisualizer : InitializableMonoBehaviour
{
    public int nodeId;
    public Material pointMaterial;
    public float R;
    public int treeDepth;
    public GameObject allTreeStart;

    private int meshCount = 0;
    
    private List<NodeView> _nodeViews = new List<NodeView>();

    public override async UniTask Init()
    {
        NodesDataFileCreator.SetNodesNames();
        NodesDataFileCreator.SetNodesData();
        
        StartCreatingMeshes( NodesDataFileCreator.nodes);

        await UniTask.Yield();
    }

    private void StartCreatingMeshes(Dictionary<int, NodeView> nodes)
    {
        CreateTreeMeshes(nodes);
        CreateMesh();
    }

    private void CreateTreeMeshes(Dictionary<int, NodeView> nodes)
    {
        NodeView rootNode = nodes[nodeId];
        CreateSubTree(rootNode, treeDepth, R, Vector3.zero, Quaternion.identity);
    }

    private void CreateSubTree(NodeView nodeView, int depth, float r, Vector3 pos, Quaternion rot)
    {
         nodeView.Init(r, pos, rot);
          
         GameObject go = new GameObject(nodeView.nodeId.ToString());
 
         go.transform.position = pos;
 
         go.transform.rotation = Quaternion.LookRotation(Vector3.forward, pos);
         _nodeViews.Add(nodeView);

        if (depth == 0) return;
        float sumAngle = 0f;

        Matrix4x4 m = nodeView.GetMatrix4X4();

        foreach (var childNode in nodeView.childrenNodes)
        {
            float childAngle = GetNodeAngle(nodeView, childNode);
            float childRad = GetNodeRadius(childAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(childAngle / 2 + sumAngle, 1 - childRad);
            sumAngle += childAngle;

            CreateSubTree(childNode, depth - 1, childRad * r, m.MultiplyPoint3x4(childNodePos),
                Quaternion.LookRotation(Vector3.forward, m.MultiplyVector(childNodePos)));
        }
    }

    private Vector3 GetChildNodePosition(float angle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * math.cos((angle) * math.PI/180f);
        endPoint.y = branchLength * math.sin((angle) * math.PI/180f);
        endPoint.z = 0;
        return endPoint; 
    }

    private float GetNodeAngle(NodeView node,NodeView childNode)
    {
        return 180f * math.sqrt(childNode.GetSize()) / node.childrenNodes.Sum(x => math.sqrt(x.GetSize()));
    }
    private float GetNodeRadius(float nodeAngle)
    {
        float t =  math.tan(nodeAngle * Mathf.Deg2Rad);

        return t / (t + 1);
    }

    private void CreateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        var index = 0;
        var index1 = 0;
        
        foreach (var nodeView in _nodeViews)
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
        
         GameObject obj = new GameObject("TreeMesh" + meshCount);
         obj.AddComponent<MeshFilter>().mesh = mesh;
         obj.AddComponent<MeshRenderer>().material = pointMaterial;
        
         obj.transform.SetParent(parentObj.transform, true);
 
         meshCount++;
    }
    
}
