using UnityEngine;

namespace EA
{
    /// <summary>主世界搭建：地面、四大区域建筑群、装饰散布、狐狸伙伴。</summary>
    public class HomeWorld : MonoBehaviour
    {
        GameManager _gm;

        public static HomeWorld Build(GameManager gm, Transform parent)
        {
            var go = new GameObject("HomeWorld");
            go.transform.SetParent(parent, false);
            var w = go.AddComponent<HomeWorld>();
            w._gm = gm;
            w.Setup();
            return w;
        }

        void Setup()
        {
            // ---- 地面 ----
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.localScale = new Vector3(42f, 0.5f, 42f);
            var gm = ground.GetComponent<Renderer>();
            gm.sharedMaterial = Solid(new Color(0.52f, 0.80f, 0.45f)); // 草地绿

            // ---- 中央广场（浅土色圆盘 + 圆环小路）----
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "Plaza";
            plaza.transform.SetParent(transform, false);
            plaza.transform.localPosition = new Vector3(0, 0.26f, 0);
            plaza.transform.localScale = new Vector3(7.5f, 0.5f, 7.5f);
            plaza.GetComponent<Renderer>().sharedMaterial = Solid(new Color(0.88f, 0.80f, 0.60f));
            Object.Destroy(plaza.GetComponent<CapsuleCollider>());

            // ---- 四大区域 ----
            BuildVocabularyForest();
            BuildPhonicsAcademy();
            BuildSpeakingTown();
            BuildChallengeArena();

            // ---- 散布装饰（树、石、蘑菇）----
            ScatterNature();

            // ---- 狐狸伙伴站在广场中央 ----
            var fox = FoxCompanion.Spawn(new Vector3(0, 0.5f, 0), _gm.Audio);
            fox.transform.SetParent(transform, true);
        }

        // ---------------- 区域建筑 ----------------

        void BuildVocabularyForest()
        {
            var p = Pos(AreaId.Vocabulary);
            var root = Group("Area_Vocabulary", p);
            // 森林：一圈树 + 原木拱门入口
            string[] trees = { "tree_default", "tree_blocks", "tree_detailed", "tree_cone", "tree_blocks_fall" };
            for (int i = 0; i < 10; i++)
            {
                float ang = i * 36f * Mathf.Deg2Rad;
                float r = 3.6f + (i % 3) * 0.5f;
                var tp = p + new Vector3(Mathf.Cos(ang) * r, 0, Mathf.Sin(ang) * r);
                ModelFactory.Spawn("Models/Nature/" + trees[i % trees.Length], 2.6f + (i % 3) * 0.5f, tp, i * 47f)
                    ?.transform.SetParent(root, true);
            }
            // 拱门：两根原木 + 横梁 + 蘑菇点缀
            var l1 = ModelFactory.Spawn("Models/Nature/log", 1.4f, p + new Vector3(-1.2f, 0, 3.4f));
            var l2 = ModelFactory.Spawn("Models/Nature/log", 1.4f, p + new Vector3(1.2f, 0, 3.4f));
            var beam = ModelFactory.Spawn("Models/Nature/log_large", 1.0f, p + new Vector3(0, 2.2f, 3.4f));
            if (beam != null) beam.transform.rotation = Quaternion.Euler(0, 0, 90f);
            foreach (var o in new[] { l1, l2, beam }) o?.transform.SetParent(root, true);
            AddSign(root, p + new Vector3(0, 0.05f, 4.6f), AreaId.Vocabulary, 1f);
            AreaEntrance.Attach(_gm, root.gameObject, AreaId.Vocabulary);
        }

