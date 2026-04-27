using UnityEngine;
using WildernessCultivation.Core;
using WildernessCultivation.Cultivation;

namespace WildernessCultivation.Audio
{
    /// <summary>
    /// Audio core cho MVP. Singleton trên <c>GameManager</c> GameObject.
    /// Vì repo chưa có asset AudioClip thật, MVP tự <b>procedurally generate</b>
    /// tone ngắn (sine + envelope) làm placeholder. Artists có thể thay sau bằng
    /// cách set field <see cref="sfxOverrides"/> / <see cref="musicOverride"/>.
    ///
    /// Volume master/music/sfx lưu trong <see cref="PlayerPrefs"/> giữa các session.
    ///
    /// Dùng:
    ///   AudioManager.Instance?.PlaySfx(SfxKind.MeleeSwing);
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public enum SfxKind
        {
            MeleeSwing = 0,
            SkillCast = 1,
            BreakthroughSuccess = 2,
            BreakthroughFail = 3,
            MeditationStart = 4,
            ItemPickup = 5,
            UIClick = 6,
        }

        [Header("Volumes (0..1, lưu PlayerPrefs)")]
        [Range(0f, 1f)] public float masterVolume = 0.8f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        [Header("Optional overrides (artist-provided clips)")]
        public AudioClip[] sfxOverrides;   // index theo SfxKind
        public AudioClip musicOverride;

        AudioSource sfxSource;
        AudioSource musicSource;
        AudioClip[] procSfx;

        const string KeyMaster = "audio.master";
        const string KeyMusic = "audio.music";
        const string KeySfx = "audio.sfx";

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            masterVolume = PlayerPrefs.GetFloat(KeyMaster, masterVolume);
            musicVolume = PlayerPrefs.GetFloat(KeyMusic, musicVolume);
            sfxVolume = PlayerPrefs.GetFloat(KeySfx, sfxVolume);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D UI-style

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;

            BuildProceduralSfx();
            ApplyVolumes();
        }

        RealmSystem subscribedRealm;
        GameManager subscribedGm;
        bool wasPaused;

        [Header("Pause behavior")]
        [Tooltip("Khi game pause, tự Pause music (SFX vẫn chơi để UI click nghe được).")]
        public bool pauseMusicOnGamePause = true;

        void Start()
        {
            // Subscribe breakthrough SFX qua event có sẵn của RealmSystem.
            subscribedRealm = FindObjectOfType<RealmSystem>();
            if (subscribedRealm != null)
                subscribedRealm.OnBreakthroughAttempted += OnBreakthroughAttempted;

            subscribedGm = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
        }

        void Update()
        {
            if (!pauseMusicOnGamePause || musicSource == null || subscribedGm == null) return;
            if (subscribedGm.isPaused != wasPaused)
            {
                wasPaused = subscribedGm.isPaused;
                if (wasPaused)
                {
                    if (musicSource.isPlaying) musicSource.Pause();
                }
                else
                {
                    // Chỉ UnPause nếu clip đã set (tránh Play clip null).
                    if (musicSource.clip != null) musicSource.UnPause();
                }
            }
        }

        void OnDestroy()
        {
            if (subscribedRealm != null)
                subscribedRealm.OnBreakthroughAttempted -= OnBreakthroughAttempted;
            if (Instance == this) Instance = null;
        }

        void OnBreakthroughAttempted(bool success)
        {
            PlaySfx(success ? SfxKind.BreakthroughSuccess : SfxKind.BreakthroughFail);
        }

        public void SetMaster(float v) { masterVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(KeyMaster, masterVolume); ApplyVolumes(); }
        public void SetMusic(float v) { musicVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(KeyMusic, musicVolume); ApplyVolumes(); }
        public void SetSfx(float v) { sfxVolume = Mathf.Clamp01(v); PlayerPrefs.SetFloat(KeySfx, sfxVolume); ApplyVolumes(); }

        void ApplyVolumes()
        {
            if (sfxSource != null) sfxSource.volume = masterVolume * sfxVolume;
            if (musicSource != null) musicSource.volume = masterVolume * musicVolume;
        }

        public void PlaySfx(SfxKind kind)
        {
            if (sfxSource == null) return;
            var clip = GetClip(kind);
            if (clip == null) return;
            // PlayOneShot bị scale bởi AudioSource.volume (đã apply master * sfx).
            sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(AudioClip clip = null)
        {
            if (musicSource == null) return;
            var c = clip != null ? clip : musicOverride;
            if (c == null) return;
            if (musicSource.clip == c && musicSource.isPlaying) return;
            musicSource.clip = c;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        AudioClip GetClip(SfxKind kind)
        {
            int idx = (int)kind;
            if (sfxOverrides != null && idx >= 0 && idx < sfxOverrides.Length && sfxOverrides[idx] != null)
                return sfxOverrides[idx];
            if (procSfx != null && idx >= 0 && idx < procSfx.Length)
                return procSfx[idx];
            return null;
        }

        void BuildProceduralSfx()
        {
            // (freq, durationSec, waveKind)
            procSfx = new[]
            {
                MakeTone("sfx_melee",     440f, 0.10f, WaveKind.NoiseBurst),  // swish
                MakeTone("sfx_cast",      660f, 0.25f, WaveKind.SinePitchDown),
                MakeTone("sfx_bt_ok",     880f, 0.45f, WaveKind.SinePitchUp),
                MakeTone("sfx_bt_fail",   220f, 0.35f, WaveKind.SinePitchDown),
                MakeTone("sfx_medit",     330f, 0.60f, WaveKind.Sine),
                MakeTone("sfx_pickup",    770f, 0.08f, WaveKind.Sine),
                MakeTone("sfx_uiclick",   1000f, 0.05f, WaveKind.Sine),
            };
        }

        enum WaveKind { Sine, SinePitchUp, SinePitchDown, NoiseBurst }

        static AudioClip MakeTone(string name, float baseHz, float durationSec, WaveKind kind)
        {
            const int sampleRate = 22050;
            int samples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSec));
            var data = new float[samples];
            var rng = new System.Random(name.GetHashCode());

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float prog = (float)i / samples;
                float freq = baseHz;
                if (kind == WaveKind.SinePitchUp) freq = Mathf.Lerp(baseHz, baseHz * 2f, prog);
                if (kind == WaveKind.SinePitchDown) freq = Mathf.Lerp(baseHz * 1.5f, baseHz * 0.5f, prog);

                float sample;
                if (kind == WaveKind.NoiseBurst)
                    sample = (float)(rng.NextDouble() * 2.0 - 1.0);
                else
                    sample = Mathf.Sin(2f * Mathf.PI * freq * t);

                // ADSR-ish envelope: attack 10ms, linear decay
                float env = 1f;
                float attackEnd = 0.01f / durationSec;
                if (prog < attackEnd) env = prog / attackEnd;
                else env = 1f - (prog - attackEnd) / (1f - attackEnd);

                data[i] = sample * env * 0.6f;
            }

            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
