using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianTowerSFX : MonoBehaviour
{
    [SerializeField] private GuardianTower guardianTower;

    private AudioSource audioSource;

    private void Awake() {

        audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 10f;
        audioSource.maxDistance = 35f;

    }

    private void Start() {

        guardianTower.OnBuildingSFX += GuardianTower_OnBuildingSFX;
        guardianTower.UnBuildingSFX += GuardianTower_UnBuildingSFX;
        guardianTower.OnAssignPositionZone += GuardianTower_OnAssignPositionZone;
        guardianTower.OnDeployCompleteSFX += GuardianTower_OnDeployCompleteSFX;

        guardianTower.GetAssignPositionZoneVisual().OnMoveCommandSFX += GuardianTowerSFX_OnMoveCommandSFX;

        // After Placed Tower
        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().guardianTowerPlacedSFX, SoundManager.Instance.GetSFXVolume());
    }

    private void OnDestroy() {

        guardianTower.OnBuildingSFX -= GuardianTower_OnBuildingSFX;
        guardianTower.UnBuildingSFX -= GuardianTower_UnBuildingSFX;
        guardianTower.OnAssignPositionZone -= GuardianTower_OnAssignPositionZone;
        guardianTower.OnDeployCompleteSFX -= GuardianTower_OnDeployCompleteSFX;

        guardianTower.GetAssignPositionZoneVisual().OnMoveCommandSFX -= GuardianTowerSFX_OnMoveCommandSFX;
    }

    private void GuardianTower_OnDeployCompleteSFX(object sender, System.EventArgs e) {

        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().guardianDeployComplete, SoundManager.Instance.GetSFXVolume());

    }

    private void GuardianTowerSFX_OnMoveCommandSFX(object sender, System.EventArgs e) {

        PlayRandomAudioClipByArraySfx(SoundManager.Instance.GetAudioClipRefsSO().guardianOnCommandMovedSFX);
    }

    private void GuardianTower_OnAssignPositionZone(object sender, System.EventArgs e) {

        PlayRandomAudioClipByArraySfx(SoundManager.Instance.GetAudioClipRefsSO().guardianTowerOnAssignPosVisualSFX);

    }

    private void GuardianTower_UnBuildingSFX(object sender, System.EventArgs e) {

        if (audioSource.clip != null) {
            audioSource.Stop();
        }

        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().guardianTowerUpgradeSuccessSFX, SoundManager.Instance.GetSFXVolume());
    }

    private void GuardianTower_OnBuildingSFX(object sender, System.EventArgs e) {
        audioSource.clip = SoundManager.Instance.GetAudioClipRefsSO().towerUpgradeProgressSFX;
        audioSource.loop = true;
        audioSource.volume = SoundManager.Instance.GetSFXVolume();
        audioSource.Play();
    }

    private void PlayRandomAudioClipByArraySfx(AudioClip[] audioClipArray) {

        AudioClip audioClip = audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)];

        audioSource.PlayOneShot(audioClip, SoundManager.Instance.GetSFXVolume());

    }
}
