# Unity AI 开发指南

> 如何在 Unity 项目中实践 AI-Native 开发理念

---

## 🎯 Unity 在 AI 时代的定位

### Unity 的优势
- ✅ 成熟引擎，性能优异
- ✅ 丰富的教程和社区资源
- ✅ 强大的编辑器工具
- ✅ 跨平台发布能力强

### Unity 在 AI 协作中的挑战
- ❌ 资源文件（.unity, .prefab, .asset）AI 无法直接读取
- ❌ Inspector 配置的值 AI 看不到
- ❌ 可视化编辑操作 AI 无法执行
- ❌ 二进制格式文件不透明

### 解决方案：AI-Native Unity 开发

**核心思路**：尽可能将游戏数据和逻辑用代码表示，减少对编辑器的依赖

---

## 🏗️ Unity 项目的 AI-Native 架构

### 目录结构设计

```
Assets/
├── Scripts/
│   ├── Core/                   # 核心系统（AI 必读）
│   │   ├── Events/            # 事件系统
│   │   ├── EventBus.cs        # 全局事件总线
│   │   └── ServiceLocator.cs  # 服务定位器
│   │
│   ├── Config/                # 配置数据（AI 能改）
│   │   ├── GameConfig.cs      # 游戏数值配置
│   │   ├── UnitConfig.cs      # 单位配置
│   │   ├── EnemyConfig.cs     # 敌人配置
│   │   └── AudioConfig.cs     # 音频配置
│   │
│   ├── Managers/              # 管理器（AI 能理解）
│   │   ├── GameManager.cs     # 主游戏控制器
│   │   ├── CombatManager.cs   # 战斗管理器
│   │   ├── WaveManager.cs     # 波次管理器
│   │   └── AudioManager.cs    # 音频管理器
│   │
│   ├── Systems/               # 游戏系统（模块化）
│   │   ├── Combat/            # 战斗系统
│   │   ├── Movement/          # 移动系统
│   │   ├── AI/                # AI 系统
│   │   └── Economy/           # 经济系统
│   │
│   ├── Entities/              # 游戏实体
│   │   ├── Units/             # 玩家单位
│   │   ├── Enemies/           # 敌人
│   │   └── Structures/        # 建筑物
│   │
│   ├── UI/                    # 用户界面
│   │   ├── Controllers/       # UI 控制器
│   │   └── Views/             # UI 视图
│   │
│   ├── Utilities/             # 工具类
│   │   ├── ObjectPooler.cs    # 对象池
│   │   ├── ProceduralGen.cs   # 程序化生成
│   │   └── Extensions.cs      # 扩展方法
│   │
│   └── Editor/                # 编辑器工具
│       ├── GameSetup.cs       # 游戏设置工具
│       └── ConfigEditor.cs    # 配置编辑器
│
├── Resources/                 # 资源（最小化）
├── Prefabs/                   # 预制体（代码生成）
└── Scenes/                    # 场景文件
```

### 为什么这样设计？

