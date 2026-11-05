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
            Preload(new[] { "ui.click", "ui.click_alt", "ui.select", "ui.confirm",
                            "ui.back", "ui.error", "ui.fox_chirp", "ui.question" });
        }

        void Preload(string[] ids)
        {
            foreach (var id in ids)
            {
                var c = ResourceService.Load<AudioClip>(id);
                if (c != null) _clips[id] = c;
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
        public void Click() => Play("ui.click");
        public void Select() => Play("ui.select");
        public void Confirm() => Play("ui.confirm");
        public void Back() => Play("ui.back");
        public void Error() => Play("ui.error");
        public void Question() => Play("ui.question");
        public void FoxChirp() => Play("ui.fox_chirp");
    }
}
