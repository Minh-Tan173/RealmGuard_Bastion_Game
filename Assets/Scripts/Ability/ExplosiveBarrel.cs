using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour, IAbility
{
    public event EventHandler<IAbility.OnAttackSFXEventArgs> OnAttackSFX;

    [Header("Data")]
    [SerializeField] private AbilitySO explosiveBarrelSO;

    [SerializeField] private Transform boomEffect;

    private AbilityLevelData currentLevelData;

    private void Start() {

        UpdateDataWithLevel();

        // After spawn
        StartCoroutine(ExplosionCoroutine());
    }

    private IEnumerator ExplosionCoroutine() {

        // 1. Placed
        yield return new WaitForSeconds(0.5f);

        // 2. Armed
        yield return new WaitForSeconds(0.5f);

        // 3. Exploding
        yield return null;
        
        OnAttackSFX?.Invoke(this, new IAbility.OnAttackSFXEventArgs { audioClip = SoundManager.Instance.GetAudioClipRefsSO().explosionBarrelSFX });
        
        yield return new WaitForSeconds(0.25f);

        boomEffect.localScale = new Vector3(currentLevelData.aoeRadius, currentLevelData.aoeRadius, currentLevelData.aoeRadius);

        Collider2D[] enemyDetectedArray = Physics2D.OverlapCircleAll(this.transform.position, currentLevelData.aoeRadius, explosiveBarrelSO.canAttackLayer);

        foreach(Collider2D enemyDetected in enemyDetectedArray) {

            if (enemyDetected.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                baseEnemy.HitDamage(currentLevelData.damage);

            }

        }

        yield return new WaitForSeconds(0.25f);

        yield return new WaitForSeconds(0.8f);

        Destroy(this.gameObject);
    }


    public void UpdateDataWithLevel() {

        IAbility.AbilityLevel currentLevel = SaveData.GetAbilityStatusByType(explosiveBarrelSO.abilityType).currentLevel;

        currentLevelData = SaveData.GetAbilityLevelDataByLevelAndType(currentLevel, explosiveBarrelSO.abilityType);
    }

    private void OnDrawGizmos() {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, explosiveBarrelSO.abilityLevelDataList[0].aoeRadius);
    }

}
