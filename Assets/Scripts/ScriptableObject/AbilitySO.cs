using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AbilitySO : ScriptableObject
{
    [Header("Ability Info")]
    public IAbility.AbilityType abilityType;
    public string abilityName;
    public string description;

    [Header("Prefab")]
    public Transform prefab;
    public Transform iconPrefab;

    [Header("Layer")]
    public LayerMask canAttackLayer;
    
    [Header("Base Data")]
    public float price;

    [Header("Level Data")]
    public List<AbilityLevelData> abilityLevelDataList;

}

[System.Serializable]
public class AbilityLevelData {

    public IAbility.AbilityLevel level;

    public int pointUnlock;

    public float damage;
    public float aoeRadius;
    public float cdTimer;

    // Specific Data
    [SerializeReference] public AbilitySpecificData specificData;

}
public abstract class AbilitySpecificData { }

[System.Serializable]
public class SpikeTrapSpecificData : AbilitySpecificData
{
    public int strikeCount;
}
