using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vRemoveParent : MonoBehaviour
{

    public bool removeOnStart = true;

    private void Start()
    {
        if (removeOnStart)
        {
            RemoveParent();
        }
    }

    public void RemoveParentOfOtherTransform(Transform target)
    {
        target.parent = null;
    }
    public void RemoveParent()
    {
        transform.parent = null;
    }
}
