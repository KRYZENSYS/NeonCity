// =============================================================================
// NeonCity — Inventory / Inventory.cs
// -----------------------------------------------------------------------------
// Grid-based inventory with weight + slot limits, modular weapon mods, and
// a hotbar. Items are ScriptableObjects so designers can create new gear
// without touching code.
//
// Uses addressables for icon loading (see ItemDefinition.iconRef).
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonCity.Inventory
{
    public enum ItemCategory { Weapon, Ammo, Consumable, Mod, Cosmetic, Material }

    [CreateAssetMenu(fileName = "Item_", menuName = "NeonCity/Item", order = 0)]
    public class ItemDefinition : ScriptableObject
    {
        public string   id;
        public string   displayName;
        public ItemCategory category;
        public Sprite   icon;
        public int      maxStack = 1;
        public float    weight   = 1f;
        public bool     isCraftable;
        public string   description;
    }

    [Serializable]
    public class ItemStack
    {
        public string   itemId;
        public int      count;
        public ItemDefinition Definition => ItemDatabase.Get(itemId);
    }

    [Serializable]
    public class InventorySave
    {
        public List<ItemStack> items = new();
        public int[] hotbar = new int[4];   // indices into items
    }

    public class Inventory : MonoBehaviour
    {
        public int   slotCount = 30;
        public float maxWeight = 100f;

        public List<ItemStack> Items { get; private set; } = new();
        public ItemStack[] Hotbar { get; private set; } = new ItemStack[4];

        public event Action OnChanged;

        public float CurrentWeight
        {
            get
            {
                float w = 0;
                foreach (var s in Items) w += (s.Definition?.weight ?? 0) * s.count;
                return w;
            }
        }

        // -------------------------------------------------------------------
        public bool CanAdd(ItemDefinition def, int count = 1)
        {
            if (def == null) return false;
            if (Items.Count >= slotCount && !HasStack(def.id)) return false;
            if (CurrentWeight + def.weight * count > maxWeight) return false;
            return true;
        }

        public bool Add(string itemId, int count = 1)
        {
            var def = ItemDatabase.Get(itemId);
            if (def == null || !CanAdd(def, count)) return false;

            // Try stack first
            if (def.maxStack > 1)
            {
                foreach (var s in Items)
                {
                    if (s.itemId == itemId && s.count < def.maxStack)
                    {
                        int add = Mathf.Min(count, def.maxStack - s.count);
                        s.count += add;
                        count -= add;
                        if (count == 0) { OnChanged?.Invoke(); return true; }
                    }
                }
            }
            // New slot
            while (count > 0)
            {
                int n = def.maxStack > 1 ? Mathf.Min(count, def.maxStack) : 1;
                Items.Add(new ItemStack { itemId = itemId, count = n });
                count -= n;
            }
            OnChanged?.Invoke();
            return true;
        }

        public bool Remove(int index, int count = 1)
        {
            if (index < 0 || index >= Items.Count) return false;
            var s = Items[index];
            s.count -= count;
            if (s.count <= 0) Items.RemoveAt(index);
            OnChanged?.Invoke();
            return true;
        }

        public bool HasItem(string itemId, int count = 1)
        {
            int total = 0;
            foreach (var s in Items) if (s.itemId == itemId) total += s.count;
            return total >= count;
        }

        public void AssignToHotbar(int slot, int itemIndex)
        {
            if (slot < 0 || slot >= Hotbar.Length) return;
            Hotbar[slot] = (itemIndex >= 0 && itemIndex < Items.Count) ? Items[itemIndex] : null;
            OnChanged?.Invoke();
        }

        // -------------------------------------------------------------------
        public InventorySave ToSave() => new InventorySave
        {
            items  = new List<ItemStack>(Items),
            hotbar = HotbarToIndices()
        };

        private int[] HotbarToIndices()
        {
            var arr = new int[Hotbar.Length];
            for (int i = 0; i < Hotbar.Length; i++)
            {
                int idx = -1;
                if (Hotbar[i] != null) idx = Items.IndexOf(Hotbar[i]);
                arr[i] = idx;
            }
            return arr;
        }

        public void FromSave(InventorySave save)
        {
            Items = new List<ItemStack>(save.items);
            for (int i = 0; i < Hotbar.Length; i++)
            {
                int idx = i < save.hotbar.Length ? save.hotbar[i] : -1;
                Hotbar[i] = (idx >= 0 && idx < Items.Count) ? Items[idx] : null;
            }
            OnChanged?.Invoke();
        }
    }

    /// <summary>Lightweight DB; in production this is loaded from Addressables or JSON.</summary>
    public static class ItemDatabase
    {
        private static readonly Dictionary<string, ItemDefinition> _db = new();
        public static void Register(ItemDefinition def) => _db[def.id] = def;
        public static ItemDefinition Get(string id) =>
            _db.TryGetValue(id, out var d) ? d : null;
    }
}