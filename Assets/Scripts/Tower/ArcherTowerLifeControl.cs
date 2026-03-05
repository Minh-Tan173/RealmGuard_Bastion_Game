using System;
using System.Collections;
using UnityEngine;

public class ArcherTowerLifeControl : MonoBehaviour, IHasProgressBar 
{

    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    [SerializeField] private ProgressBarUI progressBarUI;

    private ArcherTower archerTower;

    private Coroutine currentCoroutine;
    private float currentHealth;

    private bool isRecoveringHealth;

    private void Awake() {
        archerTower = GetComponent<ArcherTower>();

        currentHealth = 0f;

        archerTower.OnUpgradeLevelProgress += ArcherTower_OnUpgradeLevelProgress;
    }

    private void Start() {

        archerTower.GetArcherTowerUI().OnFixingTower += ArcherTowerLifeControl_OnFixingTower;

        // After spawn
    }

    private void OnDestroy() {

        archerTower.OnUpgradeLevelProgress -= ArcherTower_OnUpgradeLevelProgress;
        archerTower.GetArcherTowerUI().OnFixingTower -= ArcherTowerLifeControl_OnFixingTower;
    }

    private void ArcherTowerLifeControl_OnFixingTower(object sender, EventArgs e) {

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        currentCoroutine = StartCoroutine(HealthRecoveryCoroutine());

    }

    private void ArcherTower_OnUpgradeLevelProgress(object sender, EventArgs e) {
        
        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;    
        }

        currentCoroutine = StartCoroutine(HealthRecoveryCoroutine());
    }

    private IEnumerator HealthRecoveryCoroutine() {

        isRecoveringHealth = true;

        // Update health data
        float totalHealth = archerTower.GetArcherTowerSO().healthTower;
        float upgradeTimerMax = archerTower.GetCurrentTowerStatus().upgradeTimer;

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
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = this.currentHealth / archerTower.GetArcherTowerSO().healthTower });

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
