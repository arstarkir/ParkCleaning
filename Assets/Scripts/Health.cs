using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public float maxHealth = 9;
    public NetworkVariable<float> curHealth = new NetworkVariable<float>(9);
    public float regenSpeed = 5;
    public float regenDelayTime = 120;
    public float timeSinceDmg = 0;

    [SerializeField] GameObject onDmgVFX;

    private void Awake()
    {
        if (IsServer)
            RequestSetHealthServerRpc(maxHealth);
    }

    private void Update()
    {
        if (!IsServer)
            return;

        timeSinceDmg += Time.deltaTime;
        if (timeSinceDmg >= regenDelayTime)
            RequestChangeHealthServerRpc(regenSpeed * Time.deltaTime);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangeHealthServerRpc(float amount, ServerRpcParams rpcParams = default)
    {
        curHealth.Value += amount;

        if (amount < 0)
            NotifyClientOfDmgClientRpc();

        if (curHealth.Value > maxHealth)
            curHealth.Value = maxHealth;

        if (curHealth.Value <= 0)
        {
            gameObject.AddComponent<NetworkTransform>();
            transform.position += Vector3.up;
            gameObject.AddComponent<NetworkRigidbody>().UseRigidBodyForMotion = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            //this.transform.GetComponent<NetworkObject>().Despawn();
            //Destroy(this.gameObject);
        }
    }

    [ClientRpc]
    public void NotifyClientOfDmgClientRpc()
    {
        timeSinceDmg = 0;
        //onDmgVFX.GetComponent<ParticleSystem>().Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSetHealthServerRpc(float amount, ServerRpcParams rpcParams = default)
    {
        curHealth.Value = amount;
    }
}
