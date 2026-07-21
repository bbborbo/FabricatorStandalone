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
using static R2API.DirectorAPI;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
#pragma warning disable 

namespace FabricatorStandalone
{
    [BepInDependency(R2API.LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.DirectorAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]

    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(guid, modName, version)]
    public class FabricatorPlugin : BaseUnityPlugin
    {
        public const string guid = "com." + teamName + "." + modName;
        public const string teamName = "RiskOfBrainrot";
        public const string modName = "Fabricators";
        public const string version = "1.0.2";


        public static BasicPickupDropTable doubleChestDropTable;
        public static float fabricatorEjectVelocity = 1f;//2f
        public static int fabricatorCommonFirstCost = 30; //60; cost is incurred twice
        public static int fabricatorUncommonFirstCost = 60; //90; cost is incurred twice
        public static int fabricatorSecondCost = 30; //60; cost is incurred twice
        public static int doubleChestWeight = 16; //idk
        public static DirectorCard fabricatorCommonDirectorCard;
        public static InteractableSpawnCard fabricatorCommonSpawnCard;
        public static GameObject fabricatorCommonPrefab;
        /// <summary>
        /// Not yet implemented! This is here to make my life easier :)
        /// </summary>
        public static DirectorCard fabricatorUncommonDirectorCard => fabricatorCommonDirectorCard;
        /// <summary>
        /// Not yet implemented! This is here to make my life easier :)
        /// </summary>
        public static InteractableSpawnCard fabricatorUncommonSpawnCard => fabricatorCommonSpawnCard;
        /// <summary>
        /// Not yet implemented! This is here to make my life easier :)
        /// </summary>
        public static GameObject fabricatorUncommonPrefab => fabricatorCommonPrefab;
        public void Awake()
        {
            LanguageAPI.Add("CASINOCHEST_NAME", "Fabricator Chest");
            LanguageAPI.Add("CASINOCHEST_CONTEXT", "Use Fabricator Chest");
            LanguageAPI.Add("CASINOCHEST_DESCRIPTION",
                "Costs gold to activate and will show a single item. " +
                "Pay twice to get two copies of the shown item, or once to get two Scrap.");

            LoadAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_CasinoChest.CasinoChest_prefab, (casinoChest) =>
            {
                fabricatorCommonPrefab = casinoChest;
                if (casinoChest.TryGetComponent(out PurchaseInteraction purchaseInteraction))
                {
                    purchaseInteraction.cost = fabricatorCommonFirstCost;
                    purchaseInteraction.saleStarCompatible = false;
                }
                if (casinoChest.TryGetComponent(out RouletteChestController rouletteChestController))
                {
                    rouletteChestController.dropCount = 2;
                }
            });

            fabricatorCommonSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_CasinoChest.iscCasinoChest_asset).WaitForCompletion();
            fabricatorCommonDirectorCard = DirectorCards.BuildDirectorCard(fabricatorCommonSpawnCard, doubleChestWeight, 0);

            LoadAsync<BasicPickupDropTable>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_CasinoChest.dtCasinoChest_asset, (dropTable) =>
            {
                doubleChestDropTable = dropTable;
                doubleChestDropTable.tier1Weight = 1;
                doubleChestDropTable.tier2Weight = 0;
                doubleChestDropTable.tier3Weight = 0;
                doubleChestDropTable.equipmentWeight = 0;
            });

            AddDoubleChestToStage1(); 
            Hooks.Init();
        }

        public static AssetReferenceT<T> LoadAsync<T>(string guid, Action<T> callback) where T : UnityEngine.Object
        {
            void onCompleted(AsyncOperationHandle<T> handle)
            {
                if (!(handle.Result is T) || handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Failed to load asset [{handle.DebugName}] : {handle.OperationException}");
                    return;
                }

                callback(handle.Result);
            }

            AssetReferenceT<T> ref1 = new AssetReferenceT<T>(guid);
            AsyncOperationHandle<T> handle = AssetAsyncReferenceManager<T>.LoadAsset(ref1);

            if (callback == null)
            {
                return ref1;
            }

            if (handle.IsDone)
            {
                onCompleted(handle);
                return ref1;
            }

            handle.Completed += onCompleted;
            return ref1;
        }


        public static void AddDoubleChestToStage1()
        {
            DirectorAPI.Helpers.AddNewInteractableToStage(fabricatorCommonDirectorCard, DirectorAPI.InteractableCategory.Chests, DirectorAPI.Stage.TitanicPlains);
            DirectorAPI.Helpers.AddNewInteractableToStage(fabricatorCommonDirectorCard, DirectorAPI.InteractableCategory.Chests, DirectorAPI.Stage.DistantRoost);
            DirectorAPI.Helpers.AddNewInteractableToStage(fabricatorCommonDirectorCard, DirectorAPI.InteractableCategory.Chests, DirectorAPI.Stage.SiphonedForest);
        }
    }
}
