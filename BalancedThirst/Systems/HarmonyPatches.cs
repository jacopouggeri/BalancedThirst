using System;
using System.Reflection;
using BalancedThirst.HarmonyPatches.BlockLiquidContainer;
using BalancedThirst.HarmonyPatches.CharExtraDialogs;
using BalancedThirst.HarmonyPatches.CollObj;
using BalancedThirst.HarmonyPatches.Container;
using BalancedThirst.HarmonyPatches.CookedContainer;
using BalancedThirst.HarmonyPatches.Crock;
using BalancedThirst.HarmonyPatches.EntityFirepit;
using BalancedThirst.HarmonyPatches.InvSmelting;
using BalancedThirst.HarmonyPatches.Meal;
using BalancedThirst.HarmonyPatches.PerceptionEffects;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace BalancedThirst.Systems;

public class HarmonyPatches : ModSystem
{
    private ICoreAPI _api;
    private Harmony HarmonyInstance;

    public override double ExecuteOrder() => 0.03;
    public override void Start(ICoreAPI api)
    {
        this._api = api;
        HarmonyInstance = new Harmony(Mod.Info.ModID);
        var configData = ConfigSystem.SyncedConfigData;

        if (Math.Abs(configData.ContainerDrinkSpeed - 1) > 0.0001)
        {
            HarmonyInstance.Patch(
                typeof(BlockLiquidContainerBase).GetMethod("tryEatStop",
                    BindingFlags.NonPublic | BindingFlags.Instance),
                transpiler: typeof(BlockLiquidContainerBase_tryEatStop_Transpiler).GetMethod(
                    nameof(BlockLiquidContainerBase_tryEatStop_Transpiler.Transpiler)));
        }

        HarmonyInstance.Patch(typeof(BlockEntityFirepit).GetMethod("GetTemp", BindingFlags.NonPublic | BindingFlags.Instance),
            postfix: typeof(BlockEntityFirepit_Temp_Patch).GetMethod(
                nameof(BlockEntityFirepit_Temp_Patch.GetTemp)));
        HarmonyInstance.Patch(typeof(BlockEntityFirepit).GetMethod("SetTemp", BindingFlags.NonPublic | BindingFlags.Instance),
            postfix: typeof(BlockEntityFirepit_Temp_Patch).GetMethod(
                nameof(BlockEntityFirepit_Temp_Patch.SetTemp)));
        HarmonyInstance.Patch(typeof(InventorySmelting).GetMethod(nameof(InventorySmelting.GetOutputText)),
            postfix: typeof(InventorySmelting_GetOutputText_Patch).GetMethod(
                nameof(InventorySmelting_GetOutputText_Patch.Postfix)));
        
        if (!configData?.EnableThirst ?? false) return;
        HarmonyInstance.Patch(typeof(CollectibleObject).GetMethod("tryEatStop", BindingFlags.NonPublic | BindingFlags.Instance),
            prefix: typeof(CollectibleObject_tryEatStop_Patch).GetMethod(
                nameof(CollectibleObject_tryEatStop_Patch.Prefix)),
            postfix: typeof(CollectibleObject_tryEatStop_Patch).GetMethod(
                nameof(CollectibleObject_tryEatStop_Patch.Postfix)));
        
        HarmonyInstance.Patch(typeof(BlockMeal).GetMethod("tryFinishEatMeal", BindingFlags.NonPublic | BindingFlags.Instance),
            prefix: typeof(BlockMeal_tryFinishEatMeal_Patch).GetMethod(
                nameof(BlockMeal_tryFinishEatMeal_Patch.Prefix)),
            postfix: typeof(BlockMeal_tryFinishEatMeal_Patch).GetMethod(
                nameof(BlockMeal_tryFinishEatMeal_Patch.Postfix)));
        
        
        HarmonyInstance.Patch(typeof(BlockMeal).GetMethod(nameof(BlockMeal.GetHeldItemInfo)),
            postfix: typeof(BlockMeal_GetHeldItemInfo_Patch).GetMethod(
                nameof(BlockMeal_GetHeldItemInfo_Patch.Postfix)));
        HarmonyInstance.Patch(typeof(BlockCookedContainer).GetMethod(nameof(BlockCookedContainer.GetHeldItemInfo)),
            postfix: typeof(BlockCookedContainer_GetHeldItemInfo_Patch).GetMethod(
                nameof(BlockCookedContainer_GetHeldItemInfo_Patch.Postfix)));
        HarmonyInstance.Patch(typeof(BlockCrock).GetMethod(nameof(BlockCrock.GetHeldItemInfo)),
            postfix: typeof(BlockCrock_GetHeldItemInfo_Patch).GetMethod(
                nameof(BlockCrock_GetHeldItemInfo_Patch.Postfix)));
        HarmonyInstance.Patch(typeof(BlockPie).GetMethod(nameof(BlockPie.GetHeldItemInfo)),
            postfix: typeof(BlockCookedContainer_GetHeldItemInfo_Patch).GetMethod(
                nameof(BlockCookedContainer_GetHeldItemInfo_Patch.Postfix)));
        
        
        // Patch works atm but for some reason the generated description is not being displayed, will leave for other mod compatibility
        HarmonyInstance.Patch(typeof(BlockMeal).GetMethod(nameof(BlockMeal.GetContentNutritionFacts), new[] {
                typeof(IWorldAccessor),
                typeof(ItemSlot),
                typeof(ItemStack[]),
                typeof(EntityAgent),
                typeof(bool),
                typeof(float),
                typeof(float)
            }),
            postfix: typeof(BlockMeal_GetContentNutritionFacts_Patch).GetMethod(
                nameof(BlockMeal_GetContentNutritionFacts_Patch.Postfix)));
        
        HarmonyInstance.Patch(typeof(CollectibleObject).GetMethod(nameof(CollectibleObject.OnHeldInteractStop)),
            prefix: typeof(CollectibleObject_OnHeldInteractStop_Patch).GetMethod(
                nameof(CollectibleObject_OnHeldInteractStop_Patch.Prefix)),
            postfix: typeof(CollectibleObject_OnHeldInteractStop_Patch).GetMethod(
                nameof(CollectibleObject_OnHeldInteractStop_Patch.Postfix)));
        
        HarmonyInstance.Patch(typeof(BlockContainer).GetMethod(nameof(BlockContainer.GetContainingTransitionModifierContained)),
            postfix: typeof(BlockContainer_GetContainingTransitionModifier).GetMethod(
                nameof(BlockContainer_GetContainingTransitionModifier.Contained_Postfix)));
        HarmonyInstance.Patch(typeof(BlockContainer).GetMethod(nameof(BlockContainer.GetContainingTransitionModifierPlaced)),
            postfix: typeof(BlockContainer_GetContainingTransitionModifier).GetMethod(
                nameof(BlockContainer_GetContainingTransitionModifier.Placed_Postfix)));
        
        HarmonyInstance.Patch(typeof(CharacterExtraDialogs).GetMethod("Dlg_ComposeExtraGuis",  BindingFlags.NonPublic | BindingFlags.Instance),
            postfix: typeof(CharacterExtraDialogs_Dlg_ComposeExtraGuis_Patch).GetMethod(
                nameof(CharacterExtraDialogs_Dlg_ComposeExtraGuis_Patch.Postfix)));
        HarmonyInstance.Patch(typeof(CharacterExtraDialogs).GetMethod("UpdateStats",  BindingFlags.NonPublic | BindingFlags.Instance),
            postfix: typeof(CharacterExtraDialogs_UpdateStats_Patch).GetMethod(
                nameof(CharacterExtraDialogs_UpdateStats_Patch.Postfix)));
        HarmonyInstance.Patch(typeof(CharacterExtraDialogs).GetMethod("UpdateStats",  BindingFlags.NonPublic | BindingFlags.Instance),
            postfix: typeof(CharacterExtraDialogs_UpdateStatBars_Patch).GetMethod(
                nameof(CharacterExtraDialogs_UpdateStatBars_Patch.Postfix)));
        
        HarmonyInstance.Patch(typeof(PerceptionEffects).GetMethod(nameof(PerceptionEffects.OnOwnPlayerDataReceived)),
            prefix: typeof(PerceptionEffects_OnOwnPlayerDataReceived_Patch).GetMethod(
                nameof(PerceptionEffects_OnOwnPlayerDataReceived_Patch.Prefix)));
        HarmonyInstance.Patch(typeof(PerceptionEffects).GetConstructor(new[] { typeof(ICoreClientAPI) }),
            postfix: typeof(PerceptionEffects_Constructor_Patch).GetMethod(nameof(PerceptionEffects_Constructor_Patch.Postfix))
        );
    }

    public override void Dispose() {
        HarmonyInstance?.UnpatchAll(Mod.Info.ModID);
    }
    
}