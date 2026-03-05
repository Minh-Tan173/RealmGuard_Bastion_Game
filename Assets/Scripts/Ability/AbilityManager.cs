using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }


    [SerializeField] private AbilityManagerSO abilityManagerSO;

    //public AbilitySO GetAbilitySOByType(IAbility.AbilityType abilityType) {

    //    if (abilityType == IAbility.AbilityType.ThunderLightning) {
    //        return abilityManagerSO.thunderLightningSO;
    //    }

    //    if (abilityType == IAbility.AbilityType.ExplosiveBarrel) {
    //        return abilityManagerSO.explosiveBarrelSO;
    //    }

    //    if (abilityType == IAbility.AbilityType.SpikeTrap) {
    //        return abilityManagerSO.spikeTrap;
    //    }

    //    return null;

    //}

    public IAbility.AbilityType GetAbilityType(AbilitySO abilitySO) {

        if (abilitySO.prefab.GetComponent<ThunderLightning>() != null) {
            return IAbility.AbilityType.ThunderLightning;
        }

        if (abilitySO.prefab.GetComponent<ExplosiveBarrel>() != null) {
            return IAbility.AbilityType.ExplosiveBarrel;
        }

        if (abilitySO.prefab.GetComponent<SpikeTrap>() != null) {
            return IAbility.AbilityType.SpikeTrap;
        }

        return IAbility.AbilityType.None;
    }

    //public AbilityLevelData GetAbilityLevelDataByType(IAbility.AbilityType abilityType) {

    //    IAbility.AbilityLevel currentLevel = SaveData.GetAbilityStatusByType(abilityType).currentLevel;
    //    AbilitySO currentAbilitySO = GetAbilitySOByType(abilityType);

    //    foreach(AbilityLevelData abilityLevelData in currentAbilitySO.abilityLevelDataList) {

    //        if (abilityLevelData.level == currentLevel) {

    //            return abilityLevelData;
    //        }

    //    }

    //    return null;
    //}

    public static void SpawnAbility(AbilitySO abilitySO, Vector3 spawnPos) {

        Transform abilityPrefab = abilitySO.prefab;

        Transform abilityTransform = Instantiate(abilityPrefab);
        abilityTransform.position = spawnPos;

    }
}
