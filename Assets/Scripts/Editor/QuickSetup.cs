using UnityEngine;
using UnityEditor;

namespace BadNorth3D.Editor
{
    /// <summary>
    /// 快速设置菜单
    /// </summary>
    public class QuickSetup
    {
        [MenuItem("Bad North 3D/Quick Setup/Create All", false, 1)]
        public static void CreateAll()
        {
            CreateFolders();
            CreatePrefabs();
            CreateScenes();
            BakeNavMesh();

            EditorUtility.DisplayDialog("完成", "所有基础资源已创建！", "确定");
        }

        [MenuItem("Bad North 3D/Quick Setup/Create Folders", false, 2)]
        public static void CreateFolders()
        {
            string[] folders = new string[]
            {
                "Assets/Scenes",
                "Assets/Prefabs",
                "Assets/Materials",
                "Assets/Models",
                "Assets/Textures",
                "Assets/Audio",
                "Assets/Scripts/Managers",
                "Assets/Scripts/Player",
                "Assets/Scripts/Enemies",
                "Assets/Scripts/Combat",
                "Assets/Scripts/UI",
                "Assets/Scripts/Islands",
                "Assets/Scripts/Utilities"
            };

            foreach (string folder in folders)
            {
                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("文件夹结构创建完成");
        }

        [MenuItem("Bad North 3D/Quick Setup/Create Prefabs", false, 3)]
        public static void CreatePrefabs()
        {
            // 这个菜单项会打开预制体创建窗口
            PrefabCreatorEditor.ShowWindow();
        }

        [MenuItem("Bad North 3D/Quick Setup/Create Scenes", false, 4)]
        public static void CreateScenes()
        {
            GameSetupEditor.ShowWindow();
        }

        [MenuItem("Bad North 3D/Quick Setup/Bake NavMesh", false, 5)]
        public static void BakeNavMesh()
        {
            // 打开 Navigation 窗口
            EditorWindow.GetWindow(typeof(UnityEditor.AI.NavigationWindow));
            Debug.Log("请手动点击 Bake 按钮来生成 NavMesh");
        }

        [MenuItem("Bad North 3D/Documentation/Readme", false, 10)]
        public static void OpenReadme()
        {
            Application.OpenURL("file://" + System.IO.Path.GetFullPath("README.md"));
        }

        [MenuItem("Bad North 3D/Documentation/Setup Guide", false, 11)]
        public static void OpenSetupGuide()
        {
            Application.OpenURL("file://" + System.IO.Path.GetFullPath("SETUP_GUIDE.md"));
        }

        [MenuItem("Bad North 3D/Help/Report Bug", false, 20)]
        public static void ReportBug()
        {
            Application.OpenURL("https://github.com/yourusername/3dBadNorth/issues");
        }

        [MenuItem("Bad North 3D/Help/About", false, 21)]
        public static void About()
        {
            EditorUtility.DisplayDialog(
                "关于 Bad North 3D",
                "Bad North 3D\n\n" +
                "一个受 Bad North 启发的 3D 实时战略岛屿防御游戏。\n\n" +
                "版本: 0.1.0 (Alpha)\n" +
                "引擎: Unity 2022.3 LTS\n\n" +
                "仅供学习和参考使用。",
                "确定"
            );
        }
    }
}
