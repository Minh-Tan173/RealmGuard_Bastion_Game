using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeAbilityUISFX : MonoBehaviour
{
    [SerializeField] private AbilityHubUI upgradeAbilityUI;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();

        upgradeAbilityUI.OnScrollButtonSFX += UpgradeAbilityUI_OnScrollButtonSFX;
    }

    private void OnDestroy() {
        upgradeAbilityUI.OnScrollButtonSFX -= UpgradeAbilityUI_OnScrollButtonSFX;
    }

    private void UpgradeAbilityUI_OnScrollButtonSFX(object sender, System.EventArgs e) {
        audioSource.PlayOneShot(MainMenuSoundManager.Instance.GetAudioClipRefsSO().buttonScrollClickedSFX, MainMenuSoundManager.Instance.GetSFXVolume());
    }
}
