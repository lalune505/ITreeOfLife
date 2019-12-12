using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class TreeVisualizer : MonoBehaviour
{
    public int depth;
    public GameObject halfCirclePrefab;
    public GameObject branchPrefab;
    public Material branchMat;
    private float R = 1f;

    private AssetBundle assetBundle;
    private NodesData nodes2;
    private NodesData nodes2157;
    private NodesData nodes2759;
    private void Start()
    {
        StartCoroutine(DrawTree(depth));
    }
    private void DrawChildren(Node node, GameObject parentNodeGameObject,int depth)
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
            DrawBranch(parentNodeGameObject, childNodePos);
            GameObject nodeGo = CreateNodeObj(childNode, childNodePos, parentNodeGameObject, childRad);
            if (nodeGo.transform.lossyScale.x < 0.01)
            {
                nodeGo.SetActive(false);
            }
            DrawChildren(childNode, nodeGo, depth - 1);
        }
    }
    private void DrawBranch(GameObject parentNodeGameObject,Vector3 endPoint)
    {
        GameObject go = Instantiate(branchPrefab, parentNodeGameObject.transform, false);
        var lr = go.GetComponent<LineRenderer>();
        lr.startWidth = 0.002f;
        lr.endWidth = 0.002f;
        lr.sharedMaterial = branchMat;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1,endPoint);
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
        Instantiate(halfCirclePrefab, parentGameObject.transform, false);
    }

    private void DrawTree(Tree tree)
    {
        var nodePos = GetChildNodePosition(tree.Angle, 1);
        DrawBranch(this.gameObject, nodePos);
        var node = CreateNodeObj(tree.Nodes.IntNodeDictionary[tree.RootId], nodePos, this.gameObject, 1f);
        DrawChildren(tree.Nodes.IntNodeDictionary[tree.RootId], node, tree.Depth );
    }
    
    IEnumerator LoadAssetBundle(string assetBundleName)
         {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles");
        filePath = System.IO.Path.Combine(filePath, assetBundleName);
        
        var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
        yield return assetBundleCreateRequest;

        assetBundle = assetBundleCreateRequest.assetBundle;
        nodes2 = assetBundle.LoadAsset<NodesData>(assetBundleName + "2");
        //nodes2157 = assetBundle.LoadAsset<NodesData>(assetBundleName + "2157");
       // nodes2759 = assetBundle.LoadAsset<NodesData>(assetBundleName + "2759");
      
         assetBundle.Unload(true);
    }

    private IEnumerator DrawTree(int d)
    {
        yield return StartCoroutine(LoadAssetBundle("nodes"));
        DrawTree(new Tree(nodes2,2,270f,d));
       // DrawTree(new Tree(nodes2157, 2157, 30f, d));
       // DrawTree(new Tree(nodes2759, 2759, 150f, d));
    }
}
[Serializable]
public class Tree
{
    public NodesData Nodes { get; set; }
    public int RootId { get; set; }
    public float Angle { get; set; }
    public int Depth { get; set; }
    public Tree(NodesData nodes, int rootId, float angle, int depth)
    {
        this.Nodes = nodes;
        this.RootId = rootId;
        this.Angle = angle;
        this.Depth = depth;
    }
}
