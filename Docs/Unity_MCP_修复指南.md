# Unity MCP 错误修复指南

## 🔧 快速解决方案

### 方法1：在Unity中禁用MCP（推荐）

1. **在Unity菜单栏中找到：**
   ```
   Unity MCP > Settings
   ```

2. **取消勾选：**
   - ✅ Auto-start server on load
   - ✅ Enable MCP integration

3. **点击Apply**

### 方法2：删除Unity MCP包

如果不需要Unity MCP功能，可以直接删除：

```bash
# 从项目中移除Unity MCP
rm -rf Packages/com.unitymcp.server
rm -rf Packages/manifest.json 中的com.unitymcp.server行
```

然后在Unity中让包管理器刷新。

### 方法3：修改端口配置

如果需要保留MCP但想修复端口问题：

1. 找到Unity MCP设置
2. 更改Server Port（默认可能是6589）
3. 改为其他端口，如：8590

## ✅ 验证修复

禁用后，Console应该不再显示这些错误：
- [MCPServer] Failed to start server
- [Unity MCP] Failed to start MCP server

## 🎮 重要提示

**这些错误不影响游戏功能！**
- 你的游戏代码完全正常
- 可以正常编译和运行
- 可以继续开发和测试

## 🚀 禁用后可以做什么

禁用Unity MCP后，你仍然可以：
- ✅ 正常使用Unity编辑器
- ✅ 编译和运行游戏
- ✅ 使用所有Bad North 3D功能
- ✅ 测试音频系统和战斗系统

唯一失去的是Unity MCP的远程控制功能，这对游戏开发不是必需的。
