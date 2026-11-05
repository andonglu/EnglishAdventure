# English Adventure 英语冒险乐园 V0.1.1

儿童英语学习 App（Unity 6.0.5, Built-in 渲染管线）。V0.1 交付：3D 卡通主场景 + 狐狸主角 + 四个学习区域入口 + 区域占位学习页 + HUD + 按钮音效 + Windows / Android 双端构建。V0.1.1 为架构升级（分支 `refactor/v0.1.1`）：为单词/音标/自然拼读/口语/测试系统做准备，不破坏 V0.1 任何可玩功能（tag `v0.1` 保留）。

## V0.1.1 架构升级一览

| 模块 | 内容 |
|---|---|
| 统一输入 | `InputRouter`：全工程唯一直读 Input 的组件（Tap/Drag/Pinch/Scroll/Back 事件），相机与返回键都走它 |
| 资源加载 | `ResourceService`（资源 ID → 路径，50 个 ID 全部经 manifest 装载）+ `MaterialLibrary`（Lit 材质唯一入口，`LitShaderName` 常量是 URP 迁移点） |
| 教学数据 | `ContentData`（WordData/SentenceData/PhonicsData/LessonData/QuizData 等 DTO）+ `ContentService`（JSON 装载 + 重复 ID/悬空引用/越界/缺媒体校验）。**改单词只改 `StreamingAssets/Content/*.json`，不动 C#** |
| 学习进度 | 存档 v2：`version` 字段 + `LearningProgressData`（单词掌握度/音标进度/口语记录/错题本/每日记录），v1 旧档自动迁移；星星只由学习行为发放，进区不再发星 |
| UI 分离 | `UiFactory`（Canvas/面板/文本收口）+ `NavigationService`（页面栈，返回键统一 PopTop） |
| 文本 | `EaText`：打包思源黑体（OFL），中文/音标真机不豆腐；emoji 回退映射表 |
| Prefab | `EABake.BakePrefabs` 烘焙狐狸+四区域为 Prefab（材质自动资产化到 `Resources/Materials/`），运行时优先加载 Prefab、缺失回退代码生成 |

## 教学内容自定义（不改代码）

`Assets/StreamingAssets/Content/`：

- `words.json` 单词（text/ipa/meaning/image/audio/exampleSentenceId）
- `phonics.json` 音标（symbol/exampleWords/hint）
- `sentences.json` 对话句（speaker/text）
- `lessons.json` 课程（按区域挂 wordRefs/phonicsRefs/sentenceRefs）
- `quizzes.json` 测验题（choices/answerIndex/explanation）
- `areas.json` 区域定义（标题/狐狸台词/主题色/地图位置）

媒体放 `StreamingAssets/media/`，json 里写相对路径。启动日志会输出装载统计，校验问题以 `[EA]` Warning 列出。

## 功能一览（V0.1）

