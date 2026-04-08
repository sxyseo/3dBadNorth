using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BadNorth3D
{
    /// <summary>
    /// 波次管理器 - 控制敌人的生成和波次进度
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("波次配置")]
        public Wave[] waves;

        [Header("生成设置")]
        public Transform[] spawnPoints;
        public float timeBetweenWaves = 5f;

        private int currentWaveIndex = 0;
        private bool waveInProgress = false;

        [System.Serializable]
        public class Wave
        {
            public string waveName;
            public int enemyCount;
            public float spawnInterval = 1f;
            public EnemyType[] enemyTypes;
        }

        [System.Serializable]
        public class EnemyType
        {
            public GameObject enemyPrefab;
            public int count;
            public float spawnChance = 1f;
        }

        void Start()
        {
            if (waves == null || waves.Length == 0)
            {
                GenerateDefaultWaves();
            }
        }

        void GenerateDefaultWaves()
        {
            waves = new Wave[5];

            for (int i = 0; i < waves.Length; i++)
            {
                Wave wave = new Wave();
                wave.waveName = $"Wave {i + 1}";
                wave.enemyCount = 5 + (i * 3);
                wave.spawnInterval = Mathf.Max(0.5f, 2f - (i * 0.2f));
                wave.enemyTypes = new EnemyType[1];
                wave.enemyTypes[0] = new EnemyType
                {
                    enemyPrefab = null, // 会在运行时设置
                    count = wave.enemyCount,
                    spawnChance = 1f
                };

                waves[i] = wave;
            }
        }

        public void StartWave(int waveIndex)
        {
            if (waveIndex < 0 || waveIndex >= waves.Length)
            {
                Debug.LogError($"Wave index {waveIndex} is out of range");
                return;
            }

            if (waveInProgress)
            {
                Debug.LogWarning("Wave already in progress");
                return;
            }

            StartCoroutine(SpawnWave(waveIndex));
        }

        IEnumerator SpawnWave(int waveIndex)
        {
            currentWaveIndex = waveIndex;
            waveInProgress = true;

            Wave wave = waves[waveIndex];
            Debug.Log($"Starting {wave.waveName}");

            int spawnedCount = 0;

            while (spawnedCount < wave.enemyCount)
            {
                foreach (EnemyType enemyType in wave.enemyTypes)
                {
                    if (Random.value <= enemyType.spawnChance && spawnedCount < wave.enemyCount)
                    {
                        SpawnEnemy(enemyType.enemyPrefab);
                        spawnedCount++;
                    }
                }

                yield return new WaitForSeconds(wave.spawnInterval);
            }

            Debug.Log($"{wave.waveName} spawn complete. Waiting for enemies to be defeated...");
        }

        void SpawnEnemy(GameObject enemyPrefab)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned");
                return;
            }

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.Initialize(currentWaveIndex + 1, GameManager.Instance.currentDay);
            }
        }

        public void OnEnemyDefeated()
        {
            if (!waveInProgress) return;

            Enemy[] remainingEnemies = FindObjectsOfType<Enemy>();
            if (remainingEnemies.Length == 0)
            {
                waveInProgress = false;
                Debug.Log($"Wave {currentWaveIndex + 1} complete!");

                if (currentWaveIndex < waves.Length - 1)
                {
                    StartCoroutine(StartNextWaveAfterDelay());
                }
                else
                {
                    Debug.Log("All waves complete!");
                    GameManager.Instance.WinDay();
                }
            }
        }

        IEnumerator StartNextWaveAfterDelay()
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            StartWave(currentWaveIndex + 1);
        }

        public int GetCurrentWave()
        {
            return currentWaveIndex;
        }

        public int GetTotalWaves()
        {
            return waves.Length;
        }

        public bool IsWaveInProgress()
        {
            return waveInProgress;
        }
    }
}
