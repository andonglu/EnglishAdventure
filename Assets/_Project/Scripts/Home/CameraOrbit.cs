using UnityEngine;

namespace EA
{
    /// <summary>环绕相机：拖动旋转、双指/滚轮缩放、空闲时缓慢自动环绕。</summary>
    public class CameraOrbit : MonoBehaviour
    {
        AppConfig _cfg;
        float _yaw;
        float _dist;
        float _idleTimer;
        Camera _cam;

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
            _cam = GetComponent<Camera>();
            _dist = cfg.cameraDistance;
            _yaw = 0f;
            transform.position = focus;
            Apply(focus);
        }

        void Update()
        {
            var focus = Vector3.zero;
            float yawDelta = 0f;
            float zoomDelta = 0f;

            // 触摸
            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    yawDelta = -t.deltaPosition.x * _cfg.dragOrbitSpeed;
                    _idleTimer = 0f;
                }
            }
            else if (Input.touchCount == 2)
            {
                var a = Input.GetTouch(0);
                var b = Input.GetTouch(1);
                float prev = (a.position - a.deltaPosition - (b.position - b.deltaPosition)).magnitude;
                float now = (a.position - b.position).magnitude;
                zoomDelta = (prev - now) * _cfg.zoomSpeed * _dist;
                _idleTimer = 0f;
            }
            // 鼠标（编辑器/Windows）
            else if (Input.GetMouseButton(0) && Input.mousePresent)
            {
                yawDelta = -Input.GetAxis("Mouse X") * _cfg.dragOrbitSpeed * 18f;
                _idleTimer = 0f;
            }
            zoomDelta += -Input.mouseScrollDelta.y * 1.2f;

            _yaw += yawDelta;
            _dist = Mathf.Clamp(_dist + zoomDelta, _cfg.cameraMinDistance, _cfg.cameraMaxDistance);

            _idleTimer += Time.deltaTime;
            if (_idleTimer > 4f) _yaw += _cfg.orbitSpeed * Time.deltaTime;

            Apply(focus);
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
