using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeLabelRenderController : MonoBehaviour
{
    public GameObject nodePanel;

    private Text[] textComps;
    private Image[] imageComps;
    private const float LabelGapX = 65.0f;
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
        textComps = nodePanel.GetComponentsInChildren<Text>(true);
        imageComps = nodePanel.GetComponentsInChildren<Image>(true);
    }

    public void DisableNodePanel()
    {
        EnableRenderer(false);
    }

    public void EnableNodePanel(Vector3 worldPosition)
    {
        var worldToScreenPoint = camera.WorldToScreenPoint(worldPosition);
        worldToScreenPoint.x += LabelGapX;
        worldToScreenPoint.z = 0.0f;
        nodePanel.transform.position = worldPosition;
        
        EnableRenderer(true);
    }

    private void EnableRenderer(bool enable)
    {
        foreach (var item in textComps)
        {
            item.enabled = enable;
        }
        foreach (var item in imageComps)
        {
            item.enabled = enable;
        }
    }
    
    
}
