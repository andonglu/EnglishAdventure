using UnityEngine;

namespace EA
{
    /// <summary>
    /// 统一输入入口：全工程唯一允许直接读取 UnityEngine.Input 的组件。
    /// 以事件语义分发——点按 / 拖拽 / 捏合缩放 / 滚轮 / 返回键。
    /// 各系统（相机、页面导航、未来学习交互）只订阅事件，不直接碰 Input。
    /// </summary>
    public class InputRouter : MonoBehaviour
    {
        public static InputRouter I { get; private set; }

        [Tooltip("按下后移动超过该像素距离视为拖拽，否则抬起算点按")]
        public float tapSlopPixels = 10f;

        /// <summary>单指/左键抬起且未构成拖拽。参数：屏幕坐标。</summary>
        public event System.Action<Vector2> OnTap;
        /// <summary>拖拽位移（像素），已含方向语义（与旧相机逻辑一致：右拖为负）。</summary>
        public event System.Action<Vector2> OnDragDelta;
        /// <summary>双指捏合：两指距离的变化量（像素），变大为正。</summary>
        public event System.Action<float> OnPinchDelta;
        /// <summary>滚轮缩放（一格为 ±1 量级，正 = 前滚放大意愿）。</summary>
        public event System.Action<float> OnScroll;
        /// <summary>返回键（编辑器 Escape / Android 返回键）。</summary>
        public event System.Action OnBack;

        Vector2 _pressPos;
        bool _trackingTouch;

        void Awake()
        {
            I = this;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnBack?.Invoke();

            HandleTouch();
            HandleMouse();
        }

        // ---------------- 触摸 ----------------

        void HandleTouch()
        {
            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                switch (t.phase)
                {
                    case TouchPhase.Began:
                        _pressPos = t.position;
                        _trackingTouch = true;
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (_trackingTouch &&
                            (t.position - _pressPos).sqrMagnitude > tapSlopPixels * tapSlopPixels)
                            OnDragDelta?.Invoke(-t.deltaPosition); // 保持旧语义：右拖 → 相机正向转
                        break;
                    case TouchPhase.Ended:
                        if (_trackingTouch &&
                            (t.position - _pressPos).sqrMagnitude <= tapSlopPixels * tapSlopPixels)
                            OnTap?.Invoke(t.position);
                        _trackingTouch = false;
                        break;
                    case TouchPhase.Canceled:
                        _trackingTouch = false;
                        break;
                }
            }
            else if (Input.touchCount == 2)
            {
                var a = Input.GetTouch(0);
                var b = Input.GetTouch(1);
                float prev = ((a.position - a.deltaPosition) - (b.position - b.deltaPosition)).magnitude;
                float now = (a.position - b.position).magnitude;
                OnPinchDelta?.Invoke(prev - now);
                _trackingTouch = false;
            }
            else
            {
                _trackingTouch = false;
            }
        }

        // ---------------- 鼠标（编辑器 / Windows） ----------------

        void HandleMouse()
        {
            if (!Input.mousePresent || Input.touchCount > 0) return;

            float scroll = -Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
                OnScroll?.Invoke(scroll);

            if (Input.GetMouseButtonDown(0))
                _pressPos = Input.mousePosition;

            if (Input.GetMouseButton(0) &&
                ((Vector2)Input.mousePosition - _pressPos).sqrMagnitude > tapSlopPixels * tapSlopPixels)
            {
                var delta = new Vector2(-Input.GetAxis("Mouse X") * 18f, 0f);
                if (delta.sqrMagnitude > 0.0001f)
                    OnDragDelta?.Invoke(delta); // 与旧鼠标语义一致（含 18 倍灵敏度）
            }
        }
    }
}
