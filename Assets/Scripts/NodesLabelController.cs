using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

public class NodesLabelController : InitializableMonoBehaviour
{
    public float minBranchLength;
    public int depthLevels;
    public class NodeNameLabel
        {
            public Text textLabel;
            public Vector3 pointCoord;
            public CanvasGroup alphaControl;
            public GameObject gameObject;
        }
        
        private Camera _cam;
        private int _dDepth = 0;
            
        private NodeNameLabel[] _currentTextNameLabels;

        public GameObject labelPrefab;

        private float _updatePause = 1f;
        
        private DragCamera _dragCam;
        
        private List<NodeView> _distNodeViews = new List<NodeView>();

        private MeshTreeVisualizer _meshTreeVisualizer;

        public override async UniTask Init()
        {
            _dragCam = FindObjectOfType<DragCamera>();

            _meshTreeVisualizer = FindObjectOfType<MeshTreeVisualizer>();
            
            _cam = _dragCam.GetComponent<Camera>();

            _dragCam.OnCameraZoneChanged.AddListener(OnCamZoneChanged);

            _currentTextNameLabels = new NodeNameLabel[100];

            for (var i = 0; i < _currentTextNameLabels.Length; i++)
            {
                var nodeNameLabel = new NodeNameLabel();

                var label = Instantiate(labelPrefab);
                label.transform.localScale = Vector3.one;

                nodeNameLabel.alphaControl = label.GetComponentInChildren<CanvasGroup>();
                nodeNameLabel.textLabel = label.GetComponentInChildren<Text>();
                nodeNameLabel.gameObject = label;

                _currentTextNameLabels[i] = nodeNameLabel;
            }
            

            await UniTask.Yield();
        }
        

        private void OnCamZoneChanged(int zone)
        {
            if (zone == 0)
            {
                _dDepth = 0;
            }
            else if (zone == 1)
            {
                _dDepth = 3;
            }
            else if (zone == 2)
            {
                _dDepth = 6;
            }
        }

        private void LateUpdate()
        {
            if (_updatePause > 0f)
            {
                _updatePause -= Time.deltaTime;
                UpdateLabelsScreenPositions();
            }
            else
            {
                _updatePause = 1f;
                GetRoadsInView();
                UpdateRoadNamesLabels();
            }
            
            foreach (var roadNameLabel in _currentTextNameLabels)
            {
                var trans = roadNameLabel.gameObject.transform;
                trans.localScale = Vector3.one * (_cam.transform.position - trans.position).magnitude / 1000f;
            }
        }


        private void GetRoadsInView()
        {
            StartCoroutine(GetRoadsInView_Process());
        }

        private IEnumerator GetRoadsInView_Process()
        {
            _distNodeViews.Clear();
            
            foreach (var nodeView in _meshTreeVisualizer.GetNodeViewsByDepthLevel(_dDepth))
            {
                Queue<NodeView> queue = new Queue<NodeView>();
            
                queue.Enqueue(nodeView);

                while (queue.Count > 0)
                {
                    var element = queue.Dequeue();

                    if ((element.branchLength  < (minBranchLength * _cam.transform.position.y / _dragCam.yMax)) & (element.depth - nodeView.depth) > depthLevels)
                    {
                        continue;
                    }

                    foreach (var child in element.childrenNodes)
                    {
                        queue.Enqueue(child);
                    }
                
                    _distNodeViews.Add(element);
                }
            }

            yield return null;
        }


        private void UpdateRoadNamesLabels()
        {
            StartCoroutine(UpdateRoadNamesLabels_Process());
        }

        IEnumerator UpdateRoadNamesLabels_Process()
        {
            for (int i = 0; i < _currentTextNameLabels.Length; i++)
            {
                if (_distNodeViews.Count <= i)
                {
                    _currentTextNameLabels[i].textLabel.text = "";
                    yield break;
                }

                _currentTextNameLabels[i].pointCoord = _distNodeViews[i].transform.position;
                _currentTextNameLabels[i].textLabel.text = _distNodeViews[i].sciName;
            }

            yield return null;
        }


        private void UpdateLabelsScreenPositions()
        {
            foreach (var label in _currentTextNameLabels)
            {
                label.gameObject.transform.position = new Vector3(label.pointCoord.x, 
                    label.gameObject.transform.position.y, label.pointCoord.z);
            }
        }
        
    }

