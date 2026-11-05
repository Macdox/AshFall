using System;
using UnityEngine;


public class PlayerHealth : MonoBehaviour,IDamageable
{
 

    private float maxHealth = 100f;
    private float currentHealth{ get; set; }
    private bool isInvincible;
    private bool isDashInvincible;
    public bool IsDead { get; private set; }
    private float i_frameTime = 0.5f;
    private float i_frameTimer;
    public bool IsKnockedBack { get; private set; }
    private float knockbackDuration = 0.5f;
    private float knockbackTimer;

    [SerializeField] private Animator anim;
    private Collider2D playerCollider;



    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        currentHealth = maxHealth;
    }
    
    private void Update()
    {
        if (i_frameTimer > 0)
        {
            i_frameTimer -= Time.deltaTime;
        }
        else
        {
            isInvincible = false;
        }

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
        }
        else
        {
            IsKnockedBack = false;
        }
    }


    public void SetExternalInvincibility(bool value)
    {
        isDashInvincible = value;
    }

    public void TakeDamage(int damageAmount, Vector2 knockbackDir)
    {
        Debug.Log("Player TakeDamage called!");

        if (isInvincible || isDashInvincible)
        {
            Debug.Log("Damage ignored due to invincibility.");
            return;
        }

        currentHealth -= damageAmount;

        isInvincible = true;
        i_frameTimer = i_frameTime;

        rb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);

        IsKnockedBack = true;
        knockbackTimer = knockbackDuration;

        Debug.Log($"Player damaged. Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        if (IsDead) return;

        // Mark the player as dead first.
        IsDead = true;

        // Stop the player.
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Play death animation.
        if (anim != null)
        {
            anim.SetTrigger("DeadTrigger");
        }

        // Disable the player's collider.
        // This avoids accessing a missing BoxCollider2D.
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        Debug.Log("Player has died.");
    }
}
