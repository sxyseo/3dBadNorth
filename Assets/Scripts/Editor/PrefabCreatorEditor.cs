using UnityEngine;
using UnityEditor;
using System.IO;

namespace BadNorth3D.Editor
{
    /// <summary>
    /// 编辑器工具：创建基础预制体
    /// </summary>
    public class PrefabCreatorEditor : EditorWindow
    {
        [MenuItem("Bad North 3D/Create Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<PrefabCreatorEditor>("Prefab Creator");
        }

        void OnGUI()
        {
            GUILayout.Label("预制体创建工具", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("创建玩家单位预制体", GUILayout.Height(30)))
            {
                CreateSquadUnitPrefab();
            }

            if (GUILayout.Button("创建敌人预制体", GUILayout.Height(30)))
            {
                CreateEnemyPrefab();
            }

            if (GUILayout.Button("创建所有预制体", GUILayout.Height(30)))
            {
                CreateSquadUnitPrefab();
                CreateEnemyPrefab();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("预制体将被创建到 Assets/Prefabs 文件夹", MessageType.Info);
        }

        void CreateSquadUnitPrefab()
        {
            // 创建文件夹
            if (!Directory.Exists("Assets/Prefabs"))
            {
                Directory.CreateDirectory("Assets/Prefabs");
            }

            if (!Directory.Exists("Assets/Models"))
            {
                Directory.CreateDirectory("Assets/Models");
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

            // 创建视觉表示
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(unit.transform);
            body.transform.localPosition = new Vector3(0, 1, 0);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            body.layer = LayerMask.NameToLayer("Default");

            // 创建材质
            Material unitMaterial = new Material(Shader.Find("Standard"));
            unitMaterial.color = Color.blue;
            body.GetComponent<Renderer>().material = unitMaterial;

            // 移除碰撞器（使用父对象的碰撞器）
            DestroyImmediate(body.GetComponent<CapsuleCollider>());

            // 创建武器
            GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            weapon.name = "Weapon";
            weapon.transform.SetParent(body.transform);
            weapon.transform.localPosition = new Vector3(0.5f, 0, 0);
            weapon.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
            weapon.transform.localRotation = Quaternion.Euler(0, 0, 90);

            Material weaponMaterial = new Material(Shader.Find("Standard"));
            weaponMaterial.color = Color.gray;
            weapon.GetComponent<Renderer>().material = weaponMaterial;

            DestroyImmediate(weapon.GetComponent<CapsuleCollider>());

            // 保存为预制体
            string prefabPath = "Assets/Prefabs/SquadUnit.prefab";
            PrefabUtility.SaveAsPrefabAsset(unit, prefabPath);

            DestroyImmediate(unit);

            Debug.Log($"玩家单位预制体已创建: {prefabPath}");
            AssetDatabase.Refresh();
        }

        void CreateEnemyPrefab()
        {
            // 创建文件夹
            if (!Directory.Exists("Assets/Prefabs"))
            {
                Directory.CreateDirectory("Assets/Prefabs");
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

            // 创建视觉表示
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(enemy.transform);
            body.transform.localPosition = new Vector3(0, 1, 0);
            body.transform.localScale = new Vector3(0.8f, 1.8f, 0.8f);

            // 创建材质
            Material enemyMaterial = new Material(Shader.Find("Standard"));
            enemyMaterial.color = Color.red;
            body.GetComponent<Renderer>().material = enemyMaterial;

            // 移除碰撞器（使用父对象的碰撞器）
            DestroyImmediate(body.GetComponent<BoxCollider>());

            // 保存为预制体
            string prefabPath = "Assets/Prefabs/Enemy.prefab";
            PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);

            DestroyImmediate(enemy);

            Debug.Log($"敌人预制体已创建: {prefabPath}");
            AssetDatabase.Refresh();
        }
    }
}
