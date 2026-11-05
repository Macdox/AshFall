using UnityEngine;

/// <summary>
/// Shared by every enemy. Talks to EnemyBase only, so it works for any enemy type.
/// Max health comes from the enemy's EnemyData asset.
/// </summary>
[RequireComponent(typeof(EnemyBase))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private float knockbackUp = 2f;
    [SerializeField] private float destroyDelay = 15f;

    private EnemyBase enemy;
    private Rigidbody2D rb;
    private int currentHealth;

    public bool IsDead { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => enemy.Data.maxHealth;

    // Fired after every hit that doesn't kill. Boss phase logic can listen to this later.
    public event System.Action<int, int> OnHealthChanged;   // (current, max)

    private void Awake()
    {
        enemy = GetComponent<EnemyBase>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = enemy.Data.maxHealth;
    }

    public void TakeDamage(int damageAmount, Vector2 knockbackDir)
    {
        if (IsDead) return;

        currentHealth -= damageAmount;
        OnHealthChanged?.Invoke(Mathf.Max(currentHealth, 0), MaxHealth);

        if (currentHealth <= 0)
        {
            Die(knockbackDir);
            return;
        }

        // Order matters: Stagger() zeroes horizontal velocity,
        // so knockback has to be applied AFTER it.
        enemy.Stagger();
        ApplyKnockback(knockbackDir, 1f);
    }

    private void Die(Vector2 knockbackDir)
    {
        IsDead = true;
        enemy.Kill();
        Destroy(gameObject, destroyDelay);
    }

    private void ApplyKnockback(Vector2 dir, float multiplier)
    {
        float x = dir.x == 0f ? 0f : Mathf.Sign(dir.x);
        rb.linearVelocity = new Vector2(x * knockbackForce * multiplier, knockbackUp);
    }
}