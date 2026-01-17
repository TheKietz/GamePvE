using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerTargetLock : MonoBehaviour
{
    [Header("Setup")]
    public Transform playerTransform;
    public Animator cameraAnimator; 
    public CinemachineVirtualCamera lockOnCamera; 

    [Header("UI & Settings")]
    public Image reticleImage; 
    public float scanRadius = 20f;
    public LayerMask enemyLayer;

    Transform currentTarget;
    bool isLocked = false;
    PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.LockOn.performed += _ => ToggleLock();
    }

    void OnEnable() => controls.Gameplay.Enable();
    void OnDisable() => controls.Gameplay.Disable();

    void Update()
    {
        if (isLocked)
        {
            // Nếu địch chết hoặc đi quá xa -> Hủy lock
            if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > scanRadius + 5f)
            {
                Unlock();
            }

            // Cập nhật vị trí tâm ngắm UI
            if (reticleImage != null && currentTarget != null)
            {
                reticleImage.transform.position = Camera.main.WorldToScreenPoint(currentTarget.position + Vector3.up * 1.5f);
            }
        }
    }

    void ToggleLock()
    {
        if (isLocked) Unlock();
        else LockOn();
    }

    void LockOn()
    {
        // 1. Quét kẻ thù trong phạm vi
        Collider[] enemies = Physics.OverlapSphere(transform.position, scanRadius, enemyLayer);

        Transform bestTarget = null;
        float minDstToCenter = Mathf.Infinity; 

        // Tâm màn hình luôn là (0.5, 0.5) trong hệ Viewport
        Vector2 screenCenter = new Vector2(0.5f, 0.5f);

        foreach (var enemy in enemies)
        {
            // Bỏ qua bản thân 
            if (enemy.transform == transform) continue;

            // Chuyển vị trí 3D của địch sang tọa độ màn hình (Viewport)
            // Viewport: Góc trái dưới = (0,0), Góc phải trên = (1,1), Tâm = (0.5, 0.5)
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(enemy.transform.position);

            // Kiểm tra: Địch phải nằm PHÍA TRƯỚC Camera (z > 0) 
            // và nằm TRONG màn hình (x, y từ 0 đến 1)
            bool isOnScreen = viewportPos.z > 0 &&
                              viewportPos.x > 0 && viewportPos.x < 1 &&
                              viewportPos.y > 0 && viewportPos.y < 1;

            if (isOnScreen)
            {
                // Tính khoảng cách từ địch đến tâm màn hình (0.5, 0.5)
                float dstToCenter = Vector2.Distance(new Vector2(viewportPos.x, viewportPos.y), screenCenter);

                // Ai gần tâm nhất thì chọn người đó
                if (dstToCenter < minDstToCenter)
                {
                    minDstToCenter = dstToCenter;
                    bestTarget = enemy.transform;
                }
            }
        }

        // 2. Kích hoạt Lock
        if (bestTarget != null)
        {
            currentTarget = bestTarget;
            isLocked = true;

            // Báo Animator
            cameraAnimator.SetBool("Locked", true);

            // Gán Camera LookAt
            lockOnCamera.LookAt = currentTarget;

            // Gửi mục tiêu sang Controller để xoay người
            var controller = GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.lockOnTarget = currentTarget;

                // 2. Báo cho Animator của Player biết để chuyển sang Strafe
                // Lưu ý: Phải đảm bảo Animator của Player có biến Bool "Locked"
                controller.animator.SetBool("Locked", true);
            }

            if (reticleImage) reticleImage.gameObject.SetActive(true);

            Debug.Log($"Đã lock vào: {currentTarget.name} (Gần tâm màn hình nhất)");
        }
        else
        {
            Debug.Log("Không tìm thấy địch nào trên màn hình!");
        }
    }

    void Unlock()
    {
        isLocked = false;
        currentTarget = null;
        cameraAnimator.SetBool("Locked", false);
        lockOnCamera.LookAt = null;
        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.lockOnTarget = null;
            // Tắt chế độ Strafe của nhân vật
            controller.animator.SetBool("Locked", false);
        }
        // TRẢ VỀ ĐI BỘ THƯỜNG
        controller.lockOnTarget = null;

        if (reticleImage) reticleImage.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}