# Bad North 3D - 快速开始指南

## 立即开始测试

如果你已经有 Unity 2022.3+，可以快速设置并测试：

### 最小化设置（5 分钟）

1. **打开 Unity Hub**
   - 创建新 3D 项目

2. **导入脚本**
   - 将 `Assets/Scripts` 文件夹复制到你的项目
   - Unity 会自动编译脚本

3. **创建测试场景**
   ```
   游戏对象层次结构：
   - Main Camera
     - 位置: (0, 15, -20)
     - 旋转: (60, 0, 0)
     - 添加 CameraController 脚本

   - Directional Light
     - 位置: (0, 10, 0)
     - 旋转: (50, -30, 0)

   - Plane (作为地面)
     - 缩放: (10, 1, 10)

   - GameManager
     - 添加 GameManager 脚本

   - UnitSelectionManager
     - 添加 UnitSelectionManager 脚本
   ```

4. **创建测试预制体**

   **玩家单位:**
   - 创建胶囊体
   - 添加 SquadUnit 脚本
   - 添加 NavMeshAgent
   - 保存为预制体

   **敌人:**
   - 创建立方体
   - 添加 Enemy 脚本
   - 添加 NavMeshAgent
   - 保存为预制体

5. **烘焙 NavMesh**
   - 选择 Plane
   - 设置为 Navigation Static
   - Window > AI > Navigation > Bake

6. **设置引用**
   - 在 GameManager 中分配预制体引用
   - 添加敌人生成点（空对象）

7. **测试**
   - 按 Play
   - 空格键开始波次
   - 左键选择单位
   - 右键移动

## 核心脚本说明

### 必需的脚本
- `GameManager.cs` - 游戏主控制器
- `UnitSelectionManager.cs` - 单位选择
- `SquadUnit.cs` - 玩家单位
- `Enemy.cs` - 敌人 AI

### UI 脚本
- `UIManager.cs` - UI 控制
- 需要创建 UI Canvas 和相关元素

### 辅助脚本
- `CameraController.cs` - 摄像机控制
- `IslandGenerator.cs` - 地形生成
- `CombatEffects.cs` - 战斗特效
- `HealthBar.cs` - 血条显示

## 快速调试技巧

### 没有敌人出现？
- 检查 GameManager 中的 enemySpawnPoints 是否已设置
- 确认 enemyPrefab 已正确引用
- 检查敌人生成点是否在 NavMesh 上

### 单位不移动？
- 确认已烘焙 NavMesh
- 检查 NavMeshAgent 组件是否存在
- 验证目标点在 NavMesh 范围内

### 脚本编译错误？
- 确保使用 Unity 2022.3 或更高版本
- 检查是否有缺失的引用
- 查看 Console 的具体错误信息

## 下一步扩展

### 简单扩展
1. 添加更多敌人类型（修改 Enemy.cs）
2. 调整游戏平衡性（修改数值）
3. 添加简单特效（粒子系统）

### 中级扩展
1. 实现单位升级系统
2. 添加技能树
3. 创建多种岛屿类型

### 高级扩展
1. 实现存档/读档
2. 添加多人游戏
3. 创建战役模式

## 获取帮助

- 查看 README.md 了解完整文档
- 检查 SETUP_GUIDE.md 获取详细设置说明
- Unity 官方文档: https://docs.unity3d.com/

祝你游戏开发愉快！
