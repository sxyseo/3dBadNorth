using UnityEngine;
using System.Collections.Generic;

namespace BadNorth3D
{
    /// <summary>
    /// 程序化岛屿生成器 - 使用数学算法生成多样化地形
    /// AI可以调整参数来创建不同风格的岛屿
    /// </summary>
    public class ProceduralIslandGenerator : MonoBehaviour
    {
        [Header("生成配置")]
        public int mapSize = 50;
        public float heightScale = 10f;
        public float detailScale = 8f;

        [Header("岛屿形状")]
        public IslandShape islandShape = IslandShape.Single;
        public float islandRadius = 20f;
        public float falloffPower = 2f;

        [Header("地形特征")]
        public int numMountains = 3;
        public int numTrees = 50;
        public int numRocks = 20;

        private Terrain terrain;
        private TerrainData terrainData;

        void Start()
        {
            GenerateIsland();
        }

        public void GenerateIsland()
        {
            terrain = GetComponent<Terrain>();
            if (terrain == null)
            {
                terrain = gameObject.AddComponent<Terrain>();
            }

            CreateTerrainData();
            GenerateHeightMap();
            ApplyTerrainData();
            AddDetails();
        }

        void CreateTerrainData()
        {
            terrainData = new TerrainData();
            terrainData.heightmapResolution = mapSize + 1;
            terrainData.alphamapResolution = mapSize + 1;
            terrainData.baseMapResolution = mapSize + 1;
            terrainData.size = new Vector3(mapSize, heightScale, mapSize);
        }

        void GenerateHeightMap()
        {
            float[,] heights = new float[mapSize + 1, mapSize + 1];

            // 根据岛屿形状选择生成算法
            switch (islandShape)
            {
                case IslandShape.Single:
                    GenerateSingleIsland(heights);
                    break;
                case IslandShape.Double:
                    GenerateDoubleIsland(heights);
                    break;
                case IslandShape.Peninsula:
                    GeneratePeninsula(heights);
                    break;
                case IslandShape.Bay:
                    GenerateBay(heights);
                    break;
                case IslandShape.InlandLake:
                    GenerateInlandLake(heights);
                    break;
            }

            // 应用地形平滑
            SmoothHeights(heights, 2);

            // 设置高度图
            terrainData.SetHeights(0, 0, heights);
        }

        void GenerateSingleIsland(float[,] heights)
        {
            Vector2 center = new Vector2(0.5f, 0.5f);

            for (int x = 0; x <= mapSize; x++)
            {
                for (int z = 0; z <= mapSize; z++)
                {
                    Vector2 coord = new Vector2((float)x / mapSize, (float)z / mapSize);
                    float distance = Vector2.Distance(coord, center);

                    // 岛屿形状衰减
                    float falloff = Mathf.Pow(distance * 2.2f, falloffPower);
                    falloff = Mathf.Clamp01(falloff);

                    // 柏林噪声地形
                    float noise = Mathf.PerlinNoise(
                        coord.x * detailScale,
                        coord.y * detailScale
                    );

                    // 组合
                    float height = noise * (1f - falloff);
                    heights[x, z] = Mathf.Clamp01(height);
                }
            }
        }

        void GenerateDoubleIsland(float[,] heights)
        {
            // 两个岛屿中心
            Vector2[] centers = new Vector2[]
            {
                new Vector2(0.35f, 0.5f),
                new Vector2(0.65f, 0.5f)
            };

            for (int x = 0; x <= mapSize; x++)
            {
                for (int z = 0; z <= mapSize; z++)
                {
                    Vector2 coord = new Vector2((float)x / mapSize, (float)z / mapSize);

                    float maxInfluence = 0f;
                    foreach (Vector2 center in centers)
                    {
                        float distance = Vector2.Distance(coord, center);
                        float falloff = Mathf.Pow(distance * 2.5f, falloffPower * 0.8f);
                        float influence = 1f - Mathf.Clamp01(falloff);
                        maxInfluence = Mathf.Max(maxInfluence, influence);
                    }

                    float noise = Mathf.PerlinNoise(
                        coord.x * detailScale,
                        coord.y * detailScale
                    );

                    heights[x, z] = Mathf.Clamp01(noise * maxInfluence);
                }
            }
        }

