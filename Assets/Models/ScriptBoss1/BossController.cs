using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    [Header("Cài đặt Mục tiêu")]
    public Transform player;

    [Header("Kết nối Thanh Máu")]
    public HeathBar healthBarScript; 
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Thông số Boss")]
    public float chaseRange = 10f;
    public float attackRange = 1f;      
    public float attackCooldown = 3f;  

    private NavMeshAgent agent;
    private Animator anim;
    private float nextAttackTime = 0f;
    private bool isDead = false;        // Biến kiểm tra xem chết chưa

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        currentHealth = maxHealth;

        // Gọi sang script kia để reset thanh máu đầy cây lúc đầu
        if (healthBarScript != null)
        {
            healthBarScript.UpdateHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        // 1. QUAN TRỌNG: Nếu chết rồi thì dừng mọi hoạt động
        if (isDead) return;

        // --- TEST: Bấm Space để thử mất máu ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }
        // --------------------------------------

        if (player == null) return;

        // 2. Di chuyển và Tấn công
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            agent.isStopped = true;
            anim.SetBool("isMoving", false);
            transform.LookAt(player);

            if (Time.time >= nextAttackTime)
            {
                anim.SetTrigger("Attack");
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else if (distance <= chaseRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetBool("isMoving", true);
        }
        else
        {
            agent.isStopped = true;
            anim.SetBool("isMoving", false);
        }
    }

    // Hàm này để nhận sát thương
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Gọi sang script HeathBar để cập nhật giao diện
        if (healthBarScript != null)
        {
            healthBarScript.UpdateHealth(currentHealth, maxHealth);
        }

        // Kiểm tra chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        agent.isStopped = true; // Dừng di chuyển ngay
        anim.SetBool("isMoving", false);

        anim.SetTrigger("Die"); // Chạy animation chết

        // Ẩn thanh máu đi cho đẹp
        if (healthBarScript != null)
        {
            healthBarScript.gameObject.SetActive(false);
        }

        Destroy(gameObject, 4f); // Biến mất sau 4 giây
    }
}