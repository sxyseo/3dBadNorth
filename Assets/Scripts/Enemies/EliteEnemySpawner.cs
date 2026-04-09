using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// 精英敌人生成器 - 管理精英敌人的生成逻辑
    /// AI可以调整生成概率和条件
    /// </summary>
    public class EliteEnemySpawner : MonoBehaviour
    {
        public static EliteEnemySpawner Instance { get; private set; }

        [Header("生成设置")]
        public bool enableEliteSpawns = true;
        public float eliteSpawnChance = 0.15f;      // 15%精英生成率
        public float championSpawnChance = 0.05f;    // 5%冠军生成率
        public float legendSpawnChance = 0.01f;      // 1%传说生成率

        [Header("波次相关")]
        public int minWaveForElites = 3;            // 第3波开始出现精英
        public int minWaveForChampions = 5;         // 第5波开始出现冠军
        public int minWaveForLegends = 10;          // 第10波开始出现传说

        [Header("日数相关")]
        public bool increaseDifficultyWithDays = true;
        public float dailyDifficultyIncrease = 0.02f; // 每天增加2%概率

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

        /// <summary>
        /// 检查是否应该生成精英敌人
        /// </summary>
        public bool ShouldSpawnElite(int currentWave, int currentDay)
        {
            if (!enableEliteSpawns)
                return false;

            // 检查波次要求
            if (currentWave < minWaveForElites)
                return false;

            // 计算基础概率
            float spawnChance = CalculateSpawnChance(currentWave, currentDay);

            // 随机判定
            return Random.value < spawnChance;
        }

        /// <summary>
        /// 计算生成概率
        /// </summary>
        float CalculateSpawnChance(int currentWave, int currentDay)
        {
            float chance = eliteSpawnChance;

            // 根据天数增加难度
            if (increaseDifficultyWithDays)
            {
                chance += currentDay * dailyDifficultyIncrease;
            }

            // 根据波次小幅增加
            chance += currentWave * 0.005f;

            return Mathf.Min(chance, 0.5f); // 最高50%概率
        }

        /// <summary>
        /// 决定精英敌人变种等级
        /// </summary>
        public EliteEnemy.EnemyVariant DetermineVariant(int currentWave, int currentDay)
        {
            float roll = Random.value;

            // 传说级敌人
            if (currentWave >= minWaveForLegends && roll < legendSpawnChance)
            {
                return EliteEnemy.EnemyVariant.Legend;
            }

            // 冠军级敌人
            if (currentWave >= minWaveForChampions && roll < legendSpawnChance + championSpawnChance)
            {
                return EliteEnemy.EnemyVariant.Champion;
            }

            // 精英级敌人
            return EliteEnemy.EnemyVariant.Elite;
        }

        /// <summary>
        /// 生成精英敌人
        /// </summary>
        public GameObject SpawnEliteEnemy(Vector3 position, Enemy.EnemyType baseType, int currentWave, int currentDay)
        {
            // 创建敌人对象
            GameObject enemyObj = new GameObject($"Elite_{baseType}");
            enemyObj.transform.position = position;

            // 添加基础敌人组件
            Enemy baseEnemy = enemyObj.AddComponent<Enemy>();
            baseEnemy.Initialize(currentWave, currentDay);

            // 添加精英敌人组件
            EliteEnemy eliteComponent = enemyObj.AddComponent<EliteEnemy>();
            eliteComponent.baseType = baseType;
            eliteComponent.variant = DetermineVariant(currentWave, currentDay);

            // 获取精英信息用于调试
            EliteEnemyInfo info = eliteComponent.GetInfo();
            Debug.Log($"Elite enemy spawned: {baseType} ({info.Variant}), {info.HealthMultiplier}x HP");

            // 播放生成特效
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.PlaySkillEffect(position, SkillType.GuardAura);
            }

            // 播放音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound();
            }

            // 显示警告消息
            if (UI.UIManager.Instance != null)
            {
                string warningMessage = GetEliteWarningMessage(info.Variant);
                UI.UIManager.Instance.ShowMessage(warningMessage, 3f);
            }

            return enemyObj;
        }

        /// <summary>
        /// 获取精英警告消息
        /// </summary>
        string GetEliteWarningMessage(EliteEnemy.EnemyVariant variant)
        {
            return variant switch
            {
                EliteEnemy.EnemyVariant.Elite => "⚠️ 精英敌人出现！",
                EliteEnemy.EnemyVariant.Champion => "⚠️ 冠军敌人出现！",
                EliteEnemy.EnemyVariant.Legend => "⚠️ 传说敌人出现！极度危险！",
                _ => "强力敌人出现！"
            };
        }

        /// <summary>
        /// 设置生成参数
        /// </summary>
        public void SetSpawnParameters(float eliteChance, float championChance, float legendChance)
        {
            eliteSpawnChance = Mathf.Clamp01(eliteChance);
            championSpawnChance = Mathf.Clamp01(championChance);
            legendSpawnChance = Mathf.Clamp01(legendChance);

            Debug.Log($"Elite spawn parameters updated: Elite={eliteSpawnChance}, Champion={championSpawnChance}, Legend={legendSpawnChance}");
        }

        /// <summary>
        /// 设置难度参数
        /// </summary>
        public void SetDifficultyParameters(int eliteWave, int championWave, int legendWave, float dailyIncrease)
        {
            minWaveForElites = eliteWave;
            minWaveForChampions = championWave;
            minWaveForLegends = legendWave;
            dailyDifficultyIncrease = dailyIncrease;

            Debug.Log($"Difficulty parameters updated: Elites wave {eliteWave}, Champions wave {championWave}, Legends wave {legendWave}");
        }

        /// <summary>
        /// 启用/禁用精英生成
        /// </summary>
        public void SetEliteSpawnsEnabled(bool enabled)
        {
            enableEliteSpawns = enabled;
            Debug.Log($"Elite spawns {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 获取当前生成状态
        /// </summary>
        public EliteSpawnStatus GetSpawnStatus()
        {
            return new EliteSpawnStatus
            {
                EliteSpawnsEnabled = enableEliteSpawns,
                EliteSpawnChance = eliteSpawnChance,
                ChampionSpawnChance = championSpawnChance,
                LegendSpawnChance = legendSpawnChance,
                MinWaveForElites = minWaveForElites,
                MinWaveForChampions = minWaveForChampions,
                MinWaveForLegends = minWaveForLegends
            };
        }

        /// <summary>
        /// 强制生成特定变种（用于测试）
        /// </summary>
        public GameObject ForceSpawnVariant(Vector3 position, Enemy.EnemyType baseType, EliteEnemy.EnemyVariant variant, int currentWave, int currentDay)
        {
            GameObject enemyObj = new GameObject($"TestElite_{baseType}_{variant}");
            enemyObj.transform.position = position;

            Enemy baseEnemy = enemyObj.AddComponent<Enemy>();
            baseEnemy.Initialize(currentWave, currentDay);

            EliteEnemy eliteComponent = enemyObj.AddComponent<EliteEnemy>();
            eliteComponent.baseType = baseType;
            eliteComponent.variant = variant;

            Debug.Log($"Force spawned: {baseType} ({variant})");

            return enemyObj;
        }
    }

    /// <summary>
    /// 精英生成状态
    /// </summary>
    [System.Serializable]
    public struct EliteSpawnStatus
    {
        public bool EliteSpawnsEnabled;
        public float EliteSpawnChance;
        public float ChampionSpawnChance;
        public float LegendSpawnChance;
        public int MinWaveForElites;
        public int MinWaveForChampions;
        public int MinWaveForLegends;
    }
}