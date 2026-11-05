using UnityEngine;
using UnityEngine.UI;

namespace EA
{
    /// <summary>
    /// 区域页（V0.1 占位）：主题头 + 狐狸台词 + 该区域的「内容预览」+ 返回按钮。
    /// V0.2 起各区域页替换为真正的学习流程，页面骨架保持不变。
    /// </summary>
    public class AreaPage : MonoBehaviour
    {
        public bool IsOpen { get; private set; }
        GameManager _gm;
        RectTransform _root;
        RectTransform _contentHost;
        Image _header;

        public static AreaPage Create(GameManager gm)
        {
            var go = new GameObject("AreaPage", typeof(RectTransform));
            go.transform.SetParent(gm.transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            go.AddComponent<GraphicRaycaster>();
            var page = go.AddComponent<AreaPage>();
            page._gm = gm;
            page.Build(go.GetComponent<RectTransform>());
            go.SetActive(false);
            return page;
        }

        void Build(RectTransform root)
        {
            _root = root;

            // 背景遮罩
            var mask = gameObject.AddComponent<Image>();
            mask.color = new Color(0.10f, 0.12f, 0.18f, 0.96f);

            // 主题头
            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(root, false);
            _header = header.AddComponent<Image>();
            var hrt = header.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, 1); hrt.anchorMax = new Vector2(1, 1);
            hrt.offsetMin = new Vector2(0, -150); hrt.offsetMax = new Vector2(0, 0);

            // 返回按钮
            UiButton.Create(hrt, "← 返回", 28, new Color(1f, 1f, 1f, 0.25f),
                new Vector2(150, 70), new Vector2(30, -75), new Vector2(0, 1), new Vector2(0, 1),
                _gm.Audio, () => _gm.CloseArea());

            // 内容宿主（由 Show 填充）
            var host = new GameObject("Content", typeof(RectTransform));
            host.transform.SetParent(root, false);
            var crt = host.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
            crt.offsetMin = new Vector2(60, 40); crt.offsetMax = new Vector2(-60, -180);
            _contentHost = crt;
        }

        public void Show(AreaId id)
        {
            foreach (Transform c in _contentHost) Destroy(c.gameObject);
            var info = AreaCatalog.For(id);
            _header.color = info.theme;

            // 头部标题
            var title = HomeWorld.Text("Title", info.enTitle + "  ·  " + info.cnTitle, 40, FontStyle.Bold, Color.white);
            title.rectTransform.SetParent(_header.rectTransform, false);
            title.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            title.rectTransform.anchoredPosition = new Vector2(60, 0);

            // 狐狸台词（气球）
            SpeechBubble(info.foxLine);

            // 内容预览（各区域不同）
            switch (id)
            {
                case AreaId.Vocabulary: PreviewVocabulary(); break;
                case AreaId.Phonics: PreviewPhonics(); break;
                case AreaId.Speaking: PreviewSpeaking(); break;
                case AreaId.Challenge: PreviewChallenge(); break;
            }

            // 即将开放提示
            var soon = HomeWorld.Text("Soon", "正式课程将在 V0.2 开放  ·  Full lessons coming in V0.2", 22,
                FontStyle.Italic, new Color(1f, 1f, 1f, 0.55f));
            soon.rectTransform.SetParent(_root, false);
            soon.rectTransform.anchorMin = new Vector2(0.5f, 0);
            soon.rectTransform.anchorMax = new Vector2(0.5f, 0);
            soon.rectTransform.anchoredPosition = new Vector2(0, 22);

            gameObject.SetActive(true);
            IsOpen = true;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            IsOpen = false;
        }

        void SpeechBubble(string line)
        {
            var bubble = new GameObject("Bubble", typeof(RectTransform));
            bubble.transform.SetParent(_contentHost, false);
            var bg = bubble.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.95f);
            var rt = bubble.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -10);
            rt.sizeDelta = new Vector2(640, 84);

