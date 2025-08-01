using System.Collections.Generic;
using UnityEngine;

public class TriggerTracker : MonoBehaviour
{
    List<Collider> inside = new List<Collider>();

    void OnTriggerEnter(Collider other)
    {
        if (!inside.Contains(other))
            inside.Add(other);
    }

    void OnTriggerExit(Collider other)
    {
        inside.Remove(other);
    }

    public List<Collider> GetContents()
    {
        return inside;
    }
}