        void BuildPhonicsAcademy()
        {
            var p = Pos(AreaId.Phonics);
            var root = Group("Area_Phonics", p);
            // 学院：城堡塔楼（基座+双中段+尖顶）
            var t1 = ModelFactory.Spawn("Models/Castle/tower-square-base", 1.0f, p);
            var t2 = ModelFactory.Spawn("Models/Castle/tower-square-mid", 1.0f, p + new Vector3(0, 1.0f, 0));
            var t3 = ModelFactory.Spawn("Models/Castle/tower-square-mid", 1.0f, p + new Vector3(0, 2.0f, 0));
            var t4 = ModelFactory.Spawn("Models/Castle/tower-square-roof", 2.0f, p + new Vector3(0, 3.0f, 0));
            foreach (var o in new[] { t1, t2, t3, t4 }) o?.transform.SetParent(root, true);
            // 旗帜
            var flag = ModelFactory.Spawn("Models/Castle/flag", 1.2f, p + new Vector3(0, 5.1f, 0));
            flag?.transform.SetParent(root, true);
            // 台阶 + 树
            var st = ModelFactory.Spawn("Models/Castle/stairs-stone", 0.6f, p + new Vector3(0, 0, 1.1f));
            st?.transform.SetParent(root, true);
            var tr = ModelFactory.Spawn("Models/Town/tree-large", 2.2f, p + new Vector3(-2.4f, 0, 1.6f));
            tr?.transform.SetParent(root, true);
            AddSign(root, p + new Vector3(0, 0.05f, 2.6f), AreaId.Phonics, 1f);
            AreaEntrance.Attach(_gm, root.gameObject, AreaId.Phonics);
        }

        void BuildSpeakingTown()
        {
            var p = Pos(AreaId.Speaking);
            var root = Group("Area_Speaking", p);
            var h1 = ModelFactory.Spawn("Models/Town/building-type-a", 2.6f, p + new Vector3(-1.8f, 0, -1.2f), 15f);
            var h2 = ModelFactory.Spawn("Models/Town/building-type-b", 2.4f, p + new Vector3(1.9f, 0, -1.5f), -20f);
            var h3 = ModelFactory.Spawn("Models/Town/building-type-c", 2.2f, p + new Vector3(0.2f, 0, 1.6f), 180f);
            foreach (var o in new[] { h1, h2, h3 }) o?.transform.SetParent(root, true);
            var t1 = ModelFactory.Spawn("Models/Town/tree-small", 1.4f, p + new Vector3(-3.4f, 0, 1.4f));
            var t2 = ModelFactory.Spawn("Models/Town/tree-small", 1.4f, p + new Vector3(3.4f, 0, 0.8f));
            foreach (var o in new[] { t1, t2 }) o?.transform.SetParent(root, true);
            AddSign(root, p + new Vector3(0, 0.05f, 3.4f), AreaId.Speaking, 1f);
            AreaEntrance.Attach(_gm, root.gameObject, AreaId.Speaking);
        }

        void BuildChallengeArena()
        {
            var p = Pos(AreaId.Challenge);
            var root = Group("Area_Challenge", p);
            // 竞技场：门楼 + 两侧塔 + 旗帜 + 石阶广场
            var gate = ModelFactory.Spawn("Models/Castle/gate", 3.0f, p);
            gate?.transform.SetParent(root, true);
            var twL = ModelFactory.Spawn("Models/Castle/tower-hexagon-base", 1.0f, p + new Vector3(-2.0f, 0, 0));
            var twL2 = ModelFactory.Spawn("Models/Castle/tower-hexagon-mid", 1.0f, p + new Vector3(-2.0f, 1.0f, 0));
            var twL3 = ModelFactory.Spawn("Models/Castle/tower-hexagon-roof", 1.6f, p + new Vector3(-2.0f, 2.0f, 0));
            var twR = ModelFactory.Spawn("Models/Castle/tower-hexagon-base", 1.0f, p + new Vector3(2.0f, 0, 0));
            var twR2 = ModelFactory.Spawn("Models/Castle/tower-hexagon-mid", 1.0f, p + new Vector3(2.0f, 1.0f, 0));
            var twR3 = ModelFactory.Spawn("Models/Castle/tower-hexagon-roof", 1.6f, p + new Vector3(2.0f, 2.0f, 0));
            foreach (var o in new[] { twL, twL2, twL3, twR, twR2, twR3 }) o?.transform.SetParent(root, true);
            var f1 = ModelFactory.Spawn("Models/Castle/flag", 1.0f, p + new Vector3(-2.0f, 3.7f, 0));
            var f2 = ModelFactory.Spawn("Models/Castle/flag", 1.0f, p + new Vector3(2.0f, 3.7f, 0));
            foreach (var o in new[] { f1, f2 }) o?.transform.SetParent(root, true);
            AddSign(root, p + new Vector3(0, 0.05f, 2.2f), AreaId.Challenge, 1f);
            AreaEntrance.Attach(_gm, root.gameObject, AreaId.Challenge);
        }

        // ---------------- 装饰 ----------------

