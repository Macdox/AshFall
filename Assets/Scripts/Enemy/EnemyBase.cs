using UnityEngine;

/// <summary>
/// Shared logic for every enemy: references, state dispatcher, patrol, chase,
/// attack flow (windup -> strike -> recovery), hurt, death, return-to-spawn,
/// and Animator parameters. Child classes override only what is different.
/// PerformStrike() is mandatory.
///
/// Animator parameters expected (all optional, enemy works without an Animator):
///   Speed      (float)   horizontal speed, set every frame
///   IsChasing  (bool)    true while in the Chase state
///   Attack     (trigger) start of each attack windup
///   Hurt       (trigger) when staggered
///   Die        (trigger) on death
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected EnemyData data;

    [Header("Patrol (optional, leave empty for enemies that don't patrol)")]
    [SerializeField] protected Transform patrolPointLeft;
    [SerializeField] protected Transform patrolPointRight;

    [Header("Sprite")]
    [SerializeField] protected bool spriteFacesRight = true;

    [Header("Attack Effect")]
    [SerializeField] protected AttackEffect attackEffect;
    [SerializeField] protected Vector2 effectOffset = new Vector2(0.8f, 0f);

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator animator;
    protected Transform player;
    protected PlayerHealth playerHealth;
    protected EnemyState currentState = EnemyState.Patrol;
    protected Transform currentPatrolTarget;
    protected Vector2 startingPosition;

    // +1 = facing right, -1 = facing left
    protected int FacingSign { get; private set; } = 1;

    public EnemyState CurrentState => currentState;
    public EnemyData Data => data;

    protected bool PlayerIsDead => playerHealth != null && playerHealth.IsDead;

    private enum AttackPhase { Windup, Recovery }
    private AttackPhase attackPhase;
    private float attackTimer;
    private float hurtTimer;

    // ------------------------------------------------------------------
    // Setup
    // ------------------------------------------------------------------

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        startingPosition = transform.position;

        if (data == null) Debug.LogError($"{name}: no EnemyData assigned", this);
        if (sr == null) Debug.LogWarning($"{name}: no SpriteRenderer found", this);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();

            if (playerHealth == null)
                Debug.LogError($"{name}: PlayerHealth component not found on Player!", this);
        }
        else
        {
            Debug.LogWarning($"{name}: no object tagged 'Player' found", this);
        }
    }

    protected virtual void Start()
    {
        // Detach so the points don't move with the enemy.
        if (patrolPointLeft != null) patrolPointLeft.SetParent(null);
        if (patrolPointRight != null) patrolPointRight.SetParent(null);

        if (patrolPointLeft != null) currentPatrolTarget = patrolPointLeft;
    }

    protected virtual void OnDestroy()
    {
        // The detached patrol points would otherwise be left behind in the scene.
        if (patrolPointLeft != null) Destroy(patrolPointLeft.gameObject);
        if (patrolPointRight != null) Destroy(patrolPointRight.gameObject);
    }

    protected virtual void Update()
    {
        // Walking, patrolling, chasing and returning all animate from this one value.
        if (animator != null && rb != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    protected float DistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector2.Distance(transform.position, player.position);
    }

    protected void FaceDirection(float dirX)
    {
        if (Mathf.Approximately(dirX, 0f)) return;

        FacingSign = dirX > 0f ? 1 : -1;
        if (sr != null)
        {
            sr.flipX = spriteFacesRight ? FacingSign < 0 : FacingSign > 0;
        }
    }

    protected void FacePlayer()
    {
        if (player == null) return;
        FaceDirection(player.position.x - transform.position.x);
    }

    protected void StopHorizontal()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    /// <summary>
    /// Is the player close enough to attack? Default is a plain distance check.
    /// Children can override (AshWretch uses its strike circle).
    /// </summary>
    protected virtual bool PlayerInAttackRange()
    {
        return DistanceToPlayer() <= data.attackRange;
    }

    // ------------------------------------------------------------------
    // State control. Everything changes state through these, never by
    // writing currentState directly.
    // ------------------------------------------------------------------

    public void ChangeState(EnemyState newState)
    {
        if (currentState == EnemyState.Die || currentState == newState) return;

        currentState = newState;
        StopHorizontal();   // no state inherits the previous state's velocity

        if (animator != null)
        {
            animator.SetBool("IsChasing", newState == EnemyState.Chase);
        }

        if (newState == EnemyState.Attack)
        {
            BeginAttackWindup();
        }
    }

    /// <summary>
    /// Called by EnemyHealth when hit. Call this BEFORE applying knockback,
    /// because ChangeState zeroes horizontal velocity.
    /// </summary>
    public void Stagger()
    {
        if (currentState == EnemyState.Die) return;

        hurtTimer = data.hurtDuration;   // refreshed even if already hurt
        ChangeState(EnemyState.Hurt);

        if (animator != null)
        {
            animator.ResetTrigger("Attack");   // a swing that never landed shouldn't replay after the flinch
            animator.SetTrigger("Hurt");
        }
    }

    public void Kill()
    {
        if (currentState == EnemyState.Die) return;

        ChangeState(EnemyState.Die);

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Die");
        }
        if (attackEffect != null) attackEffect.Hide();   // a swing effect shouldn't linger on a corpse

        // The corpse shouldn't block or push the player.
        if (player != null)
        {
            Collider2D[] playerCols = player.GetComponents<Collider2D>();
            foreach (Collider2D mine in GetComponents<Collider2D>())
                foreach (Collider2D theirs in playerCols)
                    Physics2D.IgnoreCollision(mine, theirs, true);
        }
    }

    // ------------------------------------------------------------------
    // Dispatcher
    // ------------------------------------------------------------------

    protected virtual void FixedUpdate()
    {
        if (data == null) return;

        // Player is dead: walk back to the spawn position and stand there.
        if (PlayerIsDead && currentState != EnemyState.Die)
        {
            if (currentState == EnemyState.Idle)
            {
                StopHorizontal();
                return;
            }

            if (currentState != EnemyState.Return)
            {
                ChangeState(EnemyState.Return);
            }

            ReturnToStartingPosition();
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrol: OnPatrol(); break;
            case EnemyState.Chase: OnChase(); break;
            case EnemyState.Attack: OnAttack(); break;
            case EnemyState.Hurt: OnHurt(); break;
            case EnemyState.Return: ReturnToStartingPosition(); break;

            case EnemyState.Idle:
            case EnemyState.Die:
                break;
        }
    }


    // ------------------------------------------------------------------
    // Default state behaviours (override in children when needed)
    // ------------------------------------------------------------------

    protected virtual void OnPatrol()
    {
        bool canPatrol = patrolPointLeft != null && patrolPointRight != null && currentPatrolTarget != null;

        if (canPatrol)
        {
            // Flip target first, then compute direction (order matters).
            if (Mathf.Abs(transform.position.x - currentPatrolTarget.position.x) < 0.1f)
            {
                currentPatrolTarget = currentPatrolTarget == patrolPointLeft ? patrolPointRight : patrolPointLeft;
            }

            float dir = Mathf.Sign(currentPatrolTarget.position.x - transform.position.x);
            FaceDirection(dir);
            rb.linearVelocity = new Vector2(dir * data.patrolSpeed, rb.linearVelocity.y);
        }
        else
        {
            StopHorizontal();
        }

        // Detection is outside the patrol check so enemies without patrol points still notice you.
        if (DistanceToPlayer() <= data.detectionRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    protected virtual void OnChase()
    {
        FacePlayer();

        // 1. Close enough: attack.
        if (PlayerInAttackRange())
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // 2. Player got away: give up and go back to patrol.
        if (DistanceToPlayer() > data.loseInterestRange)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        // 3. Otherwise keep chasing.
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * data.chaseSpeed, rb.linearVelocity.y);
    }

    protected virtual void OnAttack()
    {
        StopHorizontal();   // plant the feet
        attackTimer -= Time.fixedDeltaTime;

        if (attackPhase == AttackPhase.Windup)
        {
            FacePlayer();   // track the player during windup only

            if (attackTimer <= 0f)
            {
                PerformStrike();
                PlayAttackEffect();
                attackPhase = AttackPhase.Recovery;
                attackTimer = data.attackRecovery;
            }
        }
        else // Recovery
        {
            if (attackTimer <= 0f)
            {
                if (PlayerInAttackRange())
                    BeginAttackWindup();             // player still close: swing again
                else
                    ChangeState(EnemyState.Chase);
            }
        }
    }

    protected virtual void OnHurt()
    {
        hurtTimer -= Time.fixedDeltaTime;
        if (hurtTimer > 0f) return;

        ChangeState(DistanceToPlayer() <= data.loseInterestRange ? EnemyState.Chase : EnemyState.Patrol);
    }

    protected virtual void ReturnToStartingPosition()
    {
        float distanceX = startingPosition.x - rb.position.x;

        // Reached the original horizontal position.
        if (Mathf.Abs(distanceX) <= 0.1f)
        {
            rb.position = new Vector2(startingPosition.x, rb.position.y);
            StopHorizontal();
            ChangeState(PlayerIsDead ? EnemyState.Idle : EnemyState.Patrol);
            return;
        }

        float direction = distanceX > 0f ? 1f : -1f;
        FaceDirection(direction);

        // Don't overshoot the starting position.
        float returnSpeed = Mathf.Min(data.chaseSpeed, Mathf.Abs(distanceX) / Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(direction * returnSpeed, rb.linearVelocity.y);
    }

    /// <summary>The one thing every enemy must define: what its attack actually does.</summary>
    protected abstract void PerformStrike();

    private void BeginAttackWindup()
    {
        attackPhase = AttackPhase.Windup;
        attackTimer = data.attackWindup;

        if (animator != null) animator.SetTrigger("Attack");
    }

    protected virtual Vector2 GetEffectPosition()
    {
        return (Vector2)transform.position + new Vector2(FacingSign * effectOffset.x, effectOffset.y);
    }

    protected virtual void PlayAttackEffect()
    {
        if (attackEffect != null)
        {
            attackEffect.Play(GetEffectPosition(), FacingSign);
        }
    }
    // ------------------------------------------------------------------
    // Debug
    // ------------------------------------------------------------------

    protected virtual void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.detectionRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, data.loseInterestRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}