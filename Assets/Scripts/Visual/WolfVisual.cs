using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WolfVisual : MonoBehaviour
{
    [SerializeField] private Wolf wolf;

    #region Up
    private const string IS_WALK_UP = "IsWalkUp";
    private const string TRIGGER_DEATH_UP = "TriggerDeathUp";
    private const string TRIGGER_PREATTACK_UP = "TriggerPreAttackUp";
    private const string TRIGGER_ATTACK_UP = "TriggerAttackUp";
    #endregion

    #region Down
    private const string IS_WALK_DOWN = "IsWalkDown";
    private const string TRIGGER_DEATH_DOWN = "TriggerDeathDown";
    private const string TRIGGER_PREATTACK_DOWN = "TriggerPreAttackDown";
    private const string TRIGGER_ATTACK_DOWN = "TriggerAttackDown";
    #endregion

    #region Side
    private const string IS_WALK_SIDE = "IsWalkSide";
    private const string TRIGGER_DEATH_SIDE = "TriggerDeathSide";
    private const string TRIGGER_PREATTACK_SIDE = "TriggerPreAttackSide";
    private const string TRIGGER_ATTACK_SIDE = "TriggerAttackSide";
    #endregion

    private const string WALK_DOWN_ANIM = "Walk_Down_Anim";

    #region Bool Animator control
    private bool isWalkUp;
    private bool isWalkDown;
    private bool isWalkSide;
    #endregion

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start() {

        wolf.GetWolfLifeControl().OnSpawn += WolfVisual_OnSpawn;
        wolf.GetWolfLifeControl().OnDeathAnim += WolfVisual_OnDeathAnim;

        wolf.OnPreAttackAnim += Wolf_OnPreAttackAnim;
        wolf.OnAttackAnim += Wolf_OnAttackAnim;
        wolf.ResetAnimator += Wolf_ResetAnimator;
        wolf.ChangedLeftDir += Wolf_ChangedLeftDir;
        wolf.ChangedRightDir += Wolf_ChangedRightDir;

    }

    private void OnDestroy() {

        wolf.GetWolfLifeControl().OnSpawn -= WolfVisual_OnSpawn;
        wolf.GetWolfLifeControl().OnDeathAnim -= WolfVisual_OnDeathAnim;

        wolf.OnPreAttackAnim -= Wolf_OnPreAttackAnim;
        wolf.OnAttackAnim -= Wolf_OnAttackAnim;
        wolf.ResetAnimator -= Wolf_ResetAnimator;
        wolf.ChangedLeftDir -= Wolf_ChangedLeftDir;
        wolf.ChangedRightDir -= Wolf_ChangedRightDir;
    }

    private void Wolf_ChangedRightDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = true;
    }

    private void Wolf_ChangedLeftDir(object sender, System.EventArgs e) {
        spriteRenderer.flipY = false;
    }

    private void WolfVisual_OnDeathAnim(object sender, System.EventArgs e) {

        SetTriggerByDirection(TRIGGER_DEATH_UP, TRIGGER_DEATH_DOWN, TRIGGER_DEATH_SIDE);

    }

    // LIFE CONTROL EVENT

    private void WolfVisual_OnSpawn(object sender, System.EventArgs e) {
        ResetAnimator();
    }

    // BEHAVIOR EVENT

    private void Wolf_ResetAnimator(object sender, System.EventArgs e) {
        ResetAnimator();    
    }

    private void Wolf_OnPreAttackAnim(object sender, System.EventArgs e) {

        SetTriggerByDirection(TRIGGER_PREATTACK_UP, TRIGGER_PREATTACK_DOWN, TRIGGER_PREATTACK_SIDE);

    }


    private void Wolf_OnAttackAnim(object sender, System.EventArgs e) {

        SetTriggerByDirection(TRIGGER_ATTACK_UP, TRIGGER_ATTACK_DOWN, TRIGGER_ATTACK_SIDE);

    }


    private void Update() {

        UpdateAnimator();

    }

    private void UpdateAnimator() {

        // ---- WALK ANIM ----

        if (wolf.IsWalking()) {

            // Up Direction
            isWalkUp = wolf.GetCurrentWolfDirection() == BaseEnemy.EnemyDirection.Up;
            animator.SetBool(IS_WALK_UP, isWalkUp);

            // Down Direction
            isWalkDown = wolf.GetCurrentWolfDirection() == BaseEnemy.EnemyDirection.Down;
            animator.SetBool(IS_WALK_DOWN, isWalkDown);

            // Side Direction
            isWalkSide = (wolf.GetCurrentWolfDirection() == BaseEnemy.EnemyDirection.Left || wolf.GetCurrentWolfDirection() == BaseEnemy.EnemyDirection.Right);
            animator.SetBool(IS_WALK_SIDE, isWalkSide);

        }

    }

    private void SetTriggerByDirection(string animUp, string animDown, string animSide) {

        if (wolf.GetCurrentWolfDirection() == BaseEnemy.EnemyDirection.Up) {

            animator.SetTrigger(animUp);
        }

        if (wolf.GetCurrentWolfDirection() == BaseEnemy.EnemyDirection.Down) {
            animator.SetTrigger(animDown);
        }

        if (wolf.GetCurrentWolfDirection() == BaseEnemy.EnemyDirection.Left || wolf.GetCurrentWolfDirection() == BaseEnemy.EnemyDirection.Right) {
            animator.SetTrigger(animSide);
        }

    }

    private void ResetAnimator() {

        animator.Rebind();
        animator.Play(WALK_DOWN_ANIM);
        animator.Update(0f);

    }

}
