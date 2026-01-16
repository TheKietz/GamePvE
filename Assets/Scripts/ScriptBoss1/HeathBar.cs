using UnityEngine;
using UnityEngine.UI; 

public class HeathBar : MonoBehaviour
{
    public Image fillImage; 

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }
    }
}