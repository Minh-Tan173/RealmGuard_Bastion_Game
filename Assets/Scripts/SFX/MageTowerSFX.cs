using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageTowerSFX : MonoBehaviour
{
    [SerializeField] private MageTower mageTower;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 10f;
        audioSource.maxDistance = 35f;
    }

    private void Start() {
        mageTower.OnBuildingSFX += MageTower_OnBuildingSFX;
        mageTower.UnBuildingSFX += MageTower_UnBuildingSFX;

        // After Placed Tower
        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().mageTowerPlacedSFX, SoundManager.Instance.GetSFXVolume());
    }

    private void OnDestroy() {

        mageTower.OnBuildingSFX -= MageTower_OnBuildingSFX;
        mageTower.UnBuildingSFX -= MageTower_UnBuildingSFX;
    }

    private void MageTower_UnBuildingSFX(object sender, System.EventArgs e) {

        if (audioSource.clip != null) {
            audioSource.Stop();
        }

        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().mageTowerUpgradeSuccessSFX, SoundManager.Instance.GetSFXVolume());

    }

    private void MageTower_OnBuildingSFX(object sender, System.EventArgs e) {

        audioSource.clip = SoundManager.Instance.GetAudioClipRefsSO().towerUpgradeProgressSFX;
        audioSource.loop = true;
        audioSource.volume = SoundManager.Instance.GetSFXVolume();
        audioSource.Play();

    }
}
