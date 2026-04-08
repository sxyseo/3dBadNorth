using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// 敌人AI - 寻找并攻击玩家单位
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        public enum EnemyType
        {
            Normal,      // 普通战士
            Fast,        // 快速轻型
            Heavy,       // 重型坦克
            Ranged       // 远程弓箭手
        }

        [Header("属性")]
        public float maxHealth = 50f;
        public float currentHealth;
        public float moveSpeed = 3f;
        public float attackDamage = 10f;
        public float attackRange = 1.5f;
        public float attackCooldown = 1.5f;

        private EnemyType enemyType;

        private UnityEngine.AI.NavMeshAgent navAgent;
        private Animator animator;
        private HealthBar healthBar;
        private SquadUnit currentTarget;
        private bool isAttacking = false;
        private float lastAttackTime;
        private float goldReward = 5f;

        public void Initialize(int wave, int day)
        {
            // 从配置中读取基础数值
            float baseHealth = 50f;
            float baseDamage = 10f;
            float baseGold = 5f;

            // 根据波次和天数提升属性（使用配置）
            maxHealth = baseHealth + (wave * GameConfig.Enemies.HEALTH_PER_WAVE) + (day * 5f);
            attackDamage = baseDamage + (wave * GameConfig.Enemies.DAMAGE_PER_WAVE);
            goldReward = baseGold + (wave * GameConfig.Enemies.GOLD_PER_WAVE);

            currentHealth = maxHealth;

            navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = moveSpeed;

            animator = GetComponent<Animator>();
            healthBar = GetComponentInChildren<HealthBar>();

            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHealth);
            }

            // 设置不同类型的敌人外观
            SetEnemyType();
        }

        void SetEnemyType()
        {
            // 从配置中随机选择一种敌人类型
            GameConfig.EnemyTypeConfig config = GameConfig.Enemies.Types[Random.Range(0, GameConfig.Enemies.Types.Length)];

            // 应用配置
            enemyType = config.Type;
            moveSpeed *= config.SpeedMultiplier;
            maxHealth *= config.HealthMultiplier;
            attackDamage *= config.DamageMultiplier;
            attackRange = config.AttackRange;

            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
            }

            transform.localScale = config.Scale;

            // 应用配置的颜色
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            SetEnemyColor(renderers, config.Color);

            currentHealth = maxHealth;
        }

        void SetEnemyColor(Renderer[] renderers, Color color)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    renderer.material.color = color;
                }
            }
        }

        void Update()
        {
            if (!isAttacking)
            {
                FindAndAttackNearestUnit();
            }
        }

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
                    navAgent.SetDestination(nearestUnit.transform.position);
                }
            }
            else
            {
                // 没有找到单位，向基地移动
                navAgent.SetDestination(Vector3.zero);
            }
        }

        IEnumerator AttackUnit(SquadUnit unit)
        {
            isAttacking = true;

            while (unit != null && IsAlive())
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);

                if (distance <= attackRange)
                {
                    navAgent.isStopped = true;

                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        lastAttackTime = Time.time;

                        // 远程敌人特殊处理
                        if (enemyType == EnemyType.Ranged)
                        {
                            yield return StartCoroutine(RangedAttack(unit));
                        }
                        else
                        {
                            yield return StartCoroutine(MeleeAttack(unit));
                        }
                    }
                }
                else
                {
                    navAgent.isStopped = false;
                    navAgent.SetDestination(unit.transform.position);
                }

                yield return new WaitForSeconds(0.2f);
            }

            isAttacking = false;
            navAgent.isStopped = false;
        }

        IEnumerator MeleeAttack(SquadUnit unit)
        {
            // 近战攻击动画
            Vector3 originalPos = transform.position;
            Vector3 direction = (unit.transform.position - transform.position).normalized;

            // 向前突刺
            float attackDuration = 0.2f;
            float elapsed = 0f;

            while (elapsed < attackDuration)
            {
                float t = elapsed / attackDuration;
                float thrust = Mathf.Sin(t * Mathf.PI) * 0.5f;
                transform.position = originalPos + direction * thrust;
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPos;

            // 造成伤害
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayAttackSound();
            }

            unit.TakeDamage(attackDamage);

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }

        IEnumerator RangedAttack(SquadUnit unit)
        {
            // 远程攻击 - 发射投射物
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayAttackSound();
            }

            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Projectile";
            projectile.transform.position = transform.position + Vector3.up * 1.5f;
            projectile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // 设置投射物颜色
            Renderer projRenderer = projectile.GetComponent<Renderer>();
            projRenderer.material.color = Color.cyan;

            // 发射投射物
            float projectileSpeed = 10f;
            Vector3 targetPos = unit.transform.position + Vector3.up;
            float travelTime = 0f;
            float maxTravelTime = 2f;

            while (projectile != null && travelTime < maxTravelTime)
            {
                if (unit == null || !unit.IsAlive())
                    break;

                Vector3 direction = (targetPos - projectile.transform.position).normalized;
                projectile.transform.position += direction * projectileSpeed * Time.deltaTime;
                projectile.transform.position = Vector3.Lerp(projectile.transform.position, targetPos, Time.deltaTime * 2f);

                travelTime += Time.deltaTime;

                // 检测命中
                if (Vector3.Distance(projectile.transform.position, targetPos) < 0.5f)
                {
                    if (AudioSynthesizer.Instance != null)
                    {
                        AudioSynthesizer.Instance.PlayHitSound();
                    }

                    unit.TakeDamage(attackDamage);
                    Destroy(projectile);
                    break;
                }

                yield return null;
            }

            if (projectile != null)
            {
                Destroy(projectile);
            }

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            if (healthBar != null)
            {
                healthBar.SetHealth(currentHealth);
            }

            // 受伤效果
            ShowDamageEffect();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        void ShowDamageEffect()
        {
            // 播放受击音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayHitSound();
            }

            // 受伤闪白效果
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                StartCoroutine(FlashWhite(renderer));
            }
        }

        IEnumerator FlashWhite(Renderer renderer)
        {
            if (renderer.material.HasProperty("_Color"))
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                renderer.material.color = originalColor;
            }
        }

        void Die()
        {
            // 播放死亡音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayDeathSound();
            }

            // 通知EconomyManager
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnEnemyKilled(enemyType);
            }

            // 通知成就系统
            if (Achievements.AchievementTracker.Instance != null)
            {
                Achievements.AchievementTracker.Instance.OnEnemyKilled(enemyType);
            }

            GameManager.Instance.OnEnemyKilled(goldReward);

            // 程序化死亡效果
            StartCoroutine(PlayDeathAnimation());
        }

        IEnumerator PlayDeathAnimation()
        {
            // 根据敌人类型播放不同的死亡动画
            float deathDuration = 0.5f;
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;

            while (elapsed < deathDuration)
            {
                float t = elapsed / deathDuration;

                if (enemyType == EnemyType.Heavy)
                {
                    // 重型单位倒下
                    transform.rotation = Quaternion.Euler(t * 90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                    transform.localScale = originalScale * (1f - t * 0.3f);
                }
                else
                {
                    // 其他单位缩小消失
                    transform.localScale = originalScale * (1f - t);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }
    }
}
