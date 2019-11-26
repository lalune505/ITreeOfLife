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
    public float R;
  
    private void Start()
    {
       DrawChildren(rootNode, Vector3.zero, gameObject, R);
    }
    private void DrawChildren(Node node, Vector3 nodePos, GameObject parentNodeGameObject, float nodeRad)
    {
        GameObject nodeObj = CreateNodeObj(node, nodePos, parentNodeGameObject);
        if (node.childrenNodes.Length == 0) return;
        DrawHalfCircle(nodeObj, nodeRad);
        float sumAngle = 0f;
        foreach (var childNode in node.childrenNodes)
        {
            float currentAngle = GetHalfCircleSize(node, childNode);
            float childRad = GetHalfCircleRad(currentAngle / 2, nodeRad);
            Vector3 childNodePos = GetChildNodePosition(currentAngle / 2 + sumAngle, nodeRad - childRad);
            sumAngle += currentAngle;
            DrawBranch(nodeObj, childNodePos);
            DrawChildren(childNode, childNodePos, nodeObj, childRad);
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
    private Vector3 GetChildNodePosition(float sumAngle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * Mathf.Cos((sumAngle) * Mathf.Deg2Rad) ;
        endPoint.y = branchLength * Mathf.Sin((sumAngle) * Mathf.Deg2Rad);
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
    private float GetHalfCircleRad(float halfCircleSize, float parentRad)
    {
        float t = Mathf.Tan(halfCircleSize * Mathf.Deg2Rad);

        return parentRad * t / (t + 1);
    }

    private void DrawHalfCircle(GameObject parentGameObject, float scale)
    {
       GameObject hc = Instantiate(halfCirclePrefab, parentGameObject.transform, false);
       
       hc.transform.localScale =  new Vector3(scale, scale, scale);
    }
    
}
