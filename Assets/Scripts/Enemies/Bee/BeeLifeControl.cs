using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeLifeControl : MonoBehaviour, IHasProgressBar
{

    // Health Bar Event
    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    public event EventHandler OnSpawn;
    public event EventHandler OnDeathAnim;

    private Bee bee;

    private BaseEnemy.EnemyLifeState currentBeeLifeState;
    private float currentHealth;

    private void Awake() {
        
        bee = GetComponent<Bee>();

    }

    public void ChangeLifeStateTo(BaseEnemy.EnemyLifeState beeLifeState) {

        this.currentBeeLifeState = beeLifeState;

        switch (currentBeeLifeState) {

            case BaseEnemy.EnemyLifeState.Alive:


                break;

            case BaseEnemy.EnemyLifeState.Death:

                // DeathCoroutine Happen
                StartCoroutine(DeathCoroutine());

                break;

            case BaseEnemy.EnemyLifeState.Despawn:

                bee.Hide();

                break;
        }

    }

    public IEnumerator RespawnCoroutine() {

        currentHealth = bee.GetBeeSO().totalHealth;

        // 1. Reset Life State
        ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Alive);

        yield return null;

        // 2.Reset Movement and Animator
        OnSpawn?.Invoke(this, EventArgs.Empty);

        yield return null;

        // 3. Spawn event happen
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = currentHealth / bee.GetBeeSO().totalHealth });
    }

    private IEnumerator DeathCoroutine() {

        bee.SetCanMove(false);

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 0f });

        yield return null;

        OnDeathAnim?.Invoke(this, EventArgs.Empty); 

        yield return new WaitForSeconds(bee.GetBeeSO().deathTimer);

        LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, bee.GetBeeSO().enemyPrice);
        bee.Hide();

    }

    public void TakeDamage(float damageGet) {

        if (currentBeeLifeState == BaseEnemy.EnemyLifeState.Death) {
            return;
        }

        currentHealth -= damageGet;

        if (currentHealth <= 0f) {

            ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Death);
        }

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = Mathf.Clamp01(currentHealth / bee.GetBeeSO().totalHealth) });

    }

    public BaseEnemy.EnemyLifeState GetCurrentBeeLifeState() {
        return this.currentBeeLifeState;
    }

}
