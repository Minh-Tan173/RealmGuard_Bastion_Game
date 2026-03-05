using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [Header("Parent object")]
    [SerializeField] private Transform parentObject;

    [Header("Child object")]
    [SerializeField] private Image background;
    [SerializeField] private Image  progressBar;

    private IHasProgressBar hasProgressBar;

    private void Awake() {

        hasProgressBar = parentObject.GetComponent<IHasProgressBar>();

        if (hasProgressBar == null) {

            Debug.LogError("Parent object don't inherit IHasProgressBar interface");

        }

        hasProgressBar.OnChangeProgress += HasProgressBar_OnChangeProgress;
        
    }

    private void LateUpdate() {

        transform.rotation = Quaternion.identity;

    }

    private void OnDestroy() {

        hasProgressBar.OnChangeProgress -= HasProgressBar_OnChangeProgress;

    }

    private void HasProgressBar_OnChangeProgress(object sender, IHasProgressBar.OnChangeProgressEventArgs e) {

        progressBar.fillAmount = e.progressNormalized;

    }

    public void Show() {

        background.gameObject.SetActive(true);
        progressBar.gameObject.SetActive(true);

    }

    public void Hide() {

        background.gameObject.SetActive(false);
        progressBar.gameObject.SetActive(false);

    }
}
