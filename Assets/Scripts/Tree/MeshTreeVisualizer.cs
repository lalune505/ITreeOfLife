using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshTreeVisualizer : MonoBehaviour
{
    public GameObject branchPrefab;
    public GameObject allTreeStart;
    public Material pointMaterial;
    public float R;
    public float valueScaleMultiplier = 1;

    private void Start()
    {
        DataLoader.OnDataLoaded += CreateTreeMeshes;
    }
    public void CreateTreeMeshes(NodesData nodes)
    {
        GameObject branch = Instantiate(branchPrefab);
        Vector3[] prefabVertices = branch.GetComponent<MeshFilter>().mesh.vertices;
        int[] prefabTris = branch.GetComponent<MeshFilter>().mesh.triangles;

        List<Vector3> meshVertices = new List<Vector3>(65000);
        List<int> meshTris = new List<int>(117000);

        CreateSubTree(branch, prefabVertices, prefabTris, nodes.IntNodeDictionary[2],
            allTreeStart, 2, meshVertices, meshTris );
        
        CreateObject(meshVertices, meshTris, allTreeStart);
        meshVertices.Clear();
        meshTris.Clear();
        Destroy(branch);
        
    }
    
    private void CreateSubTree(GameObject branch,Vector3[] branchVerts, int[] branchTris, Node node, GameObject parentNodeGameObject,int depth, List<Vector3> meshVertices,
        List<int> meshTris)
    {
        if (node.childrenNodes.Count == 0 || depth == 0) return;
        //DrawHalfCircle(parentNodeGameObject);
        float sumAngle = 0f;
        foreach (var childNode in node.childrenNodes)
        {
            float currentAngle = GetHalfCircleSize(node, childNode);
            float childRad = GetHalfCircleRad(currentAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(currentAngle / 2 + sumAngle, R - childRad);
            sumAngle += currentAngle;
            AppendBranchVertices(branch, branchVerts, branchTris, parentNodeGameObject, childNodePos, meshVertices, meshTris);
            GameObject nodeGo = CreateNodeObj(childNode, childNodePos, parentNodeGameObject, childRad);
            CreateSubTree(branch,branchVerts, branchTris,childNode, nodeGo, depth - 1, meshVertices, meshTris);
        }
    }
    private void AppendBranchVertices(GameObject b, Vector3[] bVerts, int[] bTris, GameObject parentNodeGameObject,Vector3 endPoint, List<Vector3> meshVertices,
        List<int> meshTris)
    {
        b.transform.SetParent(parentNodeGameObject.transform, false);
        b.transform.localRotation = Quaternion.LookRotation(Vector3.right, endPoint);
        
        int prevVertCount = meshVertices.Count;

        for (int k = 0; k < bVerts.Length; k++)
        {
            meshVertices.Add(b.transform.TransformPoint(bVerts[k]));
        }
        for (int k = 0; k < bTris.Length; k++)
        {
            meshTris.Add(prevVertCount + bTris[k]);
        }

    }
    private void CreateObject(List<Vector3> meshertices, List<int> meshindecies, GameObject seriesObj)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = meshertices.ToArray();
        mesh.triangles = meshindecies.ToArray();
        GameObject obj = new GameObject();
        obj.transform.parent = allTreeStart.transform;
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = pointMaterial;
        obj.transform.parent = seriesObj.transform;
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

}
