using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace BadNorth3D.Editor
{
    /// <summary>
    /// 一键场景设置 - 自动创建完整的可测试游戏场景
    /// </summary>
    public class AutoSceneSetup : EditorWindow
    {
        [MenuItem("Bad North 3D/Auto Setup/Create Test Scene")]
        public static void ShowWindow()
        {
            GetWindow<AutoSceneSetup>("Auto Scene Setup");
        }

        void OnGUI()
        {
            GUILayout.Label("Bad North 3D - 一键测试场景设置", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "这个工具将自动创建完整的测试场景，包括：\n" +
                "• 地面和光照\n" +
                "• 管理器和音频系统\n" +
                "• 敌人生成点\n" +
                "• 完整的UI界面\n" +
                "• 自动配置所有引用",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("创建完整测试场景", GUILayout.Height(40)))
            {
                CreateCompleteTestScene();
            }

            if (GUILayout.Button("创建UI界面", GUILayout.Height(30)))
            {
                CreateUI();
            }

            if (GUILayout.Button("添加音频系统", GUILayout.Height(30)))
            {
                AddAudioSystem();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "提示：创建场景后记得烘焙NavMesh！\n" +
                "Window > AI > Navigation > Bake",
                MessageType.Warning);
        }

        void CreateCompleteTestScene()
        {
            // 清理当前场景
            if (!EditorUtility.DisplayDialog("确认",
                "这将清理当前场景并创建新的测试场景。继续？", "确定", "取消"))
            {
                return;
            }

            // 删除现有对象（保留Main Camera和Directional Light）
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name != "Main Camera" && obj.name != "Directional Light")
                {
                    DestroyImmediate(obj);
                }
            }

            // 创建场景
            CreateGround();
            SetupCamera();
            SetupLighting();
            CreateManagers();
            CreateSpawnPoints();
            CreateUI();
            AddAudioSystem();

            Debug.Log("✅ 测试场景创建完成！记得烘焙NavMesh。");

            // 自动打开Navigation窗口
            EditorApplication.ExecuteMenuItem("Window/AI/Navigation");
        }

        void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10, 1, 10);

            // 设置为Navigation Static
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);

            // 创建材质
            Material groundMaterial = new Material(Shader.Find("Standard"));
            groundMaterial.color = new Color(0.3f, 0.5f, 0.3f);
            ground.GetComponent<Renderer>().material = groundMaterial;
        }

        void SetupCamera()
        {
            GameObject camera = GameObject.Find("Main Camera");
            if (camera == null)
            {
                camera = new GameObject("Main Camera");
                camera.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = new Vector3(0, 15, -20);
            camera.transform.rotation = Quaternion.Euler(60, 0, 0);

            // 添加CameraController
            CameraController camController = camera.GetComponent<CameraController>();
            if (camController == null)
            {
                camera.AddComponent<CameraController>();
            }

            // 添加AudioListener
            AudioListener listener = camera.GetComponent<AudioListener>();
            if (listener == null)
            {
                camera.AddComponent<AudioListener>();
            }
        }

        void SetupLighting()
        {
            GameObject light = GameObject.Find("Directional Light");
            if (light == null)
            {
                light = new GameObject("Directional Light");
                light.AddComponent<Light>();
                light.GetComponent<Light>().type = LightType.Directional;
            }

            light.transform.position = new Vector3(0, 10, 0);
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        void CreateManagers()
        {
            // GameManager
            GameObject gameManager = new GameObject("GameManager");
            GameManager gm = gameManager.AddComponent<GameManager>();
            gameManager.tag = "GameManager";

            // UnitSelectionManager
            GameObject unitManager = new GameObject("UnitSelectionManager");
            unitManager.AddComponent<UnitSelectionManager>();

            // UIManager
            GameObject uiManager = new GameObject("UIManager");
            uiManager.AddComponent<UIManager>();

            // WaveManager
            GameObject waveManager = new GameObject("WaveManager");
            waveManager.AddComponent<WaveManager>();
        }

        void CreateSpawnPoints()
        {
            GameObject spawnPoints = new GameObject("EnemySpawnPoints");

            Vector3[] positions = {
                new Vector3(20, 0, 20),
                new Vector3(-20, 0, 20),
                new Vector3(20, 0, -20),
                new Vector3(-20, 0, -20)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject sp = new GameObject($"SpawnPoint_{i + 1}");
                sp.transform.SetParent(spawnPoints.transform);
                sp.transform.position = positions[i];
            }

            // 设置GameManager引用
            GameObject gm = GameObject.Find("GameManager");
            if (gm != null)
            {
                GameManager gameManager = gm.GetComponent<GameManager>();
                if (gameManager != null)
                {
                    // 找到预制体
                    string enemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
                    GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);

                    if (enemyPrefab != null)
                    {
                        gameManager.enemyPrefab = enemyPrefab;
                    }

                    // 设置生成点
                    gameManager.enemySpawnPoints = new Transform[4];
                    for (int i = 0; i < 4; i++)
                    {
                        gameManager.enemySpawnPoints[i] = spawnPoints.transform.GetChild(i);
                    }
                }
            }
        }

        void CreateUI()
        {
            // 创建Canvas
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("MainCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();

                // 添加EventSystem
                GameObject eventSystem = GameObject.Find("EventSystem");
                if (eventSystem == null)
                {
                    eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }

            // 创建资源面板
            CreateResourcePanel(canvas.transform);

            // 创建招募面板
            CreateRecruitPanel(canvas.transform);

            // 创建消息面板
            CreateMessagePanel(canvas.transform);

            // 设置UIManager引用
            SetupUIManagerReferences(canvas.transform);
        }

        void CreateResourcePanel(Transform parent)
        {
            GameObject panel = new GameObject("ResourcePanel");
            panel.transform.SetParent(parent);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(300, 150);

            // 背景
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            // 金币文本
            CreateTextMeshPro(panel.transform, "GoldText", "金币: 100", new Vector2(10, -10), 24);
            CreateTextMeshPro(panel.transform, "DayText", "第 1 天", new Vector2(10, -50), 20);
            CreateTextMeshPro(panel.transform, "WaveText", "波次: 0/5", new Vector2(10, -90), 20);
            CreateTextMeshPro(panel.transform, "SelectedCountText", "选中: 0", new Vector2(10, -130), 18);
        }

        void CreateRecruitPanel(Transform parent)
        {
            GameObject panel = new GameObject("RecruitPanel");
            panel.transform.SetParent(parent);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0);
            rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 20);
            rect.sizeDelta = new Vector2(200, 50);

            // 招募按钮
            GameObject buttonGO = new GameObject("RecruitButton");
            buttonGO.transform.SetParent(panel.transform);
            RectTransform btnRect = buttonGO.AddComponent<RectTransform>();
            btnRect.anchorMin = Vector2.zero;
            btnRect.anchorMax = Vector2.one;
            btnRect.sizeDelta = Vector2.zero;

            Image btnImage = buttonGO.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.6f, 0.2f);

            Button btn = buttonGO.AddComponent<Button>();

            // 按钮文本
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "招募单位 (20金币)";
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        void CreateMessagePanel(Transform parent)
        {
            GameObject panel = new GameObject("MessagePanel");
            panel.transform.SetParent(parent);
            panel.SetActive(false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400, 100);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            GameObject textGO = new GameObject("MessageText");
            textGO.transform.SetParent(panel.transform);
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = "消息内容";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-20, -20);
            textRect.anchoredPosition = Vector2.zero;
        }

        GameObject CreateTextMeshPro(Transform parent, string name, string text, Vector2 position, float fontSize)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent);

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;

            RectTransform rect = textGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(280, 30);

            return textGO;
        }

        void SetupUIManagerReferences(Transform canvas)
        {
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager == null) return;

            // 设置资源UI引用
            Transform resourcePanel = canvas.Find("ResourcePanel");
            if (resourcePanel != null)
            {
                uiManager.goldText = resourcePanel.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
                uiManager.dayText = resourcePanel.Find("DayText")?.GetComponent<TextMeshProUGUI>();
                uiManager.waveText = resourcePanel.Find("WaveText")?.GetComponent<TextMeshProUGUI>();
                uiManager.selectedCountText = resourcePanel.Find("SelectedCountText")?.GetComponent<TextMeshProUGUI>();
            }

            // 设置招募按钮
            Transform recruitPanel = canvas.Find("RecruitPanel");
            if (recruitPanel != null)
            {
                uiManager.recruitButton = recruitPanel.Find("RecruitButton")?.gameObject;
            }

            // 设置消息面板
            Transform messagePanel = canvas.Find("MessagePanel");
            if (messagePanel != null)
            {
                uiManager.messagePanel = messagePanel.gameObject;
                uiManager.messageText = messagePanel.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
            }
        }

        void AddAudioSystem()
        {
            // 查找是否已有AudioSynthesizer
            AudioSynthesizer audio = Object.FindFirstObjectByType<AudioSynthesizer>();
            if (audio != null)
            {
                Debug.Log("AudioSynthesizer已存在");
                return;
            }

            // 创建AudioSynthesizer
            GameObject audioGO = new GameObject("AudioSynthesizer");
            audioGO.AddComponent<AudioSynthesizer>();

            Debug.Log("✅ 音频系统已添加");
        }
    }
}
