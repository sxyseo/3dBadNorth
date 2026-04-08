using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace BadNorth3D.Editor
{
    /// <summary>
    /// 一键创建所有必需的Unity资源
    /// </summary>
    public class QuickStartSetup : EditorWindow
    {
        [MenuItem("Bad North 3D/Quick Start/Create All Resources")]
        public static void CreateAllResources()
        {
            Debug.Log("=== 开始创建所有Unity资源 ===");

            CreateFolders();
            CreateMainMenuScene();
            CreateGameScene();
            CreatePrefabs();
            CreateMaterials();

            Debug.Log("=== 资源创建完成！===");
            Debug.Log("下一步：在Unity中打开 GameScene 并点击Play");
        }

        static void CreateFolders()
        {
            string[] folders = new string[]
            {
                "Assets/Scenes",
                "Assets/Prefabs",
                "Assets/Prefabs/Units",
                "Assets/Prefabs/Enemies",
                "Assets/Prefabs/Effects",
                "Assets/Materials",
                "Assets/Textures",
                "Assets/Audio",
                "Assets/Resources"
            };

            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    Debug.Log($"创建文件夹: {folder}");
                }
            }

            AssetDatabase.Refresh();
        }

        static void CreateMainMenuScene()
        {
            string scenePath = "Assets/Scenes/MainMenu.unity";
            if (File.Exists(scenePath))
            {
                Debug.Log("主菜单场景已存在，跳过");
                return;
            }

            // 创建新场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 创建主相机
            GameObject camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 1, -10);
            Camera cam = camera.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            // 创建方向光
            GameObject light = new GameObject("Directional Light");
            Light lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 创建Canvas
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            // 创建标题
            CreateUIText(canvas.transform, "Title", "BAD NORTH 3D", 60, TextAnchor.MiddleCenter, new Vector2(0, 100));

            // 创建开始按钮
            CreateUIButton(canvas.transform, "PlayButton", "开始游戏", new Vector2(0, -20), 40);

            // 创建退出按钮
            CreateUIButton(canvas.transform, "QuitButton", "退出游戏", new Vector2(0, -80), 30);

            // 保存场景
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"主菜单场景创建完成: {scenePath}");
        }

        static void CreateGameScene()
        {
            string scenePath = "Assets/Scenes/GameScene.unity";
            if (File.Exists(scenePath))
            {
                Debug.Log("游戏场景已存在，跳过");
                return;
            }

            // 创建新场景
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // === 主相机 ===
            GameObject camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 20, -30);
            camera.transform.rotation = Quaternion.Euler(60, 0, 0);
            camera.AddComponent<Camera>();
            camera.AddComponent<CameraController>();

            // === 方向光 ===
            GameObject light = new GameObject("Directional Light");
            Light lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // === 地面 ===
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10, 1, 10);
            ground.tag = "Untagged";

            // 设置为Navigation Static
            GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);

            // === 管理器 ===
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();

            GameObject unitSelectionManager = new GameObject("UnitSelectionManager");
            unitSelectionManager.AddComponent<UnitSelectionManager>();

            GameObject uiManager = new GameObject("UIManager");
            uiManager.AddComponent<UIManager>();

            GameObject audioManager = new GameObject("AudioManager");
            audioManager.AddComponent<AudioSynthesizer>();

            // === 敌人生成点 ===
            GameObject spawnPoints = new GameObject("EnemySpawnPoints");
            CreateSpawnPoint(spawnPoints.transform, "SpawnPoint_1", new Vector3(25, 0, 25));
            CreateSpawnPoint(spawnPoints.transform, "SpawnPoint_2", new Vector3(-25, 0, 25));
            CreateSpawnPoint(spawnPoints.transform, "SpawnPoint_3", new Vector3(25, 0, -25));
            CreateSpawnPoint(spawnPoints.transform, "SpawnPoint_4", new Vector3(-25, 0, -25));

            // === UI Canvas ===
            GameObject canvas = new GameObject("GameUI");
            Canvas canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            // 创建UI元素
            CreateGameUI(canvas.transform);

            // === 初始玩家单位 ===
            for (int i = 0; i < 3; i++)
            {
                CreateSquadUnit(new Vector3(i * 2 - 2, 0, 0));
            }

            // 保存场景
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"游戏场景创建完成: {scenePath}");
        }

        static void CreateGameUI(Transform parent)
        {
            // 顶部信息栏
            GameObject topPanel = new GameObject("TopPanel");
            topPanel.transform.SetParent(parent);
            RectTransform rect = topPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(0, 50);
            rect.anchoredPosition = new Vector2(0, -25);

            // 金币
            CreateUIText(topPanel.transform, "GoldText", "金币: 100", 24, TextAnchor.MiddleLeft, new Vector2(-250, 0));

            // 天数
            CreateUIText(topPanel.transform, "DayText", "第 1 天", 24, TextAnchor.MiddleCenter, new Vector2(0, 0));

            // 波次
            CreateUIText(topPanel.transform, "WaveText", "波次: 0/5", 24, TextAnchor.MiddleRight, new Vector2(250, 0));

            // 底部控制栏
            GameObject bottomPanel = new GameObject("BottomPanel");
            bottomPanel.transform.SetParent(parent);
            RectTransform bottomRect = bottomPanel.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0);
            bottomRect.sizeDelta = new Vector2(0, 60);
            bottomRect.anchoredPosition = new Vector2(0, 30);

            // 选中单位数
            CreateUIText(bottomPanel.transform, "SelectedCountText", "选中: 0", 20, TextAnchor.MiddleLeft, new Vector2(-200, 0));

            // 招募按钮
            CreateUIButton(bottomPanel.transform, "RecruitButton", "招募单位 (20金)", new Vector2(150, 0), 24);

            // 消息面板（默认隐藏）
            GameObject messagePanel = new GameObject("MessagePanel");
            messagePanel.transform.SetParent(parent);
            RectTransform msgRect = messagePanel.AddComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0.5f, 0.5f);
            msgRect.anchorMax = new Vector2(0.5f, 0.5f);
            msgRect.sizeDelta = new Vector2(400, 60);
            msgRect.anchoredPosition = new Vector2(0, 100);

            GameObject msgTextObj = new GameObject("MessageText");
            msgTextObj.transform.SetParent(messagePanel.transform);
            TextMeshProUGUI msgText = msgTextObj.AddComponent<TextMeshProUGUI>();
            msgText.alignment = TextAlignmentOptions.Center;
            msgText.fontSize = 24;
            msgText.color = Color.yellow;
            RectTransform msgTextRect = msgTextObj.GetComponent<RectTransform>();
            msgTextRect.anchorMin = Vector2.zero;
            msgTextRect.anchorMax = Vector2.one;
            msgTextRect.sizeDelta = Vector2.zero;

            // 波次开始面板（默认隐藏）
            GameObject waveStartPanel = new GameObject("WaveStartPanel");
            waveStartPanel.transform.SetParent(parent);
            RectTransform waveRect = waveStartPanel.AddComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0.3f, 0.4f);
            waveRect.anchorMax = new Vector2(0.7f, 0.6f);
            waveRect.sizeDelta = Vector2.zero;

            CreateUIText(waveStartPanel.transform, "WaveStartText", "第 1 波开始！", 36, TextAnchor.MiddleCenter, Vector2.zero);
        }

        static void CreatePrefabs()
        {
            CreateSquadUnitPrefab();
            CreateEnemyPrefab();
        }

        static void CreateSquadUnitPrefab()
        {
            string prefabPath = "Assets/Prefabs/Units/SquadUnit.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log("SquadUnit预制体已存在，跳过");
                return;
            }

            // 创建单位对象
            GameObject unit = new GameObject("SquadUnit");

            // 添加组件
            SquadUnit squadUnit = unit.AddComponent<SquadUnit>();
            unit.AddComponent<UnityEngine.AI.NavMeshAgent>();
            CapsuleCollider capsule = unit.AddComponent<CapsuleCollider>();
            Rigidbody rb = unit.AddComponent<Rigidbody>();

            // 配置组件
            rb.isKinematic = true;
            capsule.height = 2f;
            capsule.radius = 0.5f;
            capsule.center = new Vector3(0, 1, 0);

            // 创建身体
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(unit.transform);
            body.transform.localPosition = new Vector3(0, 1, 0);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            body.layer = 0;

            Material blueMat = new Material(Shader.Find("Standard"));
            blueMat.color = new Color(0.2f, 0.5f, 1f);
            body.GetComponent<Renderer>().material = blueMat;
            DestroyImmediate(body.GetComponent<CapsuleCollider>());

            // 创建武器
            GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            weapon.name = "Weapon";
            weapon.transform.SetParent(body.transform);
            weapon.transform.localPosition = new Vector3(0.5f, 0, 0.5f);
            weapon.transform.localScale = new Vector3(0.1f, 0.1f, 1.2f);
            weapon.transform.localRotation = Quaternion.Euler(90, 0, 0);

            Material weaponMat = new Material(Shader.Find("Standard"));
            weaponMat.color = new Color(0.4f, 0.3f, 0.2f);
            weapon.GetComponent<Renderer>().material = weaponMat;
            DestroyImmediate(weapon.GetComponent<CapsuleCollider>());

            // 创建选择指示器
            GameObject selector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selector.name = "Selector";
            selector.transform.SetParent(unit.transform);
            selector.transform.localPosition = new Vector3(0, 0.05f, 0);
            selector.transform.localScale = new Vector3(1.2f, 0.1f, 1.2f);

            Material selectorMat = new Material(Shader.Find("Standard"));
            selectorMat.color = new Color(1f, 1f, 0f, 0.5f);
            selector.GetComponent<Renderer>().material = selectorMat;
            DestroyImmediate(selector.GetComponent<CapsuleCollider>());
            selector.SetActive(false);

            // 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(unit, prefabPath);
            DestroyImmediate(unit);

            Debug.Log($"SquadUnit预制体创建完成: {prefabPath}");
        }

        static void CreateEnemyPrefab()
        {
            string prefabPath = "Assets/Prefabs/Enemies/Enemy.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log("Enemy预制体已存在，跳过");
                return;
            }

            // 创建敌人对象
            GameObject enemy = new GameObject("Enemy");

            // 添加组件
            Enemy enemyScript = enemy.AddComponent<Enemy>();
            enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
            BoxCollider box = enemy.AddComponent<BoxCollider>();
            Rigidbody rb = enemy.AddComponent<Rigidbody>();

            // 配置组件
            rb.isKinematic = true;
            box.size = new Vector3(1, 2, 1);
            box.center = new Vector3(0, 1, 0);

            // 创建身体
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(enemy.transform);
            body.transform.localPosition = new Vector3(0, 1, 0);
            body.transform.localScale = new Vector3(0.8f, 1.8f, 0.8f);

            Material enemyMat = new Material(Shader.Find("Standard"));
            enemyMat.color = new Color(1f, 0.2f, 0.2f);
            body.GetComponent<Renderer>().material = enemyMat;
            DestroyImmediate(body.GetComponent<BoxCollider>());

            // 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
            DestroyImmediate(enemy);

            Debug.Log($"Enemy预制体创建完成: {prefabPath}");
        }

        static void CreateMaterials()
        {
            // 地面材质
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.3f, 0.5f, 0.3f);
            AssetDatabase.CreateAsset(groundMat, "Assets/Materials/Ground.mat");

            Debug.Log("材质创建完成");
        }

        // === 辅助函数 ===

        static void CreateSpawnPoint(Transform parent, string name, Vector3 position)
        {
            GameObject spawnPoint = new GameObject(name);
            spawnPoint.transform.SetParent(parent);
            spawnPoint.transform.position = position;
        }

        static GameObject CreateSquadUnit(Vector3 position)
        {
            string prefabPath = "Assets/Prefabs/Units/SquadUnit.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError("找不到SquadUnit预制体");
                return null;
            }

            GameObject unit = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            unit.transform.position = position;
            return unit;
        }

        static GameObject CreateUIText(Transform parent, string name, string text, int fontSize, TextAnchor anchor, Vector2 position)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200, 50);
            rect.anchoredPosition = position;

            return textObj;
        }

        static GameObject CreateUIButton(Transform parent, string name, string text, Vector2 position, int fontSize)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent);

            RectTransform rect = button.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200, 50);
            rect.anchoredPosition = position;

            Image image = button.AddComponent<Image>();
            Color buttonColor = new Color(0.2f, 0.4f, 0.8f);
            image.color = buttonColor;

            Button btn = button.AddComponent<Button>();

            // 按钮文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return button;
        }
    }
}
