using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct GroundFootsteps
{
    [SerializeField] public string groundType;
    [SerializeField] public AudioClip[] footstepsOnGroundType;
}

[RequireComponent(typeof(AudioSource))]
public class PlayerSounds : MonoBehaviour
{
    AudioSource source;
    [SerializeField, Range(0.1f, 1f)]
    float footstepVol = 1, jumpVol = 1, throwVol = 1, pickupVol = 1;
    [SerializeField] GroundFootsteps[] groundFootsteps;
    [SerializeField] AudioClip[] jumpSounds;
    [SerializeField] AudioClip[] throwSounds;
    [SerializeField] AudioClip[] pickupSounds;
    //[SerializeField] AudioClip[] fallSounds;
    //[SerializeField] AudioClip[] groundShaking;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayFootstepOnGroundType(string groundType)
    {
        for (int i = 0; i < groundFootsteps.Length; i++)
        {
            var gType = groundFootsteps[i];
            if (gType.groundType == groundType)
            {
                PlayRandomSound(gType.footstepsOnGroundType, footstepVol);
                break;
            }
            if (i == groundFootsteps.Length - 1) //supplied groundType was not found
                Debug.LogError("Given GroundType was not found in groundFootsteps array. Check spelling and make sure type exists.", this);
        }
    }

    public void PlayPickup()
    {
        PlayRandomSound(pickupSounds, pickupVol);
    }

    public void PlayJump()
    {
        PlayRandomSound(jumpSounds, jumpVol);
    }

    public void PlayThrow()
    {
        PlayRandomSound(throwSounds, throwVol);
    }

    void PlayRandomSound(AudioClip[] sounds, float volume)
    {
        if (sounds.Length == 0)
            Debug.LogError("Sound-list is empty.", this);
        int rand = Random.Range(0, sounds.Length);
        source.PlayOneShot(sounds[rand], volume);
    }

}