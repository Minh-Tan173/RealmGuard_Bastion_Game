using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{

    public enum EnemyLifeState {

        Alive,
        Death,
        Despawn
    }

    public enum EnemyDirection {

        Up,
        Down,
        Left,
        Right

    }

    public static event EventHandler OnActiveEnemy;
    public static event EventHandler UnActiveEnemy;


    public void OnDisable() {
        // When enemy inherit was Hide or Destroy

        UnActiveEnemy?.Invoke(this, EventArgs.Empty);
    }

    public void ActiveEvent() {

        OnActiveEnemy?.Invoke(this, EventArgs.Empty);
    }

    public virtual void HitDamage(float damageGet) {
        Debug.LogError("Trigger baseEnemy");
    }

    public virtual bool IsCantAttack() {
        Debug.LogError("Trigger baseEnemy");
        return true;
    }

    public virtual Vector3 GetEnemyVelocity() {
        Debug.LogError("Trigger baseEnemy");    
        return Vector3.zero;
    }

    public virtual float GetEnemyProgress() {
        Debug.LogError("Trigger baseEnemy");
        return 0f;
    }

    public virtual float GetEnemyHealth() {
        Debug.LogError("Trigger baseEnemy");
        return 0f;
    }

    public virtual EnemyDirection GetEnemyCurrentDirection() {
        Debug.LogError("Trigger baseEnemy");
        return EnemyDirection.Down;
    }
    
    public static BaseEnemy SpawnEnemy(EnemySO enemySO, Transform parent) {

        Transform enemyTransform = Instantiate(enemySO.prefab, parent);

        enemyTransform.localPosition = Vector3.zero;

        BaseEnemy baseEnemy = enemyTransform.GetComponent<BaseEnemy>();

        return baseEnemy;

    }

}
