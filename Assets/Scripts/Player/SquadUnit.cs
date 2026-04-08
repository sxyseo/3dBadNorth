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
                    navAgent.isStopped = true;

                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        lastAttackTime = Time.time;

                        // 播放攻击音效
                        if (AudioSynthesizer.Instance != null)
                        {
                            AudioSynthesizer.Instance.PlayAttackSound();
                        }

                        // 攻击动画（简单的旋转和缩放）
                        yield return StartCoroutine(PlayAttackAnimation());

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
                    navAgent.isStopped = false;
                    navAgent.SetDestination(enemy.transform.position);
                }

                yield return new WaitForSeconds(0.2f);
            }

            isAttacking = false;
            targetEnemy = null;
            navAgent.isStopped = false;
        }

        IEnumerator PlayAttackAnimation()
        {
            // 简单的攻击动画 - 向前突刺
            Vector3 originalPos = transform.position;
            Vector3 attackDirection = (targetEnemy != null) ?
                (targetEnemy.position - transform.position).normalized : transform.forward;

            float attackDuration = 0.15f;
            float elapsed = 0f;

            while (elapsed < attackDuration)
            {
                float t = elapsed / attackDuration;
                float thrust = Mathf.Sin(t * Mathf.PI) * 0.5f; // 前突0.5单位

                transform.position = originalPos + attackDirection * thrust;
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = originalPos;
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
            // 播放死亡音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayDeathSound();
            }

            GameManager.Instance.OnUnitKilled(gameObject);

            // 创建程序化死亡效果
            StartCoroutine(PlayDeathEffect());

            Destroy(gameObject, 0.5f);
        }

        IEnumerator PlayDeathEffect()
        {
            // 简单的死亡动画 - 倒下
            float elapsed = 0f;
            float deathDuration = 0.4f;
            Vector3 originalScale = transform.localScale;

            while (elapsed < deathDuration)
            {
                float t = elapsed / deathDuration;
                transform.localScale = originalScale * (1f - t * 0.5f); // 缩小到50%
                transform.rotation = Quaternion.Euler(t * 90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z); // 向前倒
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
