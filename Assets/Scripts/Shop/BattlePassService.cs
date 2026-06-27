// =============================================================================
// NeonCity — Shop / BattlePassService.cs
// -----------------------------------------------------------------------------
// Seasonal Battle Pass with N tiers, two tracks (free/premium). XP earned in
// matches grants tier progress. Cosmetics only — NO P2W — to keep the game
// fair and Steam/Play-Store compliant.
//
// Tiers are defined in JSON (BattlePassConfig); rewards per tier live in
// ScriptableObject assets so designers can iterate without touching code.
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonCity.Shop
{
    [CreateAssetMenu(fileName = "BP_Reward_", menuName = "NeonCity/BattlePass Reward")]
    public class BPReward : ScriptableObject
    {
        public int tier;
        public bool isPremiumOnly;
        public string cosmeticId;
        public int creditAmount;
    }

    public class BattlePassService
    {
        public int CurrentTier { get; private set; }
        public int CurrentXP   { get; private set; }
        public bool HasPremium  { get; set; }

        public event Action<int> OnTierUp;

        public int XpPerTier = 1000;
        public int MaxTier   = 100;
        public List<BPReward> Rewards = new();

        // -------------------------------------------------------------------
        public void AddXP(int amount)
        {
            CurrentXP += amount;
            while (CurrentXP >= XpPerTier && CurrentTier < MaxTier)
            {
                CurrentXP -= XpPerTier;
                CurrentTier++;
                OnTierUp?.Invoke(CurrentTier);
                GrantRewards(CurrentTier);
            }
        }

        private void GrantRewards(int tier)
        {
            foreach (var r in Rewards)
            {
                if (r.tier != tier) continue;
                if (r.isPremiumOnly && !HasPremium) continue;
                Debug.Log($"[BattlePass] Unlocked reward: {r.name} (tier {tier})");
                // Hook into InventoryService.Add here.
            }
        }

        public object ToSave() => new { CurrentTier, CurrentXP, HasPremium };
    }
}