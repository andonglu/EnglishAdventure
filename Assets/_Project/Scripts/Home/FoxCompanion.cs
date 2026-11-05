using UnityEngine;

namespace EA
{
    /// <summary>
    /// 程序化卡通狐狸学习伙伴：V0.1 无骨骼动画资产，用代码建模 + 代码待机动画
    /// （呼吸、摆尾、耳朵抖动、眨眼、开心跳）。点击会跳一下并叫一声。
    /// </summary>
    public class FoxCompanion : MonoBehaviour
    {
        Transform _body, _head, _earL, _earR, _tail, _eyeL, _eyeR, _lidL, _lidR;
        float _blinkTimer = 2f;
        float _blinkPhase = -1f;
        float _hop = -1f;
        float _earTimer = 4f;
        float _earPhase = -1f;
        float _wagPhase;
        AudioManager _audio;

        public static FoxCompanion Spawn(Vector3 pos, AudioManager audio)
        {
            var root = new GameObject("FoxCompanion").transform;
            root.position = pos;
            var fox = root.gameObject.AddComponent<FoxCompanion>();
            fox._audio = audio;
            fox.Build();
            var col = root.gameObject.AddComponent<SphereCollider>();
            col.center = new Vector3(0, 0.7f, 0);
            col.radius = 0.75f;
            root.gameObject.AddComponent<FoxTap>();
            return fox;
        }

        /// <summary>点击狐狸 → 开心跳。</summary>
        class FoxTap : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler
        {
            public void OnPointerDown(UnityEngine.EventSystems.PointerEventData e)
                => GetComponent<FoxCompanion>().HappyHop();
        }

        void Build()
        {
            var orange = Mat(new Color(0.95f, 0.55f, 0.20f));
            var cream = Mat(new Color(1.00f, 0.93f, 0.82f));
            var dark = Mat(new Color(0.20f, 0.14f, 0.12f));

            // 身体（圆角方块感：压扁球）
            _body = Part("Body", orange, new Vector3(0, 0.42f, 0), new Vector3(0.72f, 0.62f, 0.95f), PrimitiveType.Sphere);
            // 肚皮
            Part("Belly", cream, new Vector3(0, 0.36f, 0.28f), new Vector3(0.45f, 0.42f, 0.55f), PrimitiveType.Sphere, _body);

            // 头
            _head = Part("Head", orange, new Vector3(0, 0.98f, 0.30f), new Vector3(0.62f, 0.56f, 0.60f), PrimitiveType.Sphere);
            // 吻部 + 鼻子
            Part("Snout", cream, new Vector3(0, -0.06f, 0.42f), new Vector3(0.34f, 0.26f, 0.40f), PrimitiveType.Sphere, _head);
            Part("Nose", dark, new Vector3(0, 0.00f, 0.62f), new Vector3(0.12f, 0.10f, 0.10f), PrimitiveType.Sphere, _head);
            // 眼睛（白底+黑瞳，简化用黑色圆片 + 眼睑）
            _eyeL = Part("EyeL", dark, new Vector3(-0.18f, 0.08f, 0.47f), new Vector3(0.10f, 0.12f, 0.06f), PrimitiveType.Sphere, _head);
            _eyeR = Part("EyeR", dark, new Vector3(0.18f, 0.08f, 0.47f), new Vector3(0.10f, 0.12f, 0.06f), PrimitiveType.Sphere, _head);
            _lidL = Part("LidL", orange, new Vector3(-0.18f, 0.14f, 0.47f), new Vector3(0.12f, 0.02f, 0.07f), PrimitiveType.Sphere, _head);
            _lidR = Part("LidR", orange, new Vector3(0.18f, 0.14f, 0.47f), new Vector3(0.12f, 0.02f, 0.07f), PrimitiveType.Sphere, _head);
            // 耳朵（三角锥）
            _earL = Part("EarL", orange, new Vector3(-0.22f, 0.42f, 0.02f), new Vector3(0.22f, 0.30f, 0.10f), PrimitiveType.Cube, _head);
            _earR = Part("EarR", orange, new Vector3(0.22f, 0.42f, 0.02f), new Vector3(0.22f, 0.30f, 0.10f), PrimitiveType.Cube, _head);

            // 大尾巴（根部橙，尖端白）
            _tail = new GameObject("Tail").transform;
            _tail.SetParent(transform, false);
            _tail.localPosition = new Vector3(0, 0.55f, -0.55f);
            Part("TailBase", orange, new Vector3(0, 0, -0.25f), new Vector3(0.30f, 0.30f, 0.55f), PrimitiveType.Sphere, _tail);
            Part("TailTip", cream, new Vector3(0, 0, -0.62f), new Vector3(0.24f, 0.24f, 0.30f), PrimitiveType.Sphere, _tail);

            // 脚
            Part("FootL", dark, new Vector3(-0.20f, 0.10f, 0.28f), new Vector3(0.16f, 0.12f, 0.24f), PrimitiveType.Sphere);
            Part("FootR", dark, new Vector3(0.20f, 0.10f, 0.28f), new Vector3(0.16f, 0.12f, 0.24f), PrimitiveType.Sphere);
        }

