using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int width = 200;          /// Largeur du terrain
    public int depth = 200;          /// Profondeur du terrain
    public float perlinNoiseScale = 25f; /// Échelle du bruit
    public float heightMultiplier = 40f; /// Multiplicateur de hauteur pour le terrain
    public Material terrainMaterial;  /// Matériau du terrain
    public Vector3 offset;            /// Décalage pour générer différentes variations de terrain

    private PerlinNoise perlinNoise; /// Générateur de bruit de Perlin


    /// <summary>
    /// Démarre la génération du terrain
    /// </summary>
    void Start()
    {
        perlinNoise = new PerlinNoise(); 
        GenerateTerrain(); 
    }

    /// <summary>
    /// Fonction pour générer le terrain en utilisant le Perlin Noise 3D
    /// </summary>
    void GenerateTerrain()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>(); /// Récupération du MeshFilter
        if (meshFilter == null)
        {
            gameObject.AddComponent<MeshFilter>(); /// Ajout d'un MeshFilter si aucun n'existe
            meshFilter = GetComponent<MeshFilter>(); /// Récupération du MeshFilter
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>(); /// Récupération du MeshRenderer
        if (meshRenderer == null)
        {
            gameObject.AddComponent<MeshRenderer>(); /// Ajout d'un MeshRenderer si aucun n'existe
            meshRenderer = GetComponent<MeshRenderer>(); /// Récupération du MeshRenderer
        }

        meshRenderer.material = terrainMaterial; /// Application du matériau au mesh

        Mesh terrainMesh = new Mesh(); /// Création d'un nouveau mesh pour le terrain
        Vector3[] vertices = new Vector3[(width + 1) * (depth + 1)]; /// Tableau des sommets

        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float y = PerlinNoiseFractal((x + offset.x) / perlinNoiseScale, (z + offset.z) / perlinNoiseScale) * heightMultiplier; /// Calcul de la hauteur

                y += Random.Range(-1f, 1f); /// Ajout d'un bruit aléatoire pour imiter les irrégularités
                vertices[z * (width + 1) + x] = new Vector3(x, y, z); /// Création du sommet
            }
        }

        int[] triangles = new int[width * depth * 6]; /// Tableau des triangles
        int triangleIndex = 0; /// Index des triangles

        /// Génération des triangles
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

                triangleIndex += 6; /// Avance à l'index du prochain triangle
            }
        }

        terrainMesh.vertices = vertices; /// Affectation des sommets au mesh
        terrainMesh.triangles = triangles; /// Affectation des triangles au mesh
        terrainMesh.RecalculateNormals(); /// Recalcul des normales pour l'éclairage

        meshFilter.mesh = terrainMesh; /// Affectation du mesh au MeshFilter
    }

    public int octaves = 4;          /// Nombre d'octaves pour le bruit
    public float persistence = 0.5f;  /// Influence décroissante des octaves

    /// <summary>
    /// Fonction pour générer du bruit fractal à l'aide du bruit de Perlin
    /// </summary>
    /// <param name="x">Coordonnée x</param>
    /// <param name="z">Coordonnée z</param>
    /// <returns>Valeur normalisée du bruit</returns>
    float PerlinNoiseFractal(float x, float z)
    {
        float total = 0; /// Valeur totale du bruit
        float frequency = 1; /// Fréquence initiale
        float amplitude = 1; /// Amplitude initiale
        float maxValue = 0; /// Utilisé pour normaliser la valeur finale

        for (int i = 0; i < octaves; i++)
        {
            total += perlinNoise.Perlin3D(x * frequency, z * frequency, offset.y) * amplitude; /// Ajout du bruit à la valeur totale
            maxValue += amplitude; /// Mise à jour de la valeur maximale

            amplitude *= persistence; /// Ajustement de l'amplitude
            frequency *= 2; /// Augmentation de la fréquence
        }

        return total / maxValue; /// Normalisation de la valeur
    }
}
