using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NodePanelsManager : MonoBehaviour, IUserInteractable
{
    public NodeLabelController nodeLabelController;
    public NodeLabelRenderController nodeLabelRenderController;
    private NodeView _selectedNodeView;
    private Collider _lastHitCollider;
    
    public void HandleInputOccur(RaycastHit hit)
    {
        EnableNodePanel(hit);
    }

    public void HandleInputClickOccur()
    {
       MoveCameraToSelectedNode();
    }

    public void HandleInputNotOccur()
    {
        DisableNodePanel();
    }

    private void EnableNodePanel(RaycastHit hit)
    {
        if (_selectedNodeView == null ||
            _lastHitCollider != null && _lastHitCollider != hit.collider)
        {
            _selectedNodeView = hit.transform.GetComponent<NodeView>();
            _lastHitCollider = hit.collider;

            nodeLabelRenderController.EnableNodePanel(hit.point);
            nodeLabelController.UpdateLabelVisuals(
                _selectedNodeView.nodeId.ToString(),
                _selectedNodeView.sciName);
        }
    }

    private void DisableNodePanel()
    {
        if (_selectedNodeView == null)
        {
            return;
        }
        
        nodeLabelRenderController.DisableNodePanel();
        _selectedNodeView = null;
    }

    private void MoveCameraToSelectedNode()
    {
        if (_selectedNodeView == null) return;
        var nodeTransform = _selectedNodeView.transform;

        if (nodeTransform != null)
        {
            //TODO: camera.MoveToSelectedNode();
        }
    }
}
