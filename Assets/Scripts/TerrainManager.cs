using Unity.Netcode;
using UnityEngine;

public class TerrainManager : NetworkBehaviour
{
    private float[,] originalHeights;
    private float[,,] originalAlphamaps;
    private Terrain terrain;
    private TerrainData data;

    private const int CHUNK_SIZE = 64;
    private const int TEXTURE_CHUNK_SIZE = 32;

    public override void OnNetworkSpawn()
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
            return;

        data = terrain.terrainData;
        originalHeights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);
        originalAlphamaps = data.GetAlphamaps(0, 0, data.alphamapResolution, data.alphamapResolution);

        if (IsServer)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        int heightRes = data.heightmapResolution - 1;

        for (int x = 0; x < heightRes; x += CHUNK_SIZE)
        {
            for (int z = 0; z < heightRes; z += CHUNK_SIZE)
            {
                int width = Mathf.Min(CHUNK_SIZE, heightRes - x);
                int height = Mathf.Min(CHUNK_SIZE, heightRes - z);

                float[,] heights = data.GetHeights(x, z, width, height);
                float[] flatHeights = FlattenHeights(heights);

                SendTerrainChunkClientRpc(x, z, width, height, flatHeights, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
                });
            }
        }

        int alphaRes = data.alphamapResolution - 1;
        int layers = data.terrainLayers.Length;

        for (int x = 0; x < alphaRes; x += TEXTURE_CHUNK_SIZE)
        {
            for (int z = 0; z < alphaRes; z += TEXTURE_CHUNK_SIZE)
            {
                int width = Mathf.Min(TEXTURE_CHUNK_SIZE, alphaRes - x);
                int height = Mathf.Min(TEXTURE_CHUNK_SIZE, alphaRes - z);

                float[,,] alphaChunk = data.GetAlphamaps(x, z, width, height);
                float[] flatAlpha = FlattenAlphamaps(alphaChunk);

                SendTerrainTextureChunkClientRpc(x, z, width, height, layers, flatAlpha, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
                });
            }
        }
    }

    [ClientRpc]
    private void SendTerrainChunkClientRpc(int startX, int startZ, int width, int height, float[] flat, ClientRpcParams rpcParams = default)
    {
        float[,] heights = UnflattenHeights(flat, width, height);
        Terrain.activeTerrain.terrainData.SetHeights(startX, startZ, heights);
    }

    [ClientRpc]
    private void SendTerrainTextureChunkClientRpc(int startX, int startZ, int width, int height, int layers, float[] flatAlpha, ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;

        TerrainData tData = Terrain.activeTerrain.terrainData;
        int clientLayers = tData.terrainLayers.Length;

        int usedLayers = Mathf.Min(layers, clientLayers);

        float[,,] alpha = new float[width, height, clientLayers];
        float[,,] receivedAlpha = UnflattenAlphamaps(flatAlpha, width, height, layers);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int l = 0; l < usedLayers; l++)
                {
                    alpha[x, y, l] = receivedAlpha[x, y, l];
                }
            }
        }

        tData.SetAlphamaps(startX, startZ, alpha);
    }

    public void ResetTerrain()
    {
        if (data == null)
            return;

        if (originalHeights != null)
            data.SetHeights(0, 0, originalHeights);

        if (originalAlphamaps != null)
            data.SetAlphamaps(0, 0, originalAlphamaps);
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

    private float[] FlattenAlphamaps(float[,,] alpha)
    {
        int w = alpha.GetLength(0);
        int h = alpha.GetLength(1);
        int l = alpha.GetLength(2);

        float[] flat = new float[w * h * l];
        int i = 0;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                for (int z = 0; z < l; z++)
                {
                    flat[i++] = alpha[x, y, z];
                }
            }
        }
        return flat;
    }

    private float[,,] UnflattenAlphamaps(float[] flat, int width, int height, int layers)
    {
        float[,,] alpha = new float[width, height, layers];
        int i = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < layers; z++)
                {
                    alpha[x, y, z] = flat[i++];
                }
            }
        }
        return alpha;
    }

    public float[,] GetOriginalHeights()
    {
        return originalHeights;
    }
}
