using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageVisual : MonoBehaviour
{
    [SerializeField] private Mage mage;

    private const string IS_IDLE = "IsIdle";
    private const string IS_PREATTACK = "IsPreAttack";
    private const string TRIGGER_ATTACK = "TriggerAttack";

    private Animator animator;

    private bool isIdle;
    private bool isPreAttack;


    private void Awake() {

        animator = GetComponent<Animator>();

    }

    private void Start() {
        mage.OnAttackAnim += Mage_OnAttackAnim;
    }

    private void OnDestroy() {
        mage.OnAttackAnim -= Mage_OnAttackAnim;
    }

    private void Mage_OnAttackAnim(object sender, System.EventArgs e) {
        animator.SetTrigger(TRIGGER_ATTACK);
    }

    private void Update() {

        UpdateVisual();

    }

    private void UpdateVisual() {

        isIdle = mage.GetMageState() == Mage.MageBehavior.Idle;
        animator.SetBool(IS_IDLE, isIdle);

        isPreAttack = mage.GetMageState() == Mage.MageBehavior.PreAttack;
        animator.SetBool(IS_PREATTACK, isPreAttack);

    }
}
