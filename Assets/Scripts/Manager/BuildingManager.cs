using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    public enum BuildState {
        Normal,
        Building
    }

    public event EventHandler<CreateAbilityDoneEventArgs> CreateAbilityDone;
    public class CreateAbilityDoneEventArgs : EventArgs {
        public IAbility.AbilityType abilityType;
    }

    public event EventHandler OnForceReset;

    private BuildState buildState;

    // Tower building progress
    private Transform currentTowerIcon;
    private TowerSO currentTowerSO;

    // SpecialItem building progress
    private Transform currentAbilityIcon;
    private AbilitySO currentAbilitySO;


    private void Awake() {

        Instance = this;

        ResetBuildProgress();
    }

    private void Start() {

        GameInput.Instance.OnLeftClicked += GameInput_OnLeftClicked;
        GameInput.Instance.OnRightClicked += GameInput_OnRightClicked;

    }

    private void OnDestroy() {

        GameInput.Instance.OnLeftClicked -= GameInput_OnLeftClicked;
        GameInput.Instance.OnRightClicked -= GameInput_OnRightClicked;

    }

    private void GameInput_OnRightClicked(object sender, System.EventArgs e) {
        // Hủy chọn
        
        if (IsPlacingTower()) {

            ForceReset();
        }

        if (IsPlacingAbility()) {

            OnForceReset?.Invoke(this, EventArgs.Empty);

            ForceReset();

            // Change Cursor
            MouseVisual.Instance.SetActiveCursorType(MouseVisual.CursorType.Arrow);
        }

    }

    private void GameInput_OnLeftClicked(object sender, System.EventArgs e) {
        // Đặt 

        if (IsPlacingTower()) {
            // Đặt Tower

            TowerPlaced();
        }

        if (IsPlacingAbility()) {
            // Đặt Ability

            AbilityPlaced();
        }

    }

    private void Update() {

        if (buildState == BuildState.Building) {

            if (IsPlacingTower()) {
                // Has towerIcon in mouse
                currentTowerIcon.transform.position = GameInput.Instance.GetMouseWorldPos();

            }

            if (IsPlacingAbility()) {
                currentAbilityIcon.transform.position = GameInput.Instance.GetMouseWorldPos();
            }
        }
        else {

        }



    }

    private void TowerPlaced() {

        if (IsMouseClickedValidNode2_2(out Vector3 centerPos, out List<GridNode> validGridNodeList)) {

            BaseTower.SpawnTower(currentTowerSO.prefab, centerPos);

            GridManager.Instance.SetHasItemArea2x2(centerPos, isSetHasItemON: true);

            ResetBuildProgress();

        }


    }

    private void AbilityPlaced() {

        // Lấy vị trí Node mà Mouse clicked
        GridNode gridnode = GridManager.Instance.GetNodeAtPoinClicked();

        if (gridnode == null) {
            return;
        }

        if (gridnode.GetGridID() == IGridNode.Grid.Path) {
            // If Node is floor

            if (!gridnode.HasItemOn()) {
                // If dont have any item on this node --> Placed Ability

                Vector3 spawnPos = GridManager.Instance.NodePosConvertToWordPos(gridnode.GetNodePosition());
                AbilityManager.SpawnAbility(currentAbilitySO, spawnPos);

                //gridnode.SetHasItemOn(true);

                IAbility.AbilityType abilityType = AbilityManager.Instance.GetAbilityType(currentAbilitySO);

                ResetBuildProgress();

                CreateAbilityDone?.Invoke(this, new CreateAbilityDoneEventArgs { abilityType = abilityType});
                MouseVisual.Instance.SetActiveCursorType(MouseVisual.CursorType.Arrow);
            }

        }

    }

    private bool IsMouseClickedValidNode2_2(out Vector3 centerPos, out List<GridNode> validGridNodeList) {

        Vector3 mouseClickedPosition = GameInput.Instance.GetMouseWorldPos();

        float offset = 0.5f;

        Vector3 nodeCheckPos = mouseClickedPosition - new Vector3(offset, offset, 0f);

        GridNode validDownLeftNode = GridManager.Instance.GetNodeAt(nodeCheckPos); // node trái dưới

        centerPos = Vector3.zero;
        validGridNodeList = new List<GridNode>();

        if (validDownLeftNode == null) {
            // Click outside gridmap

            return false;
        }

        List<GridNode> tempList = new List<GridNode>();

        if (validDownLeftNode != null) {
            // Click inside gridmap

            Vector2Int nodePos = validDownLeftNode.GetNodePosition();

            for (int x = 0; x <= 1; x++) {
                for (int y = 0; y <= 1; y++) {
                    // Duyệt lần lượt qua (x + 0, y + 0), (x + 0, y + 1), (x+ 1, y + 0), (x + 1, y + 1)

                    Vector2Int neighborPos = new Vector2Int(nodePos.x + x, nodePos.y + y);
                    GridNode neighborNode = GridManager.Instance.GetNode(neighborPos);

                    if (neighborNode == null || neighborNode.GetGridID() != IGridNode.Grid.Floor || neighborNode.HasItemOn()) {
                        // If any node in this not valid

                        return false;

                    }
                    else {
                        // If this node is valid

                        tempList.Add(neighborNode);
                    }
                        
                }

            }
        }

        // Nếu xử lí được tới đây thì tất cả các node đều Valid

        centerPos = GridManager.Instance.NodePosConvertToWordPos(validDownLeftNode.GetNodePosition()) + new Vector3(0.5f, 0.5f, 0f);
        validGridNodeList = tempList;

        return true;

    }

    public bool IsPlacingTower() {

        return (currentTowerSO != null && buildState == BuildState.Building);

    }

    public bool IsPlacingAbility() {

        return (currentAbilitySO != null && buildState == BuildState.Building);

    }


    private void ResetBuildProgress() {

        if (currentTowerIcon != null) {
            Destroy(currentTowerIcon.gameObject);
        }

        if (currentAbilityIcon != null) {
            Destroy(currentAbilityIcon.gameObject);
        }

        currentTowerIcon = null;
        currentTowerSO = null;

        currentAbilityIcon = null;
        currentAbilitySO = null;

        buildState = BuildState.Normal;

    }

    public void ForceReset() {

        if (IsPlacingTower()) {
            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, currentTowerSO.price);
        }
        else if (IsPlacingAbility()) {
            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, currentAbilitySO.price);
        }

        LevelManagerUI.Instance.UpdateVisual();

        MouseVisual.Instance.SetActiveCursorType(MouseVisual.CursorType.Arrow);

        ResetBuildProgress();

    }

    public void SpawnTowerIcon(TowerSO towerSO) {

        currentTowerSO = towerSO;

        Transform towerIconTransform = Instantiate(currentTowerSO.icon);
        towerIconTransform.transform.position = GameInput.Instance.GetMouseWorldPos();

        currentTowerIcon = towerIconTransform;

        AoERangeVisual aoeRangeVisual = towerIconTransform.GetComponentInChildren<AoERangeVisual>();

        if (aoeRangeVisual == null) { Debug.LogError("This Icon dont have child AoRRangeVisual component"); }

        aoeRangeVisual.SetAoERange(currentTowerSO.baseRange);

        buildState = BuildState.Building;

    }

    public void SpawnAbilityIcon(AbilitySO abilitySO) {

        currentAbilitySO = abilitySO;

        Transform abilityIconTransform = Instantiate(currentAbilitySO.iconPrefab);
        abilityIconTransform.transform.position = GameInput.Instance.GetMouseWorldPos();

        currentAbilityIcon = abilityIconTransform;

        AoERangeVisual aoeRangeVisual = abilityIconTransform.GetComponentInChildren<AoERangeVisual>();

        if (aoeRangeVisual == null) { Debug.LogError("This Icon dont have child AoRRangeVisual component"); }

        IAbility.AbilityLevel abilityLevel = SaveData.GetAbilityStatusByType(currentAbilitySO.abilityType).currentLevel;

        aoeRangeVisual.SetAoERange(SaveData.GetAbilityLevelDataByLevelAndType(abilityLevel, currentAbilitySO.abilityType).aoeRadius);


        buildState = BuildState.Building;

        // Change Cursor
        MouseVisual.Instance.SetActiveCursorType(MouseVisual.CursorType.Aim);

    }

}
