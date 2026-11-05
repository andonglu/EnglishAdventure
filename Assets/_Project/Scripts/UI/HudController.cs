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
            var root = UiFactory.CreateCanvas("HUD", gm.transform);
            var hud = root.gameObject.AddComponent<HudController>();
            hud._gm = gm;
            hud.Build(root);
            return hud;
        }

        void Build(RectTransform root)
        {
            // 顶栏背景条
            var bar = UiFactory.Panel("TopBar", root, new Color(1f, 1f, 1f, 0.85f));
            UiFactory.Stretch(bar.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -70), new Vector2(0, 0));

            // 标题
            var title = UiFactory.Text("Title", bar.transform, "英语冒险乐园  English Adventure", 30, FontStyle.Bold,
                new Color(0.25f, 0.35f, 0.55f));
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0, 0.5f); trt.anchorMax = new Vector2(0, 0.5f);
            trt.pivot = new Vector2(0, 0.5f);
            trt.anchoredPosition = new Vector2(28, 0);
            title.alignment = TextAnchor.MiddleLeft;

            // 星星
            var starIco = UiFactory.Text("StarIco", bar.transform, "★", 34, FontStyle.Bold, new Color(0.98f, 0.75f, 0.15f));
            var sirt = starIco.rectTransform;
            sirt.anchorMin = new Vector2(1, 0.5f); sirt.anchorMax = new Vector2(1, 0.5f);
            sirt.pivot = new Vector2(1, 0.5f);
            sirt.anchoredPosition = new Vector2(-160, 0);
            starIco.alignment = TextAnchor.MiddleRight;

            _stars = UiFactory.Text("Stars", bar.transform, "0", 30, FontStyle.Bold, new Color(0.25f, 0.35f, 0.55f));
            var strt = _stars.rectTransform;
            strt.anchorMin = new Vector2(1, 0.5f); strt.anchorMax = new Vector2(1, 0.5f);
            strt.pivot = new Vector2(1, 0.5f);
            strt.anchoredPosition = new Vector2(-100, 0);
            _stars.alignment = TextAnchor.MiddleRight;

            // 连续天数
            var flame = UiFactory.Text("StreakIco", bar.transform, "🔥", 30, FontStyle.Normal, new Color(0.95f, 0.45f, 0.20f));
            var frt = flame.rectTransform;
            frt.anchorMin = new Vector2(1, 0.5f); frt.anchorMax = new Vector2(1, 0.5f);
            frt.pivot = new Vector2(1, 0.5f);
            frt.anchoredPosition = new Vector2(-245, 0);
            flame.alignment = TextAnchor.MiddleRight;

            _streak = UiFactory.Text("Streak", bar.transform, "1 天", 24, FontStyle.Normal, new Color(0.25f, 0.35f, 0.55f));
            var trt2 = _streak.rectTransform;
            trt2.anchorMin = new Vector2(1, 0.5f); trt2.anchorMax = new Vector2(1, 0.5f);
            trt2.pivot = new Vector2(1, 0.5f);
            trt2.anchoredPosition = new Vector2(-280, 0);
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
    }

    /// <summary>轻提示气泡。</summary>
    public class Toast : MonoBehaviour
    {
        float _timer = 2.2f;

        public static void Show(RectTransform parent, string msg)
        {
            var bg = UiFactory.Panel("Toast", parent, new Color(0.15f, 0.18f, 0.25f, 0.92f));
            var rt = bg.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(520, 120);

            var txt = UiFactory.Text("Msg", rt, msg, 24, FontStyle.Normal, Color.white);
            UiFactory.Fill(txt.rectTransform);

            rt.gameObject.AddComponent<Toast>();
        }

        void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f) Destroy(gameObject);
        }
    }
}
