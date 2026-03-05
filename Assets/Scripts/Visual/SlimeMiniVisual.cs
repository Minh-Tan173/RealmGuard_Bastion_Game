using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMiniVisual : MonoBehaviour
{
    [SerializeField] private SlimeMini slimeMini;

    #region Up
    private const string IS_WALK_UP = "IsWalkUp";
    private const string TRIGGER_DEATH_UP = "TriggerDeathUp";
    #endregion

    #region Down
    private const string IS_WALK_DOWN = "IsWalkDown";
    private const string TRIGGER_DEATH_DOWN = "TriggerDeathDown";
    #endregion

    #region Side
    private const string IS_WALK_SIDE = "IsWalkSide";
    private const string TRIGGER_DEATH_SIDE = "TriggerDeathSide";
    #endregion

    private bool isWalkUp;
    private bool isWalkDown;
    private bool isWalkSide;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        slimeMini.GetSlimeMiniLifeControl().OnDeathAnim += SlimeMiniLifeControl_OnDeathAnim;

        slimeMini.ChangedLeftDir += SlimeMini_ChangedLeftDir;
        slimeMini.ChangedRightDir += SlimeMini_ChangedRightDir;
    }

    private void OnDestroy() {
        slimeMini.GetSlimeMiniLifeControl().OnDeathAnim -= SlimeMiniLifeControl_OnDeathAnim;

        slimeMini.ChangedLeftDir -= SlimeMini_ChangedLeftDir;
        slimeMini.ChangedRightDir -= SlimeMini_ChangedRightDir;
    }

    private void SlimeMini_ChangedRightDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = true;
    }

    private void SlimeMini_ChangedLeftDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = false;
    }

    private void SlimeMiniLifeControl_OnDeathAnim(object sender, System.EventArgs e) {

        BaseEnemy.EnemyDirection slimeDirection = slimeMini.GetCurrentSlimeMiniDirection();

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

        isWalkUp = slimeMini.CanMove() && slimeMini.GetCurrentSlimeMiniDirection() == BaseEnemy.EnemyDirection.Up;
        animator.SetBool(IS_WALK_UP, isWalkUp);

        isWalkDown = slimeMini.CanMove() && slimeMini.GetCurrentSlimeMiniDirection() == BaseEnemy.EnemyDirection.Down;
        animator.SetBool(IS_WALK_DOWN, isWalkDown);

        isWalkSide = slimeMini.CanMove() && slimeMini.GetCurrentSlimeMiniDirection() == BaseEnemy.EnemyDirection.Left;
        animator.SetBool(IS_WALK_SIDE, isWalkSide);

        isWalkSide = slimeMini.CanMove() && slimeMini.GetCurrentSlimeMiniDirection() == BaseEnemy.EnemyDirection.Right;
        animator.SetBool(IS_WALK_SIDE, isWalkSide);
    }
}
