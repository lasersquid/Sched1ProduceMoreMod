using HarmonyLib;
using UnityEngine;

#if MONO_BUILD
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.UI.Shop;
using ScheduleOne.UI.Stations.Drying_rack;
using ScheduleOne.UI.Stations;
using ScheduleOne.UI;
using ScheduleOne;
using System.Reflection;
using TMPro;
#else
using Il2CppFishNet;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.StationFramework;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.UI.Phone.Delivery;
using Il2CppScheduleOne.UI.Shop;
using Il2CppScheduleOne.UI.Stations.Drying_rack;
using Il2CppScheduleOne.UI.Stations;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne;
using Il2CppTMPro;
#endif

namespace ProduceMore
{
    // Set stack sizes
    [HarmonyPatch]
    public class ItemCapacityPatches
    {
        public static ProduceMoreMod Mod = null;

        // Increase stack limit on item access
        [HarmonyPatch(typeof(ItemInstance), "StackLimit", MethodType.Getter)]
        [HarmonyPrefix]
        public static void StackLimitPostfix(ItemInstance __instance)
        {
            if (!Mod.processedDefs.Contains(__instance.Definition) && __instance.Definition.Name.ToLower() != "cash")
            {
                int stackLimit = Mod.settings.GetStackLimit(__instance);
                __instance.Definition.StackLimit = stackLimit;
                Mod.processedDefs.Add(__instance.Definition);
                
            }
        }

        // For phone delivery app
        [HarmonyPatch(typeof(ListingEntry), "Initialize")]
        [HarmonyPrefix]
        public static void InitializePrefix(ShopListing match)
        {
            if (match != null)
            {
                if (!Mod.processedDefs.Contains(match.Item))
                {
                    int stackLimit = Mod.settings.GetStackLimit(match.Item);
                    match.Item.StackLimit = stackLimit;
                    Mod.processedDefs.Add(match.Item);
                }
            }
        }
    }


    // Patch drying rack capacity and speed
    [HarmonyPatch]
    public static class DryingRackPatches
    {
        public static ProduceMoreMod Mod;


        // Modify DryingRack.ItemCapacity
        [HarmonyPatch(typeof(StartDryingRackBehaviour), "IsRackReady")]
        [HarmonyPrefix]
        public static void IsRackReadyPrefix(DryingRack rack, ref bool __result)
        {
            if (rack != null && rack.ItemCapacity != Mod.settings.GetStationCapacity("DryingRack"))
            {
                rack.ItemCapacity = Mod.settings.GetStationCapacity("DryingRack");
            }
        }


        // Modify DryingRack.ItemCapacity
        // canstartoperation runs every time a player or npc tries to interact
        // may have optimized away real access to ItemCapacity
        [HarmonyPatch(typeof(DryingRack), "CanStartOperation")]
        [HarmonyPrefix]
        public static bool CanStartOperationPrefix(DryingRack __instance, ref bool __result)
        {
            if (__instance != null && __instance.ItemCapacity != Mod.settings.GetStationCapacity("DryingRack"))
            {
                __instance.ItemCapacity = Mod.settings.GetStationCapacity("DryingRack");
            }

            __result = __instance.GetTotalDryingItems() < __instance.ItemCapacity && __instance.InputSlot.Quantity != 0 && !__instance.InputSlot.IsLocked && !__instance.InputSlot.IsRemovalLocked;

            return false;
        }


        // fix drying operation progress meter
        [HarmonyPatch(typeof(DryingOperationUI), "UpdatePosition")]
        [HarmonyPrefix]
        public static bool UpdatePositionPrefix(DryingOperationUI __instance)
        {
            float stationSpeed = 720f / Mod.settings.GetStationSpeed("DryingRack");
            float t = Mathf.Clamp01((float)__instance.AssignedOperation.Time / stationSpeed);
            int num = Mathf.Clamp((int)stationSpeed - __instance.AssignedOperation.Time, 0, (int)stationSpeed);
            int num2 = num / 60;
            int num3 = num % 60;
            __instance.Tooltip.text = num2.ToString() + "h " + num3.ToString() + "m until next tier";
            float num4 = -62.5f;
            float b = -num4;
            __instance.Rect.anchoredPosition = new Vector2(Mathf.Lerp(num4, b, t), 0f);

            return false;
        }

