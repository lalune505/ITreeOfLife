using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private const string NodeElementLayerName = "Node";
    
    private readonly Dictionary<int, IUserInteractable> _inputRedirects
        = new Dictionary<int, IUserInteractable>();

    private NodePanelsManager nodePanelsManager;
    private Camera camera;
    private LayerMask layersToHit;
    private static bool _inputEnabled;

    private void Awake()
    {
        nodePanelsManager = FindObjectOfType<NodePanelsManager>();
        FillInputRedirects();
        camera = Camera.main;
        
        layersToHit = LayerMask.GetMask(
            _inputRedirects
                .Keys
                .Select(LayerMask.LayerToName)
                .ToArray()
        );
    }

    private void Start()
    {
        EnableInput();
    }

    private void Update()
    {
        if (!_inputEnabled)
        {
            return;
        }
        HandleRayCasts();
        HandleRayCastsWithClick();
    }

    private void FillInputRedirects()
    {
        _inputRedirects.Add(
            LayerMask.NameToLayer(NodeElementLayerName), nodePanelsManager);
    }
    
    private void HandleRayCasts()
    {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layersToHit))
        {
            RedirectInputNotOccur();
            return;
        }
            
        RedirectInputOccur(hit);
    }

    private void HandleRayCastsWithClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layersToHit))
            {
                RedirectInputClickOccur(hit);
            }
        }
    }
        

    private void RedirectInputOccur(RaycastHit hit)
    {
        _inputRedirects[hit.collider.gameObject.layer]?.HandleInputOccur(hit);
    }

    private void RedirectInputClickOccur(RaycastHit hit)
    {
        _inputRedirects[hit.collider.gameObject.layer]?.HandleInputClickOccur();
    }

    private void RedirectInputNotOccur()
    {
        _inputRedirects.Values.ToList().ForEach(v => v.HandleInputNotOccur());
    }

    public static void EnableInput()
    {
        _inputEnabled = true;
    }
    
    public static void DisableInput()
    {
        _inputEnabled = false;
    }

}
