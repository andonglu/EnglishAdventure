using UnityEngine;
using UnityEngine.EventSystems;

namespace EA
{
    /// <summary>区域入口：点击 → 打开区域页；按下有回弹。进区只记访问统计，不发星星。</summary>
    public class AreaEntrance : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public AreaId area;
        Vector3 _baseScale;
        GameManager _gm;

        void Awake()
        {
            // Prefab 实例的 GameManager 引用不会随包序列化，运行时自解析
            if (_gm == null) _gm = FindFirstObjectByType<GameManager>();
            if (_baseScale == Vector3.zero) _baseScale = transform.localScale;
        }

        public static AreaEntrance Attach(GameManager gm, GameObject visual, AreaId area)
        {
            var a = visual.AddComponent<AreaEntrance>();
            a.area = area;
            a._gm = gm;
            a._baseScale = visual.transform.localScale;
            EnsureClickBox(visual);
            return a;
        }

        /// <summary>确保有覆盖整个区域的可点击盒子（烘焙 Prefab 时也会调用）。</summary>
        public static void EnsureClickBox(GameObject visual)
        {
            if (visual.GetComponent<BoxCollider>() != null) return;
            // 始终加一个覆盖整个区域的大盒子；不能依赖子物体碰撞体
            // （标牌用 Destroy 移除的碰撞体当帧仍能被查到，会导致这里误判跳过）
            //
            // 高度限 2.5m：区域建筑群（树木/塔楼）整体包围盒可达 4m+，
            // 过高的盒子会悬空拦住后方区域的点击射线。
            var bounds = visual.GetComponentsInChildren<Renderer>().Length > 0 ? CalcBounds(visual) : new Bounds(visual.transform.position, Vector3.one * 6f);
            const float maxHeight = 2.5f;
            if (bounds.size.y > maxHeight)
            {
                float bottom = bounds.center.y - bounds.size.y * 0.5f;
                bounds.size = new Vector3(bounds.size.x, maxHeight, bounds.size.z);
                bounds.center = new Vector3(bounds.center.x, bottom + maxHeight * 0.5f, bounds.center.z);
            }
            var box = visual.AddComponent<BoxCollider>();
            box.center = visual.transform.InverseTransformPoint(bounds.center);
            box.size = visual.transform.InverseTransformDirection(bounds.size);
        }

        static Bounds CalcBounds(GameObject go)
        {
            var rends = go.GetComponentsInChildren<Renderer>();
            Bounds b = rends.Length > 0 ? rends[0].bounds : new Bounds(go.transform.position, Vector3.one);
            foreach (var r in rends) b.Encapsulate(r.bounds);
            return b;
        }

        public void OnPointerDown(PointerEventData e)
        {
            StopAllCoroutines();
            StartCoroutine(Bounce(_baseScale * (1f - (_gm != null ? _gm.Config.entranceBounce : 0.12f))));
            if (_gm != null) _gm.Audio.Click();
        }

        /// <summary>点击抬起 → 打开区域页。</summary>
        public void OnPointerUp(PointerEventData e)
        {
            if (_gm == null) _gm = FindFirstObjectByType<GameManager>();
            if (_gm == null) return;
            StopAllCoroutines();
            StartCoroutine(Bounce(_baseScale));
            _gm.OpenArea(area);
        }

        System.Collections.IEnumerator Bounce(Vector3 target)
        {
            var tr = transform;
            Vector3 from = tr.localScale;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 9f;
                tr.localScale = Vector3.Lerp(from, target, Mathf.Clamp01(t));
                yield return null;
            }
        }
    }
}
