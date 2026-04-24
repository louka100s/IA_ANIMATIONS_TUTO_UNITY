using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le jeu Pierre Feuille Ciseaux en best-of-5 (premier à 3 victoires).
/// Indices des choix : 0 = Pierre, 1 = Feuille, 2 = Ciseaux.
/// Règle : Pierre bat Ciseaux, Ciseaux bat Feuille, Feuille bat Pierre.
/// </summary>
public class RPSGame : MonoBehaviour
{
    [Header("Textes résultats")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI scoreText;

    [Header("Boutons de choix")]
    public Button btnRock;
    public Button btnPaper;
    public Button btnScissors;

    [Header("Bouton retour")]
    public Button btnBack;

    // ── Constantes ──────────────────────────────────────────────
    private const int WinTarget = 3;
    private const int GoldWin   =  50;
    private const int GoldLoss  = -25;

    private static readonly string[] ChoiceNames  = { "Pierre", "Feuille", "Ciseaux" };
    private static readonly string[] ChoiceEmojis = { "🪨", "📄", "✂️" };

    // ── État ─────────────────────────────────────────────────────
    private int playerScore = 0;
    private int aiScore     = 0;

    // ── Lifecycle ────────────────────────────────────────────────

    private void Start()
    {
        btnRock    .onClick.AddListener(() => PlayRound(0));
        btnPaper   .onClick.AddListener(() => PlayRound(1));
        btnScissors.onClick.AddListener(() => PlayRound(2));
        btnBack    .onClick.AddListener(GoBack);

        btnBack.gameObject.SetActive(false);

        UpdateScoreText();
    }

    // ── Logique ──────────────────────────────────────────────────

    /// <summary>Joue un round et met à jour l'UI.</summary>
    private void PlayRound(int playerChoice)
    {
        int aiChoice = Random.Range(0, 3);

        // Égalité
        if (playerChoice == aiChoice)
        {
            resultText.text  = "Égalité ! (" + ChoiceEmojis[aiChoice] + " " + ChoiceNames[aiChoice] + ")";
            resultText.color = Color.white;
            return;
        }

        // Pierre(0) bat Ciseaux(2), Ciseaux(2) bat Feuille(1), Feuille(1) bat Pierre(0)
        // => playerWins si (playerChoice - aiChoice + 3) % 3 == 1
        bool playerWins = (playerChoice - aiChoice + 3) % 3 == 1;

        if (playerWins)
        {
            playerScore++;
            resultText.text  = "Tu gagnes ! "
                + ChoiceEmojis[playerChoice] + " " + ChoiceNames[playerChoice]
                + " bat "
                + ChoiceEmojis[aiChoice] + " " + ChoiceNames[aiChoice];
            resultText.color = Color.green;
        }
        else
        {
            aiScore++;
            resultText.text  = "L'IA gagne ! "
                + ChoiceEmojis[aiChoice] + " " + ChoiceNames[aiChoice]
                + " bat "
                + ChoiceEmojis[playerChoice] + " " + ChoiceNames[playerChoice];
            resultText.color = Color.red;
        }

        UpdateScoreText();
        CheckEndCondition();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Toi : " + playerScore + "  —  IA : " + aiScore;
    }

    private void CheckEndCondition()
    {
        if (playerScore < WinTarget && aiScore < WinTarget) return;

        // Désactiver les choix
        btnRock    .interactable = false;
        btnPaper   .interactable = false;
        btnScissors.interactable = false;
        btnBack.gameObject.SetActive(true);

        bool victory = playerScore >= WinTarget;

        if (victory)
        {
            resultText.text  = "VICTOIRE ! +" + GoldWin + " Or";
            resultText.color = Color.green;
        }
        else
        {
            resultText.text  = "DÉFAITE... " + GoldLoss + " Or";
            resultText.color = Color.red;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.EndMiniGame(victory);
    }

    private void GoBack()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMain();
    }
}
