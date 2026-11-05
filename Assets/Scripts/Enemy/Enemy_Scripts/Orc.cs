using UnityEngine;

/// <summary>
/// Basic melee enemy. Patrol, chase, windup, recovery, hurt, death and animation
/// all come from EnemyBase. This class only defines its strike circle and what the strike does.
/// </summary>
public class Orc : EnemyBase
{
    [Header("Melee Strike")]
    [SerializeField] private float strikeOffset = 0.8f;   // how far in front of the enemy the strike circle sits
    [SerializeField] private float strikeRadius = 0.7f;


    protected override Vector2 GetEffectPosition() => StrikeCenter;
    private Vector2 StrikeCenter =>
        (Vector2)transform.position + new Vector2(FacingSign * strikeOffset, 0f);

    public bool IsPlayerCenterInsideStrikeCircle()
    {
        if (player == null) return false;
        return Vector2.Distance(StrikeCenter, player.position) <= strikeRadius;
    }

    // The base class asks this question to decide when to attack.
    protected override bool PlayerInAttackRange()
    {
        return IsPlayerCenterInsideStrikeCircle();
    }

    protected override void PerformStrike()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(StrikeCenter, strikeRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;   // never hurt other enemies

            IDamageable target = hit.GetComponent<IDamageable>();
            if (target == null) continue;

            target.TakeDamage(data.attackDamage, new Vector2(FacingSign, 0f));
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        int facing = Application.isPlaying ? FacingSign : 1;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + new Vector3(facing * strikeOffset, 0f, 0f), strikeRadius);
    }
}