using UnityEngine;
public static class Vector3Extensions
{
    public static bool IsAllValuesEqual(this Vector3 vector)
    {
        return vector.x == vector.y &&
            vector.x == vector.z &&
            vector.y == vector.z;
    }

    public static Vector3 ToAbs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    /// <summary>
    /// Used to get the directions of camera without the y axis so that the player doesnt move on this axis and renormalize the vectors because of that modification
    /// </summary>
    /// <param name="camRight"></param>
    /// <param name="camForward"></param>
    public static void ModifyCamVectors(out Vector3 camRight, out Vector3 camForward)
    {
        camForward = Camera.main.transform.forward;
        camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward = camForward.normalized;
        camRight = camRight.normalized;
    }

    /// <param name="vector"></param>
    /// <returns>Vector2 that contains x and z which is based on camera's orientation.</returns>
    public static Vector2 ToCameraOrientedVec2(this Vector3 vector)
    {
        ModifyCamVectors(out Vector3 camRight, out Vector3 camForward);
        Vector3 tmp = (camForward * vector.z + camRight * vector.x);
        return new Vector2(tmp.x, tmp.z);
    }

    /// <param name="vector"></param>
    /// <returns>Vector3 based on camera's orientation.</returns>
    public static Vector3 ToCameraOrientedVec3(this Vector3 vector)
    {
        ModifyCamVectors(out Vector3 camRight, out Vector3 camForward);
        return camForward * vector.z + camRight * vector.x;
    }

    /// <param name="vector"></param>
    /// <returns>Vector3 based on camera's orientation.</returns>
    public static Vector3 ToCameraOrientedVec3(this Vector2 vector)
    {
        ModifyCamVectors(out Vector3 camRight, out Vector3 camForward);
        return camForward * vector.y + camRight * vector.x;
    }

    /// <summary>
    /// Function to rotate a 3D Point around the Y axis
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    //public static Vector3 RotatePointAroundYAxis(this Vector3 vector, float angleDegrees)
    //{
    //    float angleRadians = angleDegrees * Mathf.Deg2Rad;

    //    // using 2x2 rotation matrix to multiply with x,z vector
    //    //[cos(teta), -sin(teta)] * [x]
    //    //[sin(teta), cos(teta) ]   [z]
    //    float newX = vector.x * Mathf.Cos(angleRadians) - vector.z * Mathf.Sin(angleRadians);
    //    float newZ = vector.x * Mathf.Sin(angleRadians) + vector.z * Mathf.Cos(angleRadians);

    //    return new Vector3(newX, vector.y, newZ);
    //}

    /// <param name="vector"></param>
    /// <param name="nbDigits"></param>
    /// <returns>Rounded values of the original vector based on the number of digits passed as parameter.</returns>
    public static Vector3 ToRound(this Vector3 vector, byte nbDigits)
    {
        float multiplier = Mathf.Pow(10, nbDigits);
        return new Vector3
        (
            Mathf.Round(vector.x * multiplier) / multiplier,
            Mathf.Round(vector.y * multiplier) / multiplier,
            Mathf.Round(vector.z * multiplier) / multiplier
        );
    }
}
