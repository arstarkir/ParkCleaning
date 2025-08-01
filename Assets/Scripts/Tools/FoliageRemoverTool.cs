using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoliageRemoverTool : CoreTool
{
    [SerializeField] TriggerTracker triggerTracker;
    public float maxDist = 2;

    GameObject curFoliage;

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
            if (!hit.TryGetComponent<MultiHealth>(out MultiHealth mHealth))
                continue;
            foreach (Transform t in mHealth.transform)
                if(t.CompareTag("Foliage"))
                    curFoliage = t.gameObject;
        }

        if(curFoliage == null)
            animator.SetBool("isUse", false);
    }

    public override void Update()
    {
        base.Update();

        if (curFoliage != null)
        {
            //Debug.Log(Vector3.Distance(curFoliage.transform.position, transform.position));
            if (curFoliage.TryGetComponent<Health>(out Health health))
                health.RequestChangeHealthServerRpc(-dmg * Time.deltaTime);
            if (curFoliage.transform.root.TryGetComponent<MultiHealth>(out MultiHealth mHealth))
                mHealth.RequestChangeHealthServerRpc(curFoliage.transform.root.GetComponent<NetworkObject>(), -dmg * Time.deltaTime, curFoliage.name);
        }
    }
}
