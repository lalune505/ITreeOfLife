using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branch
{
    private Vector3 _startPoint;
    private Vector3 _endPoint;

    public Branch(Vector3 point1, Vector3 point2)
    {
        this._startPoint = point1;
        this._endPoint = point2;
    }
}
