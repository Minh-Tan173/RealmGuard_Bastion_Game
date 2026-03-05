using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeManager : MonoBehaviour
{
    public static GlobalVolumeManager Instance { get; private set; }

    private Volume globalVolume;
    private DepthOfField depthOfField;

    private void Awake() {

        Instance = this;

        globalVolume = GetComponent<Volume>();

        globalVolume.profile.TryGet(out depthOfField);
    }

    private void Start() {

        SetOffBlurBackground();
    }

    public void SetOnBlurBackground() {
        depthOfField.active = true;
    }

    public void SetOffBlurBackground() {
        depthOfField.active = false;
    }
}
