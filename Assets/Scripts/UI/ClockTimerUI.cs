using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClockTimerUI : MonoBehaviour
{
    [Header("Parent object")]
    [SerializeField] private Transform parentObject;

    [Header("Child object")]
    [SerializeField] private Image background;
    [SerializeField] private Image clockTimer;

    private IHasClockTimer hasClockTimer;

    private void Awake() {

        hasClockTimer = parentObject.GetComponent<IHasClockTimer>();

        if (hasClockTimer == null) {

            Debug.LogError("Parent object don't inherit IHasClockTimer interface");

        }

        hasClockTimer.OnChangeProgress += HasClockTimer_OnChangeProgress;

    }

    private void Start() {

        Hide();

    }

    private void OnDestroy() {

        hasClockTimer.OnChangeProgress -= HasClockTimer_OnChangeProgress;

    }

    private void HasClockTimer_OnChangeProgress(object sender, IHasClockTimer.OnChangeProgressEventArgs e) {

        clockTimer.fillAmount = e.progressNormalized;

        if (e.progressNormalized == 1f || e.progressNormalized == 0f) {

            Hide();

        }
        else {
            Show();
        }

    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }
}
