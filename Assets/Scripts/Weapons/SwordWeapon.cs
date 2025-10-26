using UnityEngine;
using Unity.Netcode;

public class SwordWeapon : NetworkBehaviour
{
    [Header("Sword Settings")]
    public float attackRange = 2f;
    public int damage = 25;
    public float attackCooldown = 1f;
    
    [Header("Visual Effects")]
    public TrailRenderer swordTrail;
    
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
        if (swordTrail != null)
            StartCoroutine(EnableTrail());
        
        CheckSwordHit();
        
        lastAttackTime = Time.time;
        canAttack = false;
    }

    private void CheckSwordHit()
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
        if (swordTrail != null)
        {
            swordTrail.emitting = true;
            yield return new WaitForSeconds(0.3f);
            swordTrail.emitting = false;
        }
    }
}
