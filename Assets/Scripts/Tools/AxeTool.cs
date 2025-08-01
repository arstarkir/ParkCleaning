using System.Collections.Generic;
using UnityEngine;

public class AxeTool : CoreTool
{
    public float dmg = 3;

    List<Collider> inside = new List<Collider>();

    public override void Use()
    {
        foreach (Collider hit in inside)
        {
            if (!hit.CompareTag("tree"))
                continue;
            hit.GetComponent<Health>().RequestChangeHealthServerRpc(-dmg);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!inside.Contains(other))
            inside.Add(other);
    }

    void OnTriggerExit(Collider other)
    {
        inside.Remove(other);
    }
}
