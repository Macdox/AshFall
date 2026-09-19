using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackCooldown = 0.5f;

    private float nextAttackTime = 0f;
    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
        // If enemyLayers is not set in inspector, default to hitting everything
        // (Useful for initial testing before proper layers are set up)
        if (enemyLayers.value == 0)
        {
            enemyLayers = ~0; // All layers
        }
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void Attack()
    {
        // Calculate hit position based on facing direction
        Vector2 attackPos = (Vector2)transform.position + new Vector2(playerController.FacingDirection * 0.75f, 0f);
        
        // Use OverlapCircleAll to hit multiple targets in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Prevent hitting ourselves
            if (enemy.gameObject == gameObject) continue;

            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 knockbackDir = new Vector2(playerController.FacingDirection, 0f).normalized;
                damageable.TakeDamage(attackDamage, knockbackDir);
                Debug.Log("Dealt " + attackDamage + " damage to " + enemy.name);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the attack range in the editor
        PlayerController pc = GetComponent<PlayerController>();
        int facing = pc != null ? pc.FacingDirection : 1;
        
        Vector2 attackPos = (Vector2)transform.position + new Vector2(facing * 0.75f, 0f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
}
