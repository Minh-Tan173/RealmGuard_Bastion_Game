using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu()]
public class GameDataTemplateSO : ScriptableObject
{
    [Header("Map Saved")]
    public AssetReferenceT<TextAsset> mapCollection;

    [Header("Ability Data")]
    public List<AbilitySO> abilitySOList;

    [Header("List EnemySO")]
    public List<EnemySO> enemySOList;
}
    