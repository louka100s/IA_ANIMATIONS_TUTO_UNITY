using UnityEngine;
using UnityEngine.AI;

public class DifficultyScaler : MonoBehaviour
{
    /// <summary>
    /// Agent NavMesh de l'IA a mettre a l'echelle.
    /// </summary>
    public NavMeshAgent agent;

    /// <summary>
    /// Vitesse de base de l'IA (tentative 0).
    /// </summary>
    public float baseSpeed = 3f;

    /// <summary>
    /// Augmentation de vitesse par tentative supplementaire.
    /// </summary>
    public float speedIncrement = 0.5f;

    /// <summary>
    /// Vitesse maximale plafonnee de l'IA.
    /// </summary>
    public float maxSpeed = 6f;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null) return;

        if (GameManager.Instance != null)
        {
            int attempts = GameManager.Instance.miniGameAttempts;
            agent.speed = Mathf.Min(baseSpeed + (attempts * speedIncrement), maxSpeed);
            Debug.Log("DifficultyScaler : vitesse IA = " + agent.speed + " (tentative " + attempts + ")");
        }
        else
        {
            agent.speed = baseSpeed;
        }
    }
}
