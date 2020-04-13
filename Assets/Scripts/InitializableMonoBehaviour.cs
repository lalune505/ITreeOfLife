using System.Collections;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;

public abstract class InitializableMonoBehaviour : MonoBehaviour
{
    public abstract UniTask Init();
}