        // speed
        [HarmonyPatch(typeof(DryingOperation), "GetQuality")]
        [HarmonyPostfix]
        public static void GetQualityPostfix(DryingOperation __instance, ref EQuality __result)
        {
            int dryingTime = (int)(720f / Mod.settings.GetStationSpeed("DryingRack"));

            if (__instance.Time >= dryingTime)
            {
                __result = __instance.StartQuality + 1;
            }
            __result = __instance.StartQuality;
        }


        // modified copy of DryingRack.MinPass
        [HarmonyPatch(typeof(DryingRack), "MinPass")]
        [HarmonyPrefix]
        public static bool MinPassPrefix(DryingRack __instance)
        {
            if (__instance == null)
            {
                return false;
            }
            foreach (DryingOperation dryingOperation in __instance.DryingOperations.ToArray())
            {
                dryingOperation.Time++;
                if (dryingOperation.Time >= (720f / Mod.settings.GetStationSpeed("DryingRack")))
                {
                    if (dryingOperation.StartQuality >= EQuality.Premium)
                    {
                        if (InstanceFinder.IsServer && __instance.GetOutputCapacityForOperation(dryingOperation, EQuality.Heavenly) >= dryingOperation.Quantity)
                        {
                            __instance.TryEndOperation(__instance.DryingOperations.IndexOf(dryingOperation), false, EQuality.Heavenly, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
                        }
                    }
                    else
                    {
                        dryingOperation.IncreaseQuality();
                    }
                }
            }
            return false;
        }

    }


    // Patch lab oven capacity and speed
    [HarmonyPatch]
    public static class LabOvenPatches
    {
        public static ProduceMoreMod Mod;


        // speed
        [HarmonyPatch(typeof(OvenCookOperation), "GetCookDuration")]
        [HarmonyPostfix]
        public static void GetCookDurationPostfix(ref int __result)
        {
            __result = (int)((float)__result / Mod.settings.GetStationSpeed("LabOven"));
        }

#if MONO_BUILD

        // might need to patch IsReadyForHarvest
        //[HarmonyPatch(typeof(OvenCookOperation), "IsReadyForHarvest")]
        //[HarmonyPostfix]
        public static void IsReadyForHarvestPostfix(OvenCookOperation __instance)
        {

        }
#else

        // call to GetCookDuration seems to have been optimized out.
        [HarmonyPatch(typeof(OvenCookOperation), "IsReady")]
        [HarmonyPostfix]
        public static void IsReadyPostfix(OvenCookOperation __instance, ref bool __result)
        {
            // Re-insert original method body.
            __result = __instance.CookProgress >= __instance.GetCookDuration();
        }
#endif

    }


    // Patch mixing station capacity and speed
    [HarmonyPatch]
    public static class MixingStationPatches
    {
        public static ProduceMoreMod Mod;

        // capacity
        [HarmonyPatch(typeof(MixingStation), "GetMixQuantity")]
        [HarmonyPrefix]
        public static bool GetMixQuantityPrefix(MixingStation __instance, ref int __result)
        {
            if (__instance.MaxMixQuantity != Mod.settings.GetStationCapacity("MixingStation"))
            {
                __instance.MaxMixQuantity = Mod.settings.GetStationCapacity("MixingStation");
            }

            if (__instance.GetProduct() == null || __instance.GetMixer() == null)
            {
                __result = 0;
                return false;
            }
            __result = Mathf.Min(Mathf.Min(__instance.ProductSlot.Quantity, __instance.MixerSlot.Quantity), __instance.MaxMixQuantity);
            return false;
        }

        // actual call to GetMixQuantity seems to have been optimized out.
        [HarmonyPatch(typeof(MixingStation), "CanStartMix")]
        [HarmonyPostfix]
        public static void CanStartMixPostfix(MixingStation __instance, ref bool __result)
        {
            // re-insert original method body.
            __result = __instance.GetMixQuantity() > 0 && __instance.OutputSlot.Quantity == 0;
        }

