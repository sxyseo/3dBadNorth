using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BadNorth3D
{
    /// <summary>
    /// 存档系统 - 保存和加载游戏进度
    /// AI可以调整存档格式和内容
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Header("存档设置")]
        public string saveFileName = "BadNorthSave.dat";
        public int maxAutoSaves = 3;

        private string savePath;
        private int currentAutoSaveIndex = 0;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 设置存档路径
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        }

        void Start()
        {
            Debug.Log($"Save system initialized. Save path: {savePath}");
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        public bool SaveGame(int slot = 0)
        {
            try
            {
                GameData data = CreateGameData();
                string slotSavePath = GetSlotPath(slot);

                // 确保目录存在
                string directory = Path.GetDirectoryName(slotSavePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 序列化并保存
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(slotSavePath, FileMode.Create))
                {
                    formatter.Serialize(stream, data);
                }

                Debug.Log($"Game saved to slot {slot}: {slotSavePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载游戏
        /// </summary>
        public bool LoadGame(int slot = 0)
        {
            try
            {
                string slotSavePath = GetSlotPath(slot);

                if (!File.Exists(slotSavePath))
                {
                    Debug.LogWarning($"Save file not found: {slotSavePath}");
                    return false;
                }

                // 反序列化
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(slotSavePath, FileMode.Open))
                {
                    GameData data = (GameData)formatter.Deserialize(stream);
                    ApplyGameData(data);
                }

                Debug.Log($"Game loaded from slot {slot}: {slotSavePath}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 自动保存（轮换存档槽）
        /// </summary>
        public void AutoSave()
        {
            int slot = 10 + currentAutoSaveIndex; // 使用10+的槽位作为自动存档
            SaveGame(slot);

            currentAutoSaveIndex = (currentAutoSaveIndex + 1) % maxAutoSaves;
            Debug.Log($"Auto-saved to slot {slot}");
        }

        /// <summary>
        /// 删除存档
        /// </summary>
        public bool DeleteSave(int slot = 0)
        {
            try
            {
                string slotSavePath = GetSlotPath(slot);

                if (File.Exists(slotSavePath))
                {
                    File.Delete(slotSavePath);
                    Debug.Log($"Save deleted: slot {slot}");
                    return true;
                }

                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查存档是否存在
        /// </summary>
        public bool SaveExists(int slot = 0)
        {
            return File.Exists(GetSlotPath(slot));
        }

        /// <summary>
        /// 获取存档信息
        /// </summary>
        public SaveInfo GetSaveInfo(int slot = 0)
        {
            string slotSavePath = GetSlotPath(slot);

            if (!File.Exists(slotSavePath))
            {
                return new SaveInfo { Exists = false };
            }

            FileInfo fileInfo = new FileInfo(slotSavePath);
            return new SaveInfo
            {
                Exists = true,
                Slot = slot,
                FilePath = slotSavePath,
                LastModified = fileInfo.LastWriteTime,
                FileSize = fileInfo.Length
            };
        }

        /// <summary>
        /// 创建游戏数据
        /// </summary>
        GameData CreateGameData()
        {
            GameData data = new GameData
            {
                version = 1,
                saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),

                // 游戏进度
                currentDay = GameManager.Instance != null ? GameManager.Instance.currentDay : 1,
                currentWave = GameManager.Instance != null ? GameManager.Instance.currentWave : 0,

                // 经济数据
                gold = EconomyManager.Instance != null ? EconomyManager.Instance.GetCurrentGold() : 0f,

                // 单位数据
                squadUnits = new UnitData[0]
            };

            // 保存单位数据
            if (GameManager.Instance != null && GameManager.Instance.squadUnits != null)
            {
                data.squadUnits = new UnitData[GameManager.Instance.squadUnits.Count];
                for (int i = 0; i < GameManager.Instance.squadUnits.Count; i++)
                {
                    GameObject unit = GameManager.Instance.squadUnits[i];
                    if (unit != null)
                    {
                        SquadUnitAdvanced unitComponent = unit.GetComponent<SquadUnitAdvanced>();
                        UnitUpgrade upgradeComponent = unit.GetComponent<UnitUpgrade>();

                        data.squadUnits[i] = new UnitData
                        {
                            unitType = unitComponent != null ? unitComponent.unitType : SquadUnitType.Warrior,
                            position = unit.transform.position,
                            level = upgradeComponent != null ? upgradeComponent.currentLevel : 1,
                            currentHealth = unitComponent != null ? unitComponent.currentHealth : 100f
                        };
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 应用游戏数据
        /// </summary>
        void ApplyGameData(GameData data)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found, cannot load game");
                return;
            }

            // 恢复游戏进度
            GameManager.Instance.currentDay = data.currentDay;
            GameManager.Instance.currentWave = data.currentWave;

            // 恢复经济
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(data.gold - EconomyManager.Instance.GetCurrentGold());
            }

            // 清除现有单位
            foreach (GameObject unit in GameManager.Instance.squadUnits)
            {
                if (unit != null)
                {
                    Destroy(unit);
                }
            }
            GameManager.Instance.squadUnits.Clear();

            // 恢复单位
            foreach (UnitData unitData in data.squadUnits)
            {
                GameObject unit = new GameObject($"LoadedUnit_{unitData.unitType}");
                unit.transform.position = unitData.position;

                SquadUnitAdvanced unitComponent = unit.AddComponent<SquadUnitAdvanced>();
                unitComponent.unitType = unitData.unitType;

                // 设置血量
                unitComponent.currentHealth = unitData.currentHealth;

                // 设置等级
                UnitUpgrade upgradeComponent = unit.GetComponent<UnitUpgrade>();
                if (upgradeComponent != null)
                {
                    upgradeComponent.currentLevel = unitData.level;
                }

                GameManager.Instance.squadUnits.Add(unit);
            }

            // 更新UI
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.UpdateDayUI(data.currentDay);
                UI.UIManager.Instance.UpdateGoldUI(data.gold);
            }

            Debug.Log($"Game loaded: Day {data.currentDay}, Wave {data.currentWave}, Gold {data.gold}");
        }

        /// <summary>
        /// 获取存档槽路径
        /// </summary>
        string GetSlotPath(int slot)
        {
            string directory = Path.Combine(Application.persistentDataPath, "Saves");
            return Path.Combine(directory, $"save_{slot}.dat");
        }

        // ==================== 数据结构 ====================

        [System.Serializable]
        public class GameData
        {
            public int version;
            public string saveTime;

            // 游戏进度
            public int currentDay;
            public int currentWave;

            // 经济
            public float gold;

            // 单位数据
            public UnitData[] squadUnits;
        }

        [System.Serializable]
        public struct UnitData
        {
            public SquadUnitType unitType;
            public Vector3 position;
            public int level;
            public float currentHealth;
        }

        public struct SaveInfo
        {
            public bool Exists;
            public int Slot;
            public string FilePath;
            public System.DateTime LastModified;
            public long FileSize;
        }
    }
}