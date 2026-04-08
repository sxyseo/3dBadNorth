# Bad North 3D 项目概览

## 🎮 项目简介

一个受 **Bad North** 启发的 3D 实时战略岛屿防御游戏，使用 Unity 2022.3 LTS 开发。

## ✨ 核心特性

### 游戏玩法
- **RTS 风格控制**：左键选择/框选单位，右键移动/攻击
- **实时战斗**：单位自动攻击范围内的敌人
- **波次系统**：抵御逐渐增强的敌人波次
- **经济管理**：通过击杀敌人和完成波次赚取金币
- **单位招募**：花费金币扩充你的小队
- **阵型移动**：智能的单位编队和移动

### 技术特点
- **3D 环境**：完全 3D 的游戏世界
- **NavMesh AI**：智能的敌人寻路和战斗
- **程序化地形**：动态生成的岛屿
- **组件化架构**：模块化、可扩展的代码结构
- **编辑器工具**：快速设置和开发工具

## 📁 项目结构

```
3dBadNorth/
├── Assets/
│   ├── Scripts/
│   │   ├── Managers/           # 游戏管理器（6个文件）
│   │   │   ├── GameManager.cs          # 主游戏控制器
│   │   │   ├── UnitSelectionManager.cs # 单位选择
│   │   │   ├── CameraController.cs     # 摄像机控制
│   │   │   ├── AudioManager.cs         # 音频管理
│   │   │   └── WaveManager.cs          # 波次管理
│   │   │
│   │   ├── Player/             # 玩家相关（1个文件）
│   │   │   └── SquadUnit.cs           # 玩家单位
│   │   │
│   │   ├── Enemies/            # 敌人系统（1个文件）
│   │   │   └── Enemy.cs               # 敌人 AI
│   │   │
│   │   ├── Combat/             # 战斗系统（2个文件）
│   │   │   ├── HealthBar.cs           # 血条显示
│   │   │   └── CombatEffects.cs       # 战斗特效
│   │   │
│   │   ├── Islands/            # 岛屿系统（1个文件）
│   │   │   └── IslandGenerator.cs     # 地形生成
│   │   │
│   │   ├── UI/                 # 用户界面（2个文件）
│   │   │   ├── UIManager.cs           # UI 控制
│   │   │   └── MainMenu.cs            # 主菜单
│   │   │
│   │   ├── Utilities/          # 工具类（3个文件）
│   │   │   ├── SimpleObjectPooler.cs  # 对象池
│   │   │   ├── GameSettings.cs        # 游戏设置
│   │   │   └── Readme.txt             # 快速开始
│   │   │
│   │   └── Editor/             # 编辑器工具（3个文件）
│   │       ├── GameSetupEditor.cs     # 场景设置
│   │       ├── PrefabCreatorEditor.cs # 预制体创建
│   │       └── QuickSetup.cs          # 快速设置菜单
│   │
│   ├── Prefabs/               # 预制体（需手动创建）
│   ├── Scenes/                # 场景文件（需手动创建）
│   ├── Materials/             # 材质
│   └── Models/                # 3D 模型
│
├── ProjectSettings/           # Unity 项目设置
├── README.md                  # 项目说明
├── SETUP_GUIDE.md            # 设置指南
├── CHANGELOG.md              # 更新日志
└── .gitignore               # Git 忽略文件
```

## 🚀 快速开始

### 1. 安装 Unity
- 下载 Unity Hub: https://unity.com/download
- 安装 Unity 2022.3 LTS 或更高版本

### 2. 打开项目
- 在 Unity Hub 中点击 "Add"
- 选择项目根目录
- 等待 Unity 打开项目

### 3. 使用编辑器工具快速设置
Unity 菜单栏 → `Bad North 3D` → `Quick Setup` → `Create All`

这会自动创建：
- ✅ 文件夹结构
- ✅ 基础预制体
- ✅ 场景模板
- ⏳ NavMesh（需手动点击 Bake）

### 4. 开始游戏
- 打开 `GameScene.unity`
- 点击 Play
- 按**空格键**开始第一波

## 🎯 游戏控制

### 单位控制
- **左键点击**：选择单位
- **左键拖拽**：框选多个单位
- **Shift + 左键**：多选单位
- **右键点击**：移动/攻击命令
- **ESC**：取消选择

### 摄像机控制
- **WASD**：移动摄像机
- **鼠标边缘**：自动滚动
- **鼠标滚轮**：缩放
- **鼠标中键**：拖拽视角
- **鼠标右键**：旋转视角

### 游戏操作
- **空格键**：开始下一波
- **UI 按钮**：招募新单位

## 🔧 核心系统说明

### GameManager
游戏主控制器，管理：
- 游戏状态（天数、波次）
- 资源（金币）
- 单位管理
- 敌人生成
- 胜利/失败条件

### UnitSelectionManager
RTS 风格单位选择系统：
- 单击选择
- 框选多单位
- 阵型移动
- 智能编队

### SquadUnit
玩家单位：
- NavMesh 寻路
- 自动攻击
- 血条显示
- 选择指示器

### Enemy
敌人 AI：
- 自动寻路
- 攻击玩家单位
- 多种类型（普通/快速/重型）
- 属性随波次增强

### IslandGenerator
程序化地形：
- 柏林噪声生成
- 岛屿形状遮罩
- 自动植被
- 多层地形材质

## 📊 游戏机制

### 经济系统
- 初始金币：100
- 招募单位：20 金币
- 击杀敌人：5+ 金币
- 波次完成：20+ 金币
- 天数完成：50 金币

### 波次系统
- 每天有 5 波敌人
- 敌人数量随波次增加
- 敌人属性随天数增强
- 波次间隔可以休息和招募

### 单位属性
- 生命值：100
- 移动速度：5
- 攻击力：20
- 攻击范围：2
- 攻击速度：1 次/秒

## 🎨 扩展方向

### 简单扩展
1. 添加更多敌人类型
2. 创建不同外观的单位
3. 实现简单的升级系统
4. 添加更多音效

### 中级扩展
1. 技能树系统
2. 装备系统
3. 更多岛屿类型
4. 天气系统

### 高级扩展
1. 多人游戏
2. 存档系统
3. 战役模式
4. MOD 支持

## 📚 文档

- **README.md**：项目说明和特性介绍
- **SETUP_GUIDE.md**：详细的设置指南
- **CHANGELOG.md**：版本更新记录
- **Assets/Scripts/Utilities/Readme.txt**：快速开始指南

## 🛠️ 开发工具

Unity 编辑器菜单：`Bad North 3D`
- **Quick Setup**：一键创建所有资源
- **Setup Game Scene**：手动创建场景
- **Create Prefabs**：创建预制体
- **Documentation**：打开文档
- **Help**：帮助和支持

## 🐛 已知问题

- 初始版本，需要更多测试
- NavMesh 需要手动烘焙
- UI 需要手动配置
- 特效需要进一步完善

## 🤝 贡献

欢迎提交 bug 报告和功能建议！

## 📄 许可

本项目仅供学习和参考使用。

---

**版本**：0.1.0 Alpha
**引擎**：Unity 2022.3 LTS
**语言**：C#
**状态**：开发中
