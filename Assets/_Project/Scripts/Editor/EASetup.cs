using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EA.EditorTools
{
    /// <summary>工程初始化与构建。菜单 Tools/English Adventure/。</summary>
    public static class EASetup
    {
        const string SceneDir = "Assets/_Project/Scenes";
        const string ScenePath = SceneDir + "/Home.unity";
        const string ConfigPath = "Assets/Resources/Settings/AppConfig.asset";

        [MenuItem("Tools/English Adventure/1. Setup Project")]
        public static void SetupProject()
        {
            // Player 设置
            PlayerSettings.companyName = "LRY";
            PlayerSettings.productName = "English Adventure";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.lry.englishadventure");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);

            // 横屏
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // 图标（程序生成，避免外部素材）
            var icon = EnsureIconAsset();
            PlayerSettings.SetIcons(NamedBuildTarget.Android, new[] { icon }, IconKind.Application);
            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, new[] { icon }, IconKind.Application);

            EnsureAppConfig();
            CreateHomeScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[EASetup] SetupProject done");
        }

        static void EnsureAppConfig()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<AppConfig>(ConfigPath);
            if (cfg == null)
            {
                Directory.CreateDirectory("Assets/Resources/Settings");
                cfg = ScriptableObject.CreateInstance<AppConfig>();
                AssetDatabase.CreateAsset(cfg, ConfigPath);
            }
        }

        static void CreateHomeScene()
        {
            Directory.CreateDirectory(SceneDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<GameManager>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static Texture2D EnsureIconAsset()
        {
            const string iconPath = "Assets/_Project/Art/app_icon.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (existing != null) return existing;

            Directory.CreateDirectory("Assets/_Project/Art");
            const int S = 256;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    float dx = x - S / 2f, dy = y - S / 2f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy) / (S / 2f);
                    Color c = r <= 1f
                        ? Color.Lerp(new Color(0.35f, 0.72f, 0.95f), new Color(0.20f, 0.50f, 0.85f), r)
                        : new Color(0, 0, 0, 0);
                    tex.SetPixel(x, y, c);
                }
            // 中央字母 E
            for (int y = 70; y < 190; y++)
                for (int x = 78; x < 180; x++)
                {
                    bool bar = y > 166 || y < 90 || x < 106;
                    if (bar) tex.SetPixel(x, y, Color.white);
                }
            tex.Apply();
            File.WriteAllBytes(iconPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(iconPath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
        }

        [MenuItem("Tools/English Adventure/2. Build Windows")]
        public static void BuildWindows()
        {
            SetupIfNeeded();
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/EnglishAdventure.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log($"[EASetup] Windows build result: {report.summary.result}, errors={report.summary.totalErrors}");
            Debug.Log("[EASetup] Windows build done");
        }

        [MenuItem("Tools/English Adventure/3. Build Android APK")]
        public static void BuildAndroid()
        {
            SetupIfNeeded();
            EnsureAndroidToolchain();
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Android/englishadventure.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log($"[EASetup] Android build result: {report.summary.result}, errors={report.summary.totalErrors}");
            Debug.Log("[EASetup] Android build done");
        }

        // 指向 Unity 安装目录自带的 OpenJDK / SDK / NDK（编辑器未通过 Hub 配置过这些路径）
        static void EnsureAndroidToolchain()
        {
            string androidPlayer = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/AndroidPlayer");
            string jdk = Path.Combine(androidPlayer, "OpenJDK");
            string sdk = Path.Combine(androidPlayer, "SDK");
            string ndk = Path.Combine(androidPlayer, "NDK");
            if (Directory.Exists(jdk)) EditorPrefs.SetString("JdkPath", jdk);
            if (Directory.Exists(sdk)) EditorPrefs.SetString("AndroidSdkPath", sdk);
            if (Directory.Exists(ndk)) EditorPrefs.SetString("AndroidNdkPath", ndk);
            Debug.Log($"[EASetup] Toolchain: jdk={jdk} sdk={sdk} ndk={ndk}");
        }

        [MenuItem("Tools/English Adventure/4. Build All")]
        public static void BuildAll()
        {
            SetupProject();
            BuildWindows();
            BuildAndroid();
        }

        static void SetupIfNeeded()
        {
            if (EditorBuildSettings.scenes.Length == 0) SetupProject();
        }
    }
}
