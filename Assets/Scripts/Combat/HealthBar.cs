using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 简单的3D血条显示
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        public Gradient healthGradient;
        public float healthBarWidth = 1f;
        public float healthBarHeight = 0.1f;
        public float offset = 2f;

        private float maxHealth;
        private float currentHealth;
        private MeshRenderer meshRenderer;
        private Material material;

        void Start()
        {
            // 创建血条
            GameObject healthBarObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            healthBarObj.name = "HealthBar";
            healthBarObj.transform.SetParent(transform);
            healthBarObj.transform.localPosition = new Vector3(0, offset, 0);
            healthBarObj.transform.localScale = new Vector3(healthBarWidth, healthBarHeight, 0.1f);
            healthBarObj.transform.rotation = Quaternion.Euler(90, 0, 0);

            meshRenderer = healthBarObj.GetComponent<MeshRenderer>();
            material = new Material(Shader.Find("Standard"));
            material.color = Color.green;
            meshRenderer.material = material;

            Destroy(healthBarObj.GetComponent<Collider>());
        }

        void Update()
        {
            // 让血条始终朝向相机
            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform);
            }
        }

        public void SetMaxHealth(float health)
        {
            maxHealth = health;
            currentHealth = health;
            UpdateHealthBar();
        }

        public void SetHealth(float health)
        {
            currentHealth = Mathf.Max(0, health);
            UpdateHealthBar();
        }

        void UpdateHealthBar()
        {
            if (material != null)
            {
                float healthPercent = currentHealth / maxHealth;
                material.color = healthGradient.Evaluate(healthPercent);

                // 缩放血条宽度
                Transform healthBar = transform.Find("HealthBar");
                if (healthBar != null)
                {
                    Vector3 scale = healthBar.localScale;
                    scale.x = healthBarWidth * healthPercent;
                    healthBar.localScale = scale;
                }
            }
        }
    }
}
