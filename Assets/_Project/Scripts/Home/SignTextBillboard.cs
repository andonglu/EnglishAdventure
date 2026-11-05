using UnityEngine;

namespace EA
{
    /// <summary>标牌 3D 文字绕 Y 轴面向相机（+Z 始终指向相机，文字正向可读）。</summary>
    public class SignTextBillboard : MonoBehaviour
    {
        public Vector3 pivot;   // 标牌板中心（世界坐标）
        public float forward = 0.1f;

        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                // Camera.main 依赖 MainCamera 标签；兜底直接找相机组件
                cam = FindFirstObjectByType<Camera>();
                if (cam == null) return;
            }
            var dir = cam.transform.position - pivot;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.001f) return;
            var d = dir.normalized;
            // 本机实测：文字从 -Z 侧读为正（+Z 侧看是镜像），所以 +Z 背对相机
            transform.rotation = Quaternion.LookRotation(-d, Vector3.up);
            transform.position = pivot + d * forward;
        }
    }
}
