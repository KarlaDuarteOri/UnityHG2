using UnityEngine;

public class AnimationEventManager : MonoBehaviour
{
    [Header("Weapon References")]
    public SwordWeapon swordWeapon;
    public BowWeapon bowWeapon;
    public SpearWeapon spearWeapon;
    
    [Header("Sound Effects")]
    public AudioSource weaponAudio;
    public AudioClip swordSwingSound;
    public AudioClip bowDrawSound;
    public AudioClip arrowReleaseSound;
    public AudioClip spearThrustSound;

    public void OnSwordAttack()
    {
        if (swordWeapon != null)
            swordWeapon.PerformAttack();
            
        PlaySound(swordSwingSound);
    }

    public void OnSpearAttack()
    {
        if (spearWeapon != null)
            spearWeapon.PerformAttack();
            
        PlaySound(spearThrustSound);
    }

    public void OnBowDraw()
    {
        PlaySound(bowDrawSound);
    }

    public void OnBowRelease()
    {
        PlaySound(arrowReleaseSound);
    }

    public void OnAttackEnd()
    {
        // Resetear estados si es necesario
    }

    private void PlaySound(AudioClip clip)
    {
        if (weaponAudio != null && clip != null)
        {
            weaponAudio.PlayOneShot(clip);
        }
    }
}
