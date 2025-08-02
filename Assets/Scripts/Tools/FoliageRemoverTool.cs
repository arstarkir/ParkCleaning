using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class FoliageRemoverTool : CoreTool
{
    [SerializeField] Camera cam;
    public float maxDist = 2;
    [SerializeField] LayerMask layerMask = ~6;

    GameObject curFoliage;

    public NetworkVariable<bool> IsUsing = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void TryUse(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer || !Ready)
            return;

        bool shouldUse = (context.interaction is HoldInteraction) && (context.ReadValue<float>() == 1);

        if (IsOwner)
            IsUsing.Value = shouldUse;

        base.TryUse(context);
    }

    public override void Use()
    {
        animator.Animator.SetBool("isUse", IsUsing.Value);

        if (!IsUsing.Value)
        {
            curFoliage = null;
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, layerMask))
        {
            if (hit.transform.root.TryGetComponent<MultiHealth>(out MultiHealth mHealth))
            {
                if (hit.collider.gameObject.transform.CompareTag("Foliage"))
                    curFoliage = hit.collider.gameObject;
            }
        }

        if (curFoliage == null)
        {
            if (IsOwner)
                IsUsing.Value = false;
        }
    }

    public override void Update()
    {
        base.Update();

        animator.Animator.SetBool("isUse", IsUsing.Value);

        if (curFoliage != null && IsUsing.Value)
        {
            if (curFoliage.TryGetComponent<Health>(out Health health))
                health.RequestChangeHealthServerRpc(-dmg * Time.deltaTime);

            if (curFoliage.transform.root.TryGetComponent<MultiHealth>(out MultiHealth mHealth))
                mHealth.RequestChangeHealthServerRpc(curFoliage.transform.root.GetComponent<NetworkObject>(), -dmg * Time.deltaTime, curFoliage.name);
        }
    }
}
