using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Affiche l'état de la sauvegarde dans le HUD principal.
/// - saveInfo   : texte en continu (boucle en cours)
/// - saveNotif  : panneau flash "Sauvegarde auto !" pendant 2s
/// </summary>
public class SaveHUD : MonoBehaviour
{
    /// <summary>Texte permanent affiché dans la TopBar (ex. "Boucle 3").</summary>
    public TextMeshProUGUI saveInfo;

    /// <summary>Panneau de notification flash, désactivé par défaut.</summary>
    public GameObject saveNotif;

    private void Update()
    {
        if (saveInfo == null) return;

        if (GameManager.Instance != null)
        {
            saveInfo.text = SaveSystem.HasSave()
                ? "Boucle " + GameManager.Instance.LoopCount
                : "Nouvelle partie";
        }
    }

    /// <summary>
    /// Affiche le panneau de confirmation de sauvegarde pendant 2 secondes.
    /// Appelé par GameManager après chaque sauvegarde.
    /// </summary>
    public void ShowNotification()
    {
        if (saveNotif != null)
            StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        saveNotif.SetActive(true);
        yield return new WaitForSeconds(2f);
        saveNotif.SetActive(false);
    }
}
