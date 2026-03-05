using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfLifeControl : MonoBehaviour, IHasProgressBar
{

    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    public event EventHandler OnSpawn;

    // Death Anim Event Happen
    public event EventHandler OnDeathAnim;

    private Wolf wolf;

    private BaseEnemy.EnemyLifeState currentWolfLifeState;

    private float currentHealth;

    private void Awake() {

        wolf = GetComponent<Wolf>();

    }

    public void ChangeLifeStateTo(BaseEnemy.EnemyLifeState wolfLifeState) {

        this.currentWolfLifeState = wolfLifeState;

        switch(this.currentWolfLifeState) {

            case BaseEnemy.EnemyLifeState.Alive:
                
                break;

            case BaseEnemy.EnemyLifeState.Death:

                StartCoroutine(DeathCoroutine());

                break;
            case BaseEnemy.EnemyLifeState.Despawn:

                wolf.Hide();

                break;  
        }
    }


    private IEnumerator DeathCoroutine() {

        // Stop all behavior

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 0f });

        yield return null;

        // Death Anim Happen
        OnDeathAnim?.Invoke(this, EventArgs.Empty);       

        yield return new WaitForSeconds(wolf.GetWolfSO().deathTimer);

        LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, wolf.GetWolfSO().enemyPrice);
        wolf.Hide();
    }

    public BaseEnemy.EnemyLifeState GetCurrentWolfLifeState() {
        return this.currentWolfLifeState;
    }

    public IEnumerator RespawnCoroutine() {

        // 1. Reset Health
        currentHealth = wolf.GetWolfSO().totalHealth;
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

        float healthNormalized = Mathf.Clamp01(this.currentHealth / wolf.GetWolfSO().totalHealth);
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = healthNormalized });
    }
}
