using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherSFX : MonoBehaviour
{
    private AudioSource audioSource;
    private Archer archer;

    private AudioClipRefsSO audioClipRefsSO;

    private void Awake() {

        audioSource = GetComponent<AudioSource>();
        archer = GetComponentInParent<Archer>();

        // 1. Setup audio source
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 10f;
        audioSource.maxDistance = 35f;

        // 2. Setup volume
        audioClipRefsSO = SoundManager.Instance.GetAudioClipRefsSO();

        archer.OnAttackSFX += Archer_OnAttackSFX;
        archer.OnSpawnSFX += Archer_OnSpawnSFX;
    }

    private void OnDestroy() {

        archer.OnAttackSFX -= Archer_OnAttackSFX;
        archer.OnSpawnSFX -= Archer_OnSpawnSFX;
    }

    private void Archer_OnAttackSFX(object sender, System.EventArgs e) {

        audioSource.PlayOneShot(audioClipRefsSO.archerAttackSFX, SoundManager.Instance.GetSFXVolume());
    }

    private void Archer_OnSpawnSFX(object sender, System.EventArgs e) {

        AudioClip spawnAudio = audioClipRefsSO.archerSpawnSFX[UnityEngine.Random.Range(0, audioClipRefsSO.archerSpawnSFX.Length)];
        audioSource.PlayOneShot(spawnAudio, SoundManager.Instance.GetSFXVolume());
    }
}
