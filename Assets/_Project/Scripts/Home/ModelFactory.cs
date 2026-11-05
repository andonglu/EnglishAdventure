using System.Collections.Generic;
using UnityEngine;

namespace EA
{
    /// <summary>
    /// Kenney 模型加载与材质修正：
    ///  - Town/Castle 系模型使用 colormap.png 图集贴图；
    ///  - Nature 系模型使用 FBX 材质名 → 颜色表（兜底：导入若丢色，按名字补色）。
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

        static Texture2D _townMap, _castleMap;
        static Material _townMat, _castleMat;

        /// <summary>实例化模型并归一化到目标高度（世界单位）。</summary>
        public static GameObject Spawn(string resourcesPath, float targetHeight, Vector3 pos, float yRot = 0f)
        {
            var prefab = Resources.Load<GameObject>(resourcesPath);
            if (prefab == null)
            {
                Debug.LogWarning("[EA] 模型缺失: " + resourcesPath);
                return null;
            }
            var go = Object.Instantiate(prefab, pos, Quaternion.Euler(0, yRot, 0));
            NormalizeAndFix(go, resourcesPath, targetHeight);
            return go;
        }

        static void NormalizeAndFix(GameObject go, string resourcesPath, float targetHeight)
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
            bool isTown = resourcesPath.Contains("/Town/");
            bool isCastle = resourcesPath.Contains("/Castle/");
            foreach (var r in rends)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    if (isTown) { mats[i] = TownMat(); }
                    else if (isCastle) { mats[i] = CastleMat(); }
                    else
                    {
                        // Nature：导入丢色时按名字补色
                        if (IsUnlitWhite(m.color) && NatureColors.TryGetValue(m.name, out var c))
                            mats[i] = Solid(c);
                    }
                }
                r.sharedMaterials = mats;
            }
        }

        static bool IsUnlitWhite(Color c) => c.r > 0.95f && c.g > 0.95f && c.b > 0.95f;

        static readonly Dictionary<Color, Material> _solidCache = new Dictionary<Color, Material>();
        static Material Solid(Color c)
        {
            if (_solidCache.TryGetValue(c, out var m) && m != null) return m;
            m = new Material(Shader.Find("Standard")) { color = c };
            m.SetFloat("_Glossiness", 0.1f);
            _solidCache[c] = m;
            return m;
        }

        static Material TownMat()
        {
            if (_townMat != null) return _townMat;
            _townMap = Resources.Load<Texture2D>("Textures/colormap_town");
            _townMat = AtlasMat(_townMap);
            return _townMat;
        }

        static Material CastleMat()
        {
            if (_castleMat != null) return _castleMat;
            _castleMap = Resources.Load<Texture2D>("Textures/colormap_castle");
            _castleMat = AtlasMat(_castleMap);
            return _castleMat;
        }

        static Material AtlasMat(Texture2D tex)
        {
            var m = new Material(Shader.Find("Standard"));
            if (tex != null) m.mainTexture = tex;
            m.SetFloat("_Glossiness", 0.1f);
            return m;
        }
    }
}
