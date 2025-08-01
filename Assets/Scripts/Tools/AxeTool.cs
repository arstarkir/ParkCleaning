using System.Collections.Generic;
using Unity.Netcode;
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
            if (hit.TryGetComponent<Health>(out Health health))
                health.RequestChangeHealthServerRpc(-dmg);
            if (hit.TryGetComponent<MultiHealth>(out MultiHealth mHealth))
                mHealth.RequestChangeHealthServerRpc(hit.transform.GetComponentInParent<NetworkObject>(), -dmg, hit.name);
        }
    }
}
