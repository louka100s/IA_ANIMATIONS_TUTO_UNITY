using UnityEngine;
using UnityEngine.AI;

public class IA_ARCHER_CONTROLLER : MonoBehaviour
{
    // ============ ENUM ============
    public enum StateType { None, Patrol, Follow, Attack, BreakCrate }

    // ============ PARAMÈTRES INSPECTOR ============
    [Header("État actuel (Debug)")]
    [SerializeField] private StateType state = StateType.Patrol;

    /// <summary>Expose l'état courant en lecture pour MiniGamePlayer.</summary>
    public StateType CurrentState => state;

    [Header("Références")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Paramètres de combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Paramètres de mouvement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Paramètres de détection")]
    public float lostPlayerDuration = 3f;

    // ============ PRIVÉS ============
    private NavMeshAgent agent;
    private Animator animator;
    private IA_Detection detection;

    private int currentPatrolIndex = 0;
    private float attackTimer = 0f;
    private float lostPlayerTimer = 0f;

    // Seuil pour considérer qu'un PatrolPoint est atteint
    private const float PatrolPointThreshold = 1.5f;

    // Antiblockage : si l'IA ne progresse pas depuis X secondes, force le point suivant
    private const float StuckTimeout = 3f;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    // BreakCrate
    private GameObject targetCrate = null;
    private const float BreakDistance = 1.5f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        detection = GetComponent<IA_Detection>();
        agent.autoBraking = false;
    }

