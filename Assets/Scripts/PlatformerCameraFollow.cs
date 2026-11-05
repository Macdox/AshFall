
using UnityEngine;

public class PlatformerCameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Header("Camera Movement")]
    [SerializeField] private float minHorizontalTravel = 1.0f;
    [SerializeField] private float minVerticalTravel = 0.6f;

    private float jumpStartX;
    private float jumpStartY;

    private bool wasGrounded;
    private bool qualifiedJump;

    private void Start()
    {
        if (player == null || groundCheck == null || playerRb == null)
        {
            Debug.LogError("Camera follow references are missing!", this);
            enabled = false;
            return;
        }

        // Start at the player's position.
        transform.position = player.position;

        wasGrounded = IsGrounded();
        jumpStartX = player.position.x;
        jumpStartY = player.position.y;
    }

    private void LateUpdate()
    {
        bool grounded = IsGrounded();

        // Detect the beginning of a jump.
        if (wasGrounded && !grounded)
        {
            jumpStartX = player.position.x;
            jumpStartY = player.position.y;
            qualifiedJump = false;
        }

        // Check whether this jump involves meaningful horizontal
        // and vertical movement.
        if (!grounded && !qualifiedJump)
        {
            float horizontalTravel =
                Mathf.Abs(player.position.x - jumpStartX);

            float verticalTravel =
                Mathf.Abs(player.position.y - jumpStartY);

            if (horizontalTravel >= minHorizontalTravel &&
                verticalTravel >= minVerticalTravel)
            {
                qualifiedJump = true;
            }
        }

        Vector3 targetPosition = transform.position;

        // Always follow horizontally.
        targetPosition.x = player.position.x;

        // Follow vertically only for qualified jumps.
        if (qualifiedJump)
        {
            targetPosition.y = player.position.y;
        }

        // When landing, retain the new height if this was
        // a qualified jump.
        if (!wasGrounded && grounded)
        {
            if (qualifiedJump)
            {
                targetPosition.y = player.position.y;
            }

            qualifiedJump = false;
        }

        transform.position = targetPosition;

        wasGrounded = grounded;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayers
        );
    }
}