| 功能 | 状态 |
|---|---|
| 3D 卡通世界（草地、树木、Kenney 低模建筑） | ✅ |
| 狐狸主角（程序化建模：耳朵抖动、尾巴摇摆、点击开心跳） | ✅ |
| 相机自动环绕 + 四区域标牌（中英文，面向相机） | ✅ |
| 四个学习区域入口：单词森林 / 音标学院 / 口语小镇 / 挑战竞技场 | ✅ 点击进入占位学习页 |
| 区域占位学习页（主题头 + 狐狸台词气泡 + 内容预览 + 返回） | ✅ V0.1.1 起内容驱动（读 Content/*.json） |
| HUD（标题、星星、连击、设置） | ✅ 设置弹 Toast 占位 |
| 按钮音效（点击 / 选择 / 确认 / 返回，Kenney interface_sounds） | ✅ |
| 本地存档（星星/连击 + V0.1.1 学习进度） | ✅ v1 旧档自动迁移 |
| Escape / 返回键退出确认弹窗 | ✅ V0.1.1 走导航栈 |

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
    Fonts/NotoSansSC.ttf     # 思源黑体（SIL OFL 1.1，见素材授权）
    Prefabs/                 # EABake 烘焙的狐狸 + 四区域
    Materials/               # 烘焙时资产化的材质
    Settings/                # AppConfig.asset + asset_manifest.json
  StreamingAssets/
    Content/*.json           # 全部教学内容（上面「教学内容自定义」）
    media/                   # 教学内容媒体（词图/词音频）
  _Project/
    Scenes/                  # Home 场景
    Scripts/
      Core/                  # GameManager / AppConfig / AudioManager / SaveManager / InputRouter
                               ResourceService / MaterialLibrary / ContentData / ContentService
      Home/                    # HomeWorld / AreaEntrance / CameraOrbit / FoxCompanion / ModelFactory
                               SignTextBillboard / AreaCatalog
      UI/                      # AreaPage / HudController / QuitConfirm / UiButton
                               UiFactory / NavigationService / EaText
      Editor/EASetup.cs        # 命令行一键搭建与构建
      Editor/EABake.cs         # Prefab 烘焙（含材质资产化）
```

## 构建方法

需要 Unity Hub 安装 6000.5.9f1（或改 `EASetup` 中版本无关的部分）。本机 Unity Package Manager 服务不可用，**所有命令必须加 `-noUpm`**；Android 构建必须带 `JAVA_HOME` 指向 Unity 自带 OpenJDK。

```bat
set UNITY="C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe"
set JDK="C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK"

:: 1) 首次生成场景/图标/配置
%UNITY% -batchmode -noUpm -projectPath . -executeMethod EA.EditorTools.EASetup.SetupProject -quit

:: 2) 烘焙 Prefab（改了世界/狐狸代码后重跑）
%UNITY% -batchmode -noUpm -projectPath . -executeMethod EA.EditorTools.EABake.BakePrefabs -quit

:: 3) Windows
%UNITY% -batchmode -noUpm -projectPath . -executeMethod EA.EditorTools.EASetup.BuildWindows -quit

:: 4) Android（记得带 JDK）
set JAVA_HOME=%JDK% & set PATH=%JDK%\bin;%PATH%
%UNITY% -batchmode -noUpm -projectPath . -executeMethod EA.EditorTools.EASetup.BuildAndroid -quit
```

产物：`Builds/Windows/EnglishAdventure.exe`、`Builds/Android/englishadventure.apk`（arm64-v8a，包名 `com.lry.englishadventure`）。

## 素材清单与授权

- **Kenney** [kenney.nl](https://kenney.nl) 模型/贴图/音效，**CC0 公有领域**：Nature Kit、Suburban Kit、Castle Kit、interface_sounds
- **思源黑体 Noto Sans SC**（`Resources/Fonts/NotoSansSC.ttf`），**SIL OFL 1.1**：可自由用于游戏打包与分发（保留许可声明即可）
- 狐狸与世界为程序化建模（代码生成），无外部资产

## 已知坑（重要）

1. **`Assets/csc.rsp` 不可删除**：vendored uGUI 的 `PhysicsRaycaster.Raycast` 整个函数体在 `#if PACKAGE_PHYSICS` 内，没有该定义时 3D 物体收不到任何 EventSystem 事件（UI 正常、3D 点击失效，无任何报错）。`PACKAGE_PHYSICS2D` 不要加——会触发 Unity 6 已移除的内部 SortingGroup API 编译错误。
2. **`-noUpm` 必须加**：本机 UnityPackageManager.exe 子进程无法启动，UPM 彻底不可用，走网络代理也无解。uGUI 源码已 vendor 进 `Plugins/UGUI` 并打过少量补丁（HelpURL 特性、MarkDirty、PhysicsRaycaster 显示检测）。标准开发环境（UPM 正常）可直接删除 `Plugins/UGUI` 与 `csc.rsp`，使用官方 `com.unity.ugui` 包。
3. **Android 构建要 `JAVA_HOME`**：EditorPrefs 里设 JDK 路径无效，必须在环境变量里给。
4. **模拟器限制**：APK 仅 arm64。x86_64 模拟器靠 ARM 翻译运行 Unity 会在场景加载时 SIGILL 崩溃（引擎层，无堆栈）。安装验证可以，画面/交互验证请在 Windows EXE 或 arm64 真机上做。
5. **标牌文字**：3D TextMesh 从 -Z 侧读才正（组件已处理朝向），改标牌代码时勿把 +Z 朝向相机。
6. **存档迁移语义**：`SaveData.version` 声明默认值必须是 1——JsonUtility 对 JSON 缺失字段保留声明默认值，v1 旧档（无 version）读入后才会被识别为 v1 并迁移。

## V0.2 计划（按开发文档）

单词森林首个学习闭环（看图学词 → 跟读 → quiz）→ 音标学院口型动画与跟读评分 → 口语小镇录音对话 → 挑战竞技场六种题型与错题本。V0.1.1 的数据/进度/导航底座已全部就绪。

