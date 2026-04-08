using UnityEngine;

namespace BadNorth3D.Islands
{
    /// <summary>
    /// 地图变体系统 - 为岛屿添加季节变化和地形特征
    /// AI可以调整季节效果和地形参数
    /// </summary>
    public class MapVariantSystem : MonoBehaviour
    {
        public static MapVariantSystem Instance { get; private set; }

        [Header("季节设置")]
        public Season currentSeason = Season.Spring;
        public bool enableSeasonalChanges = true;

        [Header("地形特征")]
        public TerrainFeatureType[] terrainFeatures = new TerrainFeatureType[0];

        [Header("季节效果设置")]
        public Color springColor = new Color(0.3f, 0.6f, 0.3f);    // 春季绿色
        public Color summerColor = new Color(0.5f, 0.7f, 0.2f);    // 夏季深绿
        public Color autumnColor = new Color(0.7f, 0.5f, 0.2f);    // 秋季橙黄
        public Color winterColor = new Color(0.9f, 0.9f, 0.95f);   // 冬季雪白

        private Terrain terrain;
        private ProceduralIslandGenerator islandGenerator;

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
            terrain = GetComponent<Terrain>();
            islandGenerator = GetComponent<ProceduralIslandGenerator>();

            ApplySeasonEffects(currentSeason);
            GenerateTerrainFeatures();
        }

        /// <summary>
        /// 应用季节效果
        /// </summary>
        public void ApplySeasonEffects(Season season)
        {
            currentSeason = season;

            if (terrain == null)
                return;

            switch (season)
            {
                case Season.Spring:
                    ApplySpringEffects();
                    break;
                case Season.Summer:
                    ApplySummerEffects();
                    break;
                case Season.Autumn:
                    ApplyAutumnEffects();
                    break;
                case Season.Winter:
                    ApplyWinterEffects();
                    break;
            }

            Debug.Log($"Applied {season} effects to terrain");
        }

        /// <summary>
        /// 应用春季效果
        /// </summary>
        void ApplySpringEffects()
        {
            // 春季：生机勃勃的绿色
            UpdateTerrainColors(springColor);
            AddSeasonalVegetation(1.2f); // 增加植被
            UpdateLighting(new Color(0.8f, 0.9f, 1f), 1f); // 明亮光照
        }

        /// <summary>
        /// 应用夏季效果
        /// </summary>
        void ApplySummerEffects()
        {
            // 夏季：深绿色，强烈光照
            UpdateTerrainColors(summerColor);
            AddSeasonalVegetation(0.8f); // 正常植被
            UpdateLighting(new Color(1f, 0.95f, 0.8f), 1.2f); // 强烈光照
        }

        /// <summary>
        /// 应用秋季效果
        /// </summary>
        void ApplyAutumnEffects()
        {
            // 秋季：橙黄色调
            UpdateTerrainColors(autumnColor);
            AddSeasonalVegetation(0.6f); // 减少植被
            UpdateLighting(new Color(1f, 0.8f, 0.6f), 0.9f); // 温暖光照
        }

        /// <summary>
        /// 应用冬季效果
        /// </summary>
        void ApplyWinterEffects()
        {
            // 冬季：雪白色调
            UpdateTerrainColors(winterColor);
            AddSeasonalVegetation(0.2f); // 极少植被
            UpdateLighting(new Color(0.7f, 0.8f, 1f), 0.7f); // 冷色光照
            AddSnowCoverage();
        }

        /// <summary>
        /// 更新地形颜色
        /// </summary>
        void UpdateTerrainColors(Color baseColor)
        {
            if (terrain == null || terrain.terrainData == null)
                return;

            // 更新地形材质颜色
            Material terrainMaterial = terrain.materialTemplate;
            if (terrainMaterial != null)
            {
                terrainMaterial.color = baseColor;
            }
        }

        /// <summary>
        /// 添加季节性植被
        /// </summary>
        void AddSeasonalVegetation(float densityMultiplier)
        {
            if (islandGenerator == null)
                return;

            // 根据密度调整树木和岩石数量
            var treesField = islandGenerator.GetType().GetField("numTrees");
            var rocksField = islandGenerator.GetType().GetField("numRocks");

            if (treesField != null)
            {
                int originalTrees = (int)treesField.GetValue(islandGenerator);
                // 这里可以动态调整树木数量，但需要在生成时设置
            }
        }

