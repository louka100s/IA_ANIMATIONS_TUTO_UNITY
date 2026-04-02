using UnityEngine;
using UnityEngine.UI;

public class MiniGameHUD : MonoBehaviour
{
    /// <summary>Texte affichant le temps restant.</summary>
    public Text timerText;

    /// <summary>Texte affichant le statut du mini-jeu.</summary>
    public Text statusText;

    /// <summary>Texte "Clic droit - Se cacher", affiche quand une caisse est proche.</summary>
    public Text cratePrompt;

    /// <summary>Decompte de cachette (5... 4... 3...), affiche quand le joueur est cache.</summary>
    public Text hideTimerText;

    /// <summary>Duree maximale du mini-jeu en secondes.</summary>
    public float maxTime = 60f;

    private float timeLeft;
    private bool gameOver = false;
    private MiniGamePlayer player;

    private const float WarningThreshold = 10f;
    private const float MinTime = 30f;
    private const float TimeReductionPerAttempt = 5f;

    private void Start()
    {
        float reduction = 0f;
        if (GameManager.Instance != null)
            reduction = GameManager.Instance.miniGameAttempts * TimeReductionPerAttempt;

        timeLeft = Mathf.Max(MinTime, maxTime - reduction);

        player = FindFirstObjectByType<MiniGamePlayer>();

        if (statusText != null)
        {
            statusText.text = "Atteins la zone verte !";
            statusText.color = Color.white;
        }

        if (cratePrompt != null) cratePrompt.gameObject.SetActive(false);
        if (hideTimerText != null) hideTimerText.gameObject.SetActive(false);

        Debug.Log("MiniGameHUD : temps = " + timeLeft + "s (tentative "
            + (GameManager.Instance != null ? GameManager.Instance.miniGameAttempts : 0) + ")");
    }

    private void Update()
    {
        if (gameOver) return;

        UpdateHideUI();
        UpdateMainTimer();
    }

    /// <summary>Met a jour le prompt de caisse et le decompte de cachette.</summary>
    private void UpdateHideUI()
    {
        if (player == null) return;

        if (player.isHiding)
        {
            if (hideTimerText != null)
            {
                hideTimerText.gameObject.SetActive(true);
                int seconds = Mathf.CeilToInt(player.hideTimer);
                hideTimerText.text = seconds.ToString();
                hideTimerText.color = seconds <= 2 ? Color.red : Color.white;
            }

            if (cratePrompt != null) cratePrompt.gameObject.SetActive(false);

            if (statusText != null)
            {
                if (player.wasSeenEntering)
                {
                    statusText.text  = "REPERE !";
                    statusText.color = Color.red;
                }
                else
                {
                    statusText.text  = "Cache !";
                    statusText.color = Color.green;
                }
            }
        }
        else
        {
            if (hideTimerText != null) hideTimerText.gameObject.SetActive(false);

            if (cratePrompt != null)
                cratePrompt.gameObject.SetActive(player.IsCrateNearby);

            if (statusText != null
                && (statusText.text == "Cache !" || statusText.text == "REPERE !"))
            {
                statusText.text  = "Atteins la zone verte !";
                statusText.color = Color.white;
            }
        }
    }

    /// <summary>Decremente le timer principal et declenche la defaite si ecoule.</summary>
    private void UpdateMainTimer()
    {
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

            if (timerText != null) timerText.text = "0s";

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

    /// <summary>Permet de changer le statut depuis d'autres scripts.</summary>
    public void SetStatus(string msg, Color color)
    {
        if (statusText != null)
        {
            statusText.text = msg;
            statusText.color = color;
        }
    }
}
