using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Force TitleScene comme scène de démarrage dans l'éditeur Unity.
/// Quel que soit l'onglet ouvert, appuyer sur Play lancera toujours TitleScene en premier.
/// </summary>
[InitializeOnLoad]
public static class PlayModeStartScene
{
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";

    static PlayModeStartScene()
    {
        ApplyStartScene();
    }

    /// <summary>
    /// Applique TitleScene comme scène de démarrage du Play Mode.
    /// </summary>
    [MenuItem("Tools/Set Play Mode Start Scene → TitleScene")]
    public static void ApplyStartScene()
    {
        SceneAsset titleScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(TitleScenePath);

        if (titleScene != null)
            EditorSceneManager.playModeStartScene = titleScene;
        else
            UnityEngine.Debug.LogWarning($"PlayModeStartScene : scène introuvable à '{TitleScenePath}'.");
    }

    /// <summary>
    /// Supprime le forçage de scène de démarrage (retour au comportement par défaut).
    /// </summary>
    [MenuItem("Tools/Clear Play Mode Start Scene")]
    public static void ClearStartScene()
    {
        EditorSceneManager.playModeStartScene = null;
    }
}
