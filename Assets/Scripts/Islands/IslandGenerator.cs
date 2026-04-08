using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 程序化生成岛屿地形
    /// </summary>
    public class IslandGenerator : MonoBehaviour
    {
        [Header("地形设置")]
        public int mapSize = 50;
        public float heightScale = 5f;
        public float detailScale = 10f;

        [Header("岛屿形状")]
        public float islandRadius = 20f;
        public float falloffPower = 2f;

        private Terrain terrain;

        void Start()
        {
            GenerateIsland();
        }

        void GenerateIsland()
        {
            terrain = GetComponent<Terrain>();
            if (terrain == null)
            {
                terrain = gameObject.AddComponent<Terrain>();
            }

            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = mapSize + 1;
            terrainData.alphamapResolution = mapSize + 1;
            terrainData.baseMapResolution = mapSize + 1;

            // 设置地形大小
            terrainData.size = new Vector3(mapSize, heightScale, mapSize);

            // 生成高度图
            float[,] heights = GenerateHeightMap();
            terrainData.SetHeights(0, 0, heights);

            // 应用地形数据
            terrain.terrainData = terrainData;

            // 添加材质
            GenerateTerrainMaterial(terrain);

            // 添加植被和装饰物
            AddVegetation();
        }

        float[,] GenerateHeightMap()
        {
            float[,] heights = new float[mapSize + 1, mapSize + 1];

            for (int x = 0; x <= mapSize; x++)
            {
                for (int z = 0; z <= mapSize; z++)
                {
                    float xCoord = (float)x / mapSize;
                    float zCoord = (float)z / mapSize;

                    // 中心点坐标
                    float centerX = 0.5f;
                    float centerZ = 0.5f;

                    // 计算到中心的距离
                    float distance = Mathf.Sqrt(Mathf.Pow(xCoord - centerX, 2) + Mathf.Pow(zCoord - centerZ, 2));

                    // 创建岛屿形状 (边缘淡出)
                    float falloff = Mathf.Pow(distance * 2f, falloffPower);
                    falloff = Mathf.Clamp01(falloff);

                    // 使用柏林噪声生成地形
                    float perlin = Mathf.PerlinNoise(xCoord * detailScale, zCoord * detailScale);

                    // 组合噪声和岛屿形状
                    float height = perlin * (1f - falloff);

                    heights[x, z] = height;
                }
            }

            return heights;
        }

        void GenerateTerrainMaterial(Terrain terrain)
        {
            // 创建简单的地形材质
            Material terrainMaterial = new Material(Shader.Find("Standard"));

            // 根据高度设置不同颜色
            Texture2D terrainTexture = GenerateTerrainTexture();
            terrainMaterial.mainTexture = terrainTexture;

            terrain.materialTemplate = terrainMaterial;
        }

        Texture2D GenerateTerrainTexture()
        {
            Texture2D texture = new Texture2D(mapSize + 1, mapSize + 1);

            for (int x = 0; x <= mapSize; x++)
            {
                for (int z = 0; z <= mapSize; z++)
                {
                    float height = terrain.terrainData.GetHeight(x, z) / heightScale;

                    Color color;
                    if (height < 0.2f)
                    {
                        // 沙滩
                        color = new Color(0.76f, 0.7f, 0.5f);
                    }
                    else if (height < 0.5f)
                    {
                        // 草地
                        color = new Color(0.2f, 0.6f, 0.2f);
                    }
                    else
                    {
                        // 岩石
                        color = new Color(0.5f, 0.5f, 0.5f);
                    }

                    texture.SetPixel(x, z, color);
                }
            }

            texture.Apply();
            return texture;
        }

        void AddVegetation()
        {
            // 添加树木和岩石
            for (int i = 0; i < 50; i++)
            {
                float x = Random.Range(-islandRadius, islandRadius);
                float z = Random.Range(-islandRadius, islandRadius);

                // 检查是否在岛屿范围内
                float distance = Mathf.Sqrt(x * x + z * z);
                if (distance < islandRadius * 0.8f)
                {
                    // 随机添加树木或岩石
                    if (Random.value > 0.5f)
                    {
                        CreateTree(new Vector3(x, 0, z));
                    }
                    else
                    {
                        CreateRock(new Vector3(x, 0, z));
                    }
                }
            }
        }

        void CreateTree(Vector3 position)
        {
            GameObject tree = new GameObject("Tree");
            tree.transform.position = position;

            // 树干
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = new Vector3(0, 1f, 0);
            trunk.transform.localScale = new Vector3(0.3f, 2f, 0.3f);

            Material trunkMaterial = new Material(Shader.Find("Standard"));
            trunkMaterial.color = new Color(0.4f, 0.3f, 0.2f);
            trunk.GetComponent<Renderer>().material = trunkMaterial;

            // 树冠
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.transform.SetParent(tree.transform);
            foliage.transform.localPosition = new Vector3(0, 2.5f, 0);
            foliage.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            Material foliageMaterial = new Material(Shader.Find("Standard"));
            foliageMaterial.color = new Color(0.1f, 0.5f, 0.1f);
            foliage.GetComponent<Renderer>().material = foliageMaterial;

            Destroy(trunk.GetComponent<Collider>());
            Destroy(foliage.GetComponent<Collider>());
        }

        void CreateRock(Vector3 position)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = "Rock";
            rock.transform.position = position;
            rock.transform.localScale = new Vector3(
                Random.Range(0.5f, 1.5f),
                Random.Range(0.3f, 0.8f),
                Random.Range(0.5f, 1.5f)
            );

            Material rockMaterial = new Material(Shader.Find("Standard"));
            rockMaterial.color = new Color(0.4f, 0.4f, 0.4f);
            rock.GetComponent<Renderer>().material = rockMaterial;

            rock.GetComponent<BoxCollider>().enabled = false;
        }
    }
}
