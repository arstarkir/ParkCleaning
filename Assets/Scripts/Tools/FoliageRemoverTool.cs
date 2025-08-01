using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoliageRemoverTool : CoreTool
{
    [SerializeField] TriggerTracker triggerTracker;
    public float maxDist = 2;

    Health curFoliage;

    public override void TryUse(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer || !Ready)
            return;

        if (context.ReadValue<float>() == 1)
            animator.SetBool("isUse", true);
        else
            animator.SetBool("isUse", false);

        base.TryUse(context);
    }

    public override void Use()
    {
        bool isUse = animator.GetBool("isUse");

        if(!isUse)
        {
            curFoliage = null;
            return;
        }

        List<Collider> hits = triggerTracker.GetContents();
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Foliage"))
                continue;
            curFoliage = hit.GetComponent<Health>();
        }

        if(curFoliage == null)
            animator.SetBool("isUse", false);
    }

    public override void Update()
    {
        base.Update();

        if (curFoliage != null)
        {
            Debug.Log(Vector3.Distance(curFoliage.transform.position, transform.position));
            curFoliage.RequestChangeHealthServerRpc(-dmg * Time.deltaTime);
        }
    }
}