        // speed
        [HarmonyPatch(typeof(MixingStation), "GetMixTimeForCurrentOperation")]
        [HarmonyPrefix]
        public static bool GetMixTimePrefix(MixingStation __instance, ref int __result)
        {
            float mixTimePerItem = (int)(15f / Mathf.Max(Mod.settings.GetStationSpeed("MixingStation"), 0.01f));
            if (__instance.MixTimePerItem != mixTimePerItem)
            {
                __instance.MixTimePerItem = (int)mixTimePerItem;
            }

            if (__instance.CurrentMixOperation == null)
            {
                __result = 0;
                return false;
            }
            __result = (int)(mixTimePerItem * (float)__instance.CurrentMixOperation.Quantity);

            return false;
        }

        [HarmonyPatch(typeof(StartMixingStationBehaviour), "StartCook")]
        [HarmonyPrefix]
        public static void StartCookPrefix(StartMixingStationBehaviour __instance)
        {
            if (__instance.targetStation.MaxMixQuantity != Mod.settings.GetStationCapacity("MixingStation"))
            {
                __instance.targetStation.MaxMixQuantity = Mod.settings.GetStationCapacity("MixingStation");
            }
        }
    }


    // Brick press patches
    // currently empty
    [HarmonyPatch]
    public static class BrickPressPatches
    {
        public static ProduceMoreMod Mod;
        // currently empty
    }


    // cauldron patches
    [HarmonyPatch]
    public static class CauldronPatches
    {
        public static ProduceMoreMod Mod;
        // patch visuals for capacity
        [HarmonyPatch(typeof(Cauldron), "UpdateIngredientVisuals")]
        [HarmonyPrefix]
        public static bool UpdateIngredientVisualsPatch(Cauldron __instance)
        {
            int cauldronCapacity = Mod.settings.GetStackLimit("Coca Leaf", EItemCategory.Growing);
            ItemInstance itemInstance;
            int num;
            ItemInstance itemInstance2;
            int num2;
            __instance.GetMainInputs(out itemInstance, out num, out itemInstance2, out num2);
            if (itemInstance != null)
            {
                __instance.PrimaryTub.Configure(CauldronDisplayTub.EContents.CocaLeaf, (float)num / (float)cauldronCapacity);
            }
            else
            {
                __instance.PrimaryTub.Configure(CauldronDisplayTub.EContents.None, 0f);
            }
            if (itemInstance2 != null)
            {
                __instance.SecondaryTub.Configure(CauldronDisplayTub.EContents.CocaLeaf, (float)num2 / (float)cauldronCapacity);
                return false;
            }
            __instance.SecondaryTub.Configure(CauldronDisplayTub.EContents.None, 0f);

            return false;
        }


        // speed
        [HarmonyPatch(typeof(StartCauldronBehaviour), "BeginCauldron")]
        [HarmonyPostfix]
        public static void BeginCauldronPostfix(StartCauldronBehaviour __instance)
        {
            int newCookTime = (int)(360f / Mod.settings.GetStationSpeed("Cauldron"));
            if (__instance.Station.RemainingCookTime > newCookTime)
            {
                __instance.Station.RemainingCookTime = newCookTime;
            }
        }


        [HarmonyPatch(typeof(Cauldron), "StartCookOperation")]
        [HarmonyPostfix]
        public static void StartCookOperationPostfix(Cauldron __instance)
        {
            int newCookTime = (int)(360f / Mod.settings.GetStationSpeed("Cauldron"));
            if (__instance.RemainingCookTime > newCookTime)
            {
                __instance.RemainingCookTime = newCookTime;
            }
        }

#if !MONO_BUILD

        [HarmonyPatch(typeof(StartCauldronBehaviour), "BeginCauldron")]
        [HarmonyPrefix]
        public static void BeginCauldronPrefix(StartCauldronBehaviour __instance)
        {
            int newCookTime = (int)(360f / Mod.settings.GetStationSpeed("Cauldron"));
            if (__instance.Station.CookTime != newCookTime)
            {
                __instance.Station.CookTime = newCookTime;
            }
        }


        [HarmonyPatch(typeof(Cauldron), "StartCookOperation")]
        [HarmonyPrefix]
        public static void StartCookOperationPrefix(Cauldron __instance)
        {
            int newCookTime = (int)(360f / Mod.settings.GetStationSpeed("Cauldron"));
            if (__instance.CookTime != newCookTime)
            {
                __instance.CookTime = newCookTime;
            }
        }
#endif