        void GeneratePeninsula(float[,] heights)
        {
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 direction = new Vector2(1f, 0f); // 向右延伸

            for (int x = 0; x <= mapSize; x++)
            {
                for (int z = 0; z <= mapSize; z++)
                {
                    Vector2 coord = new Vector2((float)x / mapSize, (float)z / mapSize);
                    float distance = Vector2.Distance(coord, center);

                    // 半岛形状：向一个方向延伸
                    float directionalFactor = Vector2.Dot(coord - center, direction) * 0.5f + 0.5f;
                    float falloff = Mathf.Pow(distance * 1.8f, falloffPower * 0.7f);

                    float noise = Mathf.PerlinNoise(
                        coord.x * detailScale,
                        coord.y * detailScale
                    );

                    float height = noise * (1f - falloff) * (0.6f + directionalFactor * 0.4f);
                    heights[x, z] = Mathf.Clamp01(height);
                }
            }
        }

        void GenerateBay(float[,] heights)
        {
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 bayCenter = new Vector2(0.7f, 0.5f);
            float bayRadius = 0.15f;

            for (int x = 0; x <= mapSize; x++)
            {
                for (int z = 0; z <= mapSize; z++)
                {
                    Vector2 coord = new Vector2((float)x / mapSize, (float)z / mapSize);
                    float distance = Vector2.Distance(coord, center);

                    // 基础岛屿
                    float falloff = Mathf.Pow(distance * 2f, falloffPower);
                    float noise = Mathf.PerlinNoise(coord.x * detailScale, coord.y * detailScale);
                    float height = noise * (1f - falloff);

                    // 海湾凹陷
                    float distToBay = Vector2.Distance(coord, bayCenter);
                    if (distToBay < bayRadius)
                    {
                        float bayDepression = 1f - (distToBay / bayRadius);
                        height -= bayDepression * 0.3f;
                    }

                    heights[x, z] = Mathf.Clamp01(height);
                }
            }
        }

        void GenerateInlandLake(float[,] heights)
        {
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 lakeCenter = new Vector2(0.5f, 0.5f);
            float lakeRadius = 0.12f;

            for (int x = 0; x <= mapSize; x++)
            {
                for (int z = 0; z <= mapSize; z++)
                {
                    Vector2 coord = new Vector2((float)x / mapSize, (float)z / mapSize);
                    float distance = Vector2.Distance(coord, center);

                    // 基础岛屿
                    float falloff = Mathf.Pow(distance * 2.2f, falloffPower);
                    float noise = Mathf.PerlinNoise(coord.x * detailScale, coord.y * detailScale);
                    float height = noise * (1f - falloff);

                    // 内陆湖
                    float distToLake = Vector2.Distance(coord, lakeCenter);
                    if (distToLake < lakeRadius)
                    {
                        float lakeDepth = 1f - (distToLake / lakeRadius);
                        height -= lakeDepth * 0.4f;
                    }

                    heights[x, z] = Mathf.Clamp01(height);
                }
            }
        }

