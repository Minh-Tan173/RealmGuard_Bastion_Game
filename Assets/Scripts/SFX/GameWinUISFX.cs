using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWinUISFX : MonoBehaviour
{
    [SerializeField] private GameWinUI gameWinUI;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();

        gameWinUI.OnGameWinSFX += GameWinUI_OnGameWinSFX;
        gameWinUI.OnClickedButtonSFX += GameWinUI_OnClickedButtonSFX;
    }


    private void OnDestroy() {

        gameWinUI.OnGameWinSFX -= GameWinUI_OnGameWinSFX;
        gameWinUI.OnClickedButtonSFX -= GameWinUI_OnClickedButtonSFX;

    }

    private void GameWinUI_OnClickedButtonSFX(object sender, System.EventArgs e) {

        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().buttonDataHubClickedSFX, SoundManager.Instance.GetSFXVolume());

    }

    private void GameWinUI_OnGameWinSFX(object sender, System.EventArgs e) {

        MusicManager.Instance.PlayMusicOneShot(MusicManager.Instance.GetMusicManagerSO().gameWinTheme);

    }
}
