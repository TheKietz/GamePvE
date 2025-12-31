using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;

    private Animator animator;
    private EnemyAI enemyAI;

    void Start()
    {
        currentHP = maxHP;

        animator = GetComponentInChildren<Animator>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        Debug.Log($"{gameObject.name} HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("hit");
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died");

        animator.SetTrigger("die");

        // Tắt AI
        if (enemyAI != null)
            enemyAI.enabled = false;

        // Tắt collider để không bị đánh tiếp
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // (Optional) hủy object sau khi chết
        Destroy(gameObject, 3f);
    }
}
