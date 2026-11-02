using UnityEngine;

public class AttackEffectVisual : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    public void Hide()
    {
        sr.enabled = false;
    }
}