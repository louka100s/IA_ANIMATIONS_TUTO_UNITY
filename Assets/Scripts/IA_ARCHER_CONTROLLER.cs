using UnityEngine;
using UnityEngine.AI;

public class IA_ARCHER_CONTROLLER : MonoBehaviour
{
    // ============ ENUM ============
    public enum StateType { None, Patrol, Follow, Attack }

    // ============ PARAMÈTRES INSPECTOR ============
    [Header("État actuel (Debug)")]
    [SerializeField] private StateType state = StateType.Patrol;

    [Header("Références")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Paramètres de combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Paramètres de mouvement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private IA_Detection detection;

    private int currentPatrolIndex = 0;
    private float attackTimer = 0f;
    private bool isWaiting = false;
    private bool triggerSent = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        detection = GetComponent<IA_Detection>();
        agent.autoBraking = false;
    }

    private void Start()
    {
        if (patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
        else
        {
            Debug.LogError("IA_ARCHER : Aucun patrol point assigné !");
        }
    }

    // ============ BOUCLE ============
    private void Update()
    {
        BehaviourAction();
    }

    // Réaliser le comportement actuel
    private void BehaviourAction()
    {
        switch (state)
        {
            case StateType.Patrol:
                PatrolBehaviour();
                break;
            case StateType.Follow:
                FollowBehaviour();
                break;
            case StateType.Attack:
                AttackBehaviour();
                break;
        }
    }

    // ============ PATROL ============
    private void PatrolBehaviour()
    {
        if (detection.CanSeeTarget())
        {
            CleanTriggers();
            isWaiting = false;
            triggerSent = false;
            agent.isStopped = false;
            state = StateType.Follow;
            return;
        }

        if (isWaiting)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);

            if (!triggerSent)
            {
                animator.SetTrigger("Look");
                triggerSent = true;
            }

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("look") && info.normalizedTime >= 0.95f)
            {
                CleanTriggers();
                isWaiting = false;
                triggerSent = false;
                agent.isStopped = false;
                GoToNextPatrolPoint();
            }
        }
        else
        {
            agent.isStopped = false;
            agent.speed = walkSpeed;
            animator.SetFloat("Speed", agent.velocity.magnitude);

            if (agent.hasPath && !agent.pathPending && agent.remainingDistance < 0.5f)
            {
                isWaiting = true;
                triggerSent = false;
            }
        }
    }

    // ============ FOLLOW ============
    private void FollowBehaviour()
    {
        agent.isStopped = false;
        agent.speed = runSpeed;
        CleanTriggers();

        animator.SetFloat("Speed", agent.velocity.magnitude);

        Transform target = detection.GetTarget();
        if (target != null)
        {
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
            else if (!detection.CanSeeTarget())
            {
                CleanTriggers();
                state = StateType.Patrol;
                GoToNextPatrolPoint();
            }
        }
        else
        {
            CleanTriggers();
            state = StateType.Patrol;
            GoToNextPatrolPoint();
        }
    }

    // ============ ATTACK ============
    private void AttackBehaviour()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetFloat("Speed", 0f);

        Transform target = detection.GetTarget();
        if (target != null)
        {
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

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > attackRange)
            {
                CleanTriggers();
                agent.isStopped = false;
                state = StateType.Follow;
            }
        }
        else
        {
            CleanTriggers();
            agent.isStopped = false;
            state = StateType.Patrol;
            GoToNextPatrolPoint();
        }
    }

    // ============ UTILITAIRES ============

    private void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // Aller au prochain point de patrouille
    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // Nettoyer les triggers pour éviter les fantômes
    private void CleanTriggers()
    {
        animator.ResetTrigger("Look");
        animator.ResetTrigger("Punch");
    }
}