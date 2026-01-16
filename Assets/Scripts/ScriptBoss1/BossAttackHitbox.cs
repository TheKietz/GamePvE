using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    public float damage = 20f;
    [Header("Hiệu ứng")]
    public GameObject bloodEffectPrefab; 
    public AudioClip hitSound;           

    bool hasHit;

    void OnEnable()
    {
        hasHit = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Chỉ xử lý khi chạm vào Player
        if (other.CompareTag("Player"))
        {
            // 1. Gây sát thương
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hasHit = true;

                // 2. Tạo hiệu ứng máu
                if (bloodEffectPrefab != null)
                {
                    // Tìm điểm va chạm trên người Player để máu văng ra từ đó
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
                }

                // 3. Phát âm thanh
                if (hitSound != null)
                {
                    // Tạo loa tạm thời tại vị trí va chạm
                    AudioSource.PlayClipAtPoint(hitSound, transform.position, 1.0f);
                }

                Debug.Log($"👹 BOSS ĐÁNH TRÚNG: {damage} sát thương!");
            }
        }
    }
}