using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx.Async;
using Unity.Mathematics;
using UnityEngine;

public class LineMeshTreeVisualizer : MonoBehaviour
{
    public Camera _cam;
    public int nodeId;
    public Material pointMaterial;
    public float R;
    public int treeDepth;
    public GameObject allTreeStart;

    List<NodeView> _nodeViews = new List<NodeView>();
    
    private List<Mesh> _meshes = new List<Mesh>();
    
    private int _meshCount = 0;
    
    private void Update()
    {
        foreach (var mesh in _meshes)
        {
            Graphics.DrawMesh(mesh,allTreeStart.transform.position,allTreeStart.transform.rotation,pointMaterial,0 , _cam);
        }
    }

    public void CreateTreeMeshes(Dictionary<int, Node> nodes)
    {
        Node rootNode = nodes[nodeId];
        CreateSubTree(rootNode, treeDepth, R, Vector3.zero, Quaternion.identity);
        
        CreateMesh();
    }
    
    private NodeView CreateSubTree(Node node, int depth,float r, Vector3 pos, Quaternion rot)
    {
        NodeView nodeView = new NodeView();
        nodeView.Init(node, r, pos, rot,new List<NodeView>());

        _nodeViews.Add(nodeView);

        if (depth == 0) return nodeView;
        float sumAngle = 0f;

        Matrix4x4 m = GetMatrix4X4(nodeView);
        
        foreach (var childNode in node.childrenNodes)
        {
            float childAngle = GetNodeAngle(node, childNode);
            float childRad = GetNodeRadius(childAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(childAngle / 2 + sumAngle, 1 - childRad);
            sumAngle += childAngle;
            nodeView.AddChildrenNode(CreateSubTree(childNode,depth - 1, childRad * r, m.MultiplyPoint3x4(childNodePos), Quaternion.LookRotation(Vector3.forward,m.MultiplyVector(childNodePos))));
        }

        return nodeView;
    }

    private Matrix4x4 GetMatrix4X4(NodeView nodeView)
    {
        return Matrix4x4.TRS(nodeView.pos,  nodeView.rot,
            new Vector3(nodeView.nodeRad, nodeView.nodeRad, nodeView.nodeRad));
    }

    private Vector3 GetChildNodePosition(float angle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * math.cos((angle) * math.PI/180f);
        endPoint.y = branchLength * math.sin((angle) * math.PI/180f);
        endPoint.z = 0;
        return endPoint; 
    }

    private float GetNodeAngle(Node node,Node childNode)
    {
        return 180f * math.sqrt(childNode.GetSize()) / node.childrenNodes.Sum(x => math.sqrt(x.GetSize()));
    }
    private float GetNodeRadius(float nodeAngle)
    {
        float result;
        if (Math.Abs(nodeAngle - 90f) < 0.00001f)
        {
            result = 0.9f;
        }
        else
        {
            float t =  math.tan(nodeAngle  * math.PI/180f);
            result = t / (t + 1);
        }
        return result;
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
        
         _meshes.Add(mesh);
         /*GameObject obj = new GameObject("TreeMesh" + _meshCount);
         obj.AddComponent<MeshFilter>().mesh = mesh;
         obj.AddComponent<MeshRenderer>().material = pointMaterial;
        
         obj.transform.SetParent(parentObj.transform, true);
 
         _meshCount++;*/
    }

}
