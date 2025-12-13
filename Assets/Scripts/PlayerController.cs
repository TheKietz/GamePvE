using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody playerRigidbody;
    private bool isGrounded;
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start project");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Start Update");
        HandleMovement();
    }
    public void HandleMovement()
    {
        Debug.Log("Handle Movement");
        float ipHorizontal = Input.GetAxis("Horizontal");
        float ipVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(ipHorizontal, 0, ipVertical);

        if (movement.magnitude > 1)
        {
            movement.Normalize();
           
        }
        if(movement == Vector3.zero)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }

        playerRigidbody.MovePosition(transform.position + movement * Time.deltaTime * 15);
        
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            playerRigidbody.AddForce(new Vector3(0,1,0) * 5, ForceMode.Impulse);
            animator.SetTrigger("Jump");
            isGrounded = false;
        }
        HandleRotation(movement);
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
}
