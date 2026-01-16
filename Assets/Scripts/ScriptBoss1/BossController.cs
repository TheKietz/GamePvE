using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    public Transform player;
    public HeathBar healthBarScript;

    public float maxHealth = 1000f;
    public float chaseRange = 50f;
    public float attackRange = 1.5f;
    [Header("Chỉ số")]
    public float moveSpeed = 3.5f;
    public float attackCooldown = 3f;  // 🔥 TĂNG 3F → TRÁNH SPAM NHANH
    [Tooltip("1 = Bình thường, 2 = Nhanh gấp đôi, 0.5 = Chậm một nửa")]
    public float attackAnimSpeed = 1.0f;
    [SerializeField] GameObject attackHitbox;
    float nextAttackTime = 0;
    float currentHealth;
    bool isDead;
    bool isAttacking;
    [Header("Điểm thưởng")]
    public int scoreValue = 100;
    [Header("Âm Thanh")]
    public AudioSource audioSource;
    NavMeshAgent agent;
    Animator anim;

    [Header("AI Settings")]
    public float wanderRadius = 5f;
    public float wanderTimer = 15f;
    private Vector3 startPosition;
    private float timer;

    private bool canMove = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = 180f;
            agent.stoppingDistance = 0.3f;
            agent.autoBraking = true;
            agent.acceleration = 2f;
        }

        if (attackHitbox != null) attackHitbox.SetActive(false);
        if (anim != null)
        {
            anim.applyRootMotion = false;
            anim.SetFloat("AttackSpeedMultiplier", attackAnimSpeed);
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        currentHealth = maxHealth;
        healthBarScript?.UpdateHealth(currentHealth, maxHealth);
        startPosition = transform.position;
        timer = Random.Range(0f, wanderTimer);

        float distToPlayer = (player != null) ? Vector3.Distance(transform.position, player.position) : 0f;
        Debug.Log($"👹 Boss spawn tại {transform.position}, dist to player: {distToPlayer:F1}m");
        nextAttackTime = Time.time + 2f;
        if (player != null)
        {
            Debug.LogError($"🚨 PHÁT HIỆN: Boss đang nghĩ Player là '{player.name}' (Vị trí: {player.position})");
        }
        else
        {
            Debug.LogError("🚨 LỖI: Boss KHÔNG tìm thấy Player nào cả!");
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (agent != null && !agent.isOnNavMesh)
        {
            agent.Warp(startPosition);
            Debug.LogWarning("👹 Boss off NavMesh → Warp lại!");
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (Time.frameCount % 120 == 0)
        {
            string state = distance <= attackRange ? "Attacking" :
                          distance <= chaseRange ? "Chasing" : "Patrolling";
            float remaining = agent.hasPath ? agent.remainingDistance : 0f;
            Debug.Log($"👹 [{gameObject.name}] State: {state} | Dist: {distance:F1}m | isAttacking: {isAttacking} | canMove: {canMove} | Vel: {agent.velocity.magnitude:F1}");
        }

        if (distance <= attackRange && Time.time >= nextAttackTime && !isAttacking)
        {
            StartAttack();
        }
        else if (distance <= chaseRange && !isAttacking && canMove)
        {
            ChasePlayer();
        }
        else if (distance > chaseRange && !isAttacking && canMove)
        {
            WanderAroundSpawnPoint();
        }
        else if (distance <= attackRange && Time.time < nextAttackTime && !isAttacking)
        {
            agent.isStopped = true;
            FacePlayer();
            if (anim != null) anim.SetBool("isWalking", false);
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude / agent.speed);
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        if (anim != null) anim.SetBool("isWalking", true);
        FacePlayer();
    }

    void WanderAroundSpawnPoint()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer && (!agent.hasPath || agent.remainingDistance < 1f))
        {
            Vector3 newPos = RandomNavSphere(startPosition, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0f;
            Debug.Log($"👹 [{gameObject.name}] Patrol to {newPos}");
        }

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            if (anim != null) anim.SetBool("isWalking", true);
        }
        else
        {
            if (anim != null) anim.SetBool("isWalking", false);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist + origin;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask))
        {
            return navHit.position;
        }
        return origin;
    }

    void FacePlayer()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    void FacePlayerInstant()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    public void EnableHitbox() { if (attackHitbox) attackHitbox.SetActive(true); }
    public void DisableHitbox() { if (attackHitbox) attackHitbox.SetActive(false); }

    void StartAttack()
    {
        isAttacking = true;
        canMove = false;
        if (agent) { agent.isStopped = true; agent.ResetPath(); }
        FacePlayerInstant();
        if (anim)
        {
            anim.SetTrigger("Attack");
            anim.SetBool("isWalking", false);
        }
        nextAttackTime = Time.time + attackCooldown;
        Debug.Log($"👹 StartAttack | Time: {Time.time:F1} | Next: {nextAttackTime:F1}");

        //  AUTO END IF EVENT MISS (TIMEOUT 5S)
        Invoke(nameof(EndAttack), 5f);  
    }

    public void EndAttack()
    {
        isAttacking = false;
        canMove = true;
        DisableHitbox();
        if (agent) agent.isStopped = false;
        Debug.Log("👹 EndAttack called");
        CancelInvoke(nameof(EndAttack));  // Ngăn timeout duplicate
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        healthBarScript?.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth > 0)
        {
            if (anim) anim.SetTrigger("Hit");
            if (agent)
            {
                canMove = false;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            CancelInvoke(nameof(RecoverFromHit));
            Invoke(nameof(RecoverFromHit), 0.5f);
        }
        else
        {
            Die();
        }
    }

    void RecoverFromHit()
    {
        if (!isDead && agent)
        {
            canMove = true;
            agent.isStopped = false;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (anim) anim.SetTrigger("Die");
        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(scoreValue);
        }
        if (agent) agent.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;
        this.enabled = false;
        DisableHitbox();

        gameObject.tag = "Untagged";
        Destroy(gameObject, 5f);
        Debug.Log($"💀 Destroying boss: {gameObject.name}");
    }
}