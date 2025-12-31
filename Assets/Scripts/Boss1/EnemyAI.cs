using UnityEngine;
using System.Collections;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack
}

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("AI Settings")]
    public float detectRange = 8f;
    public float attackRange = 2f;
    public float moveSpeed = 2.5f;

    [Header("Patrol Settings")]
    public float patrolRadius = 5f;
    public float idleTime = 2f;

    private Vector3 patrolTarget;
    private float idleTimer;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;

    private Animator animator;
    private EnemyState currentState;

    private bool isAttacking;
    private float lastAttackTime;

    int[][] combos =
    {
        new int[] { 0, 0 },
        new int[] { 0, 1 },
        new int[] { 0, 1, 2 }
    };

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (player == null)
            Debug.LogWarning("EnemyAI: Player has NOT been assigned!");

        currentState = EnemyState.Idle;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle(distance);
                break;

            case EnemyState.Patrol:
                HandlePatrol(distance);
                break;

            case EnemyState.Chase:
                HandleChase(distance);
                break;

            case EnemyState.Attack:
                HandleAttack(distance);
                break;
        }
    }

    // ================= STATES =================

    void HandleIdle(float distance)
    {
        animator.SetBool("isMoving", false);

        idleTimer += Time.deltaTime;

        if (distance <= detectRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        if (idleTimer >= idleTime)
        {
            PickPatrolPoint();
            currentState = EnemyState.Patrol;
            idleTimer = 0f;
        }
    }

    void HandlePatrol(float distance)
    {
        animator.SetBool("isMoving", true);

        Vector3 dir = (patrolTarget - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        FaceDirection(dir);

        if (Vector3.Distance(transform.position, patrolTarget) < 0.3f)
        {
            currentState = EnemyState.Idle;
        }

        if (distance <= detectRange)
        {
            currentState = EnemyState.Chase;
        }
    }

    void HandleChase(float distance)
    {
        animator.SetBool("isMoving", true);

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        FaceTarget();

        if (distance <= attackRange + 0.2f)
        {
            animator.SetBool("isMoving", false);
            currentState = EnemyState.Attack;
        }
    }

    void HandleAttack(float distance)
    {
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        if (distance > attackRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        StartCoroutine(AttackCombo());
    }

    // ================= ATTACK =================

IEnumerator AttackCombo()
{
    isAttacking = true;

    int[] combo = combos[Random.Range(0, combos.Length)];

    foreach (int atk in combo)
    {
        animator.SetInteger("attackInd", atk);
        animator.SetTrigger("attack");

        yield return new WaitForSeconds(1.1f);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange + 0.5f)
            break;
    }

    isAttacking = false;
    lastAttackTime = Time.time;
}


    // ================= UTIL =================

    void FaceTarget()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;

        if (lookDir == Vector3.zero) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(lookDir),
            Time.deltaTime * 8f
        );
    }

    void PickPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;

        patrolTarget = new Vector3(
            transform.position.x + randomCircle.x,
            transform.position.y,
            transform.position.z + randomCircle.y
        );
    }

    void FaceDirection(Vector3 dir)
    {
        dir.y = 0;
        if (dir == Vector3.zero) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 5f
        );
    }
}
