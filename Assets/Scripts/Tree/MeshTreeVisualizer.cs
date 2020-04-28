using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using Unity.Mathematics;
using UnityEngine;

public class MeshTreeVisualizer : InitializableMonoBehaviour
{
    public int nodeId;
    public GameObject branchPrefab;
    public GameObject nodePrefab;
    public Material pointMaterial;
    public float R;
    public float width;
    public int treeDepth;
    public GameObject allTreeStart;
    public SceneData sceneData;
    private Camera _cam;
    //public NodesLabelController nodesLabelController;
    private int meshCount = 0;
    private List<Mesh> _meshes = new List<Mesh>();
    
    /*private List<NodeView> nodeViews = new List<NodeView>();

    private List<MeshData> _meshData = new List<MeshData>();
    */


    void Update()
    {
        foreach (var mesh in _meshes)
        {
            Graphics.DrawMesh(mesh,allTreeStart.transform.position,Quaternion.identity,pointMaterial,0 , _cam);
        }
    }
    public override async UniTask Init()
    {
        _cam = Camera.main;
        
        DataLoader.OnDataLoaded += StartCreatingMeshes;

        foreach (var item in sceneData.meshData)
        {
            CreateMesh(item, allTreeStart);
        }
        await UniTask.Yield();
    }

    private void StartCreatingMeshes(NodesData data)
    {
        CreateTreeMeshes(data);
        
       // NodesDataFileCreator.CreateSceneDataScriptableObject(1, nodeViews, _meshData);
    }

    private void CreateTreeMeshes(NodesData nodes)
    {
        GameObject branch = Instantiate(branchPrefab);
        Vector3[] prefabVertices = branch.GetComponentInChildren<MeshFilter>().mesh.vertices;
        int[] prefabTris = branch.GetComponentInChildren<MeshFilter>().mesh.triangles;

        List<Vector3> meshVertices = new List<Vector3>(65000);
        List<int> meshTris = new List<int>(117000);

        Node rootNode = nodes.IntNodeDictionary[nodeId];
        NodeView rootNodeView = new NodeView();
        rootNodeView.Init(rootNode, treeDepth + 1, R, allTreeStart.transform.position, new List<NodeView>());
        //nodeViews.Add(rootNodeView);
        //nodesLabelController.AddNodeView(rootNode.id,rootNodeView);
        
        CreateSubTree(allTreeStart,rootNodeView, branch, prefabVertices, prefabTris, rootNode,
          treeDepth, meshVertices, meshTris );
         
        CreateObject(meshVertices, meshTris, allTreeStart.gameObject);
        meshVertices.Clear();
        meshTris.Clear();
        Destroy(branch);
        
    }
    
    private void CreateSubTree(GameObject parent,NodeView parentNodeView, GameObject branch,Vector3[] branchVerts, int[] branchTris, Node node,int depth, List<Vector3> meshVertices,
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
            CreateNodeObj(childNode,childNodePos,parent,parentNodeView, childRad,out NodeView childNodeView,  out GameObject childGo,depth);
            //nodeViews.Add(childNodeView);
            //nodesLabelController.AddNodeView(childNode.id,childNodeView);
            AppendBranchVertices(parent, branch,width * depth / treeDepth, branchVerts, branchTris,childNodePos, meshVertices, meshTris);
            
            CreateSubTree(childGo,childNodeView,branch, branchVerts, branchTris,childNode, depth - 1, meshVertices, meshTris);
            if (meshVertices.Count + branchVerts.Length > 65000)
            {
                CreateObject(meshVertices, meshTris,allTreeStart.gameObject);
                meshVertices.Clear();
                meshTris.Clear();
            }
            
        }
        
    }
    private void AppendBranchVertices(GameObject parent, GameObject b,float bWidth,Vector3[] bVerts, int[] bTris,Vector3 endPoint, List<Vector3> meshVertices,
        List<int> meshTris)
    {
        b.transform.parent = parent.transform;
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
       
        obj.transform.SetParent(parentObj.transform, true);

       meshCount++;
    }
    
    private void CreateMesh(MeshData meshData, GameObject parentObj)
    {
        Mesh mesh = new Mesh {vertices = meshData.vertices, triangles = meshData.tris};
        _meshes.Add(mesh);

       /* GameObject obj = new GameObject("TreeMesh" + meshCount);
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = pointMaterial;
       
        obj.transform.SetParent(parentObj.transform, true);

        meshCount++;*/
    }
    private Vector3 GetChildNodePosition(float angle, float branchLength)
    {
        Vector3 endPoint;
        endPoint.x = branchLength * math.cos((angle) * math.PI/180f);
        endPoint.y = branchLength * math.sin((angle) * math.PI/180f);
        endPoint.z = 0;
        return endPoint;
    }
    
    private void CreateNodeObj(Node node,Vector3 nodePos, GameObject parent,NodeView parentNodeView, float scale, out NodeView nodeView,out GameObject nodeObj, int d)
    {
        nodeObj = Instantiate(nodePrefab);
        nodeObj.name = node.id.ToString();

        nodeObj.transform.parent = parent.transform;
        nodeObj.transform.localPosition = nodePos;
        nodeObj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, nodePos);
        nodeObj.transform.localScale =  new Vector3(scale, scale, scale);

        var r = nodeObj.transform.lossyScale.x;

        nodeView = new NodeView();
        nodeView.Init(node, d, r, nodeObj.transform.position, new List<NodeView>());
        parentNodeView.AddChildrenNode(nodeView);
    }
    
    private float GetNodeAngle(Node node,Node childNode)
    {
        return 180f * math.sqrt(childNode.GetSize()) / node.childrenNodes.Sum(x => math.sqrt(x.GetSize()));
    }
    private float GetNodeRadius(float nodeAngle)
    {
        float t =  math.tan(nodeAngle * Mathf.Deg2Rad);

        return R * t / (t + 1);
    }
}
