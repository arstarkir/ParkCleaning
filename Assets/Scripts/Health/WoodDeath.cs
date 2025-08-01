using Unity.Netcode;
using UnityEngine;

public class WoodDeath : CoreDeath
{
    [SerializeField] GameObject plank;
    public override void OnDeath()
    {
        Instantiate(plank, transform.position + Vector3.right, transform.rotation).GetComponent<NetworkObject>().Spawn();
        Instantiate(plank, transform.position + Vector3.left, transform.rotation).GetComponent<NetworkObject>().Spawn();
    }
}
