using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianVisual : MonoBehaviour
{
    [SerializeField] private Guardian guardian;

    #region Up Direction
    private const string IS_IDLE_UP = "IsIdleUp";
    private const string IS_WALK_UP = "IsWalkingUp";
    private const string TRIGGER_ATTACK_UP = "TriggerAttackUp";
    private const string TRIGGER_DEATH_UP = "TriggerDeathUp";
    #endregion

    #region Side Direction
    private const string IS_IDLE_SIDE = "IsIdleSide";
    private const string IS_WALK_SIDE = "IsWalkingSide";
    private const string TRIGGER_ATTACK_SIDE = "TriggerAttackSide";
    private const string TRIGGER_DEATH_SIDE = "TriggerDeathSide";
    #endregion

    #region Down Direction
    private const string IS_IDLE_DOWN = "IsIdleDown";
    private const string IS_WALK_DOWN = "IsWalkingDown";
    private const string TRIGGER_ATTACK_DOWN = "TriggerAttackDown";
    private const string TRIGGER_DEATH_DOWN = "TriggerDeathDown";
    #endregion

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private const string DOWN_IDLE_ANIM = "Down_Idle_Anim";

    #region Idle Bool Value
    private bool isIdleUp;
    private bool isIdleSide;
    private bool isIdleDown;
    #endregion

    #region Idle Bool Value
    private bool isWalkingUp;
    private bool isWalkingSide;
    private bool isWalkingDown;
    #endregion

    private void Awake() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start() {

        guardian.OnResetAnimator += Guardian_OnResetAnimator;
        guardian.OnAttackAnim += Guardian_OnAttackAnim;
        guardian.ChangedLeftDir += Guardian_ChangedLeftDir;
        guardian.ChangedRightDir += Guardian_ChangedRightDir;

        guardian.GetGuardianLifeControl().OnDeathAnim += GuardianVisual_OnDeathAnim;

    }


    private void OnDestroy() {

        guardian.OnResetAnimator -= Guardian_OnResetAnimator;
        guardian.OnAttackAnim -= Guardian_OnAttackAnim;
        guardian.ChangedLeftDir -= Guardian_ChangedLeftDir;
        guardian.ChangedRightDir -= Guardian_ChangedRightDir;

        guardian.GetGuardianLifeControl().OnDeathAnim -= GuardianVisual_OnDeathAnim;
    }

    private void Guardian_OnResetAnimator(object sender, System.EventArgs e) {
        ResetAnimator();
    }

    private void Guardian_ChangedRightDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = true;
    }

    private void Guardian_ChangedLeftDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = false;
    }

    private void GuardianVisual_OnDeathAnim(object sender, System.EventArgs e) {

        SetTriggerByDirection(TRIGGER_DEATH_UP, TRIGGER_DEATH_DOWN, TRIGGER_DEATH_SIDE);
    }

    private void Guardian_OnAttackAnim(object sender, System.EventArgs e) {

        SetTriggerByDirection(TRIGGER_ATTACK_UP, TRIGGER_ATTACK_DOWN, TRIGGER_ATTACK_SIDE);
    }

    private void Update() {

        UpdateVisual();

    }

    private void UpdateVisual() {

        Guardian.GuadianState currentState = guardian.GetCurrentGuadianState();
        SoldierSO.SoldierDirection currentDirection = guardian.GetActiveFacingDir();

        if (currentState == Guardian.GuadianState.Attack) {
            return;
        }

        // Idle animation
        isIdleUp = !guardian.IsWalking() && currentDirection == SoldierSO.SoldierDirection.Up;
        animator.SetBool(IS_IDLE_UP, isIdleUp);

        isIdleSide = !guardian.IsWalking() && (currentDirection == SoldierSO.SoldierDirection.Left || currentDirection == SoldierSO.SoldierDirection.Right);
        animator.SetBool(IS_IDLE_SIDE, isIdleSide);

        isIdleDown = !guardian.IsWalking() && currentDirection == SoldierSO.SoldierDirection.Down;
        animator.SetBool(IS_IDLE_DOWN, isIdleDown);

        // Walking animation
        isWalkingUp = guardian.IsWalking() && currentDirection == SoldierSO.SoldierDirection.Up;
        animator.SetBool(IS_WALK_UP, isWalkingUp);

        isWalkingSide = guardian.IsWalking() && (currentDirection == SoldierSO.SoldierDirection.Left || currentDirection == SoldierSO.SoldierDirection.Right);
        animator.SetBool(IS_WALK_SIDE, isWalkingSide);

        isWalkingDown = guardian.IsWalking() && currentDirection == SoldierSO.SoldierDirection.Down;
        animator.SetBool(IS_WALK_DOWN, isWalkingDown);

    }

    private void SetTriggerByDirection(string animUp, string animDown, string animSide) {

        if (guardian.GetActiveFacingDir() == SoldierSO.SoldierDirection.Up) {

            animator.SetTrigger(animUp);
        }

        if (guardian.GetActiveFacingDir() == SoldierSO.SoldierDirection.Down) {
            animator.SetTrigger(animDown);
        }

        if (guardian.GetActiveFacingDir() == SoldierSO.SoldierDirection.Left || guardian.GetActiveFacingDir() == SoldierSO.SoldierDirection.Right) {
            animator.SetTrigger(animSide);
        }

    }

    private void ResetAnimator() {

        animator.Rebind();
        animator.Play(DOWN_IDLE_ANIM);
        animator.Update(0f);

    }

}

