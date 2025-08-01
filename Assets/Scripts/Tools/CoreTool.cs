using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CoreTool : NetworkBehaviour
{
    [SerializeField] float cooldown = 1f;
    float timer;
    public float dmg = 3;

    [HideInInspector] public Animator animator;

    public bool Ready => timer <= 0f;

    public virtual void TryUse(InputAction.CallbackContext context)
    {
        if (!IsLocalPlayer || !Ready )
            return;

        timer = cooldown;
        Use();
    }

    public abstract void Use();

    public virtual void Hide()
    {
        //puting the tool on the back
    }

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
    }

    public virtual void Update()
    {
        timer -= Time.deltaTime;
    }
}
