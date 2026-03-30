using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private const float Spacing = 1.2f;
    public const int TotalTiles = 20;
    private const float ParticleHeightOffset = 0.1f;

    // Dimensions du rectangle : 7 cases en bas et en haut, 4 a droite et a gauche (coins partages)
    // Bas(7) + Droite sans coins(3) + Haut(7) + Gauche sans coins(3) = 20 cases
    private const int LoopWidth = 7;  // cases sur les cotes horizontaux (incluant coins)
    private const int LoopHeight = 5; // cases sur les cotes verticaux (incluant coins)

    public List<Vector3> tilePositions = new List<Vector3>();
    public List<string> tileTypes = new List<string>();

    /// <summary>Prefab de particules pour les cases "gold".</summary>
    public GameObject goldParticles;

    /// <summary>Prefab de particules pour les cases "heal".</summary>
    public GameObject healParticles;

    /// <summary>Prefab de particules pour les cases "danger".</summary>
    public GameObject dangerParticles;

    /// <summary>Prefab de particules pour les cases "artifact".</summary>
    public GameObject artifactParticles;

    /// <summary>Prefab de particules pour les cases "clue".</summary>
    public GameObject clueParticles;

    /// <summary>Prefab de particules pour les cases "minigame".</summary>
    public GameObject minigameParticles;

    private void Awake()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        BuildPositions();
        BuildTypes();
        SpawnTiles();
    }

    // Construit les 20 positions dans le sens horaire.
    // w=6 pas horizontaux, h=4 pas verticaux.
    // Bas(7) + Droite sans coins(3) + Haut(7) + Gauche sans coins(3) = 20
    private void BuildPositions()
    {
        int w = LoopWidth - 1;  // nb de pas horizontaux = 6
        int h = LoopHeight - 1; // nb de pas verticaux   = 4

        // Bas : gauche → droite  (indices 0–6, 7 cases)
        for (int i = 0; i <= w; i++)
            tilePositions.Add(new Vector3(i * Spacing, 0f, 0f));

        // Droite : bas → haut, coins exclus (indices 7–9, 3 cases)
        for (int i = 1; i < h; i++)
            tilePositions.Add(new Vector3(w * Spacing, 0f, i * Spacing));

        // Haut : droite → gauche (indices 10–16, 7 cases)
        for (int i = w; i >= 0; i--)
            tilePositions.Add(new Vector3(i * Spacing, 0f, h * Spacing));

        // Gauche : haut → bas, coins exclus (indices 17–19, 3 cases)
        for (int i = h - 1; i >= 1; i--)
            tilePositions.Add(new Vector3(0f, 0f, i * Spacing));
    }

    private void BuildTypes()
    {
        // 20 types : start, dialogue, clue, artifact, danger, heal, minigame, normal
        string[] types = new string[]
        {
            "start",    // 0
            "minigame", // 1  - Mini-jeu 1
            "dialogue", // 2
            "clue",     // 3  - Indice 0
            "artifact", // 4  - Artefact 0
            "danger",   // 5
            "normal",   // 6
            "heal",     // 7
            "normal",   // 8
            "minigame", // 9  - Mini-jeu 2
            "clue",     // 10 - Indice 1
            "dialogue", // 11
            "artifact", // 12 - Artefact 1
            "danger",   // 13
            "heal",     // 14
            "minigame", // 15 - Mini-jeu 3
            "artifact", // 16 - Artefact 2
            "dialogue", // 17
            "danger",   // 18
            "clue"      // 19 - Indice 2
        };

        tileTypes.Clear();
        foreach (string t in types)
            tileTypes.Add(t);
    }

    // Instancie les cubes aplatis et les colorie
    private void SpawnTiles()
    {
        for (int i = 0; i < TotalTiles; i++)
        {
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = "Tile_" + i + "_" + tileTypes[i];
            tile.transform.SetParent(transform);
            tile.transform.localPosition = tilePositions[i];
            tile.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);
            tile.GetComponent<Renderer>().material.color = GetTileColor(tileTypes[i]);

            SpawnTileParticles(tileTypes[i], tilePositions[i], tile.transform);
        }
    }

    private void SpawnTileParticles(string tileType, Vector3 position, Transform parent)
    {
        GameObject prefab = tileType switch
        {
            "gold"     => goldParticles,
            "heal"     => healParticles,
            "danger"   => dangerParticles,
            "artifact" => artifactParticles,
            "clue"     => clueParticles,
            "minigame" => minigameParticles,
            _          => null
        };

        if (prefab == null) return;

        Instantiate(prefab, position + Vector3.up * ParticleHeightOffset, Quaternion.identity, parent);
    }

    private Color GetTileColor(string tileType)
    {
        switch (tileType)
        {
            case "start":    return new Color(0.80f, 0.80f, 0.80f); // gris clair #CCCCCC
            case "normal":   return new Color(0.80f, 0.80f, 0.80f); // gris clair
            case "dialogue": return new Color(0.27f, 0.53f, 0.80f); // bleu #4488CC
            case "clue":     return new Color(0.20f, 0.80f, 0.67f); // cyan #33CCAA
            case "artifact": return new Color(0.80f, 0.53f, 0.20f); // orange dore #CC8833
            case "danger":   return new Color(0.53f, 0.20f, 0.67f); // violet #8833AA
            case "heal":     return new Color(0.20f, 0.67f, 0.33f); // vert clair #33AA55
            case "minigame": return new Color(0.80f, 0.20f, 0.20f); // rouge #CC3333
            default:         return new Color(0.80f, 0.80f, 0.80f);
        }
    }

    /// <summary>Retourne la position d'une case avec modulo pour boucler.</summary>
    public Vector3 GetTilePosition(int index)
    {
        return tilePositions[index % TotalTiles];
    }

    /// <summary>Retourne le type d'une case avec modulo pour boucler.</summary>
    public string GetTileType(int index)
    {
        return tileTypes[index % TotalTiles];
    }
}
