using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int gold;
    public int health;
    public int loopCount;
    public int lastTileIndex;
    public int artifactsCollected;
    public bool[] artifactFound;
    public int cluesCollected;
    public bool[] clueFound;
}

/// <summary>
/// Classe statique de sauvegarde/chargement JSON vers Application.persistentDataPath.
/// Aucun MonoBehaviour — appel direct depuis GameManager.
/// </summary>
public static class SaveSystem
{
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    /// <summary>Sérialise l'état courant du GameManager dans save.json.</summary>
    public static void Save()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        SaveData data = new SaveData
        {
            gold              = gm.Gold,
            health            = gm.Health,
            loopCount         = gm.LoopCount,
            lastTileIndex     = gm.LastTileIndex,
            artifactsCollected = gm.artifactsCollected,
            artifactFound     = gm.artifactFound,
            cluesCollected    = gm.cluesCollected,
            clueFound         = gm.clueFound
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log("SaveSystem : sauvegardé → " + SavePath);
    }

    /// <summary>Charge save.json et injecte les données dans le GameManager.</summary>
    public static void Load()
    {
        if (!File.Exists(SavePath)) return;

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

        gm.Gold              = data.gold;
        gm.Health            = data.health;
        gm.SetLoopCount(data.loopCount);
        gm.LastTileIndex     = data.lastTileIndex;
        gm.artifactsCollected = data.artifactsCollected;
        gm.artifactFound     = data.artifactFound ?? gm.artifactFound;
        gm.cluesCollected    = data.cluesCollected;
        gm.clueFound         = data.clueFound ?? gm.clueFound;

        Debug.Log("SaveSystem : chargé depuis " + SavePath);
    }

    /// <summary>Supprime la sauvegarde existante (nouvelle partie).</summary>
    public static void Delete()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("SaveSystem : sauvegarde supprimée.");
        }
    }

    /// <summary>Retourne true si un fichier de sauvegarde existe.</summary>
    public static bool HasSave() => File.Exists(SavePath);
}
