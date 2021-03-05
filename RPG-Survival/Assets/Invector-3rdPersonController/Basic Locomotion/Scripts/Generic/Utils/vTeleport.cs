using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vTeleport : MonoBehaviour
{
    public Transform targetPoint;
    public bool includeRoot;
    public bool rotateToTargetForward = true;
    public void Teleport(Collider collider)
    {
        var localPosition = transform.InverseTransformPoint(includeRoot?collider.transform.root.position: collider.transform.position);
        var localForward = transform.InverseTransformDirection(includeRoot ? collider.transform.root.forward : collider.transform.forward);
        localPosition.Set(0, localPosition.y, 0);
        if(includeRoot)
        {
            collider.transform.root.position = targetPoint.TransformPoint(localPosition);
            if (rotateToTargetForward)
                collider.transform.root.rotation = targetPoint.rotation;
            else
                collider.transform.root.forward = targetPoint.TransformDirection(localForward);
        }
        else
        {
            collider.transform.position = targetPoint.TransformPoint(localPosition);
            if (rotateToTargetForward)
                collider.transform.rotation = targetPoint.rotation;
            else
                collider.transform.forward = targetPoint.TransformDirection(localForward);
        }
    }
}
