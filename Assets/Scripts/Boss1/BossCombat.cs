using UnityEngine;

public class BossCombat : MonoBehaviour
{
    public int damage = 20;
    public float attackRange = 2f;
    public LayerMask playerLayer;
    public Transform attackPoint;

    bool isAttacking = false;

    // gọi từ AI khi đủ điều kiện
    public void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        GetComponent<Animator>().SetTrigger("Attack");
    }

    // 🔥 GỌI TỪ ANIMATION EVENT
    public void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            PlayerHealth player = hit.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }

    // 🔁 GỌI TỪ ANIMATION EVENT (cuối anim)
    public void ResetAttack()
    {
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
