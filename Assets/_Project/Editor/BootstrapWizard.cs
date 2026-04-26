#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WildernessCultivation.Audio;
using WildernessCultivation.Combat;
using WildernessCultivation.Core;
using WildernessCultivation.Crafting;
using WildernessCultivation.Cultivation;
using WildernessCultivation.Items;
using WildernessCultivation.Mobs;
using WildernessCultivation.Player;
using WildernessCultivation.Player.Status;
using WildernessCultivation.UI;
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
                SOsDir + "/Biomes", SOsDir + "/StatusEffects", SOsDir + "/Database"
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

        // ---------------- SPRITES (placeholder solid color squares) ----------------
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
                ("chest",     32, 28, new Color(0.55f, 0.35f, 0.15f)),
                ("workbench", 36, 28, new Color(0.40f, 0.25f, 0.10f)),
                ("campfire",  32, 32, new Color(0.95f, 0.55f, 0.10f)),
                ("water",     40, 40, new Color(0.30f, 0.65f, 0.90f)),
                ("ground",    32, 32, new Color(0.78f, 0.70f, 0.50f)),
                ("projectile",16, 16, new Color(0.95f, 0.30f, 0.10f)),
                ("icon_stick",   24, 24, new Color(0.55f, 0.35f, 0.18f)),
                ("icon_stone",   24, 24, new Color(0.55f, 0.55f, 0.55f)),
                ("icon_meat",    24, 24, new Color(0.85f, 0.30f, 0.30f)),
                ("icon_grilled", 24, 24, new Color(0.55f, 0.30f, 0.20f)),
                ("icon_water",   24, 24, new Color(0.30f, 0.70f, 0.95f)),
                ("icon_torch",   24, 24, new Color(0.95f, 0.65f, 0.20f)),
                ("icon_fish",    24, 24, new Color(0.55f, 0.75f, 0.95f)),
                ("icon_rod",     24, 24, new Color(0.70f, 0.50f, 0.20f)),
                ("ui_white",      4,  4, Color.white),
            };

            var dict = new Dictionary<string, Sprite>();
            foreach (var d in defs)
            {
                string path = $"{SpritesDir}/{d.id}.png";
                WritePng(path, d.w, d.h, d.color);
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
                selRange: new Vector2(0f, 0.5f)));
            list.Add(MakeBiome("desert", "Hoang Mạc Tử Khí",
                treeDensity: 0.02f, rockDensity: 0.10f, waterDensity: 0.001f,
                tempDay: 25f, tempNight: -15f,
                spiritEnergy: 0.8f, ambientNightSan: 1f,
                selRange: new Vector2(0.5f, 1f)));
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
                WaterSpring, StorageChest, Workbench, Projectile;
        }

        static PrefabBundle CreatePrefabs(Dictionary<string, Sprite> sprites,
            Dictionary<string, ItemSO> items, Dictionary<string, StatusEffectSO> statusEffects)
        {
            var bundle = new PrefabBundle();
            bundle.Player = BuildPlayerPrefab(sprites);
            bundle.Tree = BuildResourceNodePrefab("Tree", sprites["tree"], items["stick"], min: 2, max: 4, hp: 4f);
            bundle.Rock = BuildResourceNodePrefab("Rock", sprites["rock"], items["stone"], min: 1, max: 3, hp: 6f);
            bundle.Rabbit = BuildRabbitPrefab(sprites["rabbit"], items["raw_meat"]);
            bundle.Wolf = BuildWolfPrefab(sprites["wolf"], items["raw_meat"]);
            bundle.FoxSpirit = BuildFoxSpiritPrefab(sprites["fox_spirit"], items["raw_meat"]);
            bundle.Campfire = BuildCampfirePrefab(sprites["campfire"], items["stick"]);
            bundle.WaterSpring = BuildWaterSpringPrefab(sprites["water"], items["water"], items["raw_fish"]);
            bundle.StorageChest = BuildStorageChestPrefab(sprites["chest"]);
            bundle.Workbench = BuildWorkbenchPrefab(sprites["workbench"], items["stick"]);
            bundle.Projectile = BuildProjectilePrefab(sprites["projectile"], statusEffects["Burn"]);
            return bundle;
        }

        static GameObject BuildPlayerPrefab(Dictionary<string, Sprite> sprites)
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites["player"];
            sr.sortingOrder = 5;

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
            go.AddComponent<SleepAction>();
            go.AddComponent<TorchAction>();
            go.AddComponent<MagicTreasureAction>();
            go.AddComponent<MeditationAction>();
            go.AddComponent<DodgeAction>();
            go.AddComponent<FishingAction>();

            // Wire SpriteRenderer ref vào PlayerController
            var pc = go.GetComponent<PlayerController>();
            pc.spriteRenderer = sr;

            // LayerMask defaults to 0 (Nothing) — Physics2D.OverlapCircle sẽ không trúng gì.
            // Set sang Everything (~0) cho mặc định placeholder; user có thể thu hẹp sau.
            var combat = go.GetComponent<PlayerCombat>();
            combat.hitMask = ~0;
            var interact = go.GetComponent<InteractAction>();
            if (interact != null) interact.interactMask = ~0;

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
            return SaveAsPrefab(go, $"{PrefabsDir}/{name}.prefab");
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
            return SaveAsPrefab(go, $"{PrefabsDir}/Rabbit.prefab");
        }

        static GameObject BuildWolfPrefab(Sprite sprite, ItemSO meatDrop)
        {
            var go = new GameObject("Wolf");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;
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
            return SaveAsPrefab(go, $"{PrefabsDir}/Wolf.prefab");
        }

        static GameObject BuildFoxSpiritPrefab(Sprite sprite, ItemSO meatDrop)
        {
            var go = new GameObject("FoxSpirit");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;
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
            return SaveAsPrefab(go, $"{PrefabsDir}/FoxSpirit.prefab");
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
            wg.size = new Vector2Int(40, 40);
            wg.seed = 12345;
            wg.contentParent = worldGo.transform;
            wg.player = player.transform;
            // Legacy fallback prefabs (dùng khi không có biome).
            wg.treePrefab = prefabs.Tree;
            wg.rockPrefab = prefabs.Rock;
            wg.waterSpringPrefab = prefabs.WaterSpring;
            // Per-biome prefabs (BiomeSO chứa từng prefab riêng).
            foreach (var b in biomes)
            {
                b.treePrefab = prefabs.Tree;
                b.rockPrefab = prefabs.Rock;
                b.waterSpringPrefab = prefabs.WaterSpring;
                EditorUtility.SetDirty(b);
            }
            wg.biomes = biomes.ToArray();

            // MobSpawner: Rabbit (passive day/night), Wolf (aggressive cả ngày lẫn đêm),
            // FoxSpirit (đêm-only — dayCap=0).
            var spawner = worldGo.AddComponent<MobSpawner>();
            spawner.entries = new[]
            {
                new MobSpawner.SpawnEntry { prefab = prefabs.Rabbit,    dayCap = 6, nightCap = 3 },
                new MobSpawner.SpawnEntry { prefab = prefabs.Wolf,      dayCap = 2, nightCap = 3 },
                new MobSpawner.SpawnEntry { prefab = prefabs.FoxSpirit, dayCap = 0, nightCap = 4 },
            };
            spawner.parent = worldGo.transform;
            wg.mobSpawner = spawner;
            saveLoad.worldGenerator = wg;

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
            BuildStatBars(canvas, player, whiteSprite);
            BuildVirtualJoystick(canvas, player, whiteSprite);
            BuildSkillButtons(canvas, whiteSprite);
            BuildInventoryUI(canvas, player, whiteSprite);
            BuildCraftingUI(canvas, player, whiteSprite);
            BuildRealmUI(canvas, player, whiteSprite);
            BuildStorageChestUI(canvas, player, whiteSprite);
            BuildTutorialHUD(canvas, player, whiteSprite);
            BuildPauseMenu(canvas, whiteSprite);
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

            var welBtnGo = new GameObject("StartDemoBtn",
                typeof(RectTransform), typeof(Image), typeof(Button));
            welBtnGo.transform.SetParent(welPanel.transform, false);
            var wbRT = (RectTransform)welBtnGo.transform;
            wbRT.anchorMin = wbRT.anchorMax = new Vector2(0.5f, 0);
            wbRT.pivot = new Vector2(0.5f, 0);
            wbRT.anchoredPosition = new Vector2(0, 20);
            wbRT.sizeDelta = new Vector2(220, 44);
            var wbImg = welBtnGo.GetComponent<Image>();
            wbImg.sprite = whiteSprite;
            wbImg.color = new Color(0.95f, 0.75f, 0.30f);
            AddTMPLabel(welBtnGo, "Bắt đầu demo", 20, Color.black,
                anchor: new Vector2(0, 0), pivot: new Vector2(0.5f, 0.5f),
                anchoredPos: Vector2.zero, size: Vector2.zero,
                alignment: TextAlignmentOptions.Center, stretch: true);

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

            // Wire TutorialHUD component.
            var hud = rootGo.AddComponent<TutorialHUD>();
            hud.tracker = tracker;
            hud.welcomePanel = welcomeOverlay;
            hud.welcomeDismissButton = welBtnGo.GetComponent<Button>();
            hud.welcomeBodyText = welBody;
            hud.objectivesListText = objList;
            hud.victoryPanel = victoryOverlay;
            hud.victoryText = vText;
            hud.victoryDismissButton = vBtnGo.GetComponent<Button>();
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
            pRT.sizeDelta = new Vector2(420, 320);
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
