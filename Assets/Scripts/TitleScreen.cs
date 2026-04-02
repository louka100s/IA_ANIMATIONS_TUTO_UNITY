using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    /// <summary>Texte clignotant "Appuyez sur une touche...".</summary>
    public Text promptText;

    /// <summary>
    /// Bouton "Nouvelle partie" — visible uniquement si une sauvegarde existe.
    /// Son OnClick doit appeler TitleScreen.OnNewGame().
    /// </summary>
    public GameObject newGameButton;

    private const float BlinkSpeed = 2f;
    private const float AlphaMin  = 0.3f;
    private const float AlphaMax  = 1f;
    private const string NextScene = "SampleScene";

    private float timer = 0f;

    private void Start()
    {
        if (newGameButton != null)
            newGameButton.SetActive(SaveSystem.HasSave());
    }

    private void Update()
    {
        // Clignotement sinusoïdal
        timer += Time.deltaTime * BlinkSpeed;
        float alpha = Mathf.Lerp(AlphaMin, AlphaMax, (Mathf.Sin(timer) + 1f) / 2f);
        if (promptText != null)
            promptText.color = new Color(1f, 1f, 1f, alpha);

        // Nouveau Input System — n'importe quelle touche clavier ou clic gauche
        bool anyKey = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                   || (Mouse.current   != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (anyKey)
            SceneManager.LoadScene(NextScene);
    }

    /// <summary>
    /// Appelé par le bouton "Nouvelle partie".
    /// Supprime la sauvegarde et lance une partie fraîche.
    /// </summary>
    public void OnNewGame()
    {
        SaveSystem.Delete();
        SceneManager.LoadScene(NextScene);
    }
}
