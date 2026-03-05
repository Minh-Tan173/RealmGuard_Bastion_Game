using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeVisual : MonoBehaviour
{
    [SerializeField] private Slime slime;

    #region Up
    private const string IS_WALK_UP = "IsWalkUp";
    private const string TRIGGER_DEATH_UP = "TriggerDeathUp";
    private const string TRIGGER_ANOMALY_UP = "TriggerAnomalyUp";
    #endregion

    #region Down
    private const string IS_WALK_DOWN = "IsWalkDown";
    private const string TRIGGER_DEATH_DOWN = "TriggerDeathDown";
    private const string TRIGGER_ANOMALY_DOWN = "TriggerAnomalyDown";
    #endregion

    #region Side
    private const string IS_WALK_SIDE = "IsWalkSide";
    private const string TRIGGER_DEATH_SIDE = "TriggerDeathSide";
    private const string TRIGGER_ANOMALY_SIDE = "TriggerAnomalySide";
    #endregion

    private const string WALK_DOWN_ANIM = "Walk_Down_Anim";

    private bool isWalkUp;
    private bool isWalkDown;
    private bool isWalkSide;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        slime.GetSlimeLifeControl().OnSpawn += SlimeVisual_OnSpawn;
        slime.GetSlimeLifeControl().OnDeathAnim += SlimeLifeControl_OnDeathAnim;
        slime.OnAnomalyAnim += Slime_OnAnomalyAnim;

        slime.ChangedLeftDir += Slime_ChangedLeftDir;
        slime.ChangedRightDir += Slime_ChangedRightDir;
    }

    private void OnDestroy() {
        slime.GetSlimeLifeControl().OnSpawn -= SlimeVisual_OnSpawn;
        slime.GetSlimeLifeControl().OnDeathAnim -= SlimeLifeControl_OnDeathAnim;
        slime.OnAnomalyAnim -= Slime_OnAnomalyAnim;

        slime.ChangedLeftDir -= Slime_ChangedLeftDir;
        slime.ChangedRightDir -= Slime_ChangedRightDir;
    }

    private void Slime_ChangedRightDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = true;
    }

    private void Slime_ChangedLeftDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = false;
    }


    private void Slime_OnAnomalyAnim(object sender, System.EventArgs e) {

        BaseEnemy.EnemyDirection slimeDirection = slime.GetCurrentSlimeDirection();

        if (slimeDirection == BaseEnemy.EnemyDirection.Up) {
            animator.SetTrigger(TRIGGER_ANOMALY_UP);
        }

        if (slimeDirection == BaseEnemy.EnemyDirection.Down) {
            animator.SetTrigger(TRIGGER_ANOMALY_DOWN);
        }
        if (slimeDirection == BaseEnemy.EnemyDirection.Left || slimeDirection == BaseEnemy.EnemyDirection.Right) {
            animator.SetTrigger(TRIGGER_ANOMALY_SIDE);
        }
    }

    private void SlimeVisual_OnSpawn(object sender, System.EventArgs e) {
        // ResetAnimator
        animator.Rebind();
        animator.Play(WALK_DOWN_ANIM);
        animator.Update(0f);
    }

    private void SlimeLifeControl_OnDeathAnim(object sender, System.EventArgs e) {

        BaseEnemy.EnemyDirection slimeDirection = slime.GetCurrentSlimeDirection();

        if (slimeDirection == BaseEnemy.EnemyDirection.Up) {
            animator.SetTrigger(TRIGGER_DEATH_UP);
        }

        if (slimeDirection == BaseEnemy.EnemyDirection.Down) {
            animator.SetTrigger(TRIGGER_DEATH_DOWN);
        }

        if (slimeDirection == BaseEnemy.EnemyDirection.Left || slimeDirection == BaseEnemy.EnemyDirection.Right) {
            animator.SetTrigger(TRIGGER_DEATH_SIDE);
        }

    }

    private void Update() {

        UpdateVisual();
    }

    private void UpdateVisual() {

        isWalkUp = slime.CanMove() && slime.GetCurrentSlimeDirection() == BaseEnemy.EnemyDirection.Up;
        animator.SetBool(IS_WALK_UP, isWalkUp);

        isWalkDown = slime.CanMove() && slime.GetCurrentSlimeDirection() == BaseEnemy.EnemyDirection.Down;
        animator.SetBool(IS_WALK_DOWN, isWalkDown);

        isWalkSide = slime.CanMove() && slime.GetCurrentSlimeDirection() == BaseEnemy.EnemyDirection.Left;
        animator.SetBool(IS_WALK_SIDE, isWalkSide);

        isWalkSide = slime.CanMove() && slime.GetCurrentSlimeDirection() == BaseEnemy.EnemyDirection.Right;
        animator.SetBool(IS_WALK_SIDE, isWalkSide);
    }
}
