using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 多样化的玩家单位系统 - 支持不同类型的战斗单位
    /// 每种单位有独特的属性、武器和战斗风格
    /// </summary>
    public class SquadUnit : MonoBehaviour
    {
        [Header("单位类型")]
        public SquadUnitType unitType = SquadUnitType.Warrior;

        [Header("基础属性")]
        public float maxHealth = 100f;
        public float currentHealth;
        public float moveSpeed = 5f;
        public float attackDamage = 20f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;

        [Header("状态")]
        public bool isSelected = false;
        public bool isMoving = false;
        public bool isAttacking = false;
        public int unitLevel = 1;

        // 组件引用
        private UnityEngine.AI.NavMeshAgent navAgent;
        private Animator animator;
        private HealthBar healthBar;
        private Transform targetEnemy;
        private float lastAttackTime;

        // 技能系统
        private Ability[] abilities;
        private float abilityCooldownTimer;

        void Start()
        {
            // 根据单位类型应用配置
            ApplyUnitTypeConfig();

            currentHealth = maxHealth;

            navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = moveSpeed;

            // 应用天气影响
            ApplyWeatherEffects();

            animator = GetComponent<Animator>();
            healthBar = GetComponentInChildren<HealthBar>();

            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHealth);
            }

            // 初始化技能
            InitializeAbilities();

            // 创建视觉外观
            CreateUnitVisual();

            // 创建选择指示器
            CreateSelectionIndicator();
        }

        void ApplyUnitTypeConfig()
        {
            // 从配置中获取当前单位类型的参数
            UnitTypesConfig.SquadUnitTypeConfig config = GetUnitConfig(unitType);

            if (config != null)
            {
                maxHealth = config.MaxHealth;
                moveSpeed = config.MoveSpeed;
                attackDamage = config.AttackDamage;
                attackRange = config.AttackRange;
                attackCooldown = config.AttackCooldown;
            }
        }

        UnitTypesConfig.SquadUnitTypeConfig GetUnitConfig(SquadUnitType type)
        {
            foreach (var config in UnitTypesConfig.PLAYER_UNITS)
            {
                if (config.Type == type)
                    return config;
            }
            return UnitTypesConfig.PLAYER_UNITS[0]; // 默认返回战士
        }

        void InitializeAbilities()
        {
            // 根据单位类型分配技能
            abilities = unitType switch
            {
                SquadUnitType.Warrior => new Ability[]
                {
                    new ActiveAbility("盾击", 10f, 3f, ShieldBash),
                    new PassiveAbility("坚韧", "受到的伤害减少20%")
                },
                SquadUnitType.Archer => new Ability[]
                {
                    new ActiveAbility("连射", 15f, 5f, MultiShot),
                    new PassiveAbility("精准", "暴击几率+15%")
                },
                SquadUnitType.Knight => new Ability[]
                {
                    new ActiveAbility("守护光环", 20f, 8f, GuardAura),
                    new PassiveAbility("护甲", "防御力+30%")
                },
                SquadUnitType.Berserker => new Ability[]
                {
                    new ActiveAbility("狂暴", 12f, 6f, BerserkMode),
                    new PassiveAbility("嗜血", "低血量时攻击力+50%")
                },
                _ => new Ability[0]
            };
        }

        void CreateUnitVisual()
        {
            // 根据单位类型创建不同的视觉外观
            UnitTypesConfig.SquadUnitTypeConfig config = GetUnitConfig(unitType);

            // 身体
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            body.transform.localScale = config.ModelScale;
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);

            Material bodyMaterial = new Material(Shader.Find("Standard"));
            bodyMaterial.color = config.Color;
            body.GetComponent<Renderer>().material = bodyMaterial;
            Destroy(body.GetComponent<CapsuleCollider>());

            // 武器
            CreateWeapon(config.WeaponType);

            // 根据等级添加视觉标识
            if (unitLevel > 1)
            {
                AddLevelIndicators();
            }
        }

        void CreateWeapon(WeaponType weaponType)
        {
            GameObject weapon = new GameObject("Weapon");
            weapon.transform.SetParent(transform);
            weapon.transform.localPosition = new Vector3(0.5f, 1.5f, 0);

            UnitTypesConfig.WeaponConfig weaponConfig = GetWeaponConfig(weaponType);

            switch (weaponType)
            {
                case WeaponType.Sword:
                    // 剑
                    GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    sword.transform.SetParent(weapon.transform);
                    sword.transform.localPosition = Vector3.zero;
                    sword.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
                    sword.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    SetWeaponMaterial(sword, Color.gray);
                    Destroy(sword.GetComponent<CapsuleCollider>());
                    break;

                case WeaponType.Bow:
                    // 弓
                    GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    bow.transform.SetParent(weapon.transform);
                    bow.transform.localPosition = new Vector3(0, 1.5f, 0);
                    bow.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
                    bow.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    SetWeaponMaterial(bow, new Color(0.6f, 0.4f, 0.2f));
                    Destroy(bow.GetComponent<CapsuleCollider>());

                    // 箭袋
                    GameObject quiver = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    quiver.transform.SetParent(weapon.transform);
                    quiver.transform.localPosition = new Vector3(-0.3f, 1.2f, 0);
                    quiver.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
                    SetWeaponMaterial(quiver, new Color(0.4f, 0.3f, 0.2f));
                    Destroy(quiver.GetComponent<CapsuleCollider>());
                    break;

                case WeaponType.SwordShield:
                    // 剑盾
                    GameObject sword2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    sword2.transform.SetParent(weapon.transform);
                    sword2.transform.localPosition = new Vector3(0.4f, 1.5f, 0);
                    sword2.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
                    sword2.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    SetWeaponMaterial(sword2, Color.gray);
                    Destroy(sword2.GetComponent<CapsuleCollider>());

                    // 盾牌
                    GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    shield.transform.SetParent(weapon.transform);
                    shield.transform.localPosition = new Vector3(-0.3f, 1.2f, 0);
                    shield.transform.localScale = new Vector3(0.08f, 0.6f, 0.4f);
                    SetWeaponMaterial(shield, new Color(0.7f, 0.7f, 0.7f));
                    Destroy(shield.GetComponent<CapsuleCollider>());
                    break;

                case WeaponType.Axe:
                    // 斧
                    GameObject axe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    axe.transform.SetParent(weapon.transform);
                    axe.transform.localPosition = Vector3.zero;
                    axe.transform.localScale = new Vector3(0.15f, 0.7f, 0.15f);
                    axe.transform.localRotation = Quaternion.Euler(45f, 0, 90);
                    SetWeaponMaterial(axe, new Color(0.5f, 0.3f, 0.1f));
                    Destroy(axe.GetComponent<CapsuleCollider>());
                    break;
            }
        }

        void SetWeaponMaterial(GameObject weapon, Color color)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            weapon.GetComponent<Renderer>().material = material;
        }

        UnitTypesConfig.WeaponConfig GetWeaponConfig(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Sword: return UnitTypesConfig.Weapons.SWORD;
                case WeaponType.Bow: return UnitTypesConfig.Weapons.BOW;
                case WeaponType.SwordShield: return UnitTypesConfig.Weapons.SWORD_SHIELD;
                case WeaponType.Axe: return UnitTypesConfig.Weapons.AXE;
                default: return UnitTypesConfig.Weapons.SWORD;
            }
        }

        void AddLevelIndicators()
        {
            // 根据等级添加装饰
            for (int i = 1; i < unitLevel; i++)
            {
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indicator.name = $"LevelIndicator_{i}";
                indicator.transform.SetParent(transform);
                indicator.transform.localPosition = new Vector3(-0.3f + (i * 0.15f), 2f, 0);
                indicator.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                Material indicatorMaterial = new Material(Shader.Find("Standard"));
                indicatorMaterial.color = Color.yellow;
                indicator.GetComponent<Renderer>().material = indicatorMaterial;
                Destroy(indicator.GetComponent<SphereCollider>());
            }
        }

        void Update()
        {
            UpdateSelectionIndicator();
            UpdateAbilities();
            UpdateWeatherEffects();

            if (isMoving && navAgent.remainingDistance < 0.5f)
            {
                isMoving = false;
                if (animator != null) animator.SetBool("IsMoving", false);
            }

            if (!isMoving && !isAttacking)
            {
                FindAndAttackEnemy();
            }
        }

        void UpdateAbilities()
        {
            // 更新技能冷却
            if (abilityCooldownTimer > 0)
            {
                abilityCooldownTimer -= Time.deltaTime;
            }

            // 检查被动技能效果
            foreach (var ability in abilities)
            {
                if (ability is PassiveAbility passive)
                {
                    passive.UpdateEffect(this);
                }
            }
        }

        void FindAndAttackEnemy()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var hit in hitColliders)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    targetEnemy = enemy.transform;
                    StartCoroutine(AttackEnemy(enemy));
                    break;
                }
            }
        }

        System.Collections.IEnumerator AttackEnemy(Enemy enemy)
        {
            isAttacking = true;

            while (enemy != null && enemy.IsAlive())
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance <= attackRange)
                {
                    navAgent.isStopped = true;

                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        lastAttackTime = Time.time;

                        // 播放攻击音效
                        if (AudioSynthesizer.Instance != null)
                        {
                            AudioSynthesizer.Instance.PlayAttackSound();
                        }

                        // 攻击动画
                        yield return StartCoroutine(PlayAttackAnimation());

                        // 计算伤害（考虑技能加成）
                        float finalDamage = attackDamage;
                        foreach (var ability in abilities)
                        {
                            if (ability is PassiveAbility passive)
                            finalDamage = passive.ModifyDamage(finalDamage, this);
                        }

                        enemy.TakeDamage(finalDamage);

                        if (animator != null)
                        {
                            animator.SetTrigger("Attack");
                        }
                    }
                }
                else
                {
                    navAgent.isStopped = false;

                    // 远程单位保持距离
                    if (unitType == SquadUnitType.Archer && distance < attackRange * 0.7f)
                    {
                        navAgent.isStopped = true;
                    }
                    else
                    {
                        navAgent.SetDestination(enemy.transform.position);
                    }
                }

                yield return new WaitForSeconds(0.2f);
            }

            isAttacking = false;
            targetEnemy = null;
            navAgent.isStopped = false;
        }

        System.Collections.IEnumerator PlayAttackAnimation()
        {
            Vector3 originalPos = transform.position;
            Vector3 attackDirection = (targetEnemy != null) ?
                (targetEnemy.position - transform.position).normalized : transform.forward;
            float attackDuration = 0.15f;
            float elapsed = 0f;

            while (elapsed < attackDuration)
            {
                float t = elapsed / attackDuration;
                float thrust = Mathf.Sin(t * Mathf.PI) * 0.5f;
                transform.position = originalPos + attackDirection * thrust;
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPos;
        }

        // ==================== 技能系统 ====================

        public void UseAbility(int index)
        {
            if (index < 0 || index >= abilities.Length) return;

            if (abilities[index] is ActiveAbility active)
            {
                if (active.CanUse(abilityCooldownTimer))
                {
                    active.Execute(this);
                    abilityCooldownTimer = active.Cooldown;
                }
            }
        }

        void ShieldBash(MonoBehaviour user)
        {
            SquadUnit unit = (SquadUnit)user;
            // 盾击：范围伤害+击退
            Collider[] hitColliders = Physics.OverlapSphere(unit.transform.position, 3f);
            foreach (var hit in hitColliders)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(unit.attackDamage * 1.5f);
                    // 击退效果
                    Vector3 knockback = (enemy.transform.position - unit.transform.position).normalized * 3f;
                    enemy.transform.position += knockback;
                }
            }
        }

        void MultiShot(MonoBehaviour user)
        {
            SquadUnit unit = (SquadUnit)user;
            // 连射：攻击3个目标
            Collider[] hitColliders = Physics.OverlapSphere(unit.transform.position, unit.attackRange);
            int targets = 0;
            foreach (var hit in hitColliders)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && targets < 3)
                {
                    enemy.TakeDamage(unit.attackDamage * 0.7f);
                    targets++;
                }
            }
        }

        void GuardAura(MonoBehaviour user)
        {
            SquadUnit unit = (SquadUnit)user;
            // 守护光环：附近友军防御提升
            Collider[] hitColliders = Physics.OverlapSphere(unit.transform.position, 5f);
            foreach (var hit in hitColliders)
            {
                SquadUnit ally = hit.GetComponent<SquadUnit>();
                if (ally != null && ally != unit)
                {
                    ally.StartCoroutine(GuardAuraEffect(ally, 5f));
                }
            }
        }

        System.Collections.IEnumerator GuardAuraEffect(SquadUnit ally, float duration)
        {
            // 临时提升防御
            float originalDefense = 0f; // TODO: 实现防御系统
            yield return new WaitForSeconds(duration);
        }

        void BerserkMode(MonoBehaviour user)
        {
            SquadUnit unit = (SquadUnit)user;
            // 狂暴：短时间内大幅提升攻击力
            float originalDamage = unit.attackDamage;
            unit.attackDamage *= 2f;

            StartCoroutine(BerserkEffect(unit, 6f, originalDamage));
        }

        System.Collections.IEnumerator BerserkEffect(SquadUnit unit, float duration, float originalDamage)
        {
            yield return new WaitForSeconds(duration);
            unit.attackDamage = originalDamage;
        }

        // ==================== 选择和移动 ====================

        void CreateSelectionIndicator()
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "SelectionIndicator";
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = new Vector3(0, 0.1f, 0);
            indicator.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);

            UnitTypesConfig.SquadUnitTypeConfig config = GetUnitConfig(unitType);
            indicator.GetComponent<MeshRenderer>().material.color = config.Color;
            indicator.SetActive(false);
            Destroy(indicator.GetComponent<CapsuleCollider>());
        }

        void UpdateSelectionIndicator()
        {
            Transform indicator = transform.Find("SelectionIndicator");
            if (indicator != null)
            {
                indicator.gameObject.SetActive(isSelected);
            }
        }

        public void MoveTo(Vector3 targetPosition)
        {
            isAttacking = false;
            StopAllCoroutines();
            navAgent.SetDestination(targetPosition);
            isMoving = true;

            if (animator != null)
            {
                animator.SetBool("IsMoving", true);
            }
        }

        public void Select()
        {
            isSelected = true;
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlaySelectSound();
            }
        }

        public void Deselect()
        {
            isSelected = false;
        }

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

        void Die()
        {
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayDeathSound();
            }

            GameManager.Instance.OnUnitKilled(gameObject);
            StartCoroutine(PlayDeathAnimation());
        }

        System.Collections.IEnumerator PlayDeathAnimation()
        {
            float deathDuration = 0.4f;
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;

            while (elapsed < deathDuration)
            {
                float t = elapsed / deathDuration;
                transform.localScale = originalScale * (1f - t * 0.5f);
                transform.rotation = Quaternion.Euler(t * 90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }

        void OnDestroy()
        {
            Transform indicator = transform.Find("SelectionIndicator");
            if (indicator != null)
            {
                Destroy(indicator.gameObject);
            }
        }

        // ==================== 天气系统 ====================

        void ApplyWeatherEffects()
        {
            if (Weather.WeatherSystem.Instance == null)
                return;

            float movementModifier = Weather.WeatherSystem.Instance.GetMovementModifier();

            if (navAgent != null)
            {
                navAgent.speed = moveSpeed * movementModifier;
            }
        }

        void UpdateWeatherEffects()
        {
            if (Weather.WeatherSystem.Instance == null)
                return;

            // 持续应用天气影响（因为天气可能动态变化）
            float movementModifier = Weather.WeatherSystem.Instance.GetMovementModifier();

            if (navAgent != null && !isMoving)
            {
                navAgent.speed = moveSpeed * movementModifier;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    // ==================== 技能系统 ====================

    public abstract class Ability
    {
        public string Name { get; set; }
        public float Cooldown { get; set; }
        public abstract void Execute(MonoBehaviour user);
    }

    public class ActiveAbility : Ability
    {
        public System.Action<MonoBehaviour> Action { get; set; }

        public ActiveAbility(string name, float cooldown, float duration, System.Action<MonoBehaviour> action)
        {
            Name = name;
            Cooldown = cooldown;
            Action = action;
        }

        public bool CanUse(float currentCooldown)
        {
            return currentCooldown <= 0f;
        }

        public void Execute(MonoBehaviour user)
        {
            Action?.Invoke(user);
        }
    }

    public class PassiveAbility : Ability
    {
        public string Description { get; set; }
        public System.Func<float, float, MonoBehaviour, float> DamageModifier { get; set; }

        public PassiveAbility(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void UpdateEffect(MonoBehaviour user) { }

        public float ModifyDamage(float baseDamage, MonoBehaviour user)
        {
            return DamageModifier?.Invoke(baseDamage, user) ?? baseDamage;
        }
    }
}
