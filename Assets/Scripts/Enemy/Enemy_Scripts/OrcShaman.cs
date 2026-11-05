
using UnityEngine;

public class OrcShaman : EnemyBase
{
    [Header("Magic Projectile")]
    [SerializeField] private MagicProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private int projectileDamage = 15;

    [Header("Ground Slam")]
    [SerializeField] private Vector2 slamOffset = new Vector2(1f, 0f);
    [SerializeField] private float slamRadius = 2f;
    [SerializeField] private int slamDamage = 20;

    private bool useProjectileNext = true;

    protected override void PerformStrike()
    {
        if (useProjectileNext)
        {
            CastProjectile();
        }
        else
        {
            PerformGroundSlam();
        }

        // Alternate only after each attack.
        useProjectileNext = !useProjectileNext;
    }

    private void CastProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogError(name + ": Projectile references missing!", this);
            return;
        }

        Vector2 direction = new Vector2(FacingSign, 0f);

        MagicProjectile projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        projectile.Launch(
            direction,
            projectileSpeed,
            projectileDamage
        );
    }

    private void PerformGroundSlam()
    {
        Vector2 slamPosition = (Vector2)transform.position +
            new Vector2(FacingSign * slamOffset.x, slamOffset.y);

        // Damage only if the player's center is inside the slam radius.
        if (playerHealth == null || playerHealth.IsDead)
            return;

        float distance = Vector2.Distance(
            slamPosition,
            player.position
        );

        if (distance <= slamRadius)
        {
            Vector2 knockbackDirection = new Vector2(
                player.position.x >= slamPosition.x ? 1f : -1f,
                0f
            );

            playerHealth.TakeDamage(
                slamDamage,
                knockbackDirection
            );
        }
    }

    // This enemy has its own projectile and slam effects.
    // Don't play the regular sword/axe slash effect.
    protected override void PlayAttackEffect()
    {
        // Intentionally empty.
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.magenta;

        int facing = Application.isPlaying ? FacingSign : 1;

        Vector2 slamPosition = (Vector2)transform.position +
            new Vector2(facing * slamOffset.x, slamOffset.y);

        Gizmos.DrawWireSphere(slamPosition, slamRadius);
    }
}
