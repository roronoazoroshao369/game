#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
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
            public GameObject Player, Tree, Rock, Rabbit, Campfire, WaterSpring, Projectile;
        }

        static PrefabBundle CreatePrefabs(Dictionary<string, Sprite> sprites,
            Dictionary<string, ItemSO> items, Dictionary<string, StatusEffectSO> statusEffects)
        {
            var bundle = new PrefabBundle();
            bundle.Player = BuildPlayerPrefab(sprites);
            bundle.Tree = BuildResourceNodePrefab("Tree", sprites["tree"], items["stick"], min: 2, max: 4, hp: 4f);
            bundle.Rock = BuildResourceNodePrefab("Rock", sprites["rock"], items["stone"], min: 1, max: 3, hp: 6f);
            bundle.Rabbit = BuildRabbitPrefab(sprites["rabbit"], items["raw_meat"]);
            bundle.Campfire = BuildCampfirePrefab(sprites["campfire"], items["stick"]);
            bundle.WaterSpring = BuildWaterSpringPrefab(sprites["water"], items["water"]);
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

        static GameObject BuildWaterSpringPrefab(Sprite sprite, ItemSO waterItem)
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
            return SaveAsPrefab(go, $"{PrefabsDir}/WaterSpring.prefab");
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

            // Wire SpiritRoot candidate pool (random roll on start)
            var sr = player.GetComponent<SpiritRoot>();
            sr.candidatePool = spiritRoots.ToArray();
            sr.rollOnStart = true;

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

            // MobSpawner
            var spawner = worldGo.AddComponent<MobSpawner>();
            spawner.entries = new[]
            {
                new MobSpawner.SpawnEntry { prefab = prefabs.Rabbit, dayCap = 6, nightCap = 3 },
            };
            spawner.parent = worldGo.transform;
            wg.mobSpawner = spawner;
            saveLoad.worldGenerator = wg;

            // Place 1 campfire near spawn. WorldGenerator.Start() teleports player to
            // (size.x*0.5, size.y*0.5, 0) = (20, 20, 0) với size 40x40, nên campfire phải
            // đặt gần đó chứ không phải gần origin.
            var fire = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Campfire);
            fire.transform.position = new Vector3(wg.size.x * 0.5f + 2.5f, wg.size.y * 0.5f, 0f);

            // UI Canvas
            var canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
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

        // Build minimal status bar UI
        static void BuildUI(GameObject canvas, GameObject player, Sprite whiteSprite)
        {
            var statBarUI = canvas.AddComponent<StatBarUI>();
            statBarUI.stats = player.GetComponent<PlayerStats>();
            statBarUI.controller = player.GetComponent<PlayerController>();
            statBarUI.inventory = player.GetComponent<Inventory>();

            Image MakeBar(string name, Vector2 anchoredPos, Color color)
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

            statBarUI.hpFill          = MakeBar("HP",       new Vector2(10, -10),  new Color(0.85f, 0.20f, 0.25f));
            statBarUI.hungerFill      = MakeBar("Hunger",   new Vector2(10, -28),  new Color(0.92f, 0.65f, 0.25f));
            statBarUI.thirstFill      = MakeBar("Thirst",   new Vector2(10, -46),  new Color(0.30f, 0.70f, 0.95f));
            statBarUI.sanityFill      = MakeBar("Sanity",   new Vector2(10, -64),  new Color(0.65f, 0.45f, 0.85f));
            statBarUI.manaFill        = MakeBar("Mana",     new Vector2(10, -82),  new Color(0.40f, 0.85f, 0.85f));
            statBarUI.bodyTempFill    = MakeBar("BodyTemp", new Vector2(10, -100), new Color(0.55f, 0.85f, 0.55f));
            statBarUI.encumbranceFill = MakeBar("Encumber", new Vector2(10, -118), new Color(0.85f, 0.85f, 0.40f));
        }
    }
}
#endif
