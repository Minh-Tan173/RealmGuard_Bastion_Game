using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbililtySFX : MonoBehaviour
{
    [SerializeField] private Transform parent;

    private IAbility ability;
    private AudioSource audioSource;

    private void Awake() {

        audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 10f;
        audioSource.maxDistance = 20f;

        ability = parent.GetComponent<IAbility>(); 

        if (ability == null) {
            Debug.LogError("Parent dont inherit IAbility component");
        }

        ability.OnAttackSFX += Ability_OnAttackSFX;   

    }

    private void OnDestroy() {

        ability.OnAttackSFX -= Ability_OnAttackSFX;
    }

    private void Ability_OnAttackSFX(object sender, IAbility.OnAttackSFXEventArgs e) {

        audioSource.PlayOneShot(e.audioClip, SoundManager.Instance.GetSFXVolume());

    }
}
