using UnityEngine;

namespace BadNorth3D.Islands
{
    /// <summary>
    /// 地形特征定义 - Stage 4扩展
    /// </summary>
    [System.Serializable]
    public class TerrainFeature
    {
        public string name;
        public TerrainFeatureType FeatureType;
        public Vector3 position;
        public float radius;
        public string description;

        public TerrainFeature(string name, TerrainFeatureType type, Vector3 pos, float radius, string description = "")
        {
            this.name = name;
            this.FeatureType = type;
            this.position = pos;
            this.radius = radius;
            this.description = description;
        }
    }

    /// <summary>
    /// 地形特征类型枚举 - Stage 4扩展
    /// </summary>
    public enum TerrainFeatureType
    {
        // ===== 原有地形特征 (Stage 3) =====
        River,        // 河流
        Lake,         // 湖泊
        MountainRange,// 山脉
        Forest,       // 森林
        Ruins,        // 遗迹

        // ===== 新地形特征 (Stage 4) =====
        Swamp,        // 沼泽 - 减速+持续伤害
        Volcanic,     // 火山区域 - 高伤害+爆发
        AncientRuins, // 古代遗迹 - 属性加成+治疗
        Mine,         // 矿藏 - 产生金币
        Village,      // 村庄 - NPC和任务
        Battlefield,  // 战场 - 历史遗迹和剧情
        Sanctuary     // 圣所 - 特殊强化
    }

    /// <summary>
    /// 地形特征效果
    /// </summary>
    [System.Serializable]
    public struct TerrainEffect
    {
        public float movementModifier;      // 移动修正 (0.5f = 50%减速)
        public float damageModifier;        // 伤害修正 (1.2f = +20%伤害)
        public float healthRegeneration;    // 生命恢复 (每秒恢复量)
        public float goldGeneration;        // 金币产生 (每秒产生量)
        public bool providesCover;          // 提供掩护
        public bool strategicValue;        // 战略价值
    }

    /// <summary>
    /// 地形特征信息
    /// </summary>
    [System.Serializable]
    public struct TerrainFeatureInfo
    {
        public TerrainFeatureType Type;
        public string Name;
        public string Description;
        public TerrainEffect Effects;
        public float Radius;
        public Vector3 Position;
    }

    /// <summary>
    /// 地形特征管理器 - 创建和管理所有地形特征
    /// </summary>
    public static class TerrainFeatureFactory
    {
        /// <summary>
        /// 创建地形特征
        /// </summary>
        public static TerrainFeature CreateFeature(TerrainFeatureType type, Vector3 position, float radius = 5f)
        {
            string name = type.ToString();
            string description = GetFeatureDescription(type);

            return new TerrainFeature(name, type, position, radius, description);
        }

        /// <summary>
        /// 获取地形特征描述
        /// </summary>
        public static string GetFeatureDescription(TerrainFeatureType type)
        {
            return type switch
            {
                TerrainFeatureType.Swamp => "危险的沼泽地，减缓移动并造成持续伤害",
                TerrainFeatureType.Volcanic => "火山区域，极高伤害和随机爆发",
                TerrainFeatureType.AncientRuins => "神秘古代遗迹，提供属性加成和治疗",
                TerrainFeatureType.Mine => "丰富的矿产资源，持续产生金币",
                TerrainFeatureType.Village => "友好的村庄，提供任务和补给",
                TerrainFeatureType.Battlefield => "古战场，可能有强大敌人和宝贵战利品",
                TerrainFeatureType.Sanctuary => "神圣之地，大幅强化单位能力",

                // Stage 3地形
                TerrainFeatureType.River => "河流，提供水源但阻碍通行",
                TerrainFeatureType.Lake => "湖泊，美丽但危险的水域",
                TerrainFeatureType.MountainRange => "山脉，难以通行的天然屏障",
                TerrainFeatureType.Forest => "森林，提供掩护和资源",
                TerrainFeatureType.Ruins => "遗迹，可能隐藏宝藏和秘密",

                _ => "未知地形特征"
            };
        }

