using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 单位升级系统 - 允许玩家升级他们的战斗单位
    /// AI可以调整升级参数来改变游戏平衡
    /// </summary>
    public class UnitUpgrade : MonoBehaviour
    {
        [Header("Upgrade Settings")]
        public int maxLevel = 5;
        public int currentLevel = 1;

        [Header("Upgrade Costs")]
        public float[] upgradeCosts = new float[] { 20f, 40f, 60f, 80f, 100f };

        [Header("Stat Increases Per Level")]
        public float healthIncreasePerLevel = 20f;
        public float damageIncreasePerLevel = 5f;
        public float speedIncreasePerLevel = 0.5f;

        private SquadUnitAdvanced squadUnit;

        void Start()
        {
            squadUnit = GetComponent<SquadUnitAdvanced>();
            if (squadUnit != null)
            {
                squadUnit.unitLevel = currentLevel;
            }
        }

        public bool CanUpgrade()
        {
            if (currentLevel >= maxLevel)
                return false;

            if (EconomyManager.Instance == null)
                return false;

            float cost = GetUpgradeCost();
            return EconomyManager.Instance.CanAfford(cost);
        }

        public bool Upgrade()
        {
            if (!CanUpgrade())
                return false;

            float cost = GetUpgradeCost();

            if (EconomyManager.Instance.SpendGold(cost))
            {
                currentLevel++;

                // 通知成就系统
                if (Achievements.AchievementTracker.Instance != null)
                {
                    Achievements.AchievementTracker.Instance.OnUnitUpgraded(currentLevel);
                }

                // 应用升级效果
                ApplyUpgradeBenefits();

                // 播放升级音效
                if (AudioSynthesizer.Instance != null)
                {
                    AudioSynthesizer.Instance.PlaySelectSound();
                }

                Debug.Log($"Unit upgraded to level {currentLevel} for {cost} gold!");
                return true;
            }

            return false;
        }

        void ApplyUpgradeBenefits()
        {
            if (squadUnit == null)
                return;

            // 增加属性
            squadUnit.maxHealth += healthIncreasePerLevel;
            squadUnit.currentHealth = squadUnit.maxHealth; // 升级后回满血
            squadUnit.attackDamage += damageIncreasePerLevel;
            squadUnit.moveSpeed += speedIncreasePerLevel;

            // 更新NavMeshAgent速度
            var navAgent = squadUnit.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.speed = squadUnit.moveSpeed;
            }

            // 更新血条
            var healthBar = squadUnit.GetComponentInChildren<HealthBar>();
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(squadUnit.maxHealth);
                healthBar.SetHealth(squadUnit.currentHealth);
            }

            // 更新单位等级（显示视觉标识）
            squadUnit.unitLevel = currentLevel;

            // 重新创建视觉外观以显示等级标识
            // 注意：这需要修改SquadUnitAdvanced的CreateUnitVisual方法
        }

        public float GetUpgradeCost()
        {
            if (currentLevel >= maxLevel)
                return float.MaxValue;

            int costIndex = Mathf.Min(currentLevel - 1, upgradeCosts.Length - 1);
            return upgradeCosts[costIndex];
        }

        public float GetNextUpgradeStats()
        {
            if (currentLevel >= maxLevel)
                return 0f;

            return healthIncreasePerLevel + damageIncreasePerLevel + speedIncreasePerLevel;
        }

        // 获取升级信息用于UI显示
        public UpgradeInfo GetUpgradeInfo()
        {
            return new UpgradeInfo
            {
                CurrentLevel = currentLevel,
                MaxLevel = maxLevel,
                CanUpgrade = CanUpgrade(),
                UpgradeCost = GetUpgradeCost(),
                HealthIncrease = healthIncreasePerLevel,
                DamageIncrease = damageIncreasePerLevel,
                SpeedIncrease = speedIncreasePerLevel
            };
        }

        public struct UpgradeInfo
        {
            public int CurrentLevel;
            public int MaxLevel;
            public bool CanUpgrade;
            public float UpgradeCost;
            public float HealthIncrease;
            public float DamageIncrease;
            public float SpeedIncrease;
        }
    }
}