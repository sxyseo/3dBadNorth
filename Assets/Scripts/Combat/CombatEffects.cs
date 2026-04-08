using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// 战斗特效管理器 - 增强版技能和战斗视觉反馈
    /// AI可以调整特效参数来改变视觉风格
    /// </summary>
    public class CombatEffects : MonoBehaviour
    {
        public static CombatEffects Instance { get; private set; }

        [Header("基础特效")]
        public ParticleSystem swordClashEffect;
        public ParticleSystem bloodEffect;
        public ParticleSystem deathEffect;

        [Header("技能特效")]
        public float skillEffectDuration = 1f;
        public float particleSize = 1f;

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

        public void PlaySwordClash(Vector3 position)
        {
            if (swordClashEffect != null)
            {
                ParticleSystem effect = Instantiate(swordClashEffect, position, Quaternion.identity);
                Destroy(effect.gameObject, effect.main.duration);
            }
        }

        public void PlayBloodEffect(Vector3 position)
        {
            if (bloodEffect != null)
            {
                ParticleSystem effect = Instantiate(bloodEffect, position, Quaternion.identity);
                Destroy(effect.gameObject, effect.main.duration);
            }
        }

        public void PlayDeathEffect(Vector3 position)
        {
            if (deathEffect != null)
            {
                ParticleSystem effect = Instantiate(deathEffect, position, Quaternion.identity);
                Destroy(effect.gameObject, effect.main.duration);
            }
        }

        public void ShowDamageNumber(float damage, Vector3 position)
        {
            GameObject damageText = new GameObject("DamageText");
            damageText.transform.position = position + Vector3.up * 2f;

            TextMesh textMesh = damageText.AddComponent<TextMesh>();
            textMesh.text = Mathf.CeilToInt(damage).ToString();
            textMesh.color = Color.red;
            textMesh.fontSize = 24;
            textMesh.anchor = TextAnchor.MiddleCenter;

            damageText.transform.Rotate(90, 0, 0);

            StartCoroutine(AnimateDamageText(damageText));
        }

        IEnumerator AnimateDamageText(GameObject damageText)
        {
            Vector3 startPos = damageText.transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f;

            float duration = 1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                damageText.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);

                TextMesh textMesh = damageText.GetComponent<TextMesh>();
                if (textMesh != null)
                {
                    Color color = textMesh.color;
                    color.a = 1f - (elapsed / duration);
                    textMesh.color = color;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(damageText);
        }

        // ==================== 技能特效系统 ====================

        /// <summary>
        /// 播放技能特效 - 根据技能类型显示不同的视觉效果
        /// </summary>
        public void PlaySkillEffect(Vector3 position, SkillType skillType)
        {
            Color skillColor = GetSkillColor(skillType);

            switch (skillType)
            {
                case SkillType.ShieldBash:
                    PlayShieldBashEffect(position, skillColor);
                    break;
                case SkillType.MultiShot:
                    PlayMultiShotEffect(position, skillColor);
                    break;
                case SkillType.GuardAura:
                    StartCoroutine(PlayGuardAuraEffect(position, skillColor));
                    break;
                case SkillType.Berserk:
                    PlayBerserkEffect(position, skillColor);
                    break;
            }
        }

        /// <summary>
        /// 盾击特效 - 范围冲击波
        /// </summary>
        void PlayShieldBashEffect(Vector3 position, Color color)
        {
            StartCoroutine(AnimateAreaEffect(position, 3f, color, 0.6f));
        }

        /// <summary>
        /// 连射特效 - 多个投射物
        /// </summary>
        void PlayMultiShotEffect(Vector3 position, Color color)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 direction = Quaternion.Euler(0, i * 30f, 0) * Vector3.forward;
                Vector3 targetPos = position + direction * 8f;
                StartCoroutine(AnimateProjectileTrail(position, targetPos, color, i * 0.1f));
            }
        }

        /// <summary>
        /// 守护光环特效 - 持续范围效果
        /// </summary>
        IEnumerator PlayGuardAuraEffect(Vector3 center, Color color)
        {
            float duration = 5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // 创建脉冲光环
                float pulse = Mathf.Sin(elapsed * 3f) * 0.2f + 1f;
                StartCoroutine(AnimateAreaEffect(center, 5f * pulse, color, 0.3f));

                elapsed += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// 狂暴特效 - 爆发火焰
        /// </summary>
        void PlayBerserkEffect(Vector3 position, Color color)
        {
            StartCoroutine(AnimateBerserkEffect(position, color));
        }

        /// <summary>
        /// 范围特效动画
        /// </summary>
        IEnumerator AnimateAreaEffect(Vector3 center, float radius, Color color, float duration)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "AreaEffect";
            ring.transform.position = center;
            ring.transform.localScale = new Vector3(radius * 2f, 0.1f, radius * 2f);

            // 设置半透明材质
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(color.r, color.g, color.b, 0.3f);
            ring.GetComponent<Renderer>().material = material;
            Destroy(ring.GetComponent<CapsuleCollider>());

            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float alpha = 0.3f * (1f - t);
                material.color = new Color(color.r, color.g, color.b, alpha);

                float expansion = 1f + t * 0.3f;
                ring.transform.localScale = new Vector3(radius * 2f * expansion, 0.1f, radius * 2f * expansion);

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(ring);
        }

        /// <summary>
        /// 投射物轨迹动画
        /// </summary>
        IEnumerator AnimateProjectileTrail(Vector3 start, Vector3 end, Color color, float delay = 0f)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            trail.name = "ProjectileTrail";
            trail.transform.position = start;
            trail.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            trail.GetComponent<Renderer>().material = material;
            Destroy(trail.GetComponent<SphereCollider>());

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                trail.transform.position = Vector3.Lerp(start, end, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(trail);

            // 播放命中特效
            ShowDamageNumber(15f, end); // 显示伤害数字
        }

        /// <summary>
        /// 狂暴特效动画
        /// </summary>
        IEnumerator AnimateBerserkEffect(Vector3 position, Color color)
        {
            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "BerserkEffect";
            effect.transform.position = position;
            effect.transform.localScale = Vector3.one * 0.5f;

            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(color.r, color.g, color.b, 0.5f);
            effect.GetComponent<Renderer>().material = material;
            Destroy(effect.GetComponent<SphereCollider>());

            float duration = 0.8f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = 0.5f + Mathf.Sin(t * Mathf.PI) * 1.5f;
                float alpha = 0.5f * (1f - t);
                effect.transform.localScale = Vector3.one * scale;
                material.color = new Color(color.r, color.g, color.b, alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(effect);
        }

        /// <summary>
        /// 获取技能颜色
        /// </summary>
        Color GetSkillColor(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.ShieldBash => new Color(0.8f, 0.6f, 0.2f), // 金色
                SkillType.MultiShot => new Color(0.2f, 0.8f, 0.4f), // 绿色
                SkillType.GuardAura => new Color(0.4f, 0.6f, 0.9f), // 蓝色
                SkillType.Berserk => new Color(0.9f, 0.2f, 0.2f),   // 红色
                _ => Color.white
            };
        }
    }

    /// <summary>
    /// 技能类型枚举
    /// </summary>
    public enum SkillType
    {
        ShieldBash,   // 盾击
        MultiShot,    // 连射
        GuardAura,    // 守护光环
        Berserk       // 狂暴
    }
}
