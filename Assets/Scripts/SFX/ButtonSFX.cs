using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private Button button;
    private AudioSource audioSource;

    private void Awake() {

        button = GetComponentInParent<Button>();
        audioSource = GetComponent<AudioSource>();

        button.onClick.AddListener(() => {

            float sfxVolume = SaveData.GetSFXVolumeSaved() * Convert.ToInt32(!SaveData.IsMutedSFX());

            audioSource.PlayOneShot(audioClipRefsSO.buttonClickedSFX, sfxVolume);

        });
    }
}
