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

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();

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
                anim.SetTrigger("AttackTrigger");   // only starts the body animation
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    // Called by an Animation Event on the strike frame
    public void DealAttack()
    {
        Vector2 attackPos = (Vector2)transform.position + new Vector2(playerController.FacingDirection * 0.75f, 0f);

        // slash effect
        attackEffectTransform.GetComponent<SpriteRenderer>().flipX = playerController.FacingDirection < 0;
        attackEffectTransform.position = attackPos;
        attackEffectTransform.GetComponent<SpriteRenderer>().enabled = true;
        attackEffectAnimator.Play("Attack1_Effect", 0, 0f);

        // hit detection (same code as before)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPos, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.gameObject == gameObject) continue;

            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 knockbackDir = new Vector2(playerController.FacingDirection, 0f).normalized;
                damageable.TakeDamage(attackDamage, knockbackDir);
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
