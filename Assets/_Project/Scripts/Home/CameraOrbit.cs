using UnityEngine;

namespace EA
{
    /// <summary>环绕相机：拖拽旋转、捏合/滚轮缩放、空闲时缓慢自动环绕。
    /// 输入全部来自 InputRouter 事件，不直接读取 Input。</summary>
    public class CameraOrbit : MonoBehaviour
    {
        AppConfig _cfg;
        float _yaw;
        float _dist;
        float _idleTimer;
        InputRouter _input;

        public static CameraOrbit Create(AppConfig cfg, Vector3 focus)
        {
            var go = new GameObject("Main Camera");
            var cam = go.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.60f, 0.82f, 0.95f); // 天空蓝
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 200f;
            var orbit = go.AddComponent<CameraOrbit>();
            orbit.Init(cfg, focus);
            return orbit;
        }

        void Init(AppConfig cfg, Vector3 focus)
        {
            _cfg = cfg;
            _dist = cfg.cameraDistance;
            _yaw = 0f;
            transform.position = focus;

            _input = InputRouter.I;
            if (_input != null)
            {
                _input.OnDragDelta += HandleDrag;
                _input.OnPinchDelta += HandlePinch;
                _input.OnScroll += HandleScroll;
            }
            else
            {
                Debug.LogWarning("[EA] InputRouter 缺失，相机将无法响应输入");
            }

            Apply(focus);
        }

        void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnDragDelta -= HandleDrag;
                _input.OnPinchDelta -= HandlePinch;
                _input.OnScroll -= HandleScroll;
            }
        }

        void HandleDrag(Vector2 delta)
        {
            _yaw += delta.x * _cfg.dragOrbitSpeed;
            _idleTimer = 0f;
        }

        void HandlePinch(float pinchDelta)
        {
            _dist = Mathf.Clamp(_dist + pinchDelta * _cfg.zoomSpeed * _dist,
                                _cfg.cameraMinDistance, _cfg.cameraMaxDistance);
            _idleTimer = 0f;
        }

        void HandleScroll(float scroll)
        {
            _dist = Mathf.Clamp(_dist + scroll * 1.2f,
                                _cfg.cameraMinDistance, _cfg.cameraMaxDistance);
            _idleTimer = 0f;
        }

        void Update()
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer > 4f) _yaw += _cfg.orbitSpeed * Time.deltaTime;
            Apply(Vector3.zero);
        }

        void Apply(Vector3 focus)
        {
            float pitch = _cfg.cameraPitch * Mathf.Deg2Rad;
            float yaw = _yaw * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Sin(yaw) * Mathf.Cos(pitch),
                Mathf.Sin(pitch),
                -Mathf.Cos(yaw) * Mathf.Cos(pitch)) * _dist;
            transform.position = focus + offset;
            transform.rotation = Quaternion.LookRotation(focus + Vector3.up * 1.2f - transform.position);
        }
    }
}
