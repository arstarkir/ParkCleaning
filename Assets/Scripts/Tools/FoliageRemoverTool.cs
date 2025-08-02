using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class FoliageRemoverTool : CoreTool
{
    [SerializeField] Camera cam;
    public float maxDist = 2;
    bool isUsing = false;
    [SerializeField] LayerMask layerMask = ~6;

    GameObject curFoliage;

    public override void TryUse(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer || !Ready)
            return;

        if (!(context.interaction is HoldInteraction) || !(context.ReadValue<float>() == 1))
            animator.SetBool("isUse", false);
        else
            if (!isUsing)
                animator.SetBool("isUse", true);

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

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, layerMask))
        {
            if (hit.transform.root.TryGetComponent<MultiHealth>(out MultiHealth mHealth))
                if (hit.collider.gameObject.transform.CompareTag("Foliage"))
                    curFoliage = hit.collider.gameObject;
        }

        if(curFoliage == null)
            animator.SetBool("isUse", false);
    }

    public override void Update()
    {
        base.Update();

        if (curFoliage != null)
        {
            if (curFoliage.TryGetComponent<Health>(out Health health))
                health.RequestChangeHealthServerRpc(-dmg * Time.deltaTime);
            if (curFoliage.transform.root.TryGetComponent<MultiHealth>(out MultiHealth mHealth))
                mHealth.RequestChangeHealthServerRpc(curFoliage.transform.root.GetComponent<NetworkObject>(), -dmg * Time.deltaTime, curFoliage.name);
        }
    }
}
