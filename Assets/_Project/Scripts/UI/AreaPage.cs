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

        // ---------------- 区域预览（数据来自 StreamingAssets/Content/*.json） ----------------

        void PreviewVocabulary()
        {
            var lesson = ContentService.FirstLessonOf(AreaId.Vocabulary);
            int n = 0;
            if (lesson != null)
            {
                for (int i = 0; i < lesson.wordRefs.Count && n < 3; i++, n++)
                {
                    var w = ContentService.FindWord(lesson.wordRefs[i]);
                    if (w == null) continue;
                    var card = Card(new Vector2(-330 + n * 330, 60), new Vector2(280, 240),
                        new Color(1f, 1f, 1f, 0.95f));

                    var img = HomeWorld.Text("Img", "[图片]", 22, FontStyle.Normal, new Color(0.6f, 0.6f, 0.65f));
                    img.rectTransform.SetParent(card, false);
                    img.rectTransform.anchorMin = new Vector2(0.5f, 1f); img.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    img.rectTransform.pivot = new Vector2(0.5f, 1f);
                    img.rectTransform.anchoredPosition = new Vector2(0, -14);
                    if (!string.IsNullOrEmpty(w.image))
                    {
                        var sprite = LoadMediaSprite(w.image);
                        if (sprite != null)
                        {
                            var iconGo = new GameObject("Icon", typeof(RectTransform));
                            iconGo.transform.SetParent(card, false);
                            var iconRt = iconGo.GetComponent<RectTransform>();
                            iconRt.anchorMin = iconRt.anchorMax = new Vector2(0.5f, 1f);
                            iconRt.pivot = new Vector2(0.5f, 1f);
                            iconRt.anchoredPosition = new Vector2(0, -12);
                            iconRt.sizeDelta = new Vector2(84, 84);
                            var icon = iconGo.AddComponent<Image>();
                            icon.sprite = sprite;
                            Object.Destroy(img.gameObject);
                        }
                    }

                    var wt = HomeWorld.Text("W", w.text, 36, FontStyle.Bold, new Color(0.25f, 0.35f, 0.55f));
                    wt.rectTransform.SetParent(card, false);
                    wt.rectTransform.anchorMin = new Vector2(0.5f, 0.5f); wt.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    wt.rectTransform.anchoredPosition = new Vector2(0, -20);

                    var p = HomeWorld.Text("P", w.ipa, 24, FontStyle.Normal, new Color(0.55f, 0.35f, 0.60f));
                    p.rectTransform.SetParent(card, false);
                    p.rectTransform.anchorMin = new Vector2(0.5f, 0.5f); p.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    p.rectTransform.anchoredPosition = new Vector2(0, -56);

                    var m = HomeWorld.Text("M", w.meaning, 26, FontStyle.Normal, new Color(0.40f, 0.40f, 0.45f));
                    m.rectTransform.SetParent(card, false);
                    m.rectTransform.anchorMin = new Vector2(0.5f, 0f); m.rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    m.rectTransform.pivot = new Vector2(0.5f, 0f);
                    m.rectTransform.anchoredPosition = new Vector2(0, 24);
                }
            }
            Caption(n == 0 ? "课程内容准备中（在 lessons.json / words.json 中添加）"
                          : $"{lesson.title} · 内容来自 Content/*.json，改 JSON 即可换词");
        }

        void PreviewPhonics()
        {
            var lesson = ContentService.FirstLessonOf(AreaId.Phonics);
            int n = 0;
            if (lesson != null)
            {
                for (int i = 0; i < lesson.phonicsRefs.Count && n < 6; i++, n++)
                {
                    var ph = ContentService.FindPhonics(lesson.phonicsRefs[i]);
                    if (ph == null) continue;
                    var col = n % 3; var row = n / 3;
                    var cell = Card(new Vector2(-210 + col * 210, 90 - row * 130), new Vector2(170, 100),
                        new Color(1f, 1f, 1f, 0.12f));
                    var t = HomeWorld.Text("S", ph.symbol, 44, FontStyle.Bold, Color.white);
                    t.rectTransform.SetParent(cell, false);
                    t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
                    t.rectTransform.offsetMin = Vector2.zero; t.rectTransform.offsetMax = Vector2.zero;
                }
            }
            Caption(n == 0 ? "音标课程准备中（在 phonics.json 中添加）" : lesson.title);
        }

        void PreviewSpeaking()
        {
            var lesson = ContentService.FirstLessonOf(AreaId.Speaking);
            int n = 0;
            if (lesson != null)
            {
                for (int i = 0; i < lesson.sentenceRefs.Count && n < 3; i++, n++)
                {
                    var s = ContentService.FindSentence(lesson.sentenceRefs[i]);
                    if (s == null) continue;
                    string emoji = s.speaker == "fox" ? "🦊" : (s.speaker == "girl" ? "👧" : "🙂");
                    var bubble = Card(new Vector2(0, 150 - n * 95), new Vector2(640, 76),
                        n % 2 == 0 ? new Color(0.95f, 0.65f, 0.30f) : new Color(1f, 1f, 1f, 0.92f));
                    var t = HomeWorld.Text("L", $"{emoji}  {s.text}", 26, FontStyle.Normal,
                        n % 2 == 0 ? Color.white : new Color(0.30f, 0.30f, 0.35f));
                    t.rectTransform.SetParent(bubble, false);
                    t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
                    t.rectTransform.offsetMin = new Vector2(24, 0); t.rectTransform.offsetMax = Vector2.zero;
                    t.alignment = TextAnchor.MiddleLeft;
                }
            }
            Caption(n == 0 ? "对话课程准备中（在 sentences.json 中添加）" : lesson.title);
        }

        void PreviewChallenge()
        {
            var quizzes = ContentService.Db.quizzes;
            int n = 0;
            for (int i = 0; i < quizzes.Count && n < 6; i++, n++)
            {
                var col = n % 3; var row = n / 3;
                var cell = Card(new Vector2(-210 + col * 210, 90 - row * 130), new Vector2(170, 100),
                    new Color(1f, 1f, 1f, 0.12f));
                var num = HomeWorld.Text("N", (n + 1).ToString(), 30, FontStyle.Bold,
                    new Color(0.98f, 0.75f, 0.15f));
                num.rectTransform.SetParent(cell, false);
                num.rectTransform.anchorMin = new Vector2(0.5f, 0.5f); num.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                num.rectTransform.anchoredPosition = new Vector2(0, 16);
                var t = HomeWorld.Text("T", quizzes[i].title, 24, FontStyle.Normal, Color.white);
                t.rectTransform.SetParent(cell, false);
                t.rectTransform.anchorMin = new Vector2(0.5f, 0.5f); t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                t.rectTransform.anchoredPosition = new Vector2(0, -22);
            }
            Caption(n == 0 ? "测验准备中（在 quizzes.json 中添加）" : "共 " + quizzes.Count + " 组测验 · 题目来自 quizzes.json");
        }

        /// <summary>从 StreamingAssets 加载图片为 Sprite（教学内容媒体）。</summary>
        static Sprite LoadMediaSprite(string relPath)
        {
            try
            {
                string full = System.IO.Path.Combine(Application.streamingAssetsPath, relPath);
                if (!System.IO.File.Exists(full)) return null;
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(System.IO.File.ReadAllBytes(full));
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[EA] 内容图片加载失败 {relPath}: {e.Message}");
                return null;
            }
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
