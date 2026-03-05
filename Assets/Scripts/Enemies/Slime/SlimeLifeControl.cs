using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeLifeControl : MonoBehaviour, IHasProgressBar
{


    // Health Bar Event
    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    public event EventHandler OnSpawn;

    // Anim Event
    public event EventHandler OnDeathAnim;

    private Slime slime;

    private BaseEnemy.EnemyLifeState currentSlimeLifeState;
    private float currentHealth;

    private void Awake() {

        slime = GetComponent<Slime>();

    }

    public void ChangeLifeStateTo(BaseEnemy.EnemyLifeState slimeLifeState) {

        this.currentSlimeLifeState = slimeLifeState;

        switch (currentSlimeLifeState) {

            case BaseEnemy.EnemyLifeState.Alive:


                break;

            case BaseEnemy.EnemyLifeState.Death:

                // DeathCoroutine Happen
                StartCoroutine(DeathCoroutine());

                break;

            case BaseEnemy.EnemyLifeState.Despawn:

                slime.Hide();

                break;
        }

    }

    public IEnumerator RespawnCoroutine() {

        currentHealth = slime.GetSlimeSO().totalHealth;

        // 1. Reset Life State
        ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Alive);

        yield return null;

        // 2.Reset Movement and Animator
        OnSpawn?.Invoke(this, EventArgs.Empty);

        yield return null;

        // 3. Spawn event happen
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = currentHealth / slime.GetSlimeSO().totalHealth });
    }

    private IEnumerator DeathCoroutine() {

        slime.SetCanMove(false);

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 0f });

        yield return null;

        // Death anim happen
        OnDeathAnim?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(slime.GetSlimeSO().deathTimer);

        // Death Behavior happen
        LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, slime.GetSlimeSO().enemyPrice);
        slime.Hide();

    }

    public void TakeDamage(float damageGet) {

        if (currentSlimeLifeState == BaseEnemy.EnemyLifeState.Death) {
            return;
        }

        currentHealth -= damageGet;

        if (currentHealth <= 0f) {

            ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Death);
        }

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = Mathf.Clamp01(currentHealth / slime.GetSlimeSO().totalHealth) });

    }

    public BaseEnemy.EnemyLifeState GetCurrentSlimeLifeState() {
        return this.currentSlimeLifeState;
    }

    public float GetCurrentHealth() {
        return this.currentHealth;
    }
}
