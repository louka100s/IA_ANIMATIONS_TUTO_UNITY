using UnityEngine;

public class CatchPlayer : MonoBehaviour
{
    private bool caught = false;

    private const float DefeatDelay = 1.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (caught) return;

        if (other.CompareTag("Player"))
        {
            caught = true;
            Debug.Log("ATTRAPE !");

            // Freeze le joueur
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = Vector3.zero;

            MiniGamePlayer player = other.GetComponent<MiniGamePlayer>();
            if (player != null)
                player.enabled = false;

            // Defaite apres un petit delai
            Invoke(nameof(TriggerDefeat), DefeatDelay);
        }
    }

    /// <summary>
    /// Declenche la defaite via le GameManager puis affiche l'ecran resultat.
    /// </summary>
    private void TriggerDefeat()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EndMiniGame(false);

        ResultScreen resultScreen = FindFirstObjectByType<ResultScreen>();
        if (resultScreen != null)
            resultScreen.ShowDefeat();
    }
}
