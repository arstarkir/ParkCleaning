using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class ShovelTool : CoreTool
{
    [SerializeField] TriggerTracker triggerTracker;

    public override void TryUse(InputAction.CallbackContext context)
    {
        if (context.interaction is TapInteraction)
            base.TryUse(context);
    }

    public override void Use()
    {
        animator.SetTrigger("Use");
        List<Collider> hits = triggerTracker.GetContents();
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Tree"))
                continue;
    
        }
    }
}
