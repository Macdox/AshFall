using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private float horizontal;

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
    }
    void FixedUpdate()
    {
        
        Vector3 movement = new Vector3(horizontal, 0f, 0f);
        transform.Translate(movement * speed * Time.fixedDeltaTime);
    }
}
