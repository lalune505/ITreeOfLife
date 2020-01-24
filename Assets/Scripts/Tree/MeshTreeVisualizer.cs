using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshTreeVisualizer : MonoBehaviour
{
    public GameObject branchPrefab;
    public Material pointMaterial;
    public float R;
    private GameObject allTreeStart;

    private void Start()
    {
        DataLoader.OnDataLoaded += CreateTreeMeshes;
    }
    public void CreateTreeMeshes(NodesData nodes)
    {
        GameObject branch = Instantiate(branchPrefab);
        Vector3[] prefabVertices = branch.GetComponentInChildren<MeshFilter>().mesh.vertices;
        int[] prefabTris = branch.GetComponentInChildren<MeshFilter>().mesh.triangles;

        List<Vector3> meshVertices = new List<Vector3>(65000);
        List<int> meshTris = new List<int>(117000);
        allTreeStart = new GameObject("Root");
        CreateSubTree(allTreeStart, branch, prefabVertices, prefabTris, nodes.IntNodeDictionary[2],
          2, meshVertices, meshTris );
        
        CreateObject(meshVertices, meshTris, allTreeStart);
        meshVertices.Clear();
        meshTris.Clear();
        Destroy(branch);
        
    }
    
    private void CreateSubTree(GameObject root, GameObject branch,Vector3[] branchVerts, int[] branchTris, Node node,int depth, List<Vector3> meshVertices,
        List<int> meshTris)
    {
        if (node.childrenNodes.Count == 0 || depth == 0)
        {
            Destroy(root);//check it
            return;
        }
        float sumAngle = 0f;
        foreach (var childNode in node.childrenNodes)
        {
            float childAngle = GetNodeAngle(node, childNode);
            float childRad = GetNodeRadius(childAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(childAngle / 2 + sumAngle, R - childRad);
            sumAngle += childAngle;
            GameObject childNodeGo = CreateNodeObj(childNodePos, root, childRad);
            AppendBranchVertices(root, branch, Mathf.Sqrt(root.transform.lossyScale.x), branchVerts, branchTris,childNodePos, meshVertices, meshTris);
            
            CreateSubTree(childNodeGo,branch,branchVerts, branchTris,childNode, depth - 1, meshVertices, meshTris);
            childNodeGo.transform.parent = null;
            if (meshVertices.Count + branchVerts.Length > 65000)
            {
                CreateObject(meshVertices, meshTris, allTreeStart);
                meshVertices.Clear();
                meshTris.Clear();
            }
        }
        if (root.name != allTreeStart.name)
            Destroy(root);

    }
    private void AppendBranchVertices(GameObject root, GameObject b, float scale, Vector3[] bVerts, int[] bTris,Vector3 endPoint, List<Vector3> meshVertices,
        List<int> meshTris)
    {
        b.transform.parent = root.transform;
        b.transform.localPosition = Vector3.zero;
        b.transform.localRotation = Quaternion.LookRotation(endPoint, Vector3.right);
        
        b.transform.localScale =  new Vector3(scale ,scale  ,endPoint.magnitude); 
       
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
    private void CreateObject(List<Vector3> meshVertices, List<int> meshTris, GameObject parentObj)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTris.ToArray();
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = pointMaterial;
       
        obj.transform.SetParent(parentObj.transform, false);
    }
    private Vector3 GetChildNodePosition(float angle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * Mathf.Cos((angle) * Mathf.Deg2Rad) ;
        endPoint.y = branchLength * Mathf.Sin((angle) * Mathf.Deg2Rad);
        endPoint.z = 0;
        return endPoint;
    }
    
    private GameObject CreateNodeObj(Vector3 nodePos, GameObject rootGameObject, float scale)
    {
        var nodeObj = new GameObject("Node");
        nodeObj.transform.parent = rootGameObject.transform;
        nodeObj.transform.localPosition = nodePos;
        nodeObj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, nodePos);
        nodeObj.transform.localScale =  new Vector3(scale, scale, scale);
        
        return nodeObj;
    }
    
    private float GetNodeAngle(Node node,Node childNode)
    {
        return 180f * Mathf.Sqrt(childNode.GetSize()) / node.childrenNodes.Sum(x => Mathf.Sqrt(x.GetSize()));
    }
    private float GetNodeRadius(float nodeAngle)
    {
        float t = Mathf.Tan(nodeAngle * Mathf.Deg2Rad);

        return R * t / (t + 1);
    }

}
