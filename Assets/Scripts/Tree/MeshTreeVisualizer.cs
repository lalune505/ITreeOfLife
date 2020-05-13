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

    private List<NodeView> nodeViews = new List<NodeView>();

    private List<MeshData> _meshData = new List<MeshData>();
    


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
        
        NodesDataFileCreator.SetNodesNames();
        NodesDataFileCreator.SetNodesData();
        
        StartCreatingMeshes( NodesDataFileCreator.nodes);

        /*foreach (var item in sceneData.meshData)
        {
            CreateMesh(item, allTreeStart);
        }*/
        await UniTask.Yield();
    }

    private void StartCreatingMeshes(Dictionary<int, NodeView> nodes)
    {
        CreateTreeMeshes(nodes);
        //NodesDataFileCreator.CreateSceneDataScriptableObject(nodeId, nodeViews, _meshData);
    }

    private void CreateTreeMeshes(Dictionary<int, NodeView> nodes)
    {
        GameObject branch = Instantiate(branchPrefab);
        Vector3[] prefabVertices = branch.GetComponentInChildren<MeshFilter>().mesh.vertices;
        int[] prefabTris = branch.GetComponentInChildren<MeshFilter>().mesh.triangles;

        List<Vector3> meshVertices = new List<Vector3>(65000);
        List<int> meshTris = new List<int>(117000);

        NodeView rootNode = nodes[nodeId];
        rootNode.Init(R, allTreeStart.transform.position, allTreeStart.transform.rotation);
        //nodeViews.Add(rootNodeView);
        //nodesLabelController.AddNodeView(rootNode.id,rootNodeView);
        
        CreateSubTree(allTreeStart,rootNode, branch, prefabVertices, prefabTris,
          treeDepth, meshVertices, meshTris );
         
        CreateObject(meshVertices, meshTris, allTreeStart.gameObject);
        meshVertices.Clear();
        meshTris.Clear();
        Destroy(branch);
        
    }
    
    private void CreateSubTree(GameObject parent,NodeView parentNodeView, GameObject branch,Vector3[] branchVerts, int[] branchTris,int depth, List<Vector3> meshVertices,
        List<int> meshTris)
    {
        if (parentNodeView.childrenNodes.Count == 0 || depth == 0 || parentNodeView.nodeRad < 0.001f) return;
        float sumAngle = 0f;
        
        foreach (var childNode in parentNodeView.childrenNodes)
        {
            float childAngle = GetNodeAngle(parentNodeView, childNode);
            float childRad = GetNodeRadius(childAngle / 2);
            Vector3 childNodePos = GetChildNodePosition(childAngle / 2 + sumAngle, R - childRad);
            sumAngle += childAngle;
            CreateNodeObj(childNode,childNodePos,parent,childRad,  out GameObject childGo,depth);
            //nodeViews.Add(childNodeView);
            //nodesLabelController.AddNodeView(childNode.id,childNodeView);
            AppendBranchVertices(parent, branch,  width * depth/treeDepth, branchVerts, branchTris,childNodePos, meshVertices, meshTris);
            
            CreateSubTree(childGo,childNode,branch, branchVerts, branchTris, depth - 1, meshVertices, meshTris);
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
        _meshes.Add(mesh);
        
        /*MeshData meshData = new MeshData(meshVertices.ToArray(),meshTris.ToArray());
        
        _meshData.Add(meshData);*/

       /* GameObject obj = new GameObject("TreeMesh" + meshCount);
        obj.AddComponent<MeshFilter>().mesh = mesh;
        obj.AddComponent<MeshRenderer>().material = pointMaterial;
       
        obj.transform.SetParent(parentObj.transform, true);

        meshCount++;*/
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
    
    private void CreateNodeObj(NodeView node,Vector3 nodePos, GameObject parent, float scale,out GameObject nodeObj, int d)
    {
        nodeObj = Instantiate(nodePrefab);
        nodeObj.name = node.nodeId.ToString();

        nodeObj.transform.parent = parent.transform;
        nodeObj.transform.localPosition = nodePos;
        nodeObj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, nodePos);
        nodeObj.transform.localScale =  new Vector3(scale, scale, scale);

        var r = nodeObj.transform.lossyScale.x;
        node.Init(r, nodeObj.transform.position,Quaternion.LookRotation(Vector3.forward, nodePos));
    }
    
    private float GetNodeAngle(NodeView node,NodeView childNode)
    {
        return 180f * math.sqrt(childNode.GetSize()) / node.childrenNodes.Sum(x => math.sqrt(x.GetSize()));
    }
    private float GetNodeRadius(float nodeAngle)
    {
        float t =  math.tan(nodeAngle * Mathf.Deg2Rad);

        return R * t / (t + 1);
    }
}