        // capacity takes care of itself
    }


    // packaging station patches
    [HarmonyPatch]
    public static class PackagingStationPatches
    {
        public static ProduceMoreMod Mod;


        // speed
        [HarmonyPatch(typeof(PackagingStationBehaviour), "BeginPackaging")]
        [HarmonyPrefix]
        public static void BeginPackagingPrefix(PackagingStationBehaviour __instance)
        {
            float stationSpeed = Mod.settings.GetStationSpeed("PackagingStation");
            if (__instance.Station.PackagerEmployeeSpeedMultiplier != stationSpeed)
            {
                __instance.Station.PackagerEmployeeSpeedMultiplier = stationSpeed;
            }
        }
        // capacity takes care of itself
    }


    // pot patches
    [HarmonyPatch]
    public static class PotPatches
    {
        public static ProduceMoreMod Mod;


        // speed
        [HarmonyPatch(typeof(Pot), "GetAdditiveGrowthMultiplier")]
        [HarmonyPostfix]
        public static void GetAdditiveGrowthMultiplierPostfix(ref float __result)
        {
            __result = __result * Mod.settings.GetStationSpeed("Pot");
        }
    }


    // chemistry station patches
    [HarmonyPatch]
    public static class ChemistryStationPatches
    {
        public static ProduceMoreMod Mod;


        [HarmonyPatch(typeof(StationRecipeEntry), "AssignRecipe")]
        [HarmonyPostfix]
        public static void AssignRecipePostfix(StationRecipeEntry __instance, ref StationRecipe recipe)
        {
            if (!Mod.processedRecipes.Contains(recipe))
            {
                __instance.Recipe.CookTime_Mins = (int)((float)__instance.Recipe.CookTime_Mins / Mod.settings.GetStationSpeed("ChemistryStation"));
                Mod.processedRecipes.Add(__instance.Recipe);
            }
            int hours = __instance.Recipe.CookTime_Mins / 60;
            int minutes = __instance.Recipe.CookTime_Mins % 60;
            __instance.CookingTimeLabel.text = $"{hours}h";
            if (minutes > 0)
            {
                __instance.CookingTimeLabel.text += $" {minutes}m";
            }
        }
    }


    // cash patches
    // TODO: clean up this class
    [HarmonyPatch]
    public static class CashPatches
    {
        public static ProduceMoreMod Mod;


#if MONO_BUILD
        // using transpiler patches would have been less repeating myself than this.
        // This method has hardcoded constants, so we need to replace it entirely
        // (or use a transpiler patch but that's not cross-platform friendly)
        [HarmonyPatch(typeof(ItemUIManager), "UpdateCashDragAmount")]
        [HarmonyPrefix]
        public static bool UpdateCashDragAmountPrefix(
            ItemUIManager __instance, 
            CashInstance instance, 
            ref float ___draggedCashAmount
            )
        {
            Mod.LoggerInstance.Msg($"In {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            int stackLimit = Mod.settings.GetStackLimit(EItemCategory.Cash);

            float[] array = new float[] { 50f, 10f, 1f };
            float[] array2 = new float[] { 100f, 10f, 1f };
            float num = 0f;
            if (GameInput.MouseScrollDelta > 0f)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (___draggedCashAmount >= array2[i])
                    {
                        num = array[i];
                        break;
                    }
                }
            }
            else if (GameInput.MouseScrollDelta < 0f)
            {
                for (int j = 0; j < array.Length; j++)
                {
                    if (___draggedCashAmount > array2[j])
                    {
                        num = -array[j];
                        break;
                    }
                }
            }
            if (num == 0f)
            {
                return false;
            }
            ___draggedCashAmount = Mathf.Clamp(___draggedCashAmount + num, 1f, Mathf.Min(instance.Balance, stackLimit));

            return false;
        }


