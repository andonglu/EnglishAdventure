using System.Collections.Generic;
using UnityEngine;

namespace EA
{
    /// <summary>
    /// Kenney 模型实例化与材质修正：
    ///  - 模型经 ResourceService 按 ID 加载；
    ///  - Town/Castle 系模型使用 colormap 图集（MaterialLibrary）；
    ///  - Nature 系模型按材质名补色（导入丢色兜底）。
    /// </summary>
    public static class ModelFactory
    {
        static readonly Dictionary<string, Color> NatureColors = new Dictionary<string, Color>
        {
            ["woodBark"] = new Color(0.62f, 0.40f, 0.27f),
            ["wood"] = new Color(0.66f, 0.45f, 0.31f),
            ["leafsGreen"] = new Color(0.35f, 0.72f, 0.42f),
            ["leafsFall"] = new Color(0.90f, 0.55f, 0.25f),
            ["grass"] = new Color(0.42f, 0.78f, 0.40f),
            ["rock"] = new Color(0.60f, 0.62f, 0.66f),
            ["dirt"] = new Color(0.55f, 0.40f, 0.28f),
        };

        /// <summary>按资源 ID 实例化模型并归一化到目标高度（世界单位）。</summary>
        public static GameObject Spawn(string id, float targetHeight, Vector3 pos, float yRot = 0f)
        {
            var prefab = ResourceService.Load<GameObject>(id);
            if (prefab == null) return null; // ResourceService 已告警
            var go = Object.Instantiate(prefab, pos, Quaternion.Euler(0, yRot, 0));
            NormalizeAndFix(go, id, targetHeight);
            return go;
        }

        static void NormalizeAndFix(GameObject go, string id, float targetHeight)
        {
            // 统一包大小
            var rends = go.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return;
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            float h = Mathf.Max(0.001f, b.size.y);
            float scale = targetHeight / h;
            go.transform.localScale = Vector3.one * scale;

            // 材质修正
            bool isTown = id.StartsWith("town.");
            bool isCastle = id.StartsWith("castle.");
            foreach (var r in rends)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    if (isTown) { mats[i] = MaterialLibrary.TownAtlas(); }
                    else if (isCastle) { mats[i] = MaterialLibrary.CastleAtlas(); }
                    else
                    {
                        // Nature：导入丢色时按名字补色
                        if (IsUnlitWhite(m.color) && NatureColors.TryGetValue(m.name, out var c))
                            mats[i] = MaterialLibrary.Solid(c);
                    }
                }
                r.sharedMaterials = mats;
            }
        }

        static bool IsUnlitWhite(Color c) => c.r > 0.95f && c.g > 0.95f && c.b > 0.95f;
    }
}
