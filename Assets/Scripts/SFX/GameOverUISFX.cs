using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverUISFX : MonoBehaviour
{
    [SerializeField] private GameOverUI gameOverUI;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();

        gameOverUI.OnGameOverSFX += GameOverUI_OnGameOverSFX;
        gameOverUI.OnClickedButtonSFX += GameOverUI_OnClickedButtonSFX;
    }

    private void OnDestroy() {

        gameOverUI.OnGameOverSFX -= GameOverUI_OnGameOverSFX;
        gameOverUI.OnClickedButtonSFX -= GameOverUI_OnClickedButtonSFX;

    }

    private void GameOverUI_OnClickedButtonSFX(object sender, System.EventArgs e) {

        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().buttonDataHubClickedSFX, SoundManager.Instance.GetSFXVolume());

    }

    private void GameOverUI_OnGameOverSFX(object sender, System.EventArgs e) {

        MusicManager.Instance.PlayMusicOneShot(MusicManager.Instance.GetMusicManagerSO().gameOverTheme);

    }
}
