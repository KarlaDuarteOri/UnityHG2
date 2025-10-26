using UnityEngine;
using Unity.Netcode;

public class ArrowProjectile : NetworkBehaviour
{
    [Header("Arrow Settings")]
    public int damage = 35;
    public float lifetime = 5f;
    
    [Header("Visual Effects")]
    public TrailRenderer trail;
    public ParticleSystem hitEffect;
    
    private Rigidbody rb;
    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rb != null && !hasHit && rb.velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        hasHit = true;
        
        PlayerCombat player = collision.gameObject.GetComponent<PlayerCombat>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
        
        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, transform.position, transform.rotation);
            Destroy(effect.gameObject, 2f);
        }
            
        if (trail != null)
            trail.autodestruct = true;
            
        if (rb != null)
            rb.isKinematic = true;
        
        transform.SetParent(collision.transform);
        Destroy(gameObject, 3f);
    }
}
