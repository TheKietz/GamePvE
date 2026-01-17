using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class SkillSlot
{
    public string skillName;       // Tên (Ví dụ: "Attack 1", "Taunt")
    public float cooldownTime;     // Thời gian hồi
    public Image cooldownOverlay;  // Ảnh đen mờ để xoay

    [HideInInspector] public float nextReadyTime = 0f; // Biến đếm giờ nội bộ
}

public class PlayerSkill : MonoBehaviour
{
    public Animator animator;
    public PlayerController controller;
    PlayerControls controls;

    [Header("Skill System UI")]
    // Element 0 = Attack 1
    // Element 1 = Attack 2
    // Element 2 = Attack 3
    // Element 3 = Taunt (Skill T)
    public List<SkillSlot> skills;

    [Header("Combat Settings")]
    public float detectionRange = 20f;
    public LayerMask enemyLayer;

    [Header("Taunt Battery Specifics")]
    public GameObject fireVFXPrefab;
    public Transform leftHandPos;
    public Transform rightHandPos;
    public float skillDuration = 20f;

    void Awake()
    {
        controls = new PlayerControls();

        // Gán các nút bấm vào hệ thống Skill chung
        // Lưu ý: Số thứ tự (0, 1, 2, 3) phải khớp với danh sách trong Inspector
        controls.Gameplay.Attack1.performed += _ => TryUseSkill(0);
        controls.Gameplay.Attack2.performed += _ => TryUseSkill(1);
        controls.Gameplay.Attack3.performed += _ => TryUseSkill(2);
        controls.Gameplay.Attack_T.performed += _ => TryUseSkill(3);

        controls.Gameplay.Enable();
    }

    void Start()
    {
        controller = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();

        // Reset UI khi bắt đầu
        foreach (var skill in skills)
        {
            if (skill.cooldownOverlay != null) skill.cooldownOverlay.fillAmount = 0;
        }
    }

    void Update()
    {
        // --- LOGIC UI XOAY VÒNG (GIỐNG LMHT) ---
        foreach (var skill in skills)
        {
            if (Time.time < skill.nextReadyTime)
            {
                // Tính % thời gian còn lại
                float remainingTime = skill.nextReadyTime - Time.time;

                if (skill.cooldownOverlay != null)
                    skill.cooldownOverlay.fillAmount = remainingTime / skill.cooldownTime;
            }
            else
            {
                // Hồi xong thì tắt đen
                if (skill.cooldownOverlay != null)
                    skill.cooldownOverlay.fillAmount = 0;
            }
        }
    }

    // Hàm trung gian: Kiểm tra hồi chiêu trước khi cho phép dùng
    void TryUseSkill(int index)
    {
        // Kiểm tra an toàn
        if (index >= skills.Count) return;

        SkillSlot skill = skills[index];

        // 1. Kiểm tra xem Skill đã hồi chưa?
        if (Time.time < skill.nextReadyTime)
        {
            // Debug.Log($"⏳ {skill.skillName} chưa hồi! Còn {skill.nextReadyTime - Time.time:F1}s");
            return;
        }

        // 2. Kiểm tra trạng thái Player (Chết, Đang bị đánh...) - Lấy từ code cũ
        if (controller.isDead || controller.isHit) return;

        // Nếu là chiêu đánh thường (0, 1, 2), kiểm tra xem có đang đánh dở không
        if (index < 3 && controller.isAttacking) return;

        // Nếu là chiêu Taunt (3), kiểm tra xem đang gồng chưa
        if (index == 3 && controller.isPoweredUp) return;


        // --- NẾU THỎA MÃN TẤT CẢ ---

        // A. Kích hoạt Logic
        bool success = false;
        if (index == 3)
        {
            ActivateTauntBattery();
            success = true;
        }
        else // Attack 1, 2, 3
        {
            // Gọi hàm đánh cũ của bạn
            PerformAttackLogic(index + 1); // +1 vì animator bạn đặt là 1,2,3
            success = true;
        }

        // B. Nếu kích hoạt thành công -> Bắt đầu tính giờ hồi chiêu & Quay UI
        if (success)
        {
            skill.nextReadyTime = Time.time + skill.cooldownTime;
            if (skill.cooldownOverlay != null) skill.cooldownOverlay.fillAmount = 1; // Đen sì ngay lập tức
        }
    }

    // --- LOGIC ĐÁNH (GỐC CỦA BẠN - Đã bỏ phần check cooldown vì check ở trên rồi) ---
    void PerformAttackLogic(int attackIndex)
    {
        // Debug.Log($"🗡️ BẮT ĐẦU ATTACK {attackIndex}");

        FaceClosestEnemy();

        controller.isAttacking = true;
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");

        // Logic lao tới
        float distanceToEnemy = GetDistanceToClosestEnemy();
        if (controller.rb != null && distanceToEnemy != Mathf.Infinity && distanceToEnemy > 1.2f)
        {
            controller.rb.AddForce(transform.forward * 20f, ForceMode.Impulse);
        }
        else if (controller.rb != null)
        {
            controller.rb.linearVelocity = Vector3.zero;
            controller.rb.angularVelocity = Vector3.zero;
        }

        CancelInvoke(nameof(AutoFinishAttack));
        Invoke(nameof(AutoFinishAttack), 3.0f);
    }

    // --- LOGIC TAUNT (GỐC CỦA BẠN) ---
    public void ActivateTauntBattery()
    {
        StartCoroutine(TauntRoutine());
    }

    System.Collections.IEnumerator TauntRoutine()
    {
        controller.isPoweredUp = true;
        controller.bonusDamage = 50f;
        controller.bonusDefense = 20f;
        animator.SetTrigger("Taunt");

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health) health.Heal(50f);

        GameObject vfxL = null, vfxR = null;
        if (fireVFXPrefab)
        {
            if (leftHandPos) vfxL = Instantiate(fireVFXPrefab, leftHandPos);
            if (rightHandPos) vfxR = Instantiate(fireVFXPrefab, rightHandPos);
        }

        float timer = skillDuration;
        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            if (health) health.Heal(5f);
            timer -= 1f;
        }

        controller.isPoweredUp = false;
        controller.bonusDamage = 0f;
        controller.bonusDefense = 0f;
        if (vfxL) Destroy(vfxL);
        if (vfxR) Destroy(vfxR);
    }

    // --- CÁC HÀM HỖ TRỢ (GIỮ NGUYÊN) ---
    void FaceClosestEnemy()
    {
        // (Code cũ giữ nguyên...)
        if (controller.lockOnTarget != null)
        {
            Vector3 direction = (controller.lockOnTarget.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
            return;
        }
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        Transform closestEnemy = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy.transform;
            }
        }
        if (closestEnemy != null)
        {
            Vector3 direction = (closestEnemy.position - transform.position).normalized;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    float GetDistanceToClosestEnemy()
    {
        // (Code cũ giữ nguyên...)
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        float minDistance = Mathf.Infinity;
        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance) minDistance = distance;
        }
        return minDistance;
    }

    public void FinishAttack()
    {
        controller.isAttacking = false;
    }

    private void AutoFinishAttack()
    {
        if (controller.isAttacking) controller.isAttacking = false;
    }
}