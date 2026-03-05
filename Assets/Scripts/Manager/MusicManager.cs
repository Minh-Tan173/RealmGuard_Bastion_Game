using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private MusicManagerSO musicManagerSO;

    private AudioSource audioSource;
    private bool isPlayedMainTheme;
    private float musicVolume;

    private void Awake() {

        Instance = this;

        audioSource = GetComponent<AudioSource>();

        isPlayedMainTheme = false;

        musicVolume = SaveData.GetMusicVolumeSaved() * Convert.ToInt32(!SaveData.IsMutedMusic());
        

    }

    private void Start() {

        // When start game
        PlayMusic(musicManagerSO.startGameBattleTheme);
        LevelManager.Instance.OnGameRunningState += LevelManager_OnGameRunningState;
    }

    private void OnDestroy() {

        LevelManager.Instance.OnGameRunningState -= LevelManager_OnGameRunningState;

    }

    private void LevelManager_OnGameRunningState(object sender, System.EventArgs e) {

        if (!isPlayedMainTheme) {
            
            isPlayedMainTheme = true;

            PlayMusic(musicManagerSO.mainTheme[UnityEngine.Random.Range(0, musicManagerSO.mainTheme.Length)]);
        }
    }

    public void PlayMusic(AudioClip audioClip) {

        if (audioSource.clip != null) {
            audioSource.Stop();
        }

        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.volume = this.musicVolume;
        audioSource.Play();
    }

    public void PlayMusicOneShot(AudioClip audioClip) {

        if (audioSource.clip != null) {
            audioSource.Stop();
        }

        audioSource.clip = audioClip;
        audioSource.loop = false;
        audioSource.volume = this.musicVolume;
        audioSource.Play();
    }

    public void SetVolume(float musicVolume) {

        if (musicVolume <= 0f) {
            // If music slider value is <= 0f --> Muted

            SaveData.SetMutedMusic(true);
        }
        else {

            SaveData.SetMutedMusic(false);
        }

        this.musicVolume = musicVolume;

        audioSource.volume = this.musicVolume;

        SaveData.SetVolumeData(musicVolume, hasChangedMusic: true, hasChangedSFX: false);

    }

    public MusicManagerSO GetMusicManagerSO() {
        return this.musicManagerSO;
    }

}

