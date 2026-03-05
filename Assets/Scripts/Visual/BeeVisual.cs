using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeVisual : MonoBehaviour
{
    [SerializeField] private Bee bee;

    // --- CONST STRING ---
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

    private const string WALK_DOWN_ANIM = "Walk_Down_Anim";

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    #region Bool Animator control
    private bool isWalkUp;
    private bool isWalkDown;
    private bool isWalkSide;
    #endregion


    private void Awake() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

    }

    private void Start() {

        bee.ChangedLeftDir += Bee_ChangedLeftDir;
        bee.ChangedRightDir += Bee_ChangedRightDir;

        bee.GetBeeLifeControl().OnDeathAnim += BeeVisual_OnDeathAnim;
        bee.GetBeeLifeControl().OnSpawn += BeeVisual_OnSpawn;

    }

    private void OnDestroy() {

        bee.ChangedLeftDir -= Bee_ChangedLeftDir;
        bee.ChangedRightDir -= Bee_ChangedRightDir;

        bee.GetBeeLifeControl().OnDeathAnim -= BeeVisual_OnDeathAnim;
        bee.GetBeeLifeControl().OnSpawn -= BeeVisual_OnSpawn;
    }

    private void Bee_ChangedRightDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = true;
    }

    private void Bee_ChangedLeftDir(object sender, System.EventArgs e) {
        spriteRenderer.flipX = false;
    }

    private void BeeVisual_OnSpawn(object sender, System.EventArgs e) {

        // ResetAnimator
        animator.Rebind();
        animator.Play(WALK_DOWN_ANIM);
        animator.Update(0f);
    }

    private void BeeVisual_OnDeathAnim(object sender, System.EventArgs e) {

        BaseEnemy.EnemyDirection beeDirection = bee.GetCurrentBeeDirection();

        if (beeDirection == BaseEnemy.EnemyDirection.Up) {
            animator.SetTrigger(TRIGGER_DEATH_UP);
        }

        if (beeDirection == BaseEnemy.EnemyDirection.Down) {
            animator.SetTrigger(TRIGGER_DEATH_DOWN);
        }

        if (beeDirection == BaseEnemy.EnemyDirection.Left || beeDirection == BaseEnemy.EnemyDirection.Right) {
            animator.SetTrigger(TRIGGER_DEATH_SIDE);
        }
    }


    private void Update() {

        UpdateAnimator();

    }

    private void UpdateAnimator() {

        // Up Direction
        isWalkUp = bee.GetCurrentBeeDirection() == BaseEnemy.EnemyDirection.Up;
        animator.SetBool(IS_WALK_UP, isWalkUp);

        // Down Direction
        isWalkDown = bee.GetCurrentBeeDirection() == BaseEnemy.EnemyDirection.Down;
        animator.SetBool(IS_WALK_DOWN, isWalkDown);

        // Side Direction
        isWalkSide = bee.GetCurrentBeeDirection() == BaseEnemy.EnemyDirection.Left || bee.GetCurrentBeeDirection() == BaseEnemy.EnemyDirection.Right;
        animator.SetBool(IS_WALK_SIDE, isWalkSide);

    }
}
