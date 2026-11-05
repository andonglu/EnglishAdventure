using UnityEngine;
using UnityEngine.UI;

namespace EA
{
    /// <summary>顶栏：标题、星星、连续天数、设置占位按钮。</summary>
    public class HudController : MonoBehaviour
    {
        Text _stars, _streak;
        GameManager _gm;

        public static HudController Create(GameManager gm)
        {
            var canvasGo = new GameObject("HUD", typeof(RectTransform));
            canvasGo.transform.SetParent(gm.transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            canvasGo.AddComponent<GraphicRaycaster>();

            var hud = canvasGo.AddComponent<HudController>();
            hud._gm = gm;
            hud.Build(canvasGo.GetComponent<RectTransform>());
            return hud;
        }

        void Build(RectTransform root)
        {
            // 顶栏背景条
            var bar = Panel("TopBar", new Color(1f, 1f, 1f, 0.85f));
            bar.rectTransform.SetParent(root, false);
            Stretch(bar.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -70), new Vector2(0, 0));

            // 标题
            var title = HomeWorld.Text("Title", "英语冒险乐园  English Adventure", 30, FontStyle.Bold,
                new Color(0.25f, 0.35f, 0.55f));
            title.rectTransform.SetParent(bar.rectTransform, false);
            title.rectTransform.anchorMin = new Vector2(0, 0.5f);
            title.rectTransform.anchorMax = new Vector2(0, 0.5f);
            title.rectTransform.pivot = new Vector2(0, 0.5f);
            title.rectTransform.anchoredPosition = new Vector2(28, 0);
            title.alignment = TextAnchor.MiddleLeft;

            // 星星
            var starIco = HomeWorld.Text("StarIco", "★", 34, FontStyle.Bold, new Color(0.98f, 0.75f, 0.15f));
            starIco.rectTransform.SetParent(bar.rectTransform, false);
            starIco.rectTransform.anchorMin = new Vector2(1, 0.5f);
            starIco.rectTransform.anchorMax = new Vector2(1, 0.5f);
            starIco.rectTransform.pivot = new Vector2(1, 0.5f);
            starIco.rectTransform.anchoredPosition = new Vector2(-160, 0);
            starIco.alignment = TextAnchor.MiddleRight;

            _stars = HomeWorld.Text("Stars", "0", 30, FontStyle.Bold, new Color(0.25f, 0.35f, 0.55f));
            _stars.rectTransform.SetParent(bar.rectTransform, false);
            _stars.rectTransform.anchorMin = new Vector2(1, 0.5f);
            _stars.rectTransform.anchorMax = new Vector2(1, 0.5f);
            _stars.rectTransform.pivot = new Vector2(1, 0.5f);
            _stars.rectTransform.anchoredPosition = new Vector2(-100, 0);
            _stars.alignment = TextAnchor.MiddleRight;

            // 连续天数
            var flame = HomeWorld.Text("StreakIco", "🔥", 30, FontStyle.Normal, new Color(0.95f, 0.45f, 0.20f));
            flame.rectTransform.SetParent(bar.rectTransform, false);
            flame.rectTransform.anchorMin = new Vector2(1, 0.5f);
            flame.rectTransform.anchorMax = new Vector2(1, 0.5f);
            flame.rectTransform.pivot = new Vector2(1, 0.5f);
            flame.rectTransform.anchoredPosition = new Vector2(-245, 0);
            flame.alignment = TextAnchor.MiddleRight;

            _streak = HomeWorld.Text("Streak", "1 天", 24, FontStyle.Normal, new Color(0.25f, 0.35f, 0.55f));
            _streak.rectTransform.SetParent(bar.rectTransform, false);
            _streak.rectTransform.anchorMin = new Vector2(1, 0.5f);
            _streak.rectTransform.anchorMax = new Vector2(1, 0.5f);
            _streak.rectTransform.pivot = new Vector2(1, 0.5f);
            _streak.rectTransform.anchoredPosition = new Vector2(-280, 0);
            _streak.alignment = TextAnchor.MiddleRight;

            // 设置（家长门 V1.0 占位）
            var btn = UiButton.Create(bar.rectTransform, "⚙", 30, new Color(0.85f, 0.88f, 0.95f),
                new Vector2(52, 52), new Vector2(-30, 0), new Vector2(1, 0.5f), new Vector2(1, 0.5f), _gm.Audio, () =>
                {
                    _gm.Audio.Question();
                    Toast.Show(root, "家长功能将在 V1.0 开放\nParent dashboard coming in V1.0");
                });
            btn.label.color = new Color(0.35f, 0.45f, 0.65f);
        }

        public void Refresh()
        {
            if (_stars != null) _stars.text = SaveManager.Data.stars.ToString();
            if (_streak != null) _streak.text = SaveManager.Data.streakDays + " 天";
        }

        static Image Panel(string name, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var img = go.AddComponent<Image>();
            img.color = c;
            return img;
        }

        static void Stretch(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = offMin; rt.offsetMax = offMax;
        }
    }

    /// <summary>轻提示气泡。</summary>
    public class Toast : MonoBehaviour
    {
        float _timer = 2.2f;

        public static void Show(RectTransform parent, string msg)
        {
            var go = new GameObject("Toast", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.25f, 0.92f);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(520, 120);

            var txt = HomeWorld.Text("Msg", msg, 24, FontStyle.Normal, Color.white);
            txt.rectTransform.SetParent(rt, false);
            Stretch(txt.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            go.AddComponent<Toast>();
        }

        void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f) Destroy(gameObject);
        }

        static void Stretch(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = offMin; rt.offsetMax = offMax;
        }
    }
}
