using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // Start is called before the first frame update
    public AudioSource audioSource;
    public AudioClip hitSound, hurtSound, levelUpSound;
   public void HitAudio()
    {
        audioSource.clip = hitSound;
        audioSource.Play();
    }

    public void HurtAudio()
    {
        audioSource.clip = hurtSound;
        audioSource.Play();
    }
    public void LevelUpSoundAudio()
    {
        audioSource.clip = levelUpSound;
        audioSource.Play();
    }
}