        static Material Mat(Color c) => MaterialLibrary.Solid(c);

        Transform Part(string name, Material mat, Vector3 localPos, Vector3 scale, PrimitiveType prim, Transform parent = null)
        {
            var g = GameObject.CreatePrimitive(prim);
            g.name = name;
            Object.Destroy(g.GetComponent<Collider>());
            g.transform.SetParent(parent != null ? parent : transform, false);
            g.transform.localPosition = localPos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mat;
            return g.transform;
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            float t = Time.time;

            // 呼吸
            float breathe = 1f + Mathf.Sin(t * 2.2f) * 0.02f;
            _body.localScale = new Vector3(0.72f, 0.62f * breathe, 0.95f);

            // 摆尾
            _wagPhase += dt * 3.2f;
            _tail.localRotation = Quaternion.Euler(0, Mathf.Sin(_wagPhase) * 25f, Mathf.Sin(_wagPhase * 0.5f) * 8f);

            // 眨眼
            if (_blinkPhase < 0f)
            {
                _blinkTimer -= dt;
                if (_blinkTimer <= 0f) { _blinkPhase = 0f; _blinkTimer = 2f + Random.value * 2.5f; }
            }
            else
            {
                _blinkPhase += dt * 6f;
                float k = _blinkPhase < 0.5f ? _blinkPhase * 2f : (1f - _blinkPhase) * 2f;
                k = Mathf.Clamp01(k);
                SetLid(k);
                if (_blinkPhase >= 1f) { _blinkPhase = -1f; SetLid(0f); }
            }

            // 耳朵偶尔抖动
            if (_earPhase < 0f)
            {
                _earTimer -= dt;
                if (_earTimer <= 0f) { _earPhase = 0f; _earTimer = 3f + Random.value * 4f; }
            }
            else
            {
                _earPhase += dt * 5f;
                float k = Mathf.Clamp01(1f - _earPhase);
                _earL.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(_earPhase * 20f) * 12f * k);
                if (_earPhase >= 1f) { _earPhase = -1f; _earL.localRotation = Quaternion.identity; }
            }

            // 开心跳
            if (_hop >= 0f)
            {
                _hop += dt * 2.4f;
                float h = _hop < 1f ? Mathf.Sin(_hop * Mathf.PI) * 0.45f : 0f;
                transform.localPosition = new Vector3(transform.localPosition.x, h, transform.localPosition.z);
                if (_hop >= 1f) _hop = -1f;
            }
        }

        void SetLid(float k)
        {
            float y = Mathf.Lerp(0.20f, 0.06f, k);
            _lidL.localScale = new Vector3(0.12f, y, 0.07f);
            _lidR.localScale = new Vector3(0.12f, y, 0.07f);
        }

        public void HappyHop()
        {
            if (_hop >= 0f) return;
            _hop = 0f;
            _audio?.FoxChirp();
        }
    }
}
