using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("SFX Data")]
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float sfxVolume;

    private void Awake() {

        Instance = this;

        // Setup sfx volume After Spawn
        sfxVolume = SaveData.GetSFXVolumeSaved() * Convert.ToInt32(!SaveData.IsMutedSFX());

    }

    private void Start() {

        Catapult.OnAttackSFX += Catapult_OnAttackSFX;
    }

    private void OnDestroy() {

        Catapult.OnAttackSFX -= Catapult_OnAttackSFX;
    }


    private void Catapult_OnAttackSFX(object sender, System.EventArgs e) {
        Catapult catapult = sender as Catapult;
        PlaySFX(audioClipRefsSO.catapultAttackSFX, catapult.transform.position);
    }


    private void PlaySFX(AudioClip audioClip, Vector3 position, float volumeMultiply = 1f) {

        AudioSource.PlayClipAtPoint(audioClip, position, this.sfxVolume * volumeMultiply);
    }

    public float GetSFXVolume() {
        return this.sfxVolume;
    }

    public AudioClipRefsSO GetAudioClipRefsSO() {
        return this.audioClipRefsSO;
    }

    public void SetSFXVolume(float sfxVolume) {

        if (sfxVolume <= 0f) {
            // If slider value <= 0f --> muted

            SaveData.SetMutedSFX(true);
        }
        else {
            // If slider value > 0f ---> unMuted

            SaveData.SetMutedSFX(false);
        }

        this.sfxVolume = sfxVolume;
        SaveData.SetVolumeData(sfxVolume, hasChangedMusic: false, hasChangedSFX: true);

    }
}
