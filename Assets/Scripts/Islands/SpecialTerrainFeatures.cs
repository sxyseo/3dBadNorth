using UnityEngine;
using System.Collections;

namespace BadNorth3D.Islands
{
    /// <summary>
    /// 特殊地形特征系统 - Stage 4新地形
    /// 包括沼泽、火山、古代遗迹、矿藏等战略地形
    /// </summary>
    public class SpecialTerrainFeatures : MonoBehaviour
    {
        public static SpecialTerrainFeatures Instance { get; private set; }

        [Header("地形特征设置")]
        public TerrainFeature[] activeFeatures;

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
            // 生成配置的地形特征
            GenerateTerrainFeatures();
        }

        /// <summary>
        /// 生成地形特征
        /// </summary>
        public void GenerateTerrainFeatures()
        {
            if (activeFeatures == null || activeFeatures.Length == 0)
                return;

            foreach (var feature in activeFeatures)
            {
                CreateTerrainFeature(feature);
            }
        }

        /// <summary>
        /// 创建单个地形特征
        /// </summary>
        void CreateTerrainFeature(TerrainFeature feature)
        {
            switch (feature.FeatureType)
            {
                case TerrainFeatureType.Swamp:
                    CreateSwampFeature(feature);
                    break;
                case TerrainFeatureType.Volcanic:
                    CreateVolcanicFeature(feature);
                    break;
                case TerrainFeatureType.AncientRuins:
                    CreateAncientRuinsFeature(feature);
                    break;
                case TerrainFeatureType.Mine:
                    CreateMineFeature(feature);
                    break;
            }
        }

        /// <summary>
        /// 创建沼泽特征
        /// </summary>
        void CreateSwampFeature(TerrainFeature feature)
        {
            GameObject swamp = new GameObject($"Swamp_{feature.name}");
            swamp.transform.position = feature.position;
            swamp.transform.SetParent(transform);

            // 沼泽地面
            GameObject swampGround = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            swampGround.transform.SetParent(swamp.transform);
            swampGround.transform.position = feature.position + Vector3.up * 0.05f;
            swampGround.transform.localScale = new Vector3(feature.radius * 2f, 0.1f, feature.radius * 2f);

            Material swampMaterial = new Material(Shader.Find("Standard"));
            swampMaterial.color = new Color(0.2f, 0.4f, 0.2f, 0.7f); // 深绿色半透明
            swampGround.GetComponent<Renderer>().material = swampMaterial;
            Destroy(swampGround.GetComponent<CapsuleCollider>());

            // 沼泽区域触发器
            SphereCollider swampCollider = swamp.AddComponent<SphereCollider>();
            swampCollider.radius = feature.radius;
            swampCollider.isTrigger = true;

            SwampEffect swampEffect = swamp.AddComponent<SwampEffect>();
            swampEffect.slowAmount = 0.5f; // 50%减速
            swampEffect.damagePerSecond = 5f; // 持续伤害

            // 沼泽装饰
            CreateSwampVegetation(swamp, feature);

            Debug.Log($"Created swamp feature at {feature.position}");
        }

