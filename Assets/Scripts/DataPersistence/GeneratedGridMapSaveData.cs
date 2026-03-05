using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GeneratedGridMapSaveData
{
    public static void LoadNewData() {
        // Chỉ dùng khi có cập nhật lớn, thay đổi kiểu dữ liệu dẫn tới thay thế toàn bộ file json

        string mapCollectionPath = Path.Combine(Application.persistentDataPath, SaveData.MAP_DATA_PATH);

        Debug.Log(mapCollectionPath);


        // 1. Khởi tạo dữ liệu rỗng
        SaveData.mapCollection = new MapCollection();

        // 2.Nạp file mới 
        string newData = JsonUtility.ToJson(SaveData.mapCollection);
        File.WriteAllText(mapCollectionPath, newData);
    }

    public static void UpdateForestGridMapList(ILevelManager.GameLevel levelState, List<GridNode> gridMap, List<NodePath2x2> nodePath2x2List) {

        string mapCollectionPath = Path.Combine(Application.persistentDataPath, SaveData.MAP_DATA_PATH);
        SaveData.mapCollection = new MapCollection();

        string oldData = File.ReadAllText(mapCollectionPath);
        JsonUtility.FromJsonOverwrite(oldData, SaveData.mapCollection);

        SavedGridMap savedGridMap = new SavedGridMap(gridMap, nodePath2x2List);

        ForestBiomeMapCollection forestBiomeMapCollection = SaveData.mapCollection.forestBiomeMapCollection;

        if (levelState == ILevelManager.GameLevel.Level1) {
            SaveData.mapCollection.forestBiomeMapCollection.forestGridMapCollectionLevel1.Add(savedGridMap);
        }
        if (levelState == ILevelManager.GameLevel.Level2) {
            SaveData.mapCollection.forestBiomeMapCollection.forestGridMapCollectionLevel2.Add(savedGridMap);
        }
        if (levelState == ILevelManager.GameLevel.Level3) {
            SaveData.mapCollection.forestBiomeMapCollection.forestGridMapCollectionLevel3.Add(savedGridMap);
        }
        if (levelState == ILevelManager.GameLevel.Level4) {
            SaveData.mapCollection.forestBiomeMapCollection.forestGridMapCollectionLevel4.Add(savedGridMap);
        }
        if (levelState == ILevelManager.GameLevel.Level5) {
            SaveData.mapCollection.forestBiomeMapCollection.forestGridMapCollectionLevel5.Add(savedGridMap);
        }

        string gridmapListNewData = JsonUtility.ToJson(SaveData.mapCollection);
        File.WriteAllText(mapCollectionPath, gridmapListNewData);

    }

    public static List<SavedGridMap> GetForestBiomeMapCollectionByLevel(ILevelManager.GameLevel levelState) {

        string mapCollectionPath = Path.Combine(Application.persistentDataPath, SaveData.MAP_DATA_PATH);

        ForestBiomeMapCollection forestBiomeMapCollection = SaveData.mapCollection.forestBiomeMapCollection;

        if (levelState == ILevelManager.GameLevel.Level1) {
            return forestBiomeMapCollection.forestGridMapCollectionLevel1;
        }
        if (levelState == ILevelManager.GameLevel.Level2) {
            return forestBiomeMapCollection.forestGridMapCollectionLevel2;
        }
        if (levelState == ILevelManager.GameLevel.Level3) {
            return forestBiomeMapCollection.forestGridMapCollectionLevel3;
        }
        if (levelState == ILevelManager.GameLevel.Level4) {
            return forestBiomeMapCollection.forestGridMapCollectionLevel4;
        }
        if (levelState == ILevelManager.GameLevel.Level5) {
            return forestBiomeMapCollection.forestGridMapCollectionLevel5;
        }

        return null;
    }

    public static void ResetDataLevelForestBiome(ILevelManager.GameLevel levelState) {

        string gridmapCollectionPath = Path.Combine(Application.persistentDataPath, SaveData.MAP_DATA_PATH);

        SaveData.mapCollection = new MapCollection();

        string oldData = File.ReadAllText(gridmapCollectionPath);
        JsonUtility.FromJsonOverwrite(oldData, SaveData.mapCollection);

        Debug.Log(SaveData.mapCollection.forestBiomeMapCollection.forestGridMapCollectionLevel1.Count);

        ForestBiomeMapCollection forestBiomeMapCollection = SaveData.mapCollection.forestBiomeMapCollection;

        // 1. Clear all old data in selected Level
        if (levelState == ILevelManager.GameLevel.Level1) {
            forestBiomeMapCollection.forestGridMapCollectionLevel1.Clear();
        }
        if (levelState == ILevelManager.GameLevel.Level2) {
            forestBiomeMapCollection.forestGridMapCollectionLevel2.Clear();
        }
        if (levelState == ILevelManager.GameLevel.Level3) {
            forestBiomeMapCollection.forestGridMapCollectionLevel3.Clear();
        }
        if (levelState == ILevelManager.GameLevel.Level4) {
            forestBiomeMapCollection.forestGridMapCollectionLevel4.Clear();
        }
        if (levelState == ILevelManager.GameLevel.Level5) {
            forestBiomeMapCollection.forestGridMapCollectionLevel5.Clear();
        }

        // 2. Update empty data to json
        string updateStringData = JsonUtility.ToJson(SaveData.mapCollection);
        File.WriteAllText(gridmapCollectionPath, updateStringData);

    }
}
