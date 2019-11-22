using System;
using System.Linq;
using UnityEngine;

public class TreeVisualizer : MonoBehaviour
{
    public Node rootNode;
    public Material lineMaterial;
    public float startWidth;
    public float endWidth;
    
    private float _branchLength = 1f;
    private void Start()
    {
        if (rootNode == null) return;
        var root = gameObject;
        DrawChildren(rootNode, DrawBranch(root, root.transform.position + Vector3.up));
    }

    private void DrawChildren(Node node,GameObject parentBranch)
    {
        var parentNode = CreateNodeObj(node, parentBranch);
        for (var i = 0; i < node.childrenNodes.Length; i++)
        {
            Vector3 nextNodePos = GetNextNodePosition(GetBranchAngle(node,i));
            DrawChildren(node.childrenNodes[i], DrawBranch(parentNode, nextNodePos));
        }
    }
    private GameObject DrawBranch(GameObject parentNode,Vector3 endPoint)
    {
        GameObject go = new GameObject("Branch");
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;
        lr.material = lineMaterial;

        go.transform.SetParent(parentNode.transform, false);
        
        lr.SetPosition(0, Vector3.zero);
        
        lr.SetPosition(1,endPoint);

        return go;
    }
    private Vector3 GetNextNodePosition(float angle)
    {
        Vector3 endPoint;
        endPoint.x = _branchLength * Mathf.Cos(angle * Mathf.Deg2Rad);
        endPoint.y = _branchLength * Mathf.Sin(angle * Mathf.Deg2Rad);
        endPoint.z = 0;

        return endPoint;
    }
    private GameObject CreateNodeObj(Node node, GameObject parentBranch)
    {
        var nodeObj = new GameObject("Node" + node.id.ToString());
        nodeObj.transform.SetParent(parentBranch.transform, false);
        nodeObj.transform.localPosition = parentBranch.GetComponent<LineRenderer>().GetPosition(1);
        return nodeObj;
    }
    private int GetSize(Node node)
    {
        /*if (node.childrenNodes.Length == 0)
        {
            return 0;
        }*/
        return 1 + node.childrenNodes.Sum(GetSize);
    }

    private float GetHalfCircleSize(Node node, int branchIndex)
    {
        return 180f * GetSize(node.childrenNodes[branchIndex]) / GetSize(node);;
    }

    private float GetBranchAngle(Node node,int branchIndex)
    {
        if (branchIndex == 0)
        {
            return GetHalfCircleSize(node, branchIndex) / 2;
        }
        return GetBranchAngle(node, branchIndex - 1) + GetHalfCircleSize(node, branchIndex - 1) / 2 + GetHalfCircleSize(node, branchIndex) / 2;
    }

}
