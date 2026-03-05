using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MouseVisual;

public class MagicBoltVisual : MonoBehaviour
{
    [SerializeField] private MagicBolt magicBolt;
    [SerializeField] private List<MagicBoltAnimation> magicBoltAnimationList;

    private MagicBoltAnimation currentMagicBoltAnimation;
    private int currentFrame;
    private float frameTimer;
    private int frameCount;

    private SpriteRenderer spriteRenderer;

    private void Awake() {

        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    private void Start() {

        magicBolt.GetMageTower().OnChangeMageTowerType += MagicBoltVisual_OnChangeMageTowerType;
        
        SetMagicBoltAnimation(MageTowerSO.MageType.Ground);

    }

    private void OnDestroy() {

        magicBolt.GetMageTower().OnChangeMageTowerType -= MagicBoltVisual_OnChangeMageTowerType;

    }

    private void MagicBoltVisual_OnChangeMageTowerType(object sender, System.EventArgs e) {

        SetMagicBoltAnimation(magicBolt.GetMageTower().GetCurrentMageType());

    }

    private void Update() {

        frameTimer -= Time.deltaTime;

        if (frameTimer <= 0f) {

            frameTimer += currentMagicBoltAnimation.frameRate;
            currentFrame = (currentFrame + 1) % frameCount;

            spriteRenderer.sprite = currentMagicBoltAnimation.spriteArray[currentFrame];
        }
    }

    private void SetMagicBoltAnimation(MageTowerSO.MageType magicBoltType) {

        foreach (MagicBoltAnimation magicBoltAnimation in magicBoltAnimationList) {

            if (magicBoltAnimation.magicBoltType == magicBoltType) {

                currentFrame = 0;
                frameTimer = magicBoltAnimation.frameRate;
                frameCount = magicBoltAnimation.spriteArray.Length;
                currentMagicBoltAnimation = magicBoltAnimation;
            }
        }

    }

}

[System.Serializable]
public class MagicBoltAnimation {
    public Sprite[] spriteArray;
    public float frameRate;
    public MageTowerSO.MageType magicBoltType;
}
