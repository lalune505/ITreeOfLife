
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [HideInInspector]
    public bool m_SceneReadyToActivate;
    RawImage m_SpinnerImage;

    void Start()
    {
        m_SpinnerImage = GetComponentInChildren<RawImage>();
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    void Update()
    {
        if (m_SpinnerImage)
        {
            if (!m_SceneReadyToActivate)
            {
                m_SpinnerImage.rectTransform.Rotate(Vector3.forward, 2f * 90.0f * Time.deltaTime); 
            }
            else
            {
                m_SpinnerImage.enabled = false;
            }
        }
    }

}
