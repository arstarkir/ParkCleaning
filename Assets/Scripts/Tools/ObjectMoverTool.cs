using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class ObjectMoverTool : CoreTool
{
    [SerializeField] Camera cam;
    [SerializeField] float dist = 3f;
    [SerializeField] float holdForce = 250f;
    [SerializeField] float damping = 5f;
    [SerializeField] LayerMask layerMask;

    Rigidbody rb;
    Vector3 moveOffset;
    bool isHolding;

    public override void TryUse(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer || !Ready)
            return;

        if (!(context.interaction is HoldInteraction) || !(context.ReadValue<float>() == 1))
            ReleaseGrab();
        else
            if(!isHolding)
                StartGrab();
        
        base.TryUse(context);
    }

    public override void Use()
    {
        
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer) 
            return;

        if (rb != null && isHolding)
        {
            Vector3 targetPos = cam.transform.position + cam.transform.forward * dist;
            Vector3 currentPoint = rb.position + moveOffset;

            Vector3 force = (targetPos - currentPoint) * holdForce;
            rb.AddForceAtPosition(force, currentPoint, ForceMode.Force);

            rb.linearVelocity *= (1f - Time.fixedDeltaTime * damping);
        }
    }

    private void StartGrab()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, dist, layerMask))
        {
            if (hit.rigidbody != null)
            {
                rb = hit.rigidbody;
                moveOffset = rb.transform.InverseTransformPoint(hit.point);
                isHolding = true;
            }
            Debug.Log(hit.collider.gameObject.name);
        }
    }

    private void ReleaseGrab()
    {
        if (rb != null)
        {
            rb = null;
        }

        isHolding = false;
    }
}
