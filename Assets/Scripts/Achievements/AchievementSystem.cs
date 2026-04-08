using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BadNorth3D.Achievements
{
    /// <summary>
    /// 成就系统 - 追踪玩家成就和里程碑
    /// AI可以轻松添加新成就和调整解锁条件
    /// </summary>
    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }

        [Header("成就设置")]
        public bool showAchievementNotifications = true;
        public float notificationDuration = 3f;

        // 成就数据
        private Dictionary<string, Achievement> achievements;
        private Dictionary<string, AchievementProgress> progress;

        // 成就解锁事件
        public System.Action<Achievement> OnAchievementUnlocked;

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
        }

        void Start()
        {
            InitializeAchievements();
            LoadAchievementProgress();
        }

        /// <summary>
        /// 初始化所有成就
        /// </summary>
        void InitializeAchievements()
        {
            achievements = new Dictionary<string, Achievement>();
            progress = new Dictionary<string, AchievementProgress>();

            // 战斗成就
            AddAchievement(new Achievement
            {
                Id = "first_blood",
                Name = "初露锋芒",
                Description = "击杀第一个敌人",
                Category = AchievementCategory.Combat,
                Type = AchievementType.Single,
                IconColor = Color.red,
                Reward = 10f
            });

            AddAchievement(new Achievement
            {
                Id = "killer_100",
                Name = "百人斩",
                Description = "累计击杀100个敌人",
                Category = AchievementCategory.Combat,
                Type = AchievementType.Progressive,
                Target = 100,
                IconColor = Color.red,
                Reward = 50f
            });

            AddAchievement(new Achievement
            {
                Id = "killer_1000",
                Name = "千人斩",
                Description = "累计击杀1000个敌人",
                Category = AchievementCategory.Combat,
                Type = AchievementType.Progressive,
                Target = 1000,
                IconColor = new Color(0.8f, 0.2f, 0.2f),
                Reward = 200f
            });

            // 经济成就
            AddAchievement(new Achievement
            {
                Id = "rich_1000",
                Name = "小有积蓄",
                Description = "累计获得1000金币",
                Category = AchievementCategory.Economy,
                Type = AchievementType.Progressive,
                Target = 1000,
                IconColor = Color.yellow,
                Reward = 30f
            });

            AddAchievement(new Achievement
            {
                Id = "rich_10000",
                Name = "财源滚滚",
                Description = "累计获得10000金币",
                Category = AchievementCategory.Economy,
                Type = AchievementType.Progressive,
                Target = 10000,
                IconColor = new Color(1f, 0.8f, 0f),
                Reward = 150f
            });

            // 单位成就
            AddAchievement(new Achievement
            {
                Id = "recruit_10",
                Name = "组建小队",
                Description = "累计招募10个单位",
                Category = AchievementCategory.Units,
                Type = AchievementType.Progressive,
                Target = 10,
                IconColor = Color.blue,
                Reward = 25f
            });

            AddAchievement(new Achievement
                {
                Id = "upgrade_max",
                Name = "精锐部队",
                Description = "将一个单位升级到满级",
                Category = AchievementCategory.Units,
                Type = AchievementType.Single,
                IconColor = Color.cyan,
                Reward = 40f
            });

            // 进度成就
            AddAchievement(new Achievement
            {
                Id = "survive_day_5",
                Name = "生存专家",
                Description = "存活到第5天",
                Category = AchievementCategory.Progress,
                Type = AchievementType.Progressive,
                Target = 5,
                IconColor = Color.green,
                Reward = 35f
            });

            AddAchievement(new Achievement
            {
                Id = "survive_day_20",
                Name = "岛屿守护者",
                Description = "存活到第20天",
                Category = AchievementCategory.Progress,
                Type = AchievementType.Progressive,
                Target = 20,
                IconColor = new Color(0.2f, 0.8f, 0.2f),
                Reward = 100f
            });

            // 特殊成就
            AddAchievement(new Achievement
            {
                Id = "no_casualties",
                Name = "完美防守",
                Description = "在不损失任何单位的情况下完成一波",
                Category = AchievementCategory.Special,
                Type = AchievementType.Single,
                IconColor = Color.white,
                Reward = 60f
            });

            AddAchievement(new Achievement
            {
                Id = "speed_clear",
                Name = "速战速决",
                Description = "在30秒内完成一波",
                Category = AchievementCategory.Special,
                Type = AchievementType.Single,
                IconColor = new Color(1f, 0.5f, 0f),
                Reward = 45f
            });

            Debug.Log($"Initialized {achievements.Count} achievements");
        }

        /// <summary>
        /// 添加成就到系统
        /// </summary>
        void AddAchievement(Achievement achievement)
        {
            achievements[achievement.Id] = achievement;
            progress[achievement.Id] = new AchievementProgress
            {
                AchievementId = achievement.Id,
                CurrentValue = 0f,
                Unlocked = false,
                UnlockTime = System.DateTime.MinValue
            };
        }

        /// <summary>
        /// 更新成就进度
        /// </summary>
        public void UpdateAchievementProgress(string achievementId, float value)
        {
            if (!achievements.ContainsKey(achievementId))
                return;

            Achievement achievement = achievements[achievementId];
            AchievementProgress currentProgress = progress[achievementId];

            // 已经解锁的不处理
            if (currentProgress.Unlocked)
                return;

            switch (achievement.Type)
            {
                case AchievementType.Single:
                    // 一次性成就，直接解锁
                    UnlockAchievement(achievementId);
                    break;

                case AchievementType.Progressive:
                    // 进度成就，更新当前值
                    currentProgress.CurrentValue = Mathf.Max(currentProgress.CurrentValue, value);

                    // 检查是否达成目标
                    if (currentProgress.CurrentValue >= achievement.Target)
                    {
                        UnlockAchievement(achievementId);
                    }
                    break;
            }
        }

        /// <summary>
        /// 增加成就进度（用于累加类型）
        /// </summary>
        public void AddAchievementProgress(string achievementId, float amount)
        {
            if (!achievements.ContainsKey(achievementId))
                return;

            Achievement achievement = achievements[achievementId];
            AchievementProgress currentProgress = progress[achievementId];

            if (currentProgress.Unlocked)
                return;

            if (achievement.Type == AchievementType.Progressive)
            {
                currentProgress.CurrentValue += amount;

                if (currentProgress.CurrentValue >= achievement.Target)
                {
                    UnlockAchievement(achievementId);
                }
            }
        }

        /// <summary>
        /// 解锁成就
        /// </summary>
        void UnlockAchievement(string achievementId)
        {
            if (!achievements.ContainsKey(achievementId))
                return;

            AchievementProgress currentProgress = progress[achievementId];
            if (currentProgress.Unlocked)
                return;

            // 标记为已解锁
            currentProgress.Unlocked = true;
            currentProgress.UnlockTime = System.DateTime.Now;

            Achievement achievement = achievements[achievementId];

            // 发放奖励
            if (EconomyManager.Instance != null && achievement.Reward > 0)
            {
                EconomyManager.Instance.AddGold(achievement.Reward);
            }

            // 显示通知
            if (showAchievementNotifications)
            {
                ShowAchievementNotification(achievement);
            }

            // 触发事件
            OnAchievementUnlocked?.Invoke(achievement);

            // 播放音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound(); // 临时使用选择音效
            }

            Debug.Log($"Achievement unlocked: {achievement.Name}");
        }

        /// <summary>
        /// 显示成就解锁通知
        /// </summary>
        void ShowAchievementNotification(Achievement achievement)
        {
            // 这里可以创建专门的UI通知
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.ShowMessage($"🏆 成就解锁: {achievement.Name}\n{achievement.Description}", notificationDuration);
            }
        }

        /// <summary>
        /// 获取成就信息
        /// </summary>
        public Achievement GetAchievement(string achievementId)
        {
            return achievements.ContainsKey(achievementId) ? achievements[achievementId] : null;
        }

        /// <summary>
        /// 获取成就进度
        /// </summary>
        public AchievementProgress GetProgress(string achievementId)
        {
            return progress.ContainsKey(achievementId) ? progress[achievementId] : default;
        }

        /// <summary>
        /// 获取所有成就
        /// </summary>
        public List<Achievement> GetAllAchievements()
        {
            return achievements.Values.ToList();
        }

        /// <summary>
        /// 获取已解锁成就数量
        /// </summary>
        public int GetUnlockedCount()
        {
            return progress.Values.Count(p => p.Unlocked);
        }

        /// <summary>
        /// 获取成就总数量
        /// </summary>
        public int GetTotalCount()
        {
            return achievements.Count;
        }

        /// <summary>
        /// 按分类获取成就
        /// </summary>
        public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
        {
            return achievements.Values.Where(a => a.Category == category).ToList();
        }

        /// <summary>
        /// 重置所有成就进度（用于测试）
        /// </summary>
        public void ResetAllProgress()
        {
            foreach (var kvp in progress)
            {
                kvp.Value.CurrentValue = 0f;
                kvp.Value.Unlocked = false;
                kvp.Value.UnlockTime = System.DateTime.MinValue;
            }

            Debug.Log("All achievement progress has been reset");
        }

        /// <summary>
        /// 保存成就进度
        /// </summary>
        public void SaveAchievementProgress()
        {
            // 这里可以集成到SaveSystem中
            Debug.Log("Achievement progress saved");
        }

        /// <summary>
        /// 加载成就进度
        /// </summary>
        void LoadAchievementProgress()
        {
            // 这里可以从SaveSystem中加载
            Debug.Log("Achievement progress loaded");
        }

        /// <summary>
        /// 获取成就完成百分比
        /// </summary>
        public float GetCompletionPercentage()
        {
            int total = achievements.Count;
            int unlocked = GetUnlockedCount();
            return total > 0 ? (float)unlocked / total * 100f : 0f;
        }
    }

    // ==================== 成就数据结构 ====================

    /// <summary>
    /// 成就信息
    /// </summary>
    [System.Serializable]
    public class Achievement
    {
        public string Id;
        public string Name;
        public string Description;
        public AchievementCategory Category;
        public AchievementType Type;
        public float Target;
        public Color IconColor;
        public float Reward;
    }

    /// <summary>
    /// 成就进度
    /// </summary>
    [System.Serializable]
    public class AchievementProgress
    {
        public string AchievementId;
        public float CurrentValue;
        public bool Unlocked;
        public System.DateTime UnlockTime;
    }

    /// <summary>
    /// 成就分类
    /// </summary>
    public enum AchievementCategory
    {
        Combat,    // 战斗
        Economy,   // 经济
        Units,     // 单位
        Progress,  // 进度
        Special    // 特殊
    }

    /// <summary>
    /// 成就类型
    /// </summary>
    public enum AchievementType
    {
        Single,       // 一次性
        Progressive   // 进度型
    }
}