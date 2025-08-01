using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TreeDeath : CoreDeath
{
    [SerializeField] GameObject leaves;

    public override void OnNetworkSpawn()
    {
        GetComponent<Health>().onDeath.AddListener(OnDeath);
    }

    public override void OnDeath()
    {
        gameObject.AddComponent<NetworkTransform>();
        transform.position += Vector3.up;
        gameObject.AddComponent<NetworkRigidbody>().UseRigidBodyForMotion = true;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        Destroy(GetComponent<Health>());
        LeavesSetUp();
    }

    void LeavesSetUp()
    {
        leaves.tag = "Foliage";
        gameObject.AddComponent<Health>().maxHealth = 3;
    }
}
