using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RelicAreaSpawn : NetworkBehaviour
{
    public List<GameObject> areas = new List<GameObject>();

    AudioSource audioSource;
    RelicRegister relicRegister;

    public static RelicAreaSpawn instance;
    
    public override void OnNetworkSpawn()
    {
        if (instance == null)
            instance = this;
        relicRegister = Resources.Load<RelicRegister>("SO/Relics/MainRelicRegister");
        audioSource = GetComponent<AudioSource>();
    }

    [ServerRpc]
    public void RequestExtractionPrefomedServerRpc(ExtractionType extraction, Vector3 spawnPoint, ServerRpcParams rpcParams = default)
    {
        ulong requesterId = rpcParams.Receive.SenderClientId;
        NetworkObject requesterPlayer = NetworkManager.Singleton.ConnectedClients[requesterId].PlayerObject;
        if(!IsObjInArea(requesterPlayer.gameObject))
            return;
        foreach (RelicData data in relicRegister.relicDatas)
            if (data.type == extraction && data.spawnChance > UnityEngine.Random.Range(0, 1f))
            { 
                GameObject unit = Instantiate(data.pref, spawnPoint, Quaternion.identity);
                NetworkObject netObj = unit.GetComponent<NetworkObject>();
                netObj.Spawn();
                audioSource.clip = data.onPickupSound;
                audioSource.Play();
            }
    }

    bool IsObjInArea(GameObject obj)
    {
        Collider temp = obj.GetComponent<Collider>();
        foreach (GameObject area in areas)
            if(area.GetComponent<TriggerTracker>().DoseContain(temp))
                return true;
        return false;
    }
}
