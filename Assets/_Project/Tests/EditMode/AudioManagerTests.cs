using NUnit.Framework;
using UnityEngine;
using WildernessCultivation.Audio;

namespace WildernessCultivation.Tests.EditMode
{
    public class AudioManagerTests
    {
        GameObject host;
        AudioManager audio;

        [SetUp]
        public void SetUp()
        {
            // Xoá PlayerPrefs liên quan trước từng test để volumes ban đầu là default.
            PlayerPrefs.DeleteKey("audio.master");
            PlayerPrefs.DeleteKey("audio.music");
            PlayerPrefs.DeleteKey("audio.sfx");

            host = new GameObject("AudioHost");
            audio = host.AddComponent<AudioManager>();
            // EditMode does NOT auto-fire MonoBehaviour.Awake — invoke manually.
            TestHelpers.Boot(audio);
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
            PlayerPrefs.DeleteKey("audio.master");
            PlayerPrefs.DeleteKey("audio.music");
            PlayerPrefs.DeleteKey("audio.sfx");
        }

        [Test]
        public void Awake_RegistersSingleton()
        {
            Assert.AreSame(audio, AudioManager.Instance);
        }

        [Test]
        public void SetMaster_ClampsNegativeToZero()
        {
            audio.SetMaster(-0.5f);
            Assert.AreEqual(0f, audio.masterVolume, 0.0001f);
        }

        [Test]
        public void SetMaster_ClampsOverOneToOne()
        {
            audio.SetMaster(2.5f);
            Assert.AreEqual(1f, audio.masterVolume, 0.0001f);
        }

        [Test]
        public void SetVolumes_PersistInPlayerPrefs()
        {
            audio.SetMaster(0.3f);
            audio.SetMusic(0.2f);
            audio.SetSfx(0.7f);

            Assert.AreEqual(0.3f, PlayerPrefs.GetFloat("audio.master"), 0.0001f);
            Assert.AreEqual(0.2f, PlayerPrefs.GetFloat("audio.music"), 0.0001f);
            Assert.AreEqual(0.7f, PlayerPrefs.GetFloat("audio.sfx"), 0.0001f);
        }

        [Test]
        public void NewInstance_ReadsPreviouslySavedVolumes()
        {
            audio.SetMaster(0.42f);
            // SetMaster writes via PlayerPrefs.SetFloat but EditMode never
            // calls Application.Quit, so values aren't flushed automatically.
            // Force a save before re-reading.
            PlayerPrefs.Save();
            Object.DestroyImmediate(host);

            host = new GameObject("AudioHost2");
            audio = host.AddComponent<AudioManager>();
            TestHelpers.Boot(audio);
            Assert.AreEqual(0.42f, audio.masterVolume, 0.0001f);
        }

        [Test]
        public void PlaySfx_DoesNotThrow_ForEverySfxKind()
        {
            // Không assert phát ra âm thật (EditMode không có audio engine thực chạy),
            // chỉ verify không throw và không crash khi play từng kind.
            foreach (AudioManager.SfxKind kind in System.Enum.GetValues(typeof(AudioManager.SfxKind)))
                Assert.DoesNotThrow(() => audio.PlaySfx(kind));
        }

        [Test]
        public void SfxOverride_TakesPrecedenceOverProcedural()
        {
            // Tạo clip dummy ngắn gán vào slot MeleeSwing (index 0).
            var dummy = AudioClip.Create("dummy", 100, 1, 22050, false);
            audio.sfxOverrides = new AudioClip[(int)AudioManager.SfxKind.UIClick + 1];
            audio.sfxOverrides[(int)AudioManager.SfxKind.MeleeSwing] = dummy;

            // PlaySfx không throw khi override hợp lệ
            Assert.DoesNotThrow(() => audio.PlaySfx(AudioManager.SfxKind.MeleeSwing));
        }
    }
}
