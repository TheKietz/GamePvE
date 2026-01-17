using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Cần thêm dòng này để dùng Coroutine

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public Image damageFlashImage; // 🔥 Kéo cái ảnh DamageFlash vào đây

    [Header("Settings")]
    public float flashSpeed = 5f;  // Tốc độ mờ dần của màu đỏ
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f); // Màu đỏ, độ trong suốt 0.5

    public PlayerController controller;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }
    public void Heal(float amount)
    {
        if (controller.isDead) return;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Không cho máu vượt quá Max
        UpdateUI();
        Debug.Log($"💚 Đã hồi {amount} máu! Hiện tại: {currentHealth}");
    }
    public void TakeDamage(float damage)
    {
        // Debug.Log("TAKE DAMAGE: " + damage);
        if (controller.isDead) return;
        float finalDamage = Mathf.Max(damage - controller.bonusDefense, 0);
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        // 🔥 GỌI HIỆU ỨNG CHỚP ĐỎ
        if (damageFlashImage != null)
        {
            StartCoroutine(FlashDamage());
        }

        if (currentHealth <= 0)
        {
            controller.Die();
            if (GameManager.instance != null)
            {
                GameManager.instance.Invoke("GameOver", 3f);
            }
        }
        else
        {
            if (controller != null)
            {
                controller.ForceTakeHit();
            }
        }
    }

    void UpdateUI()
    {
        if (healthSlider)
            healthSlider.value = currentHealth / maxHealth;
    }

    // 🔥 Coroutine làm màn hình chớp đỏ rồi mờ dần
    IEnumerator FlashDamage()
    {
        // 1. Bật màu đỏ lên ngay lập tức
        damageFlashImage.color = flashColor;

        // 2. Mờ dần (Fade out)
        while (damageFlashImage.color.a > 0.01f)
        {
            Color currentColor = damageFlashImage.color;
            // Giảm Alpha từ từ
            currentColor.a = Mathf.Lerp(currentColor.a, 0f, flashSpeed * Time.deltaTime);
            damageFlashImage.color = currentColor;
            yield return null; // Chờ đến frame tiếp theo
        }

        // 3. Đảm bảo tắt hẳn
        damageFlashImage.color = Color.clear;
    }
}