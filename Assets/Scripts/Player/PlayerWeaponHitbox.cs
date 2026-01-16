using UnityEngine;

public class PlayerWeaponHitbox : MonoBehaviour
{
    public float damage = 20f;
    public Collider myCollider;
    public GameObject bloodEffectPrefab;
    public AudioClip hitSound;
    bool hasHit;
    bool isActive; 

    void Start()
    {
        if (myCollider == null) myCollider = GetComponent<Collider>();
        myCollider.enabled = false;
        myCollider.isTrigger = true;
    }

    public void EnableHitbox()
    {
        myCollider.enabled = true;
        hasHit = false;
        isActive = true; // Bắt đầu tính hit
    }

    public void DisableHitbox()
    {
        // 🔍 LOGIC BÁO TRƯỢT:
        // Nếu đang tấn công (isActive) VÀ chưa trúng ai (!hasHit) -> Đích thị là trượt
        if (isActive && !hasHit)
        {
            Debug.Log($"❌ ĐÁNH TRƯỢT! ({gameObject.name}) - Hãy chỉnh Collider to/dài ra!");
        }

        myCollider.enabled = false;
        isActive = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Check va chạm với Enemy
        if (other.CompareTag("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            BossController boss = other.GetComponentInParent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                hasHit = true;

                // 📊 BÁO DAME CỤ THỂ
                Debug.Log($"✅ ĐÁNH TRÚNG BOSS! Gây {damage} sát thương! (Vũ khí: {gameObject.name})");
                // 🔥 TẠO HIỆU ỨNG MÁU
                if (bloodEffectPrefab != null)
                {
                    // Tìm điểm va chạm gần nhất để máu văng ra đúng chỗ
                    Vector3 hitPoint = other.ClosestPoint(transform.position);

                    // Sinh ra hiệu ứng máu tại điểm va chạm
                    // Quaternion.LookRotation(hitPoint - transform.position): Hướng máu văng ra xa người đánh
                    Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
                }
                if (hitSound != null)
                {
                    // Dùng PlayClipAtPoint để tạo ra một cái loa tạm thời tại vị trí va chạm
                    // (Giúp âm thanh nghe đúng vị trí 3D)
                    AudioSource.PlayClipAtPoint(hitSound, transform.position, 1.0f);
                }
            }
        }
    }
}