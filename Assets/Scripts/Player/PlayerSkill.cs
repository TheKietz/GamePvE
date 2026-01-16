using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : MonoBehaviour
{
    public Animator animator;
    public PlayerController controller;

    float detectionRange = 20f;
    public LayerMask enemyLayer;
    PlayerControls controls;

    float cd1, cd2, cd3;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Attack1.performed += _ => TryAttack(1, ref cd1, 0.5f);
        controls.Gameplay.Attack2.performed += _ => TryAttack(2, ref cd2, 0.7f);
        controls.Gameplay.Attack3.performed += _ => TryAttack(3, ref cd3, 1.0f);
        controls.Gameplay.Enable();
    }

    void TryAttack(int index, ref float nextTime, float cooldown)
    {
        // 🔧 FIX 1: Kiểm tra điều kiện CHI TIẾT HƠN
        if (controller.isDead)
        {
            Debug.Log("❌ Không tấn công được: Đã chết");
            return;
        }

        if (controller.isAttacking)
        {
            Debug.Log("❌ Không tấn công được: Đang tấn công");
            return;
        }

        if (controller.isHit)
        {
            Debug.Log("❌ Không tấn công được: Đang bị hit");
            return;
        }

        if (Time.time < nextTime)
        {
            Debug.Log($"❌ Không tấn công được: Cooldown ({nextTime - Time.time:F1}s)");
            return;
        }

        // 🔧 FIX 2: LOG ra để debug
        Debug.Log($"🗡️ BẮT ĐẦU ATTACK {index} | Locked: {controller.lockOnTarget != null}");

        // 1. Xoay mặt về địch (nếu có)
        FaceClosestEnemy();

        // 🔧 FIX 3: Set isAttacking TRƯỚC KHI trigger animation
        controller.isAttacking = true;
        // 2. Set animator parameters
        animator.SetInteger("AttackIndex", index);
        animator.SetTrigger("Attack");

        // 3. Tính khoảng cách
        float distanceToEnemy = GetDistanceToClosestEnemy();

        if (controller.rb != null)
        {
            // Logic lao tới
            if (distanceToEnemy != Mathf.Infinity && distanceToEnemy > 1.2f)
            {
                // Lao tới
                controller.rb.AddForce(transform.forward * 20f, ForceMode.Impulse);
                Debug.Log($"⚡ Lao tới địch (khoảng cách: {distanceToEnemy:F1}m)");
            }
            else
            {
                // Đứng yên đánh
                controller.rb.linearVelocity = Vector3.zero;
                controller.rb.angularVelocity = Vector3.zero;
                Debug.Log("🎯 Đánh tại chỗ");
            }
        }

        // 4. Set cooldown
        nextTime = Time.time + cooldown;

        // 🔧 FIX 4: Tăng thời gian AutoFinish để đủ thời gian animation chạy
        CancelInvoke(nameof(AutoFinishAttack));
        Invoke(nameof(AutoFinishAttack), 3.0f);
    }

    void FaceClosestEnemy()
    {
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

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    float GetDistanceToClosestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        float minDistance = Mathf.Infinity;

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }
        return minDistance;
    }

    // 🔧 FIX 5: Hàm này được gọi từ Animation Event
    public void FinishAttack()
    {
        controller.isAttacking = false;
        Debug.Log("✅ Animation Event: FinishAttack");
    }

    // 🔧 FIX 6: Backup nếu Animation Event không được gọi
    private void AutoFinishAttack()
    {
        if (controller.isAttacking)
        {
            controller.isAttacking = false;
            Debug.Log("⚠️ AutoFinishAttack (Animation Event bị miss?)");
        }
    }
}