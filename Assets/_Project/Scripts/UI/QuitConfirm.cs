using UnityEngine;
using UnityEngine.UI;

namespace EA
{
    /// <summary>主菜单返回键的退出二次确认。</summary>
    public class QuitConfirm : MonoBehaviour
    {
        GameManager _gm;

        public static QuitConfirm Create(GameManager gm)
        {
            var go = new GameObject("QuitConfirm", typeof(RectTransform));
            go.transform.SetParent(gm.transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            go.AddComponent<GraphicRaycaster>();
            var q = go.AddComponent<QuitConfirm>();
            q._gm = gm;
            q.Build(go.GetComponent<RectTransform>());
            go.SetActive(false);
            return q;
        }

        void Build(RectTransform root)
        {
            var mask = gameObject.AddComponent<Image>();
            mask.color = new Color(0f, 0f, 0f, 0.55f);

            var box = new GameObject("Box", typeof(RectTransform));
            box.transform.SetParent(root, false);
            var bg = box.AddComponent<Image>();
            bg.color = Color.white;
            var brt = box.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(460, 280);

            var t = HomeWorld.Text("T", "要退出英语冒险乐园吗？\nQuit English Adventure?", 28, FontStyle.Bold,
                new Color(0.25f, 0.35f, 0.55f));
            t.rectTransform.SetParent(brt, false);
            t.rectTransform.anchorMin = new Vector2(0.5f, 1f); t.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            t.rectTransform.pivot = new Vector2(0.5f, 1f);
            t.rectTransform.anchoredPosition = new Vector2(0, -40);

            UiButton.Create(brt, "再玩一会", 26, new Color(0.42f, 0.78f, 0.45f),
                new Vector2(170, 70), new Vector2(-95, 60), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                _gm.Audio, () => { gameObject.SetActive(false); _gm.Audio.Confirm(); });
            UiButton.Create(brt, "退出", 26, new Color(0.90f, 0.45f, 0.40f),
                new Vector2(170, 70), new Vector2(95, 60), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                _gm.Audio, () => { _gm.Audio.Back(); Application.Quit(); });
        }

        public void Toggle()
        {
            if (gameObject.activeSelf) { gameObject.SetActive(false); _gm.Audio.Back(); }
            else { gameObject.SetActive(true); _gm.Audio.Question(); }
        }
    }
}
