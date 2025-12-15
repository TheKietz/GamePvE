using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerController : MonoBehaviour
{
    public Rigidbody playerRigidbody;
    private bool isGrounded;
    public Animator animator;

    [HideInInspector] public bool isAttacking = false;

    // Khai báo Input Actions Asset
    private PlayerControls controls;
    // Tốc độ di chuyển
    public float walkSpeed = 3.0f; // Tốc độ đi bộ (đặt trong Inspector)
    public float sprintSpeed = 6.0f; // Tốc độ chạy nhanh (đặt trong Inspector)
    private float currentMoveSpeed; // Tốc độ thay đổi trong runtime
    // Biến lưu trữ giá trị di chuyển mới
    private Vector2 moveInput;
    
    void Awake()
    {
        currentMoveSpeed = walkSpeed; // Bắt đầu bằng tốc độ đi bộ
        // Khởi tạo Input Actions
        controls = new PlayerControls();

        // Thiết lập Event cho hành động Jump
        controls.Gameplay.Jump.performed += OnJumpPerformed;

        // Thiết lập sự kiện đọc giá trị di chuyển liên tục
        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;

        // Thiết lập Events cho hành động Sprint (Nhấn giữ Shift)
        controls.Gameplay.Sprint.performed += OnSprintPerformed; // Khi nhấn giữ Shift
        controls.Gameplay.Sprint.canceled += OnSprintCanceled;   // Khi nhả Shift

        // Kích hoạt Action Map
        controls.Gameplay.Enable();

    }

    void OnDestroy()
    {
        // Đảm bảo tắt Actions khi đối tượng bị hủy
        controls.Gameplay.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start project");
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Xử lý nhảy (Jump) chỉ khi KHÔNG tấn công
        if (!isAttacking && isGrounded == true)
        {
            playerRigidbody.AddForce(new Vector3(0, 1, 0) * 5, ForceMode.Impulse);
            animator.SetTrigger("Jump");
            isGrounded = false;
        }
    }

    public void HandleMovement()
    {
        Debug.Log("Handle Movement");

        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);

        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // Luôn kiểm tra điều kiện isWalking:
        if (movement == Vector3.zero || isAttacking)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }

        // === LOGIC DI CHUYỂN VÀ NHẢY CHỈ KHI KHÔNG TẤN CÔNG ===
        if (!isAttacking)
        {
            // ÁP DỤNG DI CHUYỂN BẰNG CODE CHO WALKING/RUNNING
            if (animator.GetBool("isWalking"))
            {
                playerRigidbody.MovePosition(transform.position + movement * Time.deltaTime * currentMoveSpeed);
            }

            HandleRotation(movement);
        }
    }

    // Hàm được gọi khi người chơi nhấn giữ phím Shift
    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        // Nếu đang tấn công, không cho phép thay đổi tốc độ
        if (isAttacking) return;

        // Đặt tốc độ hiện tại thành tốc độ chạy nhanh
        currentMoveSpeed = sprintSpeed;
        Debug.Log("Sprinting: " + currentMoveSpeed);
    }

    // Hàm được gọi khi người chơi nhả phím Shift
    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        // Đặt tốc độ hiện tại trở lại tốc độ đi bộ cơ bản
        currentMoveSpeed = walkSpeed;
        Debug.Log("Stopping Sprint: " + currentMoveSpeed);
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            isGrounded = true;
        }
    }
    public void HandleRotation(Vector3 playerMovementInput)
    {
        Vector3 lookDirection = playerMovementInput;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = rotation;
        }
    }
    private void OnAnimatorMove()
    {
        if (animator && playerRigidbody)
        {
            Vector3 deltaPosition = animator.deltaPosition;
            playerRigidbody.MovePosition(playerRigidbody.position + deltaPosition);
        }
    }
}