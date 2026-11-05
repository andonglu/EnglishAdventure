using UnityEngine;
using UnityEngine.UI;

namespace EA
{
    /// <summary>
    /// UI 构件工厂：收口 Canvas/Scaler/面板/文本的创建方式。
    /// 业务页面（HudController/AreaPage/QuitConfirm…）只负责布局组合与业务逻辑，
    /// 不直接 new Canvas、CanvasScaler，保证全工程 UI 基础构件一致。
    /// P7 起文本统一改由 EaText 提供（本类的 Text 方法是唯一过渡入口）。
    /// </summary>
    public static class UiFactory
    {
        /// <summary>创建带 Scaler/Raycaster 的 Overlay Canvas，返回其 RectTransform。</summary>
        public static RectTransform CreateCanvas(string name, Transform parent, int sortOrder = 0)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            go.AddComponent<GraphicRaycaster>();
            return go.GetComponent<RectTransform>();
        }

        /// <summary>纯色面板（Image）。</summary>
        public static Image Panel(string name, Transform parent, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = c;
            return img;
        }

        /// <summary>UI 文本（当前直接包装旧 Text，P7 换 EaText 时只改这里）。</summary>
        public static Text Text(string name, Transform parent, string content, int size, FontStyle style, Color color)
        {
            var txt = HomeWorld.Text(name, content, size, style, color);
            txt.rectTransform.SetParent(parent, false);
            return txt;
        }

        public static void Stretch(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.offsetMin = offMin; rt.offsetMax = offMax;
        }

        /// <summary>四角拉伸铺满父级。</summary>
        public static void Fill(RectTransform rt)
        {
            Stretch(rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }
    }
}
