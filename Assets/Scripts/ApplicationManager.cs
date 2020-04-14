using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField]
    private List<InitializableMonoBehaviour> _initializables;

    private async void Start()
    {
        foreach (var manager in _initializables)
        {
            if (manager != null)
            {
                await manager.Init();
            }
        }
    }
}
