using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class TreeVisualizer : MonoBehaviour
{
    public float threshold = 0f;
    public float thresholdCluster = 0f;
    public int depth;
    public GameObject halfCirclePrefab;
    public GameObject branchPrefab;
    public Material branchMat;
    private float R = 1f;
    
    private void Start()
    {
        DataLoader.OnDataLoaded += CreateObjectFromData;
    }
    private void CreateTree(Node node, GameObject parentNodeGameObject,int depth)
    {
        if (node.childrenNodes.Count == 0 || depth == 0) return;
        DrawHalfCircle(parentNodeGameObject);
        float sumAngle = 0f;
        foreach (var childNode in node.childrenNodes)
        {
            float currentAngle = GetHalfCircleSize(node, childNode);
            float childRad = GetHalfCircleRad(currentAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(currentAngle / 2 + sumAngle, R - childRad);
            sumAngle += currentAngle;
            CreateBranch(parentNodeGameObject, childNodePos);
            GameObject nodeGo = CreateNodeObj(childNode, childNodePos, parentNodeGameObject, childRad);
            
            CreateTree(childNode, nodeGo, depth - 1);
        }
    }
    private GameObject CreateBranch(GameObject parentNodeGameObject,Vector3 endPoint)
    {
        GameObject go = Instantiate(branchPrefab, parentNodeGameObject.transform, false);
        var lr = go.GetComponent<LineRenderer>();
        lr.startWidth = 0.007f;
        lr.endWidth = 0.007f;
        lr.sharedMaterial = branchMat;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1,endPoint);
        if (Vector3.Scale(endPoint, go.transform.lossyScale).magnitude > threshold)
        {
            lr.enabled = true;
        }
        return go;
    }
    private Vector3 GetChildNodePosition(float sumAngle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * Mathf.Cos((sumAngle) * Mathf.Deg2Rad) ;
        endPoint.y = branchLength * Mathf.Sin((sumAngle) * Mathf.Deg2Rad);
        endPoint.z = 0;
        return endPoint;
    }
    private GameObject CreateNodeObj(Node node, Vector3 nodePos, GameObject parentNodeGameObject, float scale)
    {
        var nodeObj = new GameObject("Node" + node.id);
        nodeObj.transform.SetParent(parentNodeGameObject.transform, false);
        nodeObj.transform.localPosition = nodePos;
        nodeObj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, nodePos);
        nodeObj.transform.localScale =  new Vector3(scale, scale, scale);
        
        return nodeObj;
    }

    private float GetHalfCircleSize(Node parentNode,Node childNode)
    {
        return 180f * Mathf.Sqrt(childNode.GetSize()) / parentNode.childrenNodes.Sum(x => Mathf.Sqrt(x.GetSize()));
    }
    private float GetHalfCircleRad(float halfCircleSize)
    {
        float t = Mathf.Tan(halfCircleSize * Mathf.Deg2Rad);

        return R * t / (t + 1);
    }

    private void DrawHalfCircle(GameObject parentGameObject)
    { 
        var go = Instantiate(halfCirclePrefab, parentGameObject.transform, false);
        if (parentGameObject.transform.lossyScale.x > thresholdCluster)
        {
            go.GetComponentInChildren<Renderer>().enabled = true;
        }
    }

    public void DrawTree(NodesData data, Tree tree)
    {
        var nodePos = GetChildNodePosition(tree.Angle, 1);
        var branch = CreateBranch(this.gameObject, nodePos);
        var node = CreateNodeObj(data.IntNodeDictionary[tree.RootId], nodePos, this.gameObject, 1f);
        CreateTree(data.IntNodeDictionary[tree.RootId], node, tree.Depth );
    }

    public void CreateObjectFromData(NodesData nodes)
    {
        DrawTree(nodes, new Tree(2,270f,depth));
       // DrawTree(nodes,new Tree(2157, 30f, depth));
       // DrawTree(nodes, new Tree(2759, 150f, depth));
    }
}
[Serializable]
public class Tree
{   public int RootId { get; set; }
    public float Angle { get; set; }
    public int Depth { get; set; }
    public Tree(int rootId, float angle, int depth)
    {
        this.RootId = rootId;
        this.Angle = angle;
        this.Depth = depth;
    }
}
