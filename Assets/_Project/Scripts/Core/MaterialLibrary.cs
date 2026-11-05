using System.Collections.Generic;
using UnityEngine;

namespace EA
{
    /// <summary>
    /// 运行时材质唯一入口。全工程只有这里允许创建 Lit 材质。
    /// 未来迁移 URP 时只需改 LitShaderName 并把缓存材质资产化。
    /// </summary>
    public static class MaterialLibrary
    {
        // URP 迁移目标："Universal Render Pipeline/Lit"
        const string LitShaderName = "Standard";

        static readonly Dictionary<Color, Material> SolidCache = new Dictionary<Color, Material>();
        static Material _townAtlas, _castleAtlas;

        public static Shader LitShader => Shader.Find(LitShaderName);

        /// <summary>纯色材质（缓存复用）。</summary>
        public static Material Solid(Color c)
        {
            if (SolidCache.TryGetValue(c, out var m) && m != null) return m;
            m = new Material(LitShader) { color = c };
            m.SetFloat("_Glossiness", 0.12f);
            SolidCache[c] = m;
            return m;
        }

        /// <summary>Kenney Town 图集材质。</summary>
        public static Material TownAtlas()
        {
            if (_townAtlas != null) return _townAtlas;
            _townAtlas = Atlas(ResourceService.Load<Texture2D>("colormap.town"));
            return _townAtlas;
        }

        /// <summary>Kenney Castle 图集材质。</summary>
        public static Material CastleAtlas()
        {
            if (_castleAtlas != null) return _castleAtlas;
            _castleAtlas = Atlas(ResourceService.Load<Texture2D>("colormap.castle"));
            return _castleAtlas;
        }

        static Material Atlas(Texture2D tex)
        {
            var m = new Material(LitShader);
            if (tex != null) m.mainTexture = tex;
            m.SetFloat("_Glossiness", 0.1f);
            return m;
        }
    }
}
