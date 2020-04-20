using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Async;
using Unity.Collections;
using Unity.Jobs;
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

        private List<NodeView> _largeNodeViews = new List<NodeView>();
        private List<NodeView>_nodeViews = new List<NodeView>();
        private NativeArray<float> _rads;

        private bool _created = false;

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
                depthLevel = 7;
            }
            else if (zone == 1)
            {
                depthLevel = 9;
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

                if (!_created)
                {
                    CreateNativeArray();
                }
                ExecuteAJob();
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
            Queue<NodeView> queue = new Queue<NodeView>();
            
            //List<NodeView> list = new List<NodeView>();
            
            queue.Enqueue(nodeView);

            while (queue.Count > 0)
            {
                /*var element = queue.Dequeue();

                if (!(element.nodeRad >  & ()
                      (nodeView.depth - element.depth) < dDepth)) continue;
                foreach (var child in element.childrenNodes)
                {
                    queue.Enqueue(child);
                }
                
                _largeDistNodeViews.Add(element);*/
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
                if (_largeNodeViews.Count <= i)
                {
                    _currentTextNameLabels[i].textLabel.text = "";
                    yield break;
                }

                _currentTextNameLabels[i].pointCoord = _largeNodeViews[i].pos;
                _currentTextNameLabels[i].textLabel.text = _largeNodeViews[i].sciName;
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

        public void AddNodeView(int id,NodeView nodeView)
        {
            _nodeViews.Add(nodeView);
        }

        private void CreateNativeArray()
        {
            _rads = new NativeArray<float>(_nodeViews.Count, Allocator.Persistent);

            for (var i = 0; i < _nodeViews.Count; i++)
            {
                _rads[i] = _nodeViews[i].nodeRad;
            }

            _created = true;
        }

        private void ExecuteAJob()
        {
            NativeArray<int> bools = new NativeArray<int>(_nodeViews.Count, Allocator.TempJob);
            
            NodeViewJob nodeViewJob = new NodeViewJob{yMax = _dragCam.yMax, camPosY = _cam.transform.position.y, nodesRads = _rads, nodeIsLarge = bools};
            
            var jobHandle = nodeViewJob.Schedule(_nodeViews.Count, 250);
            
            jobHandle.Complete();
            
            _largeNodeViews.Clear();

            for (var i = 0; i < bools.Length; i++)
            {
                if (bools[i] == 1)
                {
                    _largeNodeViews.Add(_nodeViews[i]);
                }
            }
            
            bools.Dispose();
            
        }
        
    
    }

