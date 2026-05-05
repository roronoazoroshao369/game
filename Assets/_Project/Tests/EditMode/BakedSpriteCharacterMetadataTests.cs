using System;
using NUnit.Framework;
using WildernessCultivation.Vfx;

namespace WildernessCultivation.Tests.EditMode
{
    /// <summary>
    /// Verify <see cref="BakedSpriteCharacterMetadata"/> parser cho output từ
    /// <c>tools/blender_sprite_render/frame_packer.py</c>.
    /// JsonUtility schema constraint: List&lt;T&gt; OK, dict KHÔNG OK — kiểm tra
    /// schema field name + nested list deserialize chính xác.
    /// </summary>
    public class BakedSpriteCharacterMetadataTests
    {
        const string MinimalJson = @"{
  ""version"": 1,
  ""characterId"": ""player"",
  ""cell"": 256,
  ""atlasWidth"": 4096,
  ""atlasHeight"": 4096,
  ""framesPerAnim"": 2,
  ""frameRate"": 12,
  ""anims"": [
    {
      ""name"": ""idle"",
      ""loop"": true,
      ""directions"": [
        {
          ""dir"": ""S"",
          ""frames"": [
            { ""x"": 0, ""y"": 0, ""w"": 256, ""h"": 256 },
            { ""x"": 256, ""y"": 0, ""w"": 256, ""h"": 256 }
          ]
        },
        {
          ""dir"": ""E"",
          ""frames"": [
            { ""x"": 512, ""y"": 0, ""w"": 256, ""h"": 256 },
            { ""x"": 768, ""y"": 0, ""w"": 256, ""h"": 256 }
          ]
        }
      ]
    },
    {
      ""name"": ""attack"",
      ""loop"": false,
      ""directions"": [
        {
          ""dir"": ""S"",
          ""frames"": [
            { ""x"": 1024, ""y"": 0, ""w"": 256, ""h"": 256 },
            { ""x"": 1280, ""y"": 0, ""w"": 256, ""h"": 256 }
          ]
        },
        {
          ""dir"": ""E"",
          ""frames"": [
            { ""x"": 1536, ""y"": 0, ""w"": 256, ""h"": 256 },
            { ""x"": 1792, ""y"": 0, ""w"": 256, ""h"": 256 }
          ]
        }
      ]
    }
  ]
}";

        [Test]
        public void Parse_top_level_fields_extract_correctly()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            Assert.AreEqual(1, meta.version);
            Assert.AreEqual("player", meta.characterId);
            Assert.AreEqual(256, meta.cell);
            Assert.AreEqual(4096, meta.atlasWidth);
            Assert.AreEqual(4096, meta.atlasHeight);
            Assert.AreEqual(2, meta.framesPerAnim);
            Assert.AreEqual(12, meta.frameRate);
        }

        [Test]
        public void Parse_anims_list_has_correct_count_and_names()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            Assert.AreEqual(2, meta.anims.Count);
            Assert.AreEqual("idle", meta.anims[0].name);
            Assert.AreEqual("attack", meta.anims[1].name);
        }

        [Test]
        public void Parse_loop_flag_per_anim()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            Assert.IsTrue(meta.FindAnim("idle").loop);
            Assert.IsFalse(meta.FindAnim("attack").loop);
        }

        [Test]
        public void Parse_directions_per_anim_have_correct_dir_and_frame_count()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            var idle = meta.FindAnim("idle");
            Assert.AreEqual(2, idle.directions.Count);
            Assert.AreEqual("S", idle.directions[0].dir);
            Assert.AreEqual("E", idle.directions[1].dir);
            Assert.AreEqual(2, idle.directions[0].frames.Count);
        }

        [Test]
        public void Parse_frame_rect_xywh_match_input()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            var rect = meta.FindAnim("idle").FindDirection("S").frames[1];
            Assert.AreEqual(256, rect.x);
            Assert.AreEqual(0, rect.y);
            Assert.AreEqual(256, rect.w);
            Assert.AreEqual(256, rect.h);
        }

        [Test]
        public void Total_frames_sums_all_directions_and_anims()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            // 2 anims × 2 dirs × 2 frames = 8
            Assert.AreEqual(8, meta.TotalFrames());
        }

        [Test]
        public void Find_anim_returns_null_for_unknown_name()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            Assert.IsNull(meta.FindAnim("nonexistent"));
        }

        [Test]
        public void Find_direction_returns_null_for_unknown_dir()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            Assert.IsNull(meta.FindAnim("idle").FindDirection("Z"));
        }

        [Test]
        public void Validate_rejects_empty_character_id()
        {
            string json = MinimalJson.Replace("\"player\"", "\"\"");
            Assert.Throws<InvalidOperationException>(
                () => BakedSpriteCharacterMetadata.Parse(json));
        }

        [Test]
        public void Validate_rejects_zero_atlas_size()
        {
            string json = MinimalJson.Replace("\"atlasWidth\": 4096", "\"atlasWidth\": 0");
            Assert.Throws<InvalidOperationException>(
                () => BakedSpriteCharacterMetadata.Parse(json));
        }

        [Test]
        public void Validate_rejects_frame_count_mismatch_with_framesPerAnim()
        {
            // Change framesPerAnim from 2 to 3 — 2 actual frames should mismatch
            string json = MinimalJson.Replace("\"framesPerAnim\": 2", "\"framesPerAnim\": 3");
            Assert.Throws<InvalidOperationException>(
                () => BakedSpriteCharacterMetadata.Parse(json));
        }

        [Test]
        public void Validate_rejects_frame_extending_beyond_atlas()
        {
            // Stretch a frame width beyond atlasWidth
            string json = MinimalJson.Replace(
                "{ \"x\": 1792, \"y\": 0, \"w\": 256, \"h\": 256 }",
                "{ \"x\": 1792, \"y\": 0, \"w\": 99999, \"h\": 256 }");
            Assert.Throws<InvalidOperationException>(
                () => BakedSpriteCharacterMetadata.Parse(json));
        }

        [Test]
        public void Parse_throws_argument_exception_on_empty_string()
        {
            Assert.Throws<ArgumentException>(
                () => BakedSpriteCharacterMetadata.Parse(""));
            Assert.Throws<ArgumentException>(
                () => BakedSpriteCharacterMetadata.Parse(null));
        }

        [Test]
        public void Parse_throws_when_anims_list_empty()
        {
            string json = @"{
  ""version"": 1,
  ""characterId"": ""x"",
  ""cell"": 256,
  ""atlasWidth"": 256,
  ""atlasHeight"": 256,
  ""framesPerAnim"": 1,
  ""frameRate"": 12,
  ""anims"": []
}";
            Assert.Throws<InvalidOperationException>(
                () => BakedSpriteCharacterMetadata.Parse(json));
        }

        [Test]
        public void Parse_round_trip_via_JsonUtility_to_json_preserves_data()
        {
            var meta = BakedSpriteCharacterMetadata.Parse(MinimalJson);
            string serialized = UnityEngine.JsonUtility.ToJson(meta);
            var rt = BakedSpriteCharacterMetadata.Parse(serialized);
            Assert.AreEqual(meta.characterId, rt.characterId);
            Assert.AreEqual(meta.atlasWidth, rt.atlasWidth);
            Assert.AreEqual(meta.anims.Count, rt.anims.Count);
            Assert.AreEqual(meta.TotalFrames(), rt.TotalFrames());
        }
    }
}
