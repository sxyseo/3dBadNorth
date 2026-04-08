using UnityEngine;

namespace BadNorth3D.Achievements
{
    /// <summary>
    /// 成就追踪器 - 自动监听游戏事件并更新成就进度
    /// AI可以添加新的追踪条件和成就类型
    /// </summary>
    public class AchievementTracker : MonoBehaviour
    {
        public static AchievementTracker Instance { get; private set; }

        // 追踪统计
        private int totalKills = 0;
        private float totalGoldEarned = 0f;
        private int totalRecruits = 0;
        private int maxLevelUnits = 0;
        private int currentDay = 1;
        private int currentWave = 0;
        private float waveStartTime = 0f;
        private int unitsAtWaveStart = 0;
        private bool waveHadCasualties = false;

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
            // 注册游戏事件监听器
            RegisterEventListeners();
        }

        /// <summary>
        /// 注册游戏事件监听器
        /// </summary>
        void RegisterEventListeners()
        {
            // 这里可以订阅各种游戏事件
            // 由于Unity事件系统需要具体的组件，我们通过Update轮询来追踪
            Debug.Log("Achievement tracker initialized");
        }

        void Update()
        {
            // 定期检查游戏状态并更新成就进度
            CheckGameStates();
        }

        /// <summary>
        /// 检查游戏状态
        /// </summary>
        void CheckGameStates()
        {
            if (AchievementSystem.Instance == null)
                return;

            // 检查天数进度
            if (GameManager.Instance != null)
            {
                int newDay = GameManager.Instance.currentDay;
                if (newDay > currentDay)
                {
                    currentDay = newDay;
                    AchievementSystem.Instance.UpdateAchievementProgress(\"survive_day_5\", currentDay);
                    AchievementSystem.Instance.UpdateAchievementProgress(\"survive_day_20\", currentDay);
                }
            }

            // 检查经济状态
            if (EconomyManager.Instance != null)
            {
                var economyData = EconomyManager.Instance.GetEconomyData();
                if (economyData.TotalGoldEarned > totalGoldEarned)
                {
                    totalGoldEarned = economyData.TotalGoldEarned;
                    AchievementSystem.Instance.UpdateAchievementProgress(\"rich_1000\", totalGoldEarned);
                    AchievementSystem.Instance.UpdateAchievementProgress(\"rich_10000\", totalGoldEarned);
                }
            }
        }

        /// <summary>
        /// 敌人被击杀时调用
        /// </summary>
        public void OnEnemyKilled(Enemy.EnemyType enemyType)
        {
            if (AchievementSystem.Instance == null)
                return;

            totalKills++;

            // 更新战斗成就
            AchievementSystem.Instance.UpdateAchievementProgress(\"first_blood\", 1);
            AchievementSystem.Instance.UpdateAchievementProgress(\"killer_100\", totalKills);
            AchievementSystem.Instance.UpdateAchievementProgress(\"killer_1000\", totalKills);
        }

        /// <summary>
        /// 单位被招募时调用
        /// </summary>
        public void OnUnitRecruited(SquadUnitType unitType)
        {
            if (AchievementSystem.Instance == null)
                return;

            totalRecruits++;

            // 更新招募成就
            AchievementSystem.Instance.UpdateAchievementProgress(\"recruit_10\", totalRecruits);
        }

        /// <summary>
        /// 单位升级时调用
        /// </summary>
        public void OnUnitUpgraded(int newLevel)
        {
            if (AchievementSystem.Instance == null)
                return;

            // 检查是否达到满级
            if (newLevel >= 5)
            {
                maxLevelUnits++;
                AchievementSystem.Instance.UpdateAchievementProgress(\"upgrade_max\", 1);
            }
        }

        /// <summary>
        /// 波次开始时调用
        /// </summary>
        public void OnWaveStart(int waveNumber)
        {
            currentWave = waveNumber;
            waveStartTime = Time.time;
            waveHadCasualties = false;

            // 记录波次开始时的单位数量
            if (GameManager.Instance != null)
            {
                unitsAtWaveStart = GameManager.Instance.squadUnits.Count;
            }
        }

        /// <summary>
        /// 波次完成时调用
        /// </summary>
        public void OnWaveComplete(int waveNumber)
        {
            if (AchievementSystem.Instance == null)
                return;

            // 检查速战速决成就
            float waveDuration = Time.time - waveStartTime;
            if (waveDuration <= 30f)
            {
                AchievementSystem.Instance.UpdateAchievementProgress(\"speed_clear\", 1);
            }

            // 检查完美防守成就
            if (!waveHadCasualties && unitsAtWaveStart > 0)
            {
                AchievementSystem.Instance.UpdateAchievementProgress(\"no_casualties\", 1);
            }
        }

        /// <summary>
        /// 单位阵亡时调用
        /// </summary>
        public void OnUnitKilled()
        {
            waveHadCasualties = true;
        }

        /// <summary>
        /// 获取追踪统计
        /// </summary>
        public TrackerStats GetStats()
        {
            return new TrackerStats
            {
                TotalKills = totalKills,
                TotalGoldEarned = totalGoldEarned,
                TotalRecruits = totalRecruits,
                MaxLevelUnits = maxLevelUnits,
                CurrentDay = currentDay,
                CurrentWave = currentWave
            };
        }

        /// <summary>
        /// 重置追踪统计
        /// </summary>
        public void ResetStats()
        {
            totalKills = 0;
            totalGoldEarned = 0f;
            totalRecruits = 0;
            maxLevelUnits = 0;
            currentDay = 1;
            currentWave = 0;
            waveStartTime = 0f;
            unitsAtWaveStart = 0;
            waveHadCasualties = false;

            Debug.Log("Achievement tracker stats reset");
        }
    }

    /// <summary>
    /// 追踪统计数据
    /// </summary>
    [System.Serializable]
    public struct TrackerStats
    {
        public int TotalKills;
        public float TotalGoldEarned;
        public int TotalRecruits;
        public int MaxLevelUnits;
        public int CurrentDay;
        public int CurrentWave;
    }
}