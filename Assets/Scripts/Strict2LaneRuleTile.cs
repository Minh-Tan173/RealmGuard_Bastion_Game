using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Strict2LaneRuleTile")]
public class Strict2LaneRuleTile : RuleTile
{
    //// Biến tạm để lưu vị trí (vì hàm RuleMatch mặc định không có vị trí)
    //private Vector3Int _currentPos;
    //private Vector3Int _currentPosition;

    //public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
    //    // Mẹo: Lưu vị trí lại trước khi RuleMatch chạy
    //    _currentPosition = position;
    //    base.GetTileData(position, tilemap, ref tileData);
    //}

    //public override bool RuleMatch(int neighbor, TileBase other) {
    //    // 1. Nếu hàng xóm khác loại -> Không nối (dùng logic mặc định)
    //    if (other != this) return base.RuleMatch(neighbor, other);

    //    // 2. TÍNH TOÁN TỌA ĐỘ HÀNG XÓM
    //    Vector3Int offset = GetOffsetByNeighborIndex(neighbor);
    //    Vector3Int otherPos = _currentPos + offset;

    //    // 3. KIỂM TRA CHẴN LẺ (LOGIC 2x2)
    //    // Đây là chìa khóa: Kiểm tra xem 2 viên gạch có thuộc cùng 1 khối 2x2 không
    //    if (!IsSame2x2Block(_currentPos, otherPos)) {
    //        return false; // Khác khối -> Cấm nối -> Tự vẽ viền ngăn cách
    //    }

    //    return true;
    //}

    //// Hàm kiểm tra xem 2 tọa độ có thuộc cùng 1 khối 2x2 không
    //private bool IsSame2x2Block(Vector3Int posA, Vector3Int posB) {
    //    // Giả sử map sinh từ (1,1). Chia đôi tọa độ để gom nhóm.
    //    // Ví dụ: (2,2), (3,2), (2,3), (3,3) sẽ cùng ra kết quả chia 2 giống nhau.

    //    // Bạn có thể cần chỉnh +0 hoặc +1 tùy vào toạ độ Start của bạn là Chẵn hay Lẻ
    //    int xGroupA = Mathf.FloorToInt(posA.x / 2f);
    //    int yGroupA = Mathf.FloorToInt(posA.y / 2f);

    //    int xGroupB = Mathf.FloorToInt(posB.x / 2f);
    //    int yGroupB = Mathf.FloorToInt(posB.y / 2f);

    //    return (xGroupA == xGroupB) && (yGroupA == yGroupB);
    //}

    //// Hàm phụ lấy Offset từ ID hàng xóm
    //private Vector3Int GetOffsetByNeighborIndex(int neighbor) {
    //    switch (neighbor) {
    //        case TilingRuleOutput.Neighbor.This: return Vector3Int.zero;
    //        case TilingRuleOutput.Neighbor.NotThis: return Vector3Int.zero;
    //        case 1: return new Vector3Int(1, 1, 0);   // Top Right
    //        case 2: return new Vector3Int(0, 1, 0);   // Top
    //        case 3: return new Vector3Int(-1, 1, 0);  // Top Left
    //        case 4: return new Vector3Int(-1, 0, 0);  // Left
    //        case 5: return new Vector3Int(-1, -1, 0); // Bottom Left
    //        case 6: return new Vector3Int(0, -1, 0);  // Bottom
    //        case 7: return new Vector3Int(1, -1, 0);  // Bottom Right
    //        case 8: return new Vector3Int(1, 0, 0);   // Right
    //    }
    //    return Vector3Int.zero;
    //}
}
