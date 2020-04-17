using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

public class NodesLabelController : InitializableMonoBehaviour
{
    public int dDepth;
    public class NodeNameLabel
        {
            public Text textLabel;
            public Vector3 pointCoord;
            public CanvasGroup alphaControl;
            public GameObject gameObject;
        }
        
        private Camera _cam;
        private int depthLevel = 11;
            
        private NodeNameLabel[] _currentTextNameLabels;

        public GameObject labelPrefab;

        private float _updatePause = 1f;
        
        private DragCamera _dragCam;
        private float yMin = 0f;
        private float yMax = 3f;

        private List<NodeView> _distNodeViews = new List<NodeView>();
        private List<NodeView> _nodeViews = new List<NodeView>();
        
        public bool updNodes = false;
        public override async UniTask Init()
        {
            _dragCam = FindObjectOfType<DragCamera>();
            
            _cam = _dragCam.GetComponent<Camera>();

            _dragCam.OnCameraZoneChanged.AddListener(OnCamZoneChanged);

            _currentTextNameLabels = new NodeNameLabel[30];

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
                depthLevel = 9;
            }
            else if (zone == 1)
            {
                depthLevel = 10;
            }
            else if (zone == 2)
            {
                depthLevel = 11;
            }

            yMin = _dragCam.camZonesHeights[zone];
            yMax = _dragCam.camZonesHeights[zone + 1];
        }

        private void LateUpdate()
        {
            if (!(updNodes))
            {
                return;
            }
            if (_updatePause > 0f)
            {
                _updatePause -= Time.deltaTime;
                UpdateLabelsScreenPositions();
            }
            else
            {
                _updatePause = 1f;
                GetNodesInView(_nodeViews.FirstOrDefault(x => x.depth == depthLevel));
                UpdateRoadNamesLabels();
            }
            
            foreach (var roadNameLabel in _currentTextNameLabels)
            {
                var trans = roadNameLabel.gameObject.transform;
                trans.localScale = Vector3.one * (_cam.transform.position - trans.position).magnitude / 1000f;
            }
        }


        private void GetNodesInView(NodeView nodeView)
        {
            StartCoroutine(GetNodesInView_Process(nodeView));
        }

        private IEnumerator GetNodesInView_Process(NodeView nodeView)
        {
            if (nodeView == null)
            {
                yield break;
            }
            _distNodeViews.Clear();

            Queue<NodeView> queue = new Queue<NodeView>();
            
            queue.Enqueue(nodeView);

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();

                if (!(element.nodeRad > _cam.transform.position.y / (yMax - yMin) &
                      (nodeView.depth - element.depth) < dDepth)) continue;
                foreach (var child in element.childrenNodes)
                {
                    queue.Enqueue(child);
                }
                
                _distNodeViews.Add(element);
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

        public void AddNodeView(NodeView nodeView)
        {
            _nodeViews.Add(nodeView);
        }

        private List<NodeView> GetNodeViewsByDepthLevel(int d)
        {
            return _nodeViews.Where(x => x.depth == d).ToList();
        }
    
    }

