using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcVisual : MonoBehaviour
{
    [SerializeField] private Orc orc;

    #region Up
    private const string IS_WALK_UP = "IsWalkUp";
    private const string TRIGGER_DEATH_UP = "TriggerDeathUp";
    private const string TRIGGER_ATTACK_UP = "TriggerAttackUp";
    #endregion

    #region Side
    private const string IS_WALK_SIDE = "IsWalkSide";
    private const string TRIGGER_DEATH_SIDE = "TriggerDeathSide";
    private const string TRIGGER_ATTACK_SIDE = "TriggerAttackSide";
    #endregion

    #region Down
    private const string IS_WALK_DOWN = "IsWalkDown";
    private const string TRIGGER_DEATH_DOWN = "TriggerDeathDown";
    private const string TRIGGER_ATTACK_DOWN = "TriggerAttackDown";
    #endregion

    private const string WALK_DOWN_ANIM = "Walk_Down_Anim";

    private const string STRIKE_ANIM = "Strike_Anim";

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isWalkUp;
    private bool isWalkSide;
    private bool isWalkDown;

    private void Awake() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start() {

        orc.OnResetAnim += Orc_OnResetAnim;
        orc.OnAttackAnim += Orc_OnAttackAnim;

        orc.ChangedLeftDir += Orc_ChangedLeftDir;
        orc.ChangedRightDir += Orc_ChangedRightDir;

        orc.GetOrcLifeControl().OnSpawn += OrcVisual_OnSpawn;
        orc.GetOrcLifeControl().OnDeathAnim += OrcVisual_OnDeathAnim;

    }

    private void OnDestroy() {

        orc.OnResetAnim -= Orc_OnResetAnim;
        orc.OnAttackAnim -= Orc_OnAttackAnim;

        orc.ChangedLeftDir -= Orc_ChangedLeftDir;
        orc.ChangedRightDir -= Orc_ChangedRightDir;

        orc.GetOrcLifeControl().OnSpawn -= OrcVisual_OnSpawn;
        orc.GetOrcLifeControl().OnDeathAnim -= OrcVisual_OnDeathAnim;

    }


    private void Orc_ChangedRightDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = true;
    }

    private void Orc_ChangedLeftDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = false;
    }

    private void Orc_OnResetAnim(object sender, System.EventArgs e) {
        ResetAnimator();
    }

    private void Orc_OnAttackAnim(object sender, System.EventArgs e) {

        SetTriggerByDirection(TRIGGER_ATTACK_UP, TRIGGER_ATTACK_DOWN, TRIGGER_ATTACK_SIDE);
    }


    private void OrcVisual_OnSpawn(object sender, System.EventArgs e) {
        ResetAnimator();
    }

    private void OrcVisual_OnDeathAnim(object sender, System.EventArgs e) {

        SetTriggerByDirection(TRIGGER_DEATH_UP, TRIGGER_DEATH_DOWN, TRIGGER_DEATH_SIDE);
    }

    private void Update() {

        UpdateVisual(); 
        
    }

    private void UpdateVisual() {

        if (orc.GetOrcLifeControl().GetCurrentOrcLifeState() != BaseEnemy.EnemyLifeState.Alive) {
      
            return;

        }

        if (orc.GetCurrentOrcBehavior() != Orc.OrcBehavior.Walk) {
            return;
        }

        BaseEnemy.EnemyDirection currentDirection = orc.GetEnemyCurrentDirection();

        isWalkUp = currentDirection == BaseEnemy.EnemyDirection.Up;
        animator.SetBool(IS_WALK_UP, isWalkUp);

        isWalkSide = currentDirection == BaseEnemy.EnemyDirection.Left || currentDirection == BaseEnemy.EnemyDirection.Right;
        animator.SetBool(IS_WALK_SIDE, isWalkSide);

        isWalkDown = currentDirection == BaseEnemy.EnemyDirection.Down;
        animator.SetBool(IS_WALK_DOWN, isWalkDown);
    }

    private void SetTriggerByDirection(string animUp, string animDown, string animSide) {

        if (orc.GetEnemyCurrentDirection() == BaseEnemy.EnemyDirection.Up) {

            animator.SetTrigger(animUp);
        }

        if (orc.GetEnemyCurrentDirection() == BaseEnemy.EnemyDirection.Down) {
            animator.SetTrigger(animDown);
        }

        if (orc.GetEnemyCurrentDirection() == BaseEnemy.EnemyDirection.Left || orc.GetEnemyCurrentDirection() == BaseEnemy.EnemyDirection.Right) {
            animator.SetTrigger(animSide);
        }

    }

    private void ResetAnimator() {

        animator.Rebind();
        animator.Play(WALK_DOWN_ANIM);
        animator.Update(0f);

    }
}
