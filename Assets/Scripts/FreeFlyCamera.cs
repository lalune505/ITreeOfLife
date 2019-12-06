using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFlyCamera : MonoBehaviour {

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

    void Start()
    {
        _camera = Camera.main;
    }

	void Update ()
    {
       /* if (Input.GetKey(Forward))
        {
            _camera.orthographicSize -= .01f;
            _camera.orthographicSize = Mathf.Max(_camera.orthographicSize, minZoom);
        }
        if (Input.GetKey(Backward))
        {
            _camera.orthographicSize += .01f;
            _camera.orthographicSize = Mathf.Min(_camera.orthographicSize, maxZoom);   
        }*/
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            ZoomOrthoCamera(_camera.ScreenToWorldPoint(Input.mousePosition), 0.1f);
        
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
            ZoomOrthoCamera(_camera.ScreenToWorldPoint(Input.mousePosition), -0.1f);
        
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
    
    private void ZoomOrthoCamera(Vector3 desiredPosition, float amount)
    {
        float prevCameraOrthoSize = _camera.orthographicSize;
        float cameraOrthoSize = Mathf.Clamp(prevCameraOrthoSize - amount, minZoom, maxZoom);
        _camera.orthographicSize = cameraOrthoSize;
        
        desiredPosition.z = _camera.transform.position.z;
        if ((_camera.orthographicSize - prevCameraOrthoSize) != 0)
        {
            _camera.transform.position = Vector3.MoveTowards(_camera.transform.position, desiredPosition,
                Time.deltaTime * speed);
        }
    }

}
