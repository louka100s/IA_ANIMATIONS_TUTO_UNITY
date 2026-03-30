using UnityEngine;

public class MiniGameCamera : MonoBehaviour
{
    /// <summary>
    /// Transform du joueur a suivre.
    /// </summary>
    public Transform target;

    /// <summary>
    /// Decalage de la camera par rapport au joueur.
    /// </summary>
    public Vector3 offset = new Vector3(0, 10, -5);

    /// <summary>
    /// Vitesse de lissage du suivi camera.
    /// </summary>
    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
}
