using UnityEngine;

public static class BezierUtility
{

    public static Vector3 CalculateQuadraticBezierPoint(float _t, Vector3 _start, Vector3 _firtPointOfControl, Vector3 _end)
    {
        float u = 1 - _t;
        float tt = _t * _t;
        float uu = u * u;

        Vector3 p = uu * _start;
        p += 2 * u * _t * _firtPointOfControl;
        p += tt * _end;

        return p;
    }

    public static Vector3 CalculateCubicBezierPoint(float _t, Vector3 _start, Vector3 _firtPointOfControl, Vector3 _secondePointOfControl, Vector3 _end)
    {
        float u = 1 - _t;
        float tt = _t * _t;
        float uu = u * u;
        float ttt = tt * _t;
        float uuu = uu * u;

        Vector3 p = uuu * _start; 
        p += 3 * uu * _t * _firtPointOfControl; 
        p += 3 * u * tt * _secondePointOfControl; 
        p += ttt * _end;

        return p;
    }
}