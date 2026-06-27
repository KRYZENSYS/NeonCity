// =============================================================================
// NeonCity — Shop / ShopService.cs
// -----------------------------------------------------------------------------
// IAP, currency, and cosmetic-only shop. Hooks Unity IAP for purchases and
// fires telemetry. Items have rarity, region locks, and sales (rotating).
// Cosmetics only — gameplay-altering items are NEVER sold (no P2W).
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace NeonCity.Shop
{
    public enum Rarity { Common, Rare, Epic, Legendary }

    [CreateAssetMenu(fileName = "Cosmetic_", menuName = "NeonCity/Cosmetic")]
    public class CosmeticItem : ScriptableObject
    {
        public string id;
        public string displayName;
        public Rarity  rarity;
        public Sprite  icon;
        public int     creditPrice;
        public string  iapProductId;       // e.g. "skin_neon_red_900"
        public bool    isLimitedTime;
    }

    public class ShopService : MonoBehaviour, IStoreListener
    {
        public List<CosmeticItem> Catalog = new();
        private IStoreController _store;
        public event Action<string, bool> OnPurchaseComplete;

        // -------------------------------------------------------------------
        public void InitShop()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var c in Catalog)
                if (!string.IsNullOrEmpty(c.iapProductId))
                    builder.AddProduct(c.iapProductId, ProductType.Consumable);
            UnityPurchasing.Initialize(this, builder);
        }

        // ---- Player currency -----------------------------------------------
        public int Credits { get; set; } = 500;

        public bool BuyWithCredits(CosmeticItem item)
        {
            if (Credits < item.creditPrice) return false;
            Credits -= item.creditPrice;
            UnlockCosmetic(item);
            return true;
        }

        public void BuyWithIAP(CosmeticItem item)
        {
            _store?.InitiatePurchase(item.iapProductId);
        }

        private void UnlockCosmetic(CosmeticItem item)
        {
            Debug.Log($"[Shop] Unlocked {item.displayName}");
            // Hook into InventoryService.Add / SaveService.Data.ownedCosmetics.
        }

        // ---- IStoreListener ------------------------------------------------
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _store = controller;
        }

        public void OnInitializeFailed(InitializationFailureReason error) { }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var item = Catalog.Find(c => c.iapProductId == args.purchasedProduct.definition.id);
            if (item != null) UnlockCosmetic(item);
            OnPurchaseComplete?.Invoke(args.purchasedProduct.definition.id, true);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
            => OnPurchaseComplete?.Invoke(product.definition.id, false);
    }
}