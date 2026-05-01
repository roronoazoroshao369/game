#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.UI;
using WildernessCultivation.Audio;
using WildernessCultivation.CameraFx;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Crafting;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Mobs;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Status;
using WildernessCultivation.UI;
using WildernessCultivation.Vfx;
using WildernessCultivation.World;

namespace WildernessCultivation.EditorTools
{
    /// <summary>
    /// Tạo scene mặc định + assets placeholder cho project. Chạy 1 lần sau khi clone:
    /// Tools > Wilderness Cultivation > Bootstrap Default Scene.
    ///
    /// Sinh: sprites (PNG solid color), ItemSO/RecipeSO/SpiritRootSO/BiomeSO/StatusEffectSO
    /// assets, prefab Player/Tree/Rock/Rabbit/Campfire/WaterSpring/Projectile, MainScene wire sẵn
    /// GameManager + WorldGenerator + Player + Camera + UI, add MainScene vào Build Settings.
    ///
    /// An toàn để gọi nhiều lần — luôn ghi đè asset cũ.
    /// </summary>
    public static class BootstrapWizard
    {
        const string SpritesDir = "Assets/_Project/Sprites";
        const string PrefabsDir = "Assets/_Project/Prefabs";
        const string SOsDir = "Assets/_Project/SOs";
        const string ScenesDir = "Assets/Scenes";
        const string MainScenePath = ScenesDir + "/MainScene.unity";

        [MenuItem("Tools/Wilderness Cultivation/Bootstrap Default Scene")]
        public static void Bootstrap()
        {
            try
            {
                EnsureDirs();
                var sprites = CreateSprites();
                CreateSpriteAtlas();
                var items = CreateItems(sprites);
                var recipes = CreateRecipes(items);
                var spiritRoots = CreateSpiritRoots();
                var statusEffects = CreateStatusEffects();
                var biomes = CreateBiomes();
                var itemDb = CreateItemDatabase(items);
                var prefabs = CreatePrefabs(sprites, items, statusEffects);
                BuildScene(prefabs, items, recipes, spiritRoots, biomes, sprites, itemDb);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog(
                    "Bootstrap Done",
                    "Scene + assets generated.\n\nMở Assets/Scenes/MainScene.unity rồi bấm Play.\n\nLưu ý: sprites là placeholder hình vuông màu — thay bằng art thật sau.",
                    "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Bootstrap FAILED", ex.Message + "\n\nXem Console để biết chi tiết.", "OK");
            }
        }

        // ---------------- DIRECTORIES ----------------
        static void EnsureDirs()
        {
            foreach (var d in new[]
            {
                SpritesDir, PrefabsDir, SOsDir, ScenesDir,
                SOsDir + "/Items", SOsDir + "/Recipes", SOsDir + "/SpiritRoots",
                SOsDir + "/Biomes", SOsDir + "/StatusEffects", SOsDir + "/Database",
                SOsDir + "/Tiles"
            })
            {
                if (!AssetDatabase.IsValidFolder(d))
                {
                    string parent = Path.GetDirectoryName(d).Replace('\\', '/');
                    string leaf = Path.GetFileName(d);
                    Directory.CreateDirectory(d);
                    AssetDatabase.Refresh();
                    if (!AssetDatabase.IsValidFolder(d))
                        AssetDatabase.CreateFolder(parent, leaf);
                }
            }
        }

        // ---------------- SPRITES ----------------
        // Real pixel-art PNGs live in Assets/_Project/Sprites/ and are committed.
        // tools/gen_sprites.py is the source-of-truth generator (re-run it offline
        // when art needs to change, then commit the resulting PNGs). The (w,h,color)
        // entries in `defs` below are kept only as solid-color FALLBACKS that fire
        // on a fresh checkout where the PNG happens to be missing.
        static Dictionary<string, Sprite> CreateSprites()
        {
            var defs = new (string id, int w, int h, Color color)[]
            {
                ("player",    32, 32, new Color(0.30f, 0.55f, 0.95f)),
                ("tree",      32, 48, new Color(0.18f, 0.55f, 0.20f)),
                ("rock",      32, 24, new Color(0.55f, 0.55f, 0.55f)),
                ("rabbit",    24, 20, new Color(0.92f, 0.92f, 0.85f)),
                ("wolf",      32, 24, new Color(0.45f, 0.40f, 0.35f)),
                ("fox_spirit",28, 24, new Color(0.85f, 0.45f, 0.85f)),
                ("boar",      36, 28, new Color(0.45f, 0.30f, 0.20f)),
                ("deer_spirit",30, 30, new Color(0.95f, 0.85f, 0.65f)),
                ("crow",      24, 18, new Color(0.10f, 0.10f, 0.12f)),
                ("snake",     28, 16, new Color(0.30f, 0.55f, 0.20f)),
                ("bat",       24, 18, new Color(0.20f, 0.10f, 0.30f)),
                ("chest",     32, 28, new Color(0.55f, 0.35f, 0.15f)),
                ("workbench", 36, 28, new Color(0.40f, 0.25f, 0.10f)),
                ("campfire",  32, 32, new Color(0.95f, 0.55f, 0.10f)),
                ("water",     40, 40, new Color(0.30f, 0.65f, 0.90f)),
                ("ground",    32, 32, new Color(0.78f, 0.70f, 0.50f)),
                // Grass-tile decoration — small green clump rabbit ăn được. Distinct màu khỏi
                // ground (vàng đất) + tree (xanh đậm) → đứng ra trên terrain.
                ("grass_tile",16, 12, new Color(0.45f, 0.78f, 0.30f)),
                ("projectile",16, 16, new Color(0.95f, 0.30f, 0.10f)),
                ("icon_stick",   24, 24, new Color(0.55f, 0.35f, 0.18f)),
                ("icon_stone",   24, 24, new Color(0.55f, 0.55f, 0.55f)),
                ("icon_meat",    24, 24, new Color(0.85f, 0.30f, 0.30f)),
                ("icon_grilled", 24, 24, new Color(0.55f, 0.30f, 0.20f)),
                ("icon_water",   24, 24, new Color(0.30f, 0.70f, 0.95f)),
                ("icon_torch",   24, 24, new Color(0.95f, 0.65f, 0.20f)),
                ("icon_fish",    24, 24, new Color(0.55f, 0.75f, 0.95f)),
                ("icon_rod",     24, 24, new Color(0.70f, 0.50f, 0.20f)),
                ("icon_tough_hide",   24, 24, new Color(0.45f, 0.30f, 0.20f)),
                ("icon_tusk",         24, 24, new Color(0.95f, 0.92f, 0.80f)),
                ("icon_spirit_antler",24, 24, new Color(0.85f, 0.95f, 0.85f)),
                ("icon_spirit_meat",  24, 24, new Color(0.95f, 0.45f, 0.55f)),
                ("icon_feather",      24, 24, new Color(0.20f, 0.20f, 0.25f)),
                ("icon_snake_skin",   24, 24, new Color(0.30f, 0.55f, 0.25f)),
                ("icon_venom_gland",  24, 24, new Color(0.55f, 0.85f, 0.30f)),
                ("icon_bat_wing",     24, 24, new Color(0.30f, 0.15f, 0.35f)),
                // Flora plants (world prefab sprites)
                ("linh_mushroom", 24, 24, new Color(0.85f, 0.45f, 0.85f)),
                ("berry_bush",    28, 22, new Color(0.50f, 0.20f, 0.30f)),
                ("cactus",        24, 32, new Color(0.30f, 0.65f, 0.30f)),
                ("death_lily",    24, 28, new Color(0.55f, 0.30f, 0.55f)),
                ("linh_bamboo",   20, 40, new Color(0.55f, 0.85f, 0.45f)),
                // Item icons cho flora drops
                ("icon_linh_mushroom", 24, 24, new Color(0.85f, 0.55f, 0.85f)),
                ("icon_berry",         24, 24, new Color(0.65f, 0.20f, 0.30f)),
                ("icon_cactus_water",  24, 24, new Color(0.40f, 0.75f, 0.50f)),
                ("icon_death_pollen",  24, 24, new Color(0.55f, 0.25f, 0.65f)),
                ("icon_bamboo",        24, 24, new Color(0.65f, 0.85f, 0.55f)),
                ("icon_mineral_ore",   24, 24, new Color(0.40f, 0.40f, 0.55f)),
                ("ui_white",      4,  4, Color.white),
                // Drop shadow ellipse (Vfx). Dùng làm child sprite cho mọi entity có DropShadow
                // → cảm giác "grounded" giống Don't Starve. WritePng fallback chỉ là tile vuông;
                // file shadow.png hand-authored sẽ được tools/gen_sprites.py tạo dạng ellipse.
                ("shadow",       32, 16, new Color(0f, 0f, 0f, 0.45f)),
            };

            var dict = new Dictionary<string, Sprite>();
            foreach (var d in defs)
            {
                // Resource art override: nếu user drop PNG vào Art/Resources/{id}/ → dùng PNG
                // đó (auto-PPU sao cho world size khớp placeholder). KHÔNG copy/overwrite
                // Sprites/{id}.png — file user-art ở vị trí gốc giúp tracking + iterate dễ.
                Sprite userArt = ResourceArtImporter.TryLoadSprite(d.id, d.h);
                if (userArt != null)
                {
                    dict[d.id] = userArt;
                    continue;
                }

                string path = $"{SpritesDir}/{d.id}.png";
                // If a hand-authored PNG is already committed (see tools/gen_sprites.py),
                // keep it and only re-import. Only fall back to the solid-color
                // placeholder when the file is genuinely missing — this keeps
                // Bootstrap idempotent on a fresh checkout while preserving real art.
                if (!File.Exists(path))
                {
                    if (d.id == "shadow") WriteEllipsePng(path, d.w, d.h, d.color);
                    else WritePng(path, d.w, d.h, d.color);
                }
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32f;
                    importer.filterMode = FilterMode.Point;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.SaveAndReimport();
                }
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                dict[d.id] = sp;
            }
            return dict;
        }

