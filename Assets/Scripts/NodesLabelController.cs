using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Async;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

    public class NodesLabelController : InitializableMonoBehaviour
    {
        public ImageManager imageManager;
        public bool updNodes = false;

        public class NodeLabel
        { 
            public Text textLabel;
            public RawImage image;
            public Image mask;
            public Vector3 pointCoord;
            public CanvasGroup alphaControl;
            public GameObject gameObject;
            
            private readonly Color _hiddenColor = new Color(1f,1f,1f,0f);
            private readonly Color _defaultColor = new Color(1f,1f,1f,0.6f);

            public void HideImage()
            {
                mask.color = _hiddenColor;
            }

            public void ShowImage()
            {
                mask.color = _defaultColor;
            }
        }
        
        private Camera _cam;
        private int _depthLevel = 11;
            
        private NodeLabel[] _currentLabels;

        public GameObject labelPrefab;

        public SceneData sceneData;

        private float _updatePause = 1f;
        
        private DragCamera _dragCam;
        
        private readonly int dDepth = 3;

        private bool _created = false;
        
        private List<NodeView> _nodeViews;
        private List<NodeView> _visibleNodeViews = new List<NodeView>();
        
        private JobHandle _sizesJobHandle;
        private CheckSizeNodeViewJob _checkSizeNodeViewJob;
        private NativeArray<int> _nodeViewsSizes;
        private NativeArray<float> _rads;
        private NativeArray<float3> _poses;

        public override async UniTask Init()
        {
            _nodeViews = sceneData.nodeViews;
            _dragCam = FindObjectOfType<DragCamera>();
            
            _cam = _dragCam.GetComponent<Camera>();

            _dragCam.OnCameraZoneChanged.AddListener(OnCamZoneChanged);
            
            NetworkManager.CheckConnection();

            _currentLabels = new NodeLabel[60];

            for (var i = 0; i < _currentLabels.Length; i++)
            {
                var nodeNameLabel = new NodeLabel();

                var label = Instantiate(labelPrefab);
                label.transform.localScale = Vector3.one;

                nodeNameLabel.alphaControl = label.GetComponentInChildren<CanvasGroup>();
                nodeNameLabel.textLabel = label.GetComponentInChildren<Text>();
                nodeNameLabel.mask = label.GetComponentInChildren<Image>();
                nodeNameLabel.image = label.GetComponentInChildren<RawImage>();
                nodeNameLabel.gameObject = label;

                _currentLabels[i] = nodeNameLabel;
            }

            await UniTask.Yield();

        }
        

        private void OnCamZoneChanged(int zone)
        {
            if (zone == 0)
            {
                _depthLevel = 7;
            }
            else if (zone == 1)
            {
                _depthLevel = 9;
            }
            else if (zone == 2)
            {
                _depthLevel = 11;
            }
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
                ExecuteJobs();
            }

            foreach (var roadNameLabel in _currentLabels)
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

            _visibleNodeViews.Clear();
            
            queue.Enqueue(nodeView);

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();

                if (!(element.nodeRad > math.lerp(0.0001f, 1f, 0.4 * _cam.transform.position.y / _dragCam.yMax)))
                { continue;}
                foreach (var child in element.childrenNodes)
                {
                    queue.Enqueue(child);
                }
                
                _visibleNodeViews.Add(element);
            }


            yield return null;
            
        }


        private void UpdateRoadNamesLabels()
        {
            StartCoroutine(UpdateRoadNamesLabels_Process());
        }

        IEnumerator UpdateRoadNamesLabels_Process()
        {
            for (int i = 0; i < _currentLabels.Length; i++)
            {
                if (_visibleNodeViews.Count <= i)
                {
                    _currentLabels[i].textLabel.text = "";
                    _currentLabels[i].HideImage();
                    continue;
                }

                if (_currentLabels[i].textLabel.text != _visibleNodeViews[i].sciName)
                {
                    _currentLabels[i].pointCoord = _visibleNodeViews[i].pos;
                    _currentLabels[i].textLabel.text = _visibleNodeViews[i].sciName;
                    imageManager.GetPage(_visibleNodeViews[i].sciName, _currentLabels[i]);
                }
            }

            yield return null;
        }


        private void UpdateLabelsScreenPositions()
        {
            foreach (var label in _currentLabels)
            {
                label.gameObject.transform.position = new Vector3(label.pointCoord.x, 
                    label.gameObject.transform.position.y, label.pointCoord.z);
            }
        }

        public void AddNodeView(NodeView nodeView)
        {
            _nodeViews.Add(nodeView);
        }

        private void CreateNativeArray()
        {
            _rads = new NativeArray<float>(_nodeViews.Count, Allocator.Persistent);
            _poses = new NativeArray<float3>(_nodeViews.Count, Allocator.Persistent);

            for (var i = 0; i < _nodeViews.Count; i++)
            {
                _rads[i] = _nodeViews[i].nodeRad;
                _poses[i] = _cam.WorldToViewportPoint(_nodeViews[i].pos);
            }

            _created = true;
        }

        private void ExecuteJobs()
        {
             var t = _cam.transform.position.y / _dragCam.yMax;
            _nodeViewsSizes = new NativeArray<int>(_nodeViews.Count, Allocator.TempJob);

            _checkSizeNodeViewJob = new CheckSizeNodeViewJob{t = t, nodesRads = _rads, positions = _poses, nodesSizes = _nodeViewsSizes};
            
            _sizesJobHandle = _checkSizeNodeViewJob.Schedule(_nodeViews.Count, 255);
            
            _sizesJobHandle.Complete();
            
            _visibleNodeViews.Clear();

            for (var i = 0; i < _nodeViewsSizes.Length; i++)
            {
                if (_nodeViewsSizes[i] == 1)
                {
                    _visibleNodeViews.Add(_nodeViews[i]);
                }
            }
            _nodeViewsSizes.Dispose();
            UpdateRoadNamesLabels();

        }

        private void OnDestroy()
        {
            if (_created)
            {
                _rads.Dispose();
                _poses.Dispose();
            }
        }
    }

