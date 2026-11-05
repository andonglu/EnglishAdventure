# English Adventure 英语冒险乐园 V0.1

儿童英语学习 App（Unity 6.0.5, Built-in 渲染管线）。V0.1 交付：3D 卡通主场景 + 狐狸主角 + 四个学习区域入口 + 区域占位学习页 + HUD + 按钮音效 + Windows / Android 双端构建。

## 功能一览（V0.1）

| 功能 | 状态 |
|---|---|
| 3D 卡通世界（草地、树木、Kenney 低模建筑） | ✅ |
| 狐狸主角（程序化建模：耳朵抖动、尾巴摇摆、点击开心跳） | ✅ |
| 相机自动环绕 + 四区域标牌（中英文，面向相机） | ✅ |
| 四个学习区域入口：单词森林 / 音标学院 / 口语小镇 / 挑战竞技场 | ✅ 点击进入占位学习页 |
| 区域占位学习页（主题头 + 狐狸台词气泡 + 内容预览 + 返回） | ✅ |
| HUD（标题、星星、连击、设置） | ✅ 设置弹 Toast 占位 |
| 按钮音效（点击 / 选择 / 确认 / 返回，Kenney interface_sounds） | ✅ |
| 首次访问区域奖励星星 + 本地存档 | ✅ |
| Escape / 返回键退出确认弹窗 | ✅ |

## 工程结构

```
Assets/
  csc.rsp                    # 关键：定义 PACKAGE_PHYSICS（见「已知坑」）
  Plugins/UGUI/              # vendor 的 uGUI 源码（本机 UPM 不可用，见「已知坑」）
  Resources/
    Models/Nature/           # Kenney Nature Kit（18 个 FBX）
    Models/Town/             # Kenney Suburban Kit（房子等）
    Models/Castle/           # Kenney Castle Kit（竞技场）
    Textures/colormap_*.png  # Kenney 调色板贴图
    Audio/*.ogg              # Kenney interface_sounds（14 个）
  _Project/
    Scenes/                  # Home 场景
    Scripts/
      Core/                  # GameManager / AppConfig / AudioManager / SaveManager
      Home/                    # HomeWorld / AreaEntrance / CameraOrbit / FoxCompanion / ModelFactory / SignTextBillboard / AreaCatalog
      UI/                      # AreaPage / HudController / QuitConfirm / UiButton
      Editor/EASetup.cs        # 命令行一键搭建与构建
```

## 构建方法

需要 Unity Hub 安装 6000.5.9f1（或改 `EASetup` 中版本无关的部分）。本机 Unity Package Manager 服务不可用，**所有命令必须加 `-noUpm`**；Android 构建必须带 `JAVA_HOME` 指向 Unity 自带 OpenJDK。

```bat
set UNITY="C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe"
set JDK="C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK"

:: 1) 首次生成场景/图标/配置
%UNITY% -batchmode -noUpm -projectPath . -executeMethod EA.EditorTools.EASetup.SetupProject -quit

:: 2) Windows
%UNITY% -batchmode -noUpm -projectPath . -executeMethod EA.EditorTools.EASetup.BuildWindows -quit

:: 3) Android（记得带 JDK）
set JAVA_HOME=%JDK% & set PATH=%JDK%\bin;%PATH%
%UNITY% -batchmode -noUpm -projectPath . -executeMethod EA.EditorTools.EASetup.BuildAndroid -quit
```

产物：`Builds/Windows/EnglishAdventure.exe`、`Builds/Android/englishadventure.apk`（约 17 MB，arm64-v8a，包名 `com.lry.englishadventure`）。

## 素材清单与授权

全部素材来自 [Kenney](https://kenney.nl)，**CC0 公有领域**：

- **模型**：Nature Kit（树/蘑菇/石头/木头）、Suburban Kit（口语小镇房子）、Castle Kit（挑战竞技场），FBX + colormap 调色板贴图
- **音效**：interface_sounds（click / back / confirmation / open / close / error 等 14 个 ogg）
- 狐狸为程序化建模（代码生成），无外部资产

## 已知坑（重要）

1. **`Assets/csc.rsp` 不可删除**：vendored uGUI 的 `PhysicsRaycaster.Raycast` 整个函数体在 `#if PACKAGE_PHYSICS` 内，没有该定义时 3D 物体收不到任何 EventSystem 事件（UI 正常、3D 点击失效，无任何报错）。`PACKAGE_PHYSICS2D` 不要加——会触发 Unity 6 已移除的内部 SortingGroup API 编译错误。
2. **`-noUpm` 必须加**：本机 UnityPackageManager.exe 子进程无法启动，UPM 彻底不可用，走网络代理也无解。uGUI 源码已 vendor 进 `Plugins/UGUI` 并打过少量补丁（HelpURL 特性、MarkDirty、PhysicsRaycaster 显示检测）。
3. **Android 构建要 `JAVA_HOME`**：EditorPrefs 里设 JDK 路径无效，必须在环境变量里给。
4. **模拟器限制**：APK 仅 arm64。x86_64 模拟器靠 ARM 翻译运行 Unity 会在场景加载时 SIGILL 崩溃（引擎层，无堆栈）。安装验证可以，画面/交互验证请在 Windows EXE 或 arm64 真机上做。
5. **标牌文字**：3D TextMesh 从 -Z 侧读才正（组件已处理朝向），改标牌代码时勿把 +Z 朝向相机。

## V0.2 计划（按开发文档）

单词森林首个学习闭环（看图学词 → 跟读 → quiz）→ 音标学院口型动画与跟读评分 → 口语小镇录音对话 → 挑战竞技场六种题型与错题本。
