using UnityEngine;

public class IA_Detection : MonoBehaviour
{
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float fieldOfViewAngle = 90f;

    private Transform playerTarget;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
    }

    public bool CanSeeTarget()
    {
        if (playerTarget == null)
            return false;

        Vector3 directionToTarget = playerTarget.position - transform.position;
        float distance = directionToTarget.magnitude;

        if (distance > detectionRange)
            return false;

        float dot = Vector3.Dot(transform.forward, directionToTarget.normalized);
        float threshold = Mathf.Cos(fieldOfViewAngle * 0.5f * Mathf.Deg2Rad);

        if (dot < threshold)
            return false;

        Vector3 rayOrigin = transform.position + Vector3.up;
        Vector3 rayDirection = (playerTarget.position - rayOrigin).normalized;
        float rayDistance = Vector3.Distance(rayOrigin, playerTarget.position);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance))
        {
            return hit.transform == playerTarget;
        }

        return true;
    }

    public Transform GetTarget()
    {
        return playerTarget;
    }
}
