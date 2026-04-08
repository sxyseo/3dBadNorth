using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// 敌人AI - 寻找并攻击玩家单位
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("属性")]
        public float maxHealth = 50f;
        public float currentHealth;
        public float moveSpeed = 3f;
        public float attackDamage = 10f;
        public float attackRange = 1.5f;
        public float attackCooldown = 1.5f;

        private UnityEngine.AI.NavMeshAgent navAgent;
        private Animator animator;
        private HealthBar healthBar;
        private SquadUnit currentTarget;
        private bool isAttacking = false;
        private float lastAttackTime;
        private float goldReward = 5f;

        public void Initialize(int wave, int day)
        {
            // 根据波次和天数提升属性
            maxHealth = 50f + (wave * 10f) + (day * 5f);
            attackDamage = 10f + (wave * 2f);
            goldReward = 5f + (wave * 1f);

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
            // 随机敌人类型，影响外观和属性
            int type = Random.Range(0, 3);

            switch (type)
            {
                case 0: // 普通战士
                    transform.localScale = Vector3.one;
                    break;
                case 1: // 快速轻型单位
                    moveSpeed = 5f;
                    maxHealth *= 0.7f;
                    navAgent.speed = moveSpeed;
                    transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    break;
                case 2: // 重型坦克
                    moveSpeed = 2f;
                    maxHealth *= 1.5f;
                    attackDamage *= 1.3f;
                    navAgent.speed = moveSpeed;
                    transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                    break;
            }

            currentHealth = maxHealth;
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
                        unit.TakeDamage(attackDamage);

                        if (animator != null)
                        {
                            animator.SetTrigger("Attack");
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
            // 简单的受伤闪红效果
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                StartCoroutine(FlashRed(renderer));
            }
        }

        IEnumerator FlashRed(Renderer renderer)
        {
            if (renderer.material.HasProperty("_Color"))
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                renderer.material.color = originalColor;
            }
        }

        void Die()
        {
            GameManager.Instance.OnEnemyKilled(goldReward);

            // 死亡效果
            GameObject deathEffect = Instantiate(Resources.Load<GameObject>("Prefabs/EnemyDeathEffect"), transform.position, Quaternion.identity);
            Destroy(deathEffect, 2f);

            Destroy(gameObject);
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }
    }
}
