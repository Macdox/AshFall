using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.2f);
    [SerializeField] private float coyoteTimeMax = 0.15f;
    [SerializeField] private float jumpBufferMax = 0.1f;


    private Rigidbody2D rb;




    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private float horizontal;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();


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
            jumpBufferCounter = jumpBufferMax;
        }
    }
    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapBox(
            GetGroundCheckPosition(),
            groundCheckSize,
            0f,
            groundLayer) != null;
    
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeMax;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        jumpBufferCounter -= Time.fixedDeltaTime;

        Jump();
        Move();
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }
    private Vector2 GetGroundCheckPosition()
    {
        if (groundCheck != null)
        {
            // Debug.Log("Using groundCheck position for ground detection.");
            return groundCheck.position;
        }

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            // Debug.Log("Using player collider bounds for ground detection.");
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
