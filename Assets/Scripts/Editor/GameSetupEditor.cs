using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BadNorth3D.Editor
{
    /// <summary>
    /// 编辑器工具：快速设置游戏场景
    /// </summary>
    public class GameSetupEditor : EditorWindow
    {
        [MenuItem("Bad North 3D/Setup Game Scene")]
        public static void ShowWindow()
        {
            GetWindow<GameSetupEditor>("Game Setup");
        }

        void OnGUI()
        {
            GUILayout.Label("Bad North 3D - 场景设置工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("创建基础游戏对象", GUILayout.Height(30)))
            {
                CreateBaseGameObjects();
            }

            if (GUILayout.Button("创建主菜单场景", GUILayout.Height(30)))
            {
                CreateMainMenuScene();
            }

            if (GUILayout.Button("创建游戏场景", GUILayout.Height(30)))
            {
                CreateGameScene();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("注意：这些操作会修改当前场景。建议在新场景中使用。", MessageType.Info);
        }

        void CreateBaseGameObjects()
        {
            // 查找或创建 Main Camera
            GameObject camera = GameObject.Find("Main Camera");
            if (camera == null)
            {
                camera = new GameObject("Main Camera");
                camera.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.transform.position = new Vector3(0, 15, -20);
            camera.transform.rotation = Quaternion.Euler(60, 0, 0);

            if (camera.GetComponent<CameraController>() == null)
            {
                camera.AddComponent<CameraController>();
            }

            // 创建方向光
            GameObject light = GameObject.Find("Directional Light");
            if (light == null)
            {
                light = new GameObject("Directional Light");
                light.AddComponent<Light>();
                light.GetComponent<Light>().type = LightType.Directional;
            }

            light.transform.position = new Vector3(0, 10, 0);
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 创建地面
            GameObject plane = GameObject.Find("Ground");
            if (plane == null)
            {
                plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.name = "Ground";
                plane.transform.localScale = new Vector3(10, 1, 10);
            }

            // 设置为 Navigation Static (使用最新的NavMesh API)
            // 注意：Unity 2022.3+ 建议使用 NavMeshBuilder，但这里保持向后兼容
            // GameObjectUtility.SetStaticEditorFlags(plane, StaticEditorFlags.NavigationStatic);
            // 改为直接标记为静态，让Unity自动处理
            GameObjectUtility.SetStaticEditorFlags(plane, StaticEditorFlags.ContributeGI);

            // 创建管理器
            CreateManager("GameManager", typeof(GameManager));
            CreateManager("UnitSelectionManager", typeof(UnitSelectionManager));
            CreateManager("UIManager", typeof(UIManager));
            CreateManager("AudioManager", typeof(AudioManager));

            // 创建敌人生成点
            CreateSpawnPoints();

            Debug.Log("基础游戏对象创建完成！");
        }

        void CreateMainMenuScene()
        {
            CreateBaseGameObjects();

            // 创建主菜单
            GameObject menu = new GameObject("MainMenu");
            menu.AddComponent<MainMenu>();

            // 创建 Canvas
            CreateCanvas();

            // 保存场景
            string scenePath = "Assets/Scenes/MainMenu.unity";
            if (!System.IO.Directory.Exists("Assets/Scenes"))
            {
                System.IO.Directory.CreateDirectory("Assets/Scenes");
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
            Debug.Log($"主菜单场景已保存到 {scenePath}");
        }

        void CreateGameScene()
        {
            CreateBaseGameObjects();

            // 创建岛屿
            GameObject island = new GameObject("Island");
            Terrain terrain = island.AddComponent<Terrain>();
            island.AddComponent<IslandGenerator>();

            // 保存场景
            string scenePath = "Assets/Scenes/GameScene.unity";
            if (!System.IO.Directory.Exists("Assets/Scenes"))
            {
                System.IO.Directory.CreateDirectory("Assets/Scenes");
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
            Debug.Log($"游戏场景已保存到 {scenePath}");
        }

        void CreateManager(string name, System.Type type)
        {
            GameObject manager = GameObject.Find(name);
            if (manager == null)
            {
                manager = new GameObject(name);
                manager.AddComponent(type);
            }
            else if (manager.GetComponent(type) == null)
            {
                manager.AddComponent(type);
            }
        }

        void CreateSpawnPoints()
        {
            GameObject spawnPoints = GameObject.Find("EnemySpawnPoints");
            if (spawnPoints == null)
            {
                spawnPoints = new GameObject("EnemySpawnPoints");
            }

            // 清除现有的生成点
            while (spawnPoints.transform.childCount > 0)
            {
                DestroyImmediate(spawnPoints.transform.GetChild(0).gameObject);
            }

            // 创建 4 个生成点
            Vector3[] positions = new Vector3[]
            {
                new Vector3(20, 0, 20),
                new Vector3(-20, 0, 20),
                new Vector3(20, 0, -20),
                new Vector3(-20, 0, -20)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i + 1}");
                spawnPoint.transform.SetParent(spawnPoints.transform);
                spawnPoint.transform.position = positions[i];
            }
        }

        void CreateCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                canvas = new GameObject("Canvas").AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.gameObject.AddComponent<CanvasScaler>();
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
