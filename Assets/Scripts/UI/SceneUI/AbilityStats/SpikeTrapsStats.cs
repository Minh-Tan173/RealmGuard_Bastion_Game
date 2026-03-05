using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpikeTrapsStats : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private Transform parentUI;

    [Header("Ability Type")]
    [SerializeField] private IAbility.AbilityType abilityType;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI aoeRangeText;
    [SerializeField] private TextMeshProUGUI cdTimer;
    [SerializeField] private TextMeshProUGUI strikeCount;
    [SerializeField] private TextMeshProUGUI costText;

    private IHasAbilityStats hasAbilityStats;

    private void Awake() {


        if (parentUI == null) {
            // Không có parent ---> Là Stats UI trong Game

            UpdateVisual();
        }
        else {
            // Có Parent

            hasAbilityStats = parentUI.GetComponent<IHasAbilityStats>();

            if (hasAbilityStats == null) {

                Debug.LogError("Parent dont inherit script IHasAbilityStats");
                return;
            }

            hasAbilityStats.OnUpdateDataVisual += HasAbilityStats_OnUpdateDataVisual;
        }
    }

    private void OnDestroy() {

        if (parentUI != null) {
            hasAbilityStats.OnUpdateDataVisual -= HasAbilityStats_OnUpdateDataVisual;
        }

    }

    private void HasAbilityStats_OnUpdateDataVisual(object sender, IHasAbilityStats.OnUpdateVisualEventArgs e) {
        
        if (this.abilityType == e.abilityType) {

            UpdateVisual();

            this.gameObject.SetActive(true);

        }
        else {

            this.gameObject.SetActive(false);
        }

    }

    private void UpdateVisual() {

        AbilityStatus currentStatus = SaveData.GetAbilityStatusByType(this.abilityType);
        AbilitySO abilitySO = SaveData.GetAbilitySOByType(this.abilityType);
        AbilityLevelData currentData = SaveData.GetAbilityLevelDataByLevelAndType(currentStatus.currentLevel, this.abilityType);
        SpikeTrapSpecificData strikeTrapSpecificData = currentData.specificData as SpikeTrapSpecificData;

        if (abilityNameText != null) {
            abilityNameText.text = $"{abilitySO.abilityName}";
        }

        damageText.text = $"{currentData.damage}";
        aoeRangeText.text = $"{currentData.aoeRadius}";
        cdTimer.text = $"{currentData.cdTimer}";
        strikeCount.text = $"{strikeTrapSpecificData.strikeCount} times";
        costText.text = $"{abilitySO.price}$";
    }
}
