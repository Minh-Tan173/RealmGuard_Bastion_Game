using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeVisual : MonoBehaviour
{
    [SerializeField] private SpikeTrap spike;
    [SerializeField] private List<SpikeAnimatorData> spikeAnimatorDataList;

    private const string SPIKE_ATTACK_LEVEL1_ANIM = "Spike_Attack_Level1_Anim";
    private const string SPIKE_ATTACK_LEVEL2_ANIM = "Spike_Attack_Level2_Anim";
    private const string SPIKE_ATTACK_LEVEL3_ANIM = "Spike_Attack_Level3_Anim";

    private Animator animator;
    private IAbility.AbilityLevel currentLevel;


    private void Awake() {

        animator = GetComponent<Animator>();

        
    }

    private void Start() {

        AbilityStatus abilityStatus = SaveData.GetAbilityStatusByType(spike.GetAbilitySO().abilityType);

        foreach (SpikeAnimatorData spikeAnimatorData in spikeAnimatorDataList) {

            if (spikeAnimatorData.currentLevel == abilityStatus.currentLevel) {
                currentLevel = abilityStatus.currentLevel;
                animator.runtimeAnimatorController = spikeAnimatorData.currentAnimator;
            }

        }

        spike.OnResetAnimator += Spike_OnResetAnimator;
    }

    private void OnDestroy() {
        spike.OnResetAnimator -= Spike_OnResetAnimator;
    }

    private void Spike_OnResetAnimator(object sender, System.EventArgs e) {
        if (currentLevel == IAbility.AbilityLevel.Level1) {
            PlayAnim(SPIKE_ATTACK_LEVEL1_ANIM);
        }
        if (currentLevel == IAbility.AbilityLevel.Level2) {
            PlayAnim(SPIKE_ATTACK_LEVEL2_ANIM);
        }
        if (currentLevel == IAbility.AbilityLevel.Level3) {
            PlayAnim(SPIKE_ATTACK_LEVEL3_ANIM);
        }
    }

    private void PlayAnim(string animName) {
        animator.Rebind();
        animator.Play(animName);
        animator.Update(0f);
        
    }
}

[System.Serializable]
public class SpikeAnimatorData {
    public IAbility.AbilityLevel currentLevel;
    public RuntimeAnimatorController currentAnimator;
}
