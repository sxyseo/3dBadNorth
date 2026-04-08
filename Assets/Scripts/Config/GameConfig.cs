using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 游戏配置中心 - AI友好的集中配置系统
    /// 所有游戏参数都在这里，AI可以通过对话轻松修改
    /// </summary>
    public static partial class GameConfig
    {
        // ==================== 玩家单位配置 ====================
        public static class Units
        {
            // 基础属性
            public const float MAX_HEALTH = 100f;
            public const float MOVE_SPEED = 5f;
            public const float ATTACK_DAMAGE = 20f;
            public const float ATTACK_RANGE = 2f;
            public const float ATTACK_COOLDOWN = 1f;

            // 经济
            public const float RECRUIT_COST = 20f;
            public const int MAX_SQUAD_SIZE = 12;

            // 视觉
            public static readonly Color UNIT_COLOR = new Color(0.2f, 0.4f, 0.8f);
            public static readonly Vector3 UNIT_SCALE = Vector3.one;
        }

        // ==================== 敌人配置 ====================
        public static class Enemies
        {
            // 4种敌人类型的完整配置
            public static readonly EnemyTypeConfig[] TYPES = new[]
            {
                // 普通战士（红色）
                new EnemyTypeConfig
                {
                    Type = EnemyType.Normal,
                    Name = "普通战士",
                    Color = new Color(1f, 0.2f, 0.2f),
                    Scale = new Vector3(1f, 1f, 1f),
                    HealthMultiplier = 1f,
                    SpeedMultiplier = 1f,
                    DamageMultiplier = 1f,
                    AttackRange = 1.5f,
                    Description = "平衡型敌人"
                },

                // 快速轻型（橙色）
                new EnemyTypeConfig
                {
                    Type = EnemyType.Fast,
                    Name = "快速轻型",
                    Color = new Color(1f, 0.6f, 0.2f),
                    Scale = new Vector3(0.7f, 0.7f, 0.7f),
                    HealthMultiplier = 0.7f,
                    SpeedMultiplier = 1.6f,
                    DamageMultiplier = 0.8f,
                    AttackRange = 1.5f,
                    Description = "高速低血量"
                },

                // 重型坦克（紫色）
                new EnemyTypeConfig
                {
                    Type = EnemyType.Heavy,
                    Name = "重型坦克",
                    Color = new Color(0.6f, 0.2f, 0.6f),
                    Scale = new Vector3(1.4f, 1.4f, 1.4f),
                    HealthMultiplier = 1.8f,
                    SpeedMultiplier = 0.6f,
                    DamageMultiplier = 1.4f,
                    AttackRange = 1.5f,
                    Description = "低速高血量"
                },

                // 远程弓箭手（青色）
                new EnemyTypeConfig
                {
                    Type = EnemyType.Ranged,
                    Name = "远程弓箭手",
                    Color = new Color(0.2f, 0.8f, 1f),
                    Scale = new Vector3(0.9f, 1.2f, 0.9f),
                    HealthMultiplier = 0.6f,
                    SpeedMultiplier = 0.8f,
                    DamageMultiplier = 0.7f,
                    AttackRange = 8f,
                    Description = "远程攻击"
                }
            };

            // 波次增长配置
            public const float HEALTH_PER_WAVE = 10f;
            public const float DAMAGE_PER_WAVE = 2f;
            public const float GOLD_PER_WAVE = 1f;
        }

        // ==================== 游戏流程配置 ====================
        public static class Gameplay
        {
            // 天数和波次
            public const int START_DAY = 1;
            public const int WAVES_PER_DAY = 5;

            // 经济
            public const float START_GOLD = 100f;
            public const float GOLD_PER_ENEMY_BASE = 5f;
            public const float WAVE_COMPLETE_BONUS = 20f;
            public const float DAY_COMPLETE_BONUS = 50f;

            // 生成
            public const float ENEMY_WAVE_DELAY = 3f;
            public const float WAVE_REST_TIME = 5f;
        }

        // ==================== 音频配置 ====================
        public static class Audio
        {
            // 主音量
            public const float MASTER_VOLUME = 0.5f;
            public const float SFX_VOLUME = 0.7f;
            public const float MUSIC_VOLUME = 0.3f;

            // 音乐生成
            public const int MUSIC_TEMPO = 120;
            public const int MUSIC_SCALE_BASE = 261; // C4

            // 音效参数
            public const float ATTACK_SOUND_FREQ = 800f;
            public const float HIT_SOUND_FREQ = 200f;
            public const float DEATH_SOUND_FREQ = 400f;
        }

        // ==================== UI配置 ====================
        public static class UI
        {
            // 颜色
            public static readonly Color PRIMARY_COLOR = new Color(0.2f, 0.6f, 0.2f);
            public static readonly Color WARNING_COLOR = new Color(1f, 0.6f, 0.2f);
            public static readonly Color DANGER_COLOR = new Color(1f, 0.2f, 0.2f);

            // 动画
            public const float UI_ANIMATION_SPEED = 0.3f;
            public const float MESSAGE_DISPLAY_TIME = 2f;
        }

        // ==================== 战斗配置 ====================
        public static class Combat
        {
            // 伤害计算
            public const float CRITICAL_HIT_CHANCE = 0.1f;
            public const float CRITICAL_HIT_MULTIPLIER = 2f;

            // 视觉反馈
            public const float HIT_FLASH_DURATION = 0.1f;
            public const float DEATH_ANIMATION_DURATION = 0.5f;

            // 范围效果
            public const float ATTACK_THRUST_DISTANCE = 0.5f;
            public const float ATTACK_ANIMATION_DURATION = 0.15f;
        }
    }

    // ==================== 配置数据结构 ====================

    /// <summary>
    /// 敌人类型配置
    /// </summary>
    public class EnemyTypeConfig
    {
        public EnemyType Type;
        public string Name;
        public Color Color;
        public Vector3 Scale;
        public float HealthMultiplier;
        public float SpeedMultiplier;
        public float DamageMultiplier;
        public float AttackRange;
        public string Description;
    }

    /// <summary>
    /// 敌人类型枚举
    /// </summary>
    public enum EnemyType
    {
        Normal,
        Fast,
        Heavy,
        Ranged
    }
}
