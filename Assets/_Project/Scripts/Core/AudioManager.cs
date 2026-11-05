using System.Collections.Generic;
using UnityEngine;

namespace EA
{
    /// <summary>UI 音效播放（Kenney Interface Sounds, CC0）。</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        AudioSource _source;
        AppConfig _cfg;

        public void Init(AppConfig cfg)
        {
            I = this;
            _cfg = cfg;
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            Preload(new[] { "click_001", "click_002", "select_001", "confirmation_001",
                            "back_001", "error_001", "pluck_001", "question_001" });
        }

        void Preload(string[] names)
        {
            foreach (var n in names)
            {
                var c = Resources.Load<AudioClip>("Audio/" + n);
                if (c != null) _clips[n] = c;
                else Debug.LogWarning("[EA] 音效缺失: " + n);
            }
        }

        public void Play(string name)
        {
            if (_source == null) return;
            if (_clips.TryGetValue(name, out var clip) && clip != null)
                _source.PlayOneShot(clip, _cfg != null ? _cfg.uiVolume : 0.9f);
        }

        public void Play(AudioClip clip, float vol = 1f)
        {
            if (_source != null && clip != null) _source.PlayOneShot(clip, vol * (_cfg != null ? _cfg.uiVolume : 0.9f));
        }

        // 常用语义音效
        public void Click() => Play("click_001");
        public void Select() => Play("select_001");
        public void Confirm() => Play("confirmation_001");
        public void Back() => Play("back_001");
        public void Error() => Play("error_001");
        public void Question() => Play("question_001");
        public void FoxChirp() => Play("pluck_001");
    }
}
