using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int width = 200;          // Largeur du terrain
    public int depth = 200;          // Profondeur du terrain
    public float perlinNoiseScale = 25f; // Échelle du bruit
    public float heightMultiplier = 40f; // Multiplicateur de hauteur pour le terrain
    public Material terrainMaterial;  // Matériau du terrain
    public Vector3 offset;            // Décalage pour générer différentes variations de terrain
    public int octaves = 4;          // Nombre d'octaves pour le bruit
    public float persistence = 0.5f;  // Influence décroissante des octaves

    private PerlinNoise perlinNoise; // Générateur de bruit de Perlin

    /// <summary>
    /// Démarre la génération du terrain
    /// </summary>
    void Start()
    {
        perlinNoise = new PerlinNoise();
        SetupMeshComponents();           
        GenerateTerrain();               
    }

    /// <summary>
    /// Configure les composants MeshFilter et MeshRenderer pour l'objet
    /// </summary>
    void SetupMeshComponents()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>(); 
        if (meshFilter == null)
        {
            gameObject.AddComponent<MeshFilter>(); 
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>(); 
        if (meshRenderer == null)
        {
            gameObject.AddComponent<MeshRenderer>(); 
        }

        meshRenderer.material = terrainMaterial; 
    }

    /// <summary>
    /// Génère le terrain à l'aide de bruit de Perlin fractal.
    /// </summary>
    void GenerateTerrain()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>(); 
        Mesh terrainMesh = new Mesh(); 

        Vector3[] vertices = GenerateVertices();  
        int[] triangles = GenerateTriangles();    

        AssignMeshData(terrainMesh, vertices, triangles);  

        meshFilter.mesh = terrainMesh; 
    }

    /// <summary>
    /// Génère un tableau de sommets pour le maillage du terrain
    /// </summary>
    /// <returns>Tableau de sommets du terrain</returns>
    Vector3[] GenerateVertices()
    {
        Vector3[] vertices = new Vector3[(width + 1) * (depth + 1)]; // Tableau des sommets

        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float y = perlinNoise.PerlinNoiseFractal(
                    (x + offset.x) / perlinNoiseScale,
                    (z + offset.z) / perlinNoiseScale,
                    octaves, persistence, offset
                ) * heightMultiplier;

                vertices[z * (width + 1) + x] = new Vector3(x, y, z); // Création du sommet
            }
        }
        return vertices;
    }

    /// <summary>
    /// Génère un tableau de triangles pour relier les sommets du maillage
    /// </summary>
    /// <returns>Tableau d'indices des triangles du terrain</returns>
    int[] GenerateTriangles()
    {
        int[] triangles = new int[width * depth * 6]; // Tableau des triangles
        int triangleIndex = 0; 

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // Premier triangle
                triangles[triangleIndex + 0] = z * (width + 1) + x;
                triangles[triangleIndex + 1] = (z + 1) * (width + 1) + x;
                triangles[triangleIndex + 2] = z * (width + 1) + x + 1;

                // Deuxième triangle
                triangles[triangleIndex + 3] = z * (width + 1) + x + 1;
                triangles[triangleIndex + 4] = (z + 1) * (width + 1) + x;
                triangles[triangleIndex + 5] = (z + 1) * (width + 1) + x + 1;

                triangleIndex += 6; 
            }
        }
        return triangles;
    }

    /// <summary>
    /// Assigne les données des sommets et triangles au maillage
    /// </summary>
    /// <param name="mesh">Le maillage auquel les données seront assignées</param>
    /// <param name="vertices">Les sommets du maillage</param>
    /// <param name="triangles">Les indices des triangles</param>
    void AssignMeshData(Mesh mesh, Vector3[] vertices, int[] triangles)
    {
        mesh.vertices = vertices; 
        mesh.triangles = triangles;    
        mesh.RecalculateNormals();      // Recalcul des normales pour l'éclairage
    }
}
