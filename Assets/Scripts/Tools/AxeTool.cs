using System.Collections.Generic;
using UnityEngine;

public class AxeTool : CoreTool
{
    [SerializeField] TriggerTracker triggerTracker;

    public override void Use()
    {
        animator.SetTrigger("Use");
        List<Collider> hits = triggerTracker.GetContents();
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Tree"))
                continue;
            hit.GetComponent<Health>().RequestChangeHealthServerRpc(-dmg);
        }
    }
}
