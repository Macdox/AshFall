using UnityEngine;

public class AshWretch : MonoBehaviour
{
    [SerializeField] private Transform patrolPointLeft;
    [SerializeField] private Transform patrolPointRight;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;

    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float loseInterestRange = 8f;

    private EnemyState currentState = EnemyState.Patrol;

    private Transform currentPatrolTarget;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Auto-find player if not assigned in inspector
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void Start()
    {
        // Detach patrol points so they don't move with the enemy
        if (patrolPointLeft != null) patrolPointLeft.SetParent(null);
        if (patrolPointRight != null) patrolPointRight.SetParent(null);

        currentPatrolTarget = patrolPointLeft;
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                break;
            case EnemyState.Chase:
                ChaseBehavior();
                break;
        }
    }

    private void PatrolBehavior()
    {
        if (currentPatrolTarget == null) return;

        if (Mathf.Abs(transform.position.x - currentPatrolTarget.position.x) < 0.1f)
        {
            currentPatrolTarget = currentPatrolTarget == patrolPointLeft ? patrolPointRight : patrolPointLeft;
        }

        float direction = Mathf.Sign(currentPatrolTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);

        if (CanSeePlayer())
        {
            currentState = EnemyState.Chase;
        }
    }

    private void ChaseBehavior()
    {
        if (player == null) return;

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        if (Vector2.Distance(transform.position, player.position) > loseInterestRange)
        {
            currentState = EnemyState.Patrol;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }
}
