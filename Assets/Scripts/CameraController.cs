using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float speed = 1;

    public KeyCode Forward = KeyCode.Space,
        Backward = KeyCode.LeftShift,
        Up = KeyCode.W,
        Down = KeyCode.S,
        Left = KeyCode.A,
        Right = KeyCode.D;
    
    private Camera _camera;
    private float minZoom = 0.1f;
    private float maxZoom = 3f;
    private Vector3 _dragOrigin;

    void Start()
    {
        _camera = Camera.main;
    }

	void Update ()
    {
        if (Input.GetKey(Forward))
        {
            _camera.orthographicSize -= .2f;
            _camera.orthographicSize = Mathf.Max(_camera.orthographicSize, minZoom);
        }
        if (Input.GetKey(Backward))
        {
            _camera.orthographicSize += .2f;
            _camera.orthographicSize = Mathf.Min(_camera.orthographicSize, maxZoom);   
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
           ZoomOrthoCamera(_camera.ScreenToWorldPoint(Input.mousePosition), .2f);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ZoomOrthoCamera(_camera.ScreenToWorldPoint(Input.mousePosition), -.2f);
        }
        if ( Input.GetMouseButtonDown(0)){
           _dragOrigin =  new Vector3 (Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.z);
           _dragOrigin = _camera.ScreenToWorldPoint(_dragOrigin);
        }
 
        if ( Input.GetMouseButton(0)){
            var currentPos =  new Vector3 (Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.z);
            currentPos = _camera.ScreenToWorldPoint(currentPos);
            var movePos = _dragOrigin - currentPos;
            transform.position += movePos;
        }
        if (Input.GetKey(Right))
            transform.position = Vector3.MoveTowards(transform.position,
                transform.position + transform.right,
                Time.deltaTime * speed);
        if (Input.GetKey(Left))
            transform.position = Vector3.MoveTowards(transform.position,
                transform.position - transform.right,
                Time.deltaTime * speed);
        if (Input.GetKey(Up))
            transform.position = Vector3.MoveTowards(transform.position,
                transform.position + transform.up,
                Time.deltaTime * speed);
        if (Input.GetKey(Down))
            transform.position = Vector3.MoveTowards(transform.position,
                transform.position - transform.up,
                Time.deltaTime * speed);
    }
    
    void ZoomOrthoCamera(Vector3 zoomTowards, float amount)
    {
       var prevCameraOrthoSize = _camera.orthographicSize;
       
        _camera.orthographicSize -= amount;
        
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, minZoom, maxZoom);
        
        float multiplier = 1.0f / _camera.orthographicSize * (prevCameraOrthoSize -_camera.orthographicSize);

        zoomTowards.z = _camera.transform.position.z;
        
        transform.position += (zoomTowards - transform.position) * multiplier;
        
    }

}
