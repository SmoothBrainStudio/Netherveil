using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public static class PhysicsExtensions
{
    const float CAST_THRESHOLD = 0.01f;

    static void GetBoxParameters(this BoxCollider collider, out Vector3 adjustedHalfExtents, out Vector3 center)
    {
        //get the half extents with the real size by considering the scale of the object
        adjustedHalfExtents = Vector3.Scale(collider.size * 0.5f, collider.transform.localScale.ToAbs());

        // Get the position of the BoxCollider in world space
        center = collider.transform.TransformPoint(collider.center);
    }

    /// <summary>
    /// Used To Get Colliders from an overlap from an existing box collider.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="layerMask"></param>
    /// <param name="queryTriggerInteraction"></param>
    /// <returns></returns>
    public static Collider[] BoxOverlap(this BoxCollider collider, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        GetBoxParameters(collider, out Vector3 adjustedHalfExtents, out Vector3 center);
        return Physics.OverlapBox(center, adjustedHalfExtents, collider.transform.rotation, layerMask, queryTriggerInteraction);
    }

    public static RaycastHit[] BoxCastAll(this BoxCollider collider, float maxDistance = CAST_THRESHOLD, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        GetBoxParameters(collider, out Vector3 adjustedHalfExtents, out Vector3 center);
        return Physics.BoxCastAll(center, adjustedHalfExtents, collider.transform.forward, collider.transform.rotation, maxDistance, layerMask, queryTriggerInteraction);
    }

    /// <summary>
    /// Used to get colliders from a box overlap with ray check to know if there is obstacles in from of targets, based on an existing box collider.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="targetTag"></param>
    /// <param name="tagToIgnore"></param>
    /// <param name="layerMask"></param>
    /// <param name="queryTriggerInteraction"></param>
    /// <returns></returns>
    public static Collider[] BoxOverlapWithRayCheck(this BoxCollider collider, Vector3 rayOrigin, string targetTag, int obstacleLayer = -1, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        Collider[] colliders = collider.BoxOverlap(layerMask, queryTriggerInteraction);
        List<Collider> targets = new List<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.CompareTag(targetTag))
            {
                targets.Add(col);
            }
        }

        return targets.Count > 0 ? GetCollidersNotBehindObstacles(targets.ToArray(), rayOrigin, obstacleLayer, queryTriggerInteraction) : targets.ToArray();
    }

    static void GetSphereParameters(this SphereCollider collider, out float adjustedRadius, out Vector3 center)
    {
        //get the radius of the object also considering the scale if scale is equal on all values
        adjustedRadius = collider.radius;

        if (collider.transform.localScale.IsAllValuesEqual())
        {
            adjustedRadius *= Mathf.Abs(collider.transform.localScale.x);
        }
        else
        {
            Debug.LogWarning("The scale of your sphere isn't equal on all axis, this will not be taken into account for collide checks.");
        }

        // Get the position of the BoxCollider in world space
        center = collider.transform.TransformPoint(collider.center);
    }

    /// <summary>
    /// Used to get colliders from a sphere overlap from an existing sphere collider.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="layerMask"></param>
    /// <param name="queryTriggerInteraction"></param>
    /// <returns></returns>
    public static Collider[] SphereOverlap(this SphereCollider collider, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        GetSphereParameters(collider, out float adjustedRadius, out Vector3 center);
        return Physics.OverlapSphere(center, adjustedRadius, layerMask, queryTriggerInteraction);
    }

    public static RaycastHit[] SphereCastAll(this SphereCollider collider, float maxDistance = CAST_THRESHOLD, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        GetSphereParameters(collider, out float adjustedRadius, out Vector3 center);
        return Physics.SphereCastAll(center, adjustedRadius, collider.transform.forward, maxDistance, layerMask, queryTriggerInteraction);
    }

    /// <summary>
    /// Used to get colliders from a sphere overlap with ray check to know if there is obstacles in from of targets, based on an existing sphere collider.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="targetTag"></param>
    /// <param name="tagToIgnore"></param>
    /// <param name="layerMask"></param>
    /// <param name="queryTriggerInteraction"></param>
    /// <returns></returns>
    public static Collider[] SphereOverlapWithRayCheck(this SphereCollider collider, Vector3 rayOrigin, string targetTag, int obstacleLayer = -1, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        Collider[] colliders = collider.SphereOverlap(layerMask, queryTriggerInteraction);
        List<Collider> targets = new List<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.CompareTag(targetTag))
            {
                targets.Add(col);
            }
        }

        return targets.Count > 0 ? GetCollidersNotBehindObstacles(targets.ToArray(), rayOrigin, obstacleLayer, queryTriggerInteraction) : targets.ToArray();
    }

    static void GetCapsuleParameters(this CapsuleCollider collider, out float adjustedRadius, out float adjustedHeight, out Vector3 center)
    {
        // Calculate the adjusted radius and height based on the capsule's dimensions
        adjustedRadius = Mathf.Abs(collider.radius * Mathf.Max(collider.transform.localScale.x, collider.transform.localScale.y, collider.transform.localScale.z));
        adjustedHeight = Mathf.Abs(collider.height * collider.transform.localScale.y);

        // Get the position of the CapsuleCollider in world space
        center = collider.transform.TransformPoint(collider.center);
    }

    /// <summary>
    /// Used to get colliders from a capsule overlap from an existing capsule collider.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="layerMask"></param>
    /// <param name="queryTriggerInteraction"></param>
    /// <returns></returns>
    public static Collider[] CapsuleOverlap(this CapsuleCollider collider, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        GetCapsuleParameters(collider, out float adjustedRadius, out float adjustedHeight, out Vector3 center);
        return Physics.OverlapCapsule(center - new Vector3(0f, adjustedHeight / 2f - adjustedRadius, 0f),
                                      center + new Vector3(0f, adjustedHeight / 2f - adjustedRadius, 0f),
                                      adjustedRadius, layerMask, queryTriggerInteraction);
    }

    public static RaycastHit[] CapsuleCastAll(this CapsuleCollider collider, float maxDistance = CAST_THRESHOLD, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        GetCapsuleParameters(collider, out float adjustedRadius, out float adjustedHeight, out Vector3 center);
        return Physics.CapsuleCastAll(
            center - new Vector3(0f, adjustedHeight / 2f - adjustedRadius, 0f)
            , center + new Vector3(0f, adjustedHeight / 2f - adjustedRadius, 0f)
            , adjustedRadius, collider.transform.forward, maxDistance, layerMask, queryTriggerInteraction);
    }

    /// <summary>
    /// Used to get colliders from a capsule overlap with ray check to know if there is obstacles in from of targets, based on an existing capsule collider.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="targetTag"></param>
    /// <param name="tagToIgnore"></param>
    /// <param name="layerMask"></param>
    /// <param name="queryTriggerInteraction"></param>
    /// <returns></returns>
    public static Collider[] CapsuleOverlapWithRayCheck(this CapsuleCollider collider, Vector3 rayOrigin, string targetTag, int obstacleLayer = -1, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        Collider[] colliders = collider.CapsuleOverlap(layerMask, queryTriggerInteraction);
        List<Collider> targets = new List<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.CompareTag(targetTag))
            {
                targets.Add(col);
            }
        }

        return targets.Count > 0 ? GetCollidersNotBehindObstacles(targets.ToArray(), rayOrigin, obstacleLayer, queryTriggerInteraction) : targets.ToArray();
    }

    /// <summary>
    /// Find all collider toucing the vision cone.
    /// </summary>
    /// <param name="center">The start position of the cone.</param>
    /// <param name="angle">Cone's angle.</param>
    /// <param name="range">Cone's range.</param>
    /// <param name="forward">Where the cone is facing.</param>
    /// <returns></returns>
    public static Collider[] OverlapVisionCone(Vector3 center, float angle, float range, Vector3 forward, int layer = -1)
    {
        Collider[] result = Physics.OverlapSphere(center, range, layer)
            .Where(x =>
            {
                Vector3 resultPos = x.transform.position - center;
                resultPos.y = 0;
                float toCompare = Vector3.Angle(resultPos, forward);
                return toCompare >= -(angle / 2f) && toCompare <= angle / 2f;
            })
            .ToArray();

        return result;
    }

    static Collider[] GetCollidersNotBehindObstacles(Collider[] targets, Vector3 rayOrigin, int obstacleLayer = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        List<Collider> targetsAheadOfObstacles = new List<Collider>();
        foreach (Collider target in targets)
        {
            Vector3 initialToTargetVec = (target.transform.position - rayOrigin);
            initialToTargetVec.y = 0;
            Ray ray = new Ray(rayOrigin, initialToTargetVec.normalized);

            if (!Physics.Raycast(ray, initialToTargetVec.magnitude, obstacleLayer, queryTriggerInteraction))
            {
                targetsAheadOfObstacles.Add(target);
            }
            //else
            //{
            //    Debug.Log(hit.collider.name, hit.collider.gameObject);
            //    Debug.Log(hit.collider.GetType().ToString());
            //}
        }

        return targetsAheadOfObstacles.ToArray();
    }

    public static Collider[] CheckAttackCollideRayCheck(Collider collider, Vector3 rayOrigin, string targetTag, int obstacleLayer = -1, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
    {
        if (collider != null)
        {
            System.Type colliderType = collider.GetType();

            switch (colliderType.Name)
            {
                case nameof(BoxCollider):
                    return (collider as BoxCollider).BoxOverlapWithRayCheck(rayOrigin, targetTag, obstacleLayer, layerMask, queryTriggerInteraction);
                case nameof(SphereCollider):
                    return (collider as SphereCollider).SphereOverlapWithRayCheck(rayOrigin, targetTag, obstacleLayer, layerMask, queryTriggerInteraction);
                case nameof(CapsuleCollider):
                    return (collider as CapsuleCollider).CapsuleOverlapWithRayCheck(rayOrigin, targetTag, obstacleLayer, layerMask, queryTriggerInteraction);
                default:
                    Debug.LogWarning("Invalid Collider type, can't check the collision.");
                    return new Collider[0];
            }
        }

        Debug.LogWarning("Collider is null.");
        return new Collider[0];
    }
}
