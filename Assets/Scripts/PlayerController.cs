using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 1f);

    private float horizontal;
    private bool isGrounded;
    private bool jumpRequested;




    void Awake()
    {
        if (groundCheck == null)
        {
            groundCheck = transform.Find("GroundCheck");
        }

        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
        }
    }
    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapBox(
            GetGroundCheckPosition(),
            groundCheckSize,
            0f,
            groundLayer) != null;

        Jump();
        Move();
        
    }

    private void Move()
    {
        Vector3 movement = new Vector3(horizontal, 0f, 0f);
        transform.Translate(movement * speed * Time.fixedDeltaTime);
    }

    private void Jump(){
        if (jumpRequested && isGrounded)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        }
        else if (jumpRequested && !isGrounded)
        {
            Debug.Log("Player is not grounded and cannot jump.");
        }

        jumpRequested = false;
    }
    private Vector2 GetGroundCheckPosition()
    {
        if (groundCheck != null)
        {
            Debug.Log("Using groundCheck position for ground detection.");
            return groundCheck.position;
        }

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            Debug.Log("Using player collider bounds for ground detection.");
            return new Vector2(playerCollider.bounds.center.x, playerCollider.bounds.min.y);
        }

        return transform.position;
    }


    //(DEBUG) Draws a wireframe cube in the Scene view to visualize the ground check area
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 checkPosition = GetGroundCheckPosition();
        Gizmos.DrawWireCube(checkPosition, groundCheckSize);
    }
}
