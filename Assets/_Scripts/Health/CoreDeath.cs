using UnityEngine;
using Unity.Netcode;

public abstract class CoreDeath : NetworkBehaviour
{
    public abstract void OnDeath();
}
