using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EA
{
    /// <summary>
    /// 统一资源加载入口：代码只引用资源 ID，ID → 路径 的映射在
    /// Resources/Settings/asset_manifest.json 中登记。
    /// 带缓存与缺失告警；manifest 缺 ID 时回退为把 ID 当路径用（兼容期）。
    /// </summary>
    public static class ResourceService
    {
        static readonly Dictionary<string, string> IdToPath = new Dictionary<string, string>();
        static readonly Dictionary<string, Object> Cache = new Dictionary<string, Object>();
        static bool _initialized;

        /// <summary>启动时装载清单。重复 ID / 非法条目在控制台列出。</summary>
        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;
            IdToPath.Clear();
            Cache.Clear();

            var ta = Resources.Load<TextAsset>("Settings/asset_manifest");
            if (ta == null)
            {
                Debug.LogWarning("[EA] asset_manifest.json 缺失，资源 ID 将直接按路径解析");
                return;
            }

            var wrapper = JsonUtility.FromJson<ManifestWrapper>(ta.text);
            if (wrapper == null)
            {
                Debug.LogWarning("[EA] asset_manifest.json 解析失败");
                return;
            }

            int count = 0;
            count += RegisterSection("models", wrapper.models);
            count += RegisterSection("audio", wrapper.audio);
            count += RegisterSection("textures", wrapper.textures);
            Debug.Log($"[EA] AssetManifest 已装载 {count} 个资源 ID");

            ReportDuplicateKeys(ta.text);
        }

        static int RegisterSection(string section, List<ManifestEntry> entries)
        {
            if (entries == null) return 0;
            int n = 0;
            foreach (var e in entries)
            {
                if (string.IsNullOrEmpty(e.id) || string.IsNullOrEmpty(e.path))
                {
                    Debug.LogWarning($"[EA] AssetManifest[{section}] 存在缺字段条目: id='{e.id}' path='{e.path}'");
                    continue;
                }
                if (IdToPath.ContainsKey(e.id))
                {
                    Debug.LogWarning($"[EA] AssetManifest 重复 ID: {e.id}（后者被忽略）");
                    continue;
                }
                IdToPath.Add(e.id, e.path);
                n++;
            }
            return n;
        }

        /// <summary>JsonUtility 对重复 key 静默取后者，这里用文本扫描兜底报告。</summary>
        static void ReportDuplicateKeys(string raw)
        {
            var seen = new HashSet<string>();
            foreach (Match m in Regex.Matches(raw, @"\""([a-z0-9_.]+)\""\s*:\s*\""([^\""]*)\"""))
            {
                string key = m.Groups[1].Value;
                if (key == "comment") continue;
                if (key == "id" || key == "path") continue; // 条目字段，非 ID 键
                // 顶层节名
                if (key == "models" || key == "audio" || key == "textures") continue;
                if (!seen.Add(key))
                    Debug.LogWarning($"[EA] AssetManifest 文本级重复 ID: {key}");
            }
        }

        public static bool Has(string id) => IdToPath.ContainsKey(id);

        /// <summary>按 ID（或兼容：直接路径）加载资源，带缓存。缺失时告警并返回 null。</summary>
        public static T Load<T>(string id) where T : Object
        {
            if (string.IsNullOrEmpty(id)) return null;
            string cacheKey = typeof(T).Name + ":" + id;
            if (Cache.TryGetValue(cacheKey, out var hit))
                return hit as T;

            string path = IdToPath.TryGetValue(id, out var p) ? p : id;
            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                Debug.LogWarning($"[EA] 资源缺失: id='{id}' 解析路径='{path}'（类型 {typeof(T).Name}）");
                return null;
            }
            Cache[cacheKey] = asset;
            return asset;
        }

        // JSON 结构：ID → path 的扁平映射（以数组承载以便报告重复/缺字段）
        [System.Serializable]
        class ManifestEntry
        {
            public string id;
            public string path;
        }

        [System.Serializable]
        class ManifestWrapper
        {
            public string comment;
            public List<ManifestEntry> models;
            public List<ManifestEntry> audio;
            public List<ManifestEntry> textures;
        }
    }
}