        void SmoothHeights(float[,] heights, int iterations)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                for (int x = 1; x < mapSize; x++)
                {
                    for (int z = 1; z < mapSize; z++)
                    {
                        float avg = (
                            heights[x - 1, z] +
                            heights[x + 1, z] +
                            heights[x, z - 1] +
                            heights[x, z + 1]
                        ) / 4f;

                        heights[x, z] = avg;
                    }
                }
            }
        }

        void ApplyTerrainData()
        {
            terrain.terrainData = terrainData;

            // 创建程序化材质
            Material terrainMaterial = GenerateProceduralTerrainMaterial();
            terrain.materialTemplate = terrainMaterial;
        }

        Material GenerateProceduralTerrainMaterial()
        {
            Material material = new Material(Shader.Find("Standard"));
            Texture2D texture = GenerateTerrainTexture();
            material.mainTexture = texture;
            return material;
        }

        Texture2D GenerateTerrainTexture()
        {
            Texture2D texture = new Texture2D(mapSize + 1, mapSize + 1);

            for (int x = 0; x <= mapSize; x++)
            {
                for (int z = 0; z <= mapSize; z++)
                {
                    float height = terrainData.GetHeight(x, z) / heightScale;
                    Color color = GetTerrainColor(height);
                    texture.SetPixel(x, z, color);
                }
            }

            texture.Apply();
            return texture;
        }

        Color GetTerrainColor(float height)
        {
            // 根据高度返回不同颜色
            if (height < 0.15f)
                return new Color(0.76f, 0.7f, 0.5f); // 沙滩
            else if (height < 0.4f)
                return new Color(0.2f, 0.6f, 0.2f); // 草地
            else if (height < 0.7f)
                return new Color(0.3f, 0.5f, 0.3f); // 深草
            else
                return new Color(0.5f, 0.5f, 0.5f); // 岩石
        }

        void AddDetails()
        {
            // 添加山脉
            for (int i = 0; i < numMountains; i++)
            {
                CreateMountain();
            }

            // 添加树木
            for (int i = 0; i < numTrees; i++)
            {
                CreateTree();
            }

            // 添加岩石
            for (int i = 0; i < numRocks; i++)
            {
                CreateRock();
            }
        }

        void CreateMountain()
        {
            // 在高地区域创建山脉
            float x = Random.Range(5f, mapSize - 5f);
            float z = Random.Range(5f, mapSize - 5f);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x), Mathf.RoundToInt(z));

            if (height > heightScale * 0.5f)
            {
                GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                mountain.name = "Mountain";
                mountain.transform.position = new Vector3(x, height / 2f, z);
                mountain.transform.localScale = new Vector3(
                    Random.Range(3f, 8f),
                    height * 0.3f,
                    Random.Range(3f, 8f)
                );

                Material mountainMaterial = new Material(Shader.Find("Standard"));
                mountainMaterial.color = new Color(0.4f, 0.4f, 0.4f);
                mountain.GetComponent<Renderer>().material = mountainMaterial;

                Destroy(mountain.GetComponent<CapsuleCollider>());
            }
        }

        void CreateTree()
        {
            // 在草地区域创建树木
            float x = Random.Range(2f, mapSize - 2f);
            float z = Random.Range(2f, mapSize - 2f);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x), Mathf.RoundToInt(z));

            if (height > heightScale * 0.2f && height < heightScale * 0.6f)
            {
                GameObject tree = new GameObject("Tree");
                tree.transform.position = new Vector3(x, height, z);

                // 树干
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.transform.SetParent(tree.transform);
                trunk.transform.localPosition = new Vector3(0, 1f, 0);
                trunk.transform.localScale = new Vector3(0.3f, 2f, 0.3f);

                Material trunkMaterial = new Material(Shader.Find("Standard"));
                trunkMaterial.color = new Color(0.4f, 0.3f, 0.2f);
                trunk.GetComponent<Renderer>().material = trunkMaterial;
                Destroy(trunk.GetComponent<CapsuleCollider>());

                // 树冠
                GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foliage.transform.SetParent(tree.transform);
                foliage.transform.localPosition = new Vector3(0, 2.5f, 0);
                foliage.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                Material foliageMaterial = new Material(Shader.Find("Standard"));
                foliageMaterial.color = new Color(0.1f, 0.5f, 0.1f);
                foliage.GetComponent<Renderer>().material = foliageMaterial;
                Destroy(foliage.GetComponent<SphereCollider>());
            }
        }

        void CreateRock()
        {
            // 在岩石区域创建石头
            float x = Random.Range(2f, mapSize - 2f);
            float z = Random.Range(2f, mapSize - 2f);
            float height = terrainData.GetHeight(Mathf.RoundToInt(x), Mathf.RoundToInt(z));

            if (height > heightScale * 0.1f)
            {
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = "Rock";
                rock.transform.position = new Vector3(x, height, z);
                rock.transform.localScale = new Vector3(
                    Random.Range(0.3f, 1f),
                    Random.Range(0.2f, 0.6f),
                    Random.Range(0.3f, 1f)
                );
                rock.transform.rotation = Random.rotation;

                Material rockMaterial = new Material(Shader.Find("Standard"));
                rockMaterial.color = new Color(0.4f, 0.4f, 0.4f);
                rock.GetComponent<Renderer>().material = rockMaterial;
                Destroy(rock.GetComponent<BoxCollider>());
            }
        }

        void OnDrawGizmosSelected()
        {
            // 显示岛屿范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, islandRadius);

            // 显示地图边界
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * heightScale / 2f,
                new Vector3(mapSize, heightScale, mapSize));
        }
    }

    /// <summary>
    /// 岛屿形状类型
    /// </summary>
    public enum IslandShape
    {
        Single,      // 单岛
        Double,      // 双岛
        Peninsula,   // 半岛
        Bay,         // 海湾
        InlandLake   // 内陆湖
    }
}
