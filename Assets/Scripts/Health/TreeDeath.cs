using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

public class TreeDeath : CoreDeath
{
    public GameObject leaves;
    [SerializeField] GameObject tree;

    public override void OnNetworkSpawn()
    {
        GetComponent<Health>().onDeath.AddListener(OnDeath);
    }

    public override void OnDeath()
    {
        if (!IsServer) 
            return;
        
        GameObject fallenTree = Instantiate(tree, transform.position + Vector3.up, transform.rotation);
        fallenTree.GetComponent<NetworkObject>().Spawn();

        MultiHealth multiHealth = fallenTree.GetComponent<MultiHealth>();
        multiHealth.AddObject(fallenTree.transform, 9, fallenTree.GetComponent<WoodDeath>().OnDeath);
        LeavesSetUp(multiHealth, fallenTree);
    }

    void LeavesSetUp(MultiHealth multiHealth, GameObject fallenTree)
    {
        foreach (Transform t in fallenTree.transform)
        {
            multiHealth.AddObject(t.transform, 15);
            t.tag = "Foliage";
        }

        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
