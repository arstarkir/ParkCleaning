using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class ShovelTool : CoreTool
{
    [SerializeField] LayerMask layerMask = ~6;
    [SerializeField] float digRadius = 1f;
    [SerializeField] float digStrength = 0.002f;

    public override void TryUse(InputAction.CallbackContext context)
    {
        if (context.interaction is TapInteraction)
            base.TryUse(context);
    }

    public override void Use()
    {
        animator.SetTrigger("Use");
    }

    public void TriggerDigRequest()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit terrainHit, 4, layerMask))
        {
            Terrain terrain = terrainHit.collider.gameObject.GetComponent<Terrain>();
            if (terrain != null)
                RequestDigServerRpc(terrainHit.point, digRadius, digStrength);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDigServerRpc(Vector3 pos, float r, float strength)
    {
        Terrain terrain = Terrain.activeTerrain;
        TerrainData data = terrain.terrainData;

        Vector3 tPos = pos - terrain.transform.position;
        int mapX = Mathf.RoundToInt((tPos.x / data.size.x) * data.heightmapResolution);
        int mapZ = Mathf.RoundToInt((tPos.z / data.size.z) * data.heightmapResolution);

        int sRad = Mathf.RoundToInt(r / data.size.x * data.heightmapResolution);

        int startX = Mathf.Clamp(mapX - sRad, 0, data.heightmapResolution);
        int startZ = Mathf.Clamp(mapZ - sRad, 0, data.heightmapResolution);
        int size = sRad * 2;

        float[,] heights = data.GetHeights(startX, startZ, size, size);

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float distance = Vector2.Distance(new Vector2(x, z), new Vector2(sRad, sRad));
                if (distance < sRad)
                    heights[z, x] -= strength * (1 - (distance / sRad));
            }
        }

        data.SetHeights(startX, startZ, heights);
    }
}