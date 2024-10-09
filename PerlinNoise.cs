using UnityEngine;

public class PerlinNoise
{
    private int[] permutationTable;  

    /// <summary>
    /// Initialise une nouvelle instance de la classe PerlinNoise, en générant la table de permutation
    /// </summary>
    public PerlinNoise()
    {
        GeneratePermutationTable();
    }

    /// <summary>
    /// Génère la table de permutation nécessaire pour le bruit de Perlin
    /// Utilisée pour créer des gradients pseudo-aléatoires pour le calcul du bruit
    /// </summary>
    private void GeneratePermutationTable()
    {
        permutationTable = new int[512];
        int[] p = {151, 160, 137, 91, 90, 15,
            131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240,
            21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117,
            35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68,
            175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
            60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65,
            25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
            135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217,
            226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206,
            59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248,
            152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22,
            39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218,
            246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
            81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106,
            157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236,
            205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215,
            61, 156, 180};

        // Création de la table de permutation en dupliquant le tableau original
        for (int i = 0; i < 256; i++)
        {
            permutationTable[i] = p[i];
            permutationTable[i + 256] = p[i]; 
        }
    }

    /// <summary>
    /// Calcule le bruit de Perlin à partir des coordonnées données en paramètres
    /// Retourne un float normalisé entre 0 et 1 qui repésente l'intensité du bruit de Perlin à ce point dans l'espace 3D
    /// </summary>
    /// <param name="x">Coordonnée x</param>
    /// <param name="y">Coordonnée y</param>
    /// <param name="z">Coordonnée z</param>
    /// <returns>Valeur du bruit de Perlin aux coordonnées données</returns>
    public float Perlin3D(float x, float y, float z)
    {
        // Détermine des coordonnées de la cellule de la grille
        int xi = Mathf.FloorToInt(x) & 255;
        int yi = Mathf.FloorToInt(y) & 255;
        int zi = Mathf.FloorToInt(z) & 255;

        // Calcul des coordonnées relatives à l'intérieur de la cellule de la grille
        float xf = x - Mathf.Floor(x);
        float yf = y - Mathf.Floor(y);
        float zf = z - Mathf.Floor(z);

        // Applique la fonction fade pour adoucir les résultats
        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);

        // Hash des coordonnées des 8 coins du cube qui entoure x, y,z
        int aaa = permutationTable[permutationTable[permutationTable[xi] + yi] + zi];
        int aba = permutationTable[permutationTable[permutationTable[xi] + Inc(yi)] + zi];
        int aab = permutationTable[permutationTable[permutationTable[xi] + yi] + Inc(zi)];
        int abb = permutationTable[permutationTable[permutationTable[xi] + Inc(yi)] + Inc(zi)];
        int baa = permutationTable[permutationTable[permutationTable[Inc(xi)] + yi] + zi];
        int bba = permutationTable[permutationTable[permutationTable[Inc(xi)] + Inc(yi)] + zi];
        int bab = permutationTable[permutationTable[permutationTable[Inc(xi)] + yi] + Inc(zi)];
        int bbb = permutationTable[permutationTable[permutationTable[Inc(xi)] + Inc(yi)] + Inc(zi)];

        // Interpolation entre les résultats des huit voisins
        float x1 = Lerp(Grad(aaa, xf, yf, zf), Grad(baa, xf - 1, yf, zf), u);
        float x2 = Lerp(Grad(aba, xf, yf - 1, zf), Grad(bba, xf - 1, yf - 1, zf), u);
        float y1 = Lerp(x1, x2, v);

        x1 = Lerp(Grad(aab, xf, yf, zf - 1), Grad(bab, xf - 1, yf, zf - 1), u);
        x2 = Lerp(Grad(abb, xf, yf - 1, zf - 1), Grad(bbb, xf - 1, yf - 1, zf - 1), u);
        float y2 = Lerp(x1, x2, v);

        // Retourne la valeur interpolée et noramlisée finale
        return (Lerp(y1, y2, w) + 1) / 2;
    }


    /// <summary>
    /// Adoucit la courbe (fade de la valeur d'entrée)
    /// </summary>
    /// <param name="t">La valeur à adoucir</param>
    /// <returns>La valeur adoucie</returns>
    private float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10); // 6t^5 - 15t^4 + 10t^3
    }

    /// <summary>
    /// Interpolation de deux valeurs en fonction du facteur t
    /// </summary>
    /// <param name="a">Première valeur</param>
    /// <param name="b">Deuxième valeur</param>
    /// <param name="t">Facteur d'interpolation</param>
    /// <returns>Valeur interpolée</returns>
    private float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    /// <summary>
    /// Retourne un gradient basé sur la valeur de hachage et les coordonnées
    /// Utilisée pour calculer les contributions des coins du cube
    /// </summary>
    /// <param name="hash">Valeur de hachage</param>
    /// <param name="x">Coordonnée x</param>
    /// <param name="y">Coordonnée y</param>
    /// <param name="z">Coordonnée z</param>
    /// <returns>Gradient</returns>
    private float Grad(int hash, float x, float y, float z)
    {
        int h = hash & 15; // Récupère les 4 bits inférieurs du hash
        float u = h < 8 ? x : y; // Sélectionne une des coordonnées
        float v = h < 4 ? y : (h == 12 || h == 14 ? x : z); // Sélectionne l'autre coordonnée
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v); // Combine les coordonnées
    }

    /// <summary>
    /// Incrémente un nombre et le ramène à 0 si la valeur dépasse 255 (plage valide pour la table de permutation)
    /// </summary>
    /// <param name="num">Nombre à incrémenter</param>
    /// <returns>Nombre incrémenté compris entre 0 et 255</returns>
    private int Inc(int num)
    {
        return (num + 1) & 255; 
    }
}
