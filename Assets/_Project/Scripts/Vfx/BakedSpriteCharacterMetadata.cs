using System;
using System.Collections.Generic;
using UnityEngine;

namespace WildernessCultivation.Vfx
{
    /// <summary>
    /// DTO for `frame_metadata.json` produced by tools/blender_sprite_render.
    /// Schema: see tools/blender_sprite_render/frame_packer.py §SCHEMA.
    /// Field names MUST match JSON keys exactly (JsonUtility convention).
    /// </summary>
    [Serializable]
    public class BakedSpriteCharacterMetadata
    {
        public int version = 1;
        public string characterId;
        public int cell;
        public int atlasWidth;
        public int atlasHeight;
        public int framesPerAnim;
        public int frameRate = 12;
        public List<BakedAnimSpec> anims = new List<BakedAnimSpec>();

        public static BakedSpriteCharacterMetadata Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("frame_metadata.json content is empty", nameof(json));
            }
            var meta = JsonUtility.FromJson<BakedSpriteCharacterMetadata>(json);
            if (meta == null)
            {
                throw new ArgumentException("Failed to parse frame_metadata.json (JsonUtility returned null)");
            }
            meta.Validate();
            return meta;
        }

        public void Validate()
        {
            if (version <= 0)
            {
                throw new InvalidOperationException($"Invalid version: {version}");
            }
            if (string.IsNullOrEmpty(characterId))
            {
                throw new InvalidOperationException("characterId is required");
            }
            if (cell <= 0 || atlasWidth <= 0 || atlasHeight <= 0 || framesPerAnim <= 0)
            {
                throw new InvalidOperationException(
                    $"Invalid sizing: cell={cell} atlas={atlasWidth}x{atlasHeight} fpa={framesPerAnim}");
            }
            if (frameRate <= 0)
            {
                throw new InvalidOperationException($"frameRate must be positive: {frameRate}");
            }
            if (anims == null || anims.Count == 0)
            {
                throw new InvalidOperationException("anims list is empty");
            }
            foreach (var a in anims)
            {
                a.Validate(this);
            }
        }

        public BakedAnimSpec FindAnim(string name)
        {
            if (anims == null) return null;
            for (int i = 0; i < anims.Count; i++)
            {
                if (anims[i] != null && anims[i].name == name)
                {
                    return anims[i];
                }
            }
            return null;
        }

        public int TotalFrames()
        {
            int total = 0;
            if (anims == null) return 0;
            foreach (var a in anims)
            {
                if (a == null || a.directions == null) continue;
                foreach (var d in a.directions)
                {
                    if (d == null || d.frames == null) continue;
                    total += d.frames.Count;
                }
            }
            return total;
        }
    }

    [Serializable]
    public class BakedAnimSpec
    {
        public string name;
        public bool loop;
        public List<BakedDirectionSpec> directions = new List<BakedDirectionSpec>();

        public void Validate(BakedSpriteCharacterMetadata owner)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("anim name is required");
            }
            if (directions == null || directions.Count == 0)
            {
                throw new InvalidOperationException($"anim {name}: no directions");
            }
            foreach (var d in directions)
            {
                d.Validate(this, owner);
            }
        }

        public BakedDirectionSpec FindDirection(string dir)
        {
            if (directions == null) return null;
            for (int i = 0; i < directions.Count; i++)
            {
                if (directions[i] != null && directions[i].dir == dir)
                {
                    return directions[i];
                }
            }
            return null;
        }
    }

    [Serializable]
    public class BakedDirectionSpec
    {
        public string dir;
        public List<BakedFrameRect> frames = new List<BakedFrameRect>();

        public void Validate(BakedAnimSpec parent, BakedSpriteCharacterMetadata owner)
        {
            if (string.IsNullOrEmpty(dir))
            {
                throw new InvalidOperationException($"anim {parent.name}: direction missing dir name");
            }
            if (frames == null || frames.Count == 0)
            {
                throw new InvalidOperationException(
                    $"anim {parent.name}/{dir}: no frames");
            }
            if (frames.Count != owner.framesPerAnim)
            {
                throw new InvalidOperationException(
                    $"anim {parent.name}/{dir}: expected {owner.framesPerAnim} frames, got {frames.Count}");
            }
            foreach (var f in frames)
            {
                f.Validate(parent, dir, owner);
            }
        }
    }

    [Serializable]
    public class BakedFrameRect
    {
        public int x;
        public int y;
        public int w;
        public int h;

        public void Validate(BakedAnimSpec anim, string dir, BakedSpriteCharacterMetadata owner)
        {
            if (w <= 0 || h <= 0)
            {
                throw new InvalidOperationException(
                    $"anim {anim.name}/{dir}: frame has non-positive size {w}x{h}");
            }
            if (x < 0 || y < 0)
            {
                throw new InvalidOperationException(
                    $"anim {anim.name}/{dir}: frame at negative coord ({x},{y})");
            }
            if (x + w > owner.atlasWidth || y + h > owner.atlasHeight)
            {
                throw new InvalidOperationException(
                    $"anim {anim.name}/{dir}: frame ({x},{y},{w},{h}) extends beyond atlas {owner.atlasWidth}x{owner.atlasHeight}");
            }
        }
    }
}
