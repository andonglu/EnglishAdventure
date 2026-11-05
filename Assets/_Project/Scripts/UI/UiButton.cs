using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EA
{
    /// <summary>通用按钮：圆角色块 + 音效 + 按压回弹。</summary>
    public class UiButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Text label;
        Image _img;
        Color _base;
        Vector3 _scale;
        UnityAction _onClick;
        AudioManager _audio;

        public static UiButton Create(RectTransform parent, string text, int fontSize, Color bg,
            Vector2 size, Vector2 pos, Vector2 aMin, Vector2 aMax, AudioManager audio, UnityAction onClick)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.pivot = new Vector2(aMax.x == 1f ? 1f : 0.5f, aMax.y == 1f ? 1f : 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = bg;

            var label = HomeWorld.Text("Label", text, fontSize, FontStyle.Bold, Color.white);
            label.rectTransform.SetParent(rt, false);
            var lrt = label.rectTransform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

            var btn = go.AddComponent<UiButton>();
            btn._img = img;
            btn._base = bg;
            btn._scale = Vector3.one;
            btn._onClick = onClick;
            btn._audio = audio;
            btn.label = label;
            return btn;
        }

        public void OnPointerDown(PointerEventData e)
        {
            transform.localScale = _scale * 0.92f;
            _img.color = Color.Lerp(_base, Color.black, 0.12f);
            _audio?.Click();
        }

        public void OnPointerUp(PointerEventData e)
        {
            transform.localScale = _scale;
            _img.color = _base;
            _onClick?.Invoke();
        }
    }
}
