using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WildernessCultivation.Crafting;
using WildernessCultivation.Items;

namespace WildernessCultivation.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests cho CraftingSystem: station detection (Physics2D OverlapCircleAll) +
    /// CanCraft / TryCraft / cookTime defer.
    /// </summary>
    public class CraftingSystemTests
    {
        GameObject playerGo;
        Inventory inv;
        CraftingSystem craft;
        GameObject stationGo;

        static ItemSO MakeItem(string id)
        {
            var so = ScriptableObject.CreateInstance<ItemSO>();
            so.itemId = id;
            so.displayName = id;
            so.maxStack = 99;
            return so;
        }

        static RecipeSO MakeRecipe(ItemSO output, int outputCount, CraftStation station,
            float cookSeconds, params (ItemSO item, int count)[] ingredients)
        {
            var r = ScriptableObject.CreateInstance<RecipeSO>();
            r.recipeId = output != null ? output.itemId + "_recipe" : "recipe";
            r.output = output;
            r.outputCount = outputCount;
            r.requiredStation = station;
            r.cookTimeSeconds = cookSeconds;
            r.ingredients = new RecipeIngredient[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
                r.ingredients[i] = new RecipeIngredient { item = ingredients[i].item, count = ingredients[i].count };
            return r;
        }

        GameObject MakeStation(CraftStation type, Vector3 pos)
        {
            var go = new GameObject($"Station-{type}");
            go.transform.position = pos;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;
            var marker = go.AddComponent<CraftStationMarker>();
            marker.station = type;
            return go;
        }

        [SetUp]
        public void Setup()
        {
            playerGo = new GameObject("Player");
            playerGo.transform.position = Vector3.zero;
            inv = playerGo.AddComponent<Inventory>();
            craft = playerGo.AddComponent<CraftingSystem>();
            craft.stationDetectRadius = 2f;
            craft.stationMask = ~0; // mọi layer
        }

        [TearDown]
        public void Teardown()
        {
            if (playerGo != null) Object.Destroy(playerGo);
            if (stationGo != null) Object.Destroy(stationGo);
        }

        // ===== CanCraft =====

        [UnityTest]
        public IEnumerator CanCraft_StationInRange_ReturnsTrue()
        {
            var stick = MakeItem("stick");
            var torch = MakeItem("torch");
            inv.Add(stick, 5);
            stationGo = MakeStation(CraftStation.Campfire, new Vector3(1f, 0f, 0f));
            yield return null; // sync transforms

            var recipe = MakeRecipe(torch, 1, CraftStation.Campfire, 0f, (stick, 2));
            Assert.IsTrue(craft.CanCraft(recipe));
        }

        [UnityTest]
        public IEnumerator CanCraft_StationOutOfRange_ReturnsFalse()
        {
            var stick = MakeItem("stick");
            var torch = MakeItem("torch");
            inv.Add(stick, 5);
            stationGo = MakeStation(CraftStation.Campfire, new Vector3(10f, 0f, 0f)); // ngoài radius 2
            yield return null;

            var recipe = MakeRecipe(torch, 1, CraftStation.Campfire, 0f, (stick, 2));
            Assert.IsFalse(craft.CanCraft(recipe));
        }

        [UnityTest]
        public IEnumerator CanCraft_WrongStationType_ReturnsFalse()
        {
            var stick = MakeItem("stick");
            var torch = MakeItem("torch");
            inv.Add(stick, 5);
            stationGo = MakeStation(CraftStation.Workbench, new Vector3(1f, 0f, 0f)); // sai loại
            yield return null;

            var recipe = MakeRecipe(torch, 1, CraftStation.Campfire, 0f, (stick, 2));
            Assert.IsFalse(craft.CanCraft(recipe));
        }

        [UnityTest]
        public IEnumerator CanCraft_StationGatedOff_ReturnsFalse()
        {
            var stick = MakeItem("stick");
            var torch = MakeItem("torch");
            inv.Add(stick, 5);
            stationGo = MakeStation(CraftStation.Campfire, new Vector3(1f, 0f, 0f));
            var gate = stationGo.AddComponent<TestStationGate>();
            gate.active = false; // station "tắt"
            yield return null;

            var recipe = MakeRecipe(torch, 1, CraftStation.Campfire, 0f, (stick, 2));
            Assert.IsFalse(craft.CanCraft(recipe), "IStationGate.StationActive=false → not in range");

            gate.active = true;
            Assert.IsTrue(craft.CanCraft(recipe), "bật lại → in range");
        }

        [Test]
        public void CanCraft_NoStationRequired_OnlyChecksIngredients()
        {
            var stick = MakeItem("stick");
            var rope = MakeItem("rope");
            inv.Add(stick, 3);

            var recipe = MakeRecipe(rope, 1, CraftStation.None, 0f, (stick, 2));
            Assert.IsTrue(craft.CanCraft(recipe), "no station required + đủ ingredients");

            inv.TryConsume(stick, 3); // hết
            Assert.IsFalse(craft.CanCraft(recipe), "thiếu ingredients");
        }

        [Test]
        public void CanCraft_NullRecipe_ReturnsFalse()
        {
            Assert.IsFalse(craft.CanCraft(null));
        }

        // ===== TryCraft =====

        [UnityTest]
        public IEnumerator TryCraft_NoCookTime_ConsumesIngredientsAndAddsOutputImmediately()
        {
            var stick = MakeItem("stick");
            var torch = MakeItem("torch");
            inv.Add(stick, 5);
            stationGo = MakeStation(CraftStation.Campfire, new Vector3(1f, 0f, 0f));
            yield return null;

            var recipe = MakeRecipe(torch, 1, CraftStation.Campfire, 0f, (stick, 2));
            Assert.IsTrue(craft.TryCraft(recipe));

            Assert.AreEqual(3, inv.CountOf(stick), "tốn 2 stick");
            Assert.AreEqual(1, inv.CountOf(torch), "ra 1 torch ngay");
        }

        [UnityTest]
        public IEnumerator TryCraft_WithCookTime_ConsumesImmediatelyButDefersOutput()
        {
            var stick = MakeItem("stick");
            var torch = MakeItem("torch");
            inv.Add(stick, 5);
            stationGo = MakeStation(CraftStation.Campfire, new Vector3(1f, 0f, 0f));
            yield return null;

            var recipe = MakeRecipe(torch, 1, CraftStation.Campfire, 0.3f, (stick, 2));
            Assert.IsTrue(craft.TryCraft(recipe));

            // Ngay sau TryCraft: ingredients đã tiêu, output CHƯA xuất hiện
            Assert.AreEqual(3, inv.CountOf(stick));
            Assert.AreEqual(0, inv.CountOf(torch));

            // Wait realtime 0.4s (cookTime dùng WaitForSecondsRealtime)
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.AreEqual(1, inv.CountOf(torch), "output xuất hiện sau cookTime");
        }

        [UnityTest]
        public IEnumerator TryCraft_CannotCraft_ReturnsFalseAndDoesNotMutate()
        {
            var stick = MakeItem("stick");
            var torch = MakeItem("torch");
            inv.Add(stick, 1); // không đủ
            stationGo = MakeStation(CraftStation.Campfire, new Vector3(1f, 0f, 0f));
            yield return null;

            var recipe = MakeRecipe(torch, 1, CraftStation.Campfire, 0f, (stick, 2));
            Assert.IsFalse(craft.TryCraft(recipe));
            Assert.AreEqual(1, inv.CountOf(stick));
            Assert.AreEqual(0, inv.CountOf(torch));
        }
    }

    /// <summary>
    /// Test-only fake gate cho IStationGate: active flag điều khiển từ test.
    /// </summary>
    public class TestStationGate : MonoBehaviour, IStationGate
    {
        public bool active = true;
        public bool StationActive => active;
    }
}
