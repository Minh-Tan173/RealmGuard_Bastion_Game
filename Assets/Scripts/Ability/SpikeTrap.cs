using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour, IAbility {

    public event EventHandler<IAbility.OnAttackSFXEventArgs> OnAttackSFX;
    public event EventHandler OnResetAnimator;

    [SerializeField] private AbilitySO spikeTrapSO;

    private BoxCollider2D boxCollider2D;
    private AbilityLevelData currentLevelData;

    private int numberAttack;

    private void Awake() {

        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Start() {

        UpdateDataWithLevel();

        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine() {

        int attackNumber = 0;

        while (attackNumber < numberAttack) {

            OnResetAnimator?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(0.17f);

            OnAttackSFX?.Invoke(this, new IAbility.OnAttackSFXEventArgs { audioClip = SoundManager.Instance.GetAudioClipRefsSO().spikeAttackSFX });

            yield return new WaitForSeconds(0.08f);
            
            Vector2 sizeCheck = new Vector2(boxCollider2D.size.x, boxCollider2D.size.y);
            Collider2D[] enemyDetectedArray = Physics2D.OverlapBoxAll(this.transform.position, sizeCheck, angle: 0f, spikeTrapSO.canAttackLayer);
            
            foreach(Collider2D enemy in enemyDetectedArray) {

                if (enemy.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                    baseEnemy.HitDamage(currentLevelData.damage);
                }

            }

            attackNumber += 1;
            yield return new WaitForSeconds(0.25f);
        }

        yield return null; 

        Destroy(this.gameObject);

    }

    public void UpdateDataWithLevel() {

        IAbility.AbilityLevel currentLevel = SaveData.GetAbilityStatusByType(spikeTrapSO.abilityType).currentLevel;

        currentLevelData = SaveData.GetAbilityLevelDataByLevelAndType(currentLevel, spikeTrapSO.abilityType);

        SpikeTrapSpecificData spikeTrapSpecificData = currentLevelData.specificData as SpikeTrapSpecificData;

        this.numberAttack = spikeTrapSpecificData.strikeCount;
    }

    public AbilitySO GetAbilitySO() {
        return this.spikeTrapSO;
    }
}
