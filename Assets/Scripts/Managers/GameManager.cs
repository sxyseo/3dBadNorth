using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace BadNorth3D
{
    /// <summary>
    /// 游戏主管理器 - 控制游戏流程、波次和资源
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("游戏状态")]
        public int currentDay = 1;
        public int currentWave = 0;
        public int totalWaves = 5;
        public float gold = 100f;

        [Header("单位配置")]
        public int maxSquadSize = 12;
        public List<GameObject> squadUnits = new List<GameObject>();

        [Header("敌人生成")]
        public Transform[] enemySpawnPoints;
        public GameObject enemyPrefab;
        public float waveDelay = 3f;

        private bool waveInProgress = false;
        private int enemiesRemaining = 0;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            InitializeGame();
        }

        void InitializeGame()
        {
            // 初始生成几个单位
            for (int i = 0; i < 4; i++)
            {
                SpawnSquadUnit();
            }

            UIManager.Instance.UpdateGoldUI(gold);
            UIManager.Instance.UpdateDayUI(currentDay);
        }

        void Update()
        {
            if (!waveInProgress && Input.GetKeyDown(KeyCode.Space))
            {
                StartNextWave();
            }

            if (enemiesRemaining <= 0 && waveInProgress)
            {
                EndWave();
            }
        }

        public void StartNextWave()
        {
            if (currentWave >= totalWaves)
            {
                WinDay();
                return;
            }

            currentWave++;
            waveInProgress = true;
            UIManager.Instance.UpdateWaveUI(currentWave, totalWaves);

            int enemyCount = 5 + (currentWave * 3) + (currentDay * 2);
            StartCoroutine(SpawnWave(enemyCount));
        }

        System.Collections.IEnumerator SpawnWave(int count)
        {
            enemiesRemaining = count;

            for (int i = 0; i < count; i++)
            {
                Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
                GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                enemy.GetComponent<Enemy>().Initialize(currentWave, currentDay);

                yield return new WaitForSeconds(waveDelay);
            }
        }

        public void OnEnemyKilled(float reward)
        {
            enemiesRemaining--;

            // 通知EconomyManager
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(reward);
            }

            gold += reward;
            UIManager.Instance.UpdateGoldUI(gold);
        }

        public void OnUnitKilled(GameObject unit)
        {
            squadUnits.Remove(unit);

            if (squadUnits.Count == 0)
            {
                GameOver();
            }
        }

        void EndWave()
        {
            waveInProgress = false;
            gold += 20f + (currentWave * 5f); // 波次完成奖励
            UIManager.Instance.UpdateGoldUI(gold);
            UIManager.Instance.ShowWaveComplete();
        }

        public void WinDay()
        {
            currentDay++;
            currentWave = 0;
            gold += 50f;
            UIManager.Instance.ShowDayComplete(currentDay);
        }

        void GameOver()
        {
            UIManager.Instance.ShowGameOver();
            Time.timeScale = 0f;
        }

        public void SpawnSquadUnit()
        {
            if (squadUnits.Count >= maxSquadSize) return;

            if (gold >= 20f)
            {
                gold -= 20f;
                // 在玩家附近生成单位
                Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 3f;
                GameObject unit = Instantiate(Resources.Load<GameObject>("Prefabs/SquadUnit"), spawnPos, Quaternion.identity);
                squadUnits.Add(unit);
                UIManager.Instance.UpdateGoldUI(gold);
            }
        }

        public void RecruitUnit(SquadUnitType unitType)
        {
            if (squadUnits.Count >= maxSquadSize)
            {
                Debug.Log("Squad is full! Cannot recruit more units.");
                return;
            }

            // 从配置中获取单位信息
            UnitTypesConfig.SquadUnitTypeConfig config = GetUnitConfig(unitType);
            if (config == null)
            {
                Debug.LogError($"Unit type {unitType} not found in configuration!");
                return;
            }

            // 检查是否有足够的金币
            if (EconomyManager.Instance != null && !EconomyManager.Instance.SpendGold(config.Cost))
            {
                Debug.Log($"Not enough gold to recruit {config.Name}! Need {config.Cost} gold.");
                return;
            }

            // 在玩家附近生成单位
            Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 3f + Vector3.up * 2f;
            GameObject unit = new GameObject($"{config.Name}_{squadUnits.Count + 1}");
            unit.transform.position = spawnPos;

            // 添加SquadUnitAdvanced组件
            var unitComponent = unit.AddComponent<SquadUnitAdvanced>();
            // unitComponent.unitType = unitType; // 这会在Start中从配置自动应用

            squadUnits.Add(unit);

            Debug.Log($"Recruited {config.Name} for {config.Cost} gold!");

            // 更新UI
            if (EconomyManager.Instance != null)
            {
                UIManager.Instance.UpdateGoldUI(EconomyManager.Instance.GetCurrentGold());
            }
        }

        UnitTypesConfig.SquadUnitTypeConfig GetUnitConfig(SquadUnitType unitType)
        {
            foreach (var config in UnitTypesConfig.PLAYER_UNITS)
            {
                if (config.Type == unitType)
                    return config;
            }
            return UnitTypesConfig.PLAYER_UNITS[0]; // 默认返回战士
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
