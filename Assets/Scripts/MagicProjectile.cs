
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MagicProjectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject impactVfxPrefab;

    [Header("Projectile")]
    [SerializeField] private float lifetime = 5f;

    [Tooltip("Layers that should destroy the projectile, such as Ground.")]
    [SerializeField] private LayerMask destroyOnImpactLayers;

    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private ParticleSystem[] projectileParticles;

    private int damage;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();

        projectileCollider.isTrigger = true;
        projectileCollider.enabled = false;

        // Get all particle systems, including inactive children.
        projectileParticles = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in projectileParticles)
        {
            var main = ps.main;
            main.playOnAwake = false;

            // Keep all effects stopped until the projectile is fired.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void Launch(Vector2 direction, float speed, int damageAmount)
    {
        damage = damageAmount;

        // Enable collision only when the projectile is fired.
        projectileCollider.enabled = true;

        // Start projectile movement.
        rb.linearVelocity = direction.normalized * speed;

        // Activate all projectile particle effects.
        foreach (ParticleSystem ps in projectileParticles)
        {
            ps.Play(false);
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        // Check whether we hit the player.
        PlayerHealth target =
            other.GetComponentInParent<PlayerHealth>();

        if (target != null)
        {
            if (!target.IsDead)
            {
                Vector2 knockbackDirection = new Vector2(
                    Mathf.Sign(rb.linearVelocity.x),
                    0f
                );

                target.TakeDamage(
                    damage,
                    knockbackDirection
                );
            }

            ImpactAndDestroy();
            return;
        }

        // Destroy when hitting selected environment layers.
        int otherLayer = 1 << other.gameObject.layer;

        if ((destroyOnImpactLayers.value & otherLayer) != 0)
        {
            ImpactAndDestroy();
        }
    }


    private void ImpactAndDestroy()
    {
        if (hasHit)
            return;

        hasHit = true;

        if (projectileCollider != null)
            projectileCollider.enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (impactVfxPrefab != null)
        {
            GameObject impact = Instantiate(
                impactVfxPrefab,
                transform.position,
                Quaternion.identity
            );

            ParticleSystem[] systems =
                impact.GetComponentsInChildren<ParticleSystem>(true);

            float destroyAfter = 1f;

            // Prepare all particle systems.
            foreach (ParticleSystem ps in systems)
            {
                var main = ps.main;
                main.playOnAwake = false;

                ps.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );

                float remainingTime =
                    main.duration + main.startLifetime.constantMax;

                destroyAfter = Mathf.Max(
                    destroyAfter,
                    remainingTime
                );
            }

            // Play only when the impact happens.
            foreach (ParticleSystem ps in systems)
            {
                ps.Play(false);
            }

            // Destroy after the effects finish.
            Destroy(impact, destroyAfter);
        }

        Destroy(gameObject);
    }
}
