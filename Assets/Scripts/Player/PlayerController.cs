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
        controls.Gameplay.Attack.performed += OnAttackPerformed;
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
        animator.SetBool("Grounded", isGrounded);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // Xử lý nhảy (Jump) chỉ khi KHÔNG tấn công
        if (!isAttacking && isGrounded == true)
        {
            Debug.Log("Jump");
            playerRigidbody.AddForce(new Vector3(0, 1, 0) * 5, ForceMode.Impulse);
            animator.SetTrigger("Jump");
            isGrounded = false;
        }
    }

    public void HandleMovement()
    {
        // 1. Tính toán vector di chuyển
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);

        // 2. Xoay nhân vật
        HandleRotation(movement);

        // 3. LOGIC ANIMATOR: Chỉ "Walk" khi có di chuyển VÀ đang ở dưới đất
        // Nếu đang trên trời (isGrounded == false), bắt buộc tắt isWalking
        if (movement != Vector3.zero && !isAttacking && isGrounded)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        // 4. LOGIC VẬT LÝ: Di chuyển nhân vật
        // Cho phép di chuyển trên không (nhảy xa) nhưng không bật animation đi bộ
        if (!isAttacking && movement.magnitude > 0.1f)
        {
            // Di chuyển bằng Code thuần túy
            Vector3 moveDir = movement.normalized * currentMoveSpeed * Time.deltaTime;
            playerRigidbody.MovePosition(transform.position + moveDir);
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
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!isAttacking)
        {
            // Nếu đang nhảy thì gọi đòn đánh trên không, nếu ở đất thì gọi đòn đánh thường
            StartCoroutine(AttackRoutine());
        }
    }
    System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetTrigger("Attack"); // Trigger chung, hoặc tách ra "AirAttack"

        // Chờ hết animation (ví dụ 0.5s)
        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
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
    //private void OnAnimatorMove()
    //{
    //    if (animator && playerRigidbody)
    //    {
    //        Vector3 deltaPosition = animator.deltaPosition;
    //        // Chỉ lấy chuyển động trục X và Z (ngang), GÁN TRỤC Y = 0
    //        deltaPosition.y = 0;
    //        playerRigidbody.MovePosition(playerRigidbody.position + deltaPosition);
    //    }
    //}
}