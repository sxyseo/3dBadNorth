using UnityEngine;
using System.Collections;

namespace BadNorth3D.Weather
{
    /// <summary>
    /// 天气系统 - 动态天气效果影响游戏性和视觉效果
    /// AI可以调整天气参数和影响程度
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        public static WeatherSystem Instance { get; private set; }

        [Header("天气设置")]
        public WeatherType currentWeather = WeatherType.Sunny;
        public float weatherChangeInterval = 300f; // 5分钟换一次天气
        public bool enableDynamicWeather = true;

        [Header("天气效果设置")]
        public float rainIntensity = 0.5f;
        public float fogDensity = 0.1f;
        public float windStrength = 0.3f;

        [Header("游戏影响")]
        public float movementModifier = 1f;
        public float visibilityModifier = 1f;
        public float accuracyModifier = 1f;

        // 天气对象
        private GameObject rainSystem;
        private GameObject fogSystem;
        private GameObject cloudSystem;

        // 计时器
        private float weatherTimer;

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
            CreateWeatherEffects();
            SetWeather(currentWeather);

            if (enableDynamicWeather)
            {
                weatherTimer = weatherChangeInterval;
            }
        }

        void Update()
        {
            if (enableDynamicWeather)
            {
                weatherTimer -= Time.deltaTime;
                if (weatherTimer <= 0f)
                {
                    ChangeToRandomWeather();
                    weatherTimer = weatherChangeInterval;
                }
            }
        }

        /// <summary>
        /// 创建天气效果对象
        /// </summary>
        void CreateWeatherEffects()
        {
            // 雨水系统
            rainSystem = new GameObject("RainSystem");
            rainSystem.transform.SetParent(transform);

            // 雾气系统
            fogSystem = new GameObject("FogSystem");
            fogSystem.transform.SetParent(transform);

            // 云层系统
            cloudSystem = new GameObject("CloudSystem");
            cloudSystem.transform.SetParent(transform);

            // 初始化RenderSettings
            RenderSettings.fog = false;
            RenderSettings.fogMode = FogMode.Exponential;
        }

        /// <summary>
        /// 设置天气
        /// </summary>
        public void SetWeather(WeatherType weatherType)
        {
            currentWeather = weatherType;

            switch (weatherType)
            {
                case WeatherType.Sunny:
                    ApplySunnyWeather();
                    break;
                case WeatherType.Rain:
                    ApplyRainWeather();
                    break;
                case WeatherType.Fog:
                    ApplyFogWeather();
                    break;
                case WeatherType.Storm:
                    ApplyStormWeather();
                    break;
                case WeatherType.Snow:
                    ApplySnowWeather();
                    break;
            }

            Debug.Log($"Weather changed to: {weatherType}");
        }

        /// <summary>
        /// 切换到随机天气
        /// </summary>
        public void ChangeToRandomWeather()
        {
            WeatherType[] weatherTypes = (WeatherType[])System.Enum.GetValues(typeof(WeatherType));
            WeatherType randomWeather = weatherTypes[Random.Range(0, weatherTypes.Length)];
            SetWeather(randomWeather);
        }

        /// <summary>
        /// 应用晴天天气
        /// </summary>
        void ApplySunnyWeather()
        {
            // 清除所有天气效果
            DisableAllWeatherEffects();

            // 游戏影响
            movementModifier = 1f;
            visibilityModifier = 1f;
            accuracyModifier = 1f;

            // 视觉效果
            RenderSettings.fog = false;
            RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f);
            RenderSettings.sunIntensity = 1f;
        }

        /// <summary>
        /// 应用雨天天气
        /// </summary>
        void ApplyRainWeather()
        {
            DisableAllWeatherEffects();

            // 游戏影响
            movementModifier = 0.9f; // 移动稍慢
            visibilityModifier = 0.8f; // 视野降低
            accuracyModifier = 0.85f; // 精度降低

            // 视觉效果
            EnableRainEffect();
            RenderSettings.fog = true;
            RenderSettings.fogDensity = fogDensity * 0.5f;
            RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.6f);
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.5f);
            RenderSettings.sunIntensity = 0.7f;
        }

        /// <summary>
        /// 应用雾天天气
        /// </summary>
        void ApplyFogWeather()
        {
            DisableAllWeatherEffects();

            // 游戏影响
            movementModifier = 0.95f;
            visibilityModifier = 0.5f; // 视野大幅降低
            accuracyModifier = 0.7f; // 精度降低

            // 视觉效果
            EnableFogEffect();
            RenderSettings.fog = true;
            RenderSettings.fogDensity = fogDensity * 2f;
            RenderSettings.fogColor = new Color(0.7f, 0.7f, 0.75f);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);
            RenderSettings.sunIntensity = 0.5f;
        }

        /// <summary>
        /// 应用暴风雨天气
        /// </summary>
        void ApplyStormWeather()
        {
            DisableAllWeatherEffects();

            // 游戏影响
            movementModifier = 0.7f; // 移动大幅降低
            visibilityModifier = 0.6f; // 视野降低
            accuracyModifier = 0.6f; // 精度大幅降低

            // 视觉效果
            EnableRainEffect();
            RenderSettings.fog = true;
            RenderSettings.fogDensity = fogDensity * 1.5f;
            RenderSettings.fogColor = new Color(0.3f, 0.3f, 0.4f);
            RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.35f);
            RenderSettings.sunIntensity = 0.3f;

            // 闪电效果
            StartCoroutine(StormLightningEffect());
        }

        /// <summary>
        /// 应用雪天天气
        /// </summary>
        void ApplySnowWeather()
        {
            DisableAllWeatherEffects();

            // 游戏影响
            movementModifier = 0.8f; // 移动变慢
            visibilityModifier = 0.7f; // 视野降低
            accuracyModifier = 0.9f; // 精度略微降低

            // 视觉效果
            EnableSnowEffect();
            RenderSettings.fog = true;
            RenderSettings.fogDensity = fogDensity * 0.8f;
            RenderSettings.fogColor = new Color(0.9f, 0.9f, 0.95f);
            RenderSettings.ambientLight = new Color(0.7f, 0.7f, 0.8f);
            RenderSettings.sunIntensity = 0.8f;
        }

        /// <summary>
        /// 启用雨水效果
        /// </summary>
        void EnableRainEffect()
        {
            if (rainSystem != null)
            {
                rainSystem.SetActive(true);
                // 这里可以添加粒子系统
            }
        }

        /// <summary>
        /// 启用雾气效果
        /// </summary>
        void EnableFogEffect()
        {
            if (fogSystem != null)
            {
                fogSystem.SetActive(true);
                // 这里可以添加体积雾效果
            }
        }

        /// <summary>
        /// 启用雪花效果
        /// </summary>
        void EnableSnowEffect()
        {
            if (rainSystem != null)
            {
                rainSystem.SetActive(true);
                // 复用雨水系统但改为雪花
            }
        }

        /// <summary>
        /// 禁用所有天气效果
        /// </summary>
        void DisableAllWeatherEffects()
        {
            if (rainSystem != null)
                rainSystem.SetActive(false);

            if (fogSystem != null)
                fogSystem.SetActive(false);

            if (cloudSystem != null)
                cloudSystem.SetActive(false);

            // 停止所有协程
            StopAllCoroutines();
        }

        /// <summary>
        /// 暴风雨闪电效果
        /// </summary>
        IEnumerator StormLightningEffect()
        {
            while (currentWeather == WeatherType.Storm)
            {
                // 随机闪电间隔
                float lightningInterval = Random.Range(5f, 15f);
                yield return new WaitForSeconds(lightningInterval);

                // 闪光效果
                RenderSettings.ambientLight = new Color(1f, 1f, 1f);
                yield return new WaitForSeconds(0.1f);
                RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.35f);

                // 雷声效果（如果AudioSynthesizer支持）
                if (AudioSynthesizer.Instance != null)
                {
                    // 这里可以播放雷声音效
                }
            }
        }

        /// <summary>
        /// 获取当前移动修正
        /// </summary>
        public float GetMovementModifier()
        {
            return movementModifier;
        }

        /// <summary>
        /// 获取当前视野修正
        /// </summary>
        public float GetVisibilityModifier()
        {
            return visibilityModifier;
        }

        /// <summary>
        /// 获取当前精度修正
        /// </summary>
        public float GetAccuracyModifier()
        {
            return accuracyModifier;
        }

        /// <summary>
        /// 设置天气参数
        /// </summary>
        public void SetWeatherParameters(float rain, float fog, float wind)
        {
            rainIntensity = Mathf.Clamp01(rain);
            fogDensity = Mathf.Clamp01(fog);
            windStrength = Mathf.Clamp01(wind);

            // 重新应用当前天气
            SetWeather(currentWeather);
        }

        /// <summary>
        /// 获取天气信息
        /// </summary>
        public WeatherInfo GetWeatherInfo()
        {
            return new WeatherInfo
            {
                CurrentWeather = currentWeather,
                MovementModifier = movementModifier,
                VisibilityModifier = visibilityModifier,
                AccuracyModifier = accuracyModifier,
                RainIntensity = rainIntensity,
                FogDensity = fogDensity,
                WindStrength = windStrength
            };
        }

        void OnDrawGizmosSelected()
        {
            // 显示天气范围
            Gizmos.color = GetWeatherColor(currentWeather);
            Gizmos.DrawWireSphere(transform.position, 10f);
        }

        Color GetWeatherColor(WeatherType weather)
        {
            return weather switch
            {
                WeatherType.Sunny => Color.yellow,
                WeatherType.Rain => Color.blue,
                WeatherType.Fog => Color.gray,
                WeatherType.Storm => new Color(0.3f, 0.3f, 0.4f),
                WeatherType.Snow => Color.white,
                _ => Color.white
            };
        }
    }

    // ==================== 天气数据结构 ====================

    /// <summary>
    /// 天气类型
    /// </summary>
    public enum WeatherType
    {
        Sunny,   // 晴天
        Rain,    // 雨天
        Fog,     // 雾天
        Storm,   // 暴风雨
        Snow     // 雪天
    }

    /// <summary>
    /// 天气信息
    /// </summary>
    [System.Serializable]
    public struct WeatherInfo
    {
        public WeatherType CurrentWeather;
        public float MovementModifier;
        public float VisibilityModifier;
        public float AccuracyModifier;
        public float RainIntensity;
        public float FogDensity;
        public float WindStrength;
    }
}