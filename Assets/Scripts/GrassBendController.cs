using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBendController : MonoBehaviour
{
    public Material grassMaterial;
    public Transform bendingObject;

    void Update()
    {
        if (grassMaterial != null && bendingObject != null)
        {
            Vector3 bendOrigin = bendingObject.position;
            grassMaterial.SetVector("_trackerPosition", bendOrigin);
        }
    }
}
