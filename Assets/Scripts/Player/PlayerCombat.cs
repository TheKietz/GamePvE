using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public int damage = 30;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;

    // Hàm này sẽ được gọi từ Animation Event
    public void DealDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Hit Enemy: -" + damage);
            }
        }
    }

    // Vẽ tầm đánh (để debug)
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