        // ---------------- SPRITE ATLAS (perf) ----------------
        // Gộp tất cả PNG trong SpritesDir vào 1 atlas → SpriteRenderer dùng cùng atlas
        // texture sẽ batch chung 1 draw call thay vì 1 draw call/sprite. Quan trọng cho
        // mobile vì có hàng nghìn prop spawn từ WorldGenerator.
        static void CreateSpriteAtlas()
        {
            const string atlasPath = SpritesDir + "/WorldAtlas.spriteatlasv2";
            try
            {
                // Xoá cũ để re-run idempotent (Add() append, không replace).
                if (File.Exists(atlasPath))
                    AssetDatabase.DeleteAsset(atlasPath);

                var atlas = new SpriteAtlasAsset();
                atlas.SetIncludeInBuild(true);
                atlas.SetPackingSettings(new SpriteAtlasPackingSettings
                {
                    padding = 2,
                    enableTightPacking = false,
                    enableRotation = false,
                });
                atlas.SetTextureSettings(new SpriteAtlasTextureSettings
                {
                    filterMode = FilterMode.Point,        // pixel-art
                    generateMipMaps = false,
                    readable = false,
                    sRGB = true,
                });

                // Add toàn bộ folder Sprites (Unity sẽ pick mọi sprite con).
                var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(SpritesDir);
                if (folder != null)
                    atlas.Add(new Object[] { folder });

                SpriteAtlasAsset.Save(atlas, atlasPath);
                AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);
            }
            catch (System.Exception ex)
            {
                // Atlas là tối ưu — fail không nên block bootstrap. Chỉ warn.
                Debug.LogWarning($"[BootstrapWizard] CreateSpriteAtlas skipped: {ex.Message}");
            }
        }

        static void WritePng(string path, int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color32[w * h];
            var c32 = (Color32)color;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
            // viền 1px tối hơn
            Color32 border = new Color32(
                (byte)(c32.r * 0.5f), (byte)(c32.g * 0.5f), (byte)(c32.b * 0.5f), 255);
            for (int x = 0; x < w; x++) { pixels[x] = border; pixels[(h - 1) * w + x] = border; }
            for (int y = 0; y < h; y++) { pixels[y * w] = border; pixels[y * w + (w - 1)] = border; }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        // Ellipse PNG fill — shadow nên là ellipse mềm thay vì rect (rect viền 1px sẽ hiện ngay).
        // Pixel trong ellipse: alpha = color.a; ngoài: alpha = 0. Aliased (point filter giữ pixel-art).
        static void WriteEllipsePng(string path, int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color32[w * h];
            var transparent = new Color32(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

            float cx = (w - 1) * 0.5f;
            float cy = (h - 1) * 0.5f;
            float rx = w * 0.5f;
            float ry = h * 0.5f;
            var c32 = (Color32)color;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;
                    if (dx * dx + dy * dy <= 1f) pixels[y * w + x] = c32;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        // ---------------- ITEMS ----------------
        static Dictionary<string, ItemSO> CreateItems(Dictionary<string, Sprite> sprites)
        {
            var dict = new Dictionary<string, ItemSO>();
            dict["stick"]        = MakeItem("stick", "Cành Cây", ItemCategory.Material, sprites["icon_stick"], weight: 0.2f);
            dict["stone"]        = MakeItem("stone", "Đá", ItemCategory.Material, sprites["icon_stone"], weight: 0.5f);
            dict["raw_meat"]     = MakeItem("raw_meat", "Thịt Sống", ItemCategory.Food, sprites["icon_meat"],
                weight: 0.4f, restoreHunger: 12f, isPerishable: true, freshSeconds: 300f);
            dict["grilled_meat"] = MakeItem("grilled_meat", "Thịt Nướng", ItemCategory.Food, sprites["icon_grilled"],
                weight: 0.4f, restoreHunger: 25f, restoreSanity: 5f, isPerishable: true, freshSeconds: 900f);
            dict["water"]        = MakeItem("water", "Nước Sạch", ItemCategory.Drink, sprites["icon_water"],
                weight: 0.5f, restoreThirst: 25f);
            dict["torch"]        = MakeItem("torch", "Bó Đuốc", ItemCategory.Tool, sprites["icon_torch"],
                weight: 0.3f, hasDurability: true, maxDurability: 60f);
            dict["raw_fish"]     = MakeItem("raw_fish", "Cá Tươi", ItemCategory.Food, sprites["icon_fish"],
                weight: 0.3f, restoreHunger: 8f, restoreThirst: 4f, isPerishable: true, freshSeconds: 240f);
            dict["fishing_rod"]  = MakeItem("fishing_rod", "Cần Câu Tre", ItemCategory.Tool, sprites["icon_rod"],
                weight: 0.5f, hasDurability: true, maxDurability: 30f);
            dict["tough_hide"]    = MakeItem("tough_hide", "Da Dày", ItemCategory.Material, sprites["icon_tough_hide"],
                weight: 0.6f);
            dict["tusk"]          = MakeItem("tusk", "Nanh Heo", ItemCategory.Material, sprites["icon_tusk"],
                weight: 0.3f);
            dict["spirit_antler"] = MakeItem("spirit_antler", "Linh Lộc Giác", ItemCategory.Material, sprites["icon_spirit_antler"],
                weight: 0.4f);
            dict["spirit_meat"]   = MakeItem("spirit_meat", "Linh Thú Nhục", ItemCategory.Food, sprites["icon_spirit_meat"],
                weight: 0.4f, restoreHunger: 22f, restoreSanity: 8f, isPerishable: true, freshSeconds: 360f);
            dict["feather"]       = MakeItem("feather", "Lông Vũ", ItemCategory.Material, sprites["icon_feather"],
                weight: 0.05f);
            dict["snake_skin"]    = MakeItem("snake_skin", "Da Rắn", ItemCategory.Material, sprites["icon_snake_skin"],
                weight: 0.2f);
            dict["venom_gland"]   = MakeItem("venom_gland", "Túi Độc", ItemCategory.Material, sprites["icon_venom_gland"],
                weight: 0.15f);
            dict["bat_wing"]      = MakeItem("bat_wing", "Cánh Bức", ItemCategory.Material, sprites["icon_bat_wing"],
                weight: 0.1f);
            // Flora drops (PR C)
            dict["linh_mushroom"] = MakeItem("linh_mushroom", "Linh Nấm", ItemCategory.Food, sprites["icon_linh_mushroom"],
                weight: 0.15f, restoreHunger: 18f, restoreSanity: 4f, isPerishable: true, freshSeconds: 480f);
            dict["berry"]         = MakeItem("berry", "Linh Quả Mọng", ItemCategory.Food, sprites["icon_berry"],
                weight: 0.1f, restoreHunger: 10f, restoreThirst: 5f, isPerishable: true, freshSeconds: 240f);
            dict["cactus_water"]  = MakeItem("cactus_water", "Nước Tiên Nhân Chưởng", ItemCategory.Food, sprites["icon_cactus_water"],
                weight: 0.3f, restoreThirst: 18f);
            dict["death_pollen"]  = MakeItem("death_pollen", "Tử Khí Phấn", ItemCategory.Material, sprites["icon_death_pollen"],
                weight: 0.05f);
            dict["bamboo"]        = MakeItem("bamboo", "Trúc Nhẹ", ItemCategory.Material, sprites["icon_bamboo"],
                weight: 0.4f);
            dict["mineral_ore"]   = MakeItem("mineral_ore", "Khoáng Thạch", ItemCategory.Material, sprites["icon_mineral_ore"],
                weight: 1.0f);
            return dict;
        }

        static ItemSO MakeItem(string id, string name, ItemCategory cat, Sprite icon,
            float weight = 1f, float restoreHunger = 0, float restoreThirst = 0,
            float restoreHP = 0, float restoreSanity = 0,
            bool isPerishable = false, float freshSeconds = 600f,
            bool hasDurability = false, float maxDurability = 50f)
        {
            string path = $"{SOsDir}/Items/Item_{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<ItemSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.itemId = id;
            so.displayName = name;
            so.category = cat;
            so.icon = icon;
            so.weight = weight;
            so.restoreHunger = restoreHunger;
            so.restoreThirst = restoreThirst;
            so.restoreHP = restoreHP;
            so.restoreSanity = restoreSanity;
            so.isPerishable = isPerishable;
            so.freshSeconds = freshSeconds;
            so.hasDurability = hasDurability;
            so.maxDurability = maxDurability;
            EditorUtility.SetDirty(so);
            return so;
        }

        // ---------------- RECIPES ----------------
        static List<RecipeSO> CreateRecipes(Dictionary<string, ItemSO> items)
        {
            var list = new List<RecipeSO>();
            list.Add(MakeRecipe("grill_meat", "Nướng Thịt",
                new[] { (items["raw_meat"], 1) }, items["grilled_meat"], 1,
                CraftStation.Campfire, cookTimeSeconds: 5f));
            list.Add(MakeRecipe("torch_craft", "Chế Đuốc",
                new[] { (items["stick"], 2) }, items["torch"], 1,
                CraftStation.None, cookTimeSeconds: 0f));
            list.Add(MakeRecipe("rod_craft", "Chế Cần Câu",
                new[] { (items["stick"], 3) }, items["fishing_rod"], 1,
                CraftStation.None, cookTimeSeconds: 0f));
            return list;
        }

        static RecipeSO MakeRecipe(string id, string name,
            (ItemSO item, int count)[] inputs, ItemSO output, int outputCount,
            CraftStation station, float cookTimeSeconds = 0f)
        {
            string path = $"{SOsDir}/Recipes/Recipe_{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<RecipeSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<RecipeSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.recipeId = id;
            so.displayName = name;
            var ings = new RecipeIngredient[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                ings[i] = new RecipeIngredient { item = inputs[i].item, count = inputs[i].count };
            so.ingredients = ings;
            so.output = output;
            so.outputCount = outputCount;
            so.requiredStation = station;
            so.cookTimeSeconds = cookTimeSeconds;
            EditorUtility.SetDirty(so);
            return so;
        }

        // ---------------- SPIRIT ROOTS ----------------
        static List<SpiritRootSO> CreateSpiritRoots()
        {
            var list = new List<SpiritRootSO>();
            list.Add(MakeSpiritRoot("Tap", "Tạp Linh Căn", SpiritRootGrade.Tap, SpiritElement.None, bk: 1.2f));
            list.Add(MakeSpiritRoot("Kim", "Kim Linh Căn", SpiritRootGrade.Don, SpiritElement.Kim,
                wpn: 1.15f, dur: 0.7f, sameDmg: 1.5f, counterVuln: 1.5f, aff: 2f));
            list.Add(MakeSpiritRoot("Moc", "Mộc Linh Căn", SpiritRootGrade.Don, SpiritElement.Moc,
                hung: 0.8f, sanity: 0.7f, sameDmg: 1.5f, counterVuln: 1.5f, aff: 2f));
            list.Add(MakeSpiritRoot("Thuy", "Thuỷ Linh Căn", SpiritRootGrade.Don, SpiritElement.Thuy,
                freezeDelta: -20f, freezeDmg: 0.5f, thirst: 0.7f, sameDmg: 1.5f, counterVuln: 1.5f, aff: 2f));
            list.Add(MakeSpiritRoot("Hoa", "Hoả Linh Căn", SpiritRootGrade.Don, SpiritElement.Hoa,
                heatDelta: 20f, freezeDmg: 1.5f, sameDmg: 1.5f, counterVuln: 1.5f, aff: 2f));
            list.Add(MakeSpiritRoot("Tho", "Thổ Linh Căn", SpiritRootGrade.Don, SpiritElement.Tho,
                hp: 1.2f, carry: 1.5f, sameDmg: 1.5f, counterVuln: 1.5f, aff: 2f));
            return list;
        }

        static SpiritRootSO MakeSpiritRoot(string id, string displayName, SpiritRootGrade grade, SpiritElement element,
            float freezeDelta = 0, float heatDelta = 0, float freezeDmg = 1f,
            float thirst = 1f, float hung = 1f, float sanity = 1f,
            float hp = 1f, float carry = 1f,
            float wpn = 1f, float dur = 1f, float sameDmg = 1f, float counterVuln = 1f,
            float xpGain = 1f, float bk = 1f, float aff = 1f)
        {
            string path = $"{SOsDir}/SpiritRoots/SpiritRoot_{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<SpiritRootSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<SpiritRootSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.displayName = displayName;
            so.grade = grade;
            so.primaryElement = element;
            so.freezeThresholdDelta = freezeDelta;
            so.heatThresholdDelta = heatDelta;
            so.freezeDamageMultiplier = freezeDmg;
            so.thirstDecayMultiplier = thirst;
            so.hungerDecayMultiplier = hung;
            so.sanityDecayMultiplier = sanity;
            so.maxHPMultiplier = hp;
            so.carryWeightMultiplier = carry;
            so.weaponDamageMultiplier = wpn;
            so.durabilityWearMultiplier = dur;
            so.sameElementDamageMultiplier = sameDmg;
            so.counterElementVulnerability = counterVuln;
            so.xpGainMultiplier = xpGain;
            so.breakthroughCostMultiplier = bk;
            so.techniqueAffinityMultiplier = aff;
            EditorUtility.SetDirty(so);
            return so;
        }

        // ---------------- STATUS EFFECTS ----------------
        static Dictionary<string, StatusEffectSO> CreateStatusEffects()
        {
            var dict = new Dictionary<string, StatusEffectSO>();
            dict["Poison"]   = MakeStatusEffect("Poison",   "Trúng Độc", StatusEffectType.Poison, hpTick: 2f);
            dict["Bleed"]    = MakeStatusEffect("Bleed",    "Chảy Máu",  StatusEffectType.Bleeding, hpTick: 3f);
            dict["Sickness"] = MakeStatusEffect("Sickness", "Bệnh",      StatusEffectType.Sickness, hpTick: 1f, sanityTick: 1f);
            dict["Burn"]     = MakeStatusEffect("Burn",     "Bỏng",      StatusEffectType.Burn, hpTick: 4f, dmgIn: 1.2f);
            dict["Freeze"]   = MakeStatusEffect("Freeze",   "Băng",      StatusEffectType.Freeze, moveMul: 0.5f);
            dict["Stun"]     = MakeStatusEffect("Stun",     "Choáng",    StatusEffectType.Stun, moveMul: 0f);
            return dict;
        }

        static StatusEffectSO MakeStatusEffect(string id, string displayName, StatusEffectType type,
            float hpTick = 0, float sanityTick = 0, float moveMul = 1f, float dmgIn = 1f)
        {
            string path = $"{SOsDir}/StatusEffects/Status_{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<StatusEffectSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<StatusEffectSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.effectId = id.ToLower();
            so.displayName = displayName;
            so.type = type;
            so.hpDamagePerTick = hpTick;
            so.sanityDamagePerTick = sanityTick;
            so.moveSpeedMultiplier = moveMul;
            so.incomingDamageMultiplier = dmgIn;
            so.tickIntervalSec = 1f;
            so.defaultDurationSec = 10f;
            EditorUtility.SetDirty(so);
            return so;
        }

        // ---------------- BIOMES ----------------
        static List<BiomeSO> CreateBiomes()
        {
            var list = new List<BiomeSO>();
            list.Add(MakeBiome("forest", "Rừng Linh Mộc",
                treeDensity: 0.18f, rockDensity: 0.05f, waterDensity: 0.008f,
                tempDay: 0f, tempNight: 0f,
                spiritEnergy: 1.2f, ambientNightSan: 0f,
                selRange: new Vector2(0f, 0.40f)));
            list.Add(MakeBiome("stone_highlands", "Đá Sơn Cao Nguyên",
                treeDensity: 0.04f, rockDensity: 0.20f, waterDensity: 0.003f,
                tempDay: -3f, tempNight: -8f,
                spiritEnergy: 1.0f, ambientNightSan: 0.3f,
                selRange: new Vector2(0.40f, 0.65f)));
            list.Add(MakeBiome("desert", "Hoang Mạc Tử Khí",
                treeDensity: 0.02f, rockDensity: 0.10f, waterDensity: 0.001f,
                tempDay: 25f, tempNight: -15f,
                spiritEnergy: 0.8f, ambientNightSan: 1f,
                selRange: new Vector2(0.65f, 1f)));
            return list;
        }

        static BiomeSO MakeBiome(string id, string displayName,
            float treeDensity, float rockDensity, float waterDensity,
            float tempDay, float tempNight,
            float spiritEnergy, float ambientNightSan,
            Vector2 selRange)
        {
            string path = $"{SOsDir}/Biomes/Biome_{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<BiomeSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<BiomeSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.biomeId = id;
            so.displayName = displayName;
            so.treeDensity = treeDensity;
            so.rockDensity = rockDensity;
            so.waterDensity = waterDensity;
            so.temperatureDayOffset = tempDay;
            so.temperatureNightOffset = tempNight;
            so.spiritEnergyMultiplier = spiritEnergy;
            so.ambientNightSanDamage = ambientNightSan;
            so.selectionRange = selRange;
            EditorUtility.SetDirty(so);
            return so;
        }

        /// <summary>
        /// Bộ extraNodes (linh thảo / mineral) per biome. Density giữ thấp (0.5%-2%)
        /// để không spam. Forest = mushroom + berry + bamboo; Stone Highlands =
        /// mineral_ore + bamboo; Desert = cactus + death_lily.
        /// </summary>
        static BiomeSO.ExtraNode[] BuildExtraNodesFor(string biomeId, PrefabBundle prefabs)
        {
            switch (biomeId)
            {
                case "forest":
                    return new[]
                    {
                        new BiomeSO.ExtraNode { prefab = prefabs.LinhMushroom, density = 0.020f },
                        new BiomeSO.ExtraNode { prefab = prefabs.BerryBush,    density = 0.025f },
                        new BiomeSO.ExtraNode { prefab = prefabs.LinhBamboo,   density = 0.015f },
                    };
                case "stone_highlands":
                    return new[]
                    {
                        new BiomeSO.ExtraNode { prefab = prefabs.MineralRock,  density = 0.030f },
                        new BiomeSO.ExtraNode { prefab = prefabs.LinhBamboo,   density = 0.010f },
                    };
                case "desert":
                    return new[]
                    {
                        new BiomeSO.ExtraNode { prefab = prefabs.Cactus,    density = 0.020f },
                        new BiomeSO.ExtraNode { prefab = prefabs.DeathLily, density = 0.005f },
                    };
                default:
                    return System.Array.Empty<BiomeSO.ExtraNode>();
            }
        }

        // ---------------- TILES ----------------
        /// <summary>
        /// Tạo (hoặc load) Tile asset từ sprite cho Tilemap-based ground rendering. Dùng thay
        /// per-tile Instantiate ở WorldGenerator để giảm hàng ngàn GameObject ở map lớn.
        /// </summary>
        static Tile CreateGroundTile(string id, Sprite sprite)
        {
            string path = $"{SOsDir}/Tiles/Tile_{id}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        // ---------------- ITEM DATABASE ----------------
        static ItemDatabase CreateItemDatabase(Dictionary<string, ItemSO> items)
        {
            string path = $"{SOsDir}/Database/ItemDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(db, path);
            }
            db.items = new List<ItemSO>(items.Values);
            EditorUtility.SetDirty(db);
            return db;
        }

        // ---------------- PREFABS ----------------
        class PrefabBundle
        {
            public GameObject Player, Tree, Rock, Rabbit, Wolf, FoxSpirit, Campfire,
                WaterSpring, StorageChest, Workbench, Projectile,
                Boar, DeerSpirit, Crow,
                Snake, Bat,
                LinhMushroom, BerryBush, Cactus, DeathLily, LinhBamboo, MineralRock,
                GrassTile;
        }

        // Cache shadow sprite cho mọi BuildXxxPrefab gọi AttachDropShadow tiện hơn
        // (không cần thread sprites["shadow"] qua từng helper).
        static Sprite s_shadowSprite;

        // Build puppet child hierarchy cho character (Player / hero mob) khi user drop
        // body-part PNG ở Art/Characters/{id}/. Cấu trúc:
        //
        //   {root GameObject (caller)}
        //     └── SpriteRoot (Transform, flip X handled by PuppetAnimController)
        //           ├── Tail        (optional, sortingOrder = base+0)
        //           ├── LegLeft     (sortingOrder = base+1)
        //           ├── LegRight    (sortingOrder = base+1)
        //           ├── Torso       (sortingOrder = base+2)
        //           │     └── Head  (child of torso → bob theo torso, sortingOrder = base+5)
        //           ├── ArmLeft     (sortingOrder = base+3)
        //           └── ArmRight    (sortingOrder = base+3)
        //
        // Default body-part offsets ước lượng cho character ~1.5u tall (head 0.4u trên torso, etc.).
        // User có thể tinh chỉnh trong Inspector sau khi Bootstrap nếu sprite có pivot khác.
        static void BuildPuppetHierarchy(
            GameObject root,
            Dictionary<CharacterArtSpec.PuppetRole, Sprite> sprites,
            int sortingOrderBase,
            out Transform spriteRoot,
            out Transform torso,
            out Transform head,
            out Transform armLeft,
            out Transform armRight,
            out Transform legLeft,
            out Transform legRight,
            out Transform tail,
            out Transform forearmLeft,
            out Transform forearmRight,
            out Transform shinLeft,
            out Transform shinRight)
        {
            var rootGo = new GameObject("SpriteRoot");
            rootGo.transform.SetParent(root.transform, false);
            spriteRoot = rootGo.transform;

            torso = AddPuppetPart(spriteRoot, sprites, CharacterArtSpec.PuppetRole.Torso,
                sortingOrderBase + 2, Vector3.zero);
            head = torso != null
                ? AddPuppetPart(torso, sprites, CharacterArtSpec.PuppetRole.Head,
                    sortingOrderBase + 5, new Vector3(0f, 0.45f, 0f))
                : null;
            armLeft = AddPuppetPart(spriteRoot, sprites, CharacterArtSpec.PuppetRole.ArmLeft,
                sortingOrderBase + 3, new Vector3(-0.18f, 0.05f, 0f));
            armRight = AddPuppetPart(spriteRoot, sprites, CharacterArtSpec.PuppetRole.ArmRight,
                sortingOrderBase + 3, new Vector3(0.18f, 0.05f, 0f));
            legLeft = AddPuppetPart(spriteRoot, sprites, CharacterArtSpec.PuppetRole.LegLeft,
                sortingOrderBase + 1, new Vector3(-0.10f, -0.30f, 0f));
            legRight = AddPuppetPart(spriteRoot, sprites, CharacterArtSpec.PuppetRole.LegRight,
                sortingOrderBase + 1, new Vector3(0.10f, -0.30f, 0f));
            tail = AddPuppetPart(spriteRoot, sprites, CharacterArtSpec.PuppetRole.Tail,
                sortingOrderBase + 0, new Vector3(-0.25f, -0.05f, 0f));

            // PR K (L2): nest forearm under arm, shin under leg. Optional — missing PNG
            // → AddPuppetPart returns null → PuppetAnimController null-skips bend logic.
            // Local offset = end of parent (arm sprite ~0.25u tall pivot top, leg ~0.30u).
            forearmLeft = armLeft != null
                ? AddPuppetPart(armLeft, sprites, CharacterArtSpec.PuppetRole.ForearmLeft,
                    sortingOrderBase + 4, new Vector3(0f, -0.25f, 0f))
                : null;
            forearmRight = armRight != null
                ? AddPuppetPart(armRight, sprites, CharacterArtSpec.PuppetRole.ForearmRight,
                    sortingOrderBase + 4, new Vector3(0f, -0.25f, 0f))
                : null;
            shinLeft = legLeft != null
                ? AddPuppetPart(legLeft, sprites, CharacterArtSpec.PuppetRole.ShinLeft,
                    sortingOrderBase + 1, new Vector3(0f, -0.30f, 0f))
                : null;
            shinRight = legRight != null
                ? AddPuppetPart(legRight, sprites, CharacterArtSpec.PuppetRole.ShinRight,
                    sortingOrderBase + 1, new Vector3(0f, -0.30f, 0f))
                : null;
        }

        static Transform AddPuppetPart(Transform parent,
            Dictionary<CharacterArtSpec.PuppetRole, Sprite> sprites,
            CharacterArtSpec.PuppetRole role,
            int sortingOrder,
            Vector3 localPos)
        {
            if (!sprites.TryGetValue(role, out var sprite) || sprite == null) return null;
            var go = new GameObject(role.ToString());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return go.transform;
        }

        /// <summary>
        /// PR M — wrap placeholder sprite set thành <see cref="CharacterArtImporter.CharacterSpriteSet"/>
        /// (East-only, isMultiDirectional=false). Caller dùng giống real puppetSet → puppet path
        /// luôn active. Skeleton demo motion runs ngay không cần real art.
        /// </summary>
        static CharacterArtImporter.CharacterSpriteSet BuildPlaceholderSpriteSet(
            string characterId, bool includeTail)
        {
            var east = PuppetPlaceholderGenerator.EnsureSpriteSet(characterId, includeTail);
            var set = new CharacterArtImporter.CharacterSpriteSet { isMultiDirectional = false };
            set.spritesByDir[CharacterArtSpec.PuppetDirection.East] = east;
            return set;
        }

        // PR J — L3+ multi-direction wire.
        // Khi user gen folder layout E/N/S/ → CharacterSpriteSet.isMultiDirectional=true.
        // Populate per-role sprite arrays trên PuppetAnimController để runtime swap khi đổi
        // direction. Flat layout → arrays giữ null → controller fallback side-only legacy.
        static void WirePuppetMultiDirSprites(
            PuppetAnimController puppet,
            CharacterArtImporter.CharacterSpriteSet set)
        {
            if (puppet == null || set == null || !set.isMultiDirectional) return;
            puppet.headSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.Head);
            puppet.torsoSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.Torso);
            puppet.armLeftSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.ArmLeft);
            puppet.armRightSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.ArmRight);
            puppet.legLeftSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.LegLeft);
            puppet.legRightSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.LegRight);
            puppet.tailSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.Tail);
            puppet.forearmLeftSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.ForearmLeft);
            puppet.forearmRightSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.ForearmRight);
            puppet.shinLeftSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.ShinLeft);
            puppet.shinRightSpritesByDir = ExtractRoleArray(set, CharacterArtSpec.PuppetRole.ShinRight);
        }

        static Sprite[] ExtractRoleArray(
            CharacterArtImporter.CharacterSpriteSet set,
            CharacterArtSpec.PuppetRole role)
        {
            // Array length 4 = enum max (East/North/South/West). West luôn null (controller
            // dùng East sprite + flipX). Indices không có art → null → controller fallback East.
            var arr = new Sprite[4];
            foreach (var kv in set.spritesByDir)
            {
                if (kv.Value.TryGetValue(role, out var sprite) && sprite != null)
                    arr[(int)kv.Key] = sprite;
            }
            return arr;
        }

        // Attach DropShadow component + sinh child renderer ngay (Awake không tự
        // chạy lúc AddComponent trong Editor → phải gọi EnsureChild thủ công để
        // child shadow đi vào prefab khi SaveAsPrefab).
        static void AttachDropShadow(GameObject go,
            float offsetY = -0.35f, float scaleX = 0.85f, float scaleY = 0.35f)
        {
            if (s_shadowSprite == null) return;
            var ds = go.AddComponent<DropShadow>();
            ds.shadowSprite = s_shadowSprite;
            ds.localOffset = new Vector2(0f, offsetY);
            ds.localScale = new Vector2(scaleX, scaleY);
            ds.EnsureChild();
        }

        static void AttachWindSway(GameObject go,
            float amplitudeDegrees = 2.5f, float frequencyHz = 0.7f)
        {
            var ws = go.AddComponent<WindSway>();
            ws.amplitudeDegrees = amplitudeDegrees;
            ws.frequencyHz = frequencyHz;
        }

        // Reactive feedback preset (flash + damped shake + particle burst on hit).
        // Tuned per resource type — caller chọn preset; nếu cần tinh chỉnh sâu có thể
        // override field trên prefab Inspector sau khi Bootstrap.
        enum ReactivePreset
        {
            Tree,        // wide rotation shake, leaf-green burst
            Rock,        // small fast shake, gray dust burst
            Bush,        // wide bush rustle, green burst (berry bush)
            Plant,       // mild plant sway, no burst (mushroom / lily)
            Cactus,      // tight quiver, no burst
            Bamboo,      // tall bamboo sway
            Mob,         // creature flesh hit — quick shake + red blood-mist burst
        }

        static void AttachReactiveOnHit(GameObject go, ReactivePreset preset)
        {
            var fx = go.AddComponent<ReactiveOnHit>();
            switch (preset)
            {
                case ReactivePreset.Tree:
                    fx.shakeAmplitudeDeg = 8f; fx.shakeFrequencyHz = 6f;
                    fx.shakeDecay = 8f; fx.shakeDuration = 0.5f;
                    fx.burstCount = 4; fx.burstSpeedMin = 1.5f; fx.burstSpeedMax = 3f;
                    fx.burstLifetime = 0.7f;
                    fx.burstColor = new Color(0.45f, 0.70f, 0.30f, 1f); // leaf green
                    break;
                case ReactivePreset.Rock:
                    fx.shakeAmplitudeDeg = 3f; fx.shakeFrequencyHz = 12f;
                    fx.shakeDecay = 12f; fx.shakeDuration = 0.25f;
                    fx.burstCount = 5; fx.burstSpeedMin = 2f; fx.burstSpeedMax = 4f;
                    fx.burstLifetime = 0.5f;
                    fx.burstColor = new Color(0.60f, 0.58f, 0.52f, 1f); // dust gray
                    break;
                case ReactivePreset.Bush:
                    fx.shakeAmplitudeDeg = 14f; fx.shakeFrequencyHz = 5f;
                    fx.shakeDecay = 7f; fx.shakeDuration = 0.45f;
                    fx.burstCount = 3; fx.burstSpeedMin = 1f; fx.burstSpeedMax = 2.2f;
                    fx.burstLifetime = 0.5f;
                    fx.burstColor = new Color(0.55f, 0.30f, 0.40f, 1f); // berry purple-red
                    break;
                case ReactivePreset.Plant:
                    fx.shakeAmplitudeDeg = 10f; fx.shakeFrequencyHz = 5f;
                    fx.shakeDecay = 9f; fx.shakeDuration = 0.35f;
                    fx.enableBurst = false;
                    break;
                case ReactivePreset.Cactus:
                    fx.shakeAmplitudeDeg = 2f; fx.shakeFrequencyHz = 14f;
                    fx.shakeDecay = 14f; fx.shakeDuration = 0.2f;
                    fx.enableBurst = false;
                    break;
                case ReactivePreset.Bamboo:
                    fx.shakeAmplitudeDeg = 6f; fx.shakeFrequencyHz = 4f;
                    fx.shakeDecay = 6f; fx.shakeDuration = 0.55f;
                    fx.burstCount = 3; fx.burstSpeedMin = 1f; fx.burstSpeedMax = 2f;
                    fx.burstLifetime = 0.5f;
                    fx.burstColor = new Color(0.60f, 0.80f, 0.50f, 1f); // bamboo light green
                    break;
                case ReactivePreset.Mob:
                    fx.flashColor = new Color(1f, 0.5f, 0.5f, 0.85f); // pink-red flesh flash
                    fx.flashDuration = 0.10f;
                    fx.shakeAmplitudeDeg = 5f; fx.shakeFrequencyHz = 14f;
                    fx.shakeDecay = 14f; fx.shakeDuration = 0.18f;
                    fx.burstCount = 3; fx.burstSpeedMin = 1f; fx.burstSpeedMax = 2.2f;
                    fx.burstLifetime = 0.35f;
                    fx.burstColor = new Color(0.75f, 0.20f, 0.20f, 1f); // blood mist red
                    break;
            }
        }

        /// <summary>
        /// Mob hit feedback bundle — <see cref="ReactiveOnHit"/> (Mob preset) +
        /// <see cref="HitKnockback"/> với impulse tham số. Caller (Build*Prefab) chỉ
        /// cần truyền magnitude tuỳ kích thước (rabbit nhỏ → 1.5; wolf → 2.5; boar → 3.5).
        /// </summary>
        static void AttachMobHitFx(GameObject go, float knockbackImpulse)
        {
            AttachReactiveOnHit(go, ReactivePreset.Mob);
            var kb = go.AddComponent<HitKnockback>();
            kb.impulse = knockbackImpulse;
        }

        /// <summary>
        /// Procedural mob animation — walk bob (Y scale modulation theo speed) + tilt theo direction
        /// + idle breathing + hooks lunge/squash/crouch driven bởi FSM state. Tham số tuỳ mob size:
        /// rabbit nhanh nhẹ (freq 6, tilt 6°), wolf trung (freq 4.5, tilt 5°), boar chậm (freq 3, tilt 3°),
        /// crow/bat bay (freq 7, tilt 4°).
        /// </summary>
        static void AttachMobAnim(GameObject go, float walkBobFreq = 5f, float maxTiltDeg = 5f,
            float walkBobAmp = 0.05f)
        {
            var a = go.AddComponent<MobAnimController>();
            a.walkBobFrequency = walkBobFreq;
            a.maxTiltDeg = maxTiltDeg;
            a.walkBobAmplitude = walkBobAmp;
        }

        /// <summary>
        /// Rock progressive crack overlay — alpha tăng khi HP giảm. Caller truyền
        /// rock-tint (xám / nâu) tuỳ variant.
        /// </summary>
        static void AttachProgressiveCrack(GameObject go, Color tint, float maxAlpha = 0.55f)
        {
            var c = go.AddComponent<ProgressiveCrackOverlay>();
            c.crackTint = tint;
            c.maxAlpha = maxAlpha;
            c.startThreshold = 0.85f;
        }

        /// <summary>
        /// Water ripple ring expand on player approach.
        /// </summary>
        static void AttachWaterRipple(GameObject go, float triggerRadius = 1f, float cooldown = 1.2f)
        {
            var r = go.AddComponent<WaterRipple>();
            r.triggerRadius = triggerRadius;
            r.spawnCooldown = cooldown;
            r.playerMask = ~0;
        }

        /// <summary>
        /// Grass / flora bend khi player overlap. Caller tune amplitude theo plant size
        /// (cỏ ~12°, bush ~8°, mushroom ~5°).
        /// </summary>
        static void AttachPlayerOverlapSway(GameObject go, float maxBendDeg, float detectRadius = 0.6f)
        {
            var s = go.AddComponent<PlayerOverlapSway>();
            s.maxBendDeg = maxBendDeg;
            s.detectRadius = detectRadius;
            s.playerMask = ~0;
        }

        static PrefabBundle CreatePrefabs(Dictionary<string, Sprite> sprites,
            Dictionary<string, ItemSO> items, Dictionary<string, StatusEffectSO> statusEffects)
        {
            s_shadowSprite = sprites.TryGetValue("shadow", out var shadow) ? shadow : null;

            var bundle = new PrefabBundle();
            bundle.Player = BuildPlayerPrefab(sprites);
            bundle.Tree = BuildResourceNodePrefab("Tree", sprites["tree"], items["stick"], min: 2, max: 4, hp: 4f);
            bundle.Rock = BuildResourceNodePrefab("Rock", sprites["rock"], items["stone"], min: 1, max: 3, hp: 6f);
            bundle.Rabbit = BuildRabbitPrefab(sprites["rabbit"], items["raw_meat"]);
            bundle.Wolf = BuildWolfPrefab(sprites["wolf"], items["raw_meat"]);
            bundle.FoxSpirit = BuildFoxSpiritPrefab(sprites["fox_spirit"], items["raw_meat"]);
            bundle.Boar = BuildBoarPrefab(sprites["boar"], items["raw_meat"], items["tough_hide"], items["tusk"]);
            bundle.DeerSpirit = BuildDeerSpiritPrefab(sprites["deer_spirit"], items["spirit_meat"], items["spirit_antler"]);
            bundle.Crow = BuildCrowPrefab(sprites["crow"], items["feather"]);
            bundle.Snake = BuildSnakePrefab(sprites["snake"], items["snake_skin"], items["venom_gland"], statusEffects["Poison"]);
            bundle.Bat = BuildBatPrefab(sprites["bat"], items["bat_wing"], statusEffects["Bleed"]);
            // Flora — wild plants (PR C)
            bundle.LinhMushroom = BuildLinhMushroomPrefab(sprites["linh_mushroom"], items["linh_mushroom"]);
            bundle.BerryBush    = BuildBerryBushPrefab(sprites["berry_bush"], items["berry"]);
            bundle.Cactus       = BuildCactusPrefab(sprites["cactus"], items["cactus_water"]);
            bundle.DeathLily    = BuildDeathLilyPrefab(sprites["death_lily"], items["death_pollen"]);
            bundle.LinhBamboo   = BuildLinhBambooPrefab(sprites["linh_bamboo"], items["bamboo"], items["stick"]);
            bundle.MineralRock  = BuildMineralRockPrefab(sprites["rock"], items["mineral_ore"], items["stone"]);
            bundle.Campfire = BuildCampfirePrefab(sprites["campfire"], items["stick"]);
            bundle.WaterSpring = BuildWaterSpringPrefab(sprites["water"], items["water"], items["raw_fish"]);
            bundle.StorageChest = BuildStorageChestPrefab(sprites["chest"]);
            bundle.Workbench = BuildWorkbenchPrefab(sprites["workbench"], items["stick"]);
            bundle.Projectile = BuildProjectilePrefab(sprites["projectile"], statusEffects["Burn"]);
            bundle.GrassTile = BuildGrassTilePrefab(sprites["grass_tile"]);
            return bundle;
        }

        static GameObject BuildPlayerPrefab(Dictionary<string, Sprite> sprites)
        {
            var go = new GameObject("Player");
            go.tag = "Player";

            // Puppet path always — drops user's real PNG khi có ở Art/Characters/player/,
            // else PR M placeholder skeleton (13 colored rectangles). Demo motion runs ngay.
            // PuppetAnimController tự handle flip qua spriteRoot.localScale → KHÔNG set
            // pc.spriteRenderer (PlayerController.flipX skip via null guard) tránh double-flip.
            // PR J: dual layout support — flat (legacy) hoặc E/N/S subfolder (multi-dir).
            var puppetSet = CharacterArtImporter.TryLoadCharacterSpriteSet("player", placeholderHeightPx: 32)
                ?? BuildPlaceholderSpriteSet("player", includeTail: false);
            SpriteRenderer sr = null;
            {
                BuildPuppetHierarchy(go, puppetSet.EastSprites, sortingOrderBase: 5,
                    out var spriteRoot, out var torsoT, out var headT,
                    out var armLT, out var armRT, out var legLT, out var legRT, out var tailT,
                    out var foreLT, out var foreRT, out var shinLT, out var shinRT);
                var puppet = go.AddComponent<PuppetAnimController>();
                puppet.spriteRoot = spriteRoot;
                puppet.torso = torsoT;
                puppet.head = headT;
                puppet.armLeft = armLT;
                puppet.armRight = armRT;
                puppet.legLeft = legLT;
                puppet.legRight = legRT;
                puppet.tail = tailT;
                puppet.forearmLeft = foreLT;
                puppet.forearmRight = foreRT;
                puppet.shinLeft = shinLT;
                puppet.shinRight = shinRT;
                // Player tunings — slower step than mob, less aggressive swing.
                puppet.walkFrequency = 3f;
                puppet.armSwingDeg = 28f;
                puppet.legSwingDeg = 18f;
                WirePuppetMultiDirSprites(puppet, puppetSet);
            }

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            // Order matters:
            //  1. SpiritRoot trước PlayerStats (PlayerStats.Awake() gọi GetComponent<SpiritRoot>()).
            //  2. PlayerStats trước StatusEffectManager — StatusEffectManager có
            //     [RequireComponent(typeof(PlayerStats))]; nếu thêm StatusEffectManager trước,
            //     Unity sẽ auto-add một PlayerStats thứ hai (duplicate → double stat decay).
            go.AddComponent<SpiritRoot>();
            go.AddComponent<PlayerStats>();
            go.AddComponent<StatusEffectManager>();
            go.AddComponent<Inventory>();
            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerCombat>();
            go.AddComponent<InteractAction>();
            go.AddComponent<CraftingSystem>();
            go.AddComponent<RealmSystem>();
            go.AddComponent<AwakeningSystem>();
            go.AddComponent<SleepAction>();
            go.AddComponent<TorchAction>();
            go.AddComponent<MagicTreasureAction>();
            go.AddComponent<MeditationAction>();
            go.AddComponent<DodgeAction>();
            go.AddComponent<FishingAction>();

            // Wire SpriteRenderer ref vào PlayerController. Puppet mode: sr=null nên
            // PlayerController.flipX nullsafe-skip; PuppetAnimController handle flipping.
            var pc = go.GetComponent<PlayerController>();
            pc.spriteRenderer = sr;

            // LayerMask defaults to 0 (Nothing) — Physics2D.OverlapCircle sẽ không trúng gì.
            // Set sang Everything (~0) cho mặc định placeholder; user có thể thu hẹp sau.
            var combat = go.GetComponent<PlayerCombat>();
            combat.hitMask = ~0;
            var interact = go.GetComponent<InteractAction>();
            if (interact != null) interact.interactMask = ~0;

            AttachDropShadow(go);

            return SaveAsPrefab(go, $"{PrefabsDir}/Player.prefab");
        }

        static GameObject BuildResourceNodePrefab(string name, Sprite sprite, ItemSO drop,
            int min, int max, float hp)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            var node = go.AddComponent<ResourceNode>();
            node.nodeName = name;
            node.maxHP = hp;
            node.currentHP = hp;
            node.drops = new ResourceNode.Drop[]
            {
                new ResourceNode.Drop { item = drop, min = min, max = max }
            };
            // Tree → drop shadow rộng + wind sway để cảm giác sống. Rock không cần.
            if (name == "Tree")
            {
                AttachDropShadow(go, offsetY: -0.6f, scaleX: 1.1f, scaleY: 0.4f);
                AttachWindSway(go, amplitudeDegrees: 2.0f, frequencyHz: 0.6f);
                AttachReactiveOnHit(go, ReactivePreset.Tree);
            }
            else if (name == "Rock")
            {
                AttachReactiveOnHit(go, ReactivePreset.Rock);
                // Crack tăng dần: tint nâu xám đậm phủ rock khi HP cạn.
                AttachProgressiveCrack(go, tint: new Color(0.15f, 0.12f, 0.10f, 1f));
            }
            return SaveAsPrefab(go, $"{PrefabsDir}/{name}.prefab");
        }

        // Grass-tile decoration — small green clump rabbit ăn được. KHÔNG phải ResourceNode
        // (no HP/drops). CircleCollider2D trigger để rabbit tìm thấy qua OverlapCircle.
        static GameObject BuildGrassTilePrefab(Sprite sprite)
        {
            var go = new GameObject("GrassTile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            // SortingOrder thấp hơn resource (2) + mob (>5) để cỏ ở dưới — visible nhưng không che.
            sr.sortingOrder = 1;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.isTrigger = true;
            go.AddComponent<GrassTile>();
            return SaveAsPrefab(go, $"{PrefabsDir}/GrassTile.prefab");
        }

        static GameObject BuildRabbitPrefab(Sprite sprite, ItemSO meatDrop)
        {
            var go = new GameObject("Rabbit");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            var ai = go.AddComponent<RabbitAI>();
            ai.spriteRenderer = sr;
            ai.maxHP = 12f;
            ai.HP = 12f;
            ai.drops = new[] { new ResourceNode.Drop { item = meatDrop, min = 1, max = 2 } };
            // playerMask default 0 → RabbitAI không detect player. Set Everything cho placeholder.
            ai.playerMask = ~0;
            AttachDropShadow(go, offsetY: -0.25f, scaleX: 0.65f, scaleY: 0.3f);
            AttachMobHitFx(go, knockbackImpulse: 1.5f);
            AttachMobAnim(go, walkBobFreq: 6f, maxTiltDeg: 6f);
            return SaveAsPrefab(go, $"{PrefabsDir}/Rabbit.prefab");
        }

        static GameObject BuildWolfPrefab(Sprite sprite, ItemSO meatDrop)
        {
            var go = new GameObject("Wolf");
            // Puppet path: Art/Characters/wolf/ có Head + Torso PNG → multi-piece hierarchy.
            // Tunings: faster step (4.5Hz), aggressive leg swing (28°) cho quadruped silhouette,
            // tail sway 16° (mid range — wolf tail cứng hơn fox).
            // PR J multi-dir support: flat folder (legacy) → East-only; E/N/S subfolders → full multi-dir.
            // PR M: no real PNG → placeholder 13-joint skeleton (motion demo runs ngay).
            var puppetSet = CharacterArtImporter.TryLoadCharacterSpriteSet("wolf", placeholderHeightPx: 32)
                ?? BuildPlaceholderSpriteSet("wolf", includeTail: true);
            SpriteRenderer sr = null;
            {
                BuildPuppetHierarchy(go, puppetSet.EastSprites, sortingOrderBase: 3,
                    out var spriteRoot, out var torsoT, out var headT,
                    out var armLT, out var armRT, out var legLT, out var legRT, out var tailT,
                    out var foreLT, out var foreRT, out var shinLT, out var shinRT);
                var puppet = go.AddComponent<PuppetAnimController>();
                puppet.spriteRoot = spriteRoot;
                puppet.torso = torsoT;
                puppet.head = headT;
                puppet.armLeft = armLT;
                puppet.armRight = armRT;
                puppet.legLeft = legLT;
                puppet.legRight = legRT;
                puppet.tail = tailT;
                puppet.forearmLeft = foreLT;
                puppet.forearmRight = foreRT;
                puppet.shinLeft = shinLT;
                puppet.shinRight = shinRT;
                puppet.walkFrequency = 4.5f;
                puppet.armSwingDeg = 32f;
                puppet.legSwingDeg = 28f;
                puppet.tailSwayDeg = 16f;
                puppet.referenceSpeed = 2.5f;
                WirePuppetMultiDirSprites(puppet, puppetSet);
            }
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            var ai = go.AddComponent<WolfAI>();
            ai.spriteRenderer = sr;
            ai.maxHP = 28f;
            ai.HP = 28f;
            ai.moveSpeed = 2.0f;
            ai.damage = 7f;
            ai.attackCooldown = 1.2f;
            ai.aggroRange = 5f;
            ai.attackRange = 0.9f;
            ai.xpReward = 12f;
            ai.drops = new[] { new ResourceNode.Drop { item = meatDrop, min = 1, max = 3 } };
            ai.playerMask = ~0;
            AttachDropShadow(go, offsetY: -0.3f, scaleX: 0.95f, scaleY: 0.35f);
            AttachMobHitFx(go, knockbackImpulse: 2.5f);
            // PR M: PuppetAnimController always active (placeholder skeleton fallback). MobAnimController
            // legacy path retired — IMobAnim resolution picks PuppetAnimController qua GetComponent.
            return SaveAsPrefab(go, $"{PrefabsDir}/Wolf.prefab");
        }

        static GameObject BuildFoxSpiritPrefab(Sprite sprite, ItemSO meatDrop)
        {
            var go = new GameObject("FoxSpirit");
            // Puppet path: Art/Characters/fox_spirit/ tunings nhanh hơn Wolf — fox spirit
            // nimble: 5.5Hz step, longer arm swing 35°, tail sway 24° (fox tail vẫy mạnh).
            // PR J multi-dir support: flat folder (legacy) → East-only; E/N/S subfolders → full multi-dir.
            // PR M: no real PNG → placeholder skeleton (motion demo runs ngay).
            var puppetSet = CharacterArtImporter.TryLoadCharacterSpriteSet("fox_spirit", placeholderHeightPx: 32)
                ?? BuildPlaceholderSpriteSet("fox_spirit", includeTail: true);
            SpriteRenderer sr = null;
            {
                BuildPuppetHierarchy(go, puppetSet.EastSprites, sortingOrderBase: 3,
                    out var spriteRoot, out var torsoT, out var headT,
                    out var armLT, out var armRT, out var legLT, out var legRT, out var tailT,
                    out var foreLT, out var foreRT, out var shinLT, out var shinRT);
                var puppet = go.AddComponent<PuppetAnimController>();
                puppet.spriteRoot = spriteRoot;
                puppet.torso = torsoT;
                puppet.head = headT;
                puppet.armLeft = armLT;
                puppet.armRight = armRT;
                puppet.legLeft = legLT;
                puppet.legRight = legRT;
                puppet.tail = tailT;
                puppet.forearmLeft = foreLT;
                puppet.forearmRight = foreRT;
                puppet.shinLeft = shinLT;
                puppet.shinRight = shinRT;
                puppet.walkFrequency = 5.5f;
                puppet.armSwingDeg = 35f;
                puppet.legSwingDeg = 26f;
                puppet.tailSwayDeg = 24f;
                puppet.tailSwayFrequency = 2.2f;
                puppet.referenceSpeed = 2.6f;
                WirePuppetMultiDirSprites(puppet, puppetSet);
            }
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            var ai = go.AddComponent<FoxSpiritAI>();
            ai.spriteRenderer = sr;
            ai.maxHP = 18f;
            ai.HP = 18f;
            ai.moveSpeed = 2.6f;
            ai.damage = 5f;
            ai.attackCooldown = 0.9f;
            ai.aggroRange = 6f;
            ai.attackRange = 0.8f;
            ai.xpReward = 18f;
            ai.drops = new[] { new ResourceNode.Drop { item = meatDrop, min = 1, max = 2 } };
            ai.playerMask = ~0;
            AttachDropShadow(go, offsetY: -0.3f, scaleX: 0.85f, scaleY: 0.32f);
            AttachMobHitFx(go, knockbackImpulse: 2.0f);
            // PR M: PuppetAnimController always active.
            return SaveAsPrefab(go, $"{PrefabsDir}/FoxSpirit.prefab");
        }

        static GameObject BuildBoarPrefab(Sprite sprite, ItemSO meat, ItemSO hide, ItemSO tusk)
        {
            var go = new GameObject("Boar");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            var ai = go.AddComponent<BoarAI>();
            ai.spriteRenderer = sr;
            ai.maxHP = 45f;
            ai.HP = 45f;
            ai.moveSpeed = 1.4f;
            ai.damage = 8f;
            ai.attackCooldown = 1.4f;
            ai.attackRange = 0.9f;
            ai.xpReward = 18f;
            ai.drops = new[]
            {
                new ResourceNode.Drop { item = meat, min = 2, max = 3 },
                new ResourceNode.Drop { item = hide, min = 1, max = 2 },
                new ResourceNode.Drop { item = tusk, min = 0, max = 1 },
            };
            ai.playerMask = ~0;
            AttachDropShadow(go, offsetY: -0.35f, scaleX: 1.1f, scaleY: 0.4f);
            AttachMobHitFx(go, knockbackImpulse: 3.5f);
            AttachMobAnim(go, walkBobFreq: 3f, maxTiltDeg: 3f, walkBobAmp: 0.04f);
            return SaveAsPrefab(go, $"{PrefabsDir}/Boar.prefab");
        }

        static GameObject BuildDeerSpiritPrefab(Sprite sprite, ItemSO spiritMeat, ItemSO antler)
        {
            var go = new GameObject("DeerSpirit");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            var ai = go.AddComponent<DeerSpiritAI>();
            ai.spriteRenderer = sr;
            ai.maxHP = 16f;
            ai.HP = 16f;
            ai.moveSpeed = 2.0f;
            ai.xpReward = 14f;
            ai.drops = new[]
            {
                new ResourceNode.Drop { item = spiritMeat, min = 1, max = 2 },
                new ResourceNode.Drop { item = antler, min = 0, max = 1 },
            };
            ai.playerMask = ~0;
            AttachDropShadow(go, offsetY: -0.35f, scaleX: 0.9f, scaleY: 0.35f);
            AttachMobHitFx(go, knockbackImpulse: 2.2f);
            AttachMobAnim(go, walkBobFreq: 4f, maxTiltDeg: 5f);
            return SaveAsPrefab(go, $"{PrefabsDir}/DeerSpirit.prefab");
        }

        static GameObject BuildCrowPrefab(Sprite sprite, ItemSO feather)
        {
            var go = new GameObject("Crow");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.isTrigger = true;
            var ai = go.AddComponent<CrowAI>();
            ai.spriteRenderer = sr;
            ai.maxHP = 6f;
            ai.HP = 6f;
            ai.moveSpeed = 1.8f;
            ai.xpReward = 4f;
            ai.drops = new[] { new ResourceNode.Drop { item = feather, min = 1, max = 2 } };
            ai.playerMask = ~0;
            // Crow bay → shadow nhỏ, offsetY thấp hơn entity để cảm giác bay trên không.
            AttachDropShadow(go, offsetY: -0.45f, scaleX: 0.55f, scaleY: 0.22f);
            AttachMobHitFx(go, knockbackImpulse: 1.2f);
            AttachMobAnim(go, walkBobFreq: 7f, maxTiltDeg: 4f, walkBobAmp: 0.03f);
            return SaveAsPrefab(go, $"{PrefabsDir}/Crow.prefab");
        }

        static GameObject BuildSnakePrefab(Sprite sprite, ItemSO skin, ItemSO venom, StatusEffectSO poison)
        {
            var go = new GameObject("Snake");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.isTrigger = true;
            var ai = go.AddComponent<SnakeAI>();
            ai.spriteRenderer = sr;
            ai.maxHP = 14f;
            ai.HP = 14f;
            ai.moveSpeed = 1.4f;
            ai.damage = 6f;
            ai.attackRange = 0.7f;
            ai.attackCooldown = 1.0f;
            ai.aggroRange = 2.5f;
            ai.revealRange = 2.5f;
            ai.giveUpRange = 5f;
            ai.poisonEffect = poison;
            ai.poisonDuration = 6f;
            ai.xpReward = 8f;
            ai.drops = new[]
            {
                new ResourceNode.Drop { item = skin,  min = 1, max = 1 },
                new ResourceNode.Drop { item = venom, min = 0, max = 1 },
            };
            ai.playerMask = ~0;
            AttachDropShadow(go, offsetY: -0.15f, scaleX: 0.7f, scaleY: 0.22f);
            AttachMobHitFx(go, knockbackImpulse: 1.8f);
            AttachMobAnim(go, walkBobFreq: 5f, maxTiltDeg: 4f, walkBobAmp: 0.04f);
            return SaveAsPrefab(go, $"{PrefabsDir}/Snake.prefab");
        }

        static GameObject BuildBatPrefab(Sprite sprite, ItemSO wing, StatusEffectSO bleed)
        {
            var go = new GameObject("Bat");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.isTrigger = true;
            var ai = go.AddComponent<BatAI>();
            ai.spriteRenderer = sr;
            ai.maxHP = 10f;
            ai.HP = 10f;
            ai.moveSpeed = 2.0f;
            ai.damage = 4f;
            ai.attackRange = 0.7f;
            ai.attackCooldown = 0.9f;
            ai.aggroRange = 4.5f;
            ai.bleedEffect = bleed;
            ai.bleedDuration = 4f;
            ai.xpReward = 6f;
            ai.drops = new[] { new ResourceNode.Drop { item = wing, min = 1, max = 1 } };
            ai.playerMask = ~0;
            // Bat bay → shadow thấp hơn.
            AttachDropShadow(go, offsetY: -0.5f, scaleX: 0.6f, scaleY: 0.22f);
            AttachMobHitFx(go, knockbackImpulse: 1.5f);
            AttachMobAnim(go, walkBobFreq: 7.5f, maxTiltDeg: 4f, walkBobAmp: 0.03f);
            return SaveAsPrefab(go, $"{PrefabsDir}/Bat.prefab");
        }

        // ========== Flora (wild plants) — PR C ==========

        /// <summary>
        /// Helper: build basic plant ResourceNode prefab. caller customize side-effects sau.
        /// </summary>
        static GameObject MakePlantNode(string name, Sprite sprite, ItemSO drop, float maxHP,
            int dropMin, int dropMax, float radius)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = radius;
            col.isTrigger = false;
            var node = go.AddComponent<ResourceNode>();
            node.nodeName = name;
            node.maxHP = maxHP;
            node.currentHP = maxHP;
            node.drops = new[]
            {
                new ResourceNode.Drop { item = drop, min = dropMin, max = dropMax },
            };
            return go;
        }

        static GameObject BuildLinhMushroomPrefab(Sprite sprite, ItemSO mushroom)
        {
            // Hand-pick (HP rất thấp); food + low heal đã có trong ItemSO.restoreHunger.
            var go = MakePlantNode("LinhMushroom", sprite, mushroom, maxHP: 1f,
                dropMin: 1, dropMax: 2, radius: 0.25f);
            AttachReactiveOnHit(go, ReactivePreset.Plant);
            AttachPlayerOverlapSway(go, maxBendDeg: 5f, detectRadius: 0.5f);
            return SaveAsPrefab(go, $"{PrefabsDir}/LinhMushroom.prefab");
        }

        static GameObject BuildBerryBushPrefab(Sprite sprite, ItemSO berry)
        {
            // Bụi berry — hand-pick, drop 2-4 berry.
            var go = MakePlantNode("BerryBush", sprite, berry, maxHP: 1f,
                dropMin: 2, dropMax: 4, radius: 0.30f);
            AttachReactiveOnHit(go, ReactivePreset.Bush);
            AttachPlayerOverlapSway(go, maxBendDeg: 8f, detectRadius: 0.6f);
            return SaveAsPrefab(go, $"{PrefabsDir}/BerryBush.prefab");
        }

        static GameObject BuildCactusPrefab(Sprite sprite, ItemSO cactusWater)
        {
            // Cactus — pick lấy nước nhưng prick -2 HP. HP cao hơn để đập có nỗ lực.
            var go = MakePlantNode("Cactus", sprite, cactusWater, maxHP: 6f,
                dropMin: 1, dropMax: 2, radius: 0.30f);
            var node = go.GetComponent<ResourceNode>();
            node.harvestHpDamage = 2f;
            node.harvestThirstRestore = 0f; // restore qua cactus_water item, không qua side-effect
            // Cactus cứng — sway nhẹ (không đổ như cỏ mềm), biên độ nhỏ.
            AttachReactiveOnHit(go, ReactivePreset.Cactus);
            AttachPlayerOverlapSway(go, maxBendDeg: 3f, detectRadius: 0.5f);
            return SaveAsPrefab(go, $"{PrefabsDir}/Cactus.prefab");
        }

        static GameObject BuildDeathLilyPrefab(Sprite sprite, ItemSO deathPollen)
        {
            // Death Lily — pick = -5 SAN, drop death_pollen alchemy.
            var go = MakePlantNode("DeathLily", sprite, deathPollen, maxHP: 1f,
                dropMin: 1, dropMax: 1, radius: 0.25f);
            var node = go.GetComponent<ResourceNode>();
            node.harvestSanityDamage = 5f;
            AttachReactiveOnHit(go, ReactivePreset.Plant);
            AttachPlayerOverlapSway(go, maxBendDeg: 6f, detectRadius: 0.5f);
            return SaveAsPrefab(go, $"{PrefabsDir}/DeathLily.prefab");
        }

        static GameObject BuildLinhBambooPrefab(Sprite sprite, ItemSO bamboo, ItemSO stick)
        {
            // Linh Bamboo — đập như cây thường. drop bamboo + stick.
            var go = new GameObject("LinhBamboo");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.30f;
            var node = go.AddComponent<ResourceNode>();
            node.nodeName = "LinhBamboo";
            node.maxHP = 4f;
            node.currentHP = 4f;
            node.drops = new[]
            {
                new ResourceNode.Drop { item = bamboo, min = 1, max = 2 },
                new ResourceNode.Drop { item = stick,  min = 0, max = 1 },
            };
            AttachReactiveOnHit(go, ReactivePreset.Bamboo);
            // Bamboo cao — sway nhẹ như cây, không bổ dồn như bụi.
            AttachPlayerOverlapSway(go, maxBendDeg: 5f, detectRadius: 0.55f);
            return SaveAsPrefab(go, $"{PrefabsDir}/LinhBamboo.prefab");
        }

        static GameObject BuildMineralRockPrefab(Sprite sprite, ItemSO ore, ItemSO stone)
        {
            // Mineral Rock — đá mỏ chỉ ở Đá Sơn. drop mineral_ore + stone.
            var go = new GameObject("MineralRock");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;
            sr.color = new Color(0.55f, 0.55f, 0.70f); // tinge tím nhạt phân biệt rock thường
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            var node = go.AddComponent<ResourceNode>();
            node.nodeName = "MineralRock";
            node.maxHP = 10f;
            node.currentHP = 10f;
            node.drops = new[]
            {
                new ResourceNode.Drop { item = ore,   min = 1, max = 2 },
                new ResourceNode.Drop { item = stone, min = 1, max = 2 },
            };
            AttachReactiveOnHit(go, ReactivePreset.Rock);
            // Mineral rock crack — tint tím nhạt hơn rock thường (match base sprite tint).
            AttachProgressiveCrack(go, tint: new Color(0.20f, 0.15f, 0.20f, 1f));
            return SaveAsPrefab(go, $"{PrefabsDir}/MineralRock.prefab");
        }

        static GameObject BuildCampfirePrefab(Sprite sprite, ItemSO woodItem)
        {
            var go = new GameObject("Campfire");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;
            // CraftStationMarker phải thêm trước Campfire — Campfire có
            // [RequireComponent(typeof(CraftStationMarker))]; thêm Campfire trước sẽ
            // auto-add một CraftStationMarker rồi line sau add thêm cái thứ hai (duplicate).
            var marker = go.AddComponent<CraftStationMarker>();
            marker.station = CraftStation.Campfire;
            var fire = go.AddComponent<Campfire>();
            fire.woodItem = woodItem;
            fire.flameRenderer = sr;
            go.AddComponent<LightSource>();
            return SaveAsPrefab(go, $"{PrefabsDir}/Campfire.prefab");
        }

        static GameObject BuildWaterSpringPrefab(Sprite sprite, ItemSO waterItem, ItemSO rawFish)
        {
            var go = new GameObject("WaterSpring");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.6f;
            col.isTrigger = true;
            var w = go.AddComponent<WaterSpring>();
            w.cleanWaterItem = waterItem;

            // FishingSpot trên cùng GameObject — player đứng gần là vừa uống vừa câu được.
            var spot = go.AddComponent<FishingSpot>();
            spot.castRangeFromSpot = 2.5f;
            spot.castTimeSeconds = new Vector2(3f, 7f);
            spot.lootTable = new[]
            {
                new FishingSpot.LootEntry { item = rawFish, weight = 1f, min = 1, max = 1 },
            };
            // Ripple ring expand khi player tới gần — feedback "nước phản ứng".
            AttachWaterRipple(go, triggerRadius: 1f, cooldown: 1.2f);
            return SaveAsPrefab(go, $"{PrefabsDir}/WaterSpring.prefab");
        }

        static GameObject BuildStorageChestPrefab(Sprite sprite)
        {
            var go = new GameObject("StorageChest");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            // Collider2D bắt buộc để InteractAction.OverlapCircleAll tìm thấy. Trừ các
            // RequireComponent của StorageChest (chỉ cần Inventory) — và Collider2D phải
            // có isTrigger=true để player không bị vách khi đi đến.
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            col.isTrigger = true;
            // StorageChest [RequireComponent(typeof(Inventory))] → Unity auto-add Inventory
            // với default slotCount=16. Phải set tường minh cả hai — Inventory.Awake() có thể
            // chạy trước StorageChest.Awake() (sibling Awake order không deterministic),
            // làm khởi tạo 16 slot trước khi StorageChest có cơ hội đồng bộ về 12.
            const int chestSlots = 12;
            var chest = go.AddComponent<StorageChest>();
            chest.slotCount = chestSlots;
            chest.interactLabel = "Mở rương";
            var chestInv = go.GetComponent<Inventory>();
            if (chestInv != null) chestInv.slotCount = chestSlots;
            return SaveAsPrefab(go, $"{PrefabsDir}/StorageChest.prefab");
        }

        static GameObject BuildWorkbenchPrefab(Sprite sprite, ItemSO repairMaterial)
        {
            var go = new GameObject("Workbench");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.55f;
            col.isTrigger = true;
            // Workbench [RequireComponent(typeof(CraftStationMarker))] → marker tự add.
            // Workbench.Awake() sẽ set marker.station = CraftStation.Workbench.
            var wb = go.AddComponent<Workbench>();
            wb.repairMaterial = repairMaterial;
            wb.repairCost = 1;
            wb.repairAmount = -1f; // sửa full về maxDurability
            return SaveAsPrefab(go, $"{PrefabsDir}/Workbench.prefab");
        }

        static GameObject BuildProjectilePrefab(Sprite sprite, StatusEffectSO burn)
        {
            var go = new GameObject("Projectile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 6;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;
            var p = go.AddComponent<Projectile>();
            p.speed = 8f;
            p.damage = 12f;
            p.lifetime = 3f;
            p.element = SpiritElement.Hoa;
            p.onHitStatusEffect = burn;
            p.onHitStatusDuration = 4f;
            return SaveAsPrefab(go, $"{PrefabsDir}/Projectile.prefab");
        }

        static GameObject SaveAsPrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ---------------- SCENE ----------------
        static void BuildScene(PrefabBundle prefabs, Dictionary<string, ItemSO> items,
            List<RecipeSO> recipes, List<SpiritRootSO> spiritRoots, List<BiomeSO> biomes,
            Dictionary<string, Sprite> sprites, ItemDatabase itemDb)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.55f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraShake>(); // juice: shake on damage / breakthrough
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 0, -10);

            // GameManager (top-level managers without RequireComponent siblings)
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            var time = gmGo.AddComponent<TimeManager>();
            var saveLoad = gmGo.AddComponent<SaveLoadController>();
            gmGo.AddComponent<AudioManager>(); // placeholder procedural SFX — artists có thể gán sfxOverrides sau
            gm.timeManager = time;

            // Player instance (Player prefab carries RealmSystem + CraftingSystem + Inventory + PlayerStats)
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Player);
            player.transform.position = Vector3.zero;
            gm.player = player.transform;

            // Wire CraftingSystem.knownRecipes + stationMask (default 0 → station-required
            // recipe luôn fail vì OverlapCircleAll không tìm thấy CraftStationMarker nào).
            var craft = player.GetComponent<CraftingSystem>();
            craft.knownRecipes = recipes;
            craft.stationMask = ~0;

            // Wire inventory ref vào các action component cần item-từ-inventory.
            var inv = player.GetComponent<Inventory>();
            var torch = player.GetComponent<TorchAction>();
            if (torch != null)
            {
                torch.torchItem = items["torch"];
                torch.inventory = inv;
            }
            var combatRef = player.GetComponent<PlayerCombat>();
            if (combatRef != null) combatRef.inventory = inv;
            var ctrlRef = player.GetComponent<PlayerController>();
            if (ctrlRef != null) ctrlRef.inventory = inv;
            var magic = player.GetComponent<MagicTreasureAction>();
            if (magic != null) magic.inventory = inv;
            var fishing = player.GetComponent<FishingAction>();
            if (fishing != null)
            {
                fishing.rodItem = items["fishing_rod"];
                fishing.inventory = inv;
                fishing.controller = ctrlRef;
            }

            // Wire SpiritRoot candidate pool (random roll on start)
            var sr = player.GetComponent<SpiritRoot>();
            sr.candidatePool = spiritRoots.ToArray();
            sr.rollOnStart = true;

            // Demo objectives tracker (gắn trên Player để auto-find Inventory/RealmSystem/MeditationAction).
            if (player.GetComponent<DemoObjectivesTracker>() == null)
                player.AddComponent<DemoObjectivesTracker>();

            // Wire SaveLoadController
            saveLoad.playerStats = player.GetComponent<PlayerStats>();
            saveLoad.playerCombat = player.GetComponent<PlayerCombat>();
            saveLoad.realm = player.GetComponent<RealmSystem>();
            saveLoad.inventory = player.GetComponent<Inventory>();
            saveLoad.timeManager = time;
            saveLoad.itemDatabase = itemDb;
            saveLoad.spiritRoot = sr;
            saveLoad.spiritRootCatalog = spiritRoots.ToArray();

            // World container + WorldGenerator
            var worldGo = new GameObject("World");
            var wg = worldGo.AddComponent<WorldGenerator>();
            // 256×256 = 64 chunks (chunkSize=16) × 64 chunks. Đủ rộng cho user
            // demo wrap-around (đi >256 cells theo 1 hướng → loop về). ChunkManager
            // bên dưới chỉ load render window quanh player → KHÔNG hang Start.
            wg.size = new Vector2Int(256, 256);
            wg.seed = 12345;
            wg.contentParent = worldGo.transform;
            wg.player = player.transform;
            // Legacy fallback prefabs (dùng khi không có biome).
            wg.treePrefab = prefabs.Tree;
            wg.rockPrefab = prefabs.Rock;
            wg.waterSpringPrefab = prefabs.WaterSpring;
            // Grass-tile decoration: rabbit ăn được, persist harvest qua WorldSaveData.
            wg.grassTilePrefab = prefabs.GrassTile;

            // Tilemap-based ground render: Grid > Ground (Tilemap + TilemapRenderer). Render
            // 1 batch thay vì N×M GameObjects. SortingOrder âm để mob/player/resource đè lên.
            var gridGo = new GameObject("Grid", typeof(Grid));
            gridGo.transform.SetParent(worldGo.transform, false);
            var groundGo = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer));
            groundGo.transform.SetParent(gridGo.transform, false);
            var groundTilemap = groundGo.GetComponent<Tilemap>();
            var groundRenderer = groundGo.GetComponent<TilemapRenderer>();
            groundRenderer.sortingOrder = -10;
            wg.groundTilemap = groundTilemap;

            // Default ground tile từ sprite "ground" placeholder. Per-biome tile share cùng asset
            // ở MVP — artist có thể thay biome.groundTile bằng Tile khác (sand / leaf / cobble) sau.
            var defaultGroundTile = CreateGroundTile("ground_default", sprites["ground"]);
            wg.legacyGroundTile = defaultGroundTile;

            // Per-biome prefabs (BiomeSO chứa từng prefab riêng). Nếu có sprite thật ở
            // Assets/_Project/Art/Tiles/{biomeId}/ → BiomeTileImporter wire vào groundTileVariants[],
            // WorldGenerator.PickGroundTile() sẽ pick variant deterministic per cell. Empty folder
            // → giữ placeholder groundTile fallback.
            foreach (var b in biomes)
            {
                b.treePrefab = prefabs.Tree;
                b.rockPrefab = prefabs.Rock;
                b.waterSpringPrefab = prefabs.WaterSpring;
                b.groundTile = defaultGroundTile;
                var realVariants = BiomeTileImporter.ImportBiomeTiles(b.biomeId);
                if (realVariants.Length > 0)
                {
                    BiomeTileImporter.WireVariantsToBiome(b, realVariants);
                }
                b.extraNodes = BuildExtraNodesFor(b.biomeId, prefabs);
                EditorUtility.SetDirty(b);
            }
            wg.biomes = biomes.ToArray();

            // MobSpawner: Rabbit (passive day/night), Wolf (aggressive cả ngày lẫn đêm),
            // FoxSpirit (đêm-only — dayCap=0). Boar/DeerSpirit/Crow là day-only neutral/passive.
            var spawner = worldGo.AddComponent<MobSpawner>();
            spawner.entries = new[]
            {
                new MobSpawner.SpawnEntry { prefab = prefabs.Rabbit,     dayCap = 6, nightCap = 3 },
                new MobSpawner.SpawnEntry { prefab = prefabs.Wolf,       dayCap = 2, nightCap = 3 },
                new MobSpawner.SpawnEntry { prefab = prefabs.FoxSpirit,  dayCap = 0, nightCap = 4 },
                new MobSpawner.SpawnEntry { prefab = prefabs.Boar,       dayCap = 2, nightCap = 1 },
                new MobSpawner.SpawnEntry { prefab = prefabs.DeerSpirit, dayCap = 3, nightCap = 1 },
                new MobSpawner.SpawnEntry { prefab = prefabs.Crow,       dayCap = 4, nightCap = 1 },
                new MobSpawner.SpawnEntry { prefab = prefabs.Snake,      dayCap = 2, nightCap = 2 },
                new MobSpawner.SpawnEntry { prefab = prefabs.Bat,        dayCap = 0, nightCap = 5 },
            };
            spawner.parent = worldGo.transform;
            wg.mobSpawner = spawner;
            saveLoad.worldGenerator = wg;

            // ChunkManager: render window 4-chunk radius (=9×9=81 chunks active = 81×256
            // = 20k cells render quanh player). World 256² = 64×64 chunks → ChunkManager
            // dynamic load/unload khi player di chuyển. WorldGenerator.Start() detect
            // ChunkManager component → skip global Generate.
            var chunkMgr = worldGo.AddComponent<ChunkManager>();
            chunkMgr.player = player.transform;
            chunkMgr.mobSpawner = spawner;
            chunkMgr.renderRadiusChunks = 4;

            // Place 1 campfire near spawn. WorldGenerator.Start() teleports player to
            // (size.x*0.5, size.y*0.5, 0) = (20, 20, 0) với size 40x40, nên campfire phải
            // đặt gần đó chứ không phải gần origin.
            var fire = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Campfire);
            fire.transform.position = new Vector3(wg.size.x * 0.5f + 2.5f, wg.size.y * 0.5f, 0f);

            // Place 1 storage chest cạnh campfire để demo lưu trữ.
            var chestGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.StorageChest);
            chestGo.transform.position = new Vector3(wg.size.x * 0.5f + 4.0f, wg.size.y * 0.5f, 0f);

            // Place 1 workbench sửa đồ cạnh chest. CraftingSystem.stationDetectRadius=2 nên
            // đặt trong khuảng player có thể reach cả campfire lẫn workbench (kề nhau ≈ 1.5u).
            var workbenchGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Workbench);
            workbenchGo.transform.position = new Vector3(wg.size.x * 0.5f - 1.0f, wg.size.y * 0.5f, 0f);

            // EventSystem (required for UI input — Buttons, Joystick, etc.)
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();

            // UI Canvas
            var canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            BuildUI(canvasGo, player, sprites["ui_white"]);

            // Camera follow player by parenting (đơn giản — không cần script follow)
            camGo.transform.SetParent(player.transform);
            camGo.transform.localPosition = new Vector3(0, 0, -10);

            // Save scene + add to build settings
            EditorSceneManager.SaveScene(scene, MainScenePath);
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == MainScenePath))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(MainScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            Debug.Log("[Bootstrap] MainScene saved at " + MainScenePath);
        }

        // Build full UI: stat bars + joystick + skill buttons + inventory + crafting + realm.
        static void BuildUI(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            BuildDayNightTintOverlay(canvas, whiteSprite); // đầu tiên → ở back, mọi HUD render trên
            BuildStatBars(canvas, player, whiteSprite);
            BuildVirtualJoystick(canvas, player, whiteSprite);
            BuildSkillButtons(canvas, whiteSprite);
            BuildInventoryUI(canvas, player, whiteSprite);
            BuildCraftingUI(canvas, player, whiteSprite);
            BuildRealmUI(canvas, player, whiteSprite);
            BuildStorageChestUI(canvas, player, whiteSprite);
            BuildTutorialHUD(canvas, player, whiteSprite);
            BuildPauseMenu(canvas, whiteSprite);
            BuildInteractPrompt(canvas, player, whiteSprite);
            BuildMinimap(canvas, player, whiteSprite);
            BuildDamageNumberLayer(canvas, player);
        }

        // ---------- Minimap (RawImage + 2nd ortho top-down camera + RenderTexture) ----------
        static void BuildMinimap(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            // Camera GO — child của player để follow tự động. Render xuống RenderTexture.
            var camGo = new GameObject("MinimapCamera");
            camGo.transform.SetParent(player.transform, false);
            camGo.transform.localPosition = new Vector3(0, 0, -20f); // z xa hơn main cam (-10) để không chia view
            camGo.transform.localRotation = Quaternion.identity;
            var mcam = camGo.AddComponent<Camera>();
            mcam.orthographic = true;
            mcam.orthographicSize = 9f; // zoom-out so với main (6) → minimap nhìn rộng hơn
            mcam.backgroundColor = new Color(0.10f, 0.18f, 0.10f, 1f); // xanh đậm tối nền
            mcam.clearFlags = CameraClearFlags.SolidColor;
            mcam.depth = -2; // âm hơn main cam → render trước, khỏi block AudioListener etc.
            // Cull UI layer (ScreenSpaceOverlay UI vốn không vào camera ortho world cam, nhưng UI layer mặc định = layer 5).
            mcam.cullingMask = ~(1 << 5);
            mcam.useOcclusionCulling = false;
            mcam.allowMSAA = false;
            mcam.allowHDR = false;

            // Frame UI top-right (180×180 viewport, 220×220 frame để chừa border).
            const float framePad = 8f;
            const float frameSize = 200f;
            var frameGo = new GameObject("MinimapFrame", typeof(RectTransform), typeof(Image));
            frameGo.transform.SetParent(canvas.transform, false);
            var frt = (RectTransform)frameGo.transform;
            frt.anchorMin = frt.anchorMax = new Vector2(1f, 1f);
            frt.pivot = new Vector2(1f, 1f);
            frt.anchoredPosition = new Vector2(-framePad, -framePad);
            frt.sizeDelta = new Vector2(frameSize, frameSize);
            var frameBg = frameGo.GetComponent<Image>();
            frameBg.sprite = whiteSprite;
            frameBg.color = new Color(0.05f, 0.05f, 0.10f, 0.85f);
            frameBg.raycastTarget = false;

            // RawImage (viewport thực tế nằm trong frame, padding 6).
            const float padInner = 6f;
            var viewGo = new GameObject("MinimapView",
                typeof(RectTransform), typeof(RawImage), typeof(MinimapController));
            viewGo.transform.SetParent(frameGo.transform, false);
            var vrt = (RectTransform)viewGo.transform;
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = new Vector2(padInner, padInner);
            vrt.offsetMax = new Vector2(-padInner, -padInner);
            var raw = viewGo.GetComponent<RawImage>();
            raw.raycastTarget = false;
            var ctrl = viewGo.GetComponent<MinimapController>();
            ctrl.minimapCamera = mcam;

            // Player dot (chấm trắng giữa viewport — vì camera parent vào player nên player luôn ở center).
            var dotGo = new GameObject("PlayerDot", typeof(RectTransform), typeof(Image));
            dotGo.transform.SetParent(viewGo.transform, false);
            var drt = (RectTransform)dotGo.transform;
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.pivot = new Vector2(0.5f, 0.5f);
            drt.anchoredPosition = Vector2.zero;
            drt.sizeDelta = new Vector2(10, 10);
            var dotImg = dotGo.GetComponent<Image>();
            dotImg.sprite = whiteSprite;
            dotImg.color = new Color(1f, 0.95f, 0.2f, 1f);
            dotImg.raycastTarget = false;

            // Label "Bản đồ" trên frame (top edge).
            AddTMPLabel(frameGo, "Bản đồ", 12, new Color(1f, 1f, 1f, 0.6f),
                anchor: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
                anchoredPos: new Vector2(0, -3), size: new Vector2(80, 16),
                alignment: TMPro.TextAlignmentOptions.Center);
        }

        // ---------- Damage number layer (float-up TMP text khi damage event) ----------
        static void BuildDamageNumberLayer(GameObject canvas, GameObject player)
        {
            // Layer GameObject là RectTransform full-screen, không Image — chỉ làm parent cho
            // các DamageNumber spawn runtime. Đặt sau cùng để render trên các UI HUD khác.
            var layerGo = new GameObject("DamageNumberLayer", typeof(RectTransform));
            layerGo.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)layerGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var spawner = layerGo.AddComponent<DamageNumberSpawner>();
            spawner.canvasRect = (RectTransform)canvas.transform;
            // worldCamera tự lấy Camera.main trong Awake; không cần gán ở đây.
        }

        // ---------- Day/Night tint overlay (full-screen UI Image lerp tint theo TimeManager) ----------
        static void BuildDayNightTintOverlay(GameObject canvas, Sprite whiteSprite)
        {
            var go = new GameObject("DayNightTintOverlay",
                typeof(RectTransform), typeof(Image), typeof(DayNightTintOverlay));
            go.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = whiteSprite;
            img.raycastTarget = false; // không chặn click HUD bên dưới
            // timeManager reference tự lookup trong DayNightTintOverlay.Awake.
        }

        // ---------- Interact prompt (pop-up "[E] <label>" ở bottom-center khi có target) ----------
        static void BuildInteractPrompt(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            // Root panel — ẩn mặc định, toggle bởi InteractPromptUI khi có CurrentTarget.
            var rootGo = new GameObject("InteractPrompt",
                typeof(RectTransform), typeof(Image));
            rootGo.transform.SetParent(canvas.transform, false);
            var rt = (RectTransform)rootGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            // Đặt trên joystick + inventory bar (joystick ở y~160, inventory ở y~300).
            rt.anchoredPosition = new Vector2(0, 340);
            rt.sizeDelta = new Vector2(420, 44);
            var bg = rootGo.GetComponent<Image>();
            bg.sprite = whiteSprite;
            bg.color = new Color(0.05f, 0.05f, 0.10f, 0.85f);
            bg.raycastTarget = false;

            // Label căn trái, chừa slot cho nút bấm mobile bên phải.
            var label = AddTMPLabel(rootGo, "", 18, Color.white,
                anchor: new Vector2(0, 0.5f), pivot: new Vector2(0, 0.5f),
                anchoredPos: new Vector2(14, 0), size: new Vector2(320, 36),
                alignment: TextAlignmentOptions.Left);

            // Nút bấm tròn bên phải (mobile tap).
            var btnGo = new GameObject("InteractTapBtn",
                typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(rootGo.transform, false);
            var brt = (RectTransform)btnGo.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(1, 0.5f);
            brt.pivot = new Vector2(1, 0.5f);
            brt.anchoredPosition = new Vector2(-8, 0);
            brt.sizeDelta = new Vector2(72, 36);
            var bImg = btnGo.GetComponent<Image>();
            bImg.sprite = whiteSprite;
            bImg.color = new Color(0.25f, 0.55f, 0.30f, 1f);
            AddTMPLabel(btnGo, "E", 18, Color.white,
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);

            rootGo.SetActive(false);

            // Gắn UI controller lên canvas (cùng pattern với StatBarUI).
            var ui = canvas.AddComponent<InteractPromptUI>();
            ui.interactAction = player.GetComponent<InteractAction>();
            ui.promptRoot = rootGo;
            ui.label = label;
            ui.button = btnGo.GetComponent<Button>();
        }

        static void BuildStatBars(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            var statBarUI = canvas.AddComponent<StatBarUI>();
            statBarUI.stats = player.GetComponent<PlayerStats>();
            statBarUI.controller = player.GetComponent<PlayerController>();
            statBarUI.inventory = player.GetComponent<Inventory>();

            statBarUI.hpFill          = MakeBar(canvas, whiteSprite, "HP",       new Vector2(10, -10),  new Color(0.85f, 0.20f, 0.25f));
            statBarUI.hungerFill      = MakeBar(canvas, whiteSprite, "Hunger",   new Vector2(10, -28),  new Color(0.92f, 0.65f, 0.25f));
            statBarUI.thirstFill      = MakeBar(canvas, whiteSprite, "Thirst",   new Vector2(10, -46),  new Color(0.30f, 0.70f, 0.95f));
            statBarUI.sanityFill      = MakeBar(canvas, whiteSprite, "Sanity",   new Vector2(10, -64),  new Color(0.65f, 0.45f, 0.85f));
            statBarUI.manaFill        = MakeBar(canvas, whiteSprite, "Mana",     new Vector2(10, -82),  new Color(0.40f, 0.85f, 0.85f));
            statBarUI.bodyTempFill    = MakeBar(canvas, whiteSprite, "BodyTemp", new Vector2(10, -100), new Color(0.55f, 0.85f, 0.55f));
            statBarUI.encumbranceFill = MakeBar(canvas, whiteSprite, "Encumber", new Vector2(10, -118), new Color(0.85f, 0.85f, 0.40f));
        }

        static Image MakeBar(GameObject canvas, Sprite whiteSprite, string name, Vector2 anchoredPos, Color color)
        {
            var bgGo = new GameObject(name + "_BG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(canvas.transform, false);
            var bgRT = (RectTransform)bgGo.transform;
            bgRT.anchorMin = bgRT.anchorMax = new Vector2(0, 1);
            bgRT.pivot = new Vector2(0, 1);
            bgRT.anchoredPosition = anchoredPos;
            bgRT.sizeDelta = new Vector2(180, 14);
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = whiteSprite;
            bgImg.color = new Color(0, 0, 0, 0.55f);

            var fillGo = new GameObject(name + "_Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(bgGo.transform, false);
            var fRT = (RectTransform)fillGo.transform;
            fRT.anchorMin = new Vector2(0, 0);
            fRT.anchorMax = new Vector2(1, 1);
            fRT.offsetMin = new Vector2(2, 2);
            fRT.offsetMax = new Vector2(-2, -2);
            var fImg = fillGo.GetComponent<Image>();
            fImg.sprite = whiteSprite;
            fImg.type = Image.Type.Filled;
            fImg.fillMethod = Image.FillMethod.Horizontal;
            fImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fImg.color = color;
            return fImg;
        }

        // ---------- Virtual joystick (bottom-left) ----------
        static void BuildVirtualJoystick(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            const float radius = 110f;

            var bgGo = new GameObject("Joystick_BG", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(canvas.transform, false);
            var bgRT = (RectTransform)bgGo.transform;
            bgRT.anchorMin = bgRT.anchorMax = new Vector2(0, 0);
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.anchoredPosition = new Vector2(180, 180);
            bgRT.sizeDelta = new Vector2(radius * 2f, radius * 2f);
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = whiteSprite;
            bgImg.color = new Color(1f, 1f, 1f, 0.18f);

            var thumbGo = new GameObject("Joystick_Thumb", typeof(RectTransform), typeof(Image));
            thumbGo.transform.SetParent(bgGo.transform, false);
            var thumbRT = (RectTransform)thumbGo.transform;
            thumbRT.anchorMin = thumbRT.anchorMax = new Vector2(0.5f, 0.5f);
            thumbRT.pivot = new Vector2(0.5f, 0.5f);
            thumbRT.anchoredPosition = Vector2.zero;
            thumbRT.sizeDelta = new Vector2(radius, radius);
            var thumbImg = thumbGo.GetComponent<Image>();
            thumbImg.sprite = whiteSprite;
            thumbImg.color = new Color(1f, 1f, 1f, 0.45f);

            var joystick = bgGo.AddComponent<VirtualJoystick>();
            // Set private serialized fields via SerializedObject (background, thumb, radiusPixels)
            var so = new SerializedObject(joystick);
            so.FindProperty("background").objectReferenceValue = bgRT;
            so.FindProperty("thumb").objectReferenceValue = thumbRT;
            so.FindProperty("radiusPixels").floatValue = radius;
            so.ApplyModifiedPropertiesWithoutUndo();

            var ctrl = player.GetComponent<PlayerController>();
            if (ctrl != null) ctrl.joystick = joystick;
        }

        // ---------- Skill buttons (bottom-right) ----------
        static void BuildSkillButtons(GameObject canvas, Sprite whiteSprite)
        {
            // Container
            var rowGo = new GameObject("SkillButtonRow", typeof(RectTransform));
            rowGo.transform.SetParent(canvas.transform, false);
            var rowRT = (RectTransform)rowGo.transform;
            rowRT.anchorMin = rowRT.anchorMax = new Vector2(1, 0);
            rowRT.pivot = new Vector2(1, 0);
            rowRT.anchoredPosition = new Vector2(-20, 20);
            rowRT.sizeDelta = new Vector2(680, 280);

            (string label, SkillButton.Action action, Color tint)[] entries =
            {
                ("Đánh",     SkillButton.Action.MeleeAttack,       new Color(0.85f, 0.30f, 0.25f)),
                ("Né",       SkillButton.Action.Dodge,             new Color(0.85f, 0.85f, 0.30f)),
                ("Skill",    SkillButton.Action.CastTechnique,     new Color(0.40f, 0.65f, 0.95f)),
                ("Pháp Bảo", SkillButton.Action.UseMagicTreasure,  new Color(0.85f, 0.55f, 0.95f)),
                ("Tương Tác",SkillButton.Action.Interact,          new Color(0.55f, 0.85f, 0.55f)),
                ("Thiền",    SkillButton.Action.ToggleMeditation,  new Color(0.40f, 0.85f, 0.85f)),
                ("Đột Phá",  SkillButton.Action.Breakthrough,      new Color(0.95f, 0.75f, 0.30f)),
                ("Ngủ",      SkillButton.Action.Sleep,             new Color(0.55f, 0.55f, 0.85f)),
                ("Đuốc",     SkillButton.Action.ToggleTorch,       new Color(0.95f, 0.55f, 0.30f)),
            };

            const int cols = 3;
            const float btnSize = 110f;
            const float gap = 12f;
            for (int i = 0; i < entries.Length; i++)
            {
                int col = i % cols;
                int row = i / cols;
                float x = -(cols - 1 - col) * (btnSize + gap);
                float y = row * (btnSize + gap);

                var btnGo = new GameObject("SkillBtn_" + entries[i].label,
                    typeof(RectTransform), typeof(Image), typeof(Button), typeof(SkillButton));
                btnGo.transform.SetParent(rowGo.transform, false);
                var rt = (RectTransform)btnGo.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = new Vector2(btnSize, btnSize);
                var img = btnGo.GetComponent<Image>();
                img.sprite = whiteSprite;
                img.color = entries[i].tint;
                var btn = btnGo.GetComponent<Button>();
                var sb = btnGo.GetComponent<SkillButton>();
                sb.action = entries[i].action;
                sb.button = btn;

                AddTMPLabel(btnGo, entries[i].label, 26, Color.black);
            }
        }

        // ---------- Inventory grid (right edge, 4x4) ----------
        static void BuildInventoryUI(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            var inv = player.GetComponent<Inventory>();
            var stats = player.GetComponent<PlayerStats>();

            var panelGo = new GameObject("InventoryPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(canvas.transform, false);
            var panelRT = (RectTransform)panelGo.transform;
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(1, 1);
            panelRT.pivot = new Vector2(1, 1);
            panelRT.anchoredPosition = new Vector2(-20, -20);
            panelRT.sizeDelta = new Vector2(360, 360);
            var panelImg = panelGo.GetComponent<Image>();
            panelImg.sprite = whiteSprite;
            panelImg.color = new Color(0, 0, 0, 0.45f);

            // Header
            AddTMPLabel(panelGo, "Túi đồ", 22, Color.white,
                anchor: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
                anchoredPos: new Vector2(0, -8), size: new Vector2(200, 24));

            // Grid container
            var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(panelGo.transform, false);
            var gridRT = (RectTransform)gridGo.transform;
            gridRT.anchorMin = new Vector2(0, 0);
            gridRT.anchorMax = new Vector2(1, 1);
            gridRT.offsetMin = new Vector2(8, 8);
            gridRT.offsetMax = new Vector2(-8, -36);
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(78, 78);
            grid.spacing = new Vector2(6, 6);
            grid.padding = new RectOffset(4, 4, 4, 4);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;

            // Slot prefab (transient, not saved as asset — InventoryUI Instantiate vẫn hoạt động vì
            // Unity sẽ clone subtree). Tạo template và cache reference vào InventoryUI.slotPrefab.
            var slotPrefab = SaveAsPrefab(BuildSlotPrefab(whiteSprite),
                $"{PrefabsDir}/InventorySlot.prefab");

            var ui = panelGo.AddComponent<InventoryUI>();
            ui.inventory = inv;
            ui.playerStats = stats;
            ui.slotPrefab = slotPrefab;
            ui.slotsParent = gridGo.transform;
        }

        static GameObject BuildSlotPrefab(Sprite whiteSprite)
        {
            var slot = new GameObject("InventorySlot",
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(InventorySlotUI));
            var rt = (RectTransform)slot.transform;
            rt.sizeDelta = new Vector2(78, 78);
            var bg = slot.GetComponent<Image>();
            bg.sprite = whiteSprite;
            bg.color = new Color(1f, 1f, 1f, 0.20f);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(slot.transform, false);
            var iconRT = (RectTransform)iconGo.transform;
            iconRT.anchorMin = new Vector2(0, 0);
            iconRT.anchorMax = new Vector2(1, 1);
            iconRT.offsetMin = new Vector2(6, 6);
            iconRT.offsetMax = new Vector2(-6, -6);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.preserveAspect = true;

            var countTmp = AddTMPLabel(slot, "", 18, Color.white,
                anchor: new Vector2(1, 0), pivot: new Vector2(1, 0),
                anchoredPos: new Vector2(-4, 4), size: new Vector2(40, 20),
                alignment: TextAlignmentOptions.BottomRight);

            var slotUI = slot.GetComponent<InventorySlotUI>();
            slotUI.iconImage = iconImg;
            slotUI.countText = countTmp;
            slotUI.button = slot.GetComponent<Button>();

            return slot;
        }

        // ---------- Crafting list (left side, below stat bars) ----------
        static void BuildCraftingUI(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            var craft = player.GetComponent<CraftingSystem>();

            var panelGo = new GameObject("CraftingPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(canvas.transform, false);
            var panelRT = (RectTransform)panelGo.transform;
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(0, 1);
            panelRT.pivot = new Vector2(0, 1);
            panelRT.anchoredPosition = new Vector2(10, -150);
            panelRT.sizeDelta = new Vector2(220, 320);
            var panelImg = panelGo.GetComponent<Image>();
            panelImg.sprite = whiteSprite;
            panelImg.color = new Color(0, 0, 0, 0.45f);

            AddTMPLabel(panelGo, "Chế Tạo", 22, Color.white,
                anchor: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
                anchoredPos: new Vector2(0, -8), size: new Vector2(180, 24));

            var listGo = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listGo.transform.SetParent(panelGo.transform, false);
            var listRT = (RectTransform)listGo.transform;
            listRT.anchorMin = new Vector2(0, 0);
            listRT.anchorMax = new Vector2(1, 1);
            listRT.offsetMin = new Vector2(6, 6);
            listRT.offsetMax = new Vector2(-6, -36);
            var vlg = listGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 4f;
            vlg.padding = new RectOffset(2, 2, 2, 2);
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            var recipePrefab = SaveAsPrefab(BuildRecipeButtonPrefab(whiteSprite),
                $"{PrefabsDir}/RecipeButton.prefab");

            var ui = panelGo.AddComponent<CraftingUI>();
            ui.craftingSystem = craft;
            ui.recipeButtonPrefab = recipePrefab;
            ui.listParent = listGo.transform;
        }

        static GameObject BuildRecipeButtonPrefab(Sprite whiteSprite)
        {
            var btnGo = new GameObject("RecipeButton",
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            var rt = (RectTransform)btnGo.transform;
            rt.sizeDelta = new Vector2(0, 36);
            var le = btnGo.GetComponent<LayoutElement>();
            le.minHeight = 36f;
            le.preferredHeight = 36f;
            var bg = btnGo.GetComponent<Image>();
            bg.sprite = whiteSprite;
            bg.color = new Color(0.95f, 0.85f, 0.55f, 0.85f);

            AddTMPLabel(btnGo, "Recipe", 18, Color.black,
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);

            return btnGo;
        }

        // ---------- Realm UI (top center) ----------
        static void BuildRealmUI(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            var realm = player.GetComponent<RealmSystem>();

            var panelGo = new GameObject("RealmPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(canvas.transform, false);
            var panelRT = (RectTransform)panelGo.transform;
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 1f);
            panelRT.pivot = new Vector2(0.5f, 1f);
            panelRT.anchoredPosition = new Vector2(0, -10);
            panelRT.sizeDelta = new Vector2(420, 80);
            var bg = panelGo.GetComponent<Image>();
            bg.sprite = whiteSprite;
            bg.color = new Color(0, 0, 0, 0.45f);

            var realmLabel = AddTMPLabel(panelGo, "Phàm Nhân", 22, Color.white,
                anchor: new Vector2(0, 1), pivot: new Vector2(0, 1),
                anchoredPos: new Vector2(10, -6), size: new Vector2(260, 28),
                alignment: TextAlignmentOptions.MidlineLeft);

            // XP bar
            var xpBgGo = new GameObject("Xp_BG", typeof(RectTransform), typeof(Image));
            xpBgGo.transform.SetParent(panelGo.transform, false);
            var xpBgRT = (RectTransform)xpBgGo.transform;
            xpBgRT.anchorMin = new Vector2(0, 0);
            xpBgRT.anchorMax = new Vector2(1, 0);
            xpBgRT.pivot = new Vector2(0.5f, 0);
            xpBgRT.anchoredPosition = new Vector2(0, 8);
            xpBgRT.sizeDelta = new Vector2(-20, 16);
            var xpBgImg = xpBgGo.GetComponent<Image>();
            xpBgImg.sprite = whiteSprite;
            xpBgImg.color = new Color(0, 0, 0, 0.6f);

            var xpFillGo = new GameObject("Xp_Fill", typeof(RectTransform), typeof(Image));
            xpFillGo.transform.SetParent(xpBgGo.transform, false);
            var xpFillRT = (RectTransform)xpFillGo.transform;
            xpFillRT.anchorMin = new Vector2(0, 0);
            xpFillRT.anchorMax = new Vector2(1, 1);
            xpFillRT.offsetMin = new Vector2(2, 2);
            xpFillRT.offsetMax = new Vector2(-2, -2);
            var xpFillImg = xpFillGo.GetComponent<Image>();
            xpFillImg.sprite = whiteSprite;
            xpFillImg.type = Image.Type.Filled;
            xpFillImg.fillMethod = Image.FillMethod.Horizontal;
            xpFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            xpFillImg.color = new Color(0.40f, 0.85f, 0.85f);

            // Breakthrough button
            var btnGo = new GameObject("BreakthroughBtn",
                typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panelGo.transform, false);
            var btnRT = (RectTransform)btnGo.transform;
            btnRT.anchorMin = btnRT.anchorMax = new Vector2(1, 1);
            btnRT.pivot = new Vector2(1, 1);
            btnRT.anchoredPosition = new Vector2(-10, -6);
            btnRT.sizeDelta = new Vector2(120, 32);
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.sprite = whiteSprite;
            btnImg.color = new Color(0.95f, 0.75f, 0.30f);
            AddTMPLabel(btnGo, "Đột Phá", 20, Color.black,
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);

            var resultLabel = AddTMPLabel(panelGo, "", 16, new Color(0.95f, 0.95f, 0.55f),
                anchor: new Vector2(0, 1), pivot: new Vector2(0, 1),
                anchoredPos: new Vector2(10, -34), size: new Vector2(280, 20),
                alignment: TextAlignmentOptions.MidlineLeft);

            var realmUI = panelGo.AddComponent<RealmUI>();
            realmUI.realm = realm;
            realmUI.realmLabel = realmLabel;
            realmUI.xpFill = xpFillImg;
            realmUI.breakthroughButton = btnGo.GetComponent<Button>();
            realmUI.breakthroughResultLabel = resultLabel;

            BuildAwakeningStatusHUD(canvas, player, whiteSprite);
        }

        // ---------- Awakening Status HUD (top center, dưới RealmPanel) ----------
        static void BuildAwakeningStatusHUD(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            var stats = player.GetComponent<PlayerStats>();
            var awaken = player.GetComponent<AwakeningSystem>();

            var panelGo = new GameObject("AwakeningStatusPanel",
                typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(canvas.transform, false);
            var panelRT = (RectTransform)panelGo.transform;
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 1f);
            panelRT.pivot = new Vector2(0.5f, 1f);
            // Dưới RealmPanel (anchoredY = -10, height = 80) → -100 = ngay dưới.
            panelRT.anchoredPosition = new Vector2(0, -100);
            panelRT.sizeDelta = new Vector2(420, 50);
            var bg = panelGo.GetComponent<Image>();
            bg.sprite = whiteSprite;
            bg.color = new Color(0, 0, 0, 0.4f);

            var label = AddTMPLabel(panelGo, "", 16, new Color(0.95f, 0.95f, 0.95f),
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);

            var hud = panelGo.AddComponent<AwakeningStatusHUD>();
            hud.playerStats = stats;
            hud.awakening = awaken;
            hud.statusText = label;
            hud.panelRoot = panelGo;
        }

        // ---------- Storage Chest UI (centered overlay, hidden by default) ----------
        static void BuildStorageChestUI(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            var inv = player.GetComponent<Inventory>();
            var ctrl = player.GetComponent<PlayerController>();

            // Root container — luôn active để StorageChestUI nhận event OnAnyChestOpened.
            // Bên trong: 1 OverlayContainer chứa cả Dim + ChestPanel; toggle container này
            // (không toggle riêng panel) để dim ẩn cùng panel — nếu không, dim sẽ luôn
            // che màn hình + chặn input ngay từ frame đầu.
            var rootGo = new GameObject("ChestUIRoot", typeof(RectTransform));
            rootGo.transform.SetParent(canvas.transform, false);
            var rootRT = (RectTransform)rootGo.transform;
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            var overlayGo = new GameObject("OverlayContainer", typeof(RectTransform));
            overlayGo.transform.SetParent(rootGo.transform, false);
            var overlayRT = (RectTransform)overlayGo.transform;
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;

            // Dim background (con của OverlayContainer — ẩn cùng panel khi đóng)
            var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dimGo.transform.SetParent(overlayGo.transform, false);
            var dimRT = (RectTransform)dimGo.transform;
            dimRT.anchorMin = Vector2.zero;
            dimRT.anchorMax = Vector2.one;
            dimRT.offsetMin = Vector2.zero;
            dimRT.offsetMax = Vector2.zero;
            var dimImg = dimGo.GetComponent<Image>();
            dimImg.sprite = whiteSprite;
            dimImg.color = new Color(0, 0, 0, 0.55f);
            dimImg.raycastTarget = true; // chặn click xuyên qua xuống các UI bên dưới

            // Centered panel — 2 cột (Chest left, Player right)
            var panelGo = new GameObject("ChestPanel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(overlayGo.transform, false);
            var panelRT = (RectTransform)panelGo.transform;
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(840, 480);
            var panelImg = panelGo.GetComponent<Image>();
            panelImg.sprite = whiteSprite;
            panelImg.color = new Color(0.10f, 0.10f, 0.12f, 0.92f);

            // Headers
            AddTMPLabel(panelGo, "Rương", 22, Color.white,
                anchor: new Vector2(0, 1), pivot: new Vector2(0, 1),
                anchoredPos: new Vector2(20, -10), size: new Vector2(200, 28),
                alignment: TextAlignmentOptions.MidlineLeft);
            AddTMPLabel(panelGo, "Túi đồ", 22, Color.white,
                anchor: new Vector2(1, 1), pivot: new Vector2(1, 1),
                anchoredPos: new Vector2(-20, -10), size: new Vector2(200, 28),
                alignment: TextAlignmentOptions.MidlineRight);

            // Close button (top right)
            var closeGo = new GameObject("CloseBtn",
                typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(panelGo.transform, false);
            var closeRT = (RectTransform)closeGo.transform;
            closeRT.anchorMin = closeRT.anchorMax = new Vector2(1, 1);
            closeRT.pivot = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-8, -8);
            closeRT.sizeDelta = new Vector2(36, 36);
            var closeImg = closeGo.GetComponent<Image>();
            closeImg.sprite = whiteSprite;
            closeImg.color = new Color(0.55f, 0.20f, 0.20f);
            AddTMPLabel(closeGo, "X", 22, Color.white,
                anchor: Vector2.zero, pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);

            // Chest grid (left column)
            var chestGridGo = new GameObject("ChestGrid",
                typeof(RectTransform), typeof(GridLayoutGroup));
            chestGridGo.transform.SetParent(panelGo.transform, false);
            var chestGridRT = (RectTransform)chestGridGo.transform;
            chestGridRT.anchorMin = new Vector2(0, 0);
            chestGridRT.anchorMax = new Vector2(0.5f, 1);
            chestGridRT.offsetMin = new Vector2(20, 20);
            chestGridRT.offsetMax = new Vector2(-10, -48);
            var chestGrid = chestGridGo.GetComponent<GridLayoutGroup>();
            chestGrid.cellSize = new Vector2(78, 78);
            chestGrid.spacing = new Vector2(6, 6);
            chestGrid.padding = new RectOffset(4, 4, 4, 4);
            chestGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            chestGrid.constraintCount = 4;

            // Player grid (right column)
            var playerGridGo = new GameObject("PlayerGrid",
                typeof(RectTransform), typeof(GridLayoutGroup));
            playerGridGo.transform.SetParent(panelGo.transform, false);
            var playerGridRT = (RectTransform)playerGridGo.transform;
            playerGridRT.anchorMin = new Vector2(0.5f, 0);
            playerGridRT.anchorMax = new Vector2(1, 1);
            playerGridRT.offsetMin = new Vector2(10, 20);
            playerGridRT.offsetMax = new Vector2(-20, -48);
            var playerGrid = playerGridGo.GetComponent<GridLayoutGroup>();
            playerGrid.cellSize = new Vector2(78, 78);
            playerGrid.spacing = new Vector2(6, 6);
            playerGrid.padding = new RectOffset(4, 4, 4, 4);
            playerGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            playerGrid.constraintCount = 4;

            // Reuse existing slot prefab nếu có (do BuildInventoryUI đã build trước đó);
            // fallback build mới nếu chưa có (BuildUI gọi BuildInventoryUI trước
            // BuildStorageChestUI, nên load AssetDatabase sẽ có).
            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"{PrefabsDir}/InventorySlot.prefab");
            if (slotPrefab == null)
            {
                slotPrefab = SaveAsPrefab(BuildSlotPrefab(whiteSprite),
                    $"{PrefabsDir}/InventorySlot.prefab");
            }

            var ui = rootGo.AddComponent<StorageChestUI>();
            ui.playerInventory = inv;
            ui.playerController = ctrl;
            ui.panel = overlayGo; // toggle cả dim + ChestPanel cùng nhau
            ui.chestSlotsParent = chestGridGo.transform;
            ui.playerSlotsParent = playerGridGo.transform;
            ui.slotPrefab = slotPrefab;
            ui.closeButton = closeGo.GetComponent<Button>();
        }

        // ---------- Tutorial HUD (welcome + checklist + victory banner) ----------
        static void BuildTutorialHUD(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            var tracker = player.GetComponent<DemoObjectivesTracker>();

            // Root GameObject giữ component TutorialHUD.
            var rootGo = new GameObject("TutorialHUD", typeof(RectTransform));
            rootGo.transform.SetParent(canvas.transform, false);
            var rootRT = (RectTransform)rootGo.transform;
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // --- Objectives checklist (góc phải trên, luôn hiện) ---
            var objPanel = new GameObject("ObjectivesPanel",
                typeof(RectTransform), typeof(Image));
            objPanel.transform.SetParent(rootGo.transform, false);
            var objRT = (RectTransform)objPanel.transform;
            objRT.anchorMin = objRT.anchorMax = new Vector2(1, 1);
            objRT.pivot = new Vector2(1, 1);
            objRT.anchoredPosition = new Vector2(-10, -10);
            objRT.sizeDelta = new Vector2(320, 170);
            var objImg = objPanel.GetComponent<Image>();
            objImg.sprite = whiteSprite;
            objImg.color = new Color(0, 0, 0, 0.45f);
            objImg.raycastTarget = false;

            var objList = AddTMPLabel(objPanel, "", 16, Color.white,
                anchor: new Vector2(0, 1), pivot: new Vector2(0, 1),
                anchoredPos: new Vector2(8, -6), size: new Vector2(304, 160),
                alignment: TextAlignmentOptions.TopLeft);

            // --- Welcome panel (center overlay, dismissable) ---
            var welcomeOverlay = new GameObject("WelcomeOverlay",
                typeof(RectTransform), typeof(Image));
            welcomeOverlay.transform.SetParent(rootGo.transform, false);
            var welRT = (RectTransform)welcomeOverlay.transform;
            welRT.anchorMin = Vector2.zero;
            welRT.anchorMax = Vector2.one;
            welRT.offsetMin = Vector2.zero;
            welRT.offsetMax = Vector2.zero;
            var welDim = welcomeOverlay.GetComponent<Image>();
            welDim.sprite = whiteSprite;
            welDim.color = new Color(0, 0, 0, 0.55f);
            welDim.raycastTarget = true;

            var welPanel = new GameObject("WelcomePanel",
                typeof(RectTransform), typeof(Image));
            welPanel.transform.SetParent(welcomeOverlay.transform, false);
            var wpRT = (RectTransform)welPanel.transform;
            wpRT.anchorMin = wpRT.anchorMax = new Vector2(0.5f, 0.5f);
            wpRT.pivot = new Vector2(0.5f, 0.5f);
            wpRT.anchoredPosition = Vector2.zero;
            wpRT.sizeDelta = new Vector2(640, 380);
            var wpImg = welPanel.GetComponent<Image>();
            wpImg.sprite = whiteSprite;
            wpImg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

            AddTMPLabel(welPanel, "Hoang Vực Tu Tiên Ký — Demo MVP", 26,
                new Color(1f, 0.92f, 0.6f),
                anchor: new Vector2(0.5f, 1), pivot: new Vector2(0.5f, 1),
                anchoredPos: new Vector2(0, -14), size: new Vector2(600, 32));

            var welBody = AddTMPLabel(welPanel, "", 18, Color.white,
                anchor: new Vector2(0.5f, 1), pivot: new Vector2(0.5f, 1),
                anchoredPos: new Vector2(0, -56), size: new Vector2(600, 260),
                alignment: TextAlignmentOptions.TopLeft);

            // 3 nút menu chính: Bắt đầu mới / Tiếp tục / Thoát. Layout ngang ở đáy panel.
            var welBtnGo = MakeWelcomeMenuBtn(welPanel, whiteSprite,
                "NewGameBtn", "Bắt đầu mới", new Color(0.95f, 0.75f, 0.30f), offsetX: -210);
            var continueBtnGo = MakeWelcomeMenuBtn(welPanel, whiteSprite,
                "ContinueBtn", "Tiếp tục", new Color(0.55f, 0.85f, 0.55f), offsetX: 0);
            var quitBtnGo = MakeWelcomeMenuBtn(welPanel, whiteSprite,
                "QuitBtn", "Thoát Demo", new Color(0.80f, 0.40f, 0.40f), offsetX: 210);

            // --- Victory banner (center overlay, hidden by default) ---
            var victoryOverlay = new GameObject("VictoryOverlay",
                typeof(RectTransform), typeof(Image));
            victoryOverlay.transform.SetParent(rootGo.transform, false);
            var vRT = (RectTransform)victoryOverlay.transform;
            vRT.anchorMin = Vector2.zero;
            vRT.anchorMax = Vector2.one;
            vRT.offsetMin = Vector2.zero;
            vRT.offsetMax = Vector2.zero;
            var vDim = victoryOverlay.GetComponent<Image>();
            vDim.sprite = whiteSprite;
            vDim.color = new Color(0, 0, 0, 0.55f);

            var vPanel = new GameObject("VictoryPanel",
                typeof(RectTransform), typeof(Image));
            vPanel.transform.SetParent(victoryOverlay.transform, false);
            var vpRT = (RectTransform)vPanel.transform;
            vpRT.anchorMin = vpRT.anchorMax = new Vector2(0.5f, 0.5f);
            vpRT.pivot = new Vector2(0.5f, 0.5f);
            vpRT.anchoredPosition = Vector2.zero;
            vpRT.sizeDelta = new Vector2(560, 280);
            var vpImg = vPanel.GetComponent<Image>();
            vpImg.sprite = whiteSprite;
            vpImg.color = new Color(0.15f, 0.22f, 0.15f, 0.95f);

            AddTMPLabel(vPanel, "MVP Demo hoàn thành", 30,
                new Color(0.80f, 1f, 0.80f),
                anchor: new Vector2(0.5f, 1), pivot: new Vector2(0.5f, 1),
                anchoredPos: new Vector2(0, -18), size: new Vector2(540, 36));

            var vText = AddTMPLabel(vPanel, "", 18, Color.white,
                anchor: new Vector2(0.5f, 1), pivot: new Vector2(0.5f, 1),
                anchoredPos: new Vector2(0, -72), size: new Vector2(520, 120),
                alignment: TextAlignmentOptions.Center);

            var vBtnGo = new GameObject("CloseVictoryBtn",
                typeof(RectTransform), typeof(Image), typeof(Button));
            vBtnGo.transform.SetParent(vPanel.transform, false);
            var vbRT = (RectTransform)vBtnGo.transform;
            vbRT.anchorMin = vbRT.anchorMax = new Vector2(0.5f, 0);
            vbRT.pivot = new Vector2(0.5f, 0);
            vbRT.anchoredPosition = new Vector2(0, 20);
            vbRT.sizeDelta = new Vector2(180, 40);
            var vbImg = vBtnGo.GetComponent<Image>();
            vbImg.sprite = whiteSprite;
            vbImg.color = new Color(0.85f, 0.85f, 0.85f);
            AddTMPLabel(vBtnGo, "Tiếp tục", 20, Color.black,
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);

            // --- Objective toast (top-center, ẩn mặc định) ---
            var toastPanel = new GameObject("ObjectiveToastPanel",
                typeof(RectTransform), typeof(Image));
            toastPanel.transform.SetParent(rootGo.transform, false);
            var tpRT = (RectTransform)toastPanel.transform;
            tpRT.anchorMin = tpRT.anchorMax = new Vector2(0.5f, 1);
            tpRT.pivot = new Vector2(0.5f, 1);
            tpRT.anchoredPosition = new Vector2(0, -20);
            tpRT.sizeDelta = new Vector2(460, 44);
            var tpImg = toastPanel.GetComponent<Image>();
            tpImg.sprite = whiteSprite;
            tpImg.color = new Color(0.10f, 0.25f, 0.10f, 0.92f);
            tpImg.raycastTarget = false;
            var toastText = AddTMPLabel(toastPanel, "", 18,
                new Color(0.85f, 1f, 0.85f),
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);
            toastPanel.SetActive(false);

            // Wire TutorialHUD component.
            var hud = rootGo.AddComponent<TutorialHUD>();
            hud.tracker = tracker;
            hud.welcomePanel = welcomeOverlay;
            hud.welcomeDismissButton = welBtnGo.GetComponent<Button>();
            hud.continueButton = continueBtnGo.GetComponent<Button>();
            hud.quitButton = quitBtnGo.GetComponent<Button>();
            hud.welcomeBodyText = welBody;
            hud.objectivesListText = objList;
            hud.victoryPanel = victoryOverlay;
            hud.victoryText = vText;
            hud.victoryDismissButton = vBtnGo.GetComponent<Button>();
            hud.objectiveToastPanel = toastPanel;
            hud.objectiveToastText = toastText;
        }

        // ---------- Pause menu (top-left pause button + center overlay) ----------
        static void BuildPauseMenu(GameObject canvas, Sprite whiteSprite)
        {
            // Pause button (top-left, luôn hiện trên mobile)
            var pauseBtnGo = new GameObject("PauseBtn",
                typeof(RectTransform), typeof(Image), typeof(Button));
            pauseBtnGo.transform.SetParent(canvas.transform, false);
            var pbRT = (RectTransform)pauseBtnGo.transform;
            pbRT.anchorMin = pbRT.anchorMax = new Vector2(0, 1);
            pbRT.pivot = new Vector2(0, 1);
            pbRT.anchoredPosition = new Vector2(10, -128);
            pbRT.sizeDelta = new Vector2(52, 36);
            var pbImg = pauseBtnGo.GetComponent<Image>();
            pbImg.sprite = whiteSprite;
            pbImg.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);
            AddTMPLabel(pauseBtnGo, "II", 20, Color.white,
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);

            // Overlay dim
            var overlayGo = new GameObject("PauseOverlay",
                typeof(RectTransform), typeof(Image));
            overlayGo.transform.SetParent(canvas.transform, false);
            var oRT = (RectTransform)overlayGo.transform;
            oRT.anchorMin = Vector2.zero;
            oRT.anchorMax = Vector2.one;
            oRT.offsetMin = Vector2.zero;
            oRT.offsetMax = Vector2.zero;
            var oImg = overlayGo.GetComponent<Image>();
            oImg.sprite = whiteSprite;
            oImg.color = new Color(0, 0, 0, 0.6f);

            // Menu panel
            var panelGo = new GameObject("PauseMenuPanel",
                typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(overlayGo.transform, false);
            var pRT = (RectTransform)panelGo.transform;
            pRT.anchorMin = pRT.anchorMax = new Vector2(0.5f, 0.5f);
            pRT.pivot = new Vector2(0.5f, 0.5f);
            pRT.anchoredPosition = Vector2.zero;
            pRT.sizeDelta = new Vector2(420, 400);
            var pImg = panelGo.GetComponent<Image>();
            pImg.sprite = whiteSprite;
            pImg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

            AddTMPLabel(panelGo, "Tạm Dừng", 28, new Color(1f, 0.92f, 0.6f),
                anchor: new Vector2(0.5f, 1), pivot: new Vector2(0.5f, 1),
                anchoredPos: new Vector2(0, -18), size: new Vector2(380, 36));

            Button resumeBtn = MakePauseMenuBtn(panelGo, whiteSprite,
                "Tiếp tục", new Color(0.95f, 0.75f, 0.30f), offsetY: -80);
            Button saveNowBtn = MakePauseMenuBtn(panelGo, whiteSprite,
                "Lưu ngay", new Color(0.55f, 0.85f, 0.55f), offsetY: -140);
            Button quitBtn = MakePauseMenuBtn(panelGo, whiteSprite,
                "Thoát Demo", new Color(0.80f, 0.40f, 0.40f), offsetY: -200);

            // Volume label + slider (bind tới AudioManager.SetMaster)
            var volLabel = AddTMPLabel(panelGo, "Âm lượng: 80%", 16, new Color(0.9f, 0.9f, 0.9f),
                anchor: new Vector2(0.5f, 1), pivot: new Vector2(0.5f, 1),
                anchoredPos: new Vector2(0, -260), size: new Vector2(380, 20));
            var volSlider = MakeVolumeSlider(panelGo, whiteSprite, offsetY: -290);

            // Toast label
            var toast = AddTMPLabel(panelGo, "", 18, new Color(0.85f, 1f, 0.85f),
                anchor: new Vector2(0.5f, 0), pivot: new Vector2(0.5f, 0),
                anchoredPos: new Vector2(0, 20), size: new Vector2(380, 22));
            toast.gameObject.SetActive(false);

            // Component — attach lên GameManager GameObject để tồn tại cùng scope Save/Load,
            // nhưng target overlay/button nằm trên UICanvas.
            var gmGo = GameObject.Find("GameManager");
            var pm = gmGo != null
                ? gmGo.AddComponent<PauseMenu>()
                : canvas.AddComponent<PauseMenu>();
            pm.overlay = overlayGo;
            pm.pauseButton = pauseBtnGo.GetComponent<Button>();
            pm.resumeButton = resumeBtn;
            pm.saveNowButton = saveNowBtn;
            pm.quitButton = quitBtn;
            pm.toastText = toast;
            pm.masterVolumeSlider = volSlider;
            pm.masterVolumeLabel = volLabel;
        }

        /// <summary>Slider âm lượng 0..1 — fill xanh, handle trắng, 320×16px.</summary>
        static Slider MakeVolumeSlider(GameObject parent, Sprite whiteSprite, float offsetY)
        {
            var root = new GameObject("VolumeSlider", typeof(RectTransform));
            root.transform.SetParent(parent.transform, false);
            var rootRT = (RectTransform)root.transform;
            rootRT.anchorMin = rootRT.anchorMax = new Vector2(0.5f, 1);
            rootRT.pivot = new Vector2(0.5f, 1);
            rootRT.anchoredPosition = new Vector2(0, offsetY);
            rootRT.sizeDelta = new Vector2(320, 16);

            // Background
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(root.transform, false);
            var bgRT = (RectTransform)bgGo.transform;
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGo.GetComponent<Image>();
            bgImg.sprite = whiteSprite;
            bgImg.color = new Color(0.25f, 0.25f, 0.28f, 1f);

            // Fill area + fill
            var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGo.transform.SetParent(root.transform, false);
            var faRT = (RectTransform)fillAreaGo.transform;
            faRT.anchorMin = new Vector2(0, 0.25f);
            faRT.anchorMax = new Vector2(1, 0.75f);
            faRT.offsetMin = new Vector2(6, 0);
            faRT.offsetMax = new Vector2(-10, 0);
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fRT = (RectTransform)fillGo.transform;
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.offsetMin = Vector2.zero;
            fRT.offsetMax = Vector2.zero;
            var fImg = fillGo.GetComponent<Image>();
            fImg.sprite = whiteSprite;
            fImg.color = new Color(0.55f, 0.85f, 0.55f, 1f);

            // Handle area + handle
            var handleAreaGo = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaGo.transform.SetParent(root.transform, false);
            var haRT = (RectTransform)handleAreaGo.transform;
            haRT.anchorMin = Vector2.zero;
            haRT.anchorMax = Vector2.one;
            haRT.offsetMin = new Vector2(10, 0);
            haRT.offsetMax = new Vector2(-10, 0);
            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleGo.transform.SetParent(handleAreaGo.transform, false);
            var hRT = (RectTransform)handleGo.transform;
            hRT.sizeDelta = new Vector2(20, 28);
            var hImg = handleGo.GetComponent<Image>();
            hImg.sprite = whiteSprite;
            hImg.color = Color.white;

            var slider = root.AddComponent<Slider>();
            slider.fillRect = fRT;
            slider.handleRect = hRT;
            slider.targetGraphic = hImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = 0.8f;
            return slider;
        }

        static Button MakePauseMenuBtn(GameObject parent, Sprite whiteSprite,
            string label, Color color, float offsetY)
        {
            var btnGo = new GameObject("PM_" + label,
                typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)btnGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, offsetY);
            rt.sizeDelta = new Vector2(260, 44);
            var img = btnGo.GetComponent<Image>();
            img.sprite = whiteSprite;
            img.color = color;
            AddTMPLabel(btnGo, label, 20, Color.black,
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);
            return btnGo.GetComponent<Button>();
        }

        /// <summary>3 nút main-menu-style nằm ngang ở đáy welcome panel (200×44, offsetX từ center).</summary>
        static GameObject MakeWelcomeMenuBtn(GameObject parent, Sprite whiteSprite,
            string goName, string label, Color color, float offsetX)
        {
            var btnGo = new GameObject(goName,
                typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)btnGo.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(offsetX, 20);
            rt.sizeDelta = new Vector2(200, 44);
            var img = btnGo.GetComponent<Image>();
            img.sprite = whiteSprite;
            img.color = color;
            AddTMPLabel(btnGo, label, 20, Color.black,
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);
            return btnGo;
        }

        // ---------- Helpers ----------
        static TMP_Text AddTMPLabel(GameObject parent, string text, float fontSize, Color color,
            Vector2 anchor = default, Vector2 pivot = default, Vector2 anchoredPos = default,
            Vector2 size = default, TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            bool stretch = false)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            if (stretch)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.anchorMin = rt.anchorMax = anchor == default ? new Vector2(0.5f, 0.5f) : anchor;
                rt.pivot = pivot == default ? new Vector2(0.5f, 0.5f) : pivot;
                rt.anchoredPosition = anchoredPos;
                rt.sizeDelta = size == default ? new Vector2(120, 30) : size;
            }
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
#endif
