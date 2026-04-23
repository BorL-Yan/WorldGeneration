using Unity.Collections;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.Serialization;


public class MapGenerator : MonoBehaviour
{
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    public MapDisplay mapDisplay;
    public bool autoUpdate;
    
    [ReadOnly] public const int mapChunkSize = 241;
    [Range(0,6)] public int editorPreviewLOD;
    [Min(0.0001f)] public float noiseScale;

    [Min(1)] public int octaves;
    [Range(0f,1f)] public float persistence;
    [Min(1f)] public float lacunarity;
    
    public int seed;
    public Vector2 Offset;
    public bool useFalloff;

    [Min(0f)] public float meshHeightMultiplier;
    [BoundedCurve(0,-1f, 1f, 2.5f)] public AnimationCurve meshHeightCurve;
    
    public Gradient gradient;
    public float[,] falloff;
    private Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
    
    public void DrawMapEditor()
    {
        falloff = FalloffGenerator.GeneratorFalloffMap(mapChunkSize);
        MapData mapData = GenerateMapData(Vector2.zero);
        
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
            {
                var texture = TextureGenerator.TextureFromHeightMap(mapData.heightMap);
                mapDisplay.DrawTexture(texture);
                break;
            }
            case DrawMode.ColorMap:
            {
                var texture = TextureGenerator.TextureFromColorMap(GetColorMapToGradient(mapData.heightMap), mapChunkSize, mapChunkSize);
                mapDisplay.DrawTexture(texture);
                break;
            }
            case DrawMode.Mesh:
            {
                var texture = TextureGenerator.TextureFromColorMap(GetColorMapToGradient(mapData.heightMap), mapChunkSize, mapChunkSize);
                MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD);
                mapDisplay.DrawMesh(meshData, texture);
                break;
            }
            case DrawMode.FalloffMap:
            {
                var texture = TextureGenerator.TextureFromHeightMap(FalloffGenerator.GeneratorFalloffMap(mapChunkSize));
                mapDisplay.DrawTexture(texture);
                break;
            }
                
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };
        
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center,Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData =
            MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }
    
    private void Update()
    {
        
            if (mapDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
                {
                    var threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        
        
            if (meshDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
                {
                    var threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        
    }

    private MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, 
            seed,
            octaves, persistence, lacunarity,
            Offset + center, normalizeMode);

        return new MapData(noiseMap, GetColorMapToGradient(noiseMap));
    }

    private Color[] GetColorMapToGradient(float[,] noiseMap)
    {
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloff[x, y]);
                }
                
                float currentHeight = noiseMap[x, y];
                colorMap[y * mapChunkSize + x] = gradient.Evaluate(currentHeight);
            }
        }
        return colorMap;
    }


    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}




public enum DrawMode {NoiseMap, ColorMap, Mesh, FalloffMap}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}


