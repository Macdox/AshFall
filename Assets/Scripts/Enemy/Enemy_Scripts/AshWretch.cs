using UnityEngine;

/// <summary>
/// Basic melee enemy.
/// Attack logic is handled here.
/// All other behavior comes from EnemyBase.
/// </summary>
public class AshWretch : EnemyBase
{
    [Header("Melee Strike")]

    [SerializeField]
    private Vector2 strikeOffset = new Vector2(0.8f, 0.3f);

    [SerializeField]
    private float strikeRadius = 0.7f;

    private Vector2 GetStrikePosition()
    {
        Vector2 offset = new Vector2(
            FacingSign * strikeOffset.x,
            strikeOffset.y
        );

        return (Vector2)transform.position + offset;
    }

    protected override void PerformStrike()
    {
        if (!IsPlayerCenterInsideStrikeCircle())
            return;

        Vector2 knockbackDir = new Vector2(FacingSign, 0f);

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(
                data.attackDamage,
                knockbackDir
            );
        }
    }
    public bool IsPlayerCenterInsideStrikeCircle()
    {
        if (player == null)
            return false;

        Vector2 strikePos = GetStrikePosition();
        Vector2 playerCenter = player.position;

        return Vector2.Distance(strikePos, playerCenter) <= strikeRadius;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.magenta;

        int facing = Application.isPlaying
            ? FacingSign
            : 1;

        Vector2 offset = new Vector2(
            facing * strikeOffset.x,
            strikeOffset.y
        );

        Vector2 strikePos = (Vector2)transform.position + offset;

        Gizmos.DrawWireSphere(
            strikePos,
            strikeRadius
        );
    }
}