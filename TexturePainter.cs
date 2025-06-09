using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TexturePainter : MonoBehaviour
{
    public TerrainLayer[] terrainLayers; // Assign in Inspector
    public float grassHeight = 0.3f;
    public float rockHeight = 0.6f;

    public void PaintTextures()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;

        terrainData.terrainLayers = terrainLayers;

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;
        int layerCount = terrainLayers.Length;

        float[,,] alphamaps = new float[width, height, layerCount];
        float[,] heights = terrainData.GetHeights(0, 0, width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float currentHeight = heights[x, y];
                float[] weights = new float[layerCount];

                if (currentHeight < grassHeight)
                    weights[0] = 1f;
                else if (currentHeight < rockHeight)
                    weights[1] = 1f;
                else
                    weights[2] = 1f;

                float total = 0;
                foreach (float w in weights) total += w;
                for (int i = 0; i < layerCount; i++) weights[i] /= total;

                for (int i = 0; i < layerCount; i++)
                    alphamaps[x, y, i] = weights[i];
            }
        }

        terrainData.SetAlphamaps(0, 0, alphamaps);
    }
}
