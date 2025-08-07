using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

public class TreeDeath : CoreDeath
{
    [SerializeField] GameObject tree;

    public override void OnDeath()
    {
        if (!IsServer) 
            return;
        
        GameObject fallenTree = Instantiate(tree, transform.position + Vector3.up, transform.rotation);
        fallenTree.GetComponent<NetworkObject>().Spawn();

        MultiHealth multiHealth = fallenTree.GetComponent<MultiHealth>();
        multiHealth.GetComponent<Transform>().localScale = this.GetComponent<Transform>().localScale;
        multiHealth.AddObject(fallenTree.transform, 9, fallenTree.GetComponent<WoodDeath>().OnDeath);
        LeavesSetUp(multiHealth, fallenTree);
    }

    void LeavesSetUp(MultiHealth multiHealth, GameObject fallenTree)
    {
        foreach (Transform t in fallenTree.transform)
        {
            multiHealth.AddObject(t.transform, 25);
            t.tag = "Foliage";
        }

        NotifyClientOfDeathClientRpc(GetComponent<NetworkObject>().NetworkObjectId);
        GetComponent<NetworkObject>().Despawn(true);
    }

    [ClientRpc]
    public void NotifyClientOfDeathClientRpc(ulong objectId)
    {
        NetworkObject deadObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId];
        if(deadObj != null)
            Destroy(deadObj.gameObject);
    }
}