1. **Core/** - 让 AI 理解项目的基础架构
2. **Config/** - 所有数值配置都在代码里，AI 可以调整
3. **Systems/** - 模块化系统，AI 可以独立修改每个模块
4. **Editor/** - 编辑器工具，自动化 AI 无法完成的操作

---

## 💻 编码规范：为 AI 优化的代码风格

### 1. 配置即代码

**❌ 避免 Inspector 配置**：
```csharp
public class Unit : MonoBehaviour {
    public float maxHealth = 100;      // AI 看不到
    public float moveSpeed = 5;        // AI 看不到
    public float attackDamage = 20;    // AI 看不到
}
```

**✅ 使用代码配置**：
```csharp
// Config/UnitConfig.cs
public class UnitConfig {
    public string unitId;
    public float maxHealth;
    public float moveSpeed;
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;

    public static readonly UnitConfig ARCHER = new UnitConfig {
        unitId = "archer",
        maxHealth = 80,
        moveSpeed = 6,
        attackDamage = 15,
        attackRange = 10,
        attackCooldown = 1.2f
    };

    public static readonly UnitConfig KNIGHT = new UnitConfig {
        unitId = "knight",
        maxHealth = 150,
        moveSpeed = 4,
        attackDamage = 25,
        attackRange = 2,
        attackCooldown = 1.0f
    };
}

// Entities/Units/Unit.cs
public class Unit : MonoBehaviour {
    public UnitConfig config;  // AI 能看到完整配置

    void Start() {
        currentHealth = config.maxHealth;
        GetComponent<NavMeshAgent>().speed = config.moveSpeed;
    }
}
```

**优势**：
- AI 可以同时看到所有单位的配置
- AI 可以轻松添加新单位类型
- AI 可以调整平衡性

---

### 2. 事件驱动架构

**使用事件总线解耦系统**：

```csharp
// Core/Events/EventBus.cs
public class EventBus {
    public static event Action<Unit, float> OnUnitDamaged;
    public static event Action<Unit> OnUnitDeath;
    public static event Action<int> OnWaveComplete;

    public static void UnitDamaged(Unit unit, float damage) {
        OnUnitDamaged?.Invoke(unit, damage);
    }

    public static void UnitDeath(Unit unit) {
        OnUnitDeath?.Invoke(unit);
    }

    public static void WaveComplete(int waveNumber) {
        OnWaveComplete?.Invoke(waveNumber);
    }
}

// Systems/Combat/CombatSystem.cs
public class CombatSystem : MonoBehaviour {
    void DealDamage(Unit target, float damage) {
        target.TakeDamage(damage);
        EventBus.UnitDamaged(target, damage);  // 触发事件

        if (target.currentHealth <= 0) {
            EventBus.UnitDeath(target);  // 触发事件
        }
    }
}

// UI/Controllers/HealthUI.cs
public class HealthUI : MonoBehaviour {
    void OnEnable() {
        EventBus.OnUnitDamaged += UpdateHealthBar;
        EventBus.OnUnitDeath += ShowDeathEffect;
    }

    void OnDisable() {
        EventBus.OnUnitDamaged -= UpdateHealthBar;
        EventBus.OnUnitDeath -= ShowDeathEffect;
    }

    void UpdateHealthBar(Unit unit, float damage) {
        // 更新血条
    }

    void ShowDeathEffect(Unit unit) {
        // 显示死亡特效
    }
}
```

**优势**：
- 系统之间松耦合
- AI 可以独立修改每个系统
- 易于扩展新功能

---

### 3. 工厂模式创建对象

**不用 Inspector 拖拽，用代码创建**：

```csharp
// Utilities/Factory/UnitFactory.cs
public class UnitFactory {
    public static Unit CreateUnit(UnitConfig config, Vector3 position) {
        GameObject unitObj = new GameObject(config.unitId);
        Unit unit = unitObj.AddComponent<Unit>();

        // 设置配置
        unit.config = config;

        // 添加必要组件
        NavMeshAgent agent = unitObj.AddComponent<NavMeshAgent>();
        agent.speed = config.moveSpeed;

        CapsuleCollider collider = unitObj.AddComponent<CapsuleCollider>();

        Rigidbody rb = unitObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // 创建视觉表现
        CreateUnitVisuals(unitObj, config);

        // 创建血条
        CreateHealthBar(unitObj);

        unitObj.transform.position = position;

        return unit;
    }

    static void CreateUnitVisuals(GameObject unitObj, UnitConfig config) {
        // 根据配置创建不同的视觉
        switch (config.unitId) {
            case "archer":
                CreateArcherVisuals(unitObj);
                break;
            case "knight":
                CreateKnightVisuals(unitObj);
                break;
        }
    }

    static void CreateArcherVisuals(GameObject unitObj) {
        // 身体
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(unitObj.transform);
        body.transform.localPosition = new Vector3(0, 1, 0);
        body.GetComponent<Renderer>().material.color = Color.blue;

        // 弓
        GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bow.transform.SetParent(unitObj.transform);
        // ... 创建弓的形状
    }
}

// Managers/GameManager.cs
public class GameManager : MonoBehaviour {
    void SpawnUnit(string unitId, Vector3 position) {
        UnitConfig config = UnitConfig.GetConfig(unitId);
        Unit unit = UnitFactory.CreateUnit(config, position);
        units.Add(unit);
    }
}
```

**优势**：
- AI 可以完全控制对象创建过程
- 不需要预制体文件
- 易于单元测试

---

### 4. 程序化资源生成

**不用美术资源，用代码生成**：

```csharp
// Utilities/ProceduralGen/IslandGenerator.cs
public class IslandGenerator : MonoBehaviour {
    public TerrainData GenerateIsland(int size, float roughness) {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = size + 1;
        terrainData.size = new Vector3(size, 50, size);

        // 使用柏林噪声生成高度图
        float[,] heights = new float[size + 1, size + 1];
        for (int x = 0; x <= size; x++) {
            for (int z = 0; z <= size; z++) {
                float nx = (float)x / size * roughness;
                float nz = (float)z / size * roughness;
                heights[x, z] = Mathf.PerlinNoise(nx, nz);
            }
        }

        // 应用岛屿遮罩（中心高，边缘低）
        ApplyIslandMask(heights, size);

        // 平滑地形
        SmoothTerrain(heights, size);

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    void ApplyIslandMask(float[,] heights, int size) {
        float centerX = size / 2f;
        float centerZ = size / 2f;
        float maxDistance = size / 2f;

        for (int x = 0; x <= size; x++) {
            for (int z = 0; z <= size; z++) {
                float distance = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));
                float falloff = distance / maxDistance;
                heights[x, z] *= (1f - falloff);
            }
        }
    }

    void SmoothTerrain(float[,] heights, int size) {
        // 实现地形平滑算法
    }
}

// Managers/GameManager.cs
public class GameManager : MonoBehaviour {
    void GenerateNewIsland() {
        IslandGenerator generator = GetComponent<IslandGenerator>();
        TerrainData terrainData = generator.GenerateIsland(64, 5f);

        Terrain terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
    }
}
```

---

## 🤖 AI 协作最佳实践

### 1. 给 AI 完整的上下文

**创建 README.md 作为 AI 的"记忆"**：

```markdown
# Bad North 3D - 项目说明

## 项目结构
- `Scripts/Config/` - 所有游戏配置数据
- `Scripts/Systems/` - 游戏逻辑系统
- `Scripts/Entities/` - 游戏实体类

## 重要约定
1. 所有配置都在 `Config/` 文件夹的代码里
2. 不使用 Inspector 配置数值
3. 使用 EventBus 进行系统通信
4. 所有对象通过 Factory 创建

## 当前问题
- [ ] 弓箭手攻击速度太慢
- [ ] 敌人 AI 有时会卡住
- [ ] 需要添加新的单位类型

## 最近修改
- 2024-04-08: 添加了骑士单位
- 2024-04-07: 重构了战斗系统
```

### 2. 使用 @ 引用文件

在与 AI 对话时：

```
我想调整弓箭手的属性：

当前配置在：@Scripts/Config/UnitConfig.cs
战斗逻辑在：@Scripts/Systems/Combat/CombatSystem.cs
视觉表现在：@Scripts/Utilities/Visuals/UnitVisuals.cs

请将攻击速度提高 20%，并自动调整动画时长
```

### 3. 让 AI 自己验证

```csharp
// Editor/AITests.cs
public class AITests : EditorWindow {
    [MenuItem("AI Tests/Verify Unit Balance")]
    public static void VerifyUnitBalance() {
        var configs = UnitConfig.GetAllConfigs();

        foreach (var config in configs) {
            float dps = config.attackDamage / config.attackCooldown;
            float effectiveHP = config.maxHealth * config.armor;

            Debug.Log($"{config.unitId}: DPS={dps}, EHP={effectiveHP}");

            // 警告不平衡的单位
            if (dps > 50) {
                Debug.LogWarning($"{config.unitId} DPS 过高！");
            }
        }
    }

    [MenuItem("AI Tests/Verify All Systems")]
    public static void VerifyAllSystems() {
        // 验证所有系统是否正常工作
        VerifyEventBus();
        VerifyFactories();
        VerifyAI();
    }
}
```

---

## 📊 Unity AI-Native 检查清单

### 项目架构
- [ ] 所有配置都在代码里（不用 Inspector）
- [ ] 使用事件驱动架构（EventBus）
- [ ] 使用工厂模式创建对象
- [ ] 程序化生成资源（地形、角色）
- [ ] 模块化的系统设计

### AI 协作
- [ ] 有详细的 README.md
- [ ] 有清晰的目录结构
- [ ] 代码注释充分
- [ ] 提供了 AI 测试工具
- [ ] 使用版本控制

### 编辑器工具
- [ ] 快速设置工具
- [ ] 配置编辑器
- [ ] 验证工具
- [ ] 自动化脚本

---

## 🎯 实战案例：用 AI 添加新单位

### 步骤 1：明确需求

```
我想添加一个新的"维京狂战士"单位：

属性：
- 生命值：200（高）
- 移动速度：3.5（慢）
- 攻击力：40（极高）
- 攻击范围：1.5（近战）
- 特殊能力：生命值低于 30% 时，攻击力翻倍

请修改相关文件，并确保与所有系统协调
```

### 步骤 2：AI 会自动修改

1. **配置**：`UnitConfig.cs` - 添加 BERSERKER 配置
2. **逻辑**：`Unit.cs` - 添加狂暴技能
3. **视觉**：`UnitVisuals.cs` - 创建狂战士外观
4. **战斗**：`CombatSystem.cs` - 处理狂暴机制
5. **UI**：`UnitUI.cs` - 显示狂暴状态

### 步骤 3：AI 自己验证

```
请运行 AI 测试，确保：
- 数值平衡
- 不影响其他单位
- 特殊能力正常工作
```

---

## 🚀 下一步

1. **重构现有代码**：按照 AI-Native 原则重构
2. **创建编辑器工具**：自动化 AI 无法完成的操作
3. **建立测试系统**：让 AI 能自动验证修改
4. **优化项目结构**：让 AI 更容易理解

---

## 📚 相关文档

- [AI 游戏开发方法论](./AI游戏开发方法论.md)
- [项目路线图](./项目路线图.md)
- [AI 协作技巧](./AI协作技巧.md)

---

**记住**：

> 在 Unity 中，AI 看不到 Inspector 里的配置。
> 把一切写成代码，AI 才能帮你修改一切。
