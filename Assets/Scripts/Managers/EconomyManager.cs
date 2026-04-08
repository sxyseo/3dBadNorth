using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 经济管理器 - 处理金币奖励和花费
    /// AI可以调整经济参数来改变游戏节奏和难度
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Economy Settings")]
        public float currentGold;
        public float totalGoldEarned;
        public float totalGoldSpent;
        public int enemiesKilled;

        [Header("Reward Multipliers")]
        public float goldRewardMultiplier = 1f;
        public float difficultyMultiplier = 1f;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 初始化金币
            currentGold = GameConfig.Gameplay.START_GOLD;
        }

        void Start()
        {
            // 通知UI更新
            UpdateGoldUI();
        }

        public void AddGold(float amount)
        {
            currentGold += amount;
            totalGoldEarned += amount;
            UpdateGoldUI();

            // 播放获得金币音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayHitSound(); // 临时使用命中音效
            }
        }

        public bool SpendGold(float amount)
        {
            if (currentGold >= amount)
            {
                currentGold -= amount;
                totalGoldSpent += amount;
                UpdateGoldUI();
                return true;
            }
            return false;
        }

        public void OnEnemyKilled(Enemy.EnemyType enemyType)
        {
            enemiesKilled++;

            // 根据敌人类型计算奖励
            float baseReward = GameConfig.Gameplay.GOLD_PER_ENEMY_BASE;

            // 根据敌人类型调整奖励
            float typeMultiplier = enemyType switch
            {
                Enemy.EnemyType.Fast => 1.2f,
                Enemy.EnemyType.Heavy => 2f,
                Enemy.EnemyType.Ranged => 1.5f,
                _ => 1f
            };

            float reward = baseReward * typeMultiplier * goldRewardMultiplier * difficultyMultiplier;
            AddGold(reward);

            Debug.Log($"Enemy killed! Earned {reward} gold (Total: {currentGold})");
        }

        public float GetCurrentGold()
        {
            return currentGold;
        }

        public bool CanAfford(float cost)
        {
            return currentGold >= cost;
        }

        public void SetGoldRewardMultiplier(float multiplier)
        {
            goldRewardMultiplier = Mathf.Max(0.1f, multiplier);
            Debug.Log($"Gold reward multiplier set to {goldRewardMultiplier}");
        }

        public void SetDifficultyMultiplier(float multiplier)
        {
            difficultyMultiplier = Mathf.Max(0.5f, multiplier);
            Debug.Log($"Difficulty multiplier set to {difficultyMultiplier}");
        }

        void UpdateGoldUI()
        {
            // 更新所有UI组件
            var recruitmentPanels = FindObjectsOfType<UI.RecruitmentPanel>();
            foreach (var panel in recruitmentPanels)
            {
                // 直接更新面板内的金币显示
                panel.AddGold(0); // Hack: 触发UI更新
            }
        }

        public EconomyData GetEconomyData()
        {
            return new EconomyData
            {
                CurrentGold = currentGold,
                TotalGoldEarned = totalGoldEarned,
                TotalGoldSpent = totalGoldSpent,
                EnemiesKilled = enemiesKilled,
                GoldPerEnemy = enemiesKilled > 0 ? totalGoldEarned / enemiesKilled : 0f
            };
        }

        // 经济数据结构
        public struct EconomyData
        {
            public float CurrentGold;
            public float TotalGoldEarned;
            public float TotalGoldSpent;
            public int EnemiesKilled;
            public float GoldPerEnemy;
        }
    }
}