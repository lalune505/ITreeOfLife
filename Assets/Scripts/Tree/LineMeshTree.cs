using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class LineMeshTree 
{
    public static readonly List<NodeView> NodeViews = new List<NodeView>();
    
    public static void SetNodeViews(Dictionary<int, Node> nodes, int nodeId, int treeDepth, float r)
    {
        Node rootNode = nodes[nodeId];
        CreateSubTree(rootNode, treeDepth, r, Vector3.zero, Quaternion.identity);
    }
    
    private static NodeView CreateSubTree(Node node, int depth,float r, Vector3 pos, Quaternion rot)
    {
        NodeView nodeView = new NodeView();
        nodeView.Init(node, r, pos, rot,new List<NodeView>());

        NodeViews.Add(nodeView);

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

    private static Matrix4x4 GetMatrix4X4(NodeView nodeView)
    {
        return Matrix4x4.TRS(nodeView.pos,  nodeView.rot,
            new Vector3(nodeView.nodeRad, nodeView.nodeRad, nodeView.nodeRad));
    }

    private static Vector3 GetChildNodePosition(float angle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * math.cos((angle) * math.PI/180f);
        endPoint.y = branchLength * math.sin((angle) * math.PI/180f);
        endPoint.z = 0;
        return endPoint; 
    }

    private static float GetNodeAngle(Node node,Node childNode)
    {
        return 180f * math.sqrt(childNode.GetSize()) / node.childrenNodes.Sum(x => math.sqrt(x.GetSize()));
    }
    private static float GetNodeRadius(float nodeAngle)
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
}
