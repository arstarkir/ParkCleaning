using UnityEngine;

public class TerrainResetManager : MonoBehaviour
{
    private float[,] originalHeights;
    private Terrain terrain;
    private TerrainData data;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
            return;

        data = terrain.terrainData;

        originalHeights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);
    }

    void OnApplicationQuit()
    {
        ResetTerrain();
    }

    void OnDestroy()
    {
        ResetTerrain();
    }

    public void ResetTerrain()
    {
        if (data == null || originalHeights == null)
            return;

        data.SetHeights(0, 0, originalHeights);
    }
}