        // This method has hardcoded constants, so we need to replace it entirely
        [HarmonyPatch(typeof(ItemUIManager), "StartDragCash")]
        [HarmonyPrefix]
        public static bool StartDragCashPrefix(
            ItemUIManager __instance, 
            ref float ___draggedCashAmount, 
            ref int ___draggedAmount, 
            ref ItemSlotUI ___draggedSlot, 
            ref Vector2 ___mouseOffset, 
            ref bool ___customDragAmount,
            ref RectTransform ___tempIcon
            )
        {
            int stackLimit = Mod.settings.GetStackLimit(EItemCategory.Cash);

            CashInstance cashInstance = (___draggedSlot.assignedSlot.ItemInstance as CashInstance);
            ___draggedCashAmount = Mathf.Min(cashInstance.Balance, stackLimit);
            ___draggedAmount = 1;
            if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
            {
                ___draggedAmount = 1;
                ___draggedCashAmount = Mathf.Min(cashInstance.Balance, 100f);
                ___mouseOffset += new Vector2(-10f, -15f);
                ___customDragAmount = true;
            }
            if (___draggedCashAmount <= 0f)
            {
                ___draggedSlot = null;
                return false;
            }
            if (GameInput.GetButton(GameInput.ButtonCode.QuickMove) && __instance.QuickMoveEnabled)
            {
                //List<ItemSlot> quickMoveSlots = __instance.GetQuickMoveSlots(___draggedSlot.assignedSlot);
                MethodInfo GetQuickMoveSlotsInfo = AccessTools.DeclaredMethod(typeof(ItemUIManager), "GetQuickMoveSlots");
                List<ItemSlot> quickMoveSlots = (List<ItemSlot>)GetQuickMoveSlotsInfo.Invoke(__instance, [___draggedSlot.assignedSlot]);
                if (quickMoveSlots.Count > 0)
                {
                    Debug.Log("Quick-moving " + ___draggedAmount.ToString() + " items...");
                    float a = ___draggedCashAmount;
                    float num = 0f;
                    int num2 = 0;
                    while (num2 < quickMoveSlots.Count && num < (float)___draggedAmount)
                    {
                        ItemSlot itemSlot = quickMoveSlots[num2];
                        if (itemSlot.ItemInstance != null)
                        {
                            CashInstance cashInstance2 = (itemSlot.ItemInstance as CashInstance);
                            if (cashInstance2 != null)
                            {
                                float num3;
                                if (itemSlot is CashSlot)
                                {
                                    num3 = Mathf.Min(a, float.MaxValue - cashInstance2.Balance);
                                }
                                else
                                {
                                    num3 = Mathf.Min(a, stackLimit - cashInstance2.Balance);
                                }
                                cashInstance2.ChangeBalance(num3);
                                itemSlot.ReplicateStoredInstance();
                                num += num3;
                            }
                        }
                        else
                        {
                            CashInstance cashInstance3 = (Registry.GetItem("cash").GetDefaultInstance(1) as CashInstance);
                            cashInstance3.SetBalance(___draggedCashAmount, false);
                            itemSlot.SetStoredItem(cashInstance3, false);
                            num += ___draggedCashAmount;
                        }
                        num2++;
                    }
                    if (num >= cashInstance.Balance)
                    {
                        ___draggedSlot.assignedSlot.ClearStoredInstance(false);
                    }
                    else
                    {
                        cashInstance.ChangeBalance(-num);
                        ___draggedSlot.assignedSlot.ReplicateStoredInstance();
                    }
                }
                if (__instance.onItemMoved != null)
                {
                    __instance.onItemMoved.Invoke();
                }
                ___draggedSlot = null;
                return false;
            }
            if (__instance.onDragStart != null)
            {
                __instance.onDragStart.Invoke();
            }
            if (___draggedSlot.assignedSlot != PlayerSingleton<PlayerInventory>.Instance.cashSlot)
            {
                __instance.CashSlotHintAnim.Play();
            }
            ___tempIcon = ___draggedSlot.DuplicateIcon(Singleton<HUD>.Instance.transform, ___draggedAmount);
            ___tempIcon.Find("Balance").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(___draggedCashAmount, false, false);
            ___draggedSlot.IsBeingDragged = true;
            if (___draggedCashAmount >= cashInstance.Balance)
            {
                ___draggedSlot.SetVisible(false);
                return false;
            }
            (___draggedSlot.ItemUI as ItemUI_Cash).SetDisplayedBalance(cashInstance.Balance - ___draggedCashAmount);
            return false;
        }


