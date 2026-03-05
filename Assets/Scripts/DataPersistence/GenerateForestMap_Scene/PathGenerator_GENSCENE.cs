using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static PathGenerator;

public class PathGenerator_GENSCENE : MonoBehaviour
{
    public static PathGenerator_GENSCENE Instance { get; private set; }


    public event EventHandler PathCreateDone;

    [Header("")]
    [SerializeField] private PathSO defaultPathSO;
    [SerializeField] private Tilemap pathTileMap;

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLabels = true;

    private GroundGenerator_GENSCENE groundGenerator;
    private ILevelManager.GameLevel currentLevel;
    private PathSO currentPathSO;

    #region Grid info
    private GroundGenerator_GENSCENE.Grid[,] gridMap;
    private float mapWidth;
    private float mapHeight;
    #endregion

    #region Path 1 info
    private Vector3Int startPosition;
    private Vector3Int endPosition;
    private PathGenerator.PathDirection currentDirection;
    private int currentX;
    private int currentY;
    private int currentCount;
    private bool forceToChangeDirection = false;
    private bool isContinueLeft;
    private bool isContinueRight;
    private int numberOfPathTile;
    #endregion

    private List<Transform> wayPointList;
    private List<Vector2Int> wayPointPosList;

    private Coroutine currentCoroutine;

    private List<NodePath2x2> nodePath2X2List;

    // Test
    private int numberRePath = 0;

    private void Awake() {

        Instance = this;

        groundGenerator = GetComponentInParent<GridManager_GENSCENE>().GetGroundGenerator();

        wayPointList = new List<Transform>();
        wayPointPosList = new List<Vector2Int>();
        nodePath2X2List = new List<NodePath2x2>();

        groundGenerator.MapCreated += SquareGenerator_MapCreateDone;

        // After Spawn
        this.currentLevel = ILevelManager.GameLevel.Level1;
        this.currentPathSO = defaultPathSO;
    }

    private void Start() {

        SaveDataGridMapUI_GENSCENE.Instance.SavedMap += SaveDataGridMapUI_GENSCENE_SavedMap;
        SaveDataGridMapUI_GENSCENE.Instance.ReGenerateMap += SaveDataGridMapUI_GENSCENE_ReGenerateMap;
        SaveDataGridMapUI_GENSCENE.Instance.DeleteMap += SaveDataGridMapUI_DeleteMap;
        SaveDataGridMapUI_GENSCENE.Instance.OnChangedLevel += SaveDataGridMapUI_OnChangedLevel;

    }


    private void OnDestroy() {

        groundGenerator.MapCreated -= SquareGenerator_MapCreateDone;

        SaveDataGridMapUI_GENSCENE.Instance.SavedMap -= SaveDataGridMapUI_GENSCENE_SavedMap;
        SaveDataGridMapUI_GENSCENE.Instance.ReGenerateMap -= SaveDataGridMapUI_GENSCENE_ReGenerateMap;
        SaveDataGridMapUI_GENSCENE.Instance.DeleteMap -= SaveDataGridMapUI_DeleteMap;
        SaveDataGridMapUI_GENSCENE.Instance.OnChangedLevel -= SaveDataGridMapUI_OnChangedLevel;
    }

    private void SaveDataGridMapUI_OnChangedLevel(object sender, SaveDataGridMapUI_GENSCENE.OnChangedLevelEventArgs e) {
        // When clicked changed level Button

        this.currentLevel = e.level;
        this.currentPathSO = e.pathSO;


        // Clear old Grid Map
        for (int x = 0; x < (int)mapWidth; x++) {

            for (int y = 0; y < (int)mapHeight; y++) {

                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1) {
                    // If current Node is in Map Edges

                    groundGenerator.SetGridTileTo(new Vector2Int(x, y), GroundGenerator_GENSCENE.Grid.Fence);
                }
                else {
                    // If current Node is not in Map Edges

                    groundGenerator.SetGridTileTo(new Vector2Int(x, y), GroundGenerator_GENSCENE.Grid.Floor);
                }

                // Clear all old tile from Path Tilemap
                pathTileMap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }

        // Clear old List
        nodePath2X2List.Clear();
        wayPointPosList.Clear();
        nodePath2X2List.Clear();


        if (wayPointList.Count > 0) {

            foreach (Transform waypoint in wayPointList) {
                Destroy(waypoint.gameObject);
            }

            wayPointList.Clear();
        }
    }

