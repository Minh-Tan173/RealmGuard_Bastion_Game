using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityHubUI : MonoBehaviour, IHasAbilityStats
{

    public event EventHandler OnScrollButtonSFX;
    public event EventHandler<IHasAbilityStats.OnUpdateVisualEventArgs> OnUpdateDataVisual;

    [Header("Ability Data")]
    [SerializeField] private AbilityManagerSO abilityManagerSO;

    [Header("Button")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button unlockButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI pointUsingText;
    [SerializeField] private TextMeshProUGUI currentLevelText;

    [Header("Image Ability")]
    [SerializeField] private Image[] imageAbilityArray;

    [Header("Swipe image")]
    [SerializeField] private RectTransform levelPagesRect;
    [SerializeField] private float tweenTime;
    [SerializeField] private LeanTweenType tweenType;

    [Header("Panel")]
    [SerializeField] private RectTransform mainHubRect;
    [SerializeField] private RectTransform statusPanelRect;
    [SerializeField] private RectTransform upgradeLevelPanelRect;

    [Header("DOTween")]
    [SerializeField] private Vector3 startPosMainHub;
    [SerializeField] private Vector3 endPosMainHub;
    [SerializeField] private Vector3 startPosStatusPanel;
    [SerializeField] private Vector3 endPosStatusPanel;
    [SerializeField] private Vector3 startPosUpgradeLevelPanel;
    [SerializeField] private Vector3 endPosUpgradeLevelPanel;
    [SerializeField] private float durationOut;
    [SerializeField] private float durationIn;
    [SerializeField] private float overShoot;

    private CanvasGroup canvasGroup;

    private Vector3 pageStep;
    private int currentPage;
    private Vector3 targetPos;
    private int numberOfPage;

    public Dictionary<int, AbilityStatus> abilityDict;
    private int currentID = 1;

    private AbilitySO currentSO;
    private AbilityStatus currentAbilityStats;

    private void Awake() {

        canvasGroup = GetComponent<CanvasGroup>();

        abilityDict = new Dictionary<int, AbilityStatus>();

        currentPage = 1;

        targetPos = levelPagesRect.localPosition;

        float posX = -(imageAbilityArray[0].GetComponent<RectTransform>().sizeDelta.x + levelPagesRect.GetComponent<HorizontalLayoutGroup>().spacing);

        pageStep = new Vector3(posX, 0f, 0f);

        numberOfPage = imageAbilityArray.Length;

        nextButton.onClick.AddListener(() => {

            if (currentPage < numberOfPage) {
                
                currentPage += 1;
                targetPos += pageStep;

                MovePage();
            }

            OnScrollButtonSFX?.Invoke(this, EventArgs.Empty);

        });

        backButton.onClick.AddListener(() => {

            if (currentPage > 1) {
                
                currentPage -= 1;
                targetPos -= pageStep;


                MovePage();
            }

            OnScrollButtonSFX?.Invoke(this, EventArgs.Empty);

        });

        closeButton.onClick.AddListener(() => {
            Despawn();
        });

        unlockButton.onClick.AddListener(() => {
            // Mặc định unlock ở level 1

            int totalPoint = CampaignMapUI.Instance.GetTotalPoint();
            int pointDecrease = SaveData.GetAbilityLevelDataByLevelAndType(currentAbilityStats.currentLevel, currentAbilityStats.abilityType).pointUnlock;

            if (totalPoint >= pointDecrease) {

                totalPoint -= pointDecrease;
                CampaignMapUI.Instance.SetTotalPoint(totalPoint);

                SaveData.UnLockAbility(currentAbilityStats.abilityType);

                // 1. Ẩn nút Unlock ngay lập tức
                unlockButton.gameObject.SetActive(false);

                // 2. Update Data
                currentLevelText.text = $"Level {(int)currentAbilityStats.currentLevel}";
                statusPanelRect.gameObject.SetActive(true);

                OnUpdateDataVisual?.Invoke(this, new IHasAbilityStats.OnUpdateVisualEventArgs { abilityType = currentAbilityStats.abilityType }); // Update Stats

                Sequence unlockSequence = DOTween.Sequence();

                unlockSequence.Append(statusPanelRect.DOAnchorPos(endPosStatusPanel, durationOut).SetEase(Ease.OutBack, overShoot));

                if (CanUpgrade()) {

                    unlockSequence.AppendCallback(() => {

                        pointUsingText.text = $"{SaveData.GetAbilityLevelDataByLevelAndType((IAbility.AbilityLevel)((int)currentAbilityStats.currentLevel + 1), currentAbilityStats.abilityType).pointUnlock}P";

                        upgradeLevelPanelRect.gameObject.SetActive(true);
                    });

                    unlockSequence.Append(upgradeLevelPanelRect.DOAnchorPos(endPosUpgradeLevelPanel, durationOut).SetEase(Ease.OutBack, overShoot));
                }

            }
        });

        upgradeButton.onClick.AddListener(() => {

            int pointDecrease = SaveData.GetAbilityLevelDataByLevelAndType((IAbility.AbilityLevel)((int)currentAbilityStats.currentLevel + 1), currentAbilityStats.abilityType).pointUnlock; // Lấy giá trị pointUnlock của level kế tiếp
            int totalPoint = CampaignMapUI.Instance.GetTotalPoint();


            if (totalPoint >= pointDecrease) {

                totalPoint -= pointDecrease;

                CampaignMapUI.Instance.SetTotalPoint(totalPoint);
                SaveData.UpdateLevelAbility(currentAbilityStats.abilityType);

                currentLevelText.text = $"Level {(int)currentAbilityStats.currentLevel}";

                OnUpdateDataVisual?.Invoke(this, new IHasAbilityStats.OnUpdateVisualEventArgs { abilityType = currentAbilityStats.abilityType }); // Update Stats

                Sequence upgradeSequence = DOTween.Sequence();

                upgradeSequence.Append(upgradeLevelPanelRect.DOAnchorPos(startPosUpgradeLevelPanel, durationOut).SetEase(Ease.InBack, overShoot));

                if (CanUpgrade()) {
                    // Sau khi Upgrade mà vẫn còn có thể Upgrade tiếp

                    upgradeSequence.AppendCallback(() => {

                        pointUsingText.text = $"{SaveData.GetAbilityLevelDataByLevelAndType((IAbility.AbilityLevel)((int)currentAbilityStats.currentLevel + 1), currentAbilityStats.abilityType).pointUnlock}P";
                    });

                    upgradeSequence.Append(upgradeLevelPanelRect.DOAnchorPos(endPosUpgradeLevelPanel, durationOut).SetEase(Ease.OutBack, overShoot));
                }
                else {
                    // After upgrade reached max level

                    upgradeSequence.OnComplete(() => {

                        upgradeLevelPanelRect.gameObject.SetActive(false);
                    });
                }

            }
        });

    }

    private void Start() {

        // Khởi tạo Ability dictionary để truy xuất Data
        foreach (IAbility.AbilityType abilityType in Enum.GetValues(typeof(IAbility.AbilityType))) {

            if (abilityType == IAbility.AbilityType.None) { continue; }

            AbilityStatus abilityStatus = SaveData.GetAbilityStatusByType(abilityType);

            abilityDict.Add(currentID, abilityStatus);

            currentID += 1;
        }

        // After Spawn

        currentAbilityStats = abilityDict[currentPage];

        Spawn();

    }

    private void Spawn() {

        canvasGroup.interactable = false;

        Sequence spawnSequence = DOTween.Sequence();

        // 1. Setup khởi đầu
        unlockButton.gameObject.SetActive(currentAbilityStats.isLocked);

        mainHubRect.anchoredPosition = startPosMainHub;
        statusPanelRect.anchoredPosition = startPosStatusPanel;
        upgradeLevelPanelRect.anchoredPosition = startPosUpgradeLevelPanel;

        mainHubRect.DOKill();
        statusPanelRect.DOKill();
        upgradeLevelPanelRect.DOKill();

        canvasGroup.interactable = false;

        abilityNameText.text = $"{currentAbilityStats.abilitySO.abilityName}";
        descriptionText.text = "";
        
        // 2. DOTween - Hiện ra

        spawnSequence.Append(mainHubRect.DOAnchorPos(endPosMainHub, durationOut).SetEase(Ease.OutBack, overShoot));

        spawnSequence.AppendCallback(() => {


            // Prepared Stats Data for Ability
            currentLevelText.text = $"Level {(int)currentAbilityStats.currentLevel}";
            OnUpdateDataVisual?.Invoke(this, new IHasAbilityStats.OnUpdateVisualEventArgs { abilityType = currentAbilityStats.abilityType }); // Update Stats
        });

        spawnSequence.Append(DOTween.To(() => descriptionText.text, x => descriptionText.text = x, currentAbilityStats.abilitySO.description, 0.5f).SetEase(Ease.Linear));

        if (currentAbilityStats.isLocked) {
            // Nếu ability đang khóa ngưng xử lí

            spawnSequence.OnComplete(() => {
                canvasGroup.interactable = true;

                // Ẩn các panel không dùng
                statusPanelRect.gameObject.SetActive(false);
                upgradeLevelPanelRect.gameObject.SetActive(false);

            });
        }
        else {
            // Nếu ability không khóa

            spawnSequence.Join(statusPanelRect.DOAnchorPos(endPosStatusPanel, durationOut).SetEase(Ease.OutBack, overShoot));

            if (CanUpgrade()) {
                // Nếu chưa Max Level

                spawnSequence.AppendCallback(() => {

                    pointUsingText.text = $"{SaveData.GetAbilityLevelDataByLevelAndType((IAbility.AbilityLevel)((int)currentAbilityStats.currentLevel + 1), currentAbilityStats.abilityType).pointUnlock}P";

                });

                spawnSequence.Append(upgradeLevelPanelRect.DOAnchorPos(endPosUpgradeLevelPanel, durationOut).SetEase(Ease.OutBack, overShoot));

            }
            else {
                // Nếu Max level
                spawnSequence.AppendCallback(() => {

                    upgradeLevelPanelRect.gameObject.SetActive(false);
                });
            }

            spawnSequence.OnComplete(() => {
                canvasGroup.interactable = true;
            });
        }
    }

    private void MovePage() {

        //// TWeen animation
        levelPagesRect.LeanMoveLocal(targetPos, tweenTime).setEase(tweenType);

        // Update data
        currentAbilityStats = abilityDict[currentPage];

        unlockButton.gameObject.SetActive(currentAbilityStats.isLocked);

        // 0. Setup  
        abilityNameText.text = $"{currentAbilityStats.abilitySO.abilityName}";

        IAbility.AbilityLevel nextLevel = (IAbility.AbilityLevel)((int)currentAbilityStats.currentLevel + 1);

        Sequence movePageSequence = DOTween.Sequence();

        statusPanelRect.DOKill();
        upgradeLevelPanelRect.DOKill();

        descriptionText.text = "";

        // 1. Thu về
        movePageSequence.Append(DOTween.To(() => descriptionText.text, x => descriptionText.text = x, currentAbilityStats.abilitySO.description, 0.5f).SetEase(Ease.Linear));

        if (upgradeLevelPanelRect.gameObject.activeSelf && statusPanelRect.gameObject.activeSelf) {
            // Nếu upgradeLevelPanelRect và statusPanelRect.gameObject.activeSelf cùng on thì thu về lần lượt

            movePageSequence.Join(upgradeLevelPanelRect.DOAnchorPos(startPosUpgradeLevelPanel, durationIn / 2f).SetEase(Ease.InBack, overShoot));
            movePageSequence.Append(statusPanelRect.DOAnchorPos(startPosStatusPanel, durationIn / 2f).SetEase(Ease.InBack, overShoot));
        }
        else if (!upgradeLevelPanelRect.gameObject.activeSelf && statusPanelRect.gameObject.activeSelf) {

            movePageSequence.Join(statusPanelRect.DOAnchorPos(startPosStatusPanel, durationIn).SetEase(Ease.InBack, overShoot));

        }

        //if (statusPanelRect.gameObject.activeSelf) {
        //    // Nếu chỉ statusPanelRect onl thì thu về trước

        //    movePageSequence.Append(statusPanelRect.DOAnchorPos(startPosStatusPanel, durationIn).SetEase(Ease.InBack, overShoot));
        //}

        // 2. Hiện ra

        if (currentAbilityStats.isLocked) {
            // Nếu ability mới này đang khóa

            movePageSequence.OnComplete(() => {

                upgradeLevelPanelRect.gameObject.SetActive(false);
                statusPanelRect.gameObject.SetActive(false);

            });

        }
        else {
            // Nếu ability mới này không khóa

            movePageSequence.AppendCallback(() => {

                currentLevelText.text = $"Level {(int)currentAbilityStats.currentLevel}";

                OnUpdateDataVisual?.Invoke(this, new IHasAbilityStats.OnUpdateVisualEventArgs { abilityType = currentAbilityStats.abilityType }); // Event update Data base on ability

                statusPanelRect.gameObject.SetActive(true);
            });

            // Hiện lại với Data mới
            movePageSequence.Append(statusPanelRect.DOAnchorPos(endPosStatusPanel, durationOut).SetEase(Ease.OutBack, overShoot));

            if (CanUpgrade()) {
                // Nếu chưa Max Level

                movePageSequence.AppendCallback(() => {

                    pointUsingText.text = $"{SaveData.GetAbilityLevelDataByLevelAndType(nextLevel, currentAbilityStats.abilityType).pointUnlock}P";

                    upgradeLevelPanelRect.gameObject.SetActive(true);

                });

                movePageSequence.Append(upgradeLevelPanelRect.DOAnchorPos(endPosUpgradeLevelPanel, durationOut).SetEase(Ease.OutBack, overShoot));

            }
            else {
                // Nếu Max level

                movePageSequence.AppendCallback(() => {

                    upgradeLevelPanelRect.gameObject.SetActive(false);
                });
            }

        }
    }

    private void Despawn() {

        // 0. Setup
        Sequence despawnSequence = DOTween.Sequence();

        mainHubRect.DOKill();
        statusPanelRect.DOKill();
        upgradeLevelPanelRect.DOKill();

        canvasGroup.interactable = false;

        // 1. DOTween - Thu về

        if (upgradeLevelPanelRect.gameObject.activeSelf) {

            despawnSequence.Append(upgradeLevelPanelRect.DOAnchorPos(startPosUpgradeLevelPanel, durationIn).SetEase(Ease.InBack, overShoot));

        }

        if (statusPanelRect.gameObject.activeSelf) {

            despawnSequence.Append(statusPanelRect.DOAnchorPos(startPosStatusPanel, durationIn).SetEase(Ease.InBack, overShoot));

        }

        despawnSequence.Append(mainHubRect.DOAnchorPos(startPosMainHub, durationIn).SetEase(Ease.InBack, overShoot)).OnComplete(() => {

            Destroy(this.gameObject);

        });

    }

    private bool CanUpgrade() {
        AbilityStatus abilityStatus = abilityDict[currentPage];

        return !abilityStatus.isLocked && (int)abilityStatus.currentLevel < 3;
    }

}
