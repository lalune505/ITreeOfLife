using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragCamera : MonoBehaviour
    {
        //View zone change
        [System.Serializable]
        public class CameraZoneChangedEvent : UnityEvent<int> { internal void AddListener() { throw new NotImplementedException(); } };
        public CameraZoneChangedEvent OnCameraZoneChanged;

        public UnityEvent OnCameraStartMove;

        //Zoom [0f, 1f]
        public float zoom = 0f;
        public float zoom01 = 0f;

        //Zoom Y
        public float yMin = 0f;
        public float yMax = 100f;

        //Angles

        [Header("Fly time for cam view points")]
        public float camViewsLerpTime = 2f;
        [Header("For projects points")]
        public float lerpTime = 2f;

        //Fly curves
        public AnimationCurve cameraYAnim;
        
        public Transform[] camMainViewPoints;
        //View point or project point
        public Transform currentTargetPoint;

        [Header("Offset cam pos from project point")]
        public Vector3 _offsetPointCamPos = new Vector3(0f, 40f, -70f);

        //For smooth
       
        public Vector3 goalPosition;

        //Camera drag on map
        Vector3 hit_position = Vector3.zero;
        Vector3 current_position = Vector3.zero;
        Vector3 camera_position = Vector3.zero;

        public bool InputEnabled { private get; set; }
        public bool dragCam = false;

        [Header("Move parabolic if cam start at Y < value")]
        public float partabolaTraectoryHeight = 20f;


        public float moveSpeedOnGround = 100f;
        public float moveSpeedOnSky = 1000f;

        public Transform cameraBoundsCenterPoint;
        public float camBoundsWidth;
        public float camBoundsHeight;
        bool camOutOfBorder = false;

        public enum TargetType
        {
            None,
            CamView,
            ProjectPoint
        }

        public TargetType currentTargetType = TargetType.None;

        public float[] camZonesHeights;
        public int previousZone = 0;
        public int currentZone = 0;

        private bool _waitMoveStart = false;

        public bool canMouseDrag = true;
        public bool useZonesCalculationOnly;

        private Transform _cameraTransform;
        private Transform _savedCameraParent;
        private Vector3 _savedCameraPosition;
        private Quaternion _savedCameraRotation;

        private Coroutine _currentLerpMoveProcess;
        private int _moveStartFrame;
        

        public Camera Camera { get; private set; }

        private void Awake()
        {
            _cameraTransform = transform;
            Camera = GetComponent<Camera>();
        }

        private void Start()
        {
            OnCameraZoneChanged.Invoke(0);
            InputEnabled = true;
        }
        
        void MoveByButtons()
        {
            float xMove = Input.GetAxis("Horizontal");
            float zMove = Input.GetAxis("Vertical");

            if (xMove != 0f || zMove != 0f)
            {
                if (currentTargetPoint != null)
                {
                    currentTargetPoint = null;
                    currentTargetType = TargetType.None;
                    goalPosition = transform.position;
                }

                if (_waitMoveStart)
                {
                    _waitMoveStart = false;
                    OnCameraStartMove.Invoke();
                }
            }
            else if (xMove == 0f && zMove == 0f)
            {
                _waitMoveStart = true;
            }

            var pos = goalPosition;

            pos.x += xMove * Mathf.Lerp(moveSpeedOnSky, moveSpeedOnGround, zoom01);
            pos.z += zMove * Mathf.Lerp(moveSpeedOnSky, moveSpeedOnGround, zoom01);

            pos.y = goalPosition.y;
            goalPosition = pos;
        }


        void Update()
        {
            CalculateCameraZones();
            CheckCameraMoveBounds();

            if (useZonesCalculationOnly || !InputEnabled)
            {
                return;
            }
            
            zoom += Input.GetAxis("Mouse ScrollWheel");
            zoom = Mathf.Clamp(zoom, 0f, 2.5f);
            zoom01 = zoom / 2.5f;
            zoom01 = Mathf.Clamp01(zoom01);

            float lerpSpeed = camOutOfBorder ? Time.deltaTime * 4f : Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, goalPosition, lerpSpeed);
            
            if (currentTargetPoint == null)
            {
                var pos = transform.position;
                pos.y = Mathf.Lerp(yMin, yMax, 1f - zoom01);
                goalPosition = pos;
                
            }

            if (canMouseDrag && _moveStartFrame != Time.frameCount)
            {
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    dragCam = true;

                    hit_position = Input.mousePosition;
                    camera_position = transform.position;

                    if (currentTargetPoint != null)
                    {
                        currentTargetPoint = null;
                        currentTargetType = TargetType.None;
                        goalPosition = transform.position;
                    }

                    if (_currentLerpMoveProcess != null)
                    {
                        StopCoroutine(_currentLerpMoveProcess);
                        _currentLerpMoveProcess = null;
                    }

                    zoom = 2.5f - Map(transform.position.y, yMin, yMax, 0.0f, 2.5f);

                    if (_waitMoveStart)
                    {
                        _waitMoveStart = false;
                        OnCameraStartMove.Invoke();
                    }
                }
                else if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
                {
                    if (_waitMoveStart)
                    {
                        _waitMoveStart = false;
                        OnCameraStartMove.Invoke();
                    }
                }

                if (Input.GetMouseButton(0) && dragCam)
                {
                    current_position = Input.mousePosition;
                    LeftMouseDrag();
                }

                if (Input.GetMouseButtonUp(0) && dragCam)
                {
                    dragCam = false;

                    _waitMoveStart = true;
                }

                MoveByButtons();
            }

            camOutOfBorder = false;
            
        }

        /// <summary>
        ///   <para>Checks if object is inside camera frustum.</para>
        /// </summary>
        /// <param name="pos">Object position in world space.</param>
        /// <param name="boundSize">Object bounds.</param>
        /// <returns>
        ///   <para>Whether the object is visible or not.</para>
        /// </returns>
        public bool IsVisible(Vector3 pos, Vector3 boundSize)
        {
            var bounds = new Bounds(pos, boundSize);
            var planes = GeometryUtility.CalculateFrustumPlanes(Camera);
            
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
        
        public void ChangeParent(Transform target)
        {
            SaveCameraState();
            
            _cameraTransform.parent = target;
            _cameraTransform.localPosition = Vector3.zero;
            _cameraTransform.localRotation = Quaternion.identity;
        }

        public void RestoreParent()
        {
            _cameraTransform.parent = _savedCameraParent;
            _cameraTransform.position = _savedCameraPosition;
            _cameraTransform.rotation = _savedCameraRotation;
        }

        private void SaveCameraState()
        {
            _savedCameraParent = _cameraTransform.parent;
            _savedCameraPosition = _cameraTransform.position;
            _savedCameraRotation = _cameraTransform.rotation;
        }

        void CalculateCameraZones()
        {
            for (int i = 0; i < camZonesHeights.Length - 1; i++)
            {
                if (transform.position.y > camZonesHeights[i] && transform.position.y < camZonesHeights[i + 1])
                {
                    currentZone = i;
                }
            }

            if (currentZone != previousZone)
            {
                OnCameraZoneChanged.Invoke(currentZone);
            }

            previousZone = currentZone;
        }


        public void ZoneChange(int i)
        {
            Debug.Log(i);
        }


        public void MoveToCamViewPoint(int pointID)
        {
            if (_currentLerpMoveProcess != null)
            {
                StopCoroutine(_currentLerpMoveProcess);
            }

            currentTargetType = TargetType.CamView;
            currentTargetPoint = camMainViewPoints[pointID];

            _currentLerpMoveProcess = StartCoroutine(MoveToCameraView_Process(camMainViewPoints[pointID].position));
        }

        IEnumerator MoveToCameraView_Process(Vector3 endPos)
        {
            float currentLerpTime = 0f;
            float time = 0f;
            Vector3 startPos = transform.position;

            while (currentLerpTime < camViewsLerpTime)
            {
                time = currentLerpTime / camViewsLerpTime;

                goalPosition = Vector3.Lerp(startPos, endPos, time);

                currentLerpTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            goalPosition = endPos;
            //currentTargetPoint = null;
            _currentLerpMoveProcess = null;
        }


        public void MoveToProjectPoint(Transform _point)
        {
            if (_currentLerpMoveProcess != null)
            {
                StopCoroutine(_currentLerpMoveProcess);
            }

            currentTargetType = TargetType.ProjectPoint;
            currentTargetPoint = _point;

            _currentLerpMoveProcess = StartCoroutine(MoveToTarget_Process(_point.position + _point.lossyScale.x * 10 * _offsetPointCamPos));
        }


        IEnumerator MoveToTarget_Process(Vector3 endPos)
        {
            _moveStartFrame = Time.frameCount;
            float currentLerpTime = 0f;
            float time = 0f;

            Vector3 startPos = transform.position;

            bool parabola = false;
            float currentGraphY = Mathf.Lerp(yMin, yMax, cameraYAnim.Evaluate(time));

            if (startPos.y <= yMin + partabolaTraectoryHeight)
            {
                parabola = true;
            }

            while (currentLerpTime < lerpTime)
            {
                time = currentLerpTime / lerpTime;

                if (parabola)
                {
                    currentGraphY = Mathf.Lerp(yMin, yMax, cameraYAnim.Evaluate(time));
                }
                else
                {
                    currentGraphY = Mathf.Lerp(startPos.y, endPos.y, time);
                }

                goalPosition = Vector3.Lerp(startPos, endPos + new Vector3(0f, currentGraphY, 0f), time);

                currentLerpTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            goalPosition = endPos;

            //currentTargetPoint = null;
            _currentLerpMoveProcess = null;
        }


        void LeftMouseDrag()
        {
            current_position.z = hit_position.z = camera_position.y;
            Vector3 direction = Camera.main.ScreenToWorldPoint(current_position) - Camera.main.ScreenToWorldPoint(hit_position);

            direction = direction * -1 * (1f + (1f - zoom01));

            Vector3 position = camera_position + direction;
            position.y = transform.position.y;

            transform.position = position;
            goalPosition = position;
        }


        public float Map(float x, float in_min, float in_max, float out_min, float out_max, bool clamp = false)
        {
            if (clamp) x = Mathf.Max(in_min, Mathf.Min(x, in_max));
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        private void OnDrawGizmos()
        {
            if (cameraBoundsCenterPoint != null)
            {
                Gizmos.DrawWireCube(cameraBoundsCenterPoint.position, new Vector3(camBoundsWidth, 1f, camBoundsHeight));
            }
        }

        void CheckCameraMoveBounds()
        {
            if (transform.position.x < cameraBoundsCenterPoint.position.x - camBoundsWidth * 0.5f)
            {
                camOutOfBorder = true;
            }
            if (goalPosition.x < cameraBoundsCenterPoint.position.x - camBoundsWidth * 0.5f)
            {
                goalPosition.x = cameraBoundsCenterPoint.position.x - camBoundsWidth * 0.5f;
            }


            if (transform.position.x > cameraBoundsCenterPoint.position.x + camBoundsWidth * 0.5f)
            {
                camOutOfBorder = true;
            }
            if (goalPosition.x > cameraBoundsCenterPoint.position.x + camBoundsWidth * 0.5f)
            {
                goalPosition.x = cameraBoundsCenterPoint.position.x + camBoundsWidth * 0.5f;
            }

            if (transform.position.z < cameraBoundsCenterPoint.position.z - camBoundsHeight * 0.5f)
            {
                camOutOfBorder = true;
            }
            if (goalPosition.z < cameraBoundsCenterPoint.position.z - camBoundsHeight * 0.5f)
            {
                goalPosition.z = cameraBoundsCenterPoint.position.z - camBoundsHeight * 0.5f;
            }


            if (transform.position.z > cameraBoundsCenterPoint.position.z + camBoundsHeight * 0.5f)
            {
                camOutOfBorder = true;
            }
            if (goalPosition.z > cameraBoundsCenterPoint.position.z + camBoundsHeight * 0.5f)
            {
                goalPosition.z = cameraBoundsCenterPoint.position.z + camBoundsHeight * 0.5f;
            }
        }
        
        
    }