        /// <summary>
        /// 更新光照
        /// </summary>
        void UpdateLighting(Color lightColor, float intensity)
        {
            RenderSettings.ambientLight = lightColor;
            RenderSettings.sunIntensity = intensity;

            // 更新所有光源
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    light.color = lightColor;
                    light.intensity = intensity;
                }
            }
        }

        /// <summary>
        /// 添加雪覆盖效果
        /// </summary>
        void AddSnowCoverage()
        {
            // 为所有地形对象添加雪覆盖
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    Color originalColor = renderer.material.color;
                    Color snowColor = Color.Lerp(originalColor, Color.white, 0.7f);
                    renderer.material.color = snowColor;
                }
            }
        }

        /// <summary>
        /// 生成地形特征
        /// </summary>
        void GenerateTerrainFeatures()
        {
            foreach (TerrainFeatureType feature in terrainFeatures)
            {
                switch (feature)
                {
                    case TerrainFeatureType.River:
                        GenerateRiver();
                        break;
                    case TerrainFeatureType.Lake:
                        GenerateLake();
                        break;
                    case TerrainFeatureType.MountainRange:
                        GenerateMountainRange();
                        break;
                    case TerrainFeatureType.Forest:
                        GenerateDenseForest();
                        break;
                    case TerrainFeatureType.Ruins:
                        GenerateAncientRuins();
                        break;
                }
            }
        }

        /// <summary>
        /// 生成河流
        /// </summary>
        void GenerateRiver()
        {
            GameObject river = new GameObject("River");
            river.transform.SetParent(transform);

            // 创建河流路径
            Vector3[] riverPath = new Vector3[10];
            for (int i = 0; i < riverPath.Length; i++)
            {
                riverPath[i] = new Vector3(
                    -20f + i * 5f,
                    0.1f,
                    Mathf.Sin(i * 0.5f) * 10f
                );
            }

            // 沿路径创建水面
            for (int i = 0; i < riverPath.Length - 1; i++)
            {
                GameObject waterSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                waterSegment.transform.SetParent(river.transform);
                waterSegment.transform.position = (riverPath[i] + riverPath[i + 1]) / 2f;
                waterSegment.transform.localScale = new Vector3(6f, 0.1f, 3f);
                waterSegment.transform.LookAt(riverPath[i + 1]);

                Material waterMaterial = new Material(Shader.Find("Standard"));
                waterMaterial.color = new Color(0.3f, 0.5f, 0.8f, 0.7f);
                waterSegment.GetComponent<Renderer>().material = waterMaterial;
                Destroy(waterSegment.GetComponent<BoxCollider>());
            }

            Debug.Log("Generated river feature");
        }

        /// <summary>
        /// 生成湖泊
        /// </summary>
        void GenerateLake()
        {
            GameObject lake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lake.name = "Lake";
            lake.transform.SetParent(transform);
            lake.transform.position = new Vector3(10f, 0.1f, 10f);
            lake.transform.localScale = new Vector3(8f, 0.1f, 6f);

            Material lakeMaterial = new Material(Shader.Find("Standard"));
            lakeMaterial.color = new Color(0.3f, 0.6f, 0.9f, 0.8f);
            lake.GetComponent<Renderer>().material = lakeMaterial;
            Destroy(lake.GetComponent<CapsuleCollider>());

            Debug.Log("Generated lake feature");
        }

        /// <summary>
        /// 生成山脉
        /// </summary>
        void GenerateMountainRange()
        {
            GameObject mountainRange = new GameObject("MountainRange");
            mountainRange.transform.SetParent(transform);

            for (int i = 0; i < 5; i++)
            {
                GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                mountain.transform.SetParent(mountainRange.transform);
                mountain.transform.position = new Vector3(
                    -15f + i * 8f,
                    2f,
                    -15f + Random.Range(-3f, 3f)
                );
                mountain.transform.localScale = new Vector3(
                    4f + Random.Range(-1f, 1f),
                    6f + Random.Range(-2f, 2f),
                    4f + Random.Range(-1f, 1f)
                );

                Material mountainMaterial = new Material(Shader.Find("Standard"));
                mountainMaterial.color = new Color(0.5f, 0.4f, 0.3f);
                mountain.GetComponent<Renderer>().material = mountainMaterial;
                Destroy(mountain.GetComponent<CapsuleCollider>());
            }

            Debug.Log("Generated mountain range feature");
        }

        /// <summary>
        /// 生成密集森林
        /// </summary>
        void GenerateDenseForest()
        {
            GameObject denseForest = new GameObject("DenseForest");
            denseForest.transform.SetParent(transform);

            for (int i = 0; i < 30; i++)
            {
                GameObject tree = new GameObject("DenseTree");
                tree.transform.SetParent(denseForest.transform);
                tree.transform.position = new Vector3(
                    Random.Range(-20f, 20f),
                    0f,
                    Random.Range(-20f, 20f)
                );

                // 树干
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.transform.SetParent(tree.transform);
                trunk.transform.localPosition = new Vector3(0, 1f, 0);
                trunk.transform.localScale = new Vector3(0.4f, 2.5f, 0.4f);

                Material trunkMaterial = new Material(Shader.Find("Standard"));
                trunkMaterial.color = new Color(0.4f, 0.3f, 0.2f);
                trunk.GetComponent<Renderer>().material = trunkMaterial;
                Destroy(trunk.GetComponent<CapsuleCollider>());

                // 树冠
                GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foliage.transform.SetParent(tree.transform);
                foliage.transform.localPosition = new Vector3(0, 3f, 0);
                foliage.transform.localScale = new Vector3(2f, 2f, 2f);

                Material foliageMaterial = new Material(Shader.Find("Standard"));
                foliageMaterial.color = new Color(0.1f, 0.6f, 0.1f);
                foliage.GetComponent<Renderer>().material = foliageMaterial;
                Destroy(foliage.GetComponent<SphereCollider>());
            }

            Debug.Log("Generated dense forest feature");
        }

        /// <summary>
        /// 生成古代遗迹
        /// </summary>
        void GenerateAncientRuins()
        {
            GameObject ruins = new GameObject("AncientRuins");
            ruins.transform.SetParent(transform);
            ruins.transform.position = new Vector3(-10f, 0f, -10f);

            // 创建古代柱子
            for (int i = 0; i < 4; i++)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.SetParent(ruins.transform);
                pillar.transform.localPosition = new Vector3(
                    (i % 2) * 6f - 3f,
                    2f,
                    (i / 2) * 6f - 3f
                );
                pillar.transform.localScale = new Vector3(0.8f, 4f, 0.8f);

                Material pillarMaterial = new Material(Shader.Find("Standard"));
                pillarMaterial.color = new Color(0.6f, 0.6f, 0.5f); // 石头颜色
                pillar.GetComponent<Renderer>().material = pillarMaterial;
                Destroy(pillar.GetComponent<CapsuleCollider>());
            }

            // 创建破碎的地基
            GameObject foundation = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foundation.transform.SetParent(ruins.transform);
            foundation.transform.localPosition = new Vector3(0, 0.1f, 0);
            foundation.transform.localScale = new Vector3(10f, 0.2f, 10f);

            Material foundationMaterial = new Material(Shader.Find("Standard"));
            foundationMaterial.color = new Color(0.5f, 0.5f, 0.45f);
            foundation.GetComponent<Renderer>().material = foundationMaterial;
            Destroy(foundation.GetComponent<BoxCollider>());

            Debug.Log("Generated ancient ruins feature");
        }

        /// <summary>
        /// 随机化季节
        /// </summary>
        public void RandomizeSeason()
        {
            Season[] seasons = (Season[])System.Enum.GetValues(typeof(Season));
            Season randomSeason = seasons[Random.Range(0, seasons.Length)];
            ApplySeasonEffects(randomSeason);
        }

        /// <summary>
        /// 设置地形特征
        /// </summary>
        public void SetTerrainFeatures(TerrainFeatureType[] features)
        {
            terrainFeatures = features;
            GenerateTerrainFeatures();
        }

        /// <summary>
        /// 获取当前季节信息
        /// </summary>
        public SeasonInfo GetSeasonInfo()
        {
            return new SeasonInfo
            {
                CurrentSeason = currentSeason,
                SeasonColor = GetSeasonColor(currentSeason),
                VegetationDensity = GetVegetationDensity(),
                LightingIntensity = RenderSettings.sunIntensity
            };
        }

        Color GetSeasonColor(Season season)
        {
            return season switch
            {
                Season.Spring => springColor,
                Season.Summer => summerColor,
                Season.Autumn => autumnColor,
                Season.Winter => winterColor,
                _ => Color.white
            };
        }

        float GetVegetationDensity()
        {
            return currentSeason switch
            {
                Season.Spring => 1.2f,
                Season.Summer => 0.8f,
                Season.Autumn => 0.6f,
                Season.Winter => 0.2f,
                _ => 1f
            };
        }

        void OnDrawGizmosSelected()
        {
            // 显示地形特征位置
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 25f);
        }
    }

    // ==================== 地图变体数据结构 ====================

    /// <summary>
    /// 季节类型
    /// </summary>
    public enum Season
    {
        Spring,  // 春季
        Summer,  // 夏季
        Autumn,  // 秋季
        Winter   // 冬季
    }

    /// <summary>
    /// 地形特征类型
    /// </summary>
    public enum TerrainFeatureType
    {
        River,        // 河流
        Lake,         // 湖泊
        MountainRange,// 山脉
        Forest,       // 森林
        Ruins         // 遗迹
    }

    /// <summary>
    /// 季节信息
    /// </summary>
    [System.Serializable]
    public struct SeasonInfo
    {
        public Season CurrentSeason;
        public Color SeasonColor;
        public float VegetationDensity;
        public float LightingIntensity;
    }
}