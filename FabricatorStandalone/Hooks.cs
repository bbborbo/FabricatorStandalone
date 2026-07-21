using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using R2API;
using R2API.Utils;
using UnityEngine;
using RoR2.ExpansionManagement;
using System.Runtime.CompilerServices;
using RoR2;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using static FabricatorStandalone.FabricatorPlugin;
using static R2API.DirectorAPI;

namespace FabricatorStandalone
{
    public static class Hooks
    {
        public static void Init()
        {
            On.RoR2.RouletteChestController.Cycling.OnEnter += DoubleChestOnInteract;
            On.RoR2.RouletteChestController.GetPickupForTime += DoubleChestScrap;
            On.RoR2.RouletteChestController.EjectPickupServer += DoubleChestDoubleLoot;
            DirectorAPI.InteractableActions += ScrapperOccurrenceHook;
        }
        private static void ScrapperOccurrenceHook(DccsPool pool, StageInfo stageInfo)
        {
            ChangeInteractableWeightForPool(pool, DirectorAPI.Helpers.InteractableNames.AdaptiveChest.ToLowerInvariant(), FabricatorPlugin.doubleChestWeight);
            void ChangeInteractableWeightForPool(DccsPool pool, string interactableNameLowered, int newWeight, int maxPerStage = -1)
            {
                //Debug.Log($"Changing {interactableNameLowered} card weight!");
                if (pool)
                {
                    Helpers.ForEachPoolEntryInDccsPool(pool, (poolEntry) =>
                    {
                        for (int i = 0; i < poolEntry.dccs.categories.Length; i++)
                        {
                            var cards = poolEntry.dccs.categories[i].cards.ToList();
                            foreach (DirectorCard card in cards)
                            {
                                SpawnCard spawnCard = card.spawnCard;
                                if (spawnCard.name.ToLowerInvariant() == interactableNameLowered)
                                {
                                    card.selectionWeight = newWeight;

                                    if (maxPerStage >= 0 && spawnCard is InteractableSpawnCard)
                                    {
                                        ((InteractableSpawnCard)spawnCard).maxSpawnsPerStage = maxPerStage;
                                    }
                                }
                            }
                            poolEntry.dccs.categories[i].cards = cards.ToArray();
                        }
                    });
                }
            }
        }

        public static void DoubleChestDoubleLoot(On.RoR2.RouletteChestController.orig_EjectPickupServer orig, RouletteChestController self, UniquePickup pickup)
        {
            if (pickup.Equals(UniquePickup.none))
            {
                return;
            }
            Vector3 forward = self.ejectionTransform.rotation * self.localEjectionVelocity;
            float maxYawSpread = 60;
            float yawPerProjectile = (maxYawSpread * 2) / (self.dropCount + 1);
            for (int i = 0; i < self.dropCount; i++)
            {
                float currentYaw = (self.dropCount == 1) ? 0 : (yawPerProjectile * (i + 1)) - maxYawSpread;
                Vector3 forward2 = (self.dropCount == 1) ? forward : Util.ApplySpread(forward, 0, 0, 1f, 1f, currentYaw, 0);

                PickupDropletController.CreatePickupDroplet(
                    pickup,
                    self.ejectionTransform.position,
                    forward2 + new Vector3(fabricatorEjectVelocity, 0, 0),
                    false,
                    false);
            }
        }

        public static UniquePickup DoubleChestScrap(On.RoR2.RouletteChestController.orig_GetPickupForTime orig, RouletteChestController self, Run.FixedTimeStamp time)
        {
            float threshold = 5;
            bool isFirstItem;

            isFirstItem = (threshold > (self.bonusTime));

            if (!isFirstItem)
            {
                return new UniquePickup(PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapWhite.itemIndex));
            }
            self.bonusTime += 0.01f;
            return orig(self, time);
        }

        public static void DoubleChestOnInteract(On.RoR2.RouletteChestController.Cycling.orig_OnEnter orig, RoR2.RouletteChestController.Cycling self)
        {
            RouletteChestController chestController = self.gameObject.GetComponent<RouletteChestController>();
            //chestController.dropTable = RoR2.MultiShopController.drop
            chestController.maxEntries = 2;
            chestController.bonusTime = 3;

            orig(self);

            if (chestController == null)
            {
                Debug.Log("auuuuuh fuck :3");
                return;
            }
            PurchaseInteraction purchaseInteraction = chestController.purchaseInteraction;
            if (purchaseInteraction == null)
            {
                Debug.Log("purchase interaction null 3:");
                return;
            }
            purchaseInteraction.costType = CostTypeIndex.Money;
            purchaseInteraction.cost = Run.instance.GetDifficultyScaledCost(fabricatorSecondCost, RoR2.Stage.instance.entryDifficultyCoefficient);
            purchaseInteraction.saleStarCompatible = false;
        }
    }
}
