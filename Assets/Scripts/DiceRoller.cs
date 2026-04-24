using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gere le lancer de de, l'animation de defilement, et le systeme de relance.
/// Utilise une machine a etats pour eviter les conflits entre boutons.
/// </summary>
public class DiceRoller : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerController playerController;

    [Header("De")]
    public Button rollButton;
    public Text rollButtonLabel;
    public Text diceResultText;

    [Header("Relance")]
    public Button rerollButton;
    public Text rerollButtonLabel;
    public Text rerollCountText;

    [Header("HUD Top")]
    public Text goldText;
    public Text healthText;
    public Text loopText;
    public Text artifactText;
    public Text clueText;

    [Header("Info case")]
    public Text tileInfoText;

    private const int DiceMin = 1;
    private const int DiceMax = 6;
    private const float ScrollDuration = 0.8f;
    private const float ScrollInterval = 0.1f;
    private const float ResultDisplayDelay = 0.4f;
    private const int ResultFontSizeLarge = 48;
    private const int ResultFontSizeNormal = 28;
    private const string LabelRoll = "Lancer le d\u00e9";
    private const string LabelConfirm = "Valider \u2714";

    /// <summary>Etats possibles du systeme de de.</summary>
    private enum DiceState
    {
        /// <summary>En attente d'un lancer. Le bouton Roll est actif.</summary>
        Idle,
        /// <summary>Animation de defilement en cours. Tout est bloque.</summary>
        Rolling,
        /// <summary>Resultat affiche, le joueur peut relancer ou valider.</summary>
        WaitingForDecision,
        /// <summary>Le pion se deplace. Tout est bloque.</summary>
        Moving
    }

    private DiceState currentState = DiceState.Idle;
    private int lastRoll;
    private bool externalLock;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        UpdateHudTexts();
        UpdateButtonStates();
    }

    // ── HUD ─────────────────────────────────────────────────────

    private void UpdateHudTexts()
    {
        GameManager gm = GameManager.Instance;

        if (goldText != null)
            goldText.text = "Or : " + gm.Gold;
        if (healthText != null)
            healthText.text = "PV : " + gm.Health;
        if (loopText != null)
            loopText.text = "Boucle " + gm.LoopCount;
        if (artifactText != null)
            artifactText.text = "Artefacts : " + gm.artifactsCollected + "/3";
        if (clueText != null)
            clueText.text = "Indices : " + gm.cluesCollected + "/3";
        if (rerollCountText != null)
            rerollCountText.text = "Relances : " + gm.rerollsAvailable;
        if (rerollButtonLabel != null)
            rerollButtonLabel.text = "Relancer (" + gm.rerollsAvailable + ")";
    }

    // ── Machine a etats des boutons ────────────────────────────

    private void UpdateButtonStates()
    {
        bool playerReady = playerController != null && !playerController.isMoving;
        int rerolls = GameManager.Instance.rerollsAvailable;

        // Detecter la fin du deplacement pour revenir a Idle
        if (currentState == DiceState.Moving && playerReady)
        {
            currentState = DiceState.Idle;
            if (rollButtonLabel != null) rollButtonLabel.text = LabelRoll;
        }

        switch (currentState)
        {
            case DiceState.Idle:
                SetRollButton(playerReady && !externalLock);
                SetRerollVisible(false);
                break;

            case DiceState.Rolling:
                SetRollButton(false);
                SetRerollVisible(false);
                break;

            case DiceState.WaitingForDecision:
                SetRollButton(true);
                SetRerollVisible(rerolls > 0);
                break;

            case DiceState.Moving:
                SetRollButton(false);
                SetRerollVisible(false);
                break;
        }

        // Le compteur de relances est toujours visible si le joueur en possede
        if (rerollCountText != null)
            rerollCountText.gameObject.SetActive(rerolls > 0);
    }

    private void SetRollButton(bool interactable)
    {
        if (rollButton != null)
            rollButton.interactable = interactable;
    }

    private void SetRerollVisible(bool visible)
    {
        if (rerollButton == null) return;
        rerollButton.gameObject.SetActive(visible);
        if (visible)
            rerollButton.interactable = true;
    }

    // ── Actions joueur ──────────────────────────────────────────

    /// <summary>
    /// Appele par le bouton principal. Lance le de en Idle, ou valide le resultat en WaitingForDecision.
    /// </summary>
    public void RollDice()
    {
        // En attente de decision : le joueur confirme le resultat
        if (currentState == DiceState.WaitingForDecision)
        {
            ConfirmRoll();
            return;
        }

        // Lancer normal
        if (currentState != DiceState.Idle) return;
        if (playerController == null || playerController.isMoving) return;

        currentState = DiceState.Rolling;
        StartCoroutine(AnimateDice());
    }

    /// <summary>
    /// Appele par le bouton Relancer. Consomme une relance et relance le de.
    /// </summary>
    public void UseReroll()
    {
        if (currentState != DiceState.WaitingForDecision) return;
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.UseReroll()) return;

        currentState = DiceState.Rolling;
        StartCoroutine(AnimateDice());
    }

    // ── Animation et deplacement ────────────────────────────────

    private IEnumerator AnimateDice()
    {
        lastRoll = Random.Range(DiceMin, DiceMax + 1);

        float elapsed = 0f;
        while (elapsed < ScrollDuration)
        {
            if (diceResultText != null)
                diceResultText.text = Random.Range(DiceMin, DiceMax + 1).ToString();
            elapsed += ScrollInterval;
            yield return new WaitForSeconds(ScrollInterval);
        }

        if (diceResultText != null)
        {
            diceResultText.text = lastRoll.ToString();
            diceResultText.fontSize = ResultFontSizeLarge;
        }

        yield return new WaitForSeconds(ResultDisplayDelay);

        if (diceResultText != null)
            diceResultText.fontSize = ResultFontSizeNormal;

        // Si le joueur possede des relances, on attend sa decision
        if (GameManager.Instance != null && GameManager.Instance.rerollsAvailable > 0)
        {
            currentState = DiceState.WaitingForDecision;
            if (rollButtonLabel != null) rollButtonLabel.text = LabelConfirm;
        }
        else
        {
            // Pas de relance disponible : on avance directement (comportement original)
            ConfirmRoll();
        }
    }

    /// <summary>
    /// Confirme le resultat du de et deplace le joueur.
    /// </summary>
    private void ConfirmRoll()
    {
        currentState = DiceState.Moving;
        if (rollButtonLabel != null) rollButtonLabel.text = LabelRoll;
        playerController.MoveBySteps(lastRoll);
    }

    // ── API externe ─────────────────────────────────────────────

    /// <summary>
    /// Verrouille ou deverrouille le systeme de de depuis l'exterieur (shop, dialogues, mini-jeu).
    /// </summary>
    public void SetInteractable(bool value)
    {
        externalLock = !value;
    }
}
