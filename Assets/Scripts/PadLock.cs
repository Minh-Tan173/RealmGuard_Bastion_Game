using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PadLock : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Transform parent;
    [SerializeField] private Image padLockImage;

    [Header("Unlock Data")]
    [SerializeField] private List<Sprite> padLockSpriteList;
    [SerializeField] private float frameTimer;

    private IHasPadLock hasPadLock;
    private CanvasGroup canvasGroup;

    private void Awake() {

        hasPadLock = parent.GetComponent<IHasPadLock>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (hasPadLock == null) {
            Debug.LogError("This script dont inherit IHasPadLock");
        }

        hasPadLock.UnlockPadLock += HasPadLock_UnlockPadLock;

    }

    private void Start() {
        padLockImage.sprite = padLockSpriteList[0];
    }

    private void OnDestroy() {
        hasPadLock.UnlockPadLock -= HasPadLock_UnlockPadLock;
    }

    private void HasPadLock_UnlockPadLock(object sender, IHasPadLock.UnlockPadLockEventArgs e) {
        StartCoroutine(UnlockCoroutine(e.callbackAction));
    }

    private IEnumerator UnlockCoroutine(Action callbackAction) {

        yield return new WaitForSecondsRealtime(0.1f);

        // 1. Unlock Padlock animation
        int totalPadLockFrame = padLockSpriteList.Count;
        float timer = frameTimer;

        for (int currentFrame = 0; currentFrame < totalPadLockFrame; currentFrame++) {

            padLockImage.sprite = padLockSpriteList[currentFrame];

            yield return new WaitForSecondsRealtime(frameTimer);
        }


        // 2. PadLock and Chain dissapear
        yield return new WaitForEndOfFrame();

        canvasGroup.alpha = 1f;
        float dispapointTimer = 0.2f;

        Sequence disapperSequence = DOTween.Sequence().SetUpdate(true);

        disapperSequence.Append(canvasGroup.DOFade(0f, dispapointTimer));

        yield return disapperSequence.WaitForCompletion();

        callbackAction.Invoke();

        yield return new WaitForEndOfFrame();

        this.gameObject.SetActive(false);
    }
}
