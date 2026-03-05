using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherVisual : MonoBehaviour
{
    [SerializeField] private Archer archer;

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

        archer.OnAttackAnim += Archer_OnAttackAnim;
    }

    private void OnDestroy() {

        archer.OnAttackAnim -= Archer_OnAttackAnim;
    }

    private void Archer_OnAttackAnim(object sender, System.EventArgs e) {
        animator.SetTrigger(TRIGGER_ATTACK);
    }

    private void Update() {

        UpdateVisual();

    }

    private void UpdateVisual() {

        isIdle = archer.GetArcherState() == Archer.ArcherBehavior.Idle;
        animator.SetBool(IS_IDLE, isIdle);

        isPreAttack = archer.GetArcherState() == Archer.ArcherBehavior.PreAttack;
        animator.SetBool(IS_PREATTACK, isPreAttack);

    }
}
