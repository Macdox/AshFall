using UnityEngine;

public class TestTarget : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int contactDamage = 10;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount, Vector2 knockbackDir)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage! Health remaining: {currentHealth}");
        Debug.Log($"Received knockback in direction: {knockbackDir}");

        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} was destroyed!");
            Destroy(gameObject);
        }
    }
     private void OnCollisionEnter2D(Collision2D collision)
    {
        // Find an IDamageable on whatever collided with us (like the Player)
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Calculate knockback pushing them away from us
            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
            
            // Optional: zero out the Y axis for purely horizontal knockback
            knockbackDir.y = 0;
            knockbackDir = knockbackDir.normalized;
            damageable.TakeDamage(contactDamage, knockbackDir);
        }
    }


}
