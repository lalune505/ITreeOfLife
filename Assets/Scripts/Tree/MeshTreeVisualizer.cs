using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshTreeVisualizer : MonoBehaviour
{
    public int nodeId;
    public GameObject branchPrefab;
    public GameObject nodePrefab;
    public Material pointMaterial;
    public float R;
    public float width;
    public int treeDepth;
    private NodeView allTreeStart;
    private int meshCount = 0;
    private Dictionary<int, List<NodeView>> _nodeViews = new Dictionary<int, List<NodeView>>();
    

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
        allTreeStart = new GameObject("Tree").AddComponent<NodeView>();
        CreateSubTree(allTreeStart, branch, prefabVertices, prefabTris, nodes.IntNodeDictionary[nodeId],
          treeDepth, meshVertices, meshTris );
        
        CreateObject(meshVertices, meshTris, allTreeStart.gameObject);
        meshVertices.Clear();
        meshTris.Clear();
        Destroy(branch);
        
    }
    
    private void CreateSubTree(NodeView nodeView, GameObject branch,Vector3[] branchVerts, int[] branchTris, Node node,int depth, List<Vector3> meshVertices,
        List<int> meshTris)
    {
        if (node.childrenNodes.Count == 0 || depth == 0) return;
        float sumAngle = 0f;
        
        foreach (var childNode in node.childrenNodes)
        {
            float childAngle = GetNodeAngle(node, childNode);
            float childRad = GetNodeRadius(childAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(childAngle / 2 + sumAngle, R - childRad);
            sumAngle += childAngle;
            NodeView childNodeGo = CreateNodeObj(childNode,childNodePos,nodeView, childRad);
            childNodeGo.depth = depth;
            _nodeViews[depth].Add(childNodeGo);
            
            AppendBranchVertices(nodeView, branch,width * depth / treeDepth, branchVerts, branchTris,childNodePos, meshVertices, meshTris);
            
            CreateSubTree(childNodeGo,branch, branchVerts, branchTris,childNode, depth - 1, meshVertices, meshTris);
            if (meshVertices.Count + branchVerts.Length > 65000)
            {
                CreateObject(meshVertices, meshTris,allTreeStart.gameObject);
                meshVertices.Clear();
                meshTris.Clear();
            }
        }
        
    }
    private void AppendBranchVertices(NodeView rootView, GameObject b,float bWidth,Vector3[] bVerts, int[] bTris,Vector3 endPoint, List<Vector3> meshVertices,
        List<int> meshTris)
    {
        b.transform.parent = rootView.transform;
        b.transform.localPosition = Vector3.zero;
        b.transform.localRotation = Quaternion.LookRotation(endPoint, Vector3.right);

        b.transform.localScale = Vector3.one;
        var lossyScale = b.transform.lossyScale.x;
        b.transform.localScale = new Vector3( bWidth / lossyScale,bWidth / lossyScale, endPoint.magnitude );
        
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
        GameObject obj = new GameObject("TreeMesh" + meshCount);
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = pointMaterial;
       
        obj.transform.SetParent(parentObj.transform, false);

        meshCount++;
    }
    private Vector3 GetChildNodePosition(float angle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * Mathf.Cos((angle) * Mathf.Deg2Rad) ;
        endPoint.y = branchLength * Mathf.Sin((angle) * Mathf.Deg2Rad);
        endPoint.z = 0;
        return endPoint;
    }
    
    private NodeView CreateNodeObj(Node node,Vector3 nodePos, NodeView rootView, float scale)
    {
        var nodeObj = Instantiate(nodePrefab);
        nodeObj.name = node.id.ToString();
        var nodeView = nodeObj.AddComponent<NodeView>();
        nodeView.Init(node);
        rootView.AddChildrenNode(nodeView);
        
        nodeObj.transform.parent = rootView.transform;
        nodeObj.transform.localPosition = nodePos;
        nodeObj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, nodePos);
        nodeObj.transform.localScale =  new Vector3(scale, scale, scale);

        nodeView.branchLength = (rootView.transform.position - nodeView.transform.position).magnitude;
        
        return nodeView;
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

    public List<NodeView> GetNodeViewsByDepthLevel(int d)
    {
        return _nodeViews[d];
    }
}
