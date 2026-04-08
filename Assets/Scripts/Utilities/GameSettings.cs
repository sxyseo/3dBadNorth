using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 游戏平衡性和难度设置
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "BadNorth3D/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("玩家单位设置")]
        public UnitSettings playerUnit = new UnitSettings
        {
            maxHealth = 100f,
            moveSpeed = 5f,
            attackDamage = 20f,
            attackRange = 2f,
            attackCooldown = 1f,
            recruitmentCost = 20f
        };

        [Header("敌人设置")]
        public EnemySettings enemy = new EnemySettings
        {
            baseHealth = 50f,
            baseDamage = 10f,
            baseMoveSpeed = 3f,
            healthScalingPerWave = 10f,
            damageScalingPerWave = 2f,
            goldReward = 5f
        };

        [Header("经济设置")]
        public EconomySettings economy = new EconomySettings
        {
            startingGold = 100f,
            goldPerEnemyKill = 5f,
            goldPerWaveComplete = 20f,
            goldPerDayComplete = 50f,
            waveCompletionBonusScaling = 5f
        };

        [Header("波次设置")]
        public WaveSettings wave = new WaveSettings
        {
            baseEnemyCount = 5,
            enemiesPerWaveLevel = 3,
            enemiesPerDayLevel = 2,
            wavesPerDay = 5,
            spawnInterval = 2f,
            spawnIntervalScaling = 0.2f
        };

        [System.Serializable]
        public class UnitSettings
        {
            public float maxHealth;
            public float moveSpeed;
            public float attackDamage;
            public float attackRange;
            public float attackCooldown;
            public float recruitmentCost;
        }

        [System.Serializable]
        public class EnemySettings
        {
            public float baseHealth;
            public float baseDamage;
            public float baseMoveSpeed;
            public float healthScalingPerWave;
            public float damageScalingPerWave;
            public float goldReward;
        }

        [System.Serializable]
        public class EconomySettings
        {
            public float startingGold;
            public float goldPerEnemyKill;
            public float goldPerWaveComplete;
            public float goldPerDayComplete;
            public float waveCompletionBonusScaling;
        }

        [System.Serializable]
        public class WaveSettings
        {
            public int baseEnemyCount;
            public int enemiesPerWaveLevel;
            public int enemiesPerDayLevel;
            public int wavesPerDay;
            public float spawnInterval;
            public float spawnIntervalScaling;
        }
    }
}
