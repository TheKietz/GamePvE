using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 1.0f;
    void Start()
    {
        Destroy(gameObject, delay);
    }
}