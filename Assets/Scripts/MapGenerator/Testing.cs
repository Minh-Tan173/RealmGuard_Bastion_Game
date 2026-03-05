using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Testing : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapHeight = 10;
    [SerializeField] private float stepDelay = 0.05f; // Thời gian chờ khi sinh đường

    [Header("References")]
    [SerializeField] private Tilemap mapTilemap; // Kéo Tilemap vào đây trong Inspector

    [Header("Tiles")]
    // Thay thế Sprite bằng TileBase để hỗ trợ cả Tile thường và RuleTile
    [SerializeField] private TileBase backgroundTile;
    [SerializeField] private TileBase verticalTile;   // Tương ứng downPath cũ
    [SerializeField] private TileBase horizontalTile; // Tương ứng leftRight cũ
    [SerializeField] private TileBase cornerLeftDown; // leftDown
    [SerializeField] private TileBase cornerRightDown;// rightDown
    [SerializeField] private TileBase cornerDownLeft; // downLeft
    [SerializeField] private TileBase cornerDownRight;// downRight

    private int curX;
    private int curY;
    private TileBase tileToUse;
    private bool forceDirectionChange = false;

    private bool continueLeft = false;
    private bool continueRight = false;
    private int currentCount = 0;

    // Mảng logic để kiểm tra ô nào đã có đường đi (thay cho việc check TileData)
    // 0 = trống, 1 = đã có đường
    private int[,] gridLogic;

    private enum CurrentDirection {
        LEFT,
        RIGHT,
        DOWN,
        UP
    };
    private CurrentDirection curDirection = CurrentDirection.DOWN;

    void Awake() {
        // Khởi tạo mảng logic
        gridLogic = new int[mapWidth, mapHeight];
        GenerateMap();
    }

    void GenerateMap() {
        mapTilemap.ClearAllTiles(); // Xóa map cũ

        // Tạo nền (Background)
        for (int x = 0; x < mapWidth; x++) {
            for (int y = 0; y < mapHeight; y++) {
                gridLogic[x, y] = 0; // Đánh dấu là trống
                mapTilemap.SetTile(new Vector3Int(x, y, 0), backgroundTile);
            }
        }

        StartCoroutine(GeneratePath());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            RegenerateMap();
        }
    }

    void RegenerateMap() {
        StopAllCoroutines();
        GenerateMap(); // Gọi lại hàm GenerateMap để reset và chạy lại
    }

    IEnumerator GeneratePath() {
        curX = Random.Range(0, mapWidth);
        curY = mapHeight - 1; // Bắt đầu từ đỉnh map đi xuống (Top-down)
                              // Hoặc nếu bạn muốn đi từ dưới lên như code cũ thì để curY = 0;
                              // Tuy nhiên code cũ logic DOWN là tăng Y? 
                              // Trong Tilemap 2D chuẩn: Y tăng là đi lên, Y giảm là đi xuống.
                              // ĐỂ GIỮ ĐÚNG LOGIC CŨ CỦA BẠN (DOWN là tăng chỉ số mảng):
                              // Tôi sẽ giữ nguyên logic Y=0 start, và curY++ là đi "xuống" theo mảng, 
                              // nhưng trên màn hình Tilemap Y tăng là đi lên. 
                              // => Nếu bạn muốn Y tăng là đi xuống màn hình, Camera cần quay ngược hoặc ta vẽ map ngược.
                              // Dưới đây tôi giữ nguyên logic mảng của bạn: Start (random, 0) -> End (x, max)

        curY = 0;
        curDirection = CurrentDirection.DOWN;
        tileToUse = verticalTile;

        while (curY < mapHeight) {
            CheckCurrentDirections();
            ChooseDirection();

            if (curY < mapHeight) {
                SetTileAt(curX, curY, tileToUse);
            }

            if (curDirection == CurrentDirection.DOWN) {
                curY++;
            }

            yield return new WaitForSeconds(stepDelay);
        }
    }

    // Hàm thay thế UpdateMap cũ
    private void SetTileAt(int x, int y, TileBase tile) {
        // Cập nhật Logic
        if (IsValidCoord(x, y)) {
            gridLogic[x, y] = 1;
            // Cập nhật Visual trên Tilemap
            // Lưu ý: Tilemap dùng Vector3Int(x, y, z)
            mapTilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }

    private bool IsValidCoord(int x, int y) {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }

    // Kiểm tra xem ô đó có trống không dựa vào mảng Logic
    private bool IsCellEmpty(int x, int y) {
        if (!IsValidCoord(x, y)) return false;
        return gridLogic[x, y] == 0;
    }

    private void CheckCurrentDirections() {
        if (curDirection == CurrentDirection.LEFT && IsCellEmpty(curX - 1, curY)) {
            curX--;
        }
        else if (curDirection == CurrentDirection.RIGHT && IsCellEmpty(curX + 1, curY)) {
            curX++;
        }
        else if (curDirection == CurrentDirection.UP && IsCellEmpty(curX, curY - 1)) {
            if ((continueLeft && IsCellEmpty(curX - 1, curY - 1)) ||
                (continueRight && IsCellEmpty(curX + 1, curY - 1))) {
                curY--;
            }
            else {
                forceDirectionChange = true;
                // Trong Tilemap không cần chỉnh transform position như code cũ
            }
        }
        else if (curDirection != CurrentDirection.DOWN) {
            forceDirectionChange = true;
        }
    }

    private void ChooseDirection() {
        if (currentCount < 2 && !forceDirectionChange) {
            currentCount++;
        }
        else {
            bool chanceToChange = Mathf.FloorToInt(Random.value * 1.01f) == 0;

            if (true) {
                currentCount = 0;
                forceDirectionChange = false;
                ChangeDirection();
            }

            currentCount++;
        }
    }

    private void ChangeDirection() {
        int dirValue = Mathf.FloorToInt(Random.value * 2.99f);

        // Logic check đường cùng để quay đầu (Go Up)
        if ((dirValue == 0 && curDirection == CurrentDirection.LEFT && curX - 1 > 0) ||
            (dirValue == 0 && curDirection == CurrentDirection.RIGHT && curX + 1 < mapWidth - 1)) {
            if (curY - 1 >= 0) {
                if (IsCellEmpty(curX, curY - 1) &&
                    IsCellEmpty(curX - 1, curY - 1) &&
                    IsCellEmpty(curX + 1, curY - 1)) {
                    GoUp();
                    return;
                }
            }
        }

        // Xử lý cua góc
        if (curDirection == CurrentDirection.LEFT) {
            SetTileAt(curX, curY, cornerLeftDown);
        }
        else if (curDirection == CurrentDirection.RIGHT) {
            SetTileAt(curX, curY, cornerRightDown);
        }

        // Nếu đang đi ngang, chuyển sang đi xuống
        if (curDirection == CurrentDirection.LEFT || curDirection == CurrentDirection.RIGHT) {
            curY++;
            tileToUse = verticalTile;
            curDirection = CurrentDirection.DOWN;
            return;
        }

        // Nếu đang đi xuống, chọn hướng rẽ trái/phải
        if ((curX - 1 > 0 && curX + 1 < mapWidth - 1) || continueLeft || continueRight) {
            if (dirValue == 1 && !continueRight || continueLeft) {
                if (IsCellEmpty(curX - 1, curY)) {
                    if (continueLeft) {
                        tileToUse = cornerRightDown; // Logic cũ logic map hơi ngược? Check lại asset
                        continueLeft = false;
                    }
                    else {
                        tileToUse = cornerDownLeft;
                    }
                    curDirection = CurrentDirection.LEFT;
                }
            }
            else {
                if (IsCellEmpty(curX + 1, curY)) {
                    if (continueRight) {
                        continueRight = false;
                        tileToUse = cornerLeftDown;
                    }
                    else {
                        tileToUse = cornerDownRight;
                    }
                    curDirection = CurrentDirection.RIGHT;
                }
            }
        }
        else if (curX - 1 > 0) {
            tileToUse = cornerDownLeft;
            curDirection = CurrentDirection.LEFT;
        }
        else if (curX + 1 < mapWidth - 1) {
            tileToUse = cornerDownRight;
            curDirection = CurrentDirection.RIGHT;
        }

        // Thực hiện bước đi đầu tiên sau khi rẽ
        if (curDirection == CurrentDirection.LEFT) {
            GoLeft();
        }
        else if (curDirection == CurrentDirection.RIGHT) {
            GoRight();
        }
    }

    private void GoUp() {
        if (curDirection == CurrentDirection.LEFT) {
            SetTileAt(curX, curY, cornerDownRight);
            continueLeft = true;
        }
        else {
            SetTileAt(curX, curY, cornerDownLeft);
            continueRight = true;
        }
        curDirection = CurrentDirection.UP;
        curY--; // Đi lùi mảng Y
        tileToUse = verticalTile;
    }

    private void GoLeft() {
        SetTileAt(curX, curY, tileToUse);
        curX--;
        tileToUse = horizontalTile;
    }

    private void GoRight() {
        SetTileAt(curX, curY, tileToUse);
        curX++;
        tileToUse = horizontalTile;
    }
}
