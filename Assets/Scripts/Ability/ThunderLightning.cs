using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderLightning : MonoBehaviour, IAbility
{

    public event EventHandler<IAbility.OnAttackSFXEventArgs> OnAttackSFX;

    [Header("Data")]
    [SerializeField] private AbilitySO thunderLightningSO;

    [Header("Child")]
    [SerializeField] private Transform strikePoint;

    private Coroutine currentCoroutine;

    private AbilityLevelData currentLevelData;

    private void Awake() {
        currentCoroutine = null;    
    }

    private void Start() {

        UpdateDataWithLevel();

        // After spawn
        StartCoroutine(StrikeCoroutine());
    }

    public IEnumerator StrikeCoroutine() {

        if (currentCoroutine!= null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        // ---- Strike behavior happen ----

        yield return new WaitForSeconds(0.05f); // Wait for strikePoint reach last Pos

        OnAttackSFX?.Invoke(this, new IAbility.OnAttackSFXEventArgs { audioClip = SoundManager.Instance.GetAudioClipRefsSO().thunderStrikeSFX });

        Collider2D[] enemyStrikedArray = Physics2D.OverlapCircleAll(strikePoint.position, currentLevelData.aoeRadius, thunderLightningSO.canAttackLayer);

        if (enemyStrikedArray != null) {

            foreach (Collider2D enemy in enemyStrikedArray) {

                if (enemy.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
                    baseEnemy.HitDamage(currentLevelData.damage);
                }

            }

        }

        yield return new WaitForSeconds(0.45f); // Wait for end of Strike Anim

        yield return new WaitForSeconds(0.6f); // Wait fore end of Strike sfx

        Destroy(this.gameObject);
    }

    public void UpdateDataWithLevel() {

        IAbility.AbilityLevel currentLevel = SaveData.GetAbilityStatusByType(thunderLightningSO.abilityType).currentLevel;

        currentLevelData = SaveData.GetAbilityLevelDataByLevelAndType(currentLevel, thunderLightningSO.abilityType);
    }

    private void OnDrawGizmos() {

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawSphere(strikePoint.position, thunderLightningSO.abilityLevelDataList[0].aoeRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(strikePoint.position, thunderLightningSO.abilityLevelDataList[0].aoeRadius);
    }
}
