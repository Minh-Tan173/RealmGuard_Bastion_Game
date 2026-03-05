using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu()]
public class PathSO : ScriptableObject
{
    [Header("Tile")]
    public RuleTile pathTile;

    [Header("Straight Tile")]
    public Tile2x2 pathHorizontal;
    public Tile2x2 pathVertical;

    [Header("Corner Tile")]
    public Tile2x2 pathBottomLeft;
    public Tile2x2 pathBottmRight;
    public Tile2x2 pathTopLeft;
    public Tile2x2 pathTopRight;
    //public Tile TopLeft;
    //public Tile TopRight;
    //public Tile LeftVertical;
    //public Tile RightVertical;
    //public Tile CornerUpLeft;
    //public Tile CornerDownLeft;
    //public Tile LeftHorizontal;
    //public Tile RightHorizontal;
    //public Tile DownLeft;
    //public Tile DownRight;

    [Header("Path data")]
    public int maxNumberCanMoveStraight;
    public int minNumberCanMoveStraight;
    public float spawnTileTimer;
    public int maxNumberOfPathTile;

    [Header("WayPoint template")]
    public Transform waypointPrefab;

}

[System.Serializable]
public class Tile2x2 {

    public Tile rightUpTile;
    public Tile leftUpTile;
    public Tile rightDownTile;
    public Tile leftDownTile;
}
