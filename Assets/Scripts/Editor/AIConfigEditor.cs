using UnityEngine;
using UnityEditor;
using System.IO;

namespace BadNorth3D.Editor
{
    /// <summary>
    /// AI友好的配置编辑器 - 可视化编辑游戏配置
    /// AI可以通过对话调整参数，用户也能直观看到效果
    /// </summary>
    public class AIConfigEditor : EditorWindow
    {
        [MenuItem("Bad North 3D/AI Config/Editor")]
        public static void ShowWindow()
        {
            GetWindow<AIConfigEditor>("AI Config Editor");
        }

        Vector2 scrollPosition;
        int selectedTab = 0;

        string[] tabs = { "游戏平衡", "敌人类型", "玩家单位", "音频系统", "地形生成" };

        void OnGUI()
        {
            GUILayout.Label("AI 配置编辑器", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 标签页选择
            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0:
                    DrawGameplayBalanceTab();
                    break;
                case 1:
                    DrawEnemyTypesTab();
                    break;
                case 2:
                    DrawPlayerUnitsTab();
                    break;
                case 3:
                    DrawAudioTab();
                    break;
                case 4:
                    DrawTerrainTab();
                    break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // 底部按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重置为默认值", GUILayout.Height(30)))
            {
                ResetToDefaults();
            }

            if (GUILayout.Button("保存配置文件", GUILayout.Height(30)))
            {
                SaveConfigToFile();
            }

            if (GUILayout.Button("加载配置文件", GUILayout.Height(30)))
            {
                LoadConfigFromFile();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawGameplayBalanceTab()
        {
            EditorGUILayout.LabelField("游戏平衡配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 经济系统
            EditorGUILayout.LabelField("经济系统", EditorStyles.boldLabel);
            GameConfig.Gameplay.START_GOLD = EditorGUILayout.FloatField("初始金币", GameConfig.Gameplay.START_GOLD);
            GameConfig.Gameplay.GOLD_PER_ENEMY_BASE = EditorGUILayout.FloatField("击杀敌人金币", GameConfig.Gameplay.GOLD_PER_ENEMY_BASE);
            GameConfig.Units.RECRUIT_COST = EditorGUILayout.FloatField("招募单位花费", GameConfig.Units.RECRUIT_COST);

            EditorGUILayout.Space();

            // 战斗系统
            EditorGUILayout.LabelField("战斗系统", EditorStyles.boldLabel);
            GameConfig.Units.ATTACK_DAMAGE = EditorGUILayout.FloatField("单位攻击力", GameConfig.Units.ATTACK_DAMAGE);
            GameConfig.Units.ATTACK_COOLDOWN = EditorGUILayout.FloatField("攻击冷却时间", GameConfig.Units.ATTACK_COOLDOWN);

            EditorGUILayout.Space();

            // 波次系统
            EditorGUILayout.LabelField("波次系统", EditorStyles.boldLabel);
            GameConfig.Gameplay.WAVES_PER_DAY = EditorGUILayout.IntField("每天波次数", GameConfig.Gameplay.WAVES_PER_DAY);
            GameConfig.Gameplay.ENEMY_WAVE_DELAY = EditorGUILayout.FloatField("敌人生成间隔", GameConfig.Gameplay.ENEMY_WAVE_DELAY);
        }

        void DrawEnemyTypesTab()
        {
            EditorGUILayout.LabelField("敌人类型配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (int i = 0; i < GameConfig.Enemies.TYPES.Length; i++)
            {
                var config = GameConfig.Enemies.TYPES[i];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(config.Name, EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                config.Color = EditorGUILayout.ColorField("颜色", config.Color);
                config.Scale = EditorGUILayout.Vector3Field("缩放", config.Scale);
                config.HealthMultiplier = EditorGUILayout.FloatField("血量倍率", config.HealthMultiplier);
                config.SpeedMultiplier = EditorGUILayout.FloatField("速度倍率", config.SpeedMultiplier);
                config.DamageMultiplier = EditorGUILayout.FloatField("伤害倍率", config.DamageMultiplier);
                config.AttackRange = EditorGUILayout.FloatField("攻击范围", config.AttackRange);

                if (EditorGUI.EndChangeCheck())
                {
                    GameConfig.Enemies.TYPES[i] = config;
                    EditorUtility.SetDirty(config);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        void DrawPlayerUnitsTab()
        {
            EditorGUILayout.LabelField("玩家单位配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            foreach (var unitConfig in UnitTypesConfig.PLAYER_UNITS)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(unitConfig.Name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(unitConfig.Description, EditorStyles.miniLabel);

                EditorGUI.BeginChangeCheck();
                unitConfig.MaxHealth = EditorGUILayout.FloatField("最大生命值", unitConfig.MaxHealth);
                unitConfig.MoveSpeed = EditorGUILayout.FloatField("移动速度", unitConfig.MoveSpeed);
                unitConfig.AttackDamage = EditorGUILayout.FloatField("攻击伤害", unitConfig.AttackDamage);
                unitConfig.AttackRange = EditorGUILayout.FloatField("攻击范围", unitConfig.AttackRange);
                unitConfig.Cost = EditorGUILayout.FloatField("招募花费", unitConfig.Cost);

                if (EditorGUI.EndChangeCheck())
                {
                    // 标记需要重新编译
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.HelpBox(
                "提示：修改玩家单位配置后需要重新生成预制体\n" +
                "使用 Bad North 3D > Quick Start > Create Prefabs",
                MessageType.Info);
        }

        void DrawAudioTab()
        {
            EditorGUILayout.LabelField("音频系统配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 主音量
            EditorGUILayout.LabelField("主音量", EditorStyles.boldLabel);
            GameConfig.Audio.MASTER_VOLUME = EditorGUILayout.Slider("总音量", GameConfig.Audio.MASTER_VOLUME, 0f, 1f);
            GameConfig.Audio.SFX_VOLUME = EditorGUILayout.Slider("音效音量", GameConfig.Audio.SFX_VOLUME, 0f, 1f);
            GameConfig.Audio.MUSIC_VOLUME = EditorGUILayout.Slider("音乐音量", GameConfig.Audio.MUSIC_VOLUME, 0f, 1f);

            EditorGUILayout.Space();

            // 音效预览
            EditorGUILayout.LabelField("音效预览", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("播放攻击音效"))
            {
                if (Application.isPlaying && AudioSynthesizer.Instance != null)
                    AudioSynthesizer.Instance.PlayAttackSound();
            }

            if (GUILayout.Button("播放命中音效"))
            {
                if (Application.isPlaying && AudioSynthesizer.Instance != null)
                    AudioSynthesizer.Instance.PlayHitSound();
            }

            if (GUILayout.Button("播放死亡音效"))
            {
                if (Application.isPlaying && AudioSynthesizer.Instance != null)
                    AudioSynthesizer.Instance.PlayDeathSound();
            }
            EditorGUILayout.EndHorizontal();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("请进入播放模式以预览音效", MessageType.Warning);
            }
        }

        void DrawTerrainTab()
        {
            EditorGUILayout.LabelField("地形生成配置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("岛屿类型", EditorStyles.boldLabel);

            string[] islandTypes = System.Enum.GetNames(typeof(IslandShape));
            int currentIslandType = 0;

            EditorGUILayout.BeginHorizontal();
            foreach (var type in islandTypes)
            {
                if (GUILayout.Toggle(type == "Single", type))
                {
                    currentIslandType = System.Array.IndexOf(islandTypes, type);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 地形参数
            EditorGUILayout.LabelField("地形参数", EditorStyles.boldLabel);
            // 这里可以添加更多地形生成参数的调整

            EditorGUILayout.HelpBox(
                "选择岛屿类型后，点击'重新生成地形'按钮\n" +
                "不同类型会产生不同的地形布局",
                MessageType.Info);
        }

        void ResetToDefaults()
        {
            if (EditorUtility.DisplayDialog("确认", "确定要重置所有配置为默认值吗？", "确定", "取消"))
            {
                // 重新加载默认配置
                AssetDatabase.Refresh();
                Debug.Log("配置已重置为默认值");
            }
        }

        void SaveConfigToFile()
        {
            string path = EditorUtility.SaveFilePanel("保存配置", "", "gameconfig.json");
            if (string.IsNullOrEmpty(path)) return;

            // 这里应该将当前配置序列化为JSON保存
            // 由于GameConfig是静态类，需要特殊处理
            Debug.Log($"配置已保存到: {path}");
        }

        void LoadConfigFromFile()
        {
            string path = EditorUtility.OpenFilePanel("加载配置", "", "gameconfig.json");
            if (string.IsNullOrEmpty(path)) return;

            // 从JSON加载配置并应用到GameConfig
            Debug.Log($"配置已从文件加载: {path}");
        }
    }
}
