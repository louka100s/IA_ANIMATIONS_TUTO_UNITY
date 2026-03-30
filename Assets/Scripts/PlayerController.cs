using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public BoardManager board;

    [Header("Feedback — Floating Text")]
    public GameObject floatingTextPrefab;

    [Header("Feedback — Dialogue Panel")]
    public GameObject dialoguePanel;
    public Text dialogueText;

    [Header("Feedback — MiniGame Panel")]
    public GameObject miniGamePanel;

    [Header("Ecran de fin")]
    public GameObject endingPanel;
    public Text endingTitle;
    public Text endingText;

    private const float YOffset = 0.4f;
    private const float MoveSpeed = 5f;
    private const int TotalTiles = BoardManager.TotalTiles; // 20
    private const float BounceTime = 0.3f;
    private const float FloatingTextDuration = 1.5f;
    private const float FloatingTextSpeed = 0.5f;
    private const float FlashInterval = 0.15f;
    private const int FlashCount = 3;
    private const int HealAmount = 25;
    private const int ArtifactGold = 30;
    private const int MaxHealth = 100;

    private static readonly Vector3 BounceScale = new Vector3(0.7f, 0.7f, 0.7f);
    private static readonly Vector3 NormalScale = new Vector3(0.5f, 0.5f, 0.5f);

    private static readonly Color GoldColor = new Color(1f, 0.843f, 0f);
    private static readonly Color HealColor = new Color(0.2f, 0.8f, 0.3f);

    // Indices des cases artefact correspondant a artifactFound[0/1/2]
    private static readonly int[] ArtifactTileIndices = new int[] { 4, 12, 16 };

    // Indices des cases indice correspondant a clueFound[0/1/2]
    private static readonly int[] ClueTileIndices = new int[] { 3, 10, 19 };

    // Phrases de danger aleatoires
    private static readonly string[] DangerPhrases = new string[]
    {
        "Un dard empoisonne jaillit du mur !",
        "Le sol s'effondre sous tes pieds !",
        "Une liane te serre la gorge !"
    };

    [HideInInspector] public int currentTileIndex = 0;
    public bool isMoving = false;

    private void Start()
    {
        // Restaurer la position du joueur au retour du mini-jeu
        if (GameManager.Instance != null && GameManager.Instance.LastTileIndex > 0)
        {
            currentTileIndex = GameManager.Instance.LastTileIndex;
        }

        // Placer le cube apres un court delai pour laisser le board generer ses positions
        Invoke(nameof(PlaceOnTile), 0.15f);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);
        if (endingPanel != null)
            endingPanel.SetActive(false);
    }

    /// <summary>
    /// Place le joueur sur sa case actuelle.
    /// </summary>
    private void PlaceOnTile()
    {
        if (board == null) return;
        transform.position = board.GetTilePosition(currentTileIndex) + Vector3.up * YOffset;
    }

    /// <summary>
    /// Avance le joueur de N cases une par une avec animation, puis appelle OnLand().
    /// </summary>
    public void MoveBySteps(int steps)
    {
        if (isMoving) return;
        StartCoroutine(MoveCoroutine(steps));
    }

    private IEnumerator MoveCoroutine(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            currentTileIndex = (currentTileIndex + 1) % TotalTiles;

            if (currentTileIndex == 0)
            {
                GameManager.Instance.IncrementLoop();
                GameManager.Instance.AddGold(10);
                Debug.Log("BOUCLE COMPLETE ! +10 gold. Boucles : " + GameManager.Instance.LoopCount);
            }

            Vector3 targetPosition = board.GetTilePosition(currentTileIndex) + Vector3.up * YOffset;

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    MoveSpeed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = targetPosition;
        }

        isMoving = false;
        OnLand();
    }

    /// <summary>
    /// Feedback a l'atterrissage : bounce + effet specifique par type de case.
    /// </summary>
    private void OnLand()
    {
        string tileType = board.GetTileType(currentTileIndex);
        Debug.Log("Case " + currentTileIndex + " - type : " + tileType
            + " | Gold : " + GameManager.Instance.Gold
            + " | HP : " + GameManager.Instance.Health);

        DiceRoller diceRoller = FindFirstObjectByType<DiceRoller>();
        if (diceRoller != null && diceRoller.tileInfoText != null)
            diceRoller.tileInfoText.text = "Case " + currentTileIndex + "  -  " + tileType;

        StartCoroutine(BounceEffect());

        switch (tileType)
        {
            case "gold":
                OnGold();
                break;
            case "danger":
                OnDanger();
                break;
            case "dialogue":
                OnDialogue();
                break;
            case "minigame":
                OnMiniGame();
                break;
            case "heal":
                OnHeal();
                break;
            case "artifact":
                OnArtifact();
                break;
            case "clue":
                OnClue();
                break;
            case "start":
                Debug.Log("Case de depart");
                break;
        }

        // Verification des conditions de fin apres chaque atterrissage
        GameManager.Instance.CheckEndConditions();
        if (GameManager.Instance.gameWon)
            ShowEnding(true);
        else if (GameManager.Instance.gameLost)
            ShowEnding(false);
    }

    // ============================================================
    // BOUNCE — toutes les cases
    // ============================================================

    private IEnumerator BounceEffect()
    {
        transform.localScale = BounceScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / BounceTime;
            transform.localScale = Vector3.Lerp(BounceScale, NormalScale, t);
            yield return null;
        }
        transform.localScale = NormalScale;
    }

    // ============================================================
    // GOLD — texte flottant "+20 or"
    // ============================================================

    private void OnGold()
    {
        GameManager.Instance.AddGold(20);
        Debug.Log("OR ! +20 gold. Total : " + GameManager.Instance.Gold);
        StartCoroutine(ShowFloatingText("Des pieces anciennes ! +20 or", GoldColor));
    }

    private IEnumerator ShowFloatingText(string msg, Color color)
    {
        if (floatingTextPrefab == null) yield break;

        GameObject go = Instantiate(
            floatingTextPrefab,
            transform.position + Vector3.up * 1.2f,
            Quaternion.Euler(90f, 0f, 0f)
        );

        Text textComponent = go.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = msg;
            textComponent.color = color;
        }

        CanvasGroup canvasGroup = go.GetComponentInChildren<CanvasGroup>();

        float timer = 0f;
        while (timer < FloatingTextDuration)
        {
            timer += Time.deltaTime;
            go.transform.position += Vector3.up * FloatingTextSpeed * Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / FloatingTextDuration);
            yield return null;
        }

        Destroy(go);
    }

    // ============================================================
    // DANGER — texte flottant "-15 PV" + clignotement rouge
    // ============================================================

    private void OnDanger()
    {
        GameManager.Instance.RemoveHealth(15);
        string phrase = DangerPhrases[Random.Range(0, DangerPhrases.Length)];
        Debug.Log("DANGER ! " + phrase + " -15 HP. Total : " + GameManager.Instance.Health);
        StartCoroutine(ShowFloatingText(phrase + " -15 PV", Color.red));
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) yield break;

        Color original = rend.material.color;
        for (int i = 0; i < FlashCount; i++)
        {
            rend.material.color = Color.red;
            yield return new WaitForSeconds(FlashInterval);
            rend.material.color = original;
            yield return new WaitForSeconds(FlashInterval);
        }
    }

    // ============================================================
    // DIALOGUE — dialogues par boucle
    // ============================================================

    private void OnDialogue()
    {
        if (dialoguePanel == null || dialogueText == null) return;

        int loop = GameManager.Instance.LoopCount;

        string[][] dialoguesByLoop = new string[][]
        {
            new string[]
            {
                "L'air est lourd et humide. Les murs du temple sont couverts de glyphes que tu ne reconnais pas.",
                "Un singe t'observe depuis une corniche. Il tient quelque chose de brillant.",
                "Tu entends de l'eau couler quelque part en contrebas. Ce temple est vivant."
            },
            new string[]
            {
                "Les glyphes commencent a faire sens. Ils parlent d'un roi qui refusait de mourir.",
                "Tu trouves un piege desarme. Quelqu'un est passe avant toi. Lucia ?",
                "Les murs sont plus etroits ici. Le temple te teste."
            },
            new string[]
            {
                "\"Tu tournes en rond, Jose.\" Ta propre voix resonne. Ou est-ce le temple ?",
                "Les pieges se rearment derriere toi. Ce lieu ne veut pas te laisser partir.",
                "Tu sens le parfum de Lucia. Elle est proche. Continue."
            }
        };

        int loopIndex = Mathf.Min(loop, dialoguesByLoop.Length - 1);
        string[] pool = dialoguesByLoop[loopIndex];
        dialogueText.text = pool[Random.Range(0, pool.Length)];
        dialoguePanel.SetActive(true);

        DiceRoller diceRoller = FindFirstObjectByType<DiceRoller>();
        if (diceRoller != null)
            diceRoller.SetInteractable(false);
    }

    /// <summary>
    /// Appele par le bouton OK du DialoguePanel.
    /// </summary>
    public void CloseDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        DiceRoller diceRoller = FindFirstObjectByType<DiceRoller>();
        if (diceRoller != null)
            diceRoller.SetInteractable(true);
    }

    // ============================================================
    // HEAL — soin de 25 PV
    // ============================================================

    private void OnHeal()
    {
        GameManager.Instance.Health = Mathf.Min(MaxHealth, GameManager.Instance.Health + HealAmount);
        Debug.Log("SOIN ! +" + HealAmount + " PV. Total : " + GameManager.Instance.Health);
        StartCoroutine(ShowFloatingText("Source d'eau claire ! +" + HealAmount + " PV", HealColor));
    }

    // ============================================================
    // ARTIFACT — collecte d'un artefact unique
    // ============================================================

    private void OnArtifact()
    {
        // Identifier quel artefact correspond a cette case
        int artifactIndex = -1;
        for (int i = 0; i < ArtifactTileIndices.Length; i++)
        {
            if (currentTileIndex == ArtifactTileIndices[i])
            {
                artifactIndex = i;
                break;
            }
        }

        if (artifactIndex < 0 || dialoguePanel == null || dialogueText == null) return;

        GameManager gm = GameManager.Instance;
        DiceRoller diceRoller = FindFirstObjectByType<DiceRoller>();

        if (gm.artifactFound[artifactIndex])
        {
            // Artefact deja collecte
            dialogueText.text = "L'emplacement de " + gm.artifactNames[artifactIndex]
                + " est vide. Tu l'as deja recupere.";
        }
        else
        {
            // Nouvelle decouverte
            gm.artifactFound[artifactIndex] = true;
            gm.artifactsCollected++;
            gm.Gold += ArtifactGold;
            StartCoroutine(ShowFloatingText("+" + ArtifactGold + " or", GoldColor));

            string[] artifactDialogues = new string[]
            {
                "L'Amulette du Crepuscule. Selon les glyphes, elle ouvrait les portes cachees du temple. "
                    + "Une inscription : \"Seul celui qui voit dans l'ombre trouvera le chemin.\"",
                "La Lame Oubliee du roi Kwame. Elle servait a sceller les passages. "
                    + "Les marques sur la lame correspondent aux entailles sur les murs.",
                "L'Oeil de Pierre. La legende dit qu'il montre le temple tel qu'il etait. "
                    + "En le levant, tu apercois des chemins invisibles sur les murs."
            };

            dialogueText.text = artifactDialogues[artifactIndex]
                + "\n\nArtefact " + gm.artifactsCollected + "/3";

            Debug.Log("ARTEFACT " + gm.artifactNames[artifactIndex]
                + " collecte ! Total : " + gm.artifactsCollected + "/3");
        }

        dialoguePanel.SetActive(true);
        if (diceRoller != null)
            diceRoller.SetInteractable(false);
    }

    // ============================================================
    // CLUE — collecte d'un indice unique
    // ============================================================

    private void OnClue()
    {
        int clueIndex = -1;
        for (int i = 0; i < ClueTileIndices.Length; i++)
        {
            if (currentTileIndex == ClueTileIndices[i])
            {
                clueIndex = i;
                break;
            }
        }

        if (clueIndex < 0 || dialoguePanel == null || dialogueText == null) return;

        GameManager gm = GameManager.Instance;
        DiceRoller diceRoller = FindFirstObjectByType<DiceRoller>();

        if (gm.clueFound[clueIndex])
        {
            dialogueText.text = "Tu as deja examine cet endroit. Rien de nouveau.";
        }
        else
        {
            gm.clueFound[clueIndex] = true;
            gm.cluesCollected++;

            string[] clues = new string[]
            {
                "Tu trouves un carnet froisse. L'ecriture est celle de Lucia. "
                    + "\"Le temple cache plus que des reliques. La salle au fond du couloir ouest... j'y suis presque.\""
                    + "\n\nIndice " + gm.cluesCollected + "/3",
                "Des empreintes de pas recentes dans la poussiere. A cote, un briquet grave \"L.\" "
                    + "et une carte annotee. Elle est passee ici il y a peu."
                    + "\n\nIndice " + gm.cluesCollected + "/3",
                "Un message griffonne sur le mur : \"Papa, si tu lis ca, je suis dans la chambre du roi. "
                    + "Je ne peux plus sortir. - Lucia\""
                    + "\n\nIndice " + gm.cluesCollected + "/3"
            };

            dialogueText.text = clues[clueIndex];
            Debug.Log("INDICE " + gm.cluesCollected + "/3 trouve !");
        }

        dialoguePanel.SetActive(true);
        if (diceRoller != null)
            diceRoller.SetInteractable(false);
    }

    // ============================================================
    // ENDING — ecran de fin du jeu principal
    // ============================================================

    /// <summary>
    /// Affiche l'ecran de fin (victoire ou defaite) et bloque le jeu.
    /// </summary>
    private void ShowEnding(bool victory)
    {
        DiceRoller diceRoller = FindFirstObjectByType<DiceRoller>();
        if (diceRoller != null)
            diceRoller.SetInteractable(false);

        if (endingPanel == null || endingTitle == null || endingText == null) return;

        endingPanel.SetActive(true);

        GameManager gm = GameManager.Instance;

        if (victory)
        {
            endingTitle.text = "LUCIA EST RETROUVEE";
            endingTitle.color = Color.green;
            endingText.text = "Grace aux artefacts du roi Kwame, tu dechiffres les secrets du temple. "
                + "Les indices de Lucia te guident jusqu'a la chambre royale."
                + "\n\nElle est la, fatiguee mais vivante, entouree de reliques inestimables."
                + "\n\n\"Papa... je savais que tu viendrais.\""
                + "\n\nTu la serres dans tes bras. Le temple gronde. Il est temps de partir."
                + "\n\nOr final : " + gm.Gold
                + "\nBoucles : " + gm.LoopCount;
        }
        else
        {
            string reason = gm.Health <= 0
                ? "Tes blessures ont eu raison de toi."
                : "Sans ressources, tu ne peux plus avancer.";

            endingTitle.text = "LE TEMPLE A GAGNE";
            endingTitle.color = Color.red;
            endingText.text = reason
                + "\n\nLe temple se referme lentement. Lucia reste prisonniere de ces murs anciens."
                + "\n\nPeut-etre qu'un autre archeologue viendra..."
                + "\n\nBoucles parcourues : " + gm.LoopCount
                + "\nArtefacts trouves : " + gm.artifactsCollected + "/3"
                + "\nIndices trouves : " + gm.cluesCollected + "/3";
        }
    }

    // ============================================================
    // MINIGAME — panneau avec boutons Entrer / Annuler
    // ============================================================

    private void OnMiniGame()
    {
        if (miniGamePanel == null) return;

        miniGamePanel.SetActive(true);

        DiceRoller diceRoller = FindFirstObjectByType<DiceRoller>();
        if (diceRoller != null)
            diceRoller.SetInteractable(false);
    }

    /// <summary>
    /// Appele par le bouton Entrer du MiniGamePanel.
    /// </summary>
    public void EnterMiniGame()
    {
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);

        GameManager.Instance.LoadMiniGame();
    }

    /// <summary>
    /// Appele par le bouton Annuler du MiniGamePanel.
    /// </summary>
    public void CancelMiniGame()
    {
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);

        DiceRoller diceRoller = FindFirstObjectByType<DiceRoller>();
        if (diceRoller != null)
            diceRoller.SetInteractable(true);
    }
}
