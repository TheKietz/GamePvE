using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public Animator animator;
    private Transform mainCameraTransform;
    public float walkSpeed = 4f;
    public float sprintSpeed = 6f;
    private Quaternion lastLockOnRotation;
    [HideInInspector] public Transform lockOnTarget;
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isDead;
    [HideInInspector] public bool isHit;
    [Header("Hitbox")]
    public PlayerWeaponHitbox rightHandHitbox;
    public PlayerWeaponHitbox leftHandHitbox;
    public PlayerWeaponHitbox leftFootHitbox;
    public PlayerWeaponHitbox rightFootHitbox;
    [Header("Buff System")]
    public bool isPoweredUp = false; // Đang gồng hay không?
    public float bonusDamage = 0f;   // Dame cộng thêm
    public float bonusDefense = 0f;  // Giáp cộng thêm
    [Header("Âm Thanh")]
    public AudioSource audioSource;
    public AudioClip swingSound;
    bool isGrounded;
    Vector2 moveInput;
    float currentSpeed;
    [Header("Cài đặt Nhảy")]
    public LayerMask groundLayer;
    public float jumpForce = 5f;
    PlayerControls controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = walkSpeed;

        controls = new PlayerControls();

        // --- BỘ ĐIỀU KHIỂN ---
        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += _ => moveInput = Vector2.zero;
        controls.Gameplay.Sprint.performed += _ => currentSpeed = sprintSpeed;
        controls.Gameplay.Sprint.canceled += _ => currentSpeed = walkSpeed;
        controls.Gameplay.Jump.performed += _ => Jump();

        // Kích hoạt
        controls.Gameplay.Enable();

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        bool isMovingInput = moveInput.sqrMagnitude > 0.01f;

        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 1.2f, groundLayer);
        animator.SetBool("Grounded", isGrounded);

        // CRITICAL: Set Locked state TRƯỚC
        bool isLockedOn = lockOnTarget != null;
        animator.SetBool("Locked", isLockedOn);

        // DI CHUYỂN
        animator.applyRootMotion = false;

        if (isMovingInput && !isAttacking && !isHit)
        {
            rb.linearDamping = 0f;
            HandleMovement();
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);

            //  FORCE RESET về 0
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", 0f);
            animator.SetFloat("Turn", 0f);

            if (!isHit)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                rb.linearDamping = isGrounded ? 100f : 0f;
            }
            else
            {
                rb.linearDamping = 1f;
            }
        }

        // Ground stick
        bool isJumpingAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("Jump");
        if (isGrounded && !isJumpingAnimation)
        {
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }

        // DEBUG
        //if (Time.frameCount % 30 == 0)
        //{
        //    DebugAnimatorState();
        //}
    }

    void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            rb.MoveRotation(rb.rotation * animator.deltaRotation);
        }
    }

    void HandleMovement()
    {
        if (isAttacking || isHit || isDead)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        bool isLockedOn = lockOnTarget != null;

        // Tính hướng di chuyển WORLD SPACE
        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 targetMoveDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        if (isLockedOn)
        {         

            Vector3 localMoveDir = transform.InverseTransformDirection(targetMoveDir);

            // ⚡ Gửi LOCAL coordinates vào Animator
            float horizontal = localMoveDir.x; // Trái/phải theo hướng nhìn player
            float vertical = localMoveDir.z;   // Tới/lui theo hướng nhìn player

            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);
            animator.SetFloat("Turn", 0f);

            //Debug.Log($"🎯 Lock-on Input → Local H={horizontal:F2}, V={vertical:F2} | World Dir={targetMoveDir}");

            // Xoay mặt về địch
            if (lockOnTarget != null)
            {
                Vector3 dirToEnemy = (lockOnTarget.position - transform.position);
                dirToEnemy.y = 0;
                dirToEnemy.Normalize();
                Quaternion targetLookRotation = Quaternion.LookRotation(dirToEnemy);
                lastLockOnRotation = Quaternion.Slerp(lastLockOnRotation, targetLookRotation,
    25f * Time.fixedDeltaTime);
                localMoveDir = Quaternion.Inverse(lastLockOnRotation) * targetMoveDir;
                 horizontal = localMoveDir.x;
                 vertical = localMoveDir.z;
                transform.rotation = Quaternion.Slerp(transform.rotation, lastLockOnRotation,
    25f * Time.fixedDeltaTime);
            }
        }
        else
        {
            // ⚡ NORMAL MODE: Không cần chuyển đổi
            animator.SetFloat("Horizontal", 0f);
            animator.SetFloat("Vertical", 0f);
            animator.SetFloat("Turn", moveInput.x);

            // Xoay theo hướng chạy
            if (targetMoveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetMoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    15f * Time.fixedDeltaTime);
            }
        }

        // Apply movement (WORLD SPACE)
        if (targetMoveDir.sqrMagnitude > 0.01f)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hitInfo, 1.5f))
            {
                targetMoveDir = Vector3.ProjectOnPlane(targetMoveDir, hitInfo.normal).normalized;
                rb.linearVelocity = targetMoveDir * currentSpeed;
            }
            else
            {
                Vector3 targetVelocity = targetMoveDir * currentSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = targetVelocity;
            }
        }
    }

    void Jump()
    {
        if (isDead || !isGrounded || isAttacking || isHit)
        {
            Debug.Log($"❌ Không nhảy được: isDead={isDead}, grounded={isGrounded}, attacking={isAttacking}, hit={isHit}");
            return;
        }

        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.SetTrigger("Jump");
        isGrounded = false;

        Debug.Log("✅ Nhảy!");
    }

    // ============= HITBOX FUNCTIONS =============

    public void EnableDoubleHand()
    {
        isAttacking = true;
        if (rightHandHitbox) rightHandHitbox.EnableHitbox();
        if (leftHandHitbox) leftHandHitbox.EnableHitbox();
        PlaySwingSound();
        if (rightFootHitbox) rightFootHitbox.DisableHitbox();
    }

    public void EnableKick()
    {
        isAttacking = true;
        if (leftFootHitbox != null)
        {
            leftFootHitbox.EnableHitbox();
            Debug.Log("🦶 Đã bật Hitbox CHÂN TRÁI!");
            PlaySwingSound();
        }
        if (rightFootHitbox != null) rightFootHitbox.DisableHitbox();
        if (rightHandHitbox) rightHandHitbox.DisableHitbox();
        if (leftHandHitbox) leftHandHitbox.DisableHitbox();
    }

    public void EnableRightHand()
    {
        isAttacking = true;
        if (rightHandHitbox) rightHandHitbox.EnableHitbox();
        PlaySwingSound();
        if (leftHandHitbox) leftHandHitbox.DisableHitbox();
        if (rightFootHitbox) rightFootHitbox.DisableHitbox();
    }

    public void EnableLeftHand()
    {
        isAttacking = true;
        if (leftHandHitbox) leftHandHitbox.EnableHitbox();
        PlaySwingSound();
        if (rightHandHitbox) rightHandHitbox.DisableHitbox();
        if (rightFootHitbox) rightFootHitbox.DisableHitbox();
    }

    public void ResetHitbox()
    {
        if (rightHandHitbox) rightHandHitbox.DisableHitbox();
        if (leftHandHitbox) leftHandHitbox.DisableHitbox();
        if (rightFootHitbox) rightFootHitbox.DisableHitbox();
        if (leftFootHitbox) leftFootHitbox.DisableHitbox();
    }

    void PlaySwingSound()
    {
        if (audioSource && swingSound)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(swingSound);
        }
    }

    public void FinishAttack()
    {
        isAttacking = false;
        ResetHitbox();
        Debug.Log("✅ Combo kết thúc!");
    }

    public void FinishHit()
    {
        isHit = false;
    }

    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            isAttacking = false;
            animator.SetTrigger("Die");
        }
    }

    public void ForceTakeHit()
    {
        isAttacking = false;
        isHit = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", 0);
        animator.SetFloat("Speed", 0);
        animator.ResetTrigger("Attack");

        animator.SetTrigger("Hit");
        CancelInvoke(nameof(FinishHit));
        Invoke(nameof(FinishHit), 0.5f);
    }

    public void EndHitState()
    {
        isHit = false;
    }
    void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    void OnDisable()
    {
        controls.Gameplay.Disable();
    }
    // DEBUG FUNCTION
    //void DebugAnimatorState()
    //{
    //    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    //    string stateName = "Unknown";

    //    AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
    //    if (clipInfo.Length > 0)
    //    {
    //        stateName = clipInfo[0].clip.name;
    //    }

    //    Debug.Log($"[ANIMATOR] State: <color=cyan>{stateName}</color> | " +
    //              $"Locked: <color=yellow>{animator.GetBool("Locked")}</color> | " +
    //              $"H: <color=green>{animator.GetFloat("Horizontal"):F2}</color> | " +
    //              $"V: <color=green>{animator.GetFloat("Vertical"):F2}</color> | " +
    //              $"Turn: <color=green>{animator.GetFloat("Turn"):F2}</color> | " +
    //              $"isWalking: {animator.GetBool("isWalking")} | " +
    //              $"isAttacking: <color=red>{isAttacking}</color> | " +
    //              $"Target: {(lockOnTarget != null ? lockOnTarget.name : "None")}");
    //}
}