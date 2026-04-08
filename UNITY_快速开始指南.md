# 🚀 Unity 快速开始指南

> 一键创建所有Unity场景和资源

---

## ⚡ 快速开始（30秒）

### 1. 打开Unity项目
```
Unity Hub → 打开 3dBadNorth 项目
```

### 2. 等待编译完成
Unity 会自动编译脚本，等待几秒钟直到编译完成。

### 3. 运行一键设置
在Unity顶部菜单栏点击：
```
Bad North 3D → Quick Start → Create All Resources
```

### 4. 等待创建完成
你会看到Console窗口显示创建进度，大约需要10-20秒。

### 5. 打开游戏场景
```
Project窗口 → Scenes → 双击 GameScene
```

### 6. 烘焙NavMesh（重要！）
```
Window → AI → Navigation
点击底部的 "Bake" 按钮
等待烘焙完成
```

### 7. 开始游戏
点击Unity顶部的 **Play** 按钮 ▶️

**恭喜！游戏现在应该可以正常运行了！** 🎉

---

## 📋 创建内容清单

运行 `Create All Resources` 后会自动创建：

### 文件夹结构
```
Assets/
├── Scenes/              # 场景文件
├── Prefabs/            # 预制体
│   ├── Units/          # 玩家单位
│   ├── Enemies/        # 敌人
│   └── Effects/        # 特效
├── Materials/          # 材质
├── Textures/           # 纹理
├── Audio/              # 音频
└── Resources/          # 资源
```

### 场景文件

**1. MainMenu.unity** - 主菜单场景
- 主相机
- UI Canvas
- 标题文字
- 开始按钮
- 退出按钮

**2. GameScene.unity** - 游戏场景（完整设置）
- 主相机（已配置CameraController）
- 方向光（带阴影）
- 地面（Plane，已设置Navigation Static）
- GameManager（游戏管理器）
- UnitSelectionManager（单位选择管理器）
- UIManager（UI管理器）
- AudioManager（音频管理器）
- 4个敌人生成点
- 完整UI系统
- 3个初始玩家单位

### 预制体

**1. SquadUnit.prefab** - 玩家单位
- 完整的模型（蓝色Lowpoly风格）
- SquadUnit脚本
- NavMeshAgent组件
- CapsuleCollider碰撞体
- Rigidbody刚体
- 选择指示器（黄色圆环）
- 武器模型

**2. Enemy.prefab** - 敌人
- 完整的模型（红色立方体）
- Enemy脚本
- NavMeshAgent组件
- BoxCollider碰撞体
- Rigidbody刚体

### UI元素

**GameScene包含完整UI：**
- 顶部信息栏：金币、天数、波次
- 底部控制栏：选中单位数、招募按钮
- 消息提示面板（默认隐藏）
- 波次开始提示（默认隐藏）
- 波次完成提示（默认隐藏）

---

## 🎮 游戏操作

### 基本控制
- **WASD** - 移动摄像机
- **鼠标滚轮** - 缩放
- **鼠标中键拖拽** - 旋转视角
- **左键点击** - 选择单位
- **左键拖拽** - 框选多个单位
- **Shift+左键** - 多选单位
- **右键点击** - 移动/攻击命令
- **ESC** - 取消选择
- **空格键** - 开始下一波

### 游戏目标
1. 保护你的单位不被消灭
2. 击败所有波次的敌人
3. 用金币招募更多单位
4. 完成所有天数

---

## ⚙️ GameManager配置

打开GameScene后，需要在Inspector中配置GameManager：

**必需配置：**
1. **Enemy Prefab**：拖入 `Prefabs/Enemies/Enemy`
2. **Enemy Spawn Points**：展开 `EnemySpawnPoints`，拖入4个生成点

**可选配置：**
- Max Squad Size：最大单位数量（默认12）
- Total Waves：每天波次数（默认5）

---

## 🐛 常见问题

### 问题1：编译错误

**错误**：`The type or namespace name 'TextMeshPro' could not be found`

**解决**：
```
Window → Package Manager → TextMeshPro
点击 "Import TMP Essentials"
```

---

### 问题2：NavMesh相关错误

**错误**：`SetDestination can only be called on an active agent that has been placed on a NavMesh`

**解决**：
1. 选中Ground对象
2. 在Inspector顶部，勾选 "Static"
3. 选择 "Navigation Static"
4. `Window → AI → Navigation`
5. 点击 "Bake" 按钮

---

### 问题3：单位不移动

**原因**：NavMesh没有烘焙

**解决**：按照问题2的步骤烘焙NavMesh

---

### 问题4：敌人不生成

**原因**：GameManager没有配置生成点

**解决**：
1. 选中GameManager对象
2. 在Inspector中找到 "Enemy Spawn Points"
3. 设置Size为4
4. 拖入4个SpawnPoint对象

---

### 问题5：UI不显示

**原因**：Canvas可能被隐藏

**解决**：
1. 在Hierarchy中找到GameUI
2. 确保GameObject是激活状态（勾选框打勾）

---

## 🎯 下一步

资源创建完成后，你可以：

### 1. 测试基础功能
- [ ] 移动摄像机
- [ ] 选择和移动单位
- [ ] 生成敌人
- [ ] 战斗系统
- [ ] 招募新单位

### 2. 自定义游戏
- [ ] 调整单位数值
- [ ] 添加更多敌人类型
- [ ] 修改UI布局
- [ ] 添加音效

### 3. 开始开发
按照 `Docs/项目路线图.md` 继续开发新功能

---

## 📞 获取帮助

### 查看文档
- `DOCS_SUMMARY.md` - 文档总览
- `Docs/项目路线图.md` - 开发计划
- `Docs/AI协作技巧.md` - AI协作技巧

### 报告问题
GitHub: https://github.com/sxyseo/3dBadNorth/issues

---

## ✅ 检查清单

创建资源后，检查以下项目：

- [ ] Scenes文件夹有MainMenu.unity和GameScene.unity
- [ ] Prefabs文件夹有SquadUnit.prefab和Enemy.prefab
- [ ] GameScene中的GameManager已配置
- [ ] NavMesh已烘焙
- [ ] 点击Play没有错误
- [ ] 能看到3个蓝色单位
- [ ] 能用WASD移动摄像机
- [ ] 能用左键选择单位
- [ ] 能用右键移动单位

**全部勾选？恭喜！游戏已成功运行！** 🎉

---

**记住**：

> 先让游戏跑起来，再考虑添加新功能。
> 快速迭代 > 完美规划。

现在开始你的游戏开发之旅吧！🚀
