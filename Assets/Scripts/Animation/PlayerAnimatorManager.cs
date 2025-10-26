using UnityEngine;
using Unity.Netcode;

public class PlayerAnimatorManager : NetworkBehaviour
{
    [Header("Animator References")]
    public Animator armsAnimator;
    public AnimationEventManager animationEvents;
    
    private readonly int weaponTypeHash = Animator.StringToHash("WeaponType");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int drawBowHash = Animator.StringToHash("DrawBow");
    private readonly int releaseBowHash = Animator.StringToHash("ReleaseBow");
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");

    [Header("Animation Settings")]
    public float attackAnimationSpeed = 1f;
    
    private WeaponType currentWeapon = WeaponType.None;
    private bool isAttacking = false;

    void Start()
    {
        if (armsAnimator != null)
        {
            armsAnimator.SetFloat("Speed", attackAnimationSpeed);
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        UpdateMovementAnimation();
    }

    public void SetWeaponType(WeaponType weaponType)
    {
        if (!IsOwner) return;
        
        SetWeaponTypeServerRpc((int)weaponType);
    }

    [ServerRpc]
    private void SetWeaponTypeServerRpc(int weaponTypeInt)
    {
        SetWeaponTypeClientRpc(weaponTypeInt);
    }

    [ClientRpc]
    private void SetWeaponTypeClientRpc(int weaponTypeInt)
    {
        currentWeapon = (WeaponType)weaponTypeInt;
        
        if (armsAnimator != null)
        {
            armsAnimator.SetInteger(weaponTypeHash, weaponTypeInt);
        }
    }

    public void TriggerAttack()
    {
        if (!IsOwner || isAttacking) return;
        
        TriggerAttackServerRpc();
    }

    [ServerRpc]
    private void TriggerAttackServerRpc()
    {
        TriggerAttackClientRpc();
    }

    [ClientRpc]
    private void TriggerAttackClientRpc()
    {
        if (armsAnimator != null)
        {
            armsAnimator.SetTrigger(attackHash);
            isAttacking = true;
            Invoke("ResetAttackState", 0.5f);
        }
    }

    public void TriggerDrawBow()
    {
        if (!IsOwner) return;
        
        TriggerDrawBowServerRpc();
    }

    [ServerRpc]
    private void TriggerDrawBowServerRpc()
    {
        TriggerDrawBowClientRpc();
    }

    [ClientRpc]
    private void TriggerDrawBowClientRpc()
    {
        if (armsAnimator != null && currentWeapon == WeaponType.Bow)
        {
            armsAnimator.SetTrigger(drawBowHash);
        }
    }

    public void TriggerReleaseBow()
    {
        if (!IsOwner) return;
        
        TriggerReleaseBowServerRpc();
    }

    [ServerRpc]
    private void TriggerReleaseBowServerRpc()
    {
        TriggerReleaseBowClientRpc();
    }

    [ClientRpc]
    private void TriggerReleaseBowClientRpc()
    {
        if (armsAnimator != null && currentWeapon == WeaponType.Bow)
        {
            armsAnimator.SetTrigger(releaseBowHash);
        }
    }

    private void UpdateMovementAnimation()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                       Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        
        if (armsAnimator != null)
        {
            armsAnimator.SetBool(isMovingHash, isMoving);
        }
    }

    private void ResetAttackState()
    {
        isAttacking = false;
    }

    public void OnAnimationEvent(string eventName)
    {
        switch (eventName)
        {
            case "SwordAttack":
                if (animationEvents != null)
                    animationEvents.OnSwordAttack();
                break;
                
            case "SpearAttack":
                if (animationEvents != null)
                    animationEvents.OnSpearAttack();
                break;
                
            case "BowDraw":
                if (animationEvents != null)
                    animationEvents.OnBowDraw();
                break;
                
            case "BowRelease":
                if (animationEvents != null)
                    animationEvents.OnBowRelease();
                break;
                
            case "AttackEnd":
                if (animationEvents != null)
                    animationEvents.OnAttackEnd();
                break;
        }
    }
    
    public WeaponType GetCurrentWeapon() => currentWeapon;
}
