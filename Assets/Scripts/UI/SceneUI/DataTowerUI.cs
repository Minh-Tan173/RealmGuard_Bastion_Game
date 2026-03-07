using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DataTowerUI : MonoBehaviour
{

    public event EventHandler<UpdateLevelDataEventArgs> UpdateLevelData;
    public class UpdateLevelDataEventArgs : EventArgs {
        public ITowerObject.TowerType towerType;
    }


    [Header("Button")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button nextTowerButton;
    [SerializeField] private Button backTowerButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button backLevelButton;    

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI nameTowerText;
    [SerializeField] private TextMeshProUGUI towerDescriptionText;
    [SerializeField] private TextMeshProUGUI towerPriceText;
    [SerializeField] private TextMeshProUGUI soldierPriceText;

    [Header("Main Hub Rect")]
    [SerializeField] private float startMainHubRect;
    [SerializeField] private RectTransform mainHubRect;

    [Header("Tower Data Rect")]
    [SerializeField] private float startTowerRectXPos;
    [SerializeField] private float endTowerRectXPos;
    [SerializeField] private RectTransform archerTowerRect;
    [SerializeField] private RectTransform guardianTowerRect;
    [SerializeField] private RectTransform mageTowerRect;
    [SerializeField] private RectTransform catapultTowerRect;

    [Header("Soldier Data Rect")]
    [SerializeField] private float startSoldierRectXPos;
    [SerializeField] private float endSoldierRectXPos;
    [SerializeField] private RectTransform archerRect;
    [SerializeField] private RectTransform guardianRect;
    [SerializeField] private RectTransform mageRect;
    [SerializeField] private RectTransform catapultRect;


    [Header("Tower Database")]
    [SerializeField] private List<TowerDictionary> towerDictionaryList;

    [Header("Soldier Image")]
    [SerializeField] private Image soldierImage;

    [Header("Swipe image")]
    [SerializeField] private RectTransform childTowerRect;
    [SerializeField] private RectTransform towerIconRect;
    [SerializeField] private float tweenTime;
    [SerializeField] private LeanTweenType tweenType;

    private bool isTWeening = false;

    private CanvasGroup canvasGroup;

    private TowerDictionary currentTowerDictionary;
    private int currentTowerIndex;
    private int currentLevelIndex;

    private int totalTower;
    private TowerSO currentTowerSO;
    private Vector3 targetPos;
    private Vector3 nextStep;

    #region TowerStats Rect and SoldierStats Rect control
    private RectTransform currentTowerStatsRect;
    private RectTransform currentSoldierStatsRect;
    #endregion

    private void Awake() {

        canvasGroup = GetComponent<CanvasGroup>();

        currentTowerIndex = 0;
        currentLevelIndex = 1;

        totalTower = towerDictionaryList.Count;

        float posY = childTowerRect.sizeDelta.y + towerIconRect.GetComponent<VerticalLayoutGroup>().spacing;

        nextStep = new Vector3(0f, posY, 0f);

        targetPos = nextStep * (towerDictionaryList.Count - 1);
        towerIconRect.anchoredPosition = new Vector2(targetPos.x, targetPos.y);



        closeButton.onClick.AddListener(() => {

            if (isTWeening) { return; }

            HideStatsHubUI();
        });

        nextTowerButton.onClick.AddListener(() => {

            if (isTWeening) { return; }

            if (currentTowerIndex < totalTower - 1) {

                currentTowerIndex += 1;
                targetPos -= nextStep;

                ChangeTowerDictionaryByIndex(currentTowerIndex);
                SwitchTowerData();

                towerIconRect.LeanMoveLocal(targetPos, tweenTime).setEase(tweenType);

            }

        });

        backTowerButton.onClick.AddListener(() => {

            if (isTWeening) { return; }

            if (currentTowerIndex > 0) {

                currentTowerIndex -= 1;
                targetPos += nextStep;

                ChangeTowerDictionaryByIndex(currentTowerIndex);
                SwitchTowerData();

                towerIconRect.LeanMoveLocal(targetPos, tweenTime).setEase(tweenType);

            }


        });

        nextLevelButton.onClick.AddListener(() => {

            int totalSprite = currentTowerDictionary.towerSpriteList.Count; 

            if (currentLevelIndex < totalSprite) {

                currentLevelIndex += 1;

                SwitchTowerLevelData();
            }

        });

        backLevelButton.onClick.AddListener(() => {

            if (currentLevelIndex > 1) {

                currentLevelIndex -= 1;


                SwitchTowerLevelData();
            }

        });
    }

    private void Start() {

        ChangeTowerDictionaryByIndex(currentTowerIndex);

        ShowStatsHubUI();

    }

    private void ShowStatsHubUI() {

        // 1. Preprocessing
        canvasGroup.interactable = false;
        HideTowerStatsRect();
        HideSoldierStatsRect();

        mainHubRect.anchoredPosition = new Vector2(startMainHubRect, 0f);

        // 2. First Setup Sequence
        float showDuration = 0.3f;
        Sequence showSequence = DOTween.Sequence();

        mainHubRect.DOKill();

        showSequence.Append(mainHubRect.DOAnchorPosX(Vector3.zero.x, showDuration).SetEase(Ease.OutBack));

        showSequence.OnComplete(() => {

            canvasGroup.interactable = true;
            SwitchTowerData();
        });

    }

    private void HideStatsHubUI() {

        float hideDuration = 0.5f;
        float hideStatsUIDuration = 0.2f;
        Sequence hideSequence = DOTween.Sequence();

        hideSequence.Append(currentTowerStatsRect.DOAnchorPosX(startTowerRectXPos, hideStatsUIDuration).SetEase(Ease.InBack));
        hideSequence.Join(currentSoldierStatsRect.DOAnchorPosX(startSoldierRectXPos, hideStatsUIDuration).SetEase(Ease.InBack));

        hideSequence.AppendCallback(() => {

            currentTowerStatsRect.gameObject.SetActive(false);
            currentSoldierStatsRect.gameObject.SetActive(false);
        });

        hideSequence.Append(mainHubRect.DOAnchorPosX(startMainHubRect, hideDuration).SetEase(Ease.InBack));

        hideSequence.OnComplete(() => {
            Destroy(this.gameObject);
        });
    }

    private void ChangeTowerDictionaryByIndex(int currentIndex) {

        currentTowerDictionary = towerDictionaryList[currentIndex];

        currentLevelIndex = 1; // Mặc định khi đổi sang data về Tower khác thì bắt đầu từ level 1 (index 1)

    }

    private TowerDictionary.TowerSprite GetSpriteByTowerLevel(ITowerObject.LevelTower levelTower) {

        foreach (TowerDictionary.TowerSprite towerSprite in currentTowerDictionary.towerSpriteList) {

            if (towerSprite.LevelTower == levelTower) {
                // Tìm đúng level tương ứng
                return towerSprite;
            }

        }

        return new TowerDictionary.TowerSprite();
    }

    private void SwitchTowerData() {

        // 1. Update Title and Description
        nameTowerText.text = currentTowerDictionary.towerSO.nameTower;

        Sequence descriptionSequence = DOTween.Sequence();
        towerDescriptionText.text = "";

        string fullText = $"{currentTowerDictionary.towerSO.description}";

        descriptionSequence.Append(DOTween.To(() => towerDescriptionText.text, x => towerDescriptionText.text = x, fullText, 0.2f).SetEase(Ease.Linear));


        // 2. Update Sprite
        TowerDictionary.TowerSprite currentTowerSprite = GetSpriteByTowerLevel((ITowerObject.LevelTower)currentLevelIndex);

        currentTowerDictionary.towerImage.sprite = currentTowerSprite.towerSprite;

        if (currentTowerSprite.soldierSprite != null) {
            // If this level has new sprite of Soldier --> Update sprite

            soldierImage.sprite = currentTowerSprite.soldierSprite;
        }

        SwitchStatsUI(currentTowerDictionary.towerType);
    }

    private void SwitchTowerLevelData() {

        TowerDictionary.TowerSprite currentTowerSprite = GetSpriteByTowerLevel((ITowerObject.LevelTower)currentLevelIndex);

        currentTowerDictionary.towerImage.sprite = currentTowerSprite.towerSprite;

        if (currentTowerSprite.soldierSprite != null) {
            // If this level has new sprite of Soldier --> Update sprite

            soldierImage.sprite = currentTowerSprite.soldierSprite;
        }

        // 2. Update Data by Type
        SwitchStatsUI(currentTowerDictionary.towerType);

    }

    private void SwitchStatsUI(ITowerObject.TowerType towerType) {


        isTWeening = true;

        RectTransform newTowerRect = null;
        RectTransform newSoldierRect = null;   
        
        // 1.Update Tower Stats
        if (towerType == ITowerObject.TowerType.ArcherTower) {

            newTowerRect = archerTowerRect;
            newSoldierRect = archerRect;

            ArcherTowerSO archerTowerSO = currentTowerDictionary.towerSO as ArcherTowerSO;

            // Update Price Text
            towerPriceText.text = $"{archerTowerSO.price}$";
            soldierPriceText.text = $"{archerTowerSO.priceArcher}$";
        }

        if (towerType == ITowerObject.TowerType.GuardianTower) {

            newTowerRect = guardianTowerRect;
            newSoldierRect = guardianRect;

            GuardianTowerSO guardianTowerSO = currentTowerDictionary.towerSO as GuardianTowerSO;

            // Update Price Text
            towerPriceText.text = $"{guardianTowerSO.price}$";
            soldierPriceText.text = $"Default";
        }

        if (towerType == ITowerObject.TowerType.MageTower) {

            newTowerRect = mageTowerRect;
            newSoldierRect = mageRect;

            MageTowerSO mageTowerSO = currentTowerDictionary.towerSO as MageTowerSO;

            // Update Price Text
            towerPriceText.text = $"{mageTowerSO.price}$";
            soldierPriceText.text = $"{mageTowerSO.priceMage}$";
        }

        if (towerType == ITowerObject.TowerType.CatapultTower) {

            newTowerRect = catapultTowerRect;
            newSoldierRect = catapultRect;

            CatapultTowerSO catapultTowerSO = currentTowerDictionary.towerSO as CatapultTowerSO;

            // Update Price Text
            towerPriceText.text = $"{catapultTowerSO.price}$";
            soldierPriceText.text = $"Default";
        }

        Sequence statstUIShowSequence = DOTween.Sequence();
        float duration = 0.3f;

        if (currentTowerStatsRect != null && currentTowerStatsRect.gameObject.activeSelf) {
            // If have old Stats Rect before ---> Hide anim

            statstUIShowSequence.Append(currentTowerStatsRect.DOAnchorPosX(startTowerRectXPos, duration).SetEase(Ease.InBack));
            statstUIShowSequence.Join(currentSoldierStatsRect.DOAnchorPosX(startSoldierRectXPos, duration).SetEase(Ease.InBack));

            RectTransform oldTowerRect = currentTowerStatsRect;
            RectTransform oldSoldierRect = currentSoldierStatsRect;

            statstUIShowSequence.AppendCallback(() => {
                oldTowerRect.gameObject.SetActive(false);
                oldSoldierRect.gameObject.SetActive(false);
            });
        }

        currentTowerStatsRect = newTowerRect;
        currentSoldierStatsRect = newSoldierRect;

        statstUIShowSequence.AppendCallback(() => {

            // 1. Update data base on current type
            UpdateLevelData?.Invoke(this, new UpdateLevelDataEventArgs { towerType = towerType });

            // 2. Update visual rect
            currentTowerStatsRect.gameObject.SetActive(true);
            currentSoldierStatsRect.gameObject.SetActive(true);
        });

        statstUIShowSequence.Append(currentTowerStatsRect.DOAnchorPosX(endTowerRectXPos, duration).SetEase(Ease.OutBack));
        statstUIShowSequence.Join(currentSoldierStatsRect.DOAnchorPosX(endSoldierRectXPos, duration).SetEase(Ease.OutBack));

        statstUIShowSequence.OnComplete(() => {

            isTWeening = false;
        });

    }

    private void HideTowerStatsRect() {

        archerTowerRect.anchoredPosition = new Vector2(startTowerRectXPos, archerTowerRect.anchoredPosition.y);
        guardianTowerRect.anchoredPosition = new Vector2(startTowerRectXPos, guardianTowerRect.anchoredPosition.y);
        mageTowerRect.anchoredPosition = new Vector2(startTowerRectXPos, mageTowerRect.anchoredPosition.y);
        catapultTowerRect.anchoredPosition = new Vector2(startTowerRectXPos, catapultTowerRect.anchoredPosition.y);

        archerTowerRect.gameObject.SetActive(false);
        guardianTowerRect.gameObject.SetActive(false);
        mageTowerRect.gameObject.SetActive(false);
        catapultTowerRect.gameObject.SetActive(false);
    }

    private void HideSoldierStatsRect() {

        archerRect.anchoredPosition = new Vector2(startSoldierRectXPos, archerRect.anchoredPosition.y);
        guardianRect.anchoredPosition = new Vector2(startSoldierRectXPos, guardianRect.anchoredPosition.y);
        mageRect.anchoredPosition = new Vector2(startSoldierRectXPos, mageRect.anchoredPosition.y);
        catapultRect.anchoredPosition = new Vector2(startSoldierRectXPos, catapultRect.anchoredPosition.y);

        archerRect.gameObject.SetActive(false);
        guardianRect.gameObject.SetActive(false);
        mageRect.gameObject.SetActive(false);
        catapultRect.gameObject.SetActive(false);
    }

    public TowerDictionary GetCurrentTowerDictionary() {
        return this.currentTowerDictionary;
    }

    public ITowerObject.LevelTower GetCurrentLevelIndex() {
        return (ITowerObject.LevelTower)currentLevelIndex;
    }
}

[System.Serializable]
public class TowerDictionary {

    [System.Serializable]
    public struct TowerSprite {
        public ITowerObject.LevelTower LevelTower;
        public Sprite towerSprite;
        public Sprite soldierSprite;
    }

    [Header("Tower Data")]
    public ITowerObject.TowerType towerType;
    public TowerSO towerSO;
    public List<TowerSprite> towerSpriteList;

    [Header("Tower Data Visual By Level")]
    public Image towerImage;

}
