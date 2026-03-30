using UnityEngine;
using UnityEngine.UI;

public class MiniGameHUD : MonoBehaviour
{
    /// <summary>
    /// Texte affichant le temps restant.
    /// </summary>
    public Text timerText;

    /// <summary>
    /// Texte affichant le statut du mini-jeu.
    /// </summary>
    public Text statusText;

    /// <summary>
    /// Duree maximale du mini-jeu en secondes.
    /// </summary>
    public float maxTime = 60f;

    private float timeLeft;
    private bool gameOver = false;

    private const float WarningThreshold = 10f;
    private const float MinTime = 30f;
    private const float TimeReductionPerAttempt = 5f;

    private void Start()
    {
        float reduction = 0f;
        if (GameManager.Instance != null)
            reduction = GameManager.Instance.miniGameAttempts * TimeReductionPerAttempt;

        timeLeft = Mathf.Max(MinTime, maxTime - reduction);

        if (statusText != null)
        {
            statusText.text = "Atteins la zone verte !";
            statusText.color = Color.white;
        }

        Debug.Log("MiniGameHUD : temps = " + timeLeft + "s (tentative "
            + (GameManager.Instance != null ? GameManager.Instance.miniGameAttempts : 0) + ")");
    }

    private void Update()
    {
        if (gameOver) return;

        timeLeft -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)).ToString() + "s";

            if (timeLeft <= WarningThreshold)
                timerText.color = Color.red;
        }

        if (timeLeft <= 0f)
        {
            gameOver = true;

            if (timerText != null)
                timerText.text = "0s";

            if (statusText != null)
            {
                statusText.text = "TEMPS ECOULE !";
                statusText.color = Color.red;
            }

            if (GameManager.Instance != null)
                GameManager.Instance.EndMiniGame(false);

            ResultScreen resultScreen = FindFirstObjectByType<ResultScreen>();
            if (resultScreen != null)
                resultScreen.ShowDefeat();
        }
    }

    /// <summary>
    /// Permet de changer le statut depuis d'autres scripts.
    /// </summary>
    public void SetStatus(string msg, Color color)
    {
        if (statusText != null)
        {
            statusText.text = msg;
            statusText.color = color;
        }
    }
}
