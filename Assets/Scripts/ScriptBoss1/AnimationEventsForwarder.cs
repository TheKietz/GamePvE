using UnityEngine;

public class AnimationEventsForwarder : MonoBehaviour
{
    private BossController bossController;

    void Start()
    {
        // Tìm BossController trên parent/root
        bossController = GetComponentInParent<BossController>();
        if (bossController == null)
            Debug.LogError("❌ AnimationEventsForwarder: Không tìm thấy BossController trên parent!");
    }

    // Forward EnableHitbox (Animation Event ở frame đầu Attack anim)
    public void EnableHitbox()
    {
        bossController?.EnableHitbox();
    }

    // Forward DisableHitbox (Animation Event ở frame cuối Attack anim)
    public void DisableHitbox()
    {
        bossController?.DisableHitbox();
    }

    // Forward EndAttack (nếu dùng, Animation Event cuối Attack)
    public void EndAttack()
    {
        bossController?.EndAttack();
    }
}