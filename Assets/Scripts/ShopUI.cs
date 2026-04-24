using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panneau du shop accessible depuis le plateau.
/// Gère l'affichage et les achats de Potion de soin et de Relance de dé.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Panneau")]
    public GameObject shopPanel;

    [Header("Boutons d'achat")]
    public Button buyPotionButton;
    public Button buyRerollButton;
    public Button closeButton;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackText;

    private DiceRoller diceRoller;

    private const float FeedbackDuration = 2f;

    private void Awake()
    {
        diceRoller = FindFirstObjectByType<DiceRoller>();

        buyPotionButton.onClick.AddListener(OnBuyPotion);
        buyRerollButton.onClick.AddListener(OnBuyReroll);
        closeButton    .onClick.AddListener(CloseShop);

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void Update()
    {
        if (shopPanel == null || !shopPanel.activeSelf) return;
        if (GameManager.Instance == null) return;

        GameManager gm = GameManager.Instance;

        // Griser les boutons si l'achat est impossible
        if (buyPotionButton != null)
            buyPotionButton.interactable = gm.Gold >= 30 && gm.Health < 100;

        if (buyRerollButton != null)
            buyRerollButton.interactable = gm.Gold >= 20;
    }

    /// <summary>Ouvre le shop et bloque le dé.</summary>
    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (diceRoller != null)
            diceRoller.SetInteractable(false);

        ClearFeedback();
    }

    /// <summary>Ferme le shop et rend la main au joueur.</summary>
    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (diceRoller != null)
            diceRoller.SetInteractable(true);
    }

    // ── Achats ──────────────────────────────────────────────────

    private void OnBuyPotion()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.BuyHealthPotion())
            ShowFeedback("Potion bue ! +30 PV  (-30 or)", new Color(0.2f, 0.8f, 0.3f));
        else if (GameManager.Instance.Health >= 100)
            ShowFeedback("PV déjà au maximum.", Color.yellow);
        else
            ShowFeedback("Pas assez d'or (30 requis).", Color.red);
    }

    private void OnBuyReroll()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.BuyReroll())
            ShowFeedback("Relance achetée ! (-20 or)", new Color(0.4f, 0.7f, 1f));
        else
            ShowFeedback("Pas assez d'or (20 requis).", Color.red);
    }

    // ── Feedback ────────────────────────────────────────────────

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;

        StopAllCoroutines();
        feedbackText.text  = message;
        feedbackText.color = color;
        StartCoroutine(ClearFeedbackAfterDelay());
    }

    private System.Collections.IEnumerator ClearFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(FeedbackDuration);
        ClearFeedback();
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = string.Empty;
    }
}
