using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EA.EditorTools
{
    /// <summary>
    /// Prefab 烘焙（P8）：在编辑模式下用与运行时完全相同的代码搭建世界，
    /// 把狐狸与四大区域建筑群（含标牌、AreaEntrance、点击盒）保存为 Prefab，
    /// 运行时优先加载 Prefab，缺失时回退代码动态生成（V0.1 兼容）。
    ///
    /// 用法：Unity -batchmode -executeMethod EA.EditorTools.EABake.BakePrefabs
    /// 材质会同步资产化到 Assets/Resources/Materials/（Prefab 只引用资产，不内嵌运行时对象）。
    /// </summary>
    public static class EABake
    {
        const string PrefabDir = "Assets/Resources/Prefabs";
        const string MatDir = "Assets/Resources/Materials";

        public static void BakePrefabs()
        {
            FoxCompanion.IsBaking = true;
            try
            {
                BakePrefabsInner();
            }
            finally
            {
                FoxCompanion.IsBaking = false;
            }
        }

        static void BakePrefabsInner()
        {
            // 内容/资源服务在编辑模式可用（Resources 与 StreamingAssets 都是真实文件）
            ResourceService.Init();
            ContentService.Init();
            AreaCatalog.Init();

            // 在临时宿主上构建整个世界（GameManager 仅在播放模式需要，这里传空）
            var host = new GameObject("~BakeHost");
            var world = HomeWorld.Build(null, host.transform);

            // 1) 材质资产化：Prefab 不允许引用非资产材质
            var savedMats = MaterializeAll(world.gameObject);
            Debug.Log($"[EABake] 材质资产化 {savedMats} 个");

            // 2) 保存狐狸
            var fox = world.GetComponentInChildren<FoxCompanion>();
            if (fox != null)
                SavePrefab(fox.gameObject, "Fox");

            // 3) 保存四大区域
            foreach (AreaId id in System.Enum.GetValues(typeof(AreaId)))
            {
                var areaRoot = FindChild(world.transform, "Area_" + id);
                if (areaRoot != null)
                    SavePrefab(areaRoot.gameObject, "Area_" + id);
                else
                    Debug.LogWarning("[EABake] 未找到区域根节点: " + id);
            }

            // 4) 清理临时世界（不保存场景）
            Object.DestroyImmediate(host);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EABake] Prefab 烘焙完成 → " + PrefabDir);
        }

        static Transform FindChild(Transform parent, string name)
        {
            foreach (Transform c in parent)
                if (c.name == name) return c;
            return null;
        }

        static void SavePrefab(GameObject go, string name)
        {
            Directory.CreateDirectory(PrefabDir);
            string path = $"{PrefabDir}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Debug.Log("[EABake] 已保存 Prefab: " + path);
        }

        /// <summary>把世界中所有渲染器的非资产材质替换为 Assets/Resources/Materials/ 下的资产版本。</summary>
        static int MaterializeAll(GameObject root)
        {
            var rends = root.GetComponentsInChildren<Renderer>(true);
            var matToAsset = new Dictionary<Material, Material>();
            int created = 0;

            foreach (var r in rends)
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    if (!matToAsset.TryGetValue(m, out var asset))
                    {
                        asset = MaterializeOne(m);
                        if (asset != null) { matToAsset[m] = asset; created++; }
                        else asset = m;
                    }
                    if (!ReferenceEquals(mats[i], asset)) { mats[i] = asset; changed = true; }
                }
                if (changed) r.sharedMaterials = mats;
            }
            return created;
        }

        static Material MaterializeOne(Material m)
        {
            // 已是资产（如字体材质引用的打包资源）直接保留
            if (AssetDatabase.Contains(m)) return m;

            Directory.CreateDirectory(MatDir);
            // 命名：shader 名 + 颜色/贴图指纹，保证同参数复用、不同参数分开
            string texName = m.mainTexture != null ? m.mainTexture.name : "notex";
            string fingerprint = $"{m.shader.name}_{texName}_{ColorUtility.ToHtmlStringRGBA(m.color)}";
            string path = $"{MatDir}/{fingerprint}.mat";
            var asset = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (asset == null)
            {
                asset = new Material(m);
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                // 已有资产：同步参数（支持烘焙参数调整后的重烘）
                asset.shader = m.shader;
                asset.color = m.color;
                asset.mainTexture = m.mainTexture;
                if (m.HasProperty("_Glossiness")) asset.SetFloat("_Glossiness", m.GetFloat("_Glossiness"));
                EditorUtility.SetDirty(asset);
            }
            return asset;
        }
    }
}
