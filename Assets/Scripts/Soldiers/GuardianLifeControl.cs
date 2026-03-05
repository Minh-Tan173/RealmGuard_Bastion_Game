using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianLifeControl : MonoBehaviour, IHasProgressBar
{
    public enum GuardianLifeState {
        Alive,
        Death
    };

    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;
    public event EventHandler OnDeath;
    public event EventHandler OnDeathAnim;

    [SerializeField] private float deathTimer;

    private GuardianLifeState currentLifeState;
    private Guardian guardian;
    private float totalHealth;
    private float currentHealth;

    private void Awake() {

        guardian = GetComponent<Guardian>();

    }

    private void Start() {
        ChangeLifeStateTo(GuardianLifeState.Alive);
    }

    public void ChangeLifeStateTo(GuardianLifeState guardianLifeState) {

        this.currentLifeState = guardianLifeState;

        switch (guardianLifeState) {

            case GuardianLifeState.Alive:

                // Reset Health when back to Alive state
                totalHealth = guardian.GetGuardianTower().GetCurrentTowerStatus().healthOfGuardian;
                currentHealth = guardian.GetGuardianTower().GetCurrentTowerStatus().healthOfGuardian;

                OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 1f });

                break;

            case GuardianLifeState.Death:

                StartCoroutine(DeathCoroutine());

                break;

        }

    }

    private IEnumerator DeathCoroutine() {

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 0f });

        yield return null;

        OnDeathAnim?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(deathTimer);

        OnDeath?.Invoke(this, EventArgs.Empty); 

    }

    public void TakeDamage(float damage) {

        if (currentLifeState == GuardianLifeState.Death) {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0f) {

            ChangeLifeStateTo(GuardianLifeState.Death);
        }

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = Mathf.Clamp01(currentHealth / totalHealth) });

    }

    public GuardianLifeState GetCurrentLifeState() {
        return this.currentLifeState;
    }

    public bool IsDeath() {
        return this.currentLifeState == GuardianLifeState.Death;
    }
}
