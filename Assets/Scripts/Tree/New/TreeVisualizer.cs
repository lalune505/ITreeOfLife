using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class TreeVisualizer : MonoBehaviour
{
    private NodesData nodes;
    public GameObject halfCirclePrefab;
    public GameObject branchPrefab;
    private float R = 1f;

    private void Awake()
    {
        //DataLoader.InitNodesData();
        //nodes.IntNodeDictionary = DataLoader.nodesData;
        //DataLoader.CreateNodesDataFile();
        nodes = Resources.Load<NodesData>("ScriptableObjects/nodes");
    }

    private void Start()
    {
        DrawSuperKingDom(2759,150f);
       
       // DrawSuperKingDom(2157,30f);
        
      //  DrawSuperKingDom(2,270f);
    }
    private IEnumerator DrawChildren(Node node, GameObject parentNodeGameObject,int depth)
    {
        if (node.childrenNodes.Count == 0 || depth == 0) yield break;
        DrawHalfCircle(parentNodeGameObject);
        float sumAngle = 0f;
        foreach (var childNode in node.childrenNodes)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            float currentAngle = GetHalfCircleSize(node, childNode);
            float childRad = GetHalfCircleRad(currentAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(currentAngle / 2 + sumAngle, R - childRad);
            sumAngle += currentAngle;
            DrawBranch(parentNodeGameObject, childNodePos);
            GameObject nodeGo = CreateNodeObj(childNode, childNodePos, parentNodeGameObject, childRad);
            StartCoroutine(DrawChildren(childNode, nodeGo, depth - 1));
        }
    }
    private void DrawBranch(GameObject parentNodeGameObject,Vector3 endPoint)
    {
        GameObject go = Instantiate(branchPrefab, parentNodeGameObject.transform, false);
        var lr = go.GetComponent<LineRenderer>();
        lr.startWidth = 0.0016f;
        lr.endWidth = 0.0016f;
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

    private void DrawSuperKingDom(int nodeId, float angle)
    {
        var nodePos = GetChildNodePosition(angle, 1);
        DrawBranch(this.gameObject, nodePos);
        var node = CreateNodeObj(nodes.IntNodeDictionary[nodeId], nodePos, this.gameObject, 1f);
        StartCoroutine(DrawChildren(nodes.IntNodeDictionary[nodeId], node, 3));
    }
}
