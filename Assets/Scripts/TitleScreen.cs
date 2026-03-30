using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    /// <summary>
    /// Texte "Appuyez sur n'importe quelle touche..." qui clignote.
    /// </summary>
    public Text promptText;

    private const float BlinkSpeed = 2f;
    private const float AlphaMin = 0.3f;
    private const float AlphaMax = 1f;
    private const string NextScene = "SampleScene";

    private float timer = 0f;

    private void Update()
    {
        // Clignotement sinusoidal du texte prompt
        timer += Time.deltaTime * BlinkSpeed;
        float alpha = Mathf.Lerp(AlphaMin, AlphaMax, (Mathf.Sin(timer) + 1f) / 2f);

        if (promptText != null)
            promptText.color = new Color(1f, 1f, 1f, alpha);

        // N'importe quelle touche ou clic → lancer le jeu
        if (Input.anyKeyDown)
            SceneManager.LoadScene(NextScene);
    }
}
