using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RandomSeaFloor : MonoBehaviour
{
    private Mesh mesh;
    public float scale;
    public AnimationCurve heightCurve;
    private Vector3[] vertices;
    private int[] triangles;

    public int xSize;
    public int zSize;

    /// <summary>
    /// A higher value captures a smaller area of perlin noise, so appears smoother
    /// </summary>
    public float smoothness;
    /// <summary>
    /// How many layers of perlin noise to put together
    /// </summary>
    public int octaves;
    /// <summary>
    /// Higher value means perlin noise layers get more granular quicker
    /// </summary>
    public float lacunarity;

    private int seed;
    /// <summary>
    /// Randomly generated offsets for fetching perlin noise layers
    /// </summary>
    private Vector2[] octaveOffsets;


    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateNewMap();
    }

    public void CreateNewMap()
    {
        CreateMeshShape();
        CreateTriangles();
        UpdateMesh();
    }

    private void CreateMeshShape()
    {
        octaveOffsets = GetOffsetSeed();

        if (smoothness <= 0) smoothness = 1;

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float noiseHeight = GenerateNoiseHeight(z, x, octaveOffsets);
                vertices[i] = new Vector3(x - xSize / 2.0f, noiseHeight, z - zSize / 2.0f);
                i++;
            }
        }
    }

    private Vector2[] GetOffsetSeed()
    {
        seed = Random.Range(0, 1000);
        System.Random prng = new(seed); // pseudo-random number generator
        Vector2[] octaveOffsets = new Vector2[octaves];
        
        for (int o = 0; o < octaves; o++) {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[o] = new Vector2(offsetX, offsetY);
        }
        return octaveOffsets;
    }

    private float GenerateNoiseHeight(int z, int x, Vector2[] octaveOffsets)
    {
        float amplitude = 12;
        float frequency = 1;
        float persistence = 0.5f; // Lower value means subsequent layers are taken into account less
        float noiseHeight = 0;

        for (int y = 0; y < octaves; y++)
        {
            float mapZ = z / smoothness * frequency + octaveOffsets[y].y;
            float mapX = x / smoothness * frequency + octaveOffsets[y].x;

            // The *2-1 converts from [0,1] to [-1,1]
            float perlinValue = Mathf.PerlinNoise(mapZ, mapX) * 2 - 1;
            noiseHeight += heightCurve.Evaluate(perlinValue) * amplitude;
            frequency *= lacunarity;
            amplitude *= persistence;
        }
        noiseHeight = Mathf.Min(noiseHeight, 10); // The value will probably never be this large, but we need to make sure of it for the shader graph
        return noiseHeight;
    }

    private void CreateTriangles() 
    {
        // 6 vertices for each square (2 triangles)
        triangles = new int[xSize * zSize * 6];
        int vertexPointer = 0; // points to the top right vertex of our current square
        int i = 0; // keeps track of where we are in the triangles array

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[i + 0] = vertexPointer + 0;
                triangles[i + 1] = vertexPointer + xSize + 1; // adding xSize + 1 gets us onto the next row, so this point is directly below the first one
                triangles[i + 2] = vertexPointer + 1;
                triangles[i + 3] = vertexPointer + 1;
                triangles[i + 4] = vertexPointer + xSize + 1;
                triangles[i + 5] = vertexPointer + xSize + 2;

                vertexPointer++;
                i += 6;
            }
            vertexPointer++;
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;

        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }
}
