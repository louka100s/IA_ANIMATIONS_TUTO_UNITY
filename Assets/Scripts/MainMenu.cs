using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contrôleur du menu principal.
/// Gère l'affichage conditionnel des boutons selon l'existence d'une sauvegarde,
/// et délègue la navigation à GameManager / SceneTransition.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Boutons")]
    public Button continueButton;
    public Button newGameButton;
    public Button quitButton;

    [Header("Panneau de confirmation nouvelle partie")]
    public GameObject confirmPanel;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Version")]
    public TextMeshProUGUI versionText;

    private const string GameScene = "SampleScene";

    private void Start()
    {
        RefreshButtons();

        continueButton .onClick.AddListener(OnContinue);
        newGameButton  .onClick.AddListener(OnNewGameRequest);
        quitButton     .onClick.AddListener(OnQuit);
        confirmYesButton.onClick.AddListener(OnNewGameConfirmed);
        confirmNoButton .onClick.AddListener(OnNewGameCancelled);

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (versionText != null)
            versionText.text = "v" + Application.version;
    }

    // ── Boutons principaux ───────────────────────────────────────

    /// <summary>Reprend la partie sauvegardée.</summary>
    private void OnContinue()
    {
        // La save est déjà chargée dans GameManager.Awake — on lance directement.
        LoadGame();
    }

    /// <summary>
    /// Affiche le panneau de confirmation si une save existe,
    /// sinon lance directement une nouvelle partie.
    /// </summary>
    private void OnNewGameRequest()
    {
        if (SaveSystem.HasSave() && confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
        else
        {
            StartNewGame();
        }
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Panneau de confirmation ──────────────────────────────────

    private void OnNewGameConfirmed()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        StartNewGame();
    }

    private void OnNewGameCancelled()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    // ── Helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Active/désactive le bouton Continuer selon la présence d'une sauvegarde.
    /// </summary>
    private void RefreshButtons()
    {
        bool hasSave = SaveSystem.HasSave();

        if (continueButton != null)
            continueButton.gameObject.SetActive(hasSave);

        if (newGameButton != null)
            newGameButton.gameObject.SetActive(true);
    }

    private void StartNewGame()
    {
        SaveSystem.Delete();

        // Réinitialiser le GameManager pour repartir de zéro
        if (GameManager.Instance != null)
            GameManager.Instance.ResetState();

        LoadGame();
    }

    private void LoadGame()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.TransitionTo(GameScene);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameScene);
    }
}
