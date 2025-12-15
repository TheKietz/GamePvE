using UnityEngine;
using UnityEngine.InputSystem; // Thêm namespace quan trọng

public class PlayerSkill : MonoBehaviour
{
    public Animator animator;
    public PlayerController playerController;

    private float nextAttackTime1 = 0f;
    private float nextAttackTime2 = 0f;
    private float nextAttackTime3 = 0f;

    private const float COOLDOWN_1 = 2f;
    private const float COOLDOWN_2 = 2f;
    private const float COOLDOWN_3 = 4f;

    // THAY ĐỔI: Khai báo Input Actions Asset
    private PlayerControls controls;

    void Awake()
    {
        // Khởi tạo Input Actions
        controls = new PlayerControls();

        // Thiết lập Events cho các hành động tấn công
        controls.Gameplay.Attack_J.performed += OnAttackJPerformed;
        controls.Gameplay.Attack_K.performed += OnAttackKPerformed;
        controls.Gameplay.Attack_L.performed += OnAttackLPerformed;

        // Kích hoạt Action Map
        controls.Gameplay.Enable();
    }

    void OnDestroy()
    {
        // Đảm bảo tắt Actions khi đối tượng bị hủy
        controls.Gameplay.Disable();
    }

    void Start()
    {
        // ... (Giữ nguyên logic kiểm tra animator và playerController)
        if (animator == null)
        {
            Debug.LogError("Animator component is not assigned to PlayerSkill script.");
        }
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
        if (playerController == null)
        {
            Debug.LogError("PlayerController component is not found or assigned to PlayerSkill script.");
        }
    }

    // THAY ĐỔI: Loại bỏ hàm Update và HandleAttackInput cũ

    /// <summary>
    /// Hàm xử lý tấn công J (Slash)
    /// </summary>
    private void OnAttackJPerformed(InputAction.CallbackContext context)
    {
        if (animator == null || playerController == null || playerController.isAttacking)
            return;

        if (Time.time >= nextAttackTime1) // Kiểm tra hồi chiêu
        {
            TriggerAttack(1); // Kích hoạt tấn công
            nextAttackTime1 = Time.time + COOLDOWN_1; // Bắt đầu hồi chiêu
            Debug.Log("Trigger Attack: J (Slash). Cooldown started: 2s");
        }
    }

    /// <summary>
    /// Hàm xử lý tấn công K (Slash3)
    /// </summary>
    private void OnAttackKPerformed(InputAction.CallbackContext context)
    {
        if (animator == null || playerController == null || playerController.isAttacking)
            return;

        if (Time.time >= nextAttackTime2) // Kiểm tra hồi chiêu
        {
            TriggerAttack(2);
            nextAttackTime2 = Time.time + COOLDOWN_2;
            Debug.Log("Trigger Attack: K (Slash3). Cooldown started: 2s");
        }
    }

    /// <summary>
    /// Hàm xử lý tấn công L (Slash4)
    /// </summary>
    private void OnAttackLPerformed(InputAction.CallbackContext context)
    {
        if (animator == null || playerController == null || playerController.isAttacking)
            return;

        if (Time.time >= nextAttackTime3) // Kiểm tra hồi chiêu
        {
            TriggerAttack(3);
            nextAttackTime3 = Time.time + COOLDOWN_3;
            Debug.Log("Trigger Attack: L (Slash4). Cooldown started: 4s");
        }
    }

    // ... (Giữ nguyên TriggerAttack, StartAttack, và FinishAttack)

    void TriggerAttack(int attackIndex)
    {
        animator.SetInteger("AttackIndex", attackIndex);
        animator.SetTrigger("Attack");
        StartAttack();
    }

    public void StartAttack()
    {
        if (playerController != null)
        {
            playerController.isAttacking = true;
        }
    }

    public void FinishAttack()
    {
        if (playerController != null)
        {
            playerController.isAttacking = false;
        }
    }
}