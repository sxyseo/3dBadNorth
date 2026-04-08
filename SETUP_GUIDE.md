# Unity 项目设置指南

## 步骤 1: 安装 Unity Hub

1. 下载并安装 Unity Hub: https://unity.com/download
2. 创建 Unity ID（如果还没有）

## 步骤 2: 安装 Unity 2022.3 LTS

1. 打开 Unity Hub
2. 进入 "Installs" 标签
3. 点击 "Add"
4. 选择 "Unity 2022.3 LTS" 或更高版本
5. 等待安装完成

## 步骤 3: 创建或打开项目

### 选项 A: 创建新项目

1. 在 Unity Hub 中点击 "New Project"
2. 选择 "3D Core" 模板
3. 命名项目为 "BadNorth3D"
4. 选择位置并创建

### 选项 B: 打开现有项目

1. 在 Unity Hub 中点击 "Add"
2. 选择包含 `Assets` 和 `ProjectSettings` 文件夹的项目根目录
3. 点击 "Select Folder"

## 步骤 4: 项目配置

### 添加必要的包

1. 打开 Unity
2. 进入 `Window > Package Manager`
3. 安装以下包：
   - TextMeshPro (必需)
   - Universal Windows Platform (可选)

### 配置项目设置

1. `Edit > Project Settings > Physics`
   - 启用 "Auto Sync Transforms"

2. `Edit > Project Settings > Quality`
   - 选择适当的质量级别

3. `Edit > Project Settings > Player`
   - 设置公司名称
   - 设置产品名称

## 步骤 5: 创建初始场景

1. 创建以下场景文件：
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/GameScene.unity`

### 主菜单场景设置

**游戏对象:**
- Main Camera
  - 位置: (0, 1, -10)
  - 旋转: (0, 0, 0)

- Directional Light
  - 位置: (0, 3, 0)
  - 旋转: (50, -30, 0)

- MainMenu (空 GameObject)
  - 添加 MainMenu.cs 脚本

- Canvas
  - Render Mode: Screen Space - Overlay
  - 添加以下 UI 元素：
    - Title (TextMeshPro)
    - Play Button
    - Settings Button
    - Quit Button

### 游戏场景设置

**游戏对象:**
- Main Camera
  - 位置: (0, 15, -20)
  - 旋转: (60, 0, 0)
  - 添加 CameraController.cs 脚本

- Directional Light
  - 位置: (0, 10, 0)
  - 旋转: (50, -30, 0)

- GameManager (空 GameObject)
  - 添加 GameManager.cs 脚本

- UnitSelectionManager (空 GameObject)
  - 添加 UnitSelectionManager.cs 脚本

- UIManager (空 GameObject)
  - 添加 UIManager.cs 脚本

- AudioManager (空 GameObject)
  - 添加 AudioManager.cs 脚本

- Island (空 GameObject)
  - 添加 Terrain 组件
  - 添加 IslandGenerator.cs 脚本

- EnemySpawnPoints (空 GameObject)
  - 创建多个子对象作为生成点

- Canvas
  - 添加 UI 元素（金币、天数、波次等）

## 步骤 6: 创建预制体

### SquadUnit 预制体

1. 创建空 GameObject "SquadUnit"
2. 添加以下组件：
   - SquadUnit.cs
   - NavMeshAgent
   - Capsule Collider
   - Rigidbody (isKinematic = true)
3. 创建简单的 3D 表示（胶囊体）
4. 保存为预制体

### Enemy 预制体

1. 创建空 GameObject "Enemy"
2. 添加以下组件：
   - Enemy.cs
   - NavMeshAgent
   - Box Collider
   - Rigidbody (isKinematic = true)
3. 创建简单的 3D 表示（立方体）
4. 保存为预制体

## 步骤 7: 烘焙 NavMesh

1. 选择地形
2. 进入 `Window > AI > Navigation`
3. 在 "Bake" 标签中：
   - 设置 Agent Radius
   - 设置 Agent Height
   - 点击 "Bake"

## 步骤 8: 测试

1. 打开 GameScene
2. 设置 GameManager 的引用：
   - Enemy Prefab
   - Enemy Spawn Points
   - Squad Unit Prefab
3. 点击 Play
4. 测试基本功能：
   - 空格键开始波次
   - 左键选择单位
   - 右键移动单位
   - 招募新单位

## 常见问题

### NavMesh 不工作
- 确保已烘焙 NavMesh
- 检查 NavMeshAgent 设置
- 确保对象在 Navigation Static 静态几何体上

### UI 不显示
- 检查 Canvas 设置
- 确保 TextMeshPro 已导入
- 检查 UI Manager 的引用

### 单位不移动
- 检查 NavMeshAgent 组件
- 确保目标点在 NavMesh 上
- 检查 UnitSelectionManager 设置

## 下一步

完成基础设置后，你可以：
1. 自定义单位外观
2. 添加音效和音乐
3. 创建更多预制体变体
4. 实现存档系统
5. 添加更多游戏机制