        void ScatterNature()
        {
            string[] deco = { "tree_cone", "rock_largeA", "rock_smallA", "plant_bush", "grass_large", "mushroom_red", "mushroom_tan", "plant_bushLarge" };
            var rnd = new System.Random(20261105);
            for (int i = 0; i < 26; i++)
            {
                float ang = (float)(rnd.NextDouble() * Mathf.PI * 2);
                float r = 16f + (float)rnd.NextDouble() * 4.5f;
                var pos = new Vector3(Mathf.Cos(ang) * r, 0, Mathf.Sin(ang) * r);
                var m = deco[rnd.Next(deco.Length)];
                float h = m.StartsWith("tree") ? 2.4f : (m.StartsWith("rock") ? 0.8f : 0.5f);
                ModelFactory.Spawn("Models/Nature/" + m, h, pos, (float)rnd.NextDouble() * 360f)
                    ?.transform.SetParent(transform, true);
            }
            // 边缘几棵大树
            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f * Mathf.Deg2Rad + 0.3f;
                var pos = new Vector3(Mathf.Cos(ang) * 19.5f, 0, Mathf.Sin(ang) * 19.5f);
                ModelFactory.Spawn("Models/Nature/tree_default", 3.4f, pos, i * 33f)
                    ?.transform.SetParent(transform, true);
            }
        }

        // ---------------- 工具 ----------------

        Vector3 Pos(AreaId id) => new Vector3(AreaCatalog.For(id).mapPos.x, 0.25f, AreaCatalog.For(id).mapPos.y);

        Transform Group(string name, Vector3 pos)
        {
            var g = new GameObject(name).transform;
            g.SetParent(transform, false);
            g.position = pos;
            return g;
        }

        /// <summary>区域标牌：圆角色块 + 中英标题（UGUI Text 朝相机）。</summary>
        void AddSign(Transform parent, Vector3 pos, AreaId id, float scale)
        {
            var info = AreaCatalog.For(id);
            var root = new GameObject("Sign_" + id).transform;
            root.SetParent(parent, false);
            root.position = pos;

            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(board.GetComponent<Collider>());
            board.name = "Board";
            board.transform.SetParent(root, false);
            board.transform.localPosition = new Vector3(0, 1.1f, 0);
            board.transform.localScale = new Vector3(2.4f, 1.2f, 0.12f);
            board.GetComponent<Renderer>().sharedMaterial = Solid(info.theme);

            // 双面 3D 文字：外侧正向可读；板子是不透明物体，会挡住背面那层
            var outward = root.position; outward.y = 0;
            outward = outward.sqrMagnitude > 0.01f ? outward.normalized : Vector3.forward;
            var boardCenter = root.position + new Vector3(0, 1.1f, 0);
            var textGo = AddSignText(root, boardCenter + outward * 0.1f,
                Quaternion.LookRotation(outward, Vector3.up), info);
            var bb = textGo.AddComponent<SignTextBillboard>();
            bb.pivot = boardCenter;

            // 立杆
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(pole.GetComponent<Collider>());
            pole.transform.SetParent(root, false);
            pole.transform.localPosition = new Vector3(0, 0.35f, 0);
            pole.transform.localScale = new Vector3(0.12f, 0.7f, 0.12f);
            pole.GetComponent<Renderer>().sharedMaterial = Solid(new Color(0.45f, 0.32f, 0.22f));
        }

        GameObject AddSignText(Transform parent, Vector3 worldPos, Quaternion rot, AreaCatalog.Info info)
        {
            var go = new GameObject("SignText");
            go.transform.SetParent(parent, true);
            go.transform.SetPositionAndRotation(worldPos, rot);
            var tm = go.AddComponent<TextMesh>();
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.text = info.enTitle + "\n" + info.cnTitle;
            tm.fontSize = 48;
            tm.characterSize = 0.03f;
            tm.lineSpacing = 0.85f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = Color.white;
            go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
            return go;
        }

        public static UnityEngine.UI.Text Text(string name, string content, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var txt = go.AddComponent<UnityEngine.UI.Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = content;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        static readonly System.Collections.Generic.Dictionary<Color, Material> _mats = new System.Collections.Generic.Dictionary<Color, Material>();
        static Material Solid(Color c)
        {
            if (_mats.TryGetValue(c, out var m) && m != null) return m;
            m = new Material(Shader.Find("Standard")) { color = c };
            m.SetFloat("_Glossiness", 0.12f);
            _mats[c] = m;
            return m;
        }
    }
}
