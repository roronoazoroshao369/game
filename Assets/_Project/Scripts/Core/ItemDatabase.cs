using System.Collections.Generic;
using UnityEngine;
using WildernessCultivation.Items;

namespace WildernessCultivation.Core
{
    /// <summary>
    /// ScriptableObject đăng ký toàn bộ ItemSO trong game để lookup bằng itemId (cho save/load).
    /// </summary>
    [CreateAssetMenu(menuName = "WildernessCultivation/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemSO> items = new();

        Dictionary<string, ItemSO> lookup;

        public ItemSO GetById(string id)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<string, ItemSO>();
                foreach (var i in items)
                    if (i != null && !string.IsNullOrEmpty(i.itemId))
                        lookup[i.itemId] = i;
            }
            return lookup.TryGetValue(id, out var v) ? v : null;
        }
    }
}
