using UnityEngine;
using Unity.Netcode;

public class SpearWeapon : NetworkBehaviour
{
    [Header("Spear Settings")]
    public float attackRange = 3f;
    public int damage = 30;
    public float attackCooldown = 1.2f;
    
    [Header("Visual Effects")]
    public TrailRenderer spearTrail;
    
    private float lastAttackTime;
    private bool canAttack = true;

    void Update()
    {
        if (!IsOwner) return;
        
        if (!canAttack && Time.time - lastAttackTime >= attackCooldown)
        {
            canAttack = true;
        }
    }

    public void PerformAttack()
    {
        if (!canAttack || !IsOwner) return;
        
        PerformAttackServerRpc();
    }

    [ServerRpc]
    private void PerformAttackServerRpc()
    {
        PerformAttackClientRpc();
    }

    [ClientRpc]
    private void PerformAttackClientRpc()
    {
        if (spearTrail != null)
            StartCoroutine(EnableTrail());
        
        CheckSpearHit();
        
        lastAttackTime = Time.time;
        canAttack = false;
    }

    private void CheckSpearHit()
    {
        RaycastHit hit;
        Vector3 attackStart = transform.position + Vector3.up * 1f;
        
        if (Physics.Raycast(attackStart, transform.forward, out hit, attackRange))
        {
            PlayerCombat target = hit.collider.GetComponent<PlayerCombat>();
            if (target != null && target.gameObject != gameObject)
            {
                target.TakeDamage(damage);
            }
        }
    }

    private System.Collections.IEnumerator EnableTrail()
    {
        if (spearTrail != null)
        {
            spearTrail.emitting = true;
            yield return new WaitForSeconds(0.4f);
            spearTrail.emitting = false;
        }
    }
}
