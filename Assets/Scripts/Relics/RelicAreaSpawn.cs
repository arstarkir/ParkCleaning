using Unity.Netcode;
using UnityEngine;

public class RelicAreaSpawn : NetworkBehaviour
{
    RelicRegister relicRegister = Resources.Load<RelicRegister>("SO/Relics/MainRelicRegister");

    public static RelicAreaSpawn instance;
    
    public void OnNetworkSpawn()
    {
        if(!IsServer)
            return;

        if (instance == null)
            instance = this;
    }

    [ServerRpc]
    public void RequestExtractionPrefomedServerRpc(ExtractionType extraction, ServerRpcParams rpcParams = default)
    {
        ulong requesterId = rpcParams.Receive.SenderClientId;
        foreach (RelicData data in relicRegister.relicDatas)
            if (data.type == extraction && data.spawnChance > UnityEngine.Random.Range(0, 1f))
            { 
                GameObject unit = Instantiate(data.pref, Vector3.zero, Quaternion.identity);
                NetworkObject netObj = unit.GetComponent<NetworkObject>();
                netObj.Spawn();
            }
    }
}
