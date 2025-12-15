using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int maxHP = 300;
    int hp;

    void Start() => hp = maxHP;

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Boss Dead");
        Destroy(gameObject);
    }
}

