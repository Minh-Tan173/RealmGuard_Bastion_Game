using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class SaveDataGridMapUI_GENSCENE : MonoBehaviour
{
    public static SaveDataGridMapUI_GENSCENE Instance { get; private set; }

    public event EventHandler<OnChangedLevelEventArgs> OnChangedLevel;
    public class OnChangedLevelEventArgs : EventArgs {

        public ILevelManager.GameLevel level;
        public PathSO pathSO;
    }

    public event EventHandler SavedMap;
    public event EventHandler ReGenerateMap;
    public event EventHandler DeleteMap;

    [Header("Json Manager Button")]
    [SerializeField] private Button createNewJsonFile;
    [SerializeField] private Button overwriteJsonResouceButton;

    [Header("Function Button")]
    [SerializeField] private Button saveGridMapButton;
    [SerializeField] private Button reGenerateMapButton;
    [SerializeField] private Button deleteMapButton;

    [Header("Select Level Button")]
    [SerializeField] private Button generateLevel1;
    [SerializeField] private Button generateLevel2;
    [SerializeField] private Button generateLevel3;
    [SerializeField] private Button generateLevel4;
    [SerializeField] private Button generateLevel5;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI currentLevelText;

    [Header("Level Data")]
    [SerializeField] private PathSO pathSOLevel1;
    [SerializeField] private PathSO pathSOLevel2;
    [SerializeField] private PathSO pathSOLevel3;
    [SerializeField] private PathSO pathSOLevel4;
    [SerializeField] private PathSO pathSOLevel5;

    [Header("Json Data")]
    [SerializeField] private AssetReferenceT<TextAsset> mapCollection;

    private void Awake() {
        Instance = this;

        // ---- Json Control Button ----
        createNewJsonFile.onClick.AddListener(() => {

            // Tạo 1 file json mới hoàn toàn để nạp data
            GeneratedGridMapSaveData.LoadNewData();

        });

        overwriteJsonResouceButton.onClick.AddListener(() => {

            string newMapCollectionPath = Path.Combine(Application.persistentDataPath, SaveData.MAP_DATA_PATH);
            
            AsyncOperationHandle<TextAsset> handle = mapCollection.LoadAssetAsync<TextAsset>();

            handle.Completed += (handle) => {

                TextAsset targetAsset = handle.Result;

                #if UNITY_EDITOR
                
                // 1. Get full path of Text Asset
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(targetAsset);
                string fullAssetPath = Path.GetFullPath(assetPath);

                // 2. Overwrite Data
                string newData = File.ReadAllText(newMapCollectionPath);
                File.WriteAllText(fullAssetPath, newData);

                // 3. Refress Load Json Resource 
                UnityEditor.AssetDatabase.ImportAsset(assetPath);

                #endif

                Addressables.Release(handle);
            };

        });

        // ---- Function button -----
        saveGridMapButton.onClick.AddListener(() => {
            SavedMap?.Invoke(this, EventArgs.Empty);
        });

        reGenerateMapButton.onClick.AddListener(() => {
            ReGenerateMap?.Invoke(this, EventArgs.Empty);
        });

        deleteMapButton.onClick.AddListener(() => {
            DeleteMap?.Invoke(this, EventArgs.Empty);
        });

        // ---- Select Level Button ----
        generateLevel1.onClick.AddListener(() => {

            OnChangedLevel?.Invoke(this, new OnChangedLevelEventArgs { level = ILevelManager.GameLevel.Level1, pathSO = pathSOLevel1 });

            currentLevelText.text = $"Level {(int)ILevelManager.GameLevel.Level1}";
        });

        generateLevel2.onClick.AddListener(() => {

            OnChangedLevel?.Invoke(this, new OnChangedLevelEventArgs { level = ILevelManager.GameLevel.Level2, pathSO = pathSOLevel2 });

            currentLevelText.text = $"Level {(int)ILevelManager.GameLevel.Level2}";
        });

        generateLevel3.onClick.AddListener(() => {

            OnChangedLevel?.Invoke(this, new OnChangedLevelEventArgs { level = ILevelManager.GameLevel.Level3, pathSO = pathSOLevel3 });

            currentLevelText.text = $"Level {(int)ILevelManager.GameLevel.Level3}";
        });

        generateLevel4.onClick.AddListener(() => {

            OnChangedLevel?.Invoke(this, new OnChangedLevelEventArgs { level = ILevelManager.GameLevel.Level4, pathSO = pathSOLevel4 });

            currentLevelText.text = $"Level {(int)ILevelManager.GameLevel.Level4}";
        });

        generateLevel5.onClick.AddListener(() => {

            OnChangedLevel?.Invoke(this, new OnChangedLevelEventArgs { level = ILevelManager.GameLevel.Level5, pathSO = pathSOLevel5 });

            currentLevelText.text = $"Level {(int)ILevelManager.GameLevel.Level5}";
        });
    }

}
