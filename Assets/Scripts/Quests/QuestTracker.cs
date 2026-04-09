using UnityEngine;

namespace BadNorth3D.Quests
{
    /// <summary>
    /// 任务追踪器 - 自动监听游戏事件并更新任务进度
    /// 集成到现有的游戏系统中
    /// </summary>
    public class QuestTracker : MonoBehaviour
    {
        public static QuestTracker Instance { get; private set; }

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
            // 注册游戏事件监听
            RegisterGameEvents();
        }

        void RegisterGameEvents()
        {
            Debug.Log("Quest tracker initialized");
        }

        void Update()
        {
            // 定期检查任务状态
            CheckQuestConditions();
        }

        void CheckQuestConditions()
        {
            if (QuestSystem.Instance == null)
                return;

            // 检查天数相关目标
            if (GameManager.Instance != null)
            {
                int currentDay = GameManager.Instance.currentDay;
                QuestSystem.Instance.UpdateQuestObjective("reach_day", currentDay);
                QuestSystem.Instance.UpdateQuestObjective("reach_day_3", currentDay >= 3 ? 1f : 0f);
            }

            // 检查经济相关目标
            if (EconomyManager.Instance != null)
            {
                float currentGold = EconomyManager.Instance.GetCurrentGold();
                QuestSystem.Instance.UpdateQuestObjective("collect_500_gold", currentGold);
                QuestSystem.Instance.UpdateQuestObjective("collect_1000_gold", currentGold);
                QuestSystem.Instance.UpdateQuestObjective("collect_1000_gold_daily", currentGold);
            }

            // 检查单位相关目标
            if (GameManager.Instance != null)
            {
                int unitCount = GameManager.Instance.squadUnits.Count;
                QuestSystem.Instance.UpdateQuestObjective("recruit_3_units", unitCount);
            }
        }

        /// <summary>
        /// 敌人被击杀时调用
        /// </summary>
        public void OnEnemyKilled(Enemy.EnemyType enemyType)
        {
            if (QuestSystem.Instance == null)
                return;

            // 更新击杀相关目标
            QuestSystem.Instance.UpdateQuestObjective("kill_5_enemies", 1f);
            QuestSystem.Instance.UpdateQuestObjective("kill_50_enemies_daily", 1f);

            Debug.Log("Quest updated: Enemy killed");
        }

        /// <summary>
        /// 波次完成时调用
        /// </summary>
        public void OnWaveCompleted(int waveNumber, bool hadCasualties)
        {
            if (QuestSystem.Instance == null)
                return;

            // 更新波次相关目标
            QuestSystem.Instance.UpdateQuestObjective("survive_3_waves", 1f);

            // 检查无伤亡完成
            if (!hadCasualties)
            {
                QuestSystem.Instance.UpdateQuestObjective("complete_wave_no_loss", 1f);
            }

            // 检查快速完成
            if (WaveWasFast(waveNumber))
            {
                QuestSystem.Instance.UpdateQuestObjective("fast_wave_clear", 1f);
            }

            Debug.Log($"Quest updated: Wave {waveNumber} completed");
        }

        /// <summary>
        /// Boss被击败时调用
        /// </summary>
        public void OnBossDefeated(BossEnemy.BossType bossType, bool hadCasualties)
        {
            if (QuestSystem.Instance == null)
                return;

            // 更新Boss相关目标
            QuestSystem.Instance.UpdateQuestObjective("defeat_boss", 1f);

            // 检查无伤亡击败Boss
            if (!hadCasualties)
            {
                QuestSystem.Instance.UpdateQuestObjective("no_casualties", 1f);
            }

            Debug.Log($"Quest updated: Boss {bossType} defeated");
        }

        /// <summary>
        /// 单位被招募时调用
        /// </summary>
        public void OnUnitRecruited(SquadUnitType unitType)
        {
            if (QuestSystem.Instance == null)
                return;

            // 更新招募相关目标
            // 这里可以具体检查单位类型
            QuestSystem.Instance.UpdateQuestObjective("recruit_3_units", 1f);

            Debug.Log($"Quest updated: Unit {unitType} recruited");
        }

        bool WaveWasFast(int waveNumber)
        {
            // 这里需要实际的波次时间数据
            // 暂时简化处理
            return false;
        }

        /// <summary>
        /// 获取当前任务进度信息
        /// </summary>
        public QuestProgressInfo GetQuestProgress()
        {
            if (QuestSystem.Instance == null)
            {
                return new QuestProgressInfo();
            }

            Quest[] activeQuests = QuestSystem.Instance.GetActiveQuests();
            Quest[] availableQuests = QuestSystem.Instance.GetAvailableQuests();
            Quest[] completedQuests = QuestSystem.Instance.GetCompletedQuests();

            return new QuestProgressInfo
            {
                ActiveQuestCount = activeQuests.Length,
                AvailableQuestCount = availableQuests.Length,
                CompletedQuestCount = completedQuests.Length,
                TotalQuestCount = activeQuests.Length + availableQuests.Length + completedQuests.Length,
                CurrentChapter = GetCurrentChapter(),
                StoryProgress = GetStoryProgress()
            };
        }

        int GetCurrentChapter()
        {
            Quest[] activeQuests = QuestSystem.Instance.GetActiveQuests();
            if (activeQuests.Length > 0)
            {
                return activeQuests[0].Chapter;
            }
            return 1;
        }

        float GetStoryProgress()
        {
            Quest[] completedQuests = QuestSystem.Instance.GetCompletedQuests();
            // 简化计算：根据完成任务数量估算进度
            return Mathf.Min(1f, completedQuests.Length / 10f);
        }
    }

    /// <summary>
    /// 任务进度信息
    /// </summary>
    [System.Serializable]
    public struct QuestProgressInfo
    {
        public int ActiveQuestCount;
        public int AvailableQuestCount;
        public int CompletedQuestCount;
        public int TotalQuestCount;
        public int CurrentChapter;
        public float StoryProgress;
    }
}