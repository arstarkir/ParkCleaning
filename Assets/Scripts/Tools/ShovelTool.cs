using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class ShovelTool : CoreTool
{
    [SerializeField] LayerMask layerMask = ~6;
    [SerializeField] float digRadius = 1f;
    [SerializeField] float digStrength = 0.002f;

    private TerrainManager terrainManager;

    void Start()
    {
        terrainManager = FindAnyObjectByType<TerrainManager>();
    }

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
        if (!IsLocalPlayer)
            return;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit terrainHit, 4, layerMask))
        {
            Terrain terrain = terrainHit.collider.gameObject.GetComponent<Terrain>();
            if (terrain != null)
            {
                RequestDigServerRpc(terrainHit.point, digRadius, digStrength);
                RelicAreaSpawn.instance.RequestExtractionPrefomedServerRpc(extraction, terrainHit.point);
            }
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
        
        NotifyClientDigClientRpc(startX, startZ, size,sRad,strength);
    }

    [ClientRpc]
    public void NotifyClientDigClientRpc(int startX, int startZ, int size, int sRad, float strength)
    {
        if (IsHost)
        {
            PaintTextureBasedOnHeightChange(Terrain.activeTerrain, terrainManager.GetOriginalHeights(), startX, startZ, size, sRad);
            return;
        }

        TerrainData data = Terrain.activeTerrain.terrainData;
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

        if (terrainManager != null)
            PaintTextureBasedOnHeightChange(Terrain.activeTerrain, terrainManager.GetOriginalHeights(), startX, startZ, size, sRad);
    }

    void PaintTextureBasedOnHeightChange(Terrain terrain, float[,] originalHeights, int startX, int startZ, int size, int sRad)
    {
        TerrainData data = terrain.terrainData;

        int alphaRes = data.alphamapResolution;
        int heightRes = data.heightmapResolution;

        int alphaStartX = Mathf.RoundToInt((float)startX / heightRes * alphaRes);
        int alphaStartZ = Mathf.RoundToInt((float)startZ / heightRes * alphaRes);
        int alphaSize = Mathf.RoundToInt((float)size / heightRes * alphaRes);

        alphaStartX = Mathf.Clamp(alphaStartX, 0, alphaRes - 1);
        alphaStartZ = Mathf.Clamp(alphaStartZ, 0, alphaRes - 1);
        if (alphaStartX + alphaSize > alphaRes) alphaSize = alphaRes - alphaStartX;
        if (alphaStartZ + alphaSize > alphaRes) alphaSize = alphaRes - alphaStartZ;

        float[,] curHeights = data.GetHeights(startX, startZ, size, size);
        float[,,] splatmaps = data.GetAlphamaps(alphaStartX, alphaStartZ, alphaSize, alphaSize);

        float alphaRadius = (float)sRad / heightRes * alphaRes;
        float centerX = alphaSize / 2f;
        float centerY = alphaSize / 2f;

        for (int y = 0; y < alphaSize; y++)
        {
            for (int x = 0; x < alphaSize; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > alphaRadius)
                    continue;

                int hx = startX + Mathf.RoundToInt((float)x / alphaSize * size);
                int hz = startZ + Mathf.RoundToInt((float)y / alphaSize * size);

                hx = Mathf.Clamp(hx, 0, heightRes - 1);
                hz = Mathf.Clamp(hz, 0, heightRes - 1);

                int localX = Mathf.Clamp(hx - startX, 0, size - 1);
                int localZ = Mathf.Clamp(hz - startZ, 0, size - 1);

                if (localZ >= curHeights.GetLength(0) || localX >= curHeights.GetLength(1))
                    continue;
                if (hz >= originalHeights.GetLength(0) || hx >= originalHeights.GetLength(1))
                    continue;

                float diff = Mathf.Abs(curHeights[localZ, localX] - originalHeights[hx, hz]);

                if (diff > 0.001f)
                {
                    for (int l = 0; l < splatmaps.GetLength(2); l++)
                    {
                        splatmaps[y, x, l] = 0;
                    }
                    splatmaps[y, x, 3] = 1;
                }
            }
        }

        data.SetAlphamaps(alphaStartX, alphaStartZ, splatmaps);
    }
}