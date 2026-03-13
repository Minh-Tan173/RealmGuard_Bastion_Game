using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class EnemySO : ScriptableObject
{
    [Header("Tag")]
    public BaseEnemy.DamageResistance resistanceType;

    [Header("Description")]
    public string enemyName;
    public string description;
    public string anomalyDescription;

    [Header("Price")]
    public float enemyPrice;

    [Header("HP")]
    public float totalHealth;

    [Header("Movement Behavior")]
    public float moveSpeed;
    public float radiusOffset;

    [Header("Attack Behavior")]
    public float detectedZone;
    public float attackDamage;

    [Header("Layer")]
    public LayerMask homeBaseLayer;
    public LayerMask canAttackLayer;

    [Header("Timer")]
    public float deathTimer;
    public float randomTargetTimer;
    public float thinkingTimer;
    public float attackTimer;

    [Header("Prefab")]
    public Transform prefab;
    public Transform introductionUI;
}