    private void SaveDataGridMapUI_DeleteMap(object sender, EventArgs e) {
        GeneratedGridMapSaveData.ResetDataLevelForestBiome(this.currentLevel);
    }

    private void SaveDataGridMapUI_GENSCENE_ReGenerateMap(object sender, EventArgs e) {
        ReGeneratePath();
    }

    private void SaveDataGridMapUI_GENSCENE_SavedMap(object sender, EventArgs e) {
        GeneratedGridMapSaveData.UpdateForestGridMapList(this.currentLevel,GridManager_GENSCENE.Instance.GetGridMap(), this.nodePath2X2List);
    }

    private void SquareGenerator_MapCreateDone(object sender, System.EventArgs e) {

        // Setup grid
        gridMap = groundGenerator.GetGridMap();
        mapWidth = GridManager_GENSCENE.Instance.GetMapWidth();
        mapHeight = GridManager_GENSCENE.Instance.GetMapHeight();

        // Start generate path
        ReGeneratePath();

    }

    private void SetTile2x2At(NodePath2x2 nodePath2X2, Tile2x2 tile2X2) {

        int xPos = nodePath2X2.centerPos.x;
        int yPos = nodePath2X2.centerPos.y;

        pathTileMap.SetTile(new Vector3Int(xPos, yPos, 0), tile2X2.rightUpTile);
        pathTileMap.SetTile(new Vector3Int(xPos - 1, yPos, 0), tile2X2.leftUpTile);
        pathTileMap.SetTile(new Vector3Int(xPos, yPos - 1, 0), tile2X2.rightDownTile);
        pathTileMap.SetTile(new Vector3Int(xPos - 1, yPos - 1, 0), tile2X2.leftDownTile);
    }

    private IEnumerator SpawnPathTileVisual() {

        int currentIndex = 0;

        while (currentIndex < nodePath2X2List.Count) {

            if (currentIndex == nodePath2X2List.Count - 1) {
                // If current node is last node

                SetTile2x2At(nodePath2X2List[currentIndex], currentPathSO.pathVertical);

                break;
            }

            // If current node is not last node

            NodePath2x2 currentNodePath2X2 = nodePath2X2List[currentIndex];
            NodePath2x2 nextNodePath2x2 = nodePath2X2List[currentIndex + 1];

            if (currentNodePath2X2.currentDirection == nextNodePath2x2.currentDirection) {
                // If current node has same direction with next node -->  Spawn Straight Tile2x2

                if (currentNodePath2X2.currentDirection == PathDirection.Left || currentNodePath2X2.currentDirection == PathDirection.Right) {
                    // Horizontal

                    SetTile2x2At(currentNodePath2X2, currentPathSO.pathHorizontal);
                }
                else {
                    // Vertical

                    SetTile2x2At(currentNodePath2X2, currentPathSO.pathVertical);
                }
            }
            else {
                // If current node has different direction with next node -->  Spawn Corner Tile2x2

                // ---- CASE 1. Bottom Left ----
                bool isDownToRight = currentNodePath2X2.currentDirection == PathDirection.Down && nextNodePath2x2.currentDirection == PathDirection.Right;
                bool isLeftToUp = currentNodePath2X2.currentDirection == PathDirection.Left && nextNodePath2x2.currentDirection == PathDirection.Up;

                if (isDownToRight || isLeftToUp) {

                    SetTile2x2At(currentNodePath2X2, currentPathSO.pathBottomLeft);
                }

                // ---- CASE 2. Bottom Right
                bool isDownToLeft = currentNodePath2X2.currentDirection == PathDirection.Down && nextNodePath2x2.currentDirection == PathDirection.Left;
                bool isRightToUp = currentNodePath2X2.currentDirection == PathDirection.Right && nextNodePath2x2.currentDirection == PathDirection.Up;

                if (isDownToLeft || isRightToUp) {

                    SetTile2x2At(currentNodePath2X2, currentPathSO.pathBottmRight);
                }

                // ---- CASE 3. Top Left
                bool isUpToRight = currentNodePath2X2.currentDirection == PathDirection.Up && nextNodePath2x2.currentDirection == PathDirection.Right;
                bool isLeftToDown = currentNodePath2X2.currentDirection == PathDirection.Left && nextNodePath2x2.currentDirection == PathDirection.Down;

                if (isUpToRight || isLeftToDown) {

                    SetTile2x2At(currentNodePath2X2, currentPathSO.pathTopLeft);
                }

                // ---- CASE 4. Top Right
                bool isUpToLeft = currentNodePath2X2.currentDirection == PathDirection.Up && nextNodePath2x2.currentDirection == PathDirection.Left;
                bool isRightToDown = currentNodePath2X2.currentDirection == PathDirection.Right && nextNodePath2x2.currentDirection == PathDirection.Down;


                if (isUpToLeft || isRightToDown) {

                    SetTile2x2At(currentNodePath2X2, currentPathSO.pathTopRight);
                }
            }

            currentIndex += 1;
            yield return new WaitForSeconds(currentPathSO.spawnTileTimer);
        }

        int currentIndexWaypoint = 0;

        while (currentIndexWaypoint < wayPointPosList.Count) {

            SpawnWaypointAt(wayPointPosList[currentIndexWaypoint]);

            currentIndexWaypoint += 1;

            yield return null;

        }

    }

    private IEnumerator GeneratePath() {

        // 1. Dọn dẹp Grid Map
        for (int x = 0; x < (int)mapWidth; x++) {

            for (int y = 0; y < (int)mapHeight; y++) {

                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1) {
                    // If current Node is in Map Edges
                    
                    groundGenerator.SetGridTileTo(new Vector2Int(x, y), GroundGenerator_GENSCENE.Grid.Fence);
                }
                else {
                    // If current Node is not in Map Edges

                    groundGenerator.SetGridTileTo(new Vector2Int(x, y), GroundGenerator_GENSCENE.Grid.Floor);
                }
                
                // Clear all old tile from Path Tilemap
                pathTileMap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }

        // 2. Clear các list cục bộ
        nodePath2X2List.Clear();
        wayPointPosList.Clear();
        nodePath2X2List.Clear();

        // 3. Clear waypoint transform
        if (wayPointList.Count > 0) {

            foreach (Transform waypoint in wayPointList) {
                Destroy(waypoint.gameObject);
            }

            wayPointList.Clear();
        }

        yield return null;

        // --- First setup of generate process ---
        numberOfPathTile = 0;
        currentCount = 0;

        currentX = (int)UnityEngine.Random.Range(5, mapWidth - 5); // Tọa độ X khởi đầu nằm random trên cùng map
        currentY = (int)mapHeight - 1; // Mặc định tọa độ y trên cùng - 1 (Vì giờ là 2 lane)
        currentDirection = PathGenerator.PathDirection.Down; // Mặc định khi run thì hướng sẽ là đi xuống

        // Start position
        startPosition = new Vector3Int(currentX, currentY, 0);
        ChangeID2x2PathTileAt(startPosition.x, startPosition.y);
        wayPointPosList.Add(new Vector2Int(currentX, currentY));

        currentCount += 1;

        // --- Generate path process ---     
        while (currentY > 1) {
            // Khi chưa xuống tới đáy map

            CheckCurrentDirection();

            ChooseNextDirection();
        }

        numberRePath += 1;

        Debug.Log($"numberRePath {numberRePath}");

        yield return null;

        if (numberOfPathTile < currentPathSO.maxNumberOfPathTile || numberOfPathTile > currentPathSO.maxNumberOfPathTile + 5) {
            

            ReGeneratePath();
        }
        else {
            // Map created done

            StartCoroutine(SpawnPathTileVisual());

        }
    }

    private void ReGeneratePath() {

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);

            currentCoroutine = null;
        }


        currentCoroutine = StartCoroutine(GeneratePath());

    }

    private void CheckCurrentDirection() {

        switch (currentDirection) {

            case PathGenerator.PathDirection.Left:

                if (IsCenterSafe(currentX - 2, currentY)) {
                    // Nếu lùi 2 bước sang bên trái và vẫn Safe

                    currentX -= 2;

                    ChangeID2x2PathTileAt(currentX, currentY);

                }
                else {
                    // Nếu không đi được
                    forceToChangeDirection = true;

                }

                break;

            case PathGenerator.PathDirection.Right:

                if (IsCenterSafe(currentX + 2, currentY)) {
                    // Nếu lùi 2 bước sang bên phải và vẫn Safe

                    currentX += 2;

                    ChangeID2x2PathTileAt(currentX, currentY);

                }
                else {
                    // Nếu không đi được
                    forceToChangeDirection = true;

                }

                break;

            case PathGenerator.PathDirection.Up:
                // Nếu đi lên thì tăng y 2 đơn vị

                if (IsTileFloor(currentX, currentY + 2)) {
                    // Nếu tile bên trên là floor

                    bool sideLeftSafe = isContinueLeft && IsCenterSafe(currentX - 2, currentY + 2);
                    bool sideRightSafe = isContinueRight && IsCenterSafe(currentX + 2, currentY + 2);

                    if (sideLeftSafe || sideRightSafe) {

                        currentY += 2;
                        ChangeID2x2PathTileAt(currentX, currentY);
                    }
                    else {

                        forceToChangeDirection = true;

                    }

                }
                else {
                    // Nếu không đi được
                    forceToChangeDirection = true;
                }


                break;

            case PathGenerator.PathDirection.Down:
                // Nếu hướng xuống thì hạ y 2 đơn vị

                if (currentY > 3) {

                    if (IsCenterSafe(currentX, currentY - 2)) {
                        // Nếu đi xuống 2 đơn vị và vẫn Safe

                        currentY -= 2;
                        ChangeID2x2PathTileAt(currentX, currentY);
                    }
                    else {

                        forceToChangeDirection = true;
                    }
                }
                else {

                    currentY -= 2;
                    ChangeID2x2PathTileAt(currentX, currentY);
                    wayPointPosList.Add(new Vector2Int(currentX, currentY));
                }

                break;

        }
    }

    private void ChooseNextDirection() {

        if (currentCount < currentPathSO.minNumberCanMoveStraight && !forceToChangeDirection) {
            // Tối thiểu phải lặp lại 1 hướng minNumberCanMoveStraight và chưa gặp tường

            currentCount += 1;

        }
        else {
            // Nếu phải đổi hướng

            bool isChangeDirection = Mathf.FloorToInt(UnityEngine.Random.value * 1.99f) == 0;

            if (isChangeDirection || forceToChangeDirection || currentCount > currentPathSO.maxNumberCanMoveStraight) {

                // Reset old data before change direction
                currentCount = 0;
                forceToChangeDirection = false;

                // Create waypoint at this position
                wayPointPosList.Add(new Vector2Int(currentX, currentY));

                // Change Direction
                ChangeDirection();

            }

            currentCount += 1;

        }

    }

    private void ChangeDirection() {

        int dirValue = Mathf.FloorToInt(UnityEngine.Random.value * 2.99f);

        // ---- 1. ĐANG ĐI NGANG PHẢI ĐỔI HƯỚNG ----

        if (currentDirection == PathGenerator.PathDirection.Left || currentDirection == PathGenerator.PathDirection.Right) {
            // Nếu đang đi ngang

            if (dirValue == 0) {
                // Nếu dirValue == 0 ---> Đang đi ngang đổi hướng Up

                Vector2Int centerTopLeft = new Vector2Int(currentX - 2, currentY + 2);
                Vector2Int centerTopMid = new Vector2Int(currentX, currentY + 2);
                Vector2Int centerTopRight = new Vector2Int(currentX + 2, currentY + 2);

                Vector2Int centerSideLeft = new Vector2Int(currentX - 2, currentY);
                Vector2Int centerSideRight = new Vector2Int(currentX + 2, currentY);

                if (HasTopAndSideLeftIsSafe(centerTopLeft, centerTopMid, centerTopRight, centerSideLeft)) {
                    // Nếu trên trái, trên, trên phải và bên Trái đều là Floor

                    if (currentDirection == PathGenerator.PathDirection.Left && currentX > 0) {
                        // Nếu đang đi ngang bên trái

                        currentDirection = PathGenerator.PathDirection.Up;
                        isContinueLeft = true;

                        return;

                    }
                }

                if (HasTopAndSideRightIsSafe(centerTopLeft, centerTopMid, centerTopRight, centerSideRight)) {
                    // Nếu trên trái, trên, trên phải và bên Trái đều là Floor

                    if (currentDirection == PathGenerator.PathDirection.Right && currentX < mapWidth - 1) {
                        // Nếu đang đi ngang bên phải

                        currentDirection = PathGenerator.PathDirection.Up;
                        isContinueRight = true;

                        return;

                    }
                }

            }
            else {
                // Nếu dirValue != 0 ---> Đang đi sang ngang thì bắt buộc đổi hướng DOWN

                currentDirection = PathGenerator.PathDirection.Down;
                return;

            }


        }

        // ---- 2. ĐANG ĐI XUỐNG (DOWN) HOẶC ĐANG UP MÀ PHẢI ĐỔI HƯỚNG ----

        bool canTurnLeft = IsCenterSafe(currentX - 2, currentY);
        bool canTurnRight = IsCenterSafe(currentX + 2, currentY);

        if (canTurnLeft && canTurnRight || isContinueLeft || isContinueRight) {
            // Nếu Trái - Phải đều là Floor hoặc có "Ưu tiên rẽ trái" hoặc có "Ưu tiên rẽ phải"

            if ((dirValue == 1 && !isContinueRight) || isContinueLeft) {
                // Nếu isCoutineLeft = true hoặc dirValue = 1 đồng thời không bị ảnh hưởng bởi isCoutineRight

                if (canTurnLeft) {

                    if (isContinueLeft) { isContinueLeft = false; }

                    currentDirection = PathGenerator.PathDirection.Left;

                }


            }
            else {
                // Nếu isCountineLeft = false hoặc dirValue = 0 / 2 hoặc isCountineRight = true

                if (canTurnRight) {

                    if (isContinueRight) { isContinueRight = false; }

                    currentDirection = PathGenerator.PathDirection.Right;
                }

            }
        }
        else if (canTurnLeft) {
            // Nếu chỉ mỗi trái là Floor

            currentDirection = PathGenerator.PathDirection.Left;

        }
        else if (canTurnRight) {
            // Nếu chỉ mỗi phải là Floor

            currentDirection = PathGenerator.PathDirection.Right;

        }

    }

    private bool IsTileFloor(int xPosition, int yPosition) {

        if (xPosition < 0 || xPosition >= mapWidth || yPosition < 0 || yPosition >= mapHeight) { return false; }

        return gridMap[xPosition, yPosition] == GroundGenerator_GENSCENE.Grid.Floor;

    }

    private bool IsCenterSafe(int centerX, int centerY) {
        // 1. Check biên (Biên phải lùi vào vì ta dùng x-1, y-1 và bước nhảy 2)
        if (centerX < 2 || centerX >= mapWidth - 1 || centerY < 2 || centerY >= mapHeight - 1)
            return false;

        // 2. Check va chạm: Cả 4 ô xung quanh tâm phải là Floor
        if (!IsTileFloor(centerX, centerY)) return false;         // Top-Right
        if (!IsTileFloor(centerX - 1, centerY)) return false;     // Top-Left
        if (!IsTileFloor(centerX, centerY - 1)) return false;     // Bottom-Right
        if (!IsTileFloor(centerX - 1, centerY - 1)) return false; // Bottom-Left


        return true;
    }

    private bool HasTopAndSideLeftIsSafe(Vector2Int topLeft, Vector2Int topMid, Vector2Int topRight, Vector2Int sideLeft) {

        if (IsCenterSafe(topLeft.x, topLeft.y) && IsCenterSafe(topMid.x, topMid.y) && IsCenterSafe(topRight.x, topRight.y) && IsCenterSafe(sideLeft.x, sideLeft.y)) {
            // Check topLeft, topMid, topRight

            return true;
        }

        return false;
    }

    private bool HasTopAndSideRightIsSafe(Vector2Int topLeft, Vector2Int topMid, Vector2Int topRight, Vector2Int sideRight) {

        if (IsCenterSafe(topLeft.x, topLeft.y) && IsCenterSafe(topMid.x, topMid.y) && IsCenterSafe(topRight.x, topRight.y) && IsCenterSafe(sideRight.x, sideRight.y)) {
            // Check topLeft, topMid, topRight

            return true;
        }

        return false;
    }

    private void ChangeID2x2PathTileAt(int xPosition, int yPosition) {
        // Từ tâm xPos và yPos, vẽ spawn 4 node xung quanh

        // 1. Change node Id in gridmap
        // Node Phải_Trên
        groundGenerator.SetGridTileTo(new Vector2Int(xPosition, yPosition), GroundGenerator_GENSCENE.Grid.Path);

        // Node Trái_Trên
        groundGenerator.SetGridTileTo(new Vector2Int(xPosition - 1, yPosition), GroundGenerator_GENSCENE.Grid.Path);

        // Node Phải_Dưới
        groundGenerator.SetGridTileTo(new Vector2Int(xPosition, yPosition - 1), GroundGenerator_GENSCENE.Grid.Path);

        // Node Trái_Dưới
        groundGenerator.SetGridTileTo(new Vector2Int(xPosition - 1, yPosition - 1), GroundGenerator_GENSCENE.Grid.Path);

        numberOfPathTile += 4;

        // 2. Add new nodePath2x2 to List
        NodePath2x2 nodePath2X2 = new NodePath2x2(this.currentDirection, new Vector2Int(xPosition, yPosition));
        nodePath2X2List.Add(nodePath2X2);
    }


    private void SpawnWaypointAt(Vector2Int waypointPos) {

        Vector3Int turnTile = new Vector3Int(waypointPos.x, waypointPos.y, 0);

        Transform waypointTransform = Instantiate(currentPathSO.waypointPrefab, this.transform);
        //waypointTransform.localPosition = pathTileMap.GetCellCenterLocal(turnTile);
        waypointTransform.localPosition = new Vector3((float)turnTile.x, (float)turnTile.y, 0f);

        wayPointList.Add(waypointTransform);

    }

    public ILevelManager.GameLevel GetCurrentLevel() {
        return this.currentLevel;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() {
        if (!showDebugLabels || GridManager_GENSCENE.Instance == null) return;

        // --- PHẦN 1: VẼ TEXT CHO TỪNG Ô (Giữ nguyên để debug loại gạch) ---
        if (gridMap != null) {
            GUIStyle nodeStyle = new GUIStyle();
            nodeStyle.fontSize = 8;
            nodeStyle.alignment = TextAnchor.MiddleCenter;

            for (int x = 0; x < mapWidth; x++) {
                for (int y = 0; y < mapHeight; y++) {
                    GroundGenerator_GENSCENE.Grid tileType = gridMap[x, y];
                    switch (tileType) {
                        case GroundGenerator_GENSCENE.Grid.Path: nodeStyle.normal.textColor = Color.yellow; break;
                        case GroundGenerator_GENSCENE.Grid.Fence: nodeStyle.normal.textColor = Color.red; break;
                        case GroundGenerator_GENSCENE.Grid.Floor: nodeStyle.normal.textColor = Color.gray; break;
                    }
                    Vector3 worldPos = GridManager_GENSCENE.Instance.NodePosConvertToWordPos(new Vector2Int(x, y));
                    Handles.Label(worldPos, tileType.ToString(), nodeStyle);
                }
            }
        }

        // --- PHẦN 2: VẼ WAYPOINT TẠI ĐIỂM GIAO (INTERSECTION) ---
        if (wayPointPosList != null) {
            GUIStyle wpStyle = new GUIStyle();
            wpStyle.fontSize = 11;
            wpStyle.fontStyle = FontStyle.Bold;
            wpStyle.alignment = TextAnchor.LowerCenter; // Để chữ nằm trên điểm giao
            wpStyle.normal.textColor = Color.cyan;

            for (int i = 0; i < wayPointPosList.Count; i++) {
                Vector2Int wpPos = wayPointPosList[i];

                // SỬA TẠI ĐÂY: Sử dụng CellToWorld để lấy đúng tọa độ góc (điểm giao 4 ô)
                // Đây chính là tọa độ (x, y) mà bạn dùng để SpawnWaypointAt
                Vector3 intersectionPos = pathTileMap.CellToWorld(new Vector3Int(wpPos.x, wpPos.y, 0));

                // Vẽ nhãn Waypoint
                // Offset Y một chút để chữ không đè lên cái hình Diamond của prefab
                Handles.Label(intersectionPos + new Vector3(0, 0.2f, 0), $"[WAYPOINT {i}]", wpStyle);

                // Vẽ khung vuông debug ngay tại điểm giao để kiểm tra
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(intersectionPos, new Vector3(0.3f, 0.3f, 0.1f));
            }
        }
    }
#endif
}
