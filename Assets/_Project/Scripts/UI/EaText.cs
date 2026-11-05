using UnityEngine;
using UnityEngine.UI;

namespace EA
{
    /// <summary>
    /// 统一文本入口（TextMeshPro 过渡方案）。
    /// 全工程所有 UI/标牌文本都应通过 EaText.Create 创建；
    /// 未来迁移 TMP 时只需改本类的实现，调用方零改动。
    ///
    /// 职责：
    /// 1. 统一使用打包的 CJK 字体（Resources/Fonts/NotoSansSC，思源黑体 OFL 许可），
    ///    解决真机上中文/音标（如 /æ/）豆腐块问题；
    /// 2. Emoji 回退映射——打包字体不含彩色 emoji，统一替换为字体内存在的符号。
    /// </summary>
    public static class EaText
    {
        const string FontResourcePath = "Fonts/NotoSansSC";
        static Font _font;

        /// <summary>打包的 CJK 字体（未找到时回退 Unity 内置字体）。</summary>
        public static Font Font
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.Load<Font>(FontResourcePath);
                    if (_font == null)
                    {
                        Debug.LogWarning("[EA] CJK 字体缺失，回退 LegacyRuntime: " + FontResourcePath);
                        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    }
                }
                return _font;
            }
        }

        /// <summary>彩色 emoji → 打包字体支持的符号。新增 emoji 时在这里补映射。</summary>
        static readonly (string from, string to)[] EmojiFallback =
        {
            ("🦊", "狐"),
            ("👧", "☺"),
            ("🙂", "☺"),
            ("🔥", "☀"),   // 连续天数：太阳比爱心更贴合“天数”
            ("⚙",  "☰"),   // 设置：菜单符
        };

        /// <summary>替换字符串中打包字体不支持的彩色 emoji。</summary>
        public static string Sanitize(string content)
        {
            if (string.IsNullOrEmpty(content)) return content ?? "";
            bool hit = false;
            foreach (var (from, _) in EmojiFallback)
                if (content.Contains(from)) { hit = true; break; }
            if (!hit) return content;
            foreach (var (from, to) in EmojiFallback)
                content = content.Replace(from, to);
            return content;
        }

        /// <summary>创建文本（创建时一次性做 emoji 替换；运行时改 .text 需自行保证不含 emoji）。</summary>
        public static Text Create(string name, string content, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var txt = go.AddComponent<Text>();
            txt.font = Font;
            txt.text = Sanitize(content);
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        /// <summary>创建文本并挂到父级。</summary>
        public static Text Create(string name, Transform parent, string content, int size, FontStyle style, Color color)
        {
            var txt = Create(name, content, size, style, color);
            txt.rectTransform.SetParent(parent, false);
            return txt;
        }
    }
}
