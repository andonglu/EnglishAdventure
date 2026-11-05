using UnityEngine;
using UnityEngine.EventSystems;

namespace EA
{
    /// <summary>应用入口：加载配置与存档 → 建世界 → 建 UI。负责返回键与退出确认。</summary>
    public class GameManager : MonoBehaviour
    {
        public AppConfig Config { get; private set; }
        public AudioManager Audio { get; private set; }
        public HudController Hud { get; private set; }
        AreaPage _page;
        QuitConfirm _quit;
        FoxCompanion _fox;

        void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.SystemSetting;

            Config = AppConfig.Load();
            SaveManager.Load();

            // 服务
            Audio = gameObject.AddComponent<AudioManager>();
            Audio.Init(Config);

            // 事件系统（区域入口可点）
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // 统一输入（须先于相机创建）
            gameObject.AddComponent<InputRouter>();

            // 世界
            var world = HomeWorld.Build(this, transform);
            CameraOrbit.Create(Config, Vector3.zero);
            var cam = Camera.main;
            if (cam == null)
                cam = FindFirstObjectByType<Camera>();
            if (cam != null && cam.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>() == null)
                cam.gameObject.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();

            _fox = world.GetComponentInChildren<FoxCompanion>();

            // UI
            Hud = HudController.Create(this);
            _page = AreaPage.Create(this);
            _quit = QuitConfirm.Create(this);

            SaveManager.OnChanged += () => Hud.Refresh();
            Hud.Refresh();

            // 返回键：优先关学习页，否则弹退出确认
            InputRouter.I.OnBack += HandleBack;
        }

        void OnDestroy()
        {
            if (InputRouter.I != null) InputRouter.I.OnBack -= HandleBack;
        }

        void HandleBack()
        {
            if (_page != null && _page.IsOpen) { CloseArea(); return; }
            if (_quit != null) _quit.Toggle();
        }

        public void OpenArea(AreaId id)
        {
            bool isNew = SaveManager.MarkVisited(id.ToString(), Config.starPerNewArea);
            Audio.Select();
            if (isNew) Audio.Confirm();
            _page.Show(id);
        }

        public void CloseArea()
        {
            Audio.Back();
            _page.Hide();
        }

        /// <summary>狐狸被点（狐狸自己身上没有碰撞体，这里由 GameManager 做空检）。</summary>
        public void OnFoxTapped() => _fox?.HappyHop();
    }
}
