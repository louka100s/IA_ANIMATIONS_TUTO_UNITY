using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    /// <summary>
    /// Image noire plein ecran utilisee pour le fondu.
    /// </summary>
    public Image fadeImage;

    /// <summary>
    /// Duree du fondu entrant et sortant en secondes.
    /// </summary>
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Lance une transition fondu → chargement → fondu vers la scene cible.
    /// </summary>
    public void TransitionTo(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        // Fondu vers le noir
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            SetAlpha(Mathf.Clamp01(t));
            yield return null;
        }

        SetAlpha(1f);
        SceneManager.LoadScene(sceneName);

        // Laisser une frame pour que la scene soit chargee
        yield return null;

        // Fondu depuis le noir
        t = 1f;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime / fadeDuration;
            SetAlpha(Mathf.Clamp01(t));
            yield return null;
        }

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
    }
}
