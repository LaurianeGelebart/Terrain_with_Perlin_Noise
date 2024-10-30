using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Génère des nuages en utilisant le bruit de Perlin et des voxels
/// </summary>
public class CloudGenerator : MonoBehaviour
{
    public int width = 50;              // Largeur du nuage
    public int height = 20;             // Hauteur du nuage
    public int depth = 20;              // Profondeur du nuage
    public float perlinNoiseScale = 7f;  // Échelle du bruit de Perlin
    public float threshold = 0.7f;      // Seuil du bruit de Perlin pour générer un voxel
    public Vector3 offset;              // Décalage pour varier la génération des nuages en x, y, z
    public GameObject cloudPrefab;     // Préfabriqué pour chaque voxel de nuage
    public Material cloudMaterial;      // Matériau pour les nuages

    private PerlinNoise perlinNoise;    // Générateur de bruit de Perlin
    private MeshCombiner meshCombiner;  // Combinateur de meshes
    private List<GameObject> cloudVoxels = new List<GameObject>();  // Liste des voxels de nuage


    /// <summary>
    /// Démarre la génération des nuages
    /// Utilise une coroutine (methode de unity assynchrone qui étend les traitement sur plusieurs frames) pour ne pas bloquer le jeu
    /// </summary>
    void Start()
    {
        perlinNoise = new PerlinNoise();
        meshCombiner = gameObject.AddComponent<MeshCombiner>();
        StartCoroutine(GenerateCloudsCoroutine());
    }

    /// <summary>
    /// Génère les nuages avec du bruit de Perlin 3D
    /// Traitement assynchrone réparti sur plusiuers frames
    /// </summary>
    /// <returns>Coroutine pour générer les nuages sur plusieurs frames</returns>
    IEnumerator GenerateCloudsCoroutine()
    {
        Vector3 startingPosition = transform.position;

        for (int z = 0; z < depth; z++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (ShouldCreateVoxel(x, y, z, startingPosition))
                    {
                        CreateVoxel(x, y, z, startingPosition);
                        yield return null;  // Attend la prochaine frame avant de continuer
                    }
                }
            }
        }

        CombineAndOptimizeClouds(); 
    }

    /// <summary>
    /// Vérifie si un voxel doit être créé en fonction du bruit de Perlin
    /// </summary>
    /// <param name="x">x du voxel</param>
    /// <param name="y">y du voxel</param>
    /// <param name="z">z du voxel</param>
    /// <param name="startingPosition">Position de départ du nuage</param>
    /// <returns>Vrai si le voxel doit être créé faux sinon</returns>
    bool ShouldCreateVoxel(int x, int y, int z, Vector3 startingPosition)
    {
        float noiseValue = perlinNoise.Perlin3D(
            (x + startingPosition.x + offset.x) / perlinNoiseScale,
            (y + startingPosition.y + offset.y) / perlinNoiseScale,
            (z + startingPosition.z + offset.z) / perlinNoiseScale
        );

        return noiseValue > threshold;  // Si la valeur du bruit dépasse le seuil, un voxel sera créé
    }

    /// <summary>
    /// Crée un voxel de nuage à la position spécifiée avec une échelle aléatoire
    /// </summary>
    /// <param name="x">x du voxel</param>
    /// <param name="y">y du voxel</param>
    /// <param name="z">z du voxel </param>
    /// <param name="startingPosition">Position de départ du nuage</param>
    void CreateVoxel(int x, int y, int z, Vector3 startingPosition)
    {
        Vector3 positionVoxel = new Vector3(
            x + startingPosition.x,
            y + startingPosition.y,
            z + startingPosition.z
        );

        float randomScale = UnityEngine.Random.Range(2f, 8f);
        GameObject cloudVoxel = Instantiate(cloudPrefab, positionVoxel, Quaternion.identity);
        cloudVoxel.transform.localScale = Vector3.one * randomScale;
        cloudVoxel.transform.parent = this.transform;

        cloudVoxels.Add(cloudVoxel);
    }

    /// <summary>
    /// Combine tous les voxels de nuage pour optimiser la scène
    /// </summary>
    void CombineAndOptimizeClouds()
    {
        meshCombiner.CombineMeshes(this.transform, cloudMaterial); 

        foreach (GameObject voxel in cloudVoxels)
        {
            Destroy(voxel);  // Détruit les voxels après la combinaison pour améliorer les performances
        }
        cloudVoxels.Clear(); 
    }
}
