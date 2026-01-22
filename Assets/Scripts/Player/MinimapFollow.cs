using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // Kéo nhân vật Player vào đây

    void LateUpdate() // Dùng LateUpdate để mượt hơn
    {
        if (player != null)
        {
            // Lấy vị trí mới của Player
            Vector3 newPosition = player.position;

            // Giữ nguyên độ cao Y của Camera (để không bị rơi xuống đất)
            newPosition.y = transform.position.y;

            // Cập nhật vị trí
            transform.position = newPosition;
        }
    }
}