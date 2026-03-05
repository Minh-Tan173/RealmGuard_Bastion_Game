using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcLifeControl : MonoBehaviour, IHasProgressBar
{

    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    public event EventHandler OnSpawn;
    public event EventHandler OnDeath;

    // Death Anim Event Happen
    public event EventHandler OnDeathAnim;

    private Orc orc;

    private BaseEnemy.EnemyLifeState currentWolfLifeState;

    private float currentHealth;

    private void Awake() {

        orc = GetComponent<Orc>();

    }

    public void ChangeLifeStateTo(BaseEnemy.EnemyLifeState wolfLifeState) {

        this.currentWolfLifeState = wolfLifeState;

        switch (this.currentWolfLifeState) {

            case BaseEnemy.EnemyLifeState.Alive:

                break;

            case BaseEnemy.EnemyLifeState.Death:

                StartCoroutine(DeathCoroutine());

                break;
            case BaseEnemy.EnemyLifeState.Despawn:

                orc.Hide();

                break;
        }
    }


    private IEnumerator DeathCoroutine() {

        // Stop all behavior

        orc.SetCanMove(false);

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 0f });

        yield return null;

        // Death Anim Happen
        OnDeathAnim?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(orc.GetOrcSO().deathTimer);

        LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, orc.GetOrcSO().enemyPrice);
        OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public BaseEnemy.EnemyLifeState GetCurrentOrcLifeState() {
        return this.currentWolfLifeState;
    }

    public IEnumerator RespawnCoroutine() {

        // 1. Reset Health
        currentHealth = orc.GetOrcSO().totalHealth;
        ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Alive);

        yield return null;

        // 2. Reset movement and Animator
        OnSpawn?.Invoke(this, EventArgs.Empty);

        yield return null;

        // 3. Reset Health Bar
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 1f });

    }

    public void TakeDamage(float damageGet) {

        if (this.currentWolfLifeState == BaseEnemy.EnemyLifeState.Death) {
            return;
        }

        this.currentHealth -= damageGet;

        if (this.currentHealth <= 0f) {

            ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Death);

        }

        float healthNormalized = Mathf.Clamp01(this.currentHealth / orc.GetOrcSO().totalHealth);
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = healthNormalized });
    }
}
