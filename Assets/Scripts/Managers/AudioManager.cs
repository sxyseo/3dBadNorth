using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 简单的音频管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("音效")]
        public AudioClip swordHitSound;
        public AudioClip unitDeathSound;
        public AudioClip enemyDeathSound;
        public AudioClip waveCompleteSound;
        public AudioClip buttonClickSound;

        private AudioSource audioSource;

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

        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlaySwordHit()
        {
            PlaySound(swordHitSound);
        }

        public void PlayUnitDeath()
        {
            PlaySound(unitDeathSound);
        }

        public void PlayEnemyDeath()
        {
            PlaySound(enemyDeathSound);
        }

        public void PlayWaveComplete()
        {
            PlaySound(waveCompleteSound);
        }

        public void PlayButtonClick()
        {
            PlaySound(buttonClickSound);
        }

        void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
