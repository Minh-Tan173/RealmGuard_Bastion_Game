using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderStrikeAnomalySFX : MonoBehaviour
{
    private Orc orcParent;

    private AudioSource audioSource;

    private void Awake() {

        audioSource = GetComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 10f;
        audioSource.maxDistance = 20f;
    }

    private void Start() {
        orcParent.OnThunderStrikeAnomalySFX += OrcParent_OnThunderStrikeAnomalySFX;
    }

    private void OnDestroy() {

        orcParent.OnThunderStrikeAnomalySFX -= OrcParent_OnThunderStrikeAnomalySFX;
    }

    private void OrcParent_OnThunderStrikeAnomalySFX(object sender, System.EventArgs e) {

        audioSource.PlayOneShot(SoundManager.Instance.GetAudioClipRefsSO().thunderStrikeSFX, SoundManager.Instance.GetSFXVolume());
    }

    public void SetOrcParent(Orc orcParent) {

        this.orcParent = orcParent;
    }
}
