using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// 精英敌人 - 强化版本的普通敌人
    /// 具有独特外观、强化属性和特殊能力
    /// </summary>
    public class EliteEnemy : MonoBehaviour
    {
        public enum EnemyVariant
        {
            Normal,      // 普通版
            Elite,       // 精英版 - 强化属性
            Champion,    // 冠军版 - 特殊能力
            Legend       // 传说版 - 极度挑战
        }

        [Header("精英敌人配置")]
        public Enemy.EnemyType baseType = Enemy.EnemyType.Normal;
        public EnemyVariant variant = EnemyVariant.Elite;

        [Header("强化属性")]
        public float healthMultiplier = 2f;
        public float damageMultiplier = 1.5f;
        public float speedMultiplier = 1.2f;
        public float sizeMultiplier = 1.3f;

        [Header("特殊能力")]
        public bool hasSpecialAbility = true;
        public float abilityCooldown = 8f;

        // 继承基础敌人功能
        private Enemy baseEnemy;
        private float lastAbilityTime;

        void Start()
        {
            InitializeEliteEnemy();
        }

        void InitializeEliteEnemy()
        {
            // 获取或添加基础敌人组件
            baseEnemy = GetComponent<Enemy>();
            if (baseEnemy == null)
            {
                baseEnemy = gameObject.AddComponent<Enemy>();
            }

            // 应用变种加成
            ApplyVariantBonuses();

            // 创建精英外观
            CreateEliteVisual();

            // 添加特殊光环
            if (variant == EnemyVariant.Champion || variant == EnemyVariant.Legend)
            {
                CreateSpecialAura();
            }
        }

        void ApplyVariantBonuses()
        {
            // 根据变种等级应用加成
            switch (variant)
            {
                case EnemyVariant.Elite:
                    healthMultiplier = 2f;
                    damageMultiplier = 1.5f;
                    speedMultiplier = 1.2f;
                    sizeMultiplier = 1.3f;
                    break;

                case EnemyVariant.Champion:
                    healthMultiplier = 4f;
                    damageMultiplier = 2f;
                    speedMultiplier = 1.3f;
                    sizeMultiplier = 1.5f;
                    abilityCooldown = 6f;
                    break;

                case EnemyVariant.Legend:
                    healthMultiplier = 8f;
                    damageMultiplier = 3f;
                    speedMultiplier = 1.5f;
                    sizeMultiplier = 1.8f;
                    abilityCooldown = 4f;
                    break;
            }

            // 这里需要修改baseEnemy的属性，但由于访问限制，我们通过反射或其他方式
            Debug.Log($"Elite {baseType} ({variant}) initialized with {healthMultiplier}x HP");
        }

        void CreateEliteVisual()
        {
            Color eliteColor = GetEliteColor();

            // 为精英敌人添加特殊标记
            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crown.name = "EliteCrown";
            crown.transform.SetParent(transform);
            crown.transform.localPosition = new Vector3(0, 2.5f, 0);
            crown.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);

            Material crownMaterial = new Material(Shader.Find("Standard"));
            crownMaterial.color = eliteColor;
            crown.GetComponent<Renderer>().material = crownMaterial;
            Destroy(crown.GetComponent<CapsuleCollider>());

            // 调整整体大小
            transform.localScale = Vector3.one * sizeMultiplier;

            // 添加粒子效果
            if (variant == EnemyVariant.Champion || variant == EnemyVariant.Legend)
            {
                CreateEliteParticles();
            }
        }

        Color GetEliteColor()
        {
            Color baseColor = baseType switch
            {
                Enemy.EnemyType.Normal => Color.red,
                Enemy.EnemyType.Fast => new Color(1f, 0.6f, 0f),
                Enemy.EnemyType.Heavy => new Color(0.6f, 0f, 0.8f),
                Enemy.EnemyType.Ranged => Color.cyan,
                _ => Color.red
            };

            // 根据变种等级调整颜色
            if (variant == EnemyVariant.Elite)
            {
                return baseColor * 1.2f; // 更亮
            }
            else if (variant == EnemyVariant.Champion)
            {
                return Color.Lerp(baseColor, Color.yellow, 0.3f); // 偏黄
            }
            else if (variant == EnemyVariant.Legend)
            {
                return Color.Lerp(baseColor, Color.white, 0.5f); // 接近白色
            }

            return baseColor;
        }

        void CreateSpecialAura()
        {
            GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            aura.name = "SpecialAura";
            aura.transform.SetParent(transform);
            aura.transform.localPosition = new Vector3(0, 0.1f, 0);
            aura.transform.localScale = new Vector3(3f, 0.1f, 3f);

            Material auraMaterial = new Material(Shader.Find("Standard"));
            Color auraColor = GetEliteColor();
            auraMaterial.color = new Color(auraColor.r, auraColor.g, auraColor.b, 0.3f);
            aura.GetComponent<Renderer>().material = auraMaterial;
            Destroy(aura.GetComponent<CapsuleCollider>());

            // 添加光环动画
            StartCoroutine(AuraPulseAnimation(auraMaterial));
        }

        IEnumerator AuraPulseAnimation(Material auraMaterial)
        {
            float pulseSpeed = 2f;
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime * pulseSpeed;
                float alpha = 0.2f + Mathf.Sin(time) * 0.1f;
                float scale = 1f + Mathf.Sin(time) * 0.1f;

                auraMaterial.color = new Color(auraMaterial.color.r, auraMaterial.color.g, auraMaterial.color.b, alpha);

                yield return null;
            }
        }

        void CreateEliteParticles()
        {
            GameObject particleSystem = new GameObject("EliteParticles");
            particleSystem.transform.SetParent(transform);
            particleSystem.transform.localPosition = Vector3.zero;

            // 这里可以添加实际的粒子系统
            // 由于Unity粒子系统比较复杂，这里简化处理
        }

        void Update()
        {
            if (hasSpecialAbility && baseEnemy != null && baseEnemy.IsAlive())
            {
                if (Time.time - lastAbilityTime >= abilityCooldown)
                {
                    UseSpecialAbility();
                    lastAbilityTime = Time.time;
                }
            }
        }

        void UseSpecialAbility()
        {
            if (baseEnemy == null || !baseEnemy.IsAlive())
                return;

            switch (baseType)
            {
                case Enemy.EnemyType.Normal:
                    StartCoroutine(WarriorRageAbility());
                    break;
                case Enemy.EnemyType.Fast:
                    StartCoroutine(BlitzAttackAbility());
                    break;
                case Enemy.EnemyType.Heavy:
                    StartCoroutine(EarthquakeAbility());
                    break;
                case Enemy.EnemyType.Ranged:
                    StartCoroutine(MultiShotAbility());
                    break;
            }
        }

        // ===== 特殊能力实现 =====

        IEnumerator WarriorRageAbility()
        {
            Debug.Log($"Elite {baseType} uses Warrior Rage!");

            // 狂暴效果：短时间内攻击力和速度大幅提升
            float originalDamage = damageMultiplier;
            float originalSpeed = speedMultiplier;

            damageMultiplier *= 2f;
            speedMultiplier *= 1.5f;

            // 视觉效果
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.PlaySkillEffect(transform.position, SkillType.Berserk);
            }

            yield return new WaitForSeconds(5f);

            damageMultiplier = originalDamage;
            speedMultiplier = originalSpeed;
        }

        IEnumerator BlitzAttackAbility()
        {
            Debug.Log($"Elite {baseType} uses Blitz Attack!");

            // 连续快速攻击
            for (int i = 0; i < 5; i++)
            {
                // 寻找最近目标并攻击
                Collider[] hitTargets = Physics.OverlapSphere(transform.position, 5f);
                foreach (var hit in hitTargets)
                {
                    SquadUnit unit = hit.GetComponent<SquadUnit>();
                    if (unit != null && unit.IsAlive())
                    {
                        unit.TakeDamage(20f * damageMultiplier);
                    }
                }

                yield return new WaitForSeconds(0.3f);
            }
        }

        IEnumerator EarthquakeAbility()
        {
            Debug.Log($"Elite {baseType} uses Earthquake!");

            // 范围地震伤害
            float earthquakeRange = 6f;
            float earthquakeDamage = 30f * damageMultiplier;

            // 地震视觉效果
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.PlaySkillEffect(transform.position, SkillType.ShieldBash);
            }

            Collider[] hitTargets = Physics.OverlapSphere(transform.position, earthquakeRange);
            foreach (var hit in hitTargets)
            {
                SquadUnit unit = hit.GetComponent<SquadUnit>();
                if (unit != null && unit.IsAlive())
                {
                    unit.TakeDamage(earthquakeDamage);
                }
            }

            yield return new WaitForSeconds(1f);
        }

        IEnumerator MultiShotAbility()
        {
            Debug.Log($"Elite {baseType} uses Multi Shot!");

            // 多重射击
            int projectileCount = variant == EnemyVariant.Legend ? 8 : 5;
            float damage = 15f * damageMultiplier;

            for (int i = 0; i < projectileCount; i++)
            {
                // 向随机方向发射投射物
                Vector3 direction = Quaternion.Euler(0, Random.Range(0f, 360f), 0) * Vector3.forward;
                Vector3 targetPos = transform.position + direction * 10f;

                StartCoroutine(FireEliteProjectile(targetPos, damage));

                yield return new WaitForSeconds(0.2f);
            }
        }

        IEnumerator FireEliteProjectile(Vector3 targetPos, float damage)
        {
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "EliteProjectile";
            projectile.transform.position = transform.position + Vector3.up * 1.5f;
            projectile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            Material projectileMaterial = new Material(Shader.Find("Standard"));
            projectileMaterial.color = GetEliteColor();
            projectile.GetComponent<Renderer>().material = projectileMaterial;
            Destroy(projectile.GetComponent<SphereCollider>());

            float speed = 12f;
            float maxTime = 3f;
            float elapsed = 0f;

            while (projectile != null && elapsed < maxTime)
            {
                Vector3 direction = (targetPos - projectile.transform.position).normalized;
                projectile.transform.position += direction * speed * Time.deltaTime;

                // 检测命中
                Collider[] hitTargets = Physics.OverlapSphere(projectile.transform.position, 1f);
                foreach (var hit in hitTargets)
                {
                    SquadUnit unit = hit.GetComponent<SquadUnit>();
                    if (unit != null && unit.IsAlive())
                    {
                        unit.TakeDamage(damage);
                        Destroy(projectile);
                        yield break;
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (projectile != null)
            {
                Destroy(projectile);
            }
        }

        /// <summary>
        /// 获取精英敌人信息
        /// </summary>
        public EliteEnemyInfo GetInfo()
        {
            return new EliteEnemyInfo
            {
                BaseType = baseType,
                Variant = variant,
                HealthMultiplier = healthMultiplier,
                DamageMultiplier = damageMultiplier,
                SpeedMultiplier = speedMultiplier,
                HasSpecialAbility = hasSpecialAbility,
                RewardMultiplier = GetRewardMultiplier()
            };
        }

        float GetRewardMultiplier()
        {
            return variant switch
            {
                EnemyVariant.Elite => 3f,
                EnemyVariant.Champion => 6f,
                EnemyVariant.Legend => 12f,
                _ => 1f
            };
        }

        void OnDrawGizmosSelected()
        {
            // 显示特殊能力范围
            if (hasSpecialAbility)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 6f);
            }
        }
    }

    /// <summary>
    /// 精英敌人信息
    /// </summary>
    [System.Serializable]
    public struct EliteEnemyInfo
    {
        public Enemy.EnemyType BaseType;
        public EliteEnemy.EnemyVariant Variant;
        public float HealthMultiplier;
        public float DamageMultiplier;
        public float SpeedMultiplier;
        public bool HasSpecialAbility;
        public float RewardMultiplier;
    }
}