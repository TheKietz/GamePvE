using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Luôn xoay Canvas để đối mặt với Camera
        transform.LookAt(transform.position + cam.forward);
    }
}