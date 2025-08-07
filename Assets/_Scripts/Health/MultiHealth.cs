using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MultiHealth : NetworkBehaviour
{
    public float regenSpeed = 5;
    public float regenDelayTime = 120;

    private class HealthEntry
    {
        public float maxHealth = 10;
        public float curHealth;
        public float timeSinceDmg;
        public UnityEvent onDeath;
        public bool dead;
    }

    private Dictionary<Transform, HealthEntry> tracked = new Dictionary<Transform, HealthEntry>();

    Rigidbody rb;
    float startMass;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startMass = rb.mass;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        foreach (var kvp in tracked)
        {
            var entry = kvp.Value;
            if (entry.dead) continue;

            entry.timeSinceDmg += Time.deltaTime;
            if (entry.timeSinceDmg >= regenDelayTime)
            {
                NetworkObject parentNO = kvp.Key.GetComponentInParent<NetworkObject>();
                if (parentNO != null)
                    RequestChangeHealthServerRpc(new NetworkObjectReference(parentNO), regenSpeed * Time.deltaTime, kvp.Key.name);
            }
        }
    }

    public void AddObject(Transform t, float maxHealth, UnityAction deathEvent = null)
    {
        if (!tracked.ContainsKey(t))
        {
            HealthEntry e = new HealthEntry();
            e.curHealth = maxHealth;
            e.timeSinceDmg = 0;
            e.onDeath = new UnityEvent();
            if (deathEvent != null)
                e.onDeath.AddListener(deathEvent);
            e.dead = false;
            tracked.Add(t, e);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangeHealthServerRpc(NetworkObjectReference targetRef, float amount, string childName, ServerRpcParams rpcParams = default)
    {
        if (!targetRef.TryGet(out NetworkObject netObj))
            return;

        Transform target = FindChildByName(netObj.transform, childName);
        if (target == null)
            return;

        if (!tracked.TryGetValue(target, out HealthEntry e))
            return;

        if (e.dead) 
            return;

        e.curHealth += amount;

        if (amount < 0)
            NotifyClientOfDmgClientRpc(targetRef, childName);

        if (e.curHealth > e.maxHealth)
            e.curHealth = e.maxHealth;

        if (e.curHealth <= 0)
        {
            e.dead = true;
            e.onDeath.Invoke();

            if(target != this.transform)
                target.gameObject.SetActive(false);
            else
            {
                target.gameObject.GetComponent<Renderer>().enabled = false;
                target.gameObject.GetComponent<Collider>().enabled = false;
            }

            NotifyClientOfEntryDeathClientRpc(childName);

            bool allDead = true;
            foreach (var kvp in tracked)
            {
                if (!kvp.Value.dead)
                {
                    allDead = false;
                    break;
                }
            }

            rb.mass -= startMass / tracked.Count;

            if (allDead)
            {
                netObj.Despawn();
                Destroy(netObj.gameObject);
            }
        }
    }

    [ClientRpc]
    public void NotifyClientOfEntryDeathClientRpc(string childName)
    {
        Transform target = FindChildByName(transform, childName);
        if (target != null)
        {
            if (target != this.transform)
                target.gameObject.SetActive(false);
            else
            {
                target.gameObject.GetComponent<Renderer>().enabled = false;
                target.gameObject.GetComponent<Collider>().enabled = false;
            }
        }
    }

    [ClientRpc]
    public void NotifyClientOfDmgClientRpc(NetworkObjectReference targetRef, string childName)
    {
        if (!targetRef.TryGet(out NetworkObject netObj))
            return;

        Transform target = FindChildByName(netObj.transform, childName);
        if (target == null || !tracked.ContainsKey(target))
            return;

        tracked[target].timeSinceDmg = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSetHealthServerRpc(NetworkObjectReference targetRef, float amount, string childName, ServerRpcParams rpcParams = default)
    {
        if (!targetRef.TryGet(out NetworkObject netObj))
            return;

        Transform target = FindChildByName(netObj.transform, childName);
        if (target == null || !tracked.ContainsKey(target))
            return;

        tracked[target].curHealth = amount;
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }
        return null;
    }
}
