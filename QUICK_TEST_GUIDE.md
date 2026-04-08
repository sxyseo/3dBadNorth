# 快速测试指南 - Bad North 3D

## 📋 前提条件
- ✅ Unity编辑器已打开（正在运行）
- ✅ 所有脚本已编译完成
- ✅ 项目文件已准备

## 🚀 快速设置步骤

### 第一步：使用编辑器工具创建基础场景

1. **打开Unity编辑器**
   - Unity应该已经在运行中
   - 确保Console中没有编译错误

2. **使用Quick Setup菜单**
   
   在Unity菜单栏找到：
   ```
   Bad North 3D > Quick Setup > Create All
   ```
   
   这将自动创建：
   - ✅ 文件夹结构
   - ✅ 基础预制体
   - ✅ 场景框架

3. **或者手动分步创建**

   **创建文件夹：**
   ```
   Bad North 3D > Quick Setup > Create Folders
   ```

   **创建预制体：**
   ```
   Bad North 3D > Create Prefabs
   ```
   点击"创建所有预制体"按钮

   **创建场景：**
   ```
   Bad North 3D > Setup Game Scene
   ```
   点击"创建基础游戏对象"按钮

### 第二步：配置场景

1. **设置Main Camera**
   - 位置: (0, 15, -20)
   - 旋转: (60, 0, 0)
   - 确保有CameraController组件

2. **创建地面**
   - GameObject -> 3D Object -> Plane
   - 命名为"Ground"
   - Scale: (10, 1, 10)

3. **设置敌人生成点**
   - 创建空对象"EnemySpawnPoints"
   - 在下面创建4个子对象：
     * SpawnPoint_1: (20, 0, 20)
     * SpawnPoint_2: (-20, 0, 20)
     * SpawnPoint_3: (20, 0, -20)
     * SpawnPoint_4: (-20, 0, -20)

### 第三步：烘焙NavMesh

1. **选择地面**
   - 选中Ground对象
   - 在Inspector中，勾选"Static"
   - 选择"Navigation Static"

2. **打开Navigation窗口**
   ```
   Window > AI > Navigation
   ```

3. **烘焙导航网格**
   - 点击"Bake"标签页
   - 点击"Bake"按钮
   - 等待烘焙完成

### 第四步：设置GameManager引用

1. **选择GameManager对象**
   - 在Hierarchy中找到GameManager
   - 在Inspector中：

2. **设置Enemy Spawn Points**
   - 找到"Enemy Spawn Points"数组
   - 设置Size为4
   - 将4个SpawnPoint拖入对应位置

3. **设置Enemy Prefab**
   - 从Assets/Prefabs拖入Enemy预制体
   - 确保引用已设置

### 第五步：添加AudioSynthesizer

1. **创建AudioSynthesizer对象**
   - GameObject -> Create Empty
   - 命名为"AudioSynthesizer"
   - 添加AudioSynthesizer脚本

2. **配置音频设置**
   - Master Volume: 0.5
   - SFX Volume: 0.7
   - Music Volume: 0.3

### 第六步：创建UI Canvas

1. **创建Canvas**
   - GameObject -> UI -> Canvas
   - 命名为"MainCanvas"

2. **添加UI元素**

   **资源显示（左上角）：**
   - 创建Panel
   - 添加TextMeshPro文本：
     * Gold Text: "金币: 100"
     * Day Text: "第 1 天"
     * Wave Text: "波次: 0/5"

   **单位信息（右上角）：**
   - 选中单位计数: "选中: 0"

   **招募按钮（底部）：**
   - Button: "招募单位 (20金币)"

### 第七步：保存场景

1. **保存场景**
   - File -> Save
   - 命名为"GameScene"
   - 保存到Assets/Scenes/

2. **添加到Build Settings**
   - File -> Build Settings
   - 拖入GameScene
   - 确保在列表中

## 🎮 测试功能

### 基础测试

1. **点击Play按钮**
   - 应该看到4个玩家单位生成
   - 应该听到背景音乐

2. **测试单位选择**
   - 左键点击单位
   - 应该看到黄色选择圈
   - 听到选择音效

3. **测试移动**
   - 选中单位后右键点击地面
   - 单位应该移动到目标位置
   - 看到绿色移动指示器

4. **测试波次系统**
   - 按空格键开始波次
   - 敌人应该从4个生成点出现
   - 单位应该自动攻击敌人

5. **测试音频系统**
   - 听到背景音乐
   - 攻击音效
   - 死亡音效
   - UI点击音效

### 敌人类型测试

观察不同颜色的敌人：
- 🔴 **红色** - 普通战士（平衡）
- 🟠 **橙色** - 快速轻型（高速低血）
- 🟣 **紫色** - 重型坦克（低速高血）
- 🔵 **青色** - 远程弓箭手（发射投射物）

### 战斗测试

1. **近战战斗**
   - 普通敌人接近单位
   - 看到攻击动画
   - 听到攻击音效

2. **远程战斗**
   - 青色远程敌人发射投射物
   - 看到投射物飞行
   - 命中时有反馈

3. **死亡效果**
   - 敌人死亡时缩小消失
   - 单位死亡时向前倒下

### UI测试

1. **资源更新**
   - 击杀敌人获得金币
   - UI实时更新

2. **招募单位**
   - 点击招募按钮
   - 消耗20金币
   - 新单位生成

3. **波次进度**
   - 看到波次进度更新
   - 波次完成提示
   - 休息时间计时

## 🐛 常见问题

### 没有敌人出现？
- 检查GameManager的Enemy Spawn Points是否设置
- 确认Enemy Prefab已拖入
- 检查NavMesh是否已烘焙

### 单位不移动？
- 确认NavMesh已烘焙
- 检查点击位置是否在NavMesh范围内
- 查看Console错误信息

### 没有声音？
- 检查AudioSynthesizer对象是否存在
- 确认Master Volume不为0
- 检查AudioListener组件（通常在Main Camera）

### 脚本编译错误？
- 打开Console窗口查看具体错误
- 检查TextMeshPro包是否已安装
- 确认所有引用已正确设置

## 📊 性能检查

打开Stats面板测试性能：
- 目标FPS: 60+
- Draw Calls: <50
- 内存使用: 合理范围

## 🎯 验收标准

✅ **基础功能**
- [ ] 单位可以选择和移动
- [ ] 敌人正常生成和攻击
- [ ] 波次系统正常工作
- [ ] 音效正常播放

✅ **高级功能**
- [ ] 4种敌人类型都能出现
- [ ] 远程敌人投射物正常
- [ ] 死亡动画正常
- [ ] UI实时更新

✅ **体验**
- [ ] 游戏节奏合理
- [ ] 音效增强体验
- [ ] 操作流畅无卡顿

## 🚀 测试完成后

1. **截图记录**
   - 游戏画面截图
   - Console无错误
   - 性能数据

2. **记录问题**
   - 发现的bug
   - 需要改进的地方
   - 新功能想法

3. **准备下一阶段**
   - 规划阶段2功能
   - 准备资源需求
   - 制定开发计划

---

**祝你测试顺利！** 🎮

遇到问题请查看Console窗口的错误信息。
