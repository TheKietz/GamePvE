using UnityEngine;
using UnityEngine.UI;

public class BossLocator : MonoBehaviour
{
    [Header("Setup")]
    public Transform arrowUI; // Kéo cái Image Mũi tên vào đây
    public float hideDistance = 3f; // Nếu Boss ở gần hơn 3m thì ẩn mũi tên đi (cho đỡ vướng)

    private Transform targetBoss;

    void Update()
    {
        // 1. Tìm Boss (Nếu chưa có hoặc Boss cũ đã chết)
        if (targetBoss == null)
        {
            FindClosestBoss();
            if (arrowUI.gameObject.activeSelf) arrowUI.gameObject.SetActive(false);
            return;
        }

        // 2. Tính toán vị trí
        float distance = Vector3.Distance(transform.position, targetBoss.position);

        // Nếu Boss còn sống và ở xa -> Hiện mũi tên
        if (distance > hideDistance)
        {
            if (!arrowUI.gameObject.activeSelf) arrowUI.gameObject.SetActive(true);

            // Tính hướng từ Player tới Boss
            Vector3 direction = targetBoss.position - transform.position;
            direction.y = 0; // Chỉ xoay ngang, không chúi đầu xuống đất

            // 3. Xoay mũi tên
            if (direction != Vector3.zero)
            {
                // Mũi tên nằm bẹp (X=90), nên ta xoay trục Z của nó theo hướng Boss
                // Lưu ý: Tùy vào ảnh gốc của bạn quay đầu đi đâu mà có thể cần +/- 90 độ
                Quaternion lookRot = Quaternion.LookRotation(direction);
                arrowUI.rotation = Quaternion.Slerp(arrowUI.rotation, lookRot, Time.deltaTime * 5f);

                // Giữ cho Canvas luôn nằm ngang (nếu Player bị nghiêng)
                arrowUI.localEulerAngles = new Vector3(90, arrowUI.localEulerAngles.y, 0);
            }
        }
        else
        {
            // Boss ở ngay cạnh rồi thì ẩn đi
            arrowUI.gameObject.SetActive(false);
        }
    }

    void FindClosestBoss()
    {
        // Tìm tất cả vật thể có Tag là "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float minDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject enemy in enemies)
        {
            // Kiểm tra xem có phải Boss không (dựa vào tên hoặc component)
            // Ở đây mình check đơn giản là mọi Enemy. Bạn có thể check cụ thể tên "Boss"
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy.transform;
            }
        }
        targetBoss = closest;
    }
}