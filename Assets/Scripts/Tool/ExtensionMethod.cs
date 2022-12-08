using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod
{
    private const float dotThreshold = 0.5f;//����ֵcos60
    public static bool IsFacingTarget(this Transform transform, Transform target)//��transform������չ
    {
        var vectorToTarget = target.transform.position - transform.position;
        vectorToTarget.Normalize();
        float dot = Vector3.Dot(vectorToTarget, transform.forward);
        return dot >= dotThreshold;
    }
}
