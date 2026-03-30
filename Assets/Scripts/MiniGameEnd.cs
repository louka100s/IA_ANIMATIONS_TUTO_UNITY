using UnityEngine;

public class MiniGameEnd : MonoBehaviour
{
    /// <summary>
    /// Appele par le bouton "Victoire (test)".
    /// </summary>
    public void Win()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EndMiniGame(true);
    }

    /// <summary>
    /// Appele par le bouton "Defaite (test)".
    /// </summary>
    public void Lose()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EndMiniGame(false);
    }
}
