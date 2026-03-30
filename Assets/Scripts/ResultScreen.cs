using UnityEngine;
using UnityEngine.UI;

public class ResultScreen : MonoBehaviour
{
    /// <summary>
    /// Panel de resultat (plein ecran, noir semi-transparent).
    /// </summary>
    public GameObject resultPanel;

    /// <summary>
    /// Titre du resultat (VICTOIRE / DEFAITE).
    /// </summary>
    public Text resultTitle;

    /// <summary>
    /// Texte de recompense/penalite.
    /// </summary>
    public Text rewardText;

    /// <summary>
    /// Bouton pour continuer vers la scene principale.
    /// </summary>
    public Button continueButton;

    private void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
    }

    /// <summary>
    /// Affiche l'ecran de victoire.
    /// </summary>
    public void ShowVictory()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultTitle != null)
        {
            resultTitle.text = "VICTOIRE !";
            resultTitle.color = Color.green;
        }

        if (rewardText != null)
        {
            rewardText.text = "+50 Or";
            rewardText.color = new Color(1f, 0.843f, 0f); // jaune dore
        }

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Affiche l'ecran de defaite.
    /// </summary>
    public void ShowDefeat()
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultTitle != null)
        {
            resultTitle.text = "DEFAITE...";
            resultTitle.color = Color.red;
        }

        if (rewardText != null)
        {
            rewardText.text = "-25 Or";
            rewardText.color = Color.red;
        }

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Appele par le bouton Continuer : reprend le temps et retourne a la scene principale.
    /// </summary>
    private void OnContinue()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMain();
    }
}
