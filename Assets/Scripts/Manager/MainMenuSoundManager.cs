using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSoundManager : MonoBehaviour
{
    public static MainMenuSoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private void Awake() {

        Instance = this;

    }

    public float GetSFXVolume() {

        return SaveData.GetSFXVolumeSaved() * Convert.ToInt32(!SaveData.IsMutedSFX());
    }   

    public AudioClipRefsSO GetAudioClipRefsSO() {
        return this.audioClipRefsSO;
    }
}
