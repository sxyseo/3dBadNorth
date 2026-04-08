using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 单位类型配置 - 程序化生成不同类型的战斗单位
    /// AI可以轻松添加新的单位类型或调整现有单位的属性
    /// </summary>
    public static class UnitTypesConfig
    {
        // ==================== 玩家单位类型 ====================

        public static readonly SquadUnitTypeConfig[] PLAYER_UNITS = new[]
        {
            // 战士（基础型）
            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Warrior,
                Name = "战士",
                Description = "平衡型近战单位",
                Color = new Color(0.2f, 0.4f, 0.8f),
                MaxHealth = 100f,
                MoveSpeed = 5f,
                AttackDamage = 20f,
                AttackRange = 2f,
                AttackCooldown = 1f,
                ModelScale = new Vector3(1f, 1f, 1f),
                WeaponType = WeaponType.Sword,
                Cost = 20f
            },

            // 弓箭手（远程型）
            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Archer,
                Name = "弓箭手",
                Description = "远程攻击单位",
                Color = new Color(0.2f, 0.8f, 0.4f),
                MaxHealth = 70f,
                MoveSpeed = 4.5f,
                AttackDamage = 15f,
                AttackRange = 8f,
                AttackCooldown = 1.5f,
                ModelScale = new Vector3(0.9f, 1.1f, 0.9f),
                WeaponType = WeaponType.Bow,
                Cost = 25f
            },

            // 骑士（坦克型）
            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Knight,
                Name = "骑士",
                Description = "高血量坦克单位",
                Color = new Color(0.4f, 0.6f, 0.9f),
                MaxHealth = 150f,
                MoveSpeed = 4f,
                AttackDamage = 25f,
                AttackRange = 2f,
                AttackCooldown = 1.2f,
                ModelScale = new Vector3(1.2f, 1f, 1.2f),
                WeaponType = WeaponType.SwordShield,
                Cost = 30f
            },

            // 狂战士（高伤害型）
            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Berserker,
                Name = "狂战士",
                Description = "高伤害低血量单位",
                Color = new Color(0.8f, 0.3f, 0.3f),
                MaxHealth = 60f,
                MoveSpeed = 6f,
                AttackDamage = 35f,
                AttackRange = 2f,
                AttackCooldown = 0.8f,
                ModelScale = new Vector3(1.1f, 1f, 1.1f),
                WeaponType = WeaponType.Axe,
                Cost = 35f
            }
        };

        // ==================== 武器类型配置 ====================

        public static class Weapons
        {
            public static readonly WeaponConfig SWORD = new WeaponConfig
            {
                Name = "剑",
                Damage = 1f,
                Range = 1f,
                AttackSpeed = 1f,
                VisualScale = new Vector3(0.1f, 0.8f, 0.1f)
            };

            public static readonly WeaponConfig BOW = new WeaponConfig
            {
                Name = "弓",
                Damage = 0.75f,
                Range = 4f,
                AttackSpeed = 0.8f,
                VisualScale = new Vector3(0.05f, 0.6f, 0.05f),
                ProjectileSpeed = 15f
            };

            public static readonly WeaponConfig SWORD_SHIELD = new WeaponConfig
            {
                Name = "剑盾",
                Damage = 0.9f,
                Range = 1f,
                AttackSpeed = 0.9f,
                VisualScale = new Vector3(0.1f, 0.8f, 0.1f),
                Defense = 1.3f
            };

            public static readonly WeaponConfig AXE = new WeaponConfig
            {
                Name = "斧",
                Damage = 1.4f,
                Range = 1.2f,
                AttackSpeed = 1.2f,
                VisualScale = new Vector3(0.15f, 0.7f, 0.15f)
            };
        }
    }

    // ==================== 单位配置数据结构 ====================

    /// <summary>
    /// 小队单位类型配置
    /// </summary>
    public class SquadUnitTypeConfig
    {
        public SquadUnitType Type;
        public string Name;
        public string Description;
        public Color Color;
        public float MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackRange;
        public float AttackCooldown;
        public Vector3 ModelScale;
        public WeaponType WeaponType;
        public float Cost;
    }

    /// <summary>
    /// 武器配置
    /// </summary>
    public class WeaponConfig
    {
        public string Name;
        public float Damage;
        public float Range;
        public float AttackSpeed;
        public Vector3 VisualScale;
        public float ProjectileSpeed;
        public float Defense;
    }

    /// <summary>
    /// 单位类型枚举
    /// </summary>
    public enum SquadUnitType
    {
        Warrior,    // 战士
        Archer,     // 弓箭手
        Knight,     // 骑士
        Berserker   // 狂战士
    }

    /// <summary>
    /// 武器类型枚举
    /// </summary>
    public enum WeaponType
    {
        Sword,
        Bow,
        SwordShield,
        Axe
    }
}
