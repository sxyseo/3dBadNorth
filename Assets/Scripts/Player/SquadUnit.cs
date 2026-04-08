using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// 玩家控制的小队单位 - 支持RTS风格选择和移动
    /// </summary>
    public class SquadUnit : MonoBehaviour
    {
        [Header("属性")]
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

        private UnityEngine.AI.NavMeshAgent navAgent;
        private Animator animator;
        private HealthBar healthBar;
        private Transform targetEnemy;
        private float lastAttackTime;

        void Start()
        {
            currentHealth = maxHealth;
            navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = moveSpeed;

            animator = GetComponent<Animator>();
            healthBar = GetComponentInChildren<HealthBar>();

            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHealth);
            }

            // 创建选择指示器
            CreateSelectionIndicator();
        }

        void CreateSelectionIndicator()
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "SelectionIndicator";
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = new Vector3(0, 0.1f, 0);
            indicator.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);
            indicator.GetComponent<MeshRenderer>().material.color = Color.yellow;
            indicator.SetActive(false);

            Destroy(indicator.GetComponent<Collider>());
        }

        void Update()
        {
            UpdateSelectionIndicator();

            if (isMoving && navAgent.remainingDistance < 0.5f)
            {
                isMoving = false;
                if (animator != null) animator.SetBool("IsMoving", false);
            }

            // 自动攻击范围内的敌人
            if (!isMoving && !isAttacking)
            {
                FindAndAttackEnemy();
            }
        }

        void UpdateSelectionIndicator()
        {
            Transform indicator = transform.Find("SelectionIndicator");
            if (indicator != null)
            {
                indicator.gameObject.SetActive(isSelected);
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

        IEnumerator AttackEnemy(Enemy enemy)
        {
            isAttacking = true;

            while (enemy != null && enemy.IsAlive())
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance <= attackRange)
                {
                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        lastAttackTime = Time.time;
                        enemy.TakeDamage(attackDamage);

                        if (animator != null)
                        {
                            animator.SetTrigger("Attack");
                        }
                    }
                }
                else
                {
                    // 追击敌人
                    navAgent.SetDestination(enemy.transform.position);
                }

                yield return new WaitForSeconds(0.2f);
            }

            isAttacking = false;
            targetEnemy = null;
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
            GameManager.Instance.OnUnitKilled(gameObject);

            // 死亡效果
            GameObject deathEffect = Instantiate(Resources.Load<GameObject>("Prefabs/DeathEffect"), transform.position, Quaternion.identity);
            Destroy(deathEffect, 2f);

            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
