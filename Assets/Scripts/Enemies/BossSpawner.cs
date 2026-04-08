using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// Boss生成器 - 管理Boss出现和战斗流程
    /// AI可以调整Boss生成条件和奖励
    /// </summary>
    public class BossSpawner : MonoBehaviour
    {
        public static BossSpawner Instance { get; private set; }

        [Header("Boss生成设置")]
        public GameObject bossPrefab;
        public bool enableBossBattles = true;
        public int bossWaveInterval = 5; // 每5波出现一个Boss
        public float bossSpawnDelay = 3f;

        [Header("Boss奖励")]
        public float bossGoldReward = 200f;
        public int bossXP = 1000;

        private bool bossActive = false;
        private BossEnemy currentBoss;
        private int wavesSinceLastBoss = 0;

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
            // 监听游戏事件
            if (GameManager.Instance != null)
            {
                // 这里可以订阅波次完成事件
            }
        }

        /// <summary>
        /// 检查是否应该生成Boss
        /// </summary>
        public void CheckBossSpawn(int currentWave)
        {
            if (!enableBossBattles || bossActive)
                return;

            wavesSinceLastBoss++;

            if (wavesSinceLastBoss >= bossWaveInterval)
            {
                StartCoroutine(SpawnBossWave());
                wavesSinceLastBoss = 0;
            }
        }

        /// <summary>
        /// 生成Boss波次
        /// </summary>
        IEnumerator SpawnBossWave()
        {
            bossActive = true;

            // 显示Boss警告
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.ShowMessage("⚠️ BOSS WARNING ⚠️", 5f);
            }

            yield return new WaitForSeconds(bossSpawnDelay);

            // 生成Boss
            SpawnBoss();

            // 等待Boss被击败
            while (bossActive && currentBoss != null && currentBoss.IsAlive())
            {
                yield return new WaitForSeconds(1f);
            }

            // Boss被击败
            OnBossDefeated();
        }

        /// <summary>
        /// 生成Boss
        /// </summary>
        void SpawnBoss()
        {
            // 随机选择Boss类型
            BossEnemy.BossType[] bossTypes = (BossEnemy.BossType[])System.Enum.GetValues(typeof(BossEnemy.BossType));
            BossEnemy.BossType randomBossType = bossTypes[Random.Range(0, bossTypes.Length)];

            // 根据当前天数确定Boss等级
            int bossLevel = Mathf.Max(1, GameManager.Instance.currentDay);

            // 生成位置
            Vector3 spawnPos = GetBossSpawnPosition();

            // 创建Boss对象
            GameObject bossObj = new GameObject($"Boss_{randomBossType}");
            bossObj.transform.position = spawnPos;

            // 添加Boss组件
            currentBoss = bossObj.AddComponent<BossEnemy>();
            // 设置Boss属性（通过公共字段或方法）
            // 由于BossEnemy的这些字段是public，我们可以直接访问
            var bossField = currentBoss.GetType().GetField("bossType");
            if (bossField != null)
            {
                bossField.SetValue(currentBoss, randomBossType);
            }

            var levelField = currentBoss.GetType().GetField("bossLevel");
            if (levelField != null)
            {
                levelField.SetValue(currentBoss, bossLevel);
            }

            Debug.Log($"Boss spawned: {randomBossType} (Level {bossLevel})");

            // 播放Boss生成音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound(); // 临时使用选择音效
            }

            // 显示Boss信息
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.ShowMessage($"Boss: {randomBossType}\nLevel {bossLevel}", 3f);
            }
        }

        /// <summary>
        /// 获取Boss生成位置
        /// </summary>
        Vector3 GetBossSpawnPosition()
        {
            // 在地图边缘生成
            float spawnDistance = 20f;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            Vector3 spawnPos = new Vector3(
                Mathf.Cos(angle) * spawnDistance,
                0f,
                Mathf.Sin(angle) * spawnDistance
            );

            return spawnPos;
        }

        /// <summary>
        /// Boss被击败
        /// </summary>
        void OnBossDefeated()
        {
            bossActive = false;

            // 给予奖励
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(bossGoldReward);
            }

            // 显示胜利消息
            if (UI.UIManager.Instance != null)
            {
                UI.UIManager.Instance.ShowMessage("Boss Defeated! +200 Gold", 3f);
            }

            // 播放胜利音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound();
            }

            // 通知成就系统
            if (Achievements.AchievementTracker.Instance != null)
            {
                // 可以添加Boss击杀成就
            }

            Debug.Log("Boss defeated! Rewards granted.");
        }

        /// <summary>
        /// 强制生成Boss（用于测试）
        /// </summary>
        public void ForceSpawnBoss(BossEnemy.BossType bossType = BossEnemy.BossType.Warlord)
        {
            if (bossActive)
            {
                Debug.LogWarning("Boss already active!");
                return;
            }

            StartCoroutine(SpawnSpecificBoss(bossType));
        }

        IEnumerator SpawnSpecificBoss(BossEnemy.BossType bossType)
        {
            bossActive = true;

            yield return new WaitForSeconds(1f);

            // 创建Boss对象
            GameObject bossObj = new GameObject($"TestBoss_{bossType}");
            bossObj.transform.position = Vector3.forward * 15f;

            currentBoss = bossObj.AddComponent<BossEnemy>();

            var bossField = currentBoss.GetType().GetField("bossType");
            if (bossField != null)
            {
                bossField.SetValue(currentBoss, bossType);
            }

            Debug.Log($"Test boss spawned: {bossType}");
        }

        /// <summary>
        /// 设置Boss生成间隔
        /// </summary>
        public void SetBossWaveInterval(int interval)
        {
            bossWaveInterval = Mathf.Max(1, interval);
            Debug.Log($"Boss wave interval set to: {bossWaveInterval}");
        }

        /// <summary>
        /// 设置Boss奖励
        /// </summary>
        public void SetBossRewards(float gold, int xp)
        {
            bossGoldReward = gold;
            bossXP = xp;
            Debug.Log($"Boss rewards set to: {gold} gold, {xp} XP");
        }

        /// <summary>
        /// 启用/禁用Boss战
        /// </summary>
        public void SetBossBattlesEnabled(bool enabled)
        {
            enableBossBattles = enabled;
            Debug.Log($"Boss battles {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 获取Boss状态
        /// </summary>
        public BossStatus GetBossStatus()
        {
            return new BossStatus
            {
                BossActive = bossActive,
                CurrentBossType = currentBoss != null ? GetCurrentBossType() : BossEnemy.BossType.Warlord,
                WavesUntilNextBoss = bossWaveInterval - wavesSinceLastBoss,
                BossLevel = GameManager.Instance != null ? GameManager.Instance.currentDay : 1
            };
        }

        BossEnemy.BossType GetCurrentBossType()
        {
            if (currentBoss == null)
                return BossEnemy.BossType.Warlord;

            var bossField = currentBoss.GetType().GetField("bossType");
            if (bossField != null)
            {
                return (BossEnemy.BossType)bossField.GetValue(currentBoss);
            }

            return BossEnemy.BossType.Warlord;
        }
    }

    /// <summary>
    /// Boss状态信息
    /// </summary>
    [System.Serializable]
    public struct BossStatus
    {
        public bool BossActive;
        public BossEnemy.BossType CurrentBossType;
        public int WavesUntilNextBoss;
        public int BossLevel;
    }
}