using UnityEngine;

namespace EA
{
    /// <summary>全局配置（ScriptableObject，家长/开发可调）。</summary>
    public class AppConfig : ScriptableObject
    {
        [Header("伙伴")]
        public string foxName = "Foxy";

        [Header("相机")]
        public float cameraDistance = 16f;
        public float cameraMinDistance = 9f;
        public float cameraMaxDistance = 24f;
        public float cameraPitch = 38f;
        public float orbitSpeed = 0.08f;        // 空闲自动环绕速度（度/秒）
        public float dragOrbitSpeed = 0.25f;    // 拖动灵敏度
        public float zoomSpeed = 0.01f;

        [Header("交互")]
        public float entranceBounce = 0.12f;

        [Header("音频")]
        [Range(0f, 1f)] public float uiVolume = 0.9f;

        public static AppConfig Load()
        {
            var cfg = Resources.Load<AppConfig>("Settings/AppConfig");
            if (cfg == null)
            {
                Debug.LogWarning("[EA] AppConfig 缺失，使用内存默认配置");
                cfg = CreateInstance<AppConfig>();
            }
            return cfg;
        }
    }
}