        // This method has hardcoded constants, so we need to replace it completely
        [HarmonyPatch(typeof(ItemUIManager), "EndCashDrag")]
        [HarmonyPrefix]
        public unsafe static bool EndCashDragPrefix(
            ItemUIManager __instance,
            ref float ___draggedCashAmount,
            ref int ___draggedAmount,
            ref ItemSlotUI ___draggedSlot,
            ref Vector2 ___mouseOffset,
            ref bool ___customDragAmount,
            ref RectTransform ___tempIcon
            )
        {
            int stackLimit = Mod.settings.GetStackLimit(EItemCategory.Cash);
            CashInstance cashInstance = null;
            if (___draggedSlot != null && ___draggedSlot.assignedSlot != null)
            {
                cashInstance = (___draggedSlot.assignedSlot.ItemInstance as CashInstance);
            }

            __instance.CashSlotHintAnim.Stop();
            __instance.CashSlotHintAnimCanvasGroup.alpha = 0f;
            if (__instance.CanDragFromSlot(___draggedSlot) && __instance.HoveredSlot != null && __instance.CanCashBeDraggedIntoSlot(__instance.HoveredSlot) && !__instance.HoveredSlot.assignedSlot.IsLocked && !__instance.HoveredSlot.assignedSlot.IsAddLocked && __instance.HoveredSlot.assignedSlot.DoesItemMatchFilters(___draggedSlot.assignedSlot.ItemInstance))
            {
                if ((__instance.HoveredSlot.assignedSlot is HotbarSlot) && (__instance.HoveredSlot.assignedSlot is not CashSlot))
                {
                    MethodInfo HoveredSlotSetterInfo = AccessTools.DeclaredPropertySetter(typeof(ItemUIManager), "HoveredSlot");
                    HoveredSlotSetterInfo.Invoke(__instance, [Singleton<HUD>.Instance.cashSlotUI.GetComponent<CashSlotUI>()]);
                }
                float num = Mathf.Min(___draggedCashAmount, cashInstance.Balance);
                if (num > 0f)
                {
                    float num2 = num;
                    if (__instance.HoveredSlot.assignedSlot.ItemInstance != null)
                    {
                        CashInstance cashInstance2 = (__instance.HoveredSlot.assignedSlot.ItemInstance as CashInstance);
                        if (__instance.HoveredSlot.assignedSlot is CashSlot)
                        {
                            num2 = Mathf.Min(num, float.MaxValue - cashInstance2.Balance);
                        }
                        else
                        {
                            num2 = Mathf.Min(num, stackLimit - cashInstance2.Balance);
                        }
                        cashInstance2.ChangeBalance(num2);
                        __instance.HoveredSlot.assignedSlot.ReplicateStoredInstance();
                    }
                    else
                    {
                        CashInstance cashInstance3 = (Registry.GetItem("cash").GetDefaultInstance(1) as CashInstance);
                        cashInstance3.SetBalance(num2, false);
                        __instance.HoveredSlot.assignedSlot.SetStoredItem(cashInstance3, false);
                    }
                    if (num2 >= cashInstance.Balance)
                    {
                        ___draggedSlot.assignedSlot.ClearStoredInstance(false);
                    }
                    else
                    {
                        cashInstance.ChangeBalance(-num2);
                        ___draggedSlot.assignedSlot.ReplicateStoredInstance();
                    }
                }
            }
            ___draggedSlot.SetVisible(true);
            ___draggedSlot.UpdateUI();
            ___draggedSlot.IsBeingDragged = false;
            ___draggedSlot = null;
            UnityEngine.Object.Destroy(___tempIcon.gameObject);
            Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
            return false;
        }

#else

