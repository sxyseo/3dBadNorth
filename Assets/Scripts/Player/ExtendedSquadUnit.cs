using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BadNorth3D
{
    /// <summary>
    /// 扩展战斗单位 - 支持Stage 4的新单位类型和技能
    /// 包含牧师、工程师、游侠等新职业
    /// </summary>
    public class ExtendedSquadUnit : MonoBehaviour
    {
        [Header("单位配置")]
        public SquadUnitType unitType = SquadUnitType.Warrior;

        [Header("基础属性")]
        public float maxHealth = 100f;
        public float currentHealth;
        public float attackDamage = 20f;
        public float attackRange = 2f;
        public float moveSpeed = 3.5f;
        public float attackSpeed = 1.2f;

        [Header("扩展属性 - Stage 4")]
        public float healingPower = 0f;      // 治疗强度
        public float visionRange = 10f;      // 视野范围
        public int maxTurrets = 0;          // 最大炮塔数
        public int maxTraps = 0;            // 最大陷阱数
        public bool isSupportUnit = false;  // 辅助单位
        public bool isScoutUnit = false;    // 侦查单位

        // 组件引用
        private UnityEngine.AI.NavMeshAgent navAgent;
        private Animator animator;
        private HealthBar healthBar;
        private LineRenderer sightLine;     // 视野指示器

        // 战斗状态
        private bool isMoving = false;
        private bool isAttacking = false;
        private bool isUsingAbility = false;
        private SquadUnit currentTarget;
        private float lastAttackTime;

        // 特殊单位状态
        private List<GameObject> deployedTurrets = new List<GameObject>();
        private List<GameObject> placedTraps = new List<GameObject>();
        private Coroutine healingCoroutine;

        // 选择状态
        public bool isSelected = false;
        private GameObject selectionIndicator;

        void Start()
        {
            // 根据单位类型应用配置
            ApplyUnitConfig();

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

            CreateUnitVisual();
            CreateSelectionIndicator();

            // 特殊单位初始化
            if (isSupportUnit)
            {
                StartSupportBehavior();
            }

            if (isScoutUnit)
            {
                CreateSightIndicator();
                StartScoutBehavior();
            }
        }

        void ApplyUnitConfig()
        {
            var config = ExtendedUnitTypesConfig.GetUnitConfig(unitType);

            maxHealth = config.MaxHealth;
            attackDamage = config.AttackDamage;
            attackRange = config.AttackRange;
            moveSpeed = config.MoveSpeed;
            attackSpeed = config.AttackSpeed;
            healingPower = config.HealingPower;
            visionRange = config.VisionRange;
            maxTurrets = config.MaxTurrets;
            maxTraps = config.TrapCount;
            isSupportUnit = config.SupportRole;
            isScoutUnit = config.ScoutRole;
        }

        void Update()
        {
            UpdateSelectionIndicator();
            UpdateSpecialAbilities();

            if (isMoving && navAgent.remainingDistance < 0.5f)
            {
                isMoving = false;
                if (animator != null) animator.SetBool("IsMoving", false);
            }

            // 优先执行特殊单位行为
            if (isSupportUnit && !isUsingAbility)
            {
                UpdateSupportBehavior();
            }
            else if (!isAttacking && !isUsingAbility)
            {
                FindAndAttackEnemy();
            }
        }

        // ==================== 特殊单位行为 ====================

        void StartSupportBehavior()
        {
            if (unitType == SquadUnitType.Cleric)
            {
                // 牧师开始自动治疗附近友军
                healingCoroutine = StartCoroutine(AutoHealNearbyAllies());
            }
        }

        void UpdateSupportBehavior()
        {
            if (unitType == SquadUnitType.Cleric)
            {
                // 物师优先寻找受伤友军
                SquadUnit woundedAlly = FindWoundedAlly();
                if (woundedAlly != null)
                {
                    float distance = Vector3.Distance(transform.position, woundedAlly.transform.position);
                    if (distance > attackRange)
                    {
                        MoveToAlly(woundedAlly);
                    }
                    else
                    {
                        HealAlly(woundedAlly);
                    }
                }
            }
        }

        void StartScoutBehavior()
        {
            if (unitType == SquadUnitType.Ranger)
            {
                // 游侠扩大视野范围
                if (navAgent != null)
                {
                    // NavMesh没有直接的视野设置，这里通过其他方式模拟
                }
            }
        }

        IEnumerator AutoHealNearbyAllies()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);

                if (!IsAlive()) yield break;

                // 治疗范围内所有受伤友军
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
                foreach (var hit in hitColliders)
                {
                    SquadUnit ally = hit.GetComponent<SquadUnit>();
                    if (ally != null && ally.IsAlive() && ally.currentHealth < ally.maxHealth)
                    {
                        float healAmount = healingPower * 0.1f; // 小额持续治疗
                        ally.Heal(healAmount);

                        // 治疗特效
                        if (CombatEffects.Instance != null)
                        {
                            CombatEffects.Instance.ShowDamageNumber(healAmount, ally.transform.position);
                        }
                    }
                }
            }
        }

        SquadUnit FindWoundedAlly()
        {
            SquadUnit[] allAllies = FindObjectsOfType<SquadUnit>();
            SquadUnit mostWounded = null;
            float lowestHealthPercent = 1f;

            foreach (SquadUnit ally in allAllies)
            {
                if (ally == (SquadUnit)this) continue;

                float healthPercent = ally.currentHealth / ally.maxHealth;
                if (healthPercent < 1f && healthPercent < lowestHealthPercent)
                {
                    float distance = Vector3.Distance(transform.position, ally.transform.position);
                    if (distance < visionRange) // 在视野范围内
                    {
                        mostWounded = ally;
                        lowestHealthPercent = healthPercent;
                    }
                }
            }

            return mostWounded;
        }

        void MoveToAlly(SquadUnit ally)
        {
            if (navAgent != null && ally != null)
            {
                navAgent.SetDestination(ally.transform.position);
                isMoving = true;
                if (animator != null) animator.SetBool("IsMoving", true);
            }
        }

        void HealAlly(SquadUnit ally)
        {
            if (ally != null && healingPower > 0)
            {
                ally.Heal(healingPower);

                // 治疗特效
                if (CombatEffects.Instance != null)
                {
                    CombatEffects.Instance.ShowDamageNumber(healingPower, ally.transform.position);
                }

                // 治疗音效
                if (AudioSynthesizer.Instance != null)
                {
                    AudioSynthesizer.Instance.PlaySelectSound(); // 临时使用选择音效
                }
            }
        }

        void CreateSightIndicator()
        {
            // 创建视野范围指示器
            GameObject sightObj = new GameObject("SightIndicator");
            sightObj.transform.SetParent(transform);
            sightObj.transform.localPosition = Vector3.zero;

            LineRenderer lineRenderer = sightObj.AddComponent<LineRenderer>();
            sightLine = lineRenderer;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.color = new Color(0.3f, 1f, 0.3f, 0.3f);
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.positionCount = 50;
            lineRenderer.loop = true;

            // 创建圆形视野指示器
            UpdateSightIndicator();
        }

        void UpdateSightIndicator()
        {
            if (sightLine == null) return;

            Vector3[] positions = new Vector3[50];
            for (int i = 0; i < 50; i++)
            {
                float angle = i * 360f / 50f * Mathf.Deg2Rad;
                positions[i] = new Vector3(
                    Mathf.Cos(angle) * visionRange,
                    0.1f,
                    Mathf.Sin(angle) * visionRange
                );
            }

            sightLine.SetPositions(positions);
        }

        // ==================== 特殊能力 ====================

        void UpdateSpecialAbilities()
        {
            // 检查能力使用
            if (Input.GetKeyDown(KeyCode.Space) && isSelected && !isUsingAbility)
            {
                switch (unitType)
                {
                    case SquadUnitType.Cleric:
                        StartCoroutine(UseGroupHeal());
                        break;
                    case SquadUnitType.Engineer:
                        StartCoroutine(BuildTurret());
                        break;
                    case SquadUnitType.Ranger:
                        StartCoroutine(PlaceTraps());
                        break;
                }
            }
        }

        IEnumerator UseGroupHeal()
        {
            isUsingAbility = true;

            // 群体治疗技能
            float healRadius = 5f;
            float healAmount = healingPower * 2f;

            // 治疗特效
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.PlaySkillEffect(transform.position, SkillType.GuardAura); // 复用守护光环特效
            }

            // 治疗范围内所有友军
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, healRadius);
            foreach (var hit in hitColliders)
            {
                SquadUnit ally = hit.GetComponent<SquadUnit>();
                if (ally != null && ally.IsAlive())
                {
                    ally.Heal(healAmount);
                    if (CombatEffects.Instance != null)
                    {
                        CombatEffects.Instance.ShowDamageNumber(healAmount, ally.transform.position);
                    }
                }
            }

            // 播放音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound();
            }

            yield return new WaitForSeconds(1f);
            isUsingAbility = false;
        }

        IEnumerator BuildTurret()
        {
            isUsingAbility = true;

            if (deployedTurrets.Count >= maxTurrets)
            {
                Debug.Log("Maximum turrets reached!");
                isUsingAbility = false;
                yield break;
            }

            // 创建炮塔
            GameObject turret = new GameObject("AutoTurret");
            turret.transform.position = transform.position + transform.forward * 2f;

            // 炮塔外观
            GameObject turretBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            turretBase.transform.SetParent(turret.transform);
            turretBase.transform.localPosition = new Vector3(0, 0.5f, 0);
            turretBase.transform.localScale = new Vector3(1f, 1f, 1f);
            turretBase.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f);
            Destroy(turretBase.GetComponent<CapsuleCollider>());

            // 炮塔头部
            GameObject turretHead = GameObject.CreatePrimitive(PrimitiveType.Box);
            turretHead.transform.SetParent(turret.transform);
            turretHead.transform.localPosition = new Vector3(0, 1.5f, 0);
            turretHead.transform.localScale = new Vector3(0.5f, 0.5f, 0.8f);
            turretHead.GetComponent<Renderer>().material.color = new Color(0.4f, 0.3f, 0.1f);
            Destroy(turretHead.GetComponent<BoxCollider>());

            // 添加炮塔组件
            Turret turretComponent = turret.AddComponent<Turret>();
            turretComponent.Initialize(attackDamage * 0.8f, attackRange * 0.8f);

            deployedTurrets.Add(turret);

            // 建造特效
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.PlaySkillEffect(turret.transform.position, SkillType.ShieldBash);
            }

            // 播放音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound();
            }

            yield return new WaitForSeconds(2f);
            isUsingAbility = false;
        }

        IEnumerator PlaceTraps()
        {
            isUsingAbility = true;

            if (placedTraps.Count >= maxTraps)
            {
                // 移除最旧的陷阱
                if (placedTraps.Count > 0)
                {
                    Destroy(placedTraps[0]);
                    placedTraps.RemoveAt(0);
                }
            }

            // 创建陷阱
            GameObject trap = new GameObject("Trap");
            trap.transform.position = transform.position + transform.forward * 3f;

            // 陷阱外观
            GameObject trapVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trapVisual.transform.SetParent(trap.transform);
            trapVisual.transform.localPosition = new Vector3(0, 0.05f, 0);
            trapVisual.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
            trapVisual.GetComponent<Renderer>().material.color = new Color(0.2f, 0.4f, 0.2f, 0.5f);
            Destroy(trapVisual.GetComponent<CapsuleCollider>());

            // 添加陷阱组件
            Trap trapComponent = trap.AddComponent<Trap>();
            trapComponent.Initialize(attackDamage * 0.5f, 5f); // 减速5秒

            placedTraps.Add(trap);

            // 陷阱放置特效
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.PlaySkillEffect(trap.transform.position, SkillType.MultiShot);
            }

            // 播放音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound();
            }

            yield return new WaitForSeconds(1f);
            isUsingAbility = false;
        }

        // ==================== 基础战斗功能 ====================

        void FindAndAttackEnemy()
        {
            Enemy[] allEnemies = FindObjectsOfType<Enemy>();
            Enemy nearestEnemy = null;
            float nearestDistance = Mathf.Infinity;

            foreach (Enemy enemy in allEnemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null)
            {
                currentTarget = nearestEnemy.GetComponent<SquadUnit>();

                if (nearestDistance <= attackRange)
                {
                    StartCoroutine(AttackEnemy(nearestEnemy));
                }
                else
                {
                    if (navAgent != null)
                    {
                        navAgent.SetDestination(nearestEnemy.transform.position);
                        isMoving = true;
                        if (animator != null) animator.SetBool("IsMoving", true);
                    }
                }
            }
        }

        IEnumerator AttackEnemy(Enemy enemy)
        {
            isAttacking = true;

            while (enemy != null && IsAlive())
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance <= attackRange)
                {
                    if (navAgent != null)
                    {
                        navAgent.isStopped = true;
                    }

                    if (Time.time - lastAttackTime >= attackSpeed)
                    {
                        lastAttackTime = Time.time;

                        // 播放攻击音效
                        if (AudioSynthesizer.Instance != null)
                        {
                            AudioSynthesizer.Instance.PlayAttackSound();
                        }

                        // 造成伤害
                        enemy.TakeDamage(attackDamage);
                    }
                }
                else
                {
                    if (navAgent != null)
                    {
                        navAgent.isStopped = false;
                        navAgent.SetDestination(enemy.transform.position);
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

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }

            // 受伤特效
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.ShowDamageNumber(damage, transform.position);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }
        }

        void Die()
        {
            // 清理特殊单位的状态
            if (healingCoroutine != null)
            {
                StopCoroutine(healingCoroutine);
            }

            // 清理炮塔
            foreach (GameObject turret in deployedTurrets)
            {
                if (turret != null)
                {
                    Destroy(turret);
                }
            }

            // 清理陷阱
            foreach (GameObject trap in placedTraps)
            {
                if (trap != null)
                {
                    Destroy(trap);
                }
            }

            // 死亡音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayDeathSound();
            }

            // 通知游戏管理器
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnUnitKilled(gameObject);
            }

            Destroy(gameObject);
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }

        // ==================== 单位视觉 ====================

        void CreateUnitVisual()
        {
            var config = ExtendedUnitTypesConfig.GetUnitConfig(unitType);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);

            Material bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = config.Color;
            body.GetComponent<Renderer>().material = bodyMaterial;
            Destroy(body.GetComponent<CapsuleCollider>());

            CreateWeapon(config.WeaponType);
        }

        void CreateWeapon(Weapon weaponType)
        {
            GameObject weaponObj = new GameObject("Weapon");
            weaponObj.transform.SetParent(transform);
            weaponObj.transform.localPosition = new Vector3(0.6f, 1.2f, 0);

            switch (weaponType)
            {
                case Weapon.Sword:
                    GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    sword.transform.SetParent(weaponObj.transform);
                    sword.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
                    sword.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    Destroy(sword.GetComponent<CapsuleCollider>());
                    break;

                case Weapon.Bow:
                    GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    bow.transform.SetParent(weaponObj.transform);
                    bow.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
                    Destroy(bow.GetComponent<CapsuleCollider>());
                    break;

                case Weapon.Staff: // 物师法杖
                    GameObject staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    staff.transform.SetParent(weaponObj.transform);
                    staff.transform.localScale = new Vector3(0.15f, 1.2f, 0.15f);
                    staff.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    Material staffMaterial = new Material(Shader.Find("Standard"));
                    staffMaterial.color = new Color(0.8f, 0.6f, 0.2f); // 金色
                    staff.GetComponent<Renderer>().material = staffMaterial;
                    Destroy(staff.GetComponent<CapsuleCollider>());

                    // 法杖顶部水晶
                    GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crystal.transform.SetParent(weaponObj.transform);
                    crystal.transform.localPosition = new Vector3(0, 0.6f, 0);
                    crystal.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    Material crystalMaterial = new Material(Shader.Find("Standard"));
                    crystalMaterial.color = new Color(0.5f, 0.8f, 1f); // 蓝色
                    crystal.GetComponent<Renderer>().material = crystalMaterial;
                    Destroy(crystal.GetComponent<SphereCollider>());
                    break;

                case Weapon.Wrench: // 工程师扳手
                    GameObject wrench = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    wrench.transform.SetParent(weaponObj.transform);
                    wrench.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
                    wrench.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    Material wrenchMaterial = new Material(Shader.Find("Standard"));
                    wrenchMaterial.color = new Color(0.5f, 0.5f, 0.5f); // 银色
                    wrench.GetComponent<Renderer>().material = wrenchMaterial;
                    Destroy(wrench.GetComponent<CapsuleCollider>());
                    break;

                case Weapon.Dagger: // 游侠匕首
                    GameObject dagger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    dagger.transform.SetParent(weaponObj.transform);
                    dagger.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
                    dagger.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    Destroy(dagger.GetComponent<CapsuleCollider>());
                    break;

                default:
                    GameObject defaultWeapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    defaultWeapon.transform.SetParent(weaponObj.transform);
                    defaultWeapon.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
                    Destroy(defaultWeapon.GetComponent<BoxCollider>());
                    break;
            }
        }

        void CreateSelectionIndicator()
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "SelectionIndicator";
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = new Vector3(0, 0.05f, 0);
            indicator.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);

            Material indicatorMaterial = new Material(Shader.Find("Sprites/Default"));
            indicatorMaterial.color = new Color(0f, 1f, 0f, 0.3f);
            indicator.GetComponent<Renderer>().material = indicatorMaterial;
            Destroy(indicator.GetComponent<CapsuleCollider>());

            indicator.SetActive(false);
            selectionIndicator = indicator;
        }

        void UpdateSelectionIndicator()
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(isSelected);
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }

        void OnDrawGizmosSelected()
        {
            // 显示攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 显示视野范围（如果是侦查单位）
            if (isScoutUnit)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, visionRange);
            }
        }
    }

    // ==================== 特殊单位组件 ====================

    /// <summary>
    /// 炮塔组件 - 工程师建造
    /// </summary>
    public class Turret : MonoBehaviour
    {
        private float damage;
        private float range;
        private float fireRate = 1.5f;
        private float lastFireTime;
        private Enemy currentTarget;

        public void Initialize(float turretDamage, float turretRange)
        {
            damage = turretDamage;
            range = turretRange;
        }

        void Update()
        {
            FindTarget();
            AttackTarget();
        }

        void FindTarget()
        {
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            Enemy nearestEnemy = null;
            float nearestDistance = range;

            foreach (Enemy enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            currentTarget = nearestEnemy;
        }

        void AttackTarget()
        {
            if (currentTarget == null || !currentTarget.IsAlive())
                return;

            if (Time.time - lastFireTime >= fireRate)
            {
                lastFireTime = Time.time;

                // 发射投射物
                StartCoroutine(FireProjectile(currentTarget.transform));
            }
        }

        IEnumerator FireProjectile(Transform target)
        {
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.position = transform.position + Vector3.up * 1.5f;
            projectile.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(1f, 0.5f, 0f); // 橙色
            projectile.GetComponent<Renderer>().material = material;
            Destroy(projectile.GetComponent<SphereCollider>());

            float speed = 10f;
            float maxTime = 2f;
            float elapsed = 0f;

            while (projectile != null && elapsed < maxTime)
            {
                if (target == null)
                    break;

                Vector3 direction = (target.position - projectile.transform.position).normalized;
                projectile.transform.position += direction * speed * Time.deltaTime;

                // 检测命中
                if (Vector3.Distance(projectile.transform.position, target.position) < 0.5f)
                {
                    if (currentTarget != null && currentTarget.IsAlive())
                    {
                        currentTarget.TakeDamage(damage);
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
    }

    /// <summary>
    /// 陷阱组件 - 游侠放置
    /// </summary>
    public class Trap : MonoBehaviour
    {
        private float damage;
        private float slowDuration;
        private bool armed = true;

        public void Initialize(float trapDamage, float duration)
        {
            damage = trapDamage;
            slowDuration = duration;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!armed) return;

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null && enemy.IsAlive())
            {
                // 造成伤害
                enemy.TakeDamage(damage);

                // 施加减速效果（这里简化处理）
                Debug.Log($"Trap triggered! Damage: {damage}, Slow: {slowDuration}s");

                // 陷阱被触发后销毁
                armed = false;
                Destroy(gameObject);
            }
        }
    }
}