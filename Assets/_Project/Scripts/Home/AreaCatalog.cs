using UnityEngine;

namespace EA
{
    public enum AreaId { Vocabulary, Phonics, Speaking, Challenge }

    /// <summary>四大学习区域的静态目录：标题、引导语、主题色、V0.1 预览内容。</summary>
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

        public static readonly Info[] All =
        {
            new Info { id = AreaId.Vocabulary, cnTitle = "单词森林", enTitle = "Vocabulary Forest",
                intro = "看图识词 · 听音辨词 · 拼写练习",
                foxLine = "Let's learn new words together!",
                theme = new Color(0.35f, 0.75f, 0.40f), mapPos = new Vector2(-9f, -5f) },
            new Info { id = AreaId.Phonics, cnTitle = "音标学院", enTitle = "Phonics Academy",
                intro = "美式音标 · 口型示范 · 自然拼读",
                foxLine = "Listen and repeat after me!",
                theme = new Color(0.45f, 0.55f, 0.95f), mapPos = new Vector2(9f, -5f) },
            new Info { id = AreaId.Speaking, cnTitle = "口语小镇", enTitle = "Speaking Town",
                intro = "情景对话 · 跟读模仿 · 角色扮演",
                foxLine = "Can I have some milk? You try!",
                theme = new Color(0.95f, 0.65f, 0.30f), mapPos = new Vector2(-9f, 6f) },
            new Info { id = AreaId.Challenge, cnTitle = "挑战竞技场", enTitle = "Challenge Arena",
                intro = "随机测试 · 错题本 · 间隔复习",
                foxLine = "Ready? Show me what you know!",
                theme = new Color(0.85f, 0.40f, 0.55f), mapPos = new Vector2(9f, 6f) },
        };

        public static Info For(AreaId id)
        {
            foreach (var i in All) if (i.id == id) return i;
            return All[0];
        }
    }
}