        // This method has hardcoded constants, so we need to replace it entirely
        [HarmonyPatch(typeof(ItemUIManager), "UpdateCashDragAmount")]
        [HarmonyPrefix]
        public static bool UpdateCashDragAmountPrefix(ItemUIManager __instance, CashInstance instance)
        {
            int stackLimit = Mod.settings.GetStackLimit(EItemCategory.Cash);

            float[] array = new float[] { 50f, 10f, 1f };
            float[] array2 = new float[] { 100f, 10f, 1f };
            float num = 0f;
            if (GameInput.MouseScrollDelta > 0f)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (__instance.draggedCashAmount >= array2[i])
                    {
                        num = array[i];
                        break;
                    }
                }
            }
            else if (GameInput.MouseScrollDelta < 0f)
            {
                for (int j = 0; j < array.Length; j++)
                {
                    if (__instance.draggedCashAmount > array2[j])
                    {
                        num = -array[j];
                        break;
                    }
                }
            }
            if (num == 0f)
            {
                return false;
            }
            __instance.draggedCashAmount = Mathf.Clamp(__instance.draggedCashAmount + num, 1f, Mathf.Min(instance.Balance, stackLimit));

            return false;
        }


        // This method has hardcoded constants, so we need to replace it entirely
        [HarmonyPatch(typeof(ItemUIManager), "StartDragCash")]
        [HarmonyPrefix]
        public static bool StartDragCashPrefix(ItemUIManager __instance)
        {
            int stackLimit = Mod.settings.GetStackLimit(EItemCategory.Cash);

            CashInstance cashInstance = __instance.draggedSlot.assignedSlot.ItemInstance.Cast<CashInstance>();
            __instance.draggedCashAmount = Mathf.Min(cashInstance.Balance, stackLimit);
            __instance.draggedAmount = 1;
            if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
            {
                __instance.draggedAmount = 1;
                __instance.draggedCashAmount = Mathf.Min(cashInstance.Balance, 100f);
                __instance.mouseOffset += new Vector2(-10f, -15f);
                __instance.customDragAmount = true;
            }
            if (__instance.draggedCashAmount <= 0f)
            {
                __instance.draggedSlot = null;
                return false;
            }
            if (GameInput.GetButton(GameInput.ButtonCode.QuickMove) && __instance.QuickMoveEnabled)
            {
                Il2CppSystem.Collections.Generic.List<ItemSlot> quickMoveSlots = __instance.GetQuickMoveSlots(__instance.draggedSlot.assignedSlot);
                if (quickMoveSlots.Count > 0)
                {
                    Debug.Log("Quick-moving " + __instance.draggedAmount.ToString() + " items...");
                    float a = __instance.draggedCashAmount;
                    float num = 0f;
                    int i = 0;
                    while (i < quickMoveSlots.Count && num < (float)__instance.draggedAmount)
                    {
                        ItemSlot itemSlot = quickMoveSlots[i];
                        if (itemSlot.ItemInstance != null)
                        {
                            CashInstance cashInstance2 = itemSlot.ItemInstance.Cast<CashInstance>();
                            if (cashInstance2 != null)
                            {
                                float num3;
                                // it's probably this conditional that's failing.
                                // how to safely tell if this condition is true?
                                if (itemSlot.TryCast<CashSlot>() != null)
                                {
                                    num3 = Mathf.Min(a, float.MaxValue - cashInstance2.Balance);
                                }
                                else
                                {
                                    num3 = Mathf.Min(a, stackLimit - cashInstance2.Balance);
                                }
                                cashInstance2.ChangeBalance(num3);
                                itemSlot.ReplicateStoredInstance();
                                num += num3;
                            }
                        }
                        else
                        {
                            CashInstance cashInstance3 = Registry.GetItem("cash").GetDefaultInstance(1).Cast<CashInstance>();
                            cashInstance3.SetBalance(__instance.draggedCashAmount, false);
                            itemSlot.SetStoredItem(cashInstance3, false);
                            num += __instance.draggedCashAmount;
                        }
                        i++;
                    }
                    if (num >= cashInstance.Balance)
                    {
                        __instance.draggedSlot.assignedSlot.ClearStoredInstance(false);
                    }
                    else
                    {
                        cashInstance.ChangeBalance(-num);
                        __instance.draggedSlot.assignedSlot.ReplicateStoredInstance();
                    }
                }
                if (__instance.onItemMoved != null)
                {
                    __instance.onItemMoved.Invoke();
                }
                __instance.draggedSlot = null;
                return false;
            }
            if (__instance.onDragStart != null)
            {
                __instance.onDragStart.Invoke();
            }
            if (__instance.draggedSlot.assignedSlot != PlayerSingleton<PlayerInventory>.Instance.cashSlot)
            {
                __instance.CashSlotHintAnim.Play();
            }
            __instance.tempIcon = __instance.draggedSlot.DuplicateIcon(Singleton<HUD>.Instance.transform, __instance.draggedAmount);
            __instance.tempIcon.Find("Balance").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(__instance.draggedCashAmount, false, false);
            __instance.draggedSlot.IsBeingDragged = true;
            if (__instance.draggedCashAmount >= cashInstance.Balance)
            {
                __instance.draggedSlot.SetVisible(false);
                return false;
            }
            __instance.draggedSlot.ItemUI.Cast<ItemUI_Cash>().SetDisplayedBalance(cashInstance.Balance - __instance.draggedCashAmount);
            return false;
        }


        // This method has hardcoded constants, so we need to replace it completely
        [HarmonyPatch(typeof(ItemUIManager), "EndCashDrag")]
        [HarmonyPrefix]
        public unsafe static bool EndCashDragPrefix(ItemUIManager __instance)
        {
            int stackLimit = Mod.settings.GetStackLimit(EItemCategory.Cash);
            CashInstance cashInstance = null;
            if (__instance.draggedSlot != null && __instance.draggedSlot.assignedSlot != null)
            {
                cashInstance = __instance.draggedSlot.assignedSlot.ItemInstance.Cast<CashInstance>();
            }

            __instance.CashSlotHintAnim.Stop();
            __instance.CashSlotHintAnimCanvasGroup.alpha = 0f;
            if (__instance.CanDragFromSlot(__instance.draggedSlot) && __instance.HoveredSlot != null && __instance.CanCashBeDraggedIntoSlot(__instance.HoveredSlot) && !__instance.HoveredSlot.assignedSlot.IsLocked && !__instance.HoveredSlot.assignedSlot.IsAddLocked && __instance.HoveredSlot.assignedSlot.DoesItemMatchFilters(__instance.draggedSlot.assignedSlot.ItemInstance))
            {
                if ((__instance.HoveredSlot.assignedSlot.TryCast<HotbarSlot> != null) && (__instance.HoveredSlot.assignedSlot.TryCast<CashSlot> == null))
                {
                    __instance.HoveredSlot = Singleton<HUD>.Instance.cashSlotUI.GetComponent<CashSlotUI>();
                }
                float num = Mathf.Min(__instance.draggedCashAmount, cashInstance.Balance);
                if (num > 0f)
                {
                    float num2 = num;
                    if (__instance.HoveredSlot.assignedSlot.ItemInstance != null)
                    {
                        CashInstance cashInstance2 = __instance.HoveredSlot.assignedSlot.ItemInstance.Cast<CashInstance>();
                        if (__instance.HoveredSlot.assignedSlot.TryCast<CashSlot>() != null)
                        {
                            num2 = Mathf.Min(num, float.MaxValue - cashInstance2.Balance);
                        }
                        else
                        {
                            num2 = Mathf.Min(num, stackLimit - cashInstance2.Balance);
                        }
                        cashInstance2.ChangeBalance(num2);
                        __instance.HoveredSlot.assignedSlot.ReplicateStoredInstance();
                    }
                    else
                    {
                        CashInstance cashInstance3 = Registry.GetItem("cash").GetDefaultInstance(1).Cast<CashInstance>();
                        cashInstance3.SetBalance(num2, false);
                        __instance.HoveredSlot.assignedSlot.SetStoredItem(cashInstance3, false);
                    }
                    if (num2 >= cashInstance.Balance)
                    {
                        __instance.draggedSlot.assignedSlot.ClearStoredInstance(false);
                    }
                    else
                    {
                        cashInstance.ChangeBalance(-num2);
                        __instance.draggedSlot.assignedSlot.ReplicateStoredInstance();
                    }
                }
            }
            __instance.draggedSlot.SetVisible(true);
            __instance.draggedSlot.UpdateUI();
            __instance.draggedSlot.IsBeingDragged = false;
            __instance.draggedSlot = null;
            UnityEngine.Object.Destroy(__instance.tempIcon.gameObject);
            Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
            return false;
        }
#endif
    }
}
