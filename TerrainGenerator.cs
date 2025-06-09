using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int terrainWidth = 512;
    public int terrainHeight = 512;

    [Header("Noise Settings")]
    public float scale = 20f;
    public float heightMultiplier = 0.1f;
    [Tooltip("Raise this to flatten valleys. Range: 0.0 to 0.5 is typical.")]
    public float baseFlatten = 0.25f;

    [Header("Road Settings")]
    public List<Vector3> roadPoints = new List<Vector3>()
    {
        new Vector3(50, 0, 50),
        new Vector3(150, 0, 300),
        new Vector3(300, 0, 200)
    };
    public float roadWidth = 8f;
    public float roadHeight = 0.02f;

    private Terrain terrain;
    private TerrainData terrainData;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, 100, terrainHeight);

        // Generate base terrain heights
        float[,] heights = GenerateHeights();

        // Flatten terrain along the road path
        FlattenRoad(heights);

        // Apply final heights to terrain
        terrainData.SetHeights(0, 0, heights);
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[terrainWidth, terrainHeight];

        for (int x = 0; x < terrainWidth; x++)
        {
            for (int y = 0; y < terrainHeight; y++)
            {
                float xCoord = (float)x / terrainWidth;
                float yCoord = (float)y / terrainHeight;

                // Multiple Perlin layers
                float noise1 = Mathf.PerlinNoise(xCoord * scale, yCoord * scale); // base
                float noise2 = Mathf.PerlinNoise(xCoord * scale * 2f, yCoord * scale * 2f) * 0.5f; // mid detail
                float noise3 = Mathf.PerlinNoise(xCoord * scale * 4f, yCoord * scale * 4f) * 0.25f; // fine detail

                // Combine noise and flatten
                float combinedNoise = noise1 + noise2 + noise3;
                float adjusted = Mathf.Max(0, combinedNoise - baseFlatten);

                heights[x, y] = adjusted * heightMultiplier;
            }
        }

        return heights;
    }

    void FlattenRoad(float[,] heights)
    {
        int hmWidth = terrainData.heightmapResolution;
        int hmHeight = terrainData.heightmapResolution;

        for (int y = 0; y < hmHeight; y++)
        {
            for (int x = 0; x < hmWidth; x++)
            {
                Vector3 worldPos = new Vector3(
                    (float)x / hmWidth * terrainData.size.x + terrain.transform.position.x,
                    0,
                    (float)y / hmHeight * terrainData.size.z + terrain.transform.position.z
                );

                float minDist = float.MaxValue;

                foreach (var rp in roadPoints)
                {
                    float dist = Vector2.Distance(new Vector2(worldPos.x, worldPos.z), new Vector2(rp.x, rp.z));
                    if (dist < minDist)
                        minDist = dist;
                }

                if (minDist < roadWidth)
                {
                    float normalizedRoadHeight = roadHeight / terrainData.size.y;
                    float t = 1f - (minDist / roadWidth); // smooth edge falloff
                    heights[x, y] = Mathf.Lerp(heights[x, y], normalizedRoadHeight, t);
                }
            }
        }
    }
}
