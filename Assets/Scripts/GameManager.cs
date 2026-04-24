using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Gold { get; set; } = 0;
    public int Health { get; set; } = 100;
    public int LoopCount { get; private set; } = 0;

    /// <summary>Permet à SaveSystem de restaurer le compteur de boucles.</summary>
    public void SetLoopCount(int value) => LoopCount = value;

    /// <summary>
    /// Sauvegarde la position du joueur sur le plateau avant de charger le mini-jeu.
    /// </summary>
    public int LastTileIndex { get; set; } = 0;

    /// <summary>
    /// Stocke le resultat du dernier mini-jeu (null = pas de resultat en attente).
    /// </summary>
    public bool? LastResult { get; set; } = null;

    /// <summary>
    /// Nombre total d'artefacts collectes parmi les 3 disponibles.
    /// </summary>
    public int artifactsCollected = 0;

    /// <summary>
    /// Etat de decouverte de chaque artefact unique.
    /// </summary>
    public bool[] artifactFound = new bool[3] { false, false, false };

    /// <summary>
    /// Noms des 3 artefacts du jeu.
    /// </summary>
    public string[] artifactNames = new string[]
    {
        "Amulette du Crepuscule",
        "Lame Oubliee",
        "Oeil de Pierre"
    };

    // ============================================================
    // INDICES
    // ============================================================

    /// <summary>
    /// Nombre total d'indices collectes parmi les 3 disponibles.
    /// </summary>
    public int cluesCollected = 0;

    /// <summary>
    /// Etat de decouverte de chaque indice unique.
    /// </summary>
    public bool[] clueFound = new bool[3] { false, false, false };

    // ============================================================
    // FIN DE PARTIE
    // ============================================================

    /// <summary>
    /// Le joueur a gagne (3 artefacts + 3 indices).
    /// </summary>
    public bool gameWon = false;

    /// <summary>
    /// Le joueur a perdu (PV <= 0).
    /// </summary>
    public bool gameLost = false;

    /// <summary>
    /// Nombre de tentatives du mini-jeu. Augmente la difficulte a chaque entree.
    /// </summary>
    public int miniGameAttempts = 0;

    /// <summary>
    /// Nombre de relances de dé disponibles achetées au shop.
    /// </summary>
    public int rerollsAvailable = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // La sauvegarde est chargée explicitement depuis MainMenu (Continuer)
        // ou réinitialisée via ResetState() (Nouvelle Partie).
        if (SaveSystem.HasSave())
            SaveSystem.Load();
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save();
    }

    /// <summary>
    /// Remet tous les états du GameManager à zéro (nouvelle partie).
    /// </summary>
    public void ResetState()
    {
        Gold               = 0;
        Health             = 100;
        SetLoopCount(0);
        LastTileIndex      = 0;
        LastResult         = null;
        miniGameAttempts   = 0;
        rerollsAvailable   = 0;
        artifactsCollected = 0;
        artifactFound      = new bool[3] { false, false, false };
        cluesCollected     = 0;
        clueFound          = new bool[3] { false, false, false };
        gameWon            = false;
        gameLost           = false;
    }

    /// <summary>
    /// Achète une potion de soin (30 or → +30 PV, max 100).
    /// Retourne false si pas assez d'or ou PV déjà pleins.
    /// </summary>
    public bool BuyHealthPotion()
    {
        if (Gold < 30 || Health >= 100) return false;
        Gold   -= 30;
        Health  = Mathf.Min(100, Health + 30);
        return true;
    }

    /// <summary>
    /// Achète une relance de dé (20 or → +1 relance stockée).
    /// Retourne false si pas assez d'or.
    /// </summary>
    public bool BuyReroll()
    {
        if (Gold < 20) return false;
        Gold -= 20;
        rerollsAvailable++;
        return true;
    }

    /// <summary>
    /// Consomme une relance si disponible. Retourne true si une relance a été utilisée.
    /// </summary>
    public bool UseReroll()
    {
        if (rerollsAvailable <= 0) return false;
        rerollsAvailable--;
        return true;
    }

    /// <summary>
    /// Ajoute du gold (montant positif uniquement).
    /// </summary>
    public void AddGold(int amount)
    {
        Gold += Mathf.Max(0, amount);
    }

    /// <summary>
    /// Retire de la vie (minimum 0).
    /// </summary>
    public void RemoveHealth(int amount)
    {
        Health = Mathf.Max(0, Health - amount);
    }

    /// <summary>
    /// Verifie les conditions de victoire et de defaite.
    /// Victoire : 3 artefacts + 3 indices.
    /// Defaite : plus de vie.
    /// </summary>
    public void CheckEndConditions()
    {
        if (artifactsCollected >= 3 && cluesCollected >= 3)
        {
            gameWon = true;
            return;
        }

        if (Health <= 0)
        {
            gameLost = true;
        }
    }

    /// <summary>
    /// Applique les recompenses/penalites du mini-jeu et stocke le resultat.
    /// Ne charge PAS la scene principale — c'est ResultScreen qui s'en charge.
    /// </summary>
    public void EndMiniGame(bool victory)
    {
        if (victory)
        {
            Gold += 50;
            Debug.Log("VICTOIRE +50 or, total : " + Gold);
        }
        else
        {
            Gold = Mathf.Max(0, Gold - 25);
            Debug.Log("DEFAITE -25 or, total : " + Gold);
        }

        LastResult = victory;
    }

    /// <summary>
    /// Retour effectif a la scene principale, appele par le bouton Continuer.
    /// </summary>
    public void ReturnToMain()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.TransitionTo("SampleScene");
        else
            SceneManager.LoadScene("SampleScene");
    }

    /// <summary>
    /// Sauvegarde la position du joueur puis charge la scene du mini-jeu.
    /// Incremente le compteur de tentatives pour ajuster la difficulte.
    /// </summary>
    public void LoadMiniGame()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            LastTileIndex = player.currentTileIndex;

        miniGameAttempts++;

        if (SceneTransition.Instance != null)
            SceneTransition.Instance.TransitionTo("HideAndSeekScene");
        else
            SceneManager.LoadScene("HideAndSeekScene");
    }

    /// <summary>
    /// Sauvegarde la position du joueur puis charge la scène Pierre Feuille Ciseaux.
    /// </summary>
    public void LoadRPSMiniGame()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            LastTileIndex = player.currentTileIndex;

        if (SceneTransition.Instance != null)
            SceneTransition.Instance.TransitionTo("RPSScene");
        else
            SceneManager.LoadScene("RPSScene");
    }

    /// <summary>
    /// Incrémente le compteur de boucles, sauvegarde et notifie le HUD.
    /// </summary>
    public void IncrementLoop()
    {
        LoopCount++;
        SaveSystem.Save();

        SaveHUD hud = FindFirstObjectByType<SaveHUD>();
        if (hud != null)
            hud.ShowNotification();
    }
}
