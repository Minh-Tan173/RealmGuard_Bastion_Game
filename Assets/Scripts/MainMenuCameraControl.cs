using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCameraControl : MonoBehaviour
{
    [Header("Cấu hình Thở")]
    public float breatheAmount = 0.2f; 
    public float duration = 6f;   

    private CinemachineVirtualCamera vcam;
    private float originalSize;

    // Start is called before the first frame update
    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        originalSize = vcam.m_Lens.OrthographicSize;

        DOVirtual.Float(originalSize, originalSize - breatheAmount, duration, SetCameraSize).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    void SetCameraSize(float x) {
        vcam.m_Lens.OrthographicSize = x;
    }
}
