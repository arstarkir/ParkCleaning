using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

public class Health : NetworkBehaviour
{
    public float maxHealth = 9;
    public NetworkVariable<float> curHealth = new NetworkVariable<float>(9);
    public float regenSpeed = 5;
    public float regenDelayTime = 120;
    public float timeSinceDmg = 0;
    public AnimationCurve magnitudeCurve = AnimationCurve.Constant(0,1,1);

    private Coroutine shakeCoroutine;
    [SerializeField] List<GameObject> onDmgVFXs = new List<GameObject>();
    public UnityEvent onDeath = new UnityEvent();

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

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(Shake(0.5f));
        NotifyClientOfDmgClientRpc();

        if (curHealth.Value > maxHealth)
            curHealth.Value = maxHealth;

        if (curHealth.Value <= 0)
        {
            if(onDeath != new UnityEvent())
                onDeath.Invoke();
            else
            {
                this.transform.GetComponent<NetworkObject>().Despawn();
                Destroy(this.gameObject);
            }
        }
    }

    [ClientRpc]
    public void NotifyClientOfDmgClientRpc()
    {
        timeSinceDmg = 0;
        foreach(GameObject onDmgVFX in onDmgVFXs)
        {
            onDmgVFX.GetComponent<ParticleSystem>().Play();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSetHealthServerRpc(float amount, ServerRpcParams rpcParams = default)
    {
        curHealth.Value = amount;
    }

    IEnumerator Shake(float duration)
    {
        Vector3 originalPos = transform.localPosition;
        Vector3 originalScale = transform.localScale;
        float time = 0f;

        while (time < duration)
        {
            float pX = Random.Range(-1f, 1f) * 0.02f;
            float pY = Random.Range(-1f, 1f) * 0.02f;

            float sZ = originalScale.y * magnitudeCurve.Evaluate(time * 2);

            transform.localPosition = originalPos + new Vector3(pX, pY, 0);
            transform.localScale = new Vector3(originalScale.x, originalScale.y, sZ);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        transform.localScale = originalScale;
    }
}
