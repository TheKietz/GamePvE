using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 10f;
    public float attackRange = 2.5f;
    public float moveSpeed = 2f;

    // Animator là OPTIONAL
    public Animator animator;

    private bool isAttacking;

    void Start()
    {
        // Tự động tìm animator nếu có
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        // Nếu có animator thì mới set
        if (animator != null)
            animator.SetFloat("Distance", dist);

        if (dist > chaseRange)
        {
            // Idle
        }
        else if (dist > attackRange)
        {
            Chase();
        }
        else
        {
            Attack();
        }
    }

    void Chase()
    {
        if (isAttacking) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.forward = dir;
    }

    void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Attack");

        Invoke(nameof(ResetAttack), 1.5f);
    }

    void ResetAttack()
    {
        isAttacking = false;
    }
}
