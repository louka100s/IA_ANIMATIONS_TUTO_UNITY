using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGamePlayer : MonoBehaviour
{
    /// <summary>
    /// Vitesse de deplacement du joueur dans le mini-jeu.
    /// </summary>
    public float speed = 5f;

    /// <summary>
    /// Reference vers l'asset InputActions du projet.
    /// </summary>
    public InputActionAsset inputActions;

    private Rigidbody rb;
    private InputAction moveAction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (inputActions != null)
        {
            moveAction = inputActions.FindAction("Player/Move");
            moveAction?.Enable();
        }
        else
        {
            Debug.LogWarning("MiniGamePlayer : InputActionAsset non assigne !");
        }
    }

    private void OnDestroy()
    {
        moveAction?.Disable();
    }

    private void FixedUpdate()
    {
        if (moveAction == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y).normalized * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}
