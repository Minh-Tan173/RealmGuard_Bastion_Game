using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTowerLifeControl : MonoBehaviour, IHasProgressBar
{
    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    [SerializeField] private ProgressBarUI progressBarUI;

    private CatapultTower catapultTower;

    private Coroutine currentCoroutine;
    private float currentHealth;

    private bool isRecoveringHealth;

    private void Awake() {
        catapultTower = GetComponent<CatapultTower>();

        currentHealth = 0f;

        catapultTower.OnUpgradeLevelProgress += CatapultTower_OnUpgradeLevelProgress;
    }

    private void Start() {

        catapultTower.GetCatapultTowerUI().OnFixingTower += MageTower_OnFixingTower;

        // After spawn
    }

    private void OnDestroy() {

        catapultTower.OnUpgradeLevelProgress -= CatapultTower_OnUpgradeLevelProgress;
        catapultTower.GetCatapultTowerUI().OnFixingTower -= MageTower_OnFixingTower;
    }

    private void MageTower_OnFixingTower(object sender, EventArgs e) {

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        currentCoroutine = StartCoroutine(HealthRecoveryCoroutine());

    }

    private void CatapultTower_OnUpgradeLevelProgress(object sender, EventArgs e) {

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        currentCoroutine = StartCoroutine(HealthRecoveryCoroutine());
    }

    private IEnumerator HealthRecoveryCoroutine() {

        isRecoveringHealth = true;

        // Update health data
        float totalHealth = catapultTower.GetCatapultTowerSO().healthTower;
        float upgradeTimerMax = catapultTower.GetCurrentTowerStatus().upgradeTimer;

        float initialProgress = Mathf.Clamp01(this.currentHealth / totalHealth);

        // Update health bar first
        progressBarUI.Show();
        OnChangeProgress.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = initialProgress });

        // Start
        float upgradeTimer = initialProgress * upgradeTimerMax;

        // Progress
        while (upgradeTimer <= upgradeTimerMax) {

            upgradeTimer += Time.deltaTime;

            float progress = Mathf.Clamp01(upgradeTimer / upgradeTimerMax);

            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            this.currentHealth = smoothProgress * totalHealth;

            OnChangeProgress.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = smoothProgress });

            yield return null;
        }

        // End
        this.currentHealth = totalHealth;

        // Upgrade health bar
        OnChangeProgress.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 1f });

        yield return new WaitForSeconds(0.5f);

        currentCoroutine = null;

        isRecoveringHealth = false;

        progressBarUI.Hide();
    }

    private IEnumerator OnDestroyTower() {

        // Show HealthBar
        progressBarUI.Show();

        yield return new WaitForSeconds(0.1f);

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 0f });

        GridManager.Instance.SetHasItemArea2x2(this.transform.position, isSetHasItemON: false);

        Destroy(this.transform.gameObject);
    }

    public void TakeDamage(float dameGet) {

        currentHealth -= dameGet;

        if (currentHealth <= 0f) {

            if (currentCoroutine != null) {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            currentCoroutine = StartCoroutine(OnDestroyTower());

            return;

        }

        progressBarUI.Show();
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = this.currentHealth / catapultTower.GetCatapultTowerSO().healthTower });

        // After 5s, hide bar
        CancelInvoke(nameof(HideProgressBarUI));
        Invoke(nameof(HideProgressBarUI), 0.5f);
    }

    private void HideProgressBarUI() {
        progressBarUI.Hide();
    }

    public bool IsRecoveringHealth() {
        return this.isRecoveringHealth;
    }
}
