using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampaignMapUISFX : MonoBehaviour
{
    [SerializeField] private CampaignMapUI campaignMapUI;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {

        campaignMapUI.WorldMapScrollDownSFX += CampaignMapUI_WorldMapScrollDownSFX;
        campaignMapUI.WorldMapScrollUpSFX += CampaignMapUI_WorldMapScrollUpSFX;
        campaignMapUI.ButtonClickedSFX += CampaignMapUI_ButtonClickedSFX;
    }

    private void OnDestroy() {

        campaignMapUI.WorldMapScrollDownSFX -= CampaignMapUI_WorldMapScrollDownSFX;
        campaignMapUI.WorldMapScrollUpSFX -= CampaignMapUI_WorldMapScrollUpSFX;
        campaignMapUI.ButtonClickedSFX -= CampaignMapUI_ButtonClickedSFX;
    }

    private void CampaignMapUI_ButtonClickedSFX(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(MainMenuSoundManager.Instance.GetAudioClipRefsSO().buttonDataHubClickedSFX, MainMenuSoundManager.Instance.GetSFXVolume());
    }

    private void CampaignMapUI_WorldMapScrollDownSFX(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(MainMenuSoundManager.Instance.GetAudioClipRefsSO().paperScrollDownSFX, MainMenuSoundManager.Instance.GetSFXVolume());
    }

    private void CampaignMapUI_WorldMapScrollUpSFX(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(MainMenuSoundManager.Instance.GetAudioClipRefsSO().paperScrollUpSFX, MainMenuSoundManager.Instance.GetSFXVolume());
    }
}
