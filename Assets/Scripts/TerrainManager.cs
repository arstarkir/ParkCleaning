using Unity.Netcode;
using UnityEngine;

public class TerrainManager : NetworkBehaviour
{
    private float[,] originalHeights;
    private Terrain terrain;
    private TerrainData data;

    private const int CHUNK_SIZE = 64;

    public override void OnNetworkSpawn()
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
            return;

        data = terrain.terrainData;
        originalHeights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);

        if (IsServer)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        int resolution = data.heightmapResolution - 1;

        for (int x = 0; x < resolution; x += CHUNK_SIZE)
        {
            for (int z = 0; z < resolution; z += CHUNK_SIZE)
            {
                int width = Mathf.Min(CHUNK_SIZE, resolution - x);
                int height = Mathf.Min(CHUNK_SIZE, resolution - z);

                float[,] heights = data.GetHeights(x, z, width, height);
                float[] flat = FlattenHeights(heights);

                SendTerrainChunkClientRpc(x, z, width, height, flat, new ClientRpcParams
                { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
            }
        }
    }

    [ClientRpc]
    private void SendTerrainChunkClientRpc(int startX, int startZ, int width, int height, float[] flat, ClientRpcParams rpcParams = default)
    {
        float[,] heights = UnflattenHeights(flat, width, height);
        Terrain.activeTerrain.terrainData.SetHeights(startX, startZ, heights);
    }

    public void ResetTerrain()
    {
        if (data == null || originalHeights == null)
            return;

        data.SetHeights(0, 0, originalHeights);
    }

    void OnApplicationQuit()
    { 
        ResetTerrain();
    }

    void OnDestroy()
    {
        ResetTerrain();
    }

    private float[] FlattenHeights(float[,] heights)
    {
        int width = heights.GetLength(0);
        int height = heights.GetLength(1);
        float[] flat = new float[width * height];

        int i = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                flat[i++] = heights[x, y];
            }
        }
        return flat;
    }

    private float[,] UnflattenHeights(float[] flat, int width, int height)
    {
        float[,] heights = new float[width, height];

        int i = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = flat[i++];
            }
        }
        return heights;
    }
}
