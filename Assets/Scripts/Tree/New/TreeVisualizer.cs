using System;
using System.Linq;
using UnityEngine;

public class TreeVisualizer : MonoBehaviour
{
    public Node rootNode;
    public Material lineMaterial;
    public float startWidth;
    public float endWidth;
    public GameObject halfCirclePrefab;
    
    private float _branchLength = 1f;
    private void Start()
    {
       DrawChildren(rootNode, Vector3.zero, gameObject);
    }
    private void DrawChildren(Node node, Vector3 nodePos, GameObject parentNodeGameObject)
    {
        GameObject parentNode = CreateNodeObj(node, nodePos, parentNodeGameObject);
        float sumAngle = 0f;
        for (var i = 0; i < node.childrenNodes.Length; i++)
        {
            Vector3 childNodePos = GetChildNodePosition(GetHalfCircleSize(node, node.childrenNodes[i]) / 2 + sumAngle);
            sumAngle += GetHalfCircleSize(node, node.childrenNodes[i]);
            DrawBranch(parentNode, childNodePos);
            DrawChildren(node.childrenNodes[i], childNodePos, parentNode);
        }
    }
    private void DrawBranch(GameObject parentNodeGameObject,Vector3 endPoint)
    {
        GameObject go = new GameObject("Branch");
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;
        lr.material = lineMaterial;

        go.transform.SetParent(parentNodeGameObject.transform, false);
        
        lr.SetPosition(0, Vector3.zero);
        
        lr.SetPosition(1,endPoint);
    }
    private Vector3 GetChildNodePosition(float sumAngle)
    {
        Vector3 endPoint;
        endPoint.x = _branchLength * Mathf.Cos((sumAngle) * Mathf.Deg2Rad) ;
        endPoint.y = _branchLength * Mathf.Sin((sumAngle) * Mathf.Deg2Rad);
        endPoint.z = 0;

        return endPoint;
    }
    private GameObject CreateNodeObj(Node node, Vector3 nodePos, GameObject parentNodeGameObject)
    {
        var nodeObj = new GameObject("Node" + node.id.ToString());
        nodeObj.transform.SetParent(parentNodeGameObject.transform, false);
        nodeObj.transform.localPosition = nodePos;
        nodeObj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, nodePos);
        return nodeObj;
    }

    private float GetHalfCircleSize(Node parentNode,Node childNode)
    {
        return 180f * childNode.GetSize() / parentNode.GetSize();
    }
    private float GetHalfCircleRad(float halfCircleSize, float rad)
    {
        float t = Mathf.Tan(halfCircleSize * Mathf.Deg2Rad);

        return rad * t / (t + 1);
    }
    
    

}