        void CreateSwampVegetation(GameObject parent, TerrainFeature feature)
        {
            // 添加沼泽植被
            for (int i = 0; i < 8; i++)
            {
                Vector3 vegPos = feature.position + new Vector3(
                    Random.Range(-feature.radius, feature.radius),
                    0f,
                    Random.Range(-feature.radius, feature.radius)
                );

                // 小树桩
                GameObject stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stump.transform.SetParent(parent.transform);
                stump.transform.position = vegPos;
                stump.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);

                Material stumpMaterial = new Material(Shader.Find("Standard"));
                stumpMaterial.color = new Color(0.3f, 0.2f, 0.1f);
                stump.GetComponent<Renderer>().material = stumpMaterial;
                Destroy(stump.GetComponent<CapsuleCollider>());
            }
        }

        /// <summary>
        /// 创建火山特征
        /// </summary>
        void CreateVolcanicFeature(TerrainFeature feature)
        {
            GameObject volcanic = new GameObject($"Volcanic_{feature.name}");
            volcanic.transform.position = feature.position;
            volcanic.transform.SetParent(transform);

            // 火山地面
            GameObject volcanicGround = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            volcanicGround.transform.SetParent(volcanic.transform);
            volcanicGround.transform.position = feature.position + Vector3.up * 0.05f;
            volcanicGround.transform.localScale = new Vector3(feature.radius * 2f, 0.1f, feature.radius * 2f);

            Material volcanicMaterial = new Material(Shader.Find("Standard"));
            volcanicMaterial.color = new Color(0.3f, 0.1f, 0f, 0.6f); // 深红色半透明
            volcanicGround.GetComponent<Renderer>().material = volcanicMaterial;
            Destroy(volcanicGround.GetComponent<CapsuleCollider>());

            // 火山区域触发器
            SphereCollider volcanicCollider = volcanic.AddComponent<SphereCollider>();
            volcanicCollider.radius = feature.radius;
            volcanicCollider.isTrigger = true;

            VolcanicEffect volcanicEffect = volcanic.AddComponent<VolcanicEffect>();
            volcanicEffect.damagePerSecond = 15f; // 高额伤害
            volcanicEffect.chanceToErupt = 0.1f; // 10%爆发几率

            // 火山锥
            GameObject volcanoCone = GameObject.CreatePrimitive(PrimitiveType.Cone);
            volcanoCone.transform.SetParent(volcanic.transform);
            volcanoCone.transform.position = feature.position + Vector3.up * 2f;
            volcanoCone.transform.localScale = new Vector3(2f, 4f, 2f);

            Material coneMaterial = new Material(Shader.Find("Standard"));
            coneMaterial.color = new Color(0.4f, 0.1f, 0f);
            volcanoCone.GetComponent<Renderer>().material = coneMaterial;
            Destroy(volcanoCone.GetComponent<CapsuleCollider>());

            // 岩浆装饰
            CreateLavaPools(volcanic, feature);

            Debug.Log($"Created volcanic feature at {feature.position}");
        }

        void CreateLavaPools(GameObject parent, TerrainFeature feature)
        {
            // 创建岩浆池
            for (int i = 0; i < 3; i++)
            {
                Vector3 poolPos = feature.position + new Vector3(
                    Random.Range(-feature.radius * 0.5f, feature.radius * 0.5f),
                    0.1f,
                    Random.Range(-feature.radius * 0.5f, feature.radius * 0.5f)
                );

                GameObject lavaPool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lavaPool.transform.SetParent(parent.transform);
                lavaPool.transform.position = poolPos;
                lavaPool.transform.localScale = new Vector3(
                    Random.Range(1f, 2f),
                    0.05f,
                    Random.Range(1f, 2f)
                );

                Material lavaMaterial = new Material(Shader.Find("Standard"));
                lavaMaterial.color = new Color(1f, 0.3f, 0f, 0.8f); // 橙红色岩浆
                lavaMaterial.SetFloat("_Emission", 0.5f); // 发光效果
                lavaPool.GetComponent<Renderer>().material = lavaMaterial;
                Destroy(lavaPool.GetComponent<CapsuleCollider>());

                // 岩浆粒子效果
                StartCoroutine(LavaGlowEffect(lavaPool.GetComponent<Renderer>()));
            }
        }

        IEnumerator LavaGlowEffect(Renderer lavaRenderer)
        {
            if (lavaRenderer == null) yield break;

            Material lavaMaterial = lavaRenderer.material;
            float time = 0f;

            while (lavaRenderer != null)
            {
                time += Time.deltaTime * 2f;
                float glow = 0.5f + Mathf.Sin(time) * 0.3f;
                lavaMaterial.SetFloat("_Emission", glow);

                yield return null;
            }
        }

        /// <summary>
        /// 创建古代遗迹特征
        /// </summary>
        void CreateAncientRuinsFeature(TerrainFeature feature)
        {
            GameObject ruins = new GameObject($"AncientRuins_{feature.name}");
            ruins.transform.position = feature.position;
            ruins.transform.SetParent(transform);

            // 遗迹基座
            GameObject ruinsBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ruinsBase.transform.SetParent(ruins.transform);
            ruinsBase.transform.position = feature.position + Vector3.up * 0.1f;
            ruinsBase.transform.localScale = new Vector3(feature.radius * 1.5f, 0.2f, feature.radius * 1.5f);

            Material baseMaterial = new Material(Shader.Find("Standard"));
            baseMaterial.color = new Color(0.5f, 0.5f, 0.45f); // 石灰色
            ruinsBase.GetComponent<Renderer>().material = baseMaterial;
            Destroy(ruinsBase.GetComponent<BoxCollider>());

            // 神秘区域触发器
            SphereCollider ruinsCollider = ruins.AddComponent<SphereCollider>();
            ruinsCollider.radius = feature.radius;
            ruinsCollider.isTrigger = true;

            AncientRuinsEffect ruinsEffect = ruins.AddComponent<AncientRuinsEffect>();
            ruinsEffect.buffAmount = 1.2f; // 20%属性加成
            ruinsEffect.healPerSecond = 5f; // 持续治疗

            // 古代柱子
            CreateAncientPillars(ruins, feature);

            Debug.Log($"Created ancient ruins feature at {feature.position}");
        }

        void CreateAncientPillars(GameObject parent, TerrainFeature feature)
        {
            // 创建古代柱子阵
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                float distance = feature.radius * 0.8f;
                Vector3 pillarPos = feature.position + new Vector3(
                    Mathf.Cos(angle) * distance,
                    2f,
                    Mathf.Sin(angle) * distance
                );

                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.SetParent(parent.transform);
                pillar.transform.position = pillarPos;
                pillar.transform.localScale = new Vector3(0.6f, 4f, 0.6f);

                Material pillarMaterial = new Material(Shader.Find("Standard"));
                pillarMaterial.color = new Color(0.6f, 0.6f, 0.5f); // 古老石色
                pillar.GetComponent<Renderer>().material = pillarMaterial;
                Destroy(pillar.GetComponent<CapsuleCollider>());

                // 柱子顶部装饰
                GameObject pillarTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillarTop.transform.SetParent(pillar.transform);
                pillarTop.transform.localPosition = new Vector3(0, 2.2f, 0);
                pillarTop.transform.localScale = new Vector3(1.2f, 0.4f, 1.2f);

                pillarTop.GetComponent<Renderer>().material = pillarMaterial;
                Destroy(pillarTop.GetComponent<CapsuleCollider>());

                // 神秘粒子效果
                CreateMysticParticles(pillarTop.transform);
            }
        }

        void CreateMysticParticles(Transform parent)
        {
            GameObject particles = new GameObject("MysticParticles");
            particles.transform.SetParent(parent);
            particles.transform.localPosition = Vector3.zero;

            // 这里可以添加实际的粒子系统
            // 简化处理：创建小光球
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.transform.SetParent(particles.transform);
            orb.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            Material orbMaterial = new Material(Shader.Find("Standard"));
            orbMaterial.color = new Color(0.5f, 0.8f, 1f, 0.5f); // 蓝色半透明
            orb.GetComponent<Renderer>().material = orbMaterial;
            Destroy(orb.GetComponent<SphereCollider>());

            // 浮动动画
            StartCoroutine(OrbFloatAnimation(orb.transform));
        }

        IEnumerator OrbFloatAnimation(Transform orbTransform)
        {
            float time = 0f;
            while (orbTransform != null)
            {
                time += Time.deltaTime;
                float y = Mathf.Sin(time * 2f) * 0.3f;
                orbTransform.localPosition = new Vector3(0, y, 0);
                yield return null;
            }
        }

        /// <summary>
        /// 创建矿藏特征
        /// </summary>
        void CreateMineFeature(TerrainFeature feature)
        {
            GameObject mine = new GameObject($"Mine_{feature.name}");
            mine.transform.position = feature.position;
            mine.transform.SetParent(transform);

            // 矿藏区域标记
            GameObject mineMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mineMarker.transform.SetParent(mine.transform);
            mineMarker.transform.position = feature.position + Vector3.up * 0.1f;
            mineMarker.transform.localScale = new Vector3(feature.radius * 2f, 0.1f, feature.radius * 2f);

            Material mineMaterial = new Material(Shader.Find("Standard"));
            mineMaterial.color = new Color(0.6f, 0.5f, 0.2f, 0.5f); // 金棕色
            mineMarker.GetComponent<Renderer>().material = mineMaterial;
            Destroy(mineMarker.GetComponent<CapsuleCollider>());

            // 矿藏触发器
            SphereCollider mineCollider = mine.AddComponent<SphereCollider>();
            mineCollider.radius = feature.radius;
            mineCollider.isTrigger = true;

            MineResourceEffect mineEffect = mine.AddComponent<MineResourceEffect>();
            mineEffect.goldPerSecond = 10f; // 每秒产生金币
            mineEffect.maxMiners = 3; // 最多3个单位可采集

            // 矿藏入口
            CreateMineEntrance(mine, feature);

            Debug.Log($"Created mine feature at {feature.position}");
        }

        void CreateMineEntrance(GameObject parent, TerrainFeature feature)
        {
            GameObject entrance = new GameObject("MineEntrance");
            entrance.transform.SetParent(parent.transform);
            entrance.transform.position = feature.position;

            // 入口框架
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.transform.SetParent(entrance.transform);
            frame.transform.localPosition = new Vector3(0, 1f, 0);
            frame.transform.localScale = new Vector3(3f, 2.5f, 0.5f);

            Material frameMaterial = new Material(Shader.Find("Standard"));
            frameMaterial.color = new Color(0.4f, 0.35f, 0.2f); // 深棕色
            frame.GetComponent<Renderer>().material = frameMaterial;
            Destroy(frame.GetComponent<BoxCollider>());

            // 支撑柱
            for (int i = 0; i < 2; i++)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.SetParent(entrance.transform);
                pillar.transform.localPosition = new Vector3((i - 0.5f) * 2f, 0.5f, 0);
                pillar.transform.localScale = new Vector3(0.3f, 1f, 0.3f);

                pillar.GetComponent<Renderer>().material = frameMaterial;
                Destroy(pillar.GetComponent<CapsuleCollider>());
            }

            // 金矿粒子
            CreateGoldParticles(entrance.transform);
        }

        void CreateGoldParticles(Transform parent)
        {
            GameObject goldParticles = new GameObject("GoldParticles");
            goldParticles.transform.SetParent(parent);

            // 创建几个小金块
            for (int i = 0; i < 5; i++)
            {
                GameObject goldChunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                goldChunk.transform.SetParent(goldParticles.transform);
                goldChunk.transform.localPosition = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.1f, 0.5f),
                    Random.Range(-0.5f, 0.5f)
                );
                goldChunk.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                Material goldMaterial = new Material(Shader.Find("Standard"));
                goldMaterial.color = new Color(1f, 0.8f, 0f); // 金色
                goldMaterial.SetFloat("_Emission", 0.3f);
                goldChunk.GetComponent<Renderer>().material = goldMaterial;
                Destroy(goldChunk.GetComponent<BoxCollider>());
            }
        }

        /// <summary>
        /// 添加地形特征
        /// </summary>
        public void AddTerrainFeature(TerrainFeature feature)
        {
            if (activeFeatures == null)
            {
                activeFeatures = new TerrainFeature[0];
            }

            System.Collections.Generic.List<TerrainFeature> featureList = new System.Collections.Generic.List<TerrainFeature>(activeFeatures);
            featureList.Add(feature);
            activeFeatures = featureList.ToArray();

            CreateTerrainFeature(feature);
        }

        /// <summary>
        /// 移除地形特征
        /// </summary>
        public void RemoveTerrainFeature(string featureName)
        {
            // 这里可以实现特征移除逻辑
            Debug.Log($"Remove terrain feature: {featureName}");
        }

        /// <summary>
        /// 获取所有激活的地形特征
        /// </summary>
        public TerrainFeature[] GetActiveFeatures()
        {
            return activeFeatures;
        }
    }

    // ==================== 地形特征组件 ====================

    /// <summary>
    /// 沼泽效果 - 减速和持续伤害
    /// </summary>
    public class SwampEffect : MonoBehaviour
    {
        public float slowAmount = 0.5f;        // 减速50%
        public float damagePerSecond = 5f;     // 每秒5点伤害

        void OnTriggerStay(Collider other)
        {
            SquadUnit unit = other.GetComponent<SquadUnit>();
            if (unit != null && unit.IsAlive())
            {
                // 应用减速效果
                // 这里需要修改单位的移动速度
                // 由于访问限制，简化处理

                // 持续伤害
                unit.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// 火山效果 - 高额伤害和爆发
    /// </summary>
    public class VolcanicEffect : MonoBehaviour
    {
        public float damagePerSecond = 15f;     // 每秒15点伤害
        public float chanceToErupt = 0.1f;     // 10%爆发几率

        void OnTriggerStay(Collider other)
        {
            SquadUnit unit = other.GetComponent<SquadUnit>();
            if (unit != null && unit.IsAlive())
            {
                // 持续伤害
                unit.TakeDamage(damagePerSecond * Time.deltaTime);

                // 随机爆发
                if (Random.value < chanceToErupt * Time.deltaTime)
                {
                    VolcanicEruption();
                }
            }
        }

        void VolcanicEruption()
        {
            // 火山爆发！范围高额伤害
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.PlaySkillEffect(transform.position, SkillType.Berserk);
            }

            Collider[] hitTargets = Physics.OverlapSphere(transform.position, 5f);
            foreach (var hit in hitTargets)
            {
                SquadUnit unit = hit.GetComponent<SquadUnit>();
                if (unit != null && unit.IsAlive())
                {
                    unit.TakeDamage(30f); // 爆发伤害
                }
            }
        }
    }

    /// <summary>
    /// 古代遗迹效果 - 属性加成和治疗
    /// </summary>
    public class AncientRuinsEffect : MonoBehaviour
    {
        public float buffAmount = 1.2f;        // 20%属性加成
        public float healPerSecond = 5f;       // 每秒恢复5点生命

        void OnTriggerStay(Collider other)
        {
            SquadUnit unit = other.GetComponent<SquadUnit>();
            if (unit != null && unit.IsAlive())
            {
                // 持续治疗
                if (unit.GetComponent<ExtendedSquadUnit>() != null)
                {
                    var extendedUnit = unit.GetComponent<ExtendedSquadUnit>();
                    extendedUnit.Heal(healPerSecond * Time.deltaTime);
                }
            }
        }
    }

    /// <summary>
    /// 矿藏效果 - 产生金币
    /// </summary>
    public class MineResourceEffect : MonoBehaviour
    {
        public float goldPerSecond = 10f;      // 每秒产生10金币
        public int maxMiners = 3;              // 最多3个单位采集
        private int currentMiners = 0;

        void OnTriggerEnter(Collider other)
        {
            SquadUnit unit = other.GetComponent<SquadUnit>();
            if (unit != null && currentMiners < maxMiners)
            {
                currentMiners++;
                StartCoroutine(MineGold());
            }
        }

        void OnTriggerExit(Collider other)
        {
            SquadUnit unit = other.GetComponent<SquadUnit>();
            if (unit != null && currentMiners > 0)
            {
                currentMiners--;
            }
        }

        IEnumerator MineGold()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddGold(goldPerSecond);
                }

                // 播放获得金币音效
                if (AudioSynthesizer.Instance != null)
                {
                    AudioSynthesizer.Instance.PlaySelectSound();
                }
            }
        }
    }
}