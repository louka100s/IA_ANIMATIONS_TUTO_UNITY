using UnityEngine;

public class GoalZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("VICTOIRE !");
            if (GameManager.Instance != null)
                GameManager.Instance.EndMiniGame(true);

            ResultScreen resultScreen = FindFirstObjectByType<ResultScreen>();
            if (resultScreen != null)
                resultScreen.ShowVictory();
        }
    }
}
