using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    [SerializeField] public float MaxHealth = 1000;
    public float Health;

    static private AudioSource audioSource = null;

    private void Awake()
    {
        Health = MaxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    public void ChangeHealth( float value )
    {
        Health = Health + value;
        if( Health <= 0 ) Health = 0;
        if ( !audioSource.isPlaying ) audioSource.Play();
    }

}
