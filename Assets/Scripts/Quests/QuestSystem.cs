using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BadNorth3D.Quests
{
    /// <summary>
    /// 任务系统 - 管理主线任务、支线任务和剧情对话
    /// AI可以轻松创建新任务和剧情分支
    /// </summary>
    public class QuestSystem : MonoBehaviour
    {
        public static QuestSystem Instance { get; private set; }

        [Header("任务设置")]
        public bool showQuestNotifications = true;
        public float notificationDuration = 4f;

        // 任务数据
        private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
        private Dictionary<string, Quest> completedQuests = new Dictionary<string, Quest>();
        private Dictionary<string, Quest> availableQuests = new Dictionary<string, Quest>();

        // 当前剧情状态
        private int currentChapter = 1;
        private int storyProgress = 0;

        // 任务事件
        public System.Action<Quest> OnQuestStarted;
        public System.Action<Quest> OnQuestCompleted;
        public System.Action<Quest, QuestObjective> OnObjectiveUpdated;

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
            }
        }

        void Start()
        {
            InitializeQuests();
            StartMainStory();
        }

        /// <summary>
        /// 初始化所有任务
        /// </summary>
        void InitializeQuests()
        {
            // ===== 主线任务 =====
            AddQuestToAvailable(new Quest
            {
                Id = "main_1_defend_island",
                Name = "保卫岛屿",
                Description = "击退第一波入侵的敌人，保护我们的家园",
                Type = QuestType.Main,
                Chapter = 1,
                Objectives = new QuestObjective[]
                {
                    new QuestObjective("kill_5_enemies", "击杀5个敌人", 5, 0),
                    new QuestObjective("survive_3_waves", "存活3波", 3, 0)
                },
                Rewards = new QuestRewards
                {
                    Gold = 100f,
                    Experience = 50f,
                    UnlockUnit = SquadUnitType.Archer // 解锁弓箭手
                },
                NextQuestId = "main_2_explore_island"
            });

            AddQuestToAvailable(new Quest
            {
                Id = "main_2_explore_island",
                Name = "探索岛屿",
                Description = "探索周边区域，收集资源并了解敌人的规模",
                Type = QuestType.Main,
                Chapter = 1,
                Objectives = new QuestObjective[]
                {
                    new QuestObjective("reach_day_3", "存活到第3天", 3, 0),
                    new QuestObjective("collect_500_gold", "收集500金币", 500f, 0f),
                    new QuestObjective("recruit_3_units", "招募3个单位", 3, 0)
                },
                Rewards = new QuestRewards
                {
                    Gold = 150f,
                    Experience = 100f,
                    UnlockUnit = SquadUnitType.Knight // 解锁骑士
                },
                RequiredQuestId = "main_1_defend_island",
                NextQuestId = "main_3_boss_encounter"
            });

            AddQuestToAvailable(new Quest
            {
                Id = "main_3_boss_encounter",
                Name = "Boss遭遇战",
                Description = "一个强大的敌人首领出现了！击败他来保卫岛屿",
                Type = QuestType.Main,
                Chapter = 1,
                Objectives = new QuestObjective[]
                {
                    new QuestObjective("defeat_boss", "击败Boss", 1, 0),
                    new QuestObjective("no_casualties", "无伤亡击败Boss", 1, 0)
                },
                Rewards = new QuestRewards
                {
                    Gold = 300f,
                    Experience = 200f,
                    UnlockUnit = SquadUnitType.Berserker // 解锁狂战士
                },
                RequiredQuestId = "main_2_explore_island",
                NextQuestId = "main_4_new_allies"
            });

            // ===== 支线任务 =====
            AddQuestToAvailable(new Quest
            {
                Id = "side_1_perfect_defense",
                Name = "完美防守",
                Description = "在不损失任何单位的情况下完成一波战斗",
                Type = QuestType.Side,
                Chapter = 1,
                Objectives = new QuestObjective[]
                {
                    new QuestObjective("complete_wave_no_loss", "无伤亡完成一波", 1, 0)
                },
                Rewards = new QuestRewards
                {
                    Gold = 80f,
                    Experience = 40f
                }
            });

            AddQuestToAvailable(new Quest
            {
                Id = "side_2_speed_clear",
                Name = "速战速决",
                Description = "在30秒内完成一波战斗",
                Type = QuestType.Side,
                Chapter = 1,
                Objectives = new QuestObjective[]
                {
                    new QuestObjective("fast_wave_clear", "30秒内完成一波", 1, 0)
                },
                Rewards = new QuestRewards
                {
                    Gold = 60f,
                    Experience = 30f
                }
            });

            AddQuestToAvailable(new Quest
            {
                Id = "side_3_collect_resources",
                Name = "资源收集",
                Description = "收集1000金币来升级装备",
                Type = QuestType.Side,
                Chapter = 1,
                Objectives = new QuestObjective[]
                {
                    new QuestObjective("collect_1000_gold", "收集1000金币", 1000f, 0f)
                },
                Rewards = new QuestRewards
                {
                    Gold = 200f,
                    Experience = 80f
                }
            });

            // ===== 每日任务 =====
            GenerateDailyQuests();

            Debug.Log($"Initialized {availableQuests.Count} quests");
        }

        /// <summary>
        /// 生成每日任务
        /// </summary>
        void GenerateDailyQuests()
        {
            // 这里可以根据日期生成不同的每日任务
            Quest dailyQuest = new Quest
            {
                Id = "daily_" + System.DateTime.Now.ToString("yyyyMMdd"),
                Name = "每日挑战：连续击杀",
                Description = "连续击杀50个敌人",
                Type = QuestType.Daily,
                Chapter = 0,
                Objectives = new QuestObjective[]
                {
                    new QuestObjective("kill_50_enemies_daily", "击杀50个敌人", 50, 0)
                },
                Rewards = new QuestRewards
                {
                    Gold = 50f,
                    Experience = 25f
                },
                IsDaily = true
            };

            AddQuestToAvailable(dailyQuest);
        }

        /// <summary>
        /// 开始主线故事
        /// </summary>
        void StartMainStory()
        {
            ShowQuestDialogue("intro", new DialogueNode
            {
                Speaker = "指挥官",
                Text = "士兵们！敌人正在接近我们的岛屿。我们必须团结一致，保卫家园！\n\n首先，让我们击退第一波入侵者。",
                PortraitColor = new Color(0.2f, 0.4f, 0.8f),
                Choices = new DialogueChoice[]
                {
                    new DialogueChoice("我们准备好战斗了！", "start_combat"),
                    new DialogueChoice("我们需要更多信息", "show_intel")
                }
            });

            // 自动开始第一个主线任务
            StartQuest("main_1_defend_island");
        }

        /// <summary>
        /// 添加任务到可用列表
        /// </summary>
        void AddQuestToAvailable(Quest quest)
        {
            availableQuests[quest.Id] = quest;
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        public void StartQuest(string questId)
        {
            if (!availableQuests.ContainsKey(questId))
            {
                Debug.LogWarning($"Quest not found: {questId}");
                return;
            }

            Quest quest = availableQuests[questId];

            // 检查前置任务
            if (!string.IsNullOrEmpty(quest.RequiredQuestId))
            {
                if (!completedQuests.ContainsKey(quest.RequiredQuestId))
                {
                    Debug.LogWarning($"Required quest not completed: {quest.RequiredQuestId}");
                    return;
                }
            }

            // 移动到活跃任务
            availableQuests.Remove(questId);
            activeQuests[questId] = quest;
            quest.IsActive = true;

            // 显示任务开始通知
            if (showQuestNotifications)
            {
                ShowQuestNotification($"任务开始: {quest.Name}");
            }

            // 触发事件
            OnQuestStarted?.Invoke(quest);

            // 显示任务对话
            ShowQuestDialogue(questId, GetQuestStartDialogue(questId));

            Debug.Log($"Quest started: {quest.Name}");
        }

        /// <summary>
        /// 更新任务目标
        /// </summary>
        public void UpdateQuestObjective(string objectiveType, float progress)
        {
            foreach (var quest in activeQuests.Values)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Id == objectiveType || objective.Id.StartsWith(objectiveType))
                    {
                        objective.CurrentProgress = Mathf.Min(objective.CurrentProgress + progress, objective.TargetValue);
                        objective.IsCompleted = objective.CurrentProgress >= objective.TargetValue;

                        // 触发目标更新事件
                        OnObjectiveUpdated?.Invoke(quest, objective);

                        // 检查任务是否完成
                        CheckQuestCompletion(quest);
                    }
                }
            }
        }

        /// <summary>
        /// 检查任务完成
        /// </summary>
        void CheckQuestCompletion(Quest quest)
        {
            bool allObjectivesCompleted = quest.Objectives.All(obj => obj.IsCompleted);

            if (allObjectivesCompleted && !quest.IsCompleted)
            {
                CompleteQuest(quest.Id);
            }
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        void CompleteQuest(string questId)
        {
            if (!activeQuests.ContainsKey(questId))
                return;

            Quest quest = activeQuests[questId];
            quest.IsCompleted = true;

            // 发放奖励
            GiveQuestRewards(quest);

            // 移动到已完成任务
            activeQuests.Remove(questId);
            completedQuests[questId] = quest;

            // 显示完成通知
            if (showQuestNotifications)
            {
                ShowQuestNotification($"任务完成: {quest.Name}");
            }

            // 触发事件
            OnQuestCompleted?.Invoke(quest);

            // 显示完成对话
            ShowQuestDialogue(questId, GetQuestCompleteDialogue(questId));

            // 检查并开始下一个任务
            if (!string.IsNullOrEmpty(quest.NextQuestId))
            {
                StartQuest(quest.NextQuestId);
            }

            Debug.Log($"Quest completed: {quest.Name}");
        }

        /// <summary>
        /// 发放任务奖励
        /// </summary>
        void GiveQuestRewards(Quest quest)
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(quest.Rewards.Gold);
            }

            // 解锁新单位
            if (quest.Rewards.UnlockUnit != SquadUnitType.Warrior) // Warrior是默认单位
            {
                UnlockUnit(quest.Rewards.UnlockUnit);
            }

            Debug.Log($"Quest rewards given: {quest.Rewards.Gold} gold, {quest.Rewards.Experience} XP");
        }

        /// <summary>
        /// 解锁新单位
        /// </summary>
        void UnlockUnit(SquadUnitType unitType)
        {
            // 这里可以通知UI更新，显示新单位解锁
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.ShowMessage($"新单位解锁: {unitType}!", 3f);
            }

            Debug.Log($"Unit unlocked: {unitType}");
        }

        /// <summary>
        /// 获取任务开始对话
        /// </summary>
        DialogueNode GetQuestStartDialogue(string questId)
        {
            return questId switch
            {
                "main_1_defend_island" => new DialogueNode
                {
                    Speaker = "指挥官",
                    Text = "敌人来了！准备战斗！我们需要击杀5个敌人并存活3波攻击。",
                    PortraitColor = new Color(0.2f, 0.4f, 0.8f),
                    Choices = new DialogueChoice[]
                    {
                        new DialogueChoice("为了家园！", "accept_quest"),
                        new DialogueChoice("让我们做好准备", "delay_quest")
                    }
                },
                "main_2_explore_island" => new DialogueNode
                {
                    Speaker = "侦察兵",
                    Text = "指挥官！我们在周边发现了更多敌人。我们需要收集资源和扩充军队来应对接下来的挑战。",
                    PortraitColor = new Color(0.3f, 0.8f, 0.3f),
                    Choices = new DialogueChoice[]
                    {
                        new DialogueChoice("立即开始探索", "accept_quest")
                    }
                },
                _ => null
            };
        }

        /// <summary>
        /// 获取任务完成对话
        /// </summary>
        DialogueNode GetQuestCompleteDialogue(string questId)
        {
            return questId switch
            {
                "main_1_defend_island" => new DialogueNode
                {
                    Speaker = "指挥官",
                    Text = "干得好，士兵们！我们成功击退了第一波攻击。但这只是开始...",
                    PortraitColor = new Color(0.2f, 0.4f, 0.8f),
                    Choices = new DialogueChoice[]
                    {
                        new DialogueChoice("继续战斗", "continue_story")
                    }
                },
                _ => null
            };
        }

        /// <summary>
        /// 显示任务对话
        /// </summary>
        void ShowQuestDialogue(string dialogueId, DialogueNode node)
        {
            if (node == null) return;

            if (UI.UIManager.Instance != null)
            {
                string dialogueText = $"<color=#{ColorUtility.ToHtmlStringRGBA(node.PortraitColor)}>{node.Speaker}:</color>\n{node.Text}";
                UI.UIManager.Instance.ShowMessage(dialogueText, notificationDuration);
            }
        }

        /// <summary>
        /// 显示任务通知
        /// </summary>
        void ShowQuestNotification(string message)
        {
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.ShowMessage($"📜 {message}", notificationDuration);
            }
        }

        /// <summary>
        /// 获取活跃任务
        /// </summary>
        public Quest[] GetActiveQuests()
        {
            return activeQuests.Values.ToArray();
        }

        /// <summary>
        /// 获取可用任务
        /// </summary>
        public Quest[] GetAvailableQuests()
        {
            return availableQuests.Values.ToArray();
        }

        /// <summary>
        /// 获取已完成任务
        /// </summary>
        public Quest[] GetCompletedQuests()
        {
            return completedQuests.Values.ToArray();
        }

        /// <summary>
        /// 获取任务进度百分比
        /// </summary>
        public float GetQuestProgress(string questId)
        {
            if (activeQuests.ContainsKey(questId))
            {
                Quest quest = activeQuests[questId];
                if (quest.Objectives.Length == 0) return 1f;

                int completedObjectives = quest.Objectives.Count(obj => obj.IsCompleted);
                return (float)completedObjectives / quest.Objectives.Length;
            }
            return 0f;
        }
    }

    // ==================== 任务数据结构 ====================

    /// <summary>
    /// 任务信息
    /// </summary>
    [System.Serializable]
    public class Quest
    {
        public string Id;
        public string Name;
        public string Description;
        public QuestType Type;
        public int Chapter;
        public QuestObjective[] Objectives;
        public QuestRewards Rewards;
        public string RequiredQuestId;  // 前置任务ID
        public string NextQuestId;      // 后续任务ID
        public bool IsActive;
        public bool IsCompleted;
        public bool IsDaily;
    }

    /// <summary>
    /// 任务目标
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        public string Id;
        public string Description;
        public float TargetValue;
        public float CurrentProgress;
        public bool IsCompleted;
    }

    /// <summary>
    /// 任务奖励
    /// </summary>
    [System.Serializable]
    public class QuestRewards
    {
        public float Gold;
        public float Experience;
        public SquadUnitType UnlockUnit; // 解锁新单位
        public string[] UnlockItems;      // 解锁物品
    }

    /// <summary>
    /// 任务类型
    /// </summary>
    public enum QuestType
    {
        Main,   // 主线任务
        Side,   // 支线任务
        Daily,  // 每日任务
        Event   // 事件任务
    }

    /// <summary>
    /// 对话节点
    /// </summary>
    [System.Serializable]
    public class DialogueNode
    {
        public string Speaker;
        public string Text;
        public Color PortraitColor;
        public DialogueChoice[] Choices;
    }

    /// <summary>
    /// 对话选项
    /// </summary>
    [System.Serializable]
    public class DialogueChoice
    {
        public string Text;
        public string Action; // 触发的动作ID
    }
}