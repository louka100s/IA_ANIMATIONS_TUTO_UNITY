using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerController playerController;

    [Header("De")]
    public Button rollButton;
    public Text diceResultText;

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

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (goldText != null)
            goldText.text = "Or : " + GameManager.Instance.Gold;

        if (healthText != null)
            healthText.text = "PV : " + GameManager.Instance.Health;

        if (loopText != null)
            loopText.text = "Boucle " + GameManager.Instance.LoopCount;

        if (artifactText != null)
            artifactText.text = "Artefacts : " + GameManager.Instance.artifactsCollected + "/3";

        if (clueText != null)
            clueText.text = "Indices : " + GameManager.Instance.cluesCollected + "/3";

        if (playerController != null && rollButton != null)
            rollButton.interactable = !playerController.isMoving;
    }

    /// <summary>
    /// Appele par le bouton Lancer le de. Lance l'animation de defilement puis deplace le joueur.
    /// </summary>
    public void RollDice()
    {
        if (playerController == null || playerController.isMoving) return;

        rollButton.interactable = false;
        StartCoroutine(AnimateDice());
    }

    private IEnumerator AnimateDice()
    {
        int finalRoll = Random.Range(DiceMin, DiceMax + 1);

        // Defiler des chiffres aleatoires pendant ScrollDuration secondes
        float elapsed = 0f;
        while (elapsed < ScrollDuration)
        {
            if (diceResultText != null)
                diceResultText.text = Random.Range(DiceMin, DiceMax + 1).ToString();
            elapsed += ScrollInterval;
            yield return new WaitForSeconds(ScrollInterval);
        }

        // Afficher le vrai resultat en grand puis revenir a la taille normale
        if (diceResultText != null)
        {
            diceResultText.text = finalRoll.ToString();
            diceResultText.fontSize = ResultFontSizeLarge;
        }

        yield return new WaitForSeconds(ResultDisplayDelay);

        if (diceResultText != null)
            diceResultText.fontSize = ResultFontSizeNormal;

        playerController.MoveBySteps(finalRoll);
    }

    /// <summary>
    /// Active ou desactive le bouton depuis l'exterieur (dialogues, mini-jeu, etc.).
    /// </summary>
    public void SetInteractable(bool value)
    {
        if (rollButton != null)
            rollButton.interactable = value;
    }
}
