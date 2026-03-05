using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTowerSFX : MonoBehaviour
{
    [SerializeField] private ArcherTower archerTower;

    private AudioSource audioSource;

    private void Awake() {

        audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1f; 
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic; 
        audioSource.minDistance = 10f;   
        audioSource.maxDistance = 35f;

    }

    private void Start() {

        archerTower.OnBuildingSFX += ArcherTower_OnBuildingSFX;
        archerTower.UnBuildingSFX += ArcherTower_UnBuildingSFX;

        // After Placed Tower
        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().archerTowerPlacedSFX, SoundManager.Instance.GetSFXVolume());
    }

    private void OnDestroy() {

        archerTower.OnBuildingSFX -= ArcherTower_OnBuildingSFX;
        archerTower.UnBuildingSFX -= ArcherTower_UnBuildingSFX;
    }

    private void ArcherTower_UnBuildingSFX(object sender, System.EventArgs e) {

        if (audioSource.clip != null) {
            audioSource.Stop();
        }

        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().archerTowerUpgradeSuccessSFX, SoundManager.Instance.GetSFXVolume());
    }

    private void ArcherTower_OnBuildingSFX(object sender, System.EventArgs e) {
        audioSource.clip = SoundManager.Instance.GetAudioClipRefsSO().towerUpgradeProgressSFX;
        audioSource.loop = true;
        audioSource.volume = SoundManager.Instance.GetSFXVolume();
        audioSource.Play();
    }
}
