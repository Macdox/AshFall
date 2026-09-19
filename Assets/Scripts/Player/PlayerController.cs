using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.2f);

    
    [SerializeField] private float coyoteTimeMax = 0.15f;
    [SerializeField] private float jumpBufferMax = 0.1f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 1f;

    [SerializeField]private float dashTimer;
    [SerializeField]private float dashCooldownTimer;
    
    
    


    private Rigidbody2D rb;




    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private float horizontal;
    private int facingDirection = 1;
    public int FacingDirection => facingDirection;
    private int dashDirection = 1;
    private float defaultGravity;
    
    private bool isGrounded;
    private bool isDashing;
    private bool hasAirDashed;
    private PlayerHealth playerHealth;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        defaultGravity = rb.gravityScale;

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
        if (horizontal != 0)
        {
            facingDirection = (int)Mathf.Sign(horizontal);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferMax;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f && (isGrounded || !hasAirDashed))
        {
            if (!isGrounded)
            {
                hasAirDashed = true;
            }

            isDashing = true; 
            playerHealth?.SetExternalInvincibility(true);
            dashDirection = facingDirection;
            dashTimer = dashDuration; 
            dashCooldownTimer = dashCooldown;
            rb.gravityScale = 0f;
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
            hasAirDashed = false;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        jumpBufferCounter -= Time.fixedDeltaTime;

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.fixedDeltaTime;
        }

        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            
            if (dashTimer <= 0f)
            {
                isDashing = false;
                playerHealth?.SetExternalInvincibility(false);
                rb.gravityScale = defaultGravity;
            }
        }
        else
        {
           
            if (playerHealth == null || !playerHealth.IsKnockedBack)
            {
                Jump();
                Move();
            }
        }
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