        /// <summary>
        /// 获取地形效果
        /// </summary>
        public static TerrainEffect GetTerrainEffect(TerrainFeatureType type)
        {
            return type switch
            {
                TerrainFeatureType.Swamp => new TerrainEffect
                {
                    movementModifier = 0.5f,    // 50%减速
                    damageModifier = 1.0f,
                    healthRegeneration = -5f,   // 持续伤害
                    goldGeneration = 0f
                },

                TerrainFeatureType.Volcanic => new TerrainEffect
                {
                    movementModifier = 0.8f,    // 轻微减速
                    damageModifier = 2.0f,      // 双倍伤害
                    healthRegeneration = -15f,  // 大额伤害
                    goldGeneration = 0f
                },

                TerrainFeatureType.AncientRuins => new TerrainEffect
                {
                    movementModifier = 1.0f,
                    damageModifier = 1.2f,      // +20%伤害
                    healthRegeneration = 5f,    // 持续治疗
                    goldGeneration = 0f
                },

                TerrainFeatureType.Mine => new TerrainEffect
                {
                    movementModifier = 1.0f,
                    damageModifier = 1.0f,
                    healthRegeneration = 0f,
                    goldGeneration = 10f        // 每秒10金币
                },

                TerrainFeatureType.Village => new TerrainEffect
                {
                    movementModifier = 1.0f,
                    damageModifier = 1.0f,
                    healthRegeneration = 3f,    // 轻微治疗
                    goldGeneration = 2f         // 少量金币
                },

                TerrainFeatureType.Battlefield => new TerrainEffect
                {
                    movementModifier = 0.9f,    // 轻微妨碍
                    damageModifier = 1.5f,      // +50%伤害
                    healthRegeneration = 0f,
                    goldGeneration = 0f,
                    strategicValue = true
                },

                TerrainFeatureType.Sanctuary => new TerrainEffect
                {
                    movementModifier = 1.2f,    // 移动加速
                    damageModifier = 1.5f,      // +50%伤害
                    healthRegeneration = 10f,   // 大额治疗
                    goldGeneration = 0f,
                    strategicValue = true
                },

                _ => new TerrainEffect()
            };
        }

        /// <summary>
        /// 获取推荐的战术
        /// </summary>
        public static string[] GetRecommendedTactics(TerrainFeatureType type)
        {
            return type switch
            {
                TerrainFeatureType.Swamp => new string[]
                {
                    "避免长时间停留",
                    "使用远程单位",
                    "快速通过区域"
                },

                TerrainFeatureType.Volcanic => new string[]
                {
                    "极度危险，建议绕行",
                    "高血量坦克单位",
                    "准备好撤退路线"
                },

                TerrainFeatureType.AncientRuins => new string[]
                {
                    "战斗中在遗迹附近恢复",
                    "利用加成效果",
                    "控制关键位置"
                },

                TerrainFeatureType.Mine => new string[]
                {
                    "派遣单位采集金币",
                    "保护采集单位",
                    "建立防御阵地"
                },

                _ => new string[] { "谨慎探索", "保持警惕" }
            };
        }

        /// <summary>
        /// 生成随机地形特征
        /// </summary>
        public static TerrainFeature GenerateRandomFeature(Vector3 centerPos, float mapRadius)
        {
            TerrainFeatureType[] allTypes = (TerrainFeatureType[])System.Enum.GetValues(typeof(TerrainFeatureType));
            TerrainFeatureType randomType = allTypes[Random.Range(0, allTypes.Length)];

            Vector3 randomPos = centerPos + new Vector3(
                Random.Range(-mapRadius * 0.7f, mapRadius * 0.7f),
                0f,
                Random.Range(-mapRadius * 0.7f, mapRadius * 0.7f)
            );

            float randomRadius = Random.Range(3f, 8f);

            return CreateFeature(randomType, randomPos, randomRadius);
        }

        /// <summary>
        /// 生成主题地形特征组合
        /// </summary>
        public static TerrainFeature[] GenerateThemedFeatures(string theme, Vector3 centerPos, float mapRadius)
        {
            System.Collections.Generic.List<TerrainFeature> features = new System.Collections.Generic.List<TerrainFeature>();

            switch (theme.ToLower())
            {
                case "dangerous":
                    features.Add(CreateFeature(TerrainFeatureType.Volcanic, centerPos + new Vector3(10f, 0f, 10f), 6f));
                    features.Add(CreateFeature(TerrainFeatureType.Swamp, centerPos + new Vector3(-10f, 0f, -8f), 7f));
                    features.Add(CreateFeature(TerrainFeatureType.Battlefield, centerPos + new Vector3(0f, 0f, 15f), 8f));
                    break;

                case "rewarding":
                    features.Add(CreateFeature(TerrainFeatureType.Mine, centerPos + new Vector3(8f, 0f, 0f), 5f));
                    features.Add(CreateFeature(TerrainFeatureType.AncientRuins, centerPos + new Vector3(-8f, 0f, 8f), 6f));
                    features.Add(CreateFeature(TerrainFeatureType.Village, centerPos + new Vector3(0f, 0f, -10f), 7f));
                    break;

                case "balanced":
                    features.Add(CreateFeature(TerrainFeatureType.Mine, centerPos + new Vector3(12f, 0f, 0f), 5f));
                    features.Add(CreateFeature(TerrainFeatureType.Forest, centerPos + new Vector3(-8f, 0f, 8f), 6f));
                    features.Add(CreateFeature(TerrainFeatureType.Swamp, centerPos + new Vector3(0f, 0f, -12f), 4f));
                    features.Add(CreateFeature(TerrainFeatureType.AncientRuins, centerPos + new Vector3(5f, 0f, 10f), 5f));
                    break;

                default:
                    // 生成随机组合
                    for (int i = 0; i < 3; i++)
                    {
                        features.Add(GenerateRandomFeature(centerPos, mapRadius));
                    }
                    break;
            }

            return features.ToArray();
        }
    }
}