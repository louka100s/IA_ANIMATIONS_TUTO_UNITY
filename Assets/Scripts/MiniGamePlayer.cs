using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGamePlayer : MonoBehaviour
{
    /// <summary>Vitesse de deplacement du joueur dans le mini-jeu.</summary>
    public float speed = 5f;

    /// <summary>Reference vers l'asset InputActions du projet.</summary>
    public InputActionAsset inputActions;

    /// <summary>Duree maximale de la cachette en secondes.</summary>
    public float hideDuration = 5f;

    /// <summary>Vrai quand le joueur est cache dans une caisse.</summary>
    public bool isHiding = false;

    /// <summary>Temps restant dans la cachette (public pour que le HUD puisse le lire).</summary>
    public float hideTimer = 0f;

    private const string CrateTag = "HidingCrate";

    // Détection de caisse via OnTrigger (le OverlapSphere ignore les triggers par défaut)
    private bool isCrateNearby = false;
    private GameObject nearbyCrate = null;

    // Exposé pour MiniGameHUD (affichage du prompt)
    public bool IsCrateNearby => isCrateNearby;

    private Rigidbody rb;
    private Collider playerCollider;
    private Renderer playerRenderer;
    private InputAction moveAction;
    /// <summary>Vrai si le joueur était vu par l'IA au moment d'entrer dans la caisse.</summary>
    public bool wasSeenEntering = false;

    /// <summary>La caisse dans laquelle le joueur est actuellement cache (public pour l'IA).</summary>
    public GameObject currentCrate = null;

    private void Start()
    {
        rb             = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        playerRenderer = GetComponent<Renderer>();

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

    // ─── Détection de la zone trigger de la caisse ───────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(CrateTag))
        {
            isCrateNearby = true;
            nearbyCrate   = other.gameObject;
            Debug.Log("MiniGamePlayer : caisse à portee — " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(CrateTag) && other.gameObject == nearbyCrate)
        {
            isCrateNearby = false;
            nearbyCrate   = null;
        }
    }

    // ─── Boucle principale ────────────────────────────────────────────────────

    private void Update()
    {
        // Timer de cachette (prioritaire : bloque tout le reste)
        if (isHiding)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
                ExitCrate();
            return;
        }

        // Touche E OU clic droit pour se cacher
        // Input.GetMouseButtonDown / KeyCode sont inutilisables : le projet est en
        // "New Input System Only" (activeInputHandler = 1). On passe par Keyboard/Mouse.current.
        bool hidePressed = (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                        || (Mouse.current   != null && Mouse.current.rightButton.wasPressedThisFrame);

        if (hidePressed && isCrateNearby && nearbyCrate != null)
        {
            Debug.Log("MiniGamePlayer : touche cachette détectée, entrée dans " + nearbyCrate.name);
            EnterCrate(nearbyCrate);
        }
    }

    private void FixedUpdate()
    {
        // Aucun mouvement physique pendant la cachette
        if (isHiding || moveAction == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move  = new Vector3(input.x, 0f, input.y).normalized * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    // ─── Cachette ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Cache le joueur dans la caisse.
    /// Vérifie si l'IA était en FOLLOW pour savoir si elle a "vu" l'entrée.
    /// Désactive la physique AVANT le téléport.
    /// </summary>
    private void EnterCrate(GameObject crate)
    {
        // L'IA nous a-t-elle vu entrer ? (elle était en état Follow)
        IA_ARCHER_CONTROLLER archer = FindFirstObjectByType<IA_ARCHER_CONTROLLER>();
        wasSeenEntering = archer != null && archer.CurrentState == IA_ARCHER_CONTROLLER.StateType.Follow;

        isHiding     = true;
        hideTimer    = hideDuration;
        currentCrate = crate;

        // 1. Couper la physique en premier
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic     = true;

        // 2. Désactiver le collider solide
        if (playerCollider != null) playerCollider.enabled = false;

        // 3. Rendre invisible
        if (playerRenderer != null) playerRenderer.enabled = false;

        // 4. Téléporter au centre de la caisse
        transform.position = crate.transform.position;

        Debug.Log("MiniGamePlayer : cache dans " + crate.name
            + (wasSeenEntering ? " [REPERE par l'IA]" : " [discret]"));
    }

    /// <summary>
    /// Sortie normale (timer écoulé) : repositionne le joueur devant la caisse.
    /// </summary>
    private void ExitCrate()
    {
        isHiding        = false;
        hideTimer       = 0f;
        wasSeenEntering = false;

        if (currentCrate != null)
            transform.position = currentCrate.transform.position + Vector3.forward * 1.5f + Vector3.up * 0.1f;

        if (playerRenderer != null) playerRenderer.enabled = true;
        if (playerCollider != null) playerCollider.enabled = true;

        rb.isKinematic    = false;
        rb.linearVelocity = Vector3.zero;

        currentCrate = null;
        Debug.Log("MiniGamePlayer : sorti de la caisse !");
    }

    /// <summary>
    /// Sortie forcée par l'IA : éjecte le joueur sur le côté, réinitialise tout.
    /// </summary>
    public void ForcedExit()
    {
        isHiding        = false;
        hideTimer       = 0f;
        wasSeenEntering = false;

        if (playerRenderer != null) playerRenderer.enabled = true;
        if (playerCollider != null) playerCollider.enabled = true;

        rb.isKinematic    = false;
        rb.linearVelocity = Vector3.zero;

        // Éjecter légèrement sur le côté de la caisse
        if (currentCrate != null)
            transform.position = currentCrate.transform.position + Vector3.right * 1.5f + Vector3.up * 0.1f;

        currentCrate = null;
        Debug.Log("MiniGamePlayer : EJECTE par l'IA !");
    }
}
