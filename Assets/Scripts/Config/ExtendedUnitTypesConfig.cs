using UnityEngine;
using System.Collections.Generic;

namespace BadNorth3D
{
    /// <summary>
    /// 扩展单位类型配置 - Stage 4新单位
    /// AI可以轻松调整新单位的属性和技能
    /// </summary>
    public static class ExtendedUnitTypesConfig
    {
        /// <summary>
        /// Stage 4 扩展单位类型定义
        /// </summary>
        public enum ExtendedSquadUnitType
        {
            // 原有单位 (Stage 2)
            Warrior,    // 战士 - 平衡型近战
            Archer,     // 弓箭手 - 远程物理
            Knight,     // 骑士 - 高防御坦克
            Berserker,  // 狂战士 - 高伤害输出

            // 新增单位 (Stage 4)
            Cleric,     // 牧师 - 治疗和辅助
            Engineer,   // 工程师 - 建筑和防御
            Ranger      // 游侠 - 侦查和陷阱
        }

        /// <summary>
        /// 所有玩家单位配置（包括原有和新增）
        /// </summary>
        public static readonly SquadUnitTypeConfig[] ALL_UNITS = new SquadUnitTypeConfig[]
        {
            // ===== 原有单位配置 (保持Stage 2设置) =====
            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Warrior,
                Name = "战士",
                Description = "平衡的近战战士",
                Cost = 20f,
                MaxHealth = 100f,
                AttackDamage = 20f,
                AttackRange = 2f,
                MoveSpeed = 3.5f,
                AttackSpeed = 1.2f,
                Color = new Color(0.8f, 0.2f, 0.2f),
                WeaponType = Weapon.Sword,
                ActiveAbility = new ActiveAbility("盾击", "击退周围敌人", 8f, 3f, 3f),
                PassiveAbility = new PassiveAbility("战斗经验", "伤害随战斗时间增加", 1.1f)
            },

            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Archer,
                Name = "弓箭手",
                Description = "远程物理伤害",
                Cost = 25f,
                MaxHealth = 70f,
                AttackDamage = 15f,
                AttackRange = 8f,
                MoveSpeed = 3.2f,
                AttackSpeed = 0.8f,
                Color = new Color(0.2f, 0.8f, 0.2f),
                WeaponType = Weapon.Bow,
                ActiveAbility = new ActiveAbility("连射", "同时攻击多个目标", 10f, 12f, 8f),
                PassiveAbility = new PassiveAbility("精确打击", "对低血量敌人额外伤害", 1.15f)
            },

            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Knight,
                Name = "骑士",
                Description = "高防御坦克",
                Cost = 30f,
                MaxHealth = 150f,
                AttackDamage = 25f,
                AttackRange = 2f,
                MoveSpeed = 2.8f,
                AttackSpeed = 1.5f,
                Color = new Color(0.2f, 0.4f, 0.8f),
                WeaponType = Weapon.SwordShield,
                ActiveAbility = new ActiveAbility("守护光环", "为周围友军提供防御加成", 15f, 5f, 5f),
                PassiveAbility = new PassiveAbility("钢铁意志", "受到伤害减少", 0.8f)
            },

            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Berserker,
                Name = "狂战士",
                Description = "高伤害输出",
                Cost = 35f,
                MaxHealth = 60f,
                AttackDamage = 35f,
                AttackRange = 2f,
                MoveSpeed = 4.0f,
                AttackSpeed = 1.0f,
                Color = new Color(0.8f, 0.4f, 0.1f),
                WeaponType = Weapon.Axe,
                ActiveAbility = new ActiveAbility("狂暴模式", "大幅提升攻击力和攻速", 12f, 8f, 0f),
                PassiveAbility = new PassiveAbility("嗜血", "击杀敌人恢复生命", 1.2f)
            },

            // ===== 新增单位配置 (Stage 4) =====

            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Cleric,
                Name = "牧师",
                Description = "治疗和辅助专家",
                Cost = 30f,
                MaxHealth = 60f,
                AttackDamage = 8f,
                AttackRange = 3f,
                MoveSpeed = 3.0f,
                AttackSpeed = 1.5f,
                Color = new Color(1f, 1f, 0.8f), // 金白色
                WeaponType = Weapon.Staff,
                ActiveAbility = new ActiveAbility("群体治疗", "恢复周围友军生命值", 18f, 5f, 4f),
                PassiveAbility = new PassiveAbility("祝福光环", "缓慢恢复附近友军生命", 0.5f),
                SupportRole = true, // 辅助角色
                HealingPower = 25f,  // 治疗强度
                BuffDuration = 8f    // 增益持续时间
            },

            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Engineer,
                Name = "工程师",
                Description = "建筑和防御专家",
                Cost = 40f,
                MaxHealth = 80f,
                AttackDamage = 12f,
                AttackRange = 5f,
                MoveSpeed = 2.5f,
                AttackSpeed = 1.8f,
                Color = new Color(0.6f, 0.4f, 0.2f), // 棕色
                WeaponType = Weapon.Wrench,
                ActiveAbility = new ActiveAbility("建造炮塔", "部署自动攻击炮塔", 25f, 10f, 0f),
                PassiveAbility = new PassiveAbility("工兵技能", "建筑和维修速度加快", 1.3f),
                SupportRole = true,  // 辅助角色
                BuildSpeed = 1.5f,   // 建造速度加成
                MaxTurrets = 2       // 最大炮塔数量
            },

            new SquadUnitTypeConfig
            {
                Type = SquadUnitType.Ranger,
                Name = "游侠",
                Description = "侦查和陷阱专家",
                Cost = 28f,
                MaxHealth = 75f,
                AttackDamage = 18f,
                AttackRange = 6f,
                MoveSpeed = 4.2f, // 最快移动速度
                AttackSpeed = 0.9f,
                Color = new Color(0.3f, 0.6f, 0.3f), // 深绿色
                WeaponType = Weapon.Dagger,
                ActiveAbility = new ActiveAbility("陷阱区域", "布设减速和伤害陷阱", 16f, 6f, 5f),
                PassiveAbility = new PassiveAbility("丛林行者", "在复杂地形中移动更快", 1.25f),
                ScoutRole = true,     // 侦查角色
                VisionRange = 12f,    // 扩大视野范围
                TrapCount = 3         // 陷阱数量
            }
        };

        /// <summary>
        /// 获取单位配置
        /// </summary>
        public static SquadUnitTypeConfig GetUnitConfig(SquadUnitType unitType)
        {
            foreach (var config in ALL_UNITS)
            {
                if (config.Type == unitType)
                    return config;
            }
            return ALL_UNITS[0]; // 默认返回战士
        }

        /// <summary>
        /// 获取所有单位类型
        /// </summary>
        public static SquadUnitType[] GetAllUnitTypes()
        {
            List<SquadUnitType> types = new List<SquadUnitType>();
            foreach (var config in ALL_UNITS)
            {
                if (!types.Contains(config.Type))
                {
                    types.Add(config.Type);
                }
            }
            return types.ToArray();
        }

        /// <summary>
        /// 获取角色类型单位
        /// </summary>
        public static SquadUnitTypeConfig[] GetUnitsByRole(string role)
        {
            List<SquadUnitTypeConfig> roleUnits = new List<SquadUnitTypeConfig>();
            foreach (var config in ALL_UNITS)
            {
                if (role == "support" && config.SupportRole)
                    roleUnits.Add(config);
                else if (role == "scout" && config.ScoutRole)
                    roleUnits.Add(config);
                else if (role == "combat" && !config.SupportRole && !config.ScoutRole)
                    roleUnits.Add(config);
            }
            return roleUnits.ToArray();
        }
    }

    /// <summary>
    /// 扩展的单位配置结构
    /// </summary>
    [System.Serializable]
    public class SquadUnitTypeConfig
    {
        public SquadUnitType Type;
        public string Name;
        public string Description;
        public float Cost;
        public float MaxHealth;
        public float AttackDamage;
        public float AttackRange;
        public float MoveSpeed;
        public float AttackSpeed;
        public Color Color;
        public Weapon WeaponType;
        public ActiveAbility ActiveAbility;
        public PassiveAbility PassiveAbility;

        // 新增属性 (Stage 4)
        public bool SupportRole;     // 辅助角色
        public bool ScoutRole;       // 侦查角色
        public float HealingPower;   // 治疗强度
        public float BuffDuration;   // 增益持续时间
        public float BuildSpeed;     // 建造速度
        public int MaxTurrets;       // 最大炮塔数量
        public float VisionRange;    // 视野范围
        public int TrapCount;        // 陷阱数量
    }
}