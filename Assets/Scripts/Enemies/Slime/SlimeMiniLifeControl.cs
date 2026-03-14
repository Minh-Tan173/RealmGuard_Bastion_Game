using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMiniLifeControl : MonoBehaviour, IHasProgressBar
{

    // Health Bar Event
    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    // Anim Event
    public event EventHandler OnDeathAnim;

    private SlimeMini slimeMini;

    private BaseEnemy.EnemyLifeState currentSlimeLifeState;
    private float currentHealth;

    private void Awake() {

        slimeMini = GetComponent<SlimeMini>();

    }

    private void Start() {

        // After Spawn
        currentHealth = slimeMini.GetSlimeSO().totalHealth;
        ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Alive);
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = currentHealth / slimeMini.GetSlimeSO().totalHealth });
    }

    private void ChangeLifeStateTo(BaseEnemy.EnemyLifeState slimeLifeState) {

        this.currentSlimeLifeState = slimeLifeState;

        switch (currentSlimeLifeState) {

            case BaseEnemy.EnemyLifeState.Alive:


                break;

            case BaseEnemy.EnemyLifeState.Death:

                // DeathCoroutine Happen
                StartCoroutine(DeathCoroutine());

                break;
        }

    }


    private IEnumerator DeathCoroutine() {

        slimeMini.SetCanMove(false);

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 0f });

        yield return null;

        // Death anim happen
        OnDeathAnim?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(slimeMini.GetSlimeSO().deathTimer);

        slimeMini.OnDestroySelf(); 

    }

    public void TakeDamage(float damageGet) {

        if (currentSlimeLifeState == BaseEnemy.EnemyLifeState.Death) {
            return;
        }

        currentHealth -= damageGet;

        if (currentHealth <= 0f) {

            ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Death);
        }

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = Mathf.Clamp01(currentHealth / slimeMini.GetSlimeSO().totalHealth) });

    }

    public BaseEnemy.EnemyLifeState GetCurrentSlimeLifeState() {
        return this.currentSlimeLifeState;
    }

    public float GetCurrentHealth() {
        return this.currentHealth;
    }
}
