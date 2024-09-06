using System.Numerics;
using UnityEngine;

public static class MathsExtension
{
    public static UnityEngine.Vector2 GetPointOnCircle(this UnityEngine.Vector2 center, float radius, float time)
    {
        float x = center.x + radius * Mathf.Cos(time);
        float y = center.y + radius * Mathf.Sin(time);

        return new UnityEngine.Vector2(x, y);
    }

    /// <summary>
    /// Give a vector3, it won't take the y component 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="time"></param>
    /// <returns>return vector3 with Y component equals to 0</returns>
    public static UnityEngine.Vector3 GetPointOnCircle(this UnityEngine.Vector3 center, float radius, float time)
    {
        float x = center.x + radius * Mathf.Cos(time);
        float y = center.z + radius * Mathf.Sin(time);

        return new UnityEngine.Vector3(x, 0, y);
    }
    public static UnityEngine.Vector2 GetRandomPointOnCircle(this UnityEngine.Vector2 center, float radius)
    {
        float randomValue = Random.Range(0, 2 * Mathf.PI);
        return new UnityEngine.Vector2(center.x + Mathf.Cos(randomValue) * radius, center.y + Mathf.Sin(randomValue) * radius);
    }
    /// <summary>
    /// Give a vector3, it won't take the y component 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns>return random point on a circle with Y component equals to 0</returns>
    public static UnityEngine.Vector3 GetRandomPointOnCircle(this UnityEngine.Vector3 center, float radius)
    {
        float randomValue = Random.Range(0, 2 * Mathf.PI);
        return new UnityEngine.Vector3(center.x + Mathf.Cos(randomValue) * radius, 0, center.y + Mathf.Sin(randomValue) * radius);
    }
    public static UnityEngine.Vector2 GetRandomPointInCircle(this UnityEngine.Vector2 center, float maxRadius)
    {
        float randomValue = Random.Range(0, 2 * Mathf.PI);
        float radius = Random.Range(0, maxRadius);
        return new UnityEngine.Vector2(center.x + Mathf.Cos(randomValue) * radius, center.y + Mathf.Sin(randomValue) * radius);
    }
    public static UnityEngine.Vector3 GetRandomPointInCircle(this UnityEngine.Vector3 center, float maxRadius)
    {
        UnityEngine.Vector2 center2D = new(center.x, center.z);
        UnityEngine.Vector2 point2D = GetRandomPointInCircle(center2D, maxRadius);
        UnityEngine.Vector3 toReturn = new(point2D.x, center.y, point2D.y);
        return toReturn;
    }

    public static UnityEngine.Vector2 GetRandomPointInCircle(this UnityEngine.Vector2 center, float minRadius, float maxRadius)
    {
        float randomValue = Random.Range(0, 2 * Mathf.PI);
        float radius = Random.Range(minRadius, maxRadius);
        return new UnityEngine.Vector2(center.x + Mathf.Cos(randomValue) * radius, center.y + Mathf.Sin(randomValue) * radius);
    }
    public static UnityEngine.Vector3 GetRandomPointInCircle(this UnityEngine.Vector3 center, float minRadius, float maxRadius)
    {
        UnityEngine.Vector2 center2D = new(center.x, center.z);
        UnityEngine.Vector2 point2D = GetRandomPointInCircle(center2D, minRadius, maxRadius);
        UnityEngine.Vector3 toReturn = new(point2D.x, center.y, point2D.y);
        return toReturn;
    }
    /// <summary>
    /// Give angle in degree
    /// </summary>
    /// <param name="center"></param>
    /// <param name="direction"></param>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <returns>Random point on a cone in the given direction</returns>
    public static UnityEngine.Vector2 GetRandomPointOnCone(this UnityEngine.Vector2 center, UnityEngine.Vector2 direction, float radius, float angle)
    {
        if (direction == UnityEngine.Vector2.zero)
        {
            Debug.LogError("Impossible to find a point because direction is a vector zero");
            return UnityEngine.Vector2.zero;
        }
        float c = Mathf.Acos(direction.x / Mathf.Sqrt(direction.x * direction.x + direction.y * direction.y));
        float s = Mathf.Asin(direction.y / Mathf.Sqrt(direction.x * direction.x + direction.y * direction.y));
        float cs;
        float radAngle = angle * Mathf.Deg2Rad;
        if (s < 0)
        {
            if (c < Mathf.PI / 2)
                cs = s;
            else
                cs = Mathf.PI - s;
        }
        else
        {
            cs = c;
        }
        float randomValue = Random.Range(-radAngle + cs, radAngle + cs);
        return new UnityEngine.Vector2(center.x + Mathf.Cos(randomValue) * radius, center.y + Mathf.Sin(randomValue) * radius);
    }

    /// <summary>
    /// Give angle in degree
    /// </summary>
    /// <param name="center"></param>
    /// <param name="direction"></param>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <returns>Random point on a cone in the given direction</returns>
    public static UnityEngine.Vector3 GetRandomPointOnCone(this UnityEngine.Vector3 center, UnityEngine.Vector3 direction, float radius, float angle)
    {
        UnityEngine.Vector2 center2D = new(center.x, center.z);
        UnityEngine.Vector2 direction2D = new(direction.x, direction.z);
        if(direction2D == UnityEngine.Vector2.zero) direction2D = UnityEngine.Vector2.one;
        UnityEngine.Vector2 point2D = GetRandomPointOnCone(center2D, direction2D, radius, angle);
        UnityEngine.Vector3 toReturn = new(point2D.x, center.y, point2D.y);
        return toReturn;
    }

    public static float[] Resolve2ndDegree(float a, float b, float c, float wantedY)
    {
        c -= wantedY;
        float delta = b * b - 4 * a * c;
        float[] results = new float[2];
        if (delta == 0)
        {
            results[0] = (float)-b / (2 * a);
        }
        else if (delta > 0)
        {
            results[0] = (float)(-b + Mathf.Sqrt(delta)) / (2 * a);
            results[1] = (float)(-b - Mathf.Sqrt(delta)) / (2 * a);
        }
        else
        {
            Debug.LogWarning("No result in Real number");
            return results;
        }
        return results;
    }

    public static float SquareFunction(float a, float b, float c, float timer)
    {
        return a * timer * timer + b * timer + c;
    }
}
