using UnityEngine;
using System.Collections.Generic;

namespace BadNorth3D.Islands
{
    /// <summary>
    /// 地图选择器 - 提供多种地图组合供玩家选择
    /// AI可以创建新的地图预设和组合
    /// </summary>
    public class MapSelector : MonoBehaviour
    {
        public static MapSelector Instance { get; private set; }

        [Header("地图预设")]
        public MapPreset[] availablePresets;

        [Header("当前选择")]
        public int currentPresetIndex = 0;

        private MapPreset currentPreset;

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
            InitializeDefaultPresets();
            SelectPreset(currentPresetIndex);
        }

        /// <summary>
        /// 初始化默认地图预设
        /// </summary>
        void InitializeDefaultPresets()
        {
            List<MapPreset> presets = new List<MapPreset>();

            // 预设1: 春季单岛
            presets.Add(new MapPreset
            {
                Name = "Spring Island",
                Description = "A peaceful single island in spring",
                IslandShape = IslandShape.Single,
                Season = Season.Spring,
                Features = new TerrainFeatureType[0],
                Difficulty = MapDifficulty.Easy,
                RecommendedPlayers = 1
            });

            // 预设2: 夏季双岛
            presets.Add(new MapPreset
            {
                Name = "Summer Twin Islands",
                Description = "Two islands connected by land bridges",
                IslandShape = IslandShape.Double,
                Season = Season.Summer,
                Features = new TerrainFeatureType[0],
                Difficulty = MapDifficulty.Medium,
                RecommendedPlayers = 2
            });

            // 预设3: 秋季海湾
            presets.Add(new MapPreset
            {
                Name = "Autumn Bay",
                Description = "A sheltered bay with rich resources",
                IslandShape = IslandShape.Bay,
                Season = Season.Autumn,
                Features = new[] { TerrainFeatureType.River },
                Difficulty = MapDifficulty.Medium,
                RecommendedPlayers = 2
            });

            // 预设4: 冬季半岛
            presets.Add(new MapPreset
            {
                Name = "Winter Peninsula",
                Description = "A frozen peninsula with harsh conditions",
                IslandShape = IslandShape.Peninsula,
                Season = Season.Winter,
                Features = new[] { TerrainFeatureType.MountainRange },
                Difficulty = MapDifficulty.Hard,
                RecommendedPlayers = 3
            });

            // 预设5: 春季内陆湖
            presets.Add(new MapPreset
            {
                Name = "Spring Lake Land",
                Description = "An island with a central freshwater lake",
                IslandShape = IslandShape.InlandLake,
                Season = Season.Spring,
                Features = new[] { TerrainFeatureType.Lake, TerrainFeatureType.Forest },
                Difficulty = MapDifficulty.Easy,
                RecommendedPlayers = 1
            });

            // 预设6: 夏季遗迹
            presets.Add(new MapPreset
            {
                Name = "Ancient Summer Ruins",
                Description = "Mysterious ruins hidden in summer forest",
                IslandShape = IslandShape.Single,
                Season = Season.Summer,
                Features = new[] { TerrainFeatureType.Forest, TerrainFeatureType.Ruins },
                Difficulty = MapDifficulty.Hard,
                RecommendedPlayers = 4
            });

            // 预设7: 秋季山脉
            presets.Add(new MapPreset
            {
                Name = "Autumn Mountain Valley",
                Description = "A valley surrounded by mountain peaks",
                IslandShape = IslandShape.Bay,
                Season = Season.Autumn,
                Features = new[] { TerrainFeatureType.MountainRange, TerrainFeatureType.River },
                Difficulty = MapDifficulty.VeryHard,
                RecommendedPlayers = 4
            });

            // 预设8: 冬季荒原
            presets.Add(new MapPreset
            {
                Name = "Frozen Winter Wasteland",
                Description = "A harsh frozen landscape with limited resources",
                IslandShape = IslandShape.Single,
                Season = Season.Winter,
                Features = new TerrainFeatureType[0],
                Difficulty = MapDifficulty.VeryHard,
                RecommendedPlayers = 3
            });

            availablePresets = presets.ToArray();
        }

        /// <summary>
        /// 选择地图预设
        /// </summary>
        public void SelectPreset(int index)
        {
            if (index < 0 || index >= availablePresets.Length)
            {
                Debug.LogError($"Invalid preset index: {index}");
                return;
            }

            currentPresetIndex = index;
            currentPreset = availablePresets[index];

            ApplyPreset(currentPreset);

            Debug.Log($"Selected preset: {currentPreset.Name}");
        }

