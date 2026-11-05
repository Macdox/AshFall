using UnityEngine;

/// <summary>
/// One asset per enemy type (Create > Ashfall > Enemy Data).
/// Read-only at runtime: never write to these fields from game code,
/// the asset is shared by every enemy that uses it.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Ashfall/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 40;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Awareness")]
    public float detectionRange = 5f;
    public float loseInterestRange = 8f;   // keep noticeably larger than detectionRange

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackWindup = 0.3f;      // telegraph time before the strike lands
    public float attackRecovery = 0.5f;    // pause after the strike
    public int attackDamage = 10;

    [Header("Hurt")]
    public float hurtDuration = 0.25f;
}