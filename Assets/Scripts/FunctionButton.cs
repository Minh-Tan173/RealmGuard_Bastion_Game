using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class FunctionButton : MonoBehaviour
{
    [Header("UI Parent")]
    [SerializeField] private Transform UIParent;

    [Header("Animation data")]
    [SerializeField] private float animTimer;
    [SerializeField] private float targetDistance;
    [SerializeField] private float targetAngle;

    private GraphicRaycaster graphicRaycaster;
    private IHasFunctionButton hasFunctionButton;
    private RadialLayoutGroup radialLayoutGroup;
    private Coroutine currentCoroutine;

    private void Awake() {

        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();  

        radialLayoutGroup = GetComponent<RadialLayoutGroup>();

        hasFunctionButton = UIParent.GetComponent<IHasFunctionButton>();

        if (hasFunctionButton == null) {
            Debug.LogError("Parent dont inherit IHasFunctionButton interface");
        }

        hasFunctionButton.OnFunctionButton += HasFunctionButton_OnFunctionButton;
        hasFunctionButton.OffFunctionButton += HasFunctionButton_OffFunctionButton;
    }


    private void OnDestroy() {
        hasFunctionButton.OnFunctionButton -= HasFunctionButton_OnFunctionButton;
        hasFunctionButton.OffFunctionButton -= HasFunctionButton_OffFunctionButton;
    }

    private void HasFunctionButton_OffFunctionButton(object sender, IHasFunctionButton.OffFunctionButtonEventArgs e) {

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        currentCoroutine = StartCoroutine(DisapperCoroutine(e.callbackAction));
    }

    private void HasFunctionButton_OnFunctionButton(object sender, System.EventArgs e) {
        
        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        currentCoroutine = StartCoroutine(AppearCoroutine());

    }

    private IEnumerator AppearCoroutine() {

        // Block mouse cast
        graphicRaycaster.enabled = false;

        float timer = 0f;
        float timerMax = animTimer;

        // ---- Start value ----
        radialLayoutGroup.fDistance = 0f;
        radialLayoutGroup.MinAngle = 0f;
        radialLayoutGroup.CalculateLayoutInputVertical();

        yield return null;

        // ---- Appear Progress ----

        while (timer <= timerMax) {

            float elapsedTimer = Mathf.Clamp01(timer / timerMax);

            radialLayoutGroup.fDistance = Mathf.Lerp(0f, targetDistance, elapsedTimer);
            radialLayoutGroup.MinAngle = Mathf.Lerp(0f, targetAngle, elapsedTimer);

            radialLayoutGroup.CalculateLayoutInputVertical();

            timer += Time.deltaTime;

            yield return null;
        }

        // ---- End value ----
        radialLayoutGroup.fDistance = targetDistance;
        radialLayoutGroup.MinAngle = targetAngle;
        radialLayoutGroup.CalculateLayoutInputVertical();

        graphicRaycaster.enabled = true;

    }

    private IEnumerator DisapperCoroutine(Action action) {

        graphicRaycaster.enabled = false;

        float timer = animTimer;
        float timerMax = animTimer;

        // ---- Start value ----
        radialLayoutGroup.fDistance = targetDistance;
        radialLayoutGroup.MinAngle = targetAngle;

        radialLayoutGroup.CalculateLayoutInputVertical();

        yield return null;

        // ---- Appear Progress ----

        while (timer >= 0f) {

            float elapsedTimer = Mathf.Clamp01(timer / timerMax);

            radialLayoutGroup.fDistance = Mathf.Lerp(0f, targetDistance, elapsedTimer);
            radialLayoutGroup.MinAngle = Mathf.Lerp(0f, targetAngle, elapsedTimer);

            radialLayoutGroup.CalculateLayoutInputVertical();

            timer -= Time.deltaTime;

            yield return null;
        }

        // ---- End value ----
        radialLayoutGroup.fDistance = 0f;
        radialLayoutGroup.MinAngle = 0f;
        radialLayoutGroup.CalculateLayoutInputVertical();

        yield return null;

        // Call action
        action();
    }
}
