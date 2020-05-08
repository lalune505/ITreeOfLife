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
    public GameObject nodePrefab;
    public Material pointMaterial;
    public float R;
    public int treeDepth;
    public GameObject allTreeStart;
    public MeshFilter mesh;

    List<NodeView> _nodeViews = new List<NodeView>();
    
    private int meshCount = 0;

    public override async UniTask Init()
    {
        NodesDataFileCreator.SetNodesNames();
        NodesDataFileCreator.SetNodesData();
        
        StartCreatingMeshes( NodesDataFileCreator.nodes);
        
        await UniTask.Yield();
    }

    private void StartCreatingMeshes(Dictionary<int, Node> nodes)
    {
        CreateTreeMeshes(nodes);
        CreateMesh();
    }

    private void CreateTreeMeshes(Dictionary<int, Node> nodes)
    {
        Node rootNode = nodes[nodeId];
        NodeView rootNodeView = new NodeView();
        rootNodeView.Init(rootNode, treeDepth + 1, R, allTreeStart.transform.position, new List<NodeView>());
        
        _nodeViews.Add(rootNodeView);
        
        CreateSubTree(allTreeStart,rootNodeView, rootNode,
          treeDepth );

    }
    
    private void CreateSubTree(GameObject parent,NodeView parentNodeView, Node node,int depth)
    {
        if (node.childrenNodes.Count == 0 || depth == 0) return;
        float sumAngle = 0f;
        
        foreach (var childNode in node.childrenNodes)
        {
            float childAngle = GetNodeAngle(node, childNode);
            float childRad = GetNodeRadius(childAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(childAngle / 2 + sumAngle, R - childRad);
            sumAngle += childAngle;
            CreateNodeObj(childNode,childNodePos,parent,parentNodeView, childRad,out NodeView childNodeView,  out GameObject childGo,depth);
            CreateSubTree(childGo,childNodeView,childNode, depth - 1);
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
    
    private void CreateNodeObj(Node node,Vector3 nodePos, GameObject parent,NodeView parentNodeView, float scale, out NodeView nodeView,out GameObject nodeObj, int d)
    {
        nodeObj = Instantiate(nodePrefab);
        nodeObj.name = node.id.ToString();

        nodeObj.transform.parent = parent.transform;
        nodeObj.transform.localPosition = nodePos;
        nodeObj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, nodePos);
        nodeObj.transform.localScale =  new Vector3(scale, scale, scale);

        var r = nodeObj.transform.lossyScale.x;

        nodeView = new NodeView();
        
        nodeView.Init(node, d, r, nodeObj.transform.position, new List<NodeView>());
        
        _nodeViews.Add(nodeView);
        parentNodeView.AddChildrenNode(nodeView);
        

    }
    
    private float GetNodeAngle(Node node,Node childNode)
    {
        return 180f * math.sqrt(childNode.GetSize()) / node.childrenNodes.Sum(x => math.sqrt(x.GetSize()));
    }
    private float GetNodeRadius(float nodeAngle)
    {
        float t =  math.tan(nodeAngle * Mathf.Deg2Rad);

        return 1 * t / (t + 1);
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
