using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackCooldown = 0.5f;


    [SerializeField] private Animator attackEffectAnimator;
    [SerializeField] private Transform attackEffectTransform;

    [SerializeField] private Animator anim;

    private float nextAttackTime = 0f;

    private PlayerController playerController;
    private PlayerHealth playerHealth;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();

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
            if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0) && !playerHealth.IsDead)
            {
                anim.SetTrigger("AttackTrigger");   // only starts the body animation
                nextAttackTime = Time.time + attackCooldown;
            }
            return;
        }
    }

    // Called by an Animation Event on the strike frame
    public void DealAttack()
    {
        if (playerHealth == null || playerHealth.IsDead)
            return;

        Vector2 attackPos = (Vector2)transform.position +
            new Vector2(playerController.FacingDirection * 1.2f, 0f);

        // Play slash effect.
        if (attackEffectTransform != null)
        {
            attackEffectTransform.position = attackPos;

            SpriteRenderer effectRenderer =
                attackEffectTransform.GetComponent<SpriteRenderer>();

            if (effectRenderer != null)
            {
                effectRenderer.flipX =
                    playerController.FacingDirection < 0;

                effectRenderer.enabled = true;
            }
        }

        if (attackEffectAnimator != null)
        {
            attackEffectAnimator.Play("Attack1_Effect", 0, 0f);
        }

        // Detect enemy colliders.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPos,
            attackRange,
            enemyLayers
        );

        Debug.Log("Attack detected colliders: " + hitEnemies.Length);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Ignore the player's own colliders.
            if (enemy.transform.root == transform.root)
                continue;

            // Look for IDamageable on the collider or its parent.
            IDamageable damageable =
                enemy.GetComponentInParent<IDamageable>();

            if (damageable == null)
            {
                Debug.LogWarning(
                    "No IDamageable component found on: " + enemy.name
                );
                continue;
            }

            Vector2 knockbackDir = new Vector2(
                playerController.FacingDirection,
                0f
            );

            damageable.TakeDamage(attackDamage, knockbackDir);

            Debug.Log(
                "Player dealt " + attackDamage +
                " damage to " + enemy.name
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the attack range in the editor
        PlayerController pc = GetComponent<PlayerController>();
        int facing = pc != null ? pc.FacingDirection : 1;
        
        Vector2 attackPos = (Vector2)transform.position + new Vector2(facing * 1.2f, 0f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
}
