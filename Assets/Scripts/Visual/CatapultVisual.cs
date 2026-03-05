using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultVisual : MonoBehaviour
{

    [SerializeField] private Catapult catapult;

    [Header("Catapult Animator")]
    [SerializeField] private List<CatapultAnimator> catapultAnimatorList;

    private const string TRIGGER_UP_IDLE = "TriggerUpIdle"; 
    private const string TRIGGER_UP_SIDE_IDLE = "TriggerUpSideIdle";
    private const string TRIGGER_SIDE_IDLE = "TriggerSideIdle";
    private const string TRIGGER_DOWN_SIDE_IDLE = "TriggerDownSideIdle";

    private const string TRIGGER_UP_ATTACK = "TriggerUpAttack";
    private const string TRIGGER_UP_SIDE_ATTACK = "TriggerUpSideAttack";
    private const string TRIGGER_SIDE_ATTACK = "TriggerSideAttack";
    private const string TRIGGER_DOWN_SIDE_ATTACK = "TriggerDownSideAttack";
    private const string TRIGGER_DOWN_ATTACK = "TriggerDownAttack";

    //private const string DOWN_IDLE_ANIM = "Down_Idle_Anim";

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private CatapultAnimator currentAnimator;

    private void Awake() {

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ChangeAnimatorTo(catapultAnimatorList[0]);
    }

    private void Start() {

        catapult.OnAttackAnim += Catapult_OnAttackAnim;
        catapult.OnIdleAnim += Catapult_OnIdleAnim;

        catapult.OnChangedCatapultLevel += Catapult_OnChangedCatapultLevel;
    }

    private void OnDestroy() {

        catapult.OnAttackAnim -= Catapult_OnAttackAnim;
        catapult.OnIdleAnim -= Catapult_OnIdleAnim;

        catapult.OnChangedCatapultLevel -= Catapult_OnChangedCatapultLevel;
    }

    private void Catapult_OnChangedCatapultLevel(object sender, System.EventArgs e) {

        
        foreach (CatapultAnimator catapultAnimator in catapultAnimatorList) {

            if (catapultAnimator.soldierLevel == catapult.GetCatapultLevel()) {
                // If this animator has same level with catapult

                ChangeAnimatorTo(catapultAnimator); 
            }
        }

    }

    private void Catapult_OnIdleAnim(object sender, System.EventArgs e) {

        Catapult.CatapultDirection currentDirection = catapult.GetCurrentCatapultDirection();

        if (currentDirection == Catapult.CatapultDirection.Up) {
            SetTriggerAnim(TRIGGER_UP_IDLE);
        }

        if (currentDirection == Catapult.CatapultDirection.UpRight) {
            SetTriggerAnim(TRIGGER_UP_SIDE_IDLE);
        }

        if (currentDirection == Catapult.CatapultDirection.Right) {
            SetTriggerAnim(TRIGGER_SIDE_IDLE);
        }

        if (currentDirection == Catapult.CatapultDirection.DownRight) {
            SetTriggerAnim(TRIGGER_DOWN_SIDE_IDLE);
        }

        if (currentDirection == Catapult.CatapultDirection.Down) {
            // Default Anim
            animator.Rebind();
            //animator.Play(DOWN_IDLE_ANIM);
            animator.Update(0f);
        }

        if (currentDirection == Catapult.CatapultDirection.DownLeft) {
            SetTriggerAnim(TRIGGER_DOWN_SIDE_IDLE, isFlipX: true);
        }

        if (currentDirection == Catapult.CatapultDirection.Left) {
            SetTriggerAnim(TRIGGER_SIDE_IDLE, isFlipX: true);

        }

        if (currentDirection == Catapult.CatapultDirection.UpLeft) {
            SetTriggerAnim(TRIGGER_UP_SIDE_IDLE, isFlipX: true);
        }

    }

    private void Catapult_OnAttackAnim(object sender, System.EventArgs e) {

        Catapult.CatapultDirection currentDirection = catapult.GetCurrentCatapultDirection();

        if (currentDirection == Catapult.CatapultDirection.Up) {
            SetTriggerAnim(TRIGGER_UP_ATTACK);
        }

        if (currentDirection == Catapult.CatapultDirection.UpRight) {
            SetTriggerAnim(TRIGGER_UP_SIDE_ATTACK);
        }

        if (currentDirection == Catapult.CatapultDirection.Right) {
            SetTriggerAnim(TRIGGER_SIDE_ATTACK);
        }

        if (currentDirection == Catapult.CatapultDirection.DownRight) {
            SetTriggerAnim(TRIGGER_DOWN_SIDE_ATTACK);
        }

        if (currentDirection == Catapult.CatapultDirection.Down) {
            SetTriggerAnim(TRIGGER_DOWN_ATTACK);
        }

        if (currentDirection == Catapult.CatapultDirection.DownLeft) {
            SetTriggerAnim(TRIGGER_DOWN_SIDE_ATTACK, isFlipX: true);
        }

        if (currentDirection == Catapult.CatapultDirection.Left) {
            SetTriggerAnim(TRIGGER_SIDE_ATTACK, isFlipX: true);

        }

        if (currentDirection == Catapult.CatapultDirection.UpLeft) {
            SetTriggerAnim(TRIGGER_UP_SIDE_ATTACK, isFlipX: true);
        }
    }

    private void Update() {

    }

    private void SetTriggerAnim(string nameAnim, bool isFlipX = false) {

        animator.SetTrigger(nameAnim);
        spriteRenderer.flipX = isFlipX;
    }

    private void ChangeAnimatorTo(CatapultAnimator catapultAnimator) {

        this.currentAnimator = catapultAnimator;

        animator.runtimeAnimatorController = currentAnimator.runtimeAnimator;

        animator.Rebind();
        animator.Update(0f);
    }
}

[System.Serializable]
public class CatapultAnimator {
    
    public SoldierSO.SoldierLevel soldierLevel;
    public RuntimeAnimatorController runtimeAnimator;
}




