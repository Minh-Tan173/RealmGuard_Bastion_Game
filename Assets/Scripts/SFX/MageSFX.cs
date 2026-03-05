using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageSFX : MonoBehaviour
{
    private AudioSource audioSource;
    private Mage mage;

    private AudioClipRefsSO audioClipRefsSO;

    private void Awake() {

        audioSource = GetComponent<AudioSource>();
        mage = GetComponentInParent<Mage>();

        // 1. Setup audio source
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 10f;
        audioSource.maxDistance = 35f;

        // 2. Setup volume
        audioClipRefsSO = SoundManager.Instance.GetAudioClipRefsSO();

        mage.OnSpawnSFX += Mage_OnSpawnSFX;
        mage.OnAttackSFX += Mage_OnAttackSFX;
    }

    private void OnDestroy() {

        mage.OnSpawnSFX -= Mage_OnSpawnSFX;
        mage.OnAttackSFX -= Mage_OnAttackSFX;
    }

    private void Mage_OnAttackSFX(object sender, System.EventArgs e) {

        audioSource.PlayOneShot(audioClipRefsSO.mageAttackSFX, SoundManager.Instance.GetSFXVolume());
    }

    private void Mage_OnSpawnSFX(object sender, System.EventArgs e) {

        AudioClip spawnAudio = audioClipRefsSO.mageSpawnSFX[Random.Range(0, audioClipRefsSO.mageSpawnSFX.Length)];
        audioSource.PlayOneShot(spawnAudio, SoundManager.Instance.GetSFXVolume());
    }
}
