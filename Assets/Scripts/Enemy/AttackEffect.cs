using UnityEngine;

/// <summary>
/// Small slash/impact effect that sits on a child object of an enemy.
/// Needs on the same GameObject: SpriteRenderer + Animator (with the effect clip).
/// EnemyBase calls Play() at the moment the strike lands.
/// Each enemy type gets its own child with its own sprite/clip; the script is shared.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class AttackEffect : MonoBehaviour
{
    [Tooltip("Exact name of the effect state in this object's Animator")]
    [SerializeField] private string stateName = "Slash";

    [Tooltip("Seconds until the effect hides itself. Set to the clip length. 0 = hide via an Animation Event calling Hide() instead.")]
    [SerializeField] private float duration = 0.3f;

    [SerializeField] private bool artFacesRight = true;

    private SpriteRenderer sr;
    private Animator anim;
    private float hideAt;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        sr.enabled = false;   // invisible until the first strike, even if the Animator auto-plays
    }

    public void Play(Vector2 worldPosition, int facingSign)
    {
        transform.position = worldPosition;
        sr.flipX = artFacesRight ? facingSign < 0 : facingSign > 0;
        sr.enabled = true;
        anim.Play(stateName, 0, 0f);   // restart from frame 0 every time
        hideAt = Time.time + duration;
    }

    // Can also be called from an Animation Event on the clip's last frame.
    public void Hide()
    {
        sr.enabled = false;
    }

    private void Update()
    {
        if (sr.enabled && duration > 0f && Time.time >= hideAt)
        {
            sr.enabled = false;
        }
    }
}