using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTowerSFX : MonoBehaviour
{
    [SerializeField] private CatapultTower catapultTower;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 10f;
        audioSource.maxDistance = 35f;
    }

    private void Start() {

        catapultTower.OnBuildingSFX += CatapultTower_OnBuildingSFX;
        catapultTower.UnBuildingSFX += CatapultTower_UnBuildingSFX;

        // After Placed Tower
        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().catapultTowerPlacedSFX, SoundManager.Instance.GetSFXVolume());
    }

    private void OnDestroy() {

        catapultTower.OnBuildingSFX -= CatapultTower_OnBuildingSFX;
        catapultTower.UnBuildingSFX -= CatapultTower_UnBuildingSFX;
    }

    private void CatapultTower_UnBuildingSFX(object sender, System.EventArgs e) {

        if (audioSource.clip != null) {
            audioSource.Stop();
        }

        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().catapultTowerUpgradeSuccessSFX, SoundManager.Instance.GetSFXVolume());

    }

    private void CatapultTower_OnBuildingSFX(object sender, System.EventArgs e) {

        audioSource.clip = SoundManager.Instance.GetAudioClipRefsSO().towerUpgradeProgressSFX;
        audioSource.loop = true;
        audioSource.volume = SoundManager.Instance.GetSFXVolume();
        audioSource.Play();

    }
}
