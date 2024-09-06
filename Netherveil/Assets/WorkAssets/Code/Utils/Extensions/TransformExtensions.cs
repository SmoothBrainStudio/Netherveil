using UnityEngine;

public static class TransformExtensions
{
    public static Vector3 Center(this Transform transform)
    {
        Vector3 sumVector = new Vector3(0f, 0f, 0f);

        foreach (Transform child in transform)
        {
            if (child.transform.childCount > 0)
            {
                sumVector += child.transform.Center();
            }
            else
            {
                sumVector += child.transform.position;
            }
        }

        return transform.childCount == 0 ? Vector3.zero : sumVector / transform.childCount;
    }

    public static void SetLayerAllChildren(this GameObject gameObject, int layer, bool includeInactive = false)
    {
        gameObject.layer = layer;

        var children = gameObject.transform.GetComponentsInChildren<Transform>(includeInactive);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    /// <summary>
    /// Calculates the angle between the launcher and target to be face to face.
    /// You can add an angle threshold to do the test with a cone that matches the angle passed as parameter
    /// </summary>
    /// <returns> angle you need to add to the launcher's rotation to be oriented in front of the target if succeeded, float.maxValue otherwise.</returns>
    public static float AngleOffsetToFaceTarget(this Transform launcherTransform, Vector3 targetPos, float angleThreshold = 360, float rangeThreshold = float.MaxValue)
    {
        Vector3 launcherToTargetVec = targetPos - launcherTransform.position;
        float angle = Vector3.Angle(launcherToTargetVec, launcherTransform.forward);

        if (angle <= angleThreshold && angle > float.Epsilon && launcherToTargetVec.magnitude <= rangeThreshold)
        {
            //vector that describes the enemy's position offset from the player's position along the player's left/right, up/down, and forward/back axes
            Vector3 targetLocalPosFromLauncher = launcherTransform.InverseTransformPoint(targetPos);

            //Left side of launcher
            if (targetLocalPosFromLauncher.x < 0)
            {
                return -angle;
            }
            //Right side of launcher
            else
            {
                return angle;
            }
        }

        //target isnt in the angle
        return float.MaxValue;
    }
}
