using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// Boss敌人 - 强大的特殊敌人，具有独特技能和战斗机制
    /// AI可以调整Boss属性和技能模式
    /// </summary>
    public class BossEnemy : MonoBehaviour
    {
        public enum BossType
        {
            Warlord,      // 战争领主 - 近战猛攻
            Sorcerer,     // 术士 - 魔法攻击
            Assassin,     // 刺客 - 快速高爆发
            Guardian      // 守护者 - 高防御反击
        }

        [Header("Boss设置")]
        public BossType bossType = BossType.Warlord;
        public int bossLevel = 1;

        [Header("基础属性")]
        public float maxHealth = 500f;
        public float currentHealth;
        public float moveSpeed = 2f;
        public float attackDamage = 50f;
        public float attackRange = 3f;
        public float attackCooldown = 2f;

        [Header("技能设置")]
        public float skillCooldown = 10f;
        public float skillRange = 8f;

        // 组件引用
        private UnityEngine.AI.NavMeshAgent navAgent;
        private HealthBar healthBar;
        private SquadUnit currentTarget;
        private bool isAttacking = false;
        private bool isUsingSkill = false;
        private float lastAttackTime;
        private float lastSkillTime;

        // Boss状态
        private Phase currentPhase = Phase.Phase1;
        private float enrageThreshold = 0.3f; // 30%血量时狂暴

        void Start()
        {
            InitializeBoss();
        }

        /// <summary>
        /// 初始化Boss
        /// </summary>
        void InitializeBoss()
        {
            // 根据Boss类型调整属性
            ApplyBossTypeConfig();

            currentHealth = maxHealth;

            navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
            }

            healthBar = GetComponentInChildren<HealthBar>();
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHealth);
            }

            CreateBossVisual();
            CreateBossIndicator();

            Debug.Log($"Boss spawned: {bossType} (Level {bossLevel})");
        }

        /// <summary>
        /// 应用Boss类型配置
        /// </summary>
        void ApplyBossTypeConfig()
        {
            switch (bossType)
            {
                case BossType.Warlord:
                    maxHealth = 800f;
                    attackDamage = 60f;
                    moveSpeed = 2.5f;
                    attackRange = 2.5f;
                    break;

                case BossType.Sorcerer:
                    maxHealth = 400f;
                    attackDamage = 40f;
                    moveSpeed = 1.5f;
                    attackRange = 10f;
                    break;

                case BossType.Assassin:
                    maxHealth = 350f;
                    attackDamage = 80f;
                    moveSpeed = 4f;
                    attackRange = 2f;
                    break;

                case BossType.Guardian:
                    maxHealth = 1200f;
                    attackDamage = 30f;
                    moveSpeed = 1f;
                    attackRange = 2f;
                    break;
            }

            // 根据等级提升属性
            float levelMultiplier = 1f + (bossLevel - 1) * 0.2f;
            maxHealth *= levelMultiplier;
            attackDamage *= levelMultiplier;

            currentHealth = maxHealth;
        }

        /// <summary>
        /// 创建Boss视觉外观
        /// </summary>
        void CreateBossVisual()
        {
            Color bossColor = GetBossColor();

            // 身体
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "BossBody";
            body.transform.SetParent(transform);
            body.transform.localPosition = new Vector3(0, 2f, 0);
            body.transform.localScale = new Vector3(2f, 2f, 2f);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);

            Material bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = bossColor;
            body.GetComponent<Renderer>().material = bodyMaterial;
            Destroy(body.GetComponent<CapsuleCollider>());

            // 武器/装饰
            CreateBossWeapon();
        }

        /// <summary>
        /// 创建Boss武器
        /// </summary>
        void CreateBossWeapon()
        {
            GameObject weapon = new GameObject("BossWeapon");
            weapon.transform.SetParent(transform);
            weapon.transform.localPosition = new Vector3(1.5f, 2f, 0);

            switch (bossType)
            {
                case BossType.Warlord:
                    // 巨型武器
                    GameObject giantWeapon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    giantWeapon.transform.SetParent(weapon.transform);
                    giantWeapon.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
                    giantWeapon.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    SetWeaponMaterial(giantWeapon, Color.black);
                    Destroy(giantWeapon.GetComponent<CapsuleCollider>());
                    break;

                case BossType.Sorcerer:
                    // 魔法球
                    GameObject magicOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    magicOrb.transform.SetParent(weapon.transform);
                    magicOrb.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    SetWeaponMaterial(magicOrb, new Color(0.5f, 0f, 1f));
                    Destroy(magicOrb.GetComponent<SphereCollider>());
                    break;

                case BossType.Assassin:
                    // 双匕首
                    GameObject dagger1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    dagger1.transform.SetParent(weapon.transform);
                    dagger1.transform.localPosition = new Vector3(0.2f, 0, 0);
                    dagger1.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
                    dagger1.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    SetWeaponMaterial(dagger1, new Color(0.3f, 0.3f, 0.3f));
                    Destroy(dagger1.GetComponent<CapsuleCollider>());

                    GameObject dagger2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    dagger2.transform.SetParent(weapon.transform);
                    dagger2.transform.localPosition = new Vector3(-0.2f, 0, 0);
                    dagger2.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
                    dagger2.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    SetWeaponMaterial(dagger2, new Color(0.3f, 0.3f, 0.3f));
                    Destroy(dagger2.GetComponent<CapsuleCollider>());
                    break;

                case BossType.Guardian:
                    // 盾牌
                    GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    shield.transform.SetParent(weapon.transform);
                    shield.transform.localScale = new Vector3(0.1f, 1.5f, 1.5f);
                    SetWeaponMaterial(shield, new Color(0.6f, 0.6f, 0.6f));
                    Destroy(shield.GetComponent<CapsuleCollider>());
                    break;
            }
        }

        void SetWeaponMaterial(GameObject weapon, Color color)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            weapon.GetComponent<Renderer>().material = material;
        }

        /// <summary>
        /// 创建Boss指示器
        /// </summary>
        void CreateBossIndicator()
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "BossIndicator";
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = new Vector3(0, 0.1f, 0);
            indicator.transform.localScale = new Vector3(3f, 0.1f, 3f);

            Color bossColor = GetBossColor();
            indicator.GetComponent<Renderer>().material.color = bossColor;
            Destroy(indicator.GetComponent<CapsuleCollider>());
        }

        /// <summary>
        /// 获取Boss颜色
        /// </summary>
        Color GetBossColor()
        {
            return bossType switch
            {
                BossType.Warlord => new Color(0.8f, 0.1f, 0.1f),     // 红色
                BossType.Sorcerer => new Color(0.5f, 0f, 1f),        // 紫色
                BossType.Assassin => new Color(0.2f, 0.2f, 0.2f),     // 黑色
                BossType.Guardian => new Color(0.1f, 0.5f, 0.8f),     // 蓝色
                _ => Color.red
            };
        }

        void Update()
        {
            UpdatePhase();
            UpdateSkills();

            if (!isAttacking && !isUsingSkill)
            {
                FindAndAttackNearestUnit();
            }
        }

        /// <summary>
        /// 更新Boss阶段
        /// </summary>
        void UpdatePhase()
        {
            float healthPercent = currentHealth / maxHealth;

            // 检查是否进入狂暴阶段
            if (healthPercent <= enrageThreshold && currentPhase != Phase.Enraged)
            {
                currentPhase = Phase.Enraged;
                OnEnrage();
            }
        }

        /// <summary>
        /// 狂暴触发
        /// </summary>
        void OnEnrage()
        {
            // 提升攻击力和速度
            attackDamage *= 1.5f;
            moveSpeed *= 1.3f;

            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
            }

            // 视觉效果
            PlayEnrageEffect();

            Debug.Log($"{bossType} Boss has become ENRAGED!");
        }

        /// <summary>
        /// 狂暴视觉效果
        /// </summary>
        void PlayEnrageEffect()
        {
            // 改变颜色
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    StartCoroutine(EnrageColorEffect(renderer));
                }
            }
        }

        IEnumerator EnrageColorEffect(Renderer renderer)
        {
            Color originalColor = renderer.material.color;
            Color enragedColor = Color.red;

            float duration = 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                renderer.material.color = Color.Lerp(originalColor, enragedColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            renderer.material.color = enragedColor;
        }

        /// <summary>
        /// 更新技能
        /// </summary>
        void UpdateSkills()
        {
            if (Time.time - lastSkillTime >= skillCooldown)
            {
                // 检查是否可以使用技能
                if (!isUsingSkill && !isAttacking)
                {
                    StartCoroutine(UseBossSkill());
                }
            }
        }

        /// <summary>
        /// 使用Boss技能
        /// </summary>
        IEnumerator UseBossSkill()
        {
            isUsingSkill = true;
            lastSkillTime = Time.time;

            switch (bossType)
            {
                case BossType.Warlord:
                    yield return StartCoroutine(WarlordSkill());
                    break;
                case BossType.Sorcerer:
                    yield return StartCoroutine(SorcererSkill());
                    break;
                case BossType.Assassin:
                    yield return StartCoroutine(AssassinSkill());
                    break;
                case BossType.Guardian:
                    yield return StartCoroutine(GuardianSkill());
                    break;
            }

            isUsingSkill = false;
        }

        /// <summary>
        /// 战争领主技能：旋风斩
        /// </summary>
        IEnumerator WarlordSkill()
        {
            Debug.Log("Warlord uses Whirlwind!");

            // 范围伤害
            float whirlwindRange = 5f;
            float whirlwindDamage = attackDamage * 0.8f;

            float duration = 3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, whirlwindRange);
                foreach (var hit in hitColliders)
                {
                    SquadUnit unit = hit.GetComponent<SquadUnit>();
                    if (unit != null)
                    {
                        unit.TakeDamage(whirlwindDamage * Time.deltaTime);
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// 术士技能：魔法轰炸
        /// </summary>
        IEnumerator SorcererSkill()
        {
            Debug.Log("Sorcerer uses Magic Barrage!");

            int projectileCount = 5;
            float barrageDamage = attackDamage * 0.6f;

            for (int i = 0; i < projectileCount; i++)
            {
                // 发射魔法弹
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, skillRange);
                foreach (var hit in hitColliders)
                {
                    SquadUnit unit = hit.GetComponent<SquadUnit>();
                    if (unit != null && unit.IsAlive())
                    {
                        FireMagicProjectile(unit.transform, barrageDamage);
                        break;
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        void FireMagicProjectile(Transform target, float damage)
        {
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "MagicProjectile";
            projectile.transform.position = transform.position + Vector3.up * 2f;
            projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.5f, 0f, 1f);
            projectile.GetComponent<Renderer>().material = material;
            Destroy(projectile.GetComponent<SphereCollider>());

            StartCoroutine(MagicProjectileCoroutine(projectile, target, damage));
        }

        IEnumerator MagicProjectileCoroutine(GameObject projectile, Transform target, float damage)
        {
            float speed = 8f;
            float maxTime = 3f;
            float elapsed = 0f;

            while (projectile != null && elapsed < maxTime)
            {
                if (target == null || !target.GetComponent<SquadUnit>().IsAlive())
                    break;

                Vector3 direction = (target.position - projectile.transform.position).normalized;
                projectile.transform.position += direction * speed * Time.deltaTime;

                // 检测命中
                if (Vector3.Distance(projectile.transform.position, target.position) < 1f)
                {
                    SquadUnit unit = target.GetComponent<SquadUnit>();
                    if (unit != null)
                    {
                        unit.TakeDamage(damage);
                    }
                    Destroy(projectile);
                    break;
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
        /// 刺客技能：暗影步
        /// </summary>
        IEnumerator AssassinSkill()
        {
            Debug.Log("Assassin uses Shadow Step!");

            // 寻找目标
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, skillRange);
            foreach (var hit in hitColliders)
            {
                SquadUnit unit = hit.GetComponent<SquadUnit>();
                if (unit != null && unit.IsAlive())
                {
                    // 瞬移到目标背后
                    Vector3 shadowPosition = unit.transform.position - unit.transform.forward * 1f;
                    transform.position = shadowPosition;

                    // 造成暴击伤害
                    float critDamage = attackDamage * 2f;
                    unit.TakeDamage(critDamage);

                    // 播放特效
                    if (AudioSynthesizer.Instance != null)
                    {
                        AudioSynthesizer.Instance.PlayAttackSound();
                    }

                    break;
                }
            }

            yield return new WaitForSeconds(1f);
        }

        /// <summary>
        /// 守护者技能：防御姿态
        /// </summary>
        IEnumerator GuardianSkill()
        {
            Debug.Log("Guardian uses Defensive Stance!");

            // 临时提升防御，降低受到的伤害
            float originalDamage = 0f; // 这里需要记录受到的伤害
            float defenseDuration = 5f;

            // 视觉效果
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    Color originalColor = renderer.material.color;
                    renderer.material.color = new Color(0.2f, 0.5f, 1f); // 蓝色护盾
                }
            }

            yield return new WaitForSeconds(defenseDuration);

            // 恢复正常
            Debug.Log("Guardian defensive stance ended.");
        }

        /// <summary>
        /// 寻找并攻击最近单位
        /// </summary>
        void FindAndAttackNearestUnit()
        {
            SquadUnit[] allUnits = FindObjectsOfType<SquadUnit>();
            SquadUnit nearestUnit = null;
            float nearestDistance = Mathf.Infinity;

            foreach (SquadUnit unit in allUnits)
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestUnit = unit;
                }
            }

            if (nearestUnit != null)
            {
                currentTarget = nearestUnit;

                if (nearestDistance <= attackRange)
                {
                    StartCoroutine(AttackUnit(nearestUnit));
                }
                else
                {
                    if (navAgent != null)
                    {
                        navAgent.SetDestination(nearestUnit.transform.position);
                    }
                }
            }
        }

        /// <summary>
        /// 攻击单位
        /// </summary>
        IEnumerator AttackUnit(SquadUnit unit)
        {
            isAttacking = true;

            while (unit != null && IsAlive())
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);

                if (distance <= attackRange)
                {
                    if (navAgent != null)
                    {
                        navAgent.isStopped = true;
                    }

                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        lastAttackTime = Time.time;

                        // 播放攻击音效
                        if (AudioSynthesizer.Instance != null)
                        {
                            AudioSynthesizer.Instance.PlayAttackSound();
                        }

                        // 造成伤害
                        unit.TakeDamage(attackDamage);
                    }
                }
                else
                {
                    if (navAgent != null)
                    {
                        navAgent.isStopped = false;
                        navAgent.SetDestination(unit.transform.position);
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }

            isAttacking = false;
            if (navAgent != null)
            {
                navAgent.isStopped = false;
            }
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 死亡
        /// </summary>
        void Die()
        {
            // 播放死亡音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayDeathSound();
            }

            // 给予大量奖励
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(200f * bossLevel);
            }

            // 通知成就系统
            if (Achievements.AchievementTracker.Instance != null)
            {
                // 可以添加Boss击杀成就
            }

            Debug.Log($"{bossType} Boss defeated!");

            // 死亡动画
            StartCoroutine(PlayDeathAnimation());
        }

        /// <summary>
        /// 播放死亡动画
        /// </summary>
        IEnumerator PlayDeathAnimation()
        {
            float deathDuration = 1f;
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;

            while (elapsed < deathDuration)
            {
                float t = elapsed / deathDuration;
                transform.localScale = originalScale * (1f - t);
                transform.rotation = Quaternion.Euler(t * 90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive()
        {
            return currentHealth > 0;
        }

        void OnDrawGizmosSelected()
        {
            // 显示攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 显示技能范围
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, skillRange);
        }
    }

    /// <summary>
    /// Boss阶段
    /// </summary>
    public enum Phase
    {
        Phase1,   // 第一阶段
        Phase2,   // 第二阶段
        Enraged   // 狂暴阶段
    }
}