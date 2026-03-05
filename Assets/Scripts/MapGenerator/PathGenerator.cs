using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathGenerator : MonoBehaviour {

    public static PathGenerator Instance { get; private set; }

    public enum PathDirection {
        Right,
        Left,
        Down,
        Up
    }

    public event EventHandler PathCreateDone;
    public event EventHandler PathVisualShowAll;
    public event EventHandler SpawnFlagAtLastPathNode;

    [Header("Tilemap")]
    [SerializeField] private Tilemap pathTileMap;

    [Header("Path data")]
    [SerializeField] private PathSO pathSO;

    private GroundGenerator groundGenerator;

    #region Grid info
    private GroundGenerator.Grid[,] gridMap;
    private float mapWidth;
    private float mapHeight;
    #endregion

    #region Path 1 info
    private Vector3Int startPosition;
    private Vector3Int endPosition;
    private PathDirection currentDirection;
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

    private void Awake() {

        Instance = this;

        groundGenerator = GetComponentInParent<GridManager>().GetGroundGenerator();

        wayPointList = new List<Transform>();
        wayPointPosList = new List<Vector2Int>();
        nodePath2X2List = new List<NodePath2x2>();

        groundGenerator.MapCreated += SquareGenerator_MapCreateDone;

    }

    private void Start() {
        ObjectPlaced.Instance.ObjectPlacedDone += ObjectPlaced_ObjectPlacedDone;

    }

    private void OnDestroy() {

        groundGenerator.MapCreated -= SquareGenerator_MapCreateDone;
        ObjectPlaced.Instance.ObjectPlacedDone -= ObjectPlaced_ObjectPlacedDone;
    }


    private void ObjectPlaced_ObjectPlacedDone(object sender, EventArgs e) {
        StartCoroutine(SpawnPathTileVisual());
    }

    private void SquareGenerator_MapCreateDone(object sender, System.EventArgs e) {

        bool hasSavedFile = GridManager.Instance.HasLoadSavedFile();

        if (hasSavedFile) {

            this.nodePath2X2List = GridManager.Instance.GetSavedGridMap().nodePath2X2s;

            wayPointPosList.Add(this.nodePath2X2List[0].centerPos);

            for (int i = 1; i < nodePath2X2List.Count; i++) {

                if (nodePath2X2List[i].currentDirection != nodePath2X2List[i - 1].currentDirection) {
                    // Nếu hướng của node hiện tại khác hướng của node trước đó
                    wayPointPosList.Add(nodePath2X2List[i - 1].centerPos);
                }

            }

            wayPointPosList.Add(this.nodePath2X2List[this.nodePath2X2List.Count - 1].centerPos);

            PathCreateDone?.Invoke(this, EventArgs.Empty);

        }
        else {

            // Setup grid
            gridMap = groundGenerator.GetGridMap();
            mapWidth = GridManager.Instance.GetMapWidth();
            mapHeight = GridManager.Instance.GetMapHeight();

            // Start generate path
            ReGeneratePath();
        }

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
        // Function: Spawn path tile visual on gridmap

        int currentIndex = 0;

        while (currentIndex < nodePath2X2List.Count) {
            
            if (currentIndex == nodePath2X2List.Count - 1) {
                // If current node is last node

                SetTile2x2At(nodePath2X2List[currentIndex], pathSO.pathVertical);

                SpawnFlagAtLastPathNode?.Invoke(this, EventArgs.Empty);

                break;
            }

            // If current node is not last node

            NodePath2x2 currentNodePath2X2 = nodePath2X2List[currentIndex];
            NodePath2x2 nextNodePath2x2 = nodePath2X2List[currentIndex + 1];

            if (currentNodePath2X2.currentDirection == nextNodePath2x2.currentDirection) {
                // If current node has same direction with next node -->  Spawn Straight Tile2x2

                if (currentNodePath2X2.currentDirection == PathDirection.Left || currentNodePath2X2.currentDirection == PathDirection.Right) {
                    // Horizontal

                    SetTile2x2At(currentNodePath2X2, pathSO.pathHorizontal);
                }
                else {
                    // Vertical

                    SetTile2x2At(currentNodePath2X2, pathSO.pathVertical);
                }
            }
            else {
                // If current node has different direction with next node -->  Spawn Corner Tile2x2

                // ---- CASE 1. Bottom Left ----
                bool isDownToRight = currentNodePath2X2.currentDirection == PathDirection.Down && nextNodePath2x2.currentDirection == PathDirection.Right;
                bool isLeftToUp = currentNodePath2X2.currentDirection == PathDirection.Left && nextNodePath2x2.currentDirection == PathDirection.Up;

                if (isDownToRight || isLeftToUp) {

                    SetTile2x2At(currentNodePath2X2, pathSO.pathBottomLeft);
                }

                // ---- CASE 2. Bottom Right
                bool isDownToLeft = currentNodePath2X2.currentDirection == PathDirection.Down && nextNodePath2x2.currentDirection == PathDirection.Left;
                bool isRightToUp = currentNodePath2X2.currentDirection == PathDirection.Right && nextNodePath2x2.currentDirection == PathDirection.Up;

                if (isDownToLeft || isRightToUp) {

                    SetTile2x2At(currentNodePath2X2, pathSO.pathBottmRight);
                }

                // ---- CASE 3. Top Left
                bool isUpToRight = currentNodePath2X2.currentDirection == PathDirection.Up && nextNodePath2x2.currentDirection == PathDirection.Right;
                bool isLeftToDown = currentNodePath2X2.currentDirection == PathDirection.Left && nextNodePath2x2.currentDirection == PathDirection.Down;

                if (isUpToRight || isLeftToDown) {

                    SetTile2x2At(currentNodePath2X2, pathSO.pathTopLeft);
                }

                // ---- CASE 4. Top Right
                bool isUpToLeft = currentNodePath2X2.currentDirection == PathDirection.Up && nextNodePath2x2.currentDirection == PathDirection.Left;
                bool isRightToDown = currentNodePath2X2.currentDirection == PathDirection.Right && nextNodePath2x2.currentDirection == PathDirection.Down;


                if (isUpToLeft || isRightToDown) {

                    SetTile2x2At(currentNodePath2X2, pathSO.pathTopRight);
                }
            }

            currentIndex += 1;
            yield return new WaitForSeconds(pathSO.spawnTileTimer);
        }

        int currentIndexWaypoint = 0;

        while (currentIndexWaypoint < wayPointPosList.Count) {

            SpawnWaypointAt(wayPointPosList[currentIndexWaypoint]);

            currentIndexWaypoint += 1;

            yield return null;

        }

        PathVisualShowAll?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator GeneratePath() {

        // --- 1. Clear all old path ---
        for (int x = 0; x < (int)mapWidth; x++) {
            for (int y = 0; y < (int)mapHeight; y++) {

                if (gridMap[x, y] == GroundGenerator.Grid.Path) {

                    pathTileMap.SetTile(new Vector3Int(x, y, 0), null);

                    if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1) {
                        // Nếu là phần Wall

                        groundGenerator.SetGridTileTo(new Vector2Int(x, y), GroundGenerator.Grid.Fence);

                    }
                    else {
                        // Nếu là phần Floor
                        groundGenerator.SetGridTileTo(new Vector2Int(x, y), GroundGenerator.Grid.Floor);
                    }

                }
            }
        }
        nodePath2X2List.Clear();
        wayPointPosList.Clear();


        yield return null;

        // --- 2. First setup of generate process ---
        numberOfPathTile = 0;
        currentCount = 0;

        currentX = (int)UnityEngine.Random.Range(3, mapWidth / 2); // Tọa độ X khởi đầu nằm trong nửa trái map
        currentY = (int)mapHeight - 1; // Mặc định tọa độ y trên cùng - 1 (Vì giờ là 2 lane)
        currentDirection = PathDirection.Down; // Mặc định khi run thì hướng sẽ là đi xuống

        // Start Pos setup
        startPosition = new Vector3Int(currentX, currentY, 0);
        ChangeID2x2PathTileAt(startPosition.x, startPosition.y);
        wayPointPosList.Add(new Vector2Int(currentX, currentY));

        currentCount += 1;

        // --- 3. Generate path process ---     
        while (currentY > 1) {
            // If not moved to bottom map

            CheckCurrentDirection();

            ChooseNextDirection();
        }


        yield return null;

        if (numberOfPathTile < pathSO.maxNumberOfPathTile) {

            ReGeneratePath();
        }
        else {
            // Map created done

            PathCreateDone?.Invoke(this, EventArgs.Empty);

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

            case PathDirection.Left:

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

            case PathDirection.Right:

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

            case PathDirection.Up:
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

            case PathDirection.Down:
                // Nếu hướng DOWN thì hạ y 2 đơn vị

                if (currentY > 3) {
                    // If far from bottom, move normally

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
                    // If near bottom, force exit

                    currentY -= 2;
                    ChangeID2x2PathTileAt(currentX, currentY);
                    wayPointPosList.Add(new Vector2Int(currentX, currentY));
                }

                break;

        }
    }

    private void ChooseNextDirection() {

        if (currentCount < pathSO.minNumberCanMoveStraight && !forceToChangeDirection) {
            // Tối thiểu phải lặp lại 1 hướng minNumberCanMoveStraight và chưa gặp tường

            currentCount += 1;

        }
        else {
            // Nếu phải đổi hướng

            bool isChangeDirection = Mathf.FloorToInt(UnityEngine.Random.value * 1.99f) == 0;

            if (isChangeDirection || forceToChangeDirection || currentCount > pathSO.maxNumberCanMoveStraight) {

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

        if (currentDirection == PathDirection.Left || currentDirection == PathDirection.Right) {
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

                    if (currentDirection == PathDirection.Left && currentX > 0) {
                        // Nếu đang đi ngang bên trái

                        currentDirection = PathDirection.Up;
                        isContinueLeft = true;

                        return;

                    }
                }

                if (HasTopAndSideRightIsSafe(centerTopLeft, centerTopMid, centerTopRight, centerSideRight)) {
                    // Nếu trên trái, trên, trên phải và bên Trái đều là Floor

                    if (currentDirection == PathDirection.Right && currentX < mapWidth - 1) {
                        // Nếu đang đi ngang bên phải

                        currentDirection = PathDirection.Up;
                        isContinueRight = true;

                        return;

                    }
                }

            }
            else {
                // Nếu dirValue != 0 ---> Đang đi sang ngang thì bắt buộc đổi hướng DOWN

                currentDirection = PathDirection.Down;
                return;

            }
            

        }

        // ---- 2. ĐANG ĐI XUỐNG (DOWN) HOẶC ĐANG UP MÀ PHẢI ĐỔI HƯỚNG ----

        bool canTurnLeft = IsCenterSafe(currentX - 2, currentY);
        bool canTurnRight = IsCenterSafe(currentX + 2, currentY);

        if (canTurnLeft && canTurnRight || isContinueLeft || isContinueRight) {
            // Nếu Trái - Phải đều là Floor hoặc có "Ưu tiên rẽ trái" hoặc có "Ưu tiên rẽ phải"

            if ((dirValue  == 1 && !isContinueRight) || isContinueLeft) {
                // Nếu isCoutineLeft = true hoặc dirValue = 1 đồng thời không bị ảnh hưởng bởi isCoutineRight

                if (canTurnLeft) {

                    if (isContinueLeft) { isContinueLeft = false; }

                    currentDirection = PathDirection.Left;  

                }


            }
            else {
                // Nếu isCountineLeft = false hoặc dirValue = 0 / 2 hoặc isCountineRight = true

                if (canTurnRight) {

                    if (isContinueRight) { isContinueRight = false; }

                    currentDirection = PathDirection.Right;
                }

            }
        }
        else if (canTurnLeft) {
            // Nếu chỉ mỗi trái là Floor

            currentDirection = PathDirection.Left;

        }
        else if (canTurnRight) {
            // Nếu chỉ mỗi phải là Floor

            currentDirection = PathDirection.Right;

        }

    }

    private bool IsTileFloor(int xPosition, int yPosition) {

        if (xPosition < 0 || xPosition >= mapWidth || yPosition < 0 || yPosition >= mapHeight) { return false; }

        return gridMap[xPosition, yPosition] == GroundGenerator.Grid.Floor;

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
        groundGenerator.SetGridTileTo(new Vector2Int(xPosition, yPosition), GroundGenerator.Grid.Path);

        // Node Trái_Trên
        groundGenerator.SetGridTileTo(new Vector2Int(xPosition - 1, yPosition), GroundGenerator.Grid.Path);

        // Node Phải_Dưới
        groundGenerator.SetGridTileTo(new Vector2Int(xPosition, yPosition - 1), GroundGenerator.Grid.Path);

        // Node Trái_Dưới
        groundGenerator.SetGridTileTo(new Vector2Int(xPosition - 1, yPosition - 1), GroundGenerator.Grid.Path);

        numberOfPathTile += 4;

        // 2. Add new nodePath2x2 to List
        NodePath2x2 nodePath2X2 = new NodePath2x2(this.currentDirection, new Vector2Int(xPosition, yPosition));
        nodePath2X2List.Add(nodePath2X2);
    }


    private void SpawnWaypointAt(Vector2Int waypointPos) {

        Vector3Int turnTile = new Vector3Int(waypointPos.x, waypointPos.y, 0);

        Transform waypointTransform = Instantiate(pathSO.waypointPrefab, this.transform);
        waypointTransform.localPosition = new Vector3((float)turnTile.x, (float)turnTile.y, 0f);

        wayPointList.Add(waypointTransform);

    }

    public List<NodePath2x2> GetNodePath2X2List() {
        return this.nodePath2X2List;
    }

    public List<Vector2Int> GetWaypointPosList() {
        return this.wayPointPosList;
    }

    public List<Transform> GetWaypointList() {
        return this.wayPointList;
    }

}

[System.Serializable]
public class NodePath2x2 {
    
    public PathGenerator.PathDirection currentDirection;
    public Vector2Int centerPos;

    public NodePath2x2(PathGenerator.PathDirection currentDirection, Vector2Int centorPos) {
        this.currentDirection = currentDirection;
        this.centerPos = centorPos;
    }
}