            var fox = HomeWorld.Text("Fox", "🦊  " + line, 26, FontStyle.Bold, new Color(0.30f, 0.30f, 0.35f));
            fox.rectTransform.SetParent(rt, false);
            fox.rectTransform.anchorMin = Vector2.zero; fox.rectTransform.anchorMax = Vector2.one;
            fox.rectTransform.offsetMin = Vector2.zero; fox.rectTransform.offsetMax = Vector2.zero;
        }

        // ---------------- V0.1 预览内容 ----------------

        void PreviewVocabulary()
        {
            string[] words = { "Apple", "Cat", "Dog" };
            string[] ipas = { "/ˈæp.əl/", "/kæt/", "/dɒɡ/" };
            string[] cn = { "苹果", "猫", "狗" };
            for (int i = 0; i < 3; i++)
            {
                var card = Card(new Vector2(-330 + i * 330, 60), new Vector2(280, 240),
                    new Color(1f, 1f, 1f, 0.95f));
                var w = HomeWorld.Text("W", words[i], 40, FontStyle.Bold, new Color(0.25f, 0.35f, 0.55f));
                w.rectTransform.SetParent(card, false);
                w.rectTransform.anchorMin = new Vector2(0.5f, 1f); w.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                w.rectTransform.pivot = new Vector2(0.5f, 1f);
                w.rectTransform.anchoredPosition = new Vector2(0, -24);

                var p = HomeWorld.Text("P", ipas[i], 26, FontStyle.Normal, new Color(0.55f, 0.35f, 0.60f));
                p.rectTransform.SetParent(card, false);
                p.rectTransform.anchorMin = new Vector2(0.5f, 0.5f); p.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                p.rectTransform.anchoredPosition = new Vector2(0, -10);

                var m = HomeWorld.Text("M", cn[i], 28, FontStyle.Normal, new Color(0.40f, 0.40f, 0.45f));
                m.rectTransform.SetParent(card, false);
                m.rectTransform.anchorMin = new Vector2(0.5f, 0f); m.rectTransform.anchorMax = new Vector2(0.5f, 0f);
                m.rectTransform.pivot = new Vector2(0.5f, 0f);
                m.rectTransform.anchoredPosition = new Vector2(0, 28);
            }
            Caption("第一课将学习：动物、水果、颜色（示例卡片，非最终内容）");
        }

        void PreviewPhonics()
        {
            string[] symbols = { "/æ/", "/e/", "/ɪ/", "/ɒ/", "/ʌ/", "/iː/" };
            for (int i = 0; i < symbols.Length; i++)
            {
                var col = i % 3; var row = i / 3;
                var cell = Card(new Vector2(-210 + col * 210, 90 - row * 130), new Vector2(170, 100),
                    new Color(1f, 1f, 1f, 0.12f));
                var t = HomeWorld.Text("S", symbols[i], 44, FontStyle.Bold, Color.white);
                t.rectTransform.SetParent(cell, false);
                t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
                t.rectTransform.offsetMin = Vector2.zero; t.rectTransform.offsetMax = Vector2.zero;
            }
            Caption("美式音标分组教学：单元音 → 双元音 → 辅音（口型动画与跟读在 V0.3 加入）");
        }

        void PreviewSpeaking()
        {
            string[] lines =
            {
                "🦊  Can I have some milk?",
                "👧  Sure! Here you are.",
                "🦊  Thank you!",
            };
            for (int i = 0; i < lines.Length; i++)
            {
                var bubble = Card(new Vector2(0, 150 - i * 95), new Vector2(640, 76),
                    i % 2 == 0 ? new Color(0.95f, 0.65f, 0.30f) : new Color(1f, 1f, 1f, 0.92f));
                var t = HomeWorld.Text("L", lines[i], 26, FontStyle.Normal,
                    i % 2 == 0 ? Color.white : new Color(0.30f, 0.30f, 0.35f));
                t.rectTransform.SetParent(bubble, false);
                t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
                t.rectTransform.offsetMin = new Vector2(24, 0); t.rectTransform.offsetMax = Vector2.zero;
                t.alignment = TextAnchor.MiddleLeft;
            }
            Caption("情景对话练习：餐厅点餐场景示例（录音跟读将在 V0.4 加入）");
        }

        void PreviewChallenge()
        {
            string[] types = { "看图选词", "听音选词", "单词拼写", "音标辨认", "单词跟读", "句子表达" };
            for (int i = 0; i < types.Length; i++)
            {
                var col = i % 3; var row = i / 3;
                var cell = Card(new Vector2(-210 + col * 210, 90 - row * 130), new Vector2(170, 100),
                    new Color(1f, 1f, 1f, 0.12f));
                var num = HomeWorld.Text("N", (i + 1).ToString(), 30, FontStyle.Bold,
                    new Color(0.98f, 0.75f, 0.15f));
                num.rectTransform.SetParent(cell, false);
                num.rectTransform.anchorMin = new Vector2(0.5f, 0.5f); num.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                num.rectTransform.anchoredPosition = new Vector2(0, 16);
                var t = HomeWorld.Text("T", types[i], 24, FontStyle.Normal, Color.white);
                t.rectTransform.SetParent(cell, false);
                t.rectTransform.anchorMin = new Vector2(0.5f, 0.5f); t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                t.rectTransform.anchoredPosition = new Vector2(0, -22);
            }
            Caption("六种题型随机测验 · 错题本与间隔复习在 V0.6 加入");
        }

        // ---------------- 工具 ----------------

        RectTransform Card(Vector2 pos, Vector2 size, Color bg)
        {
            var go = new GameObject("Card", typeof(RectTransform));
            go.transform.SetParent(_contentHost, false);
            var img = go.AddComponent<Image>();
            img.color = bg;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        void Caption(string text)
        {
            var t = HomeWorld.Text("Caption", text, 22, FontStyle.Normal, new Color(1f, 1f, 1f, 0.65f));
            t.rectTransform.SetParent(_contentHost, false);
            t.rectTransform.anchorMin = new Vector2(0.5f, 0f); t.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            t.rectTransform.pivot = new Vector2(0.5f, 0f);
            t.rectTransform.anchoredPosition = new Vector2(0, 6);
        }
    }
}
