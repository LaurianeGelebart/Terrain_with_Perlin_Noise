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
        meshCombiner = gameObject.AddComponent<MeshCombiner>(); // Ajoute le composant MeshCombiner
        StartCoroutine(GenerateCloudsCoroutine()); // Lance la génération des nuages en assynchrone
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
                    // Utilise le bruit de perlin pour décider si un voxel est créé
                    float noiseValue = perlinNoise.Perlin3D(
                        (x + startingPosition.x + offset.x) / perlinNoiseScale,
                        (y + startingPosition.y + offset.y) / perlinNoiseScale,
                        (z + startingPosition.z + offset.z) / perlinNoiseScale
                    );

                    if (noiseValue > threshold)
                    {
                        Vector3 positionVoxel = new Vector3(
                            x + startingPosition.x,
                            y + startingPosition.y,
                            z + startingPosition.z
                        );

                        // Crée le voxel du nuage à la position calculée
                        float randomScale = UnityEngine.Random.Range(2f, 8f);
                        GameObject cloudVoxel = Instantiate(cloudPrefab, positionVoxel, Quaternion.identity);
                        cloudVoxel.transform.localScale = Vector3.one * randomScale;
                        cloudVoxel.transform.parent = this.transform;

                        cloudVoxels.Add(cloudVoxel);

                        yield return null;  // Attend la prochaine frame avant de continuer
                    }
                }
            }
        }

        // Combine tous les voxels pour optimiser
        meshCombiner.CombineMeshes(this.transform, cloudMaterial);

        // Détruit les voxels après  combinaison
        foreach (GameObject voxel in cloudVoxels)
        {
            Destroy(voxel);
        }
    }
}
