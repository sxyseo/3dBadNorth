using UnityEngine;
using System.Collections;

namespace BadNorth3D
{
    /// <summary>
    /// 战斗特效管理器
    /// </summary>
    public class CombatEffects : MonoBehaviour
    {
        public static CombatEffects Instance { get; private set; }

        [Header("特效")]
        public ParticleSystem swordClashEffect;
        public ParticleSystem bloodEffect;
        public ParticleSystem deathEffect;

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
    }
}