    private void Start()
    {
        lastPosition = transform.position;

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            Debug.LogError("IA_ARCHER : Aucun patrol point assigné !");
        }
    }

    // ============ BOUCLE ============
    private void Update()
    {
        switch (state)
        {
            case StateType.Patrol:    PatrolBehaviour();    break;
            case StateType.Follow:    FollowBehaviour();    break;
            case StateType.Attack:    AttackBehaviour();    break;
            case StateType.BreakCrate: BreakCrateBehaviour(); break;
        }
    }

    // ============ PATROL ============
    /// <summary>
    /// Boucle continue entre les PatrolPoints dans l'ordre.
    /// Passe en Follow dès que le joueur est détecté.
    /// Antiblockage : si l'IA reste immobile 3s, elle avance au point suivant.
    /// </summary>
    private void PatrolBehaviour()
    {
        if (patrolPoints.Length == 0) return;

        // Transition → Follow
        if (detection.CanSeeTarget())
        {
            CleanTriggers();
            lostPlayerTimer = 0f;
            agent.isStopped = false;
            state = StateType.Follow;
            return;
        }

        agent.isStopped = false;
        agent.speed = walkSpeed;
        animator.SetFloat("Speed", agent.velocity.magnitude);

        // A-t-on atteint le point courant ?
        float distToPoint = Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position);
        bool reached = !agent.pathPending
            && agent.remainingDistance < PatrolPointThreshold
            && distToPoint < PatrolPointThreshold;

        if (reached)
        {
            AdvancePatrolIndex();
            return;
        }

        // Antiblockage
        if (Vector3.Distance(transform.position, lastPosition) < 0.05f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= StuckTimeout)
            {
                Debug.LogWarning("IA_ARCHER : bloquée, passage au point suivant.");
                AdvancePatrolIndex();
            }
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
        }
    }

    // ============ FOLLOW ============
    /// <summary>
    /// Poursuit le joueur.
    /// Si le joueur se cache ET que l'IA le voyait → BreakCrate.
    /// Si le joueur se cache sans être vu → timer avant retour Patrol.
    /// </summary>
    private void FollowBehaviour()
    {
        CleanTriggers();
        agent.isStopped = false;
        agent.speed = runSpeed;
        animator.SetFloat("Speed", agent.velocity.magnitude);

        Transform target = detection.GetTarget();
        bool canSee = detection.CanSeeTarget();

        // Le joueur vient de se cacher
        MiniGamePlayer player = FindFirstObjectByType<MiniGamePlayer>();
        if (player != null && player.isHiding)
        {
            if (player.wasSeenEntering && player.currentCrate != null)
            {
                // L'IA l'a vu entrer → aller casser la caisse
                targetCrate = player.currentCrate;
                lostPlayerTimer = 0f;
                state = StateType.BreakCrate;
                Debug.Log("IA_ARCHER : joueur repere dans une caisse, passage BreakCrate.");
            }
            else
            {
                // L'IA ne l'a pas vu → perte de vue normale
                lostPlayerTimer += Time.deltaTime;
                if (lostPlayerTimer >= lostPlayerDuration)
                {
                    lostPlayerTimer = 0f;
                    ReturnToNearestPatrol();
                }
            }
            return;
        }

        if (canSee && target != null)
        {
            lostPlayerTimer = 0f;
            agent.SetDestination(target.position);

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange)
            {
                CleanTriggers();
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                animator.SetFloat("Speed", 0f);
                attackTimer = 0f;
                state = StateType.Attack;
            }
        }
        else
        {
            lostPlayerTimer += Time.deltaTime;
            if (lostPlayerTimer >= lostPlayerDuration)
            {
                lostPlayerTimer = 0f;
                ReturnToNearestPatrol();
            }
        }
    }

    // ============ BREAK CRATE ============
    /// <summary>
    /// L'IA fonce vers la caisse où le joueur s'est caché (elle l'a vu entrer).
    /// Quand elle arrive, effet de tremblement puis destruction de la caisse.
    /// </summary>
    private void BreakCrateBehaviour()
    {
        if (targetCrate == null)
        {
            // Caisse déjà détruite (timer écoulé du joueur, par ex.)
            ReturnToNearestPatrol();
            return;
        }

        agent.isStopped = false;
        agent.speed = runSpeed;
        animator.SetFloat("Speed", agent.velocity.magnitude);
        agent.SetDestination(targetCrate.transform.position);

        float dist = Vector3.Distance(transform.position, targetCrate.transform.position);
        if (dist < BreakDistance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);
            StartCoroutine(BreakCrateEffect(targetCrate));
            targetCrate = null; // évite de relancer la coroutine
        }
    }

    /// <summary>
    /// Coroutine : fait trembler la caisse 0.5s, éjecte le joueur, détruit la caisse.
    /// </summary>
    private System.Collections.IEnumerator BreakCrateEffect(GameObject crate)
    {
        Vector3 originalPos = crate.transform.position;

        for (int i = 0; i < 10; i++)
        {
            if (crate == null) yield break;
            crate.transform.position = originalPos + Random.insideUnitSphere * 0.1f;
            yield return new WaitForSeconds(0.05f);
        }

        if (crate != null)
            crate.transform.position = originalPos;

        // Éjecter le joueur s'il est toujours dedans
        MiniGamePlayer player = FindFirstObjectByType<MiniGamePlayer>();
        if (player != null && player.isHiding)
            player.ForcedExit();

        if (crate != null)
            Destroy(crate);

        Debug.Log("IA_ARCHER : caisse detruite !");

        // Reprendre la poursuite
        lostPlayerTimer = 0f;
        state = StateType.Follow;
    }

    // ============ ATTACK ============
    /// <summary>
    /// Attaque le joueur au corps à corps. Repasse en Follow si le joueur sort de portée.
    /// </summary>
    private void AttackBehaviour()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetFloat("Speed", 0f);

        Transform target = detection.GetTarget();
        if (target == null)
        {
            CleanTriggers();
            agent.isStopped = false;
            ReturnToNearestPatrol();
            return;
        }

        FaceTarget(target);
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName("punch"))
            {
                animator.SetTrigger("Punch");
                attackTimer = 0f;
            }
        }

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > attackRange)
        {
            CleanTriggers();
            agent.isStopped = false;
            lostPlayerTimer = 0f;
            state = StateType.Follow;
        }
    }

    // ============ UTILITAIRES ============

    /// <summary>Avance d'un cran dans la boucle et envoie l'agent vers le nouveau point.</summary>
    private void AdvancePatrolIndex()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        stuckTimer = 0f;
        lastPosition = transform.position;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    /// <summary>
    /// Trouve le PatrolPoint le plus proche, met à jour currentPatrolIndex
    /// et bascule en état PATROL.
    /// </summary>
    private void ReturnToNearestPatrol()
    {
        if (patrolPoints.Length == 0) return;

        float minDist = Mathf.Infinity;
        int nearestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestIndex = i;
            }
        }

        currentPatrolIndex = nearestIndex;
        stuckTimer = 0f;
        lastPosition = transform.position;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        CleanTriggers();
        state = StateType.Patrol;
    }

    private void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    /// <summary>Nettoie les triggers Animator pour éviter les états fantômes.</summary>
    private void CleanTriggers()
    {
        animator.ResetTrigger("Look");
        animator.ResetTrigger("Punch");
    }
}