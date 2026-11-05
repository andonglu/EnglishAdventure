using UnityEngine;

namespace EA
{
    public enum AreaId { Vocabulary, Phonics, Speaking, Challenge }

    /// <summary>
    /// 四大学习区域目录：标题、台词、主题色、地图方位来自
    /// StreamingAssets/Content/areas.json（经 ContentService 装载）。
    /// JSON 缺失或缺区域时回退到内置默认，保证应用可启动。
    /// </summary>
    public static class AreaCatalog
    {
        public class Info
        {
            public AreaId id;
            public string cnTitle;      // 中文名（给家长）
            public string enTitle;      // 英文名（教学主名）
            public string intro;        // 一句话介绍
            public string foxLine;      // 伙伴台词
            public Color theme;
            public Vector2 mapPos;      // 地图上的方位（米）
        }

        static Info[] _all;

        /// <summary>GameManager 启动时调用（须晚于 ContentService.Init）。</summary>
        public static void Init()
        {
            var list = new System.Collections.Generic.List<Info>();
            foreach (AreaId areaId in System.Enum.GetValues(typeof(AreaId)))
            {
                AreaData data = null;
                foreach (var a in ContentService.Db.areas)
                    if (a.id == areaId.ToString()) { data = a; break; }

                if (data == null)
                {
                    Debug.LogWarning($"[EA] areas.json 缺区域 {areaId}，使用内置默认");
                    data = Builtin(areaId);
                }
                list.Add(new Info
                {
                    id = areaId,
                    cnTitle = string.IsNullOrEmpty(data.cnTitle) ? areaId.ToString() : data.cnTitle,
                    enTitle = string.IsNullOrEmpty(data.enTitle) ? areaId.ToString() : data.enTitle,
                    intro = data.intro ?? "",
                    foxLine = string.IsNullOrEmpty(data.foxLine) ? "Welcome!" : data.foxLine,
                    theme = new Color(data.themeR, data.themeG, data.themeB),
                    mapPos = new Vector2(data.mapX, data.mapZ),
                });
            }
            _all = list.ToArray();
        }

        static AreaData Builtin(AreaId id)
        {
            switch (id)
            {
                case AreaId.Vocabulary:
                    return new AreaData { cnTitle = "单词森林", enTitle = "Vocabulary Forest",
                        intro = "看图识词", foxLine = "Let's learn new words together!",
                        themeR = 0.35f, themeG = 0.75f, themeB = 0.40f, mapX = -9, mapZ = -5 };
                case AreaId.Phonics:
                    return new AreaData { cnTitle = "音标学院", enTitle = "Phonics Academy",
                        intro = "美式音标", foxLine = "Listen and repeat after me!",
                        themeR = 0.45f, themeG = 0.55f, themeB = 0.95f, mapX = 9, mapZ = -5 };
                case AreaId.Speaking:
                    return new AreaData { cnTitle = "口语小镇", enTitle = "Speaking Town",
                        intro = "情景对话", foxLine = "Can I have some milk? You try!",
                        themeR = 0.95f, themeG = 0.65f, themeB = 0.30f, mapX = -9, mapZ = 6 };
                default:
                    return new AreaData { cnTitle = "挑战竞技场", enTitle = "Challenge Arena",
                        intro = "随机测试", foxLine = "Ready? Show me what you know!",
                        themeR = 0.85f, themeG = 0.40f, themeB = 0.55f, mapX = 9, mapZ = 6 };
            }
        }

        public static Info[] All => _all ?? new Info[0];

        public static Info For(AreaId id)
        {
            foreach (var i in All) if (i.id == id) return i;
            Init(); // 兜底：未初始化时先初始化
            foreach (var i in All) if (i.id == id) return i;
            return All.Length > 0 ? All[0] : null;
        }
    }
}