        /// <summary>
        /// 应用地图预设
        /// </summary>
        void ApplyPreset(MapPreset preset)
        {
            // 应用岛屿形状
            ProceduralIslandGenerator islandGenerator = GetComponent<ProceduralIslandGenerator>();
            if (islandGenerator != null)
            {
                var shapeField = islandGenerator.GetType().GetField("islandShape");
                if (shapeField != null)
                {
                    shapeField.SetValue(islandGenerator, preset.IslandShape);
                }
            }

            // 应用季节
            MapVariantSystem mapVariant = GetComponent<MapVariantSystem>();
            if (mapVariant != null)
            {
                mapVariant.ApplySeasonEffects(preset.Season);
                mapVariant.SetTerrainFeatures(preset.Features);
            }

            // 根据难度调整游戏参数
            ApplyDifficultySettings(preset.Difficulty);
        }

        /// <summary>
        /// 应用难度设置
        /// </summary>
        void ApplyDifficultySettings(MapDifficulty difficulty)
        {
            float enemyHealthMultiplier = difficulty switch
            {
                MapDifficulty.Easy => 0.8f,
                MapDifficulty.Medium => 1f,
                MapDifficulty.Hard => 1.3f,
                MapDifficulty.VeryHard => 1.6f,
                _ => 1f
            };

            float enemyDamageMultiplier = difficulty switch
            {
                MapDifficulty.Easy => 0.8f,
                MapDifficulty.Medium => 1f,
                MapDifficulty.Hard => 1.2f,
                MapDifficulty.VeryHard => 1.5f,
                _ => 1f
            };

            // 这里可以更新GameConfig中的敌人属性
            Debug.Log($"Applied difficulty: {difficulty} (HP: x{enemyHealthMultiplier}, Damage: x{enemyDamageMultiplier})");
        }

        /// <summary>
        /// 获取当前预设
        /// </summary>
        public MapPreset GetCurrentPreset()
        {
            return currentPreset;
        }

        /// <summary>
        /// 获取所有可用预设
        /// </summary>
        public MapPreset[] GetAllPresets()
        {
            return availablePresets;
        }

        /// <summary>
        /// 创建随机地图
        /// </summary>
        public void CreateRandomMap()
        {
            // 随机选择所有参数
            IslandShape[] shapes = (IslandShape[])System.Enum.GetValues(typeof(IslandShape));
            Season[] seasons = (Season[])System.Enum.GetValues(typeof(Season));
            TerrainFeatureType[] features = (TerrainFeatureType[])System.Enum.GetValues(typeof(TerrainFeatureType));
            MapDifficulty[] difficulties = (MapDifficulty[])System.Enum.GetValues(typeof(MapDifficulty));

            MapPreset randomPreset = new MapPreset
            {
                Name = "Random Map",
                Description = "Randomly generated map",
                IslandShape = shapes[Random.Range(0, shapes.Length)],
                Season = seasons[Random.Range(0, seasons.Length)],
                Features = new TerrainFeatureType[Random.Range(0, 3)], // 0-2个随机特征
                Difficulty = difficulties[Random.Range(0, difficulties.Length)],
                RecommendedPlayers = Random.Range(1, 5)
            };

            // 随机填充特征
            if (randomPreset.Features.Length > 0)
            {
                for (int i = 0; i < randomPreset.Features.Length; i++)
                {
                    randomPreset.Features[i] = features[Random.Range(0, features.Length)];
                }
            }

            currentPreset = randomPreset;
            ApplyPreset(randomPreset);

            Debug.Log($"Created random map: {randomPreset.IslandShape} in {randomPreset.Season}");
        }

        /// <summary>
        /// 获取地图难度描述
        /// </summary>
        public string GetDifficultyDescription(MapDifficulty difficulty)
        {
            return difficulty switch
            {
                MapDifficulty.Easy => "适合新手，资源丰富",
                MapDifficulty.Medium => "标准难度，平衡挑战",
                MapDifficulty.Hard => "困难模式，资源有限",
                MapDifficulty.VeryHard => "专家挑战，极度困难",
                _ => "未知难度"
            };
        }
    }

    // ==================== 地图预设数据结构 ====================

    /// <summary>
    /// 地图预设
    /// </summary>
    [System.Serializable]
    public class MapPreset
    {
        public string Name;
        public string Description;
        public IslandShape IslandShape;
        public Season Season;
        public TerrainFeatureType[] Features;
        public MapDifficulty Difficulty;
        public int RecommendedPlayers;
    }

    /// <summary>
    /// 地图难度
    /// </summary>
    public enum MapDifficulty
    {
        Easy,      // 简单
        Medium,    // 中等
        Hard,      // 困难
        VeryHard   // 极难
    }
}