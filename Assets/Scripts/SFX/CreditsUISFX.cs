using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsUISFX : MonoBehaviour
{
    [SerializeField] private CreditsUI creditsUI;

    private AudioSource audioSource;

    private void Awake() {

        audioSource = GetComponent<AudioSource>();

        creditsUI.AncientPaperScrollDownSFX += CreditsUI_AncientPaperScrollDownSFX;
        creditsUI.AncientPaperScrollUpSFX += CreditsUI_AncientPaperScrollUpSFX;
    }

    private void OnDestroy() {
        creditsUI.AncientPaperScrollDownSFX -= CreditsUI_AncientPaperScrollDownSFX;
        creditsUI.AncientPaperScrollUpSFX -= CreditsUI_AncientPaperScrollUpSFX;
    }


    private void CreditsUI_AncientPaperScrollUpSFX(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(MainMenuSoundManager.Instance.GetAudioClipRefsSO().paperScrollUpSFX, MainMenuSoundManager.Instance.GetSFXVolume());
    }

    private void CreditsUI_AncientPaperScrollDownSFX(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(MainMenuSoundManager.Instance.GetAudioClipRefsSO().paperScrollDownSFX, MainMenuSoundManager.Instance.GetSFXVolume());
    }
}
