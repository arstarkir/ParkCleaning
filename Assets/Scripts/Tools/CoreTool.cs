using Unity.Netcode;
using UnityEngine;

public abstract class CoreTool : NetworkBehaviour
{
    [SerializeField] float cooldown = 1f;
    float timer;

    public bool Ready => timer <= 0f;

    public void TryUse()
    {
        if (!IsLocalPlayer || !Ready )
            return;

        timer = cooldown;
        Use();
        GetComponent<Animator>().SetTrigger("Use");
    }

    public abstract void Use();

    public virtual void Hide()
    {
        //puting the tool on the back
    }

    void Update()
    {
        timer -= Time.deltaTime;
    }
}
