﻿using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Reflection.Emit;
using MelonLoader;

using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine.Events;










#if MONO_BUILD
using FishNet;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.UI.Shop;
using ScheduleOne.UI.Stations.Drying_rack;
using ScheduleOne.UI.Stations;
using ScheduleOne.Variables;
using ScheduleOne;
#else
using Il2CppFishNet;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppScheduleOne.AvatarFramework.Equipping;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Product.Packaging;
using Il2CppScheduleOne.StationFramework;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.UI.Management;
using Il2CppScheduleOne.UI.Phone.Delivery;
using Il2CppScheduleOne.UI.Shop;
using Il2CppScheduleOne.UI.Stations.Drying_rack;
using Il2CppScheduleOne.UI.Stations;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.Variables;
using Il2CppScheduleOne;
using Il2CppTMPro;
#endif

namespace ProduceMore
{ 
    public class Sched1PatchesBase
    {
        protected static ProduceMoreMod Mod;

        public static object GetField(Type type, string fieldName, object target)
        {
#if MONO_BUILD
            return AccessTools.Field(type, fieldName).GetValue(target);
#else
            return AccessTools.Property(type, fieldName).GetValue(target);
#endif
        }

        public static void SetField(Type type, string fieldName, object target, object value)
        {
#if MONO_BUILD
            AccessTools.Field(type, fieldName).SetValue(target, value);
#else
            AccessTools.Property(type, fieldName).SetValue(target, value);
#endif
        }

        public static object GetProperty(Type type, string fieldName, object target)
        {
            return AccessTools.Property(type, fieldName).GetValue(target);
        }

        public static void SetProperty(Type type, string fieldName, object target, object value)
        {
            AccessTools.Property(type, fieldName).SetValue(target, value);
        }


        public static object CallMethod(Type type, string methodName, object target, object[] args)
        {
            return AccessTools.Method(type, methodName).Invoke(target, args);
        }

        public static void SetMod(ProduceMoreMod mod)
        {
            Mod = mod;
        }

        public static T CastTo<T>(object o) where T : class
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                return null;
            }
        }

        public static bool Is<T>(object o)
        {
            return o is T;
        }

#if !MONO_BUILD
        public static T CastTo<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        { 
            return o.TryCast<T>();
        }

        public static bool Is<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        {
            return o.TryCast<T>() != null;
        }
#endif

        public static void Log(string message)
        {
            Mod.LoggerInstance.Msg(message);
        }

        public static void Warn(string message)
        {
            Mod.LoggerInstance.Warning(message);
        }

        public static void RestoreDefaults()
        {
            throw new NotImplementedException();
        }
    }


    // Set stack sizes
    [HarmonyPatch]
    public class ItemCapacityPatches : Sched1PatchesBase
    {
        // Increase stack limit on item access
        [HarmonyPatch(typeof(ItemInstance), "StackLimit", MethodType.Getter)]
        [HarmonyPrefix]
        public static void StackLimitPrefix(ItemInstance __instance)
        {
            if (!Mod.processedItemDefs.Contains(__instance.Definition) && __instance.Definition.Name.ToLower() != "cash")
            {
                // Speed Grow is classified as product for some reason
                EItemCategory category;
                if (__instance.Definition.Name == "Speed Grow")
                {
                    category = EItemCategory.Agriculture;
                }
                else
                {
                    category = __instance.Definition.Category;
                }
                if (!Mod.originalStackLimits.ContainsKey(category.ToString()))
                {
                    Mod.originalStackLimits[category.ToString()] = __instance.Definition.StackLimit;
                }

                int stackLimit = Mod.settings.GetStackLimit(__instance);
                // if lookup fails, just use stacklimit of 10
                if (stackLimit == 0)
                {
                    stackLimit = 10;
                }
                __instance.Definition.StackLimit = stackLimit;
                Mod.processedItemDefs.Add(__instance.Definition);
            }
        }

        // For phone delivery app
        [HarmonyPatch(typeof(ListingEntry), "Initialize")]
        [HarmonyPrefix]
        public static void InitializePrefix(ShopListing match)
        {
            if (match != null)
            {
                if (!Mod.processedItemDefs.Contains(match.Item) && match.Item.Name.ToLower() != "cash")
                {
                    // Speed Grow is classified as product for some reason
                    EItemCategory category;
                    if (match.Item.Name == "Speed Grow")
                    {
                        category = EItemCategory.Agriculture;
                    }
                    else
                    {
                        category = match.Item.Category;
                    }

                    if (!Mod.originalStackLimits.ContainsKey(category.ToString()))
                    {
                        Mod.originalStackLimits[category.ToString()] = match.Item.StackLimit;
                    }
                    int stackLimit = Mod.settings.GetStackLimit(match.Item);
                    match.Item.StackLimit = stackLimit;
                    Mod.processedItemDefs.Add(match.Item);
                }
            }
        }

        // update item slots to report accurate capacity
        [HarmonyPatch(typeof(ItemSlot), "GetCapacityForItem")]
        [HarmonyPostfix]
        public static void GetCapacityForItemPostfix(ItemSlot __instance, ref int __result, ItemInstance item, bool checkPlayerFilters)
        {
            if (!__instance.DoesItemMatchHardFilters(item))
            {
                __result = 0;
                return;
            }
            if (checkPlayerFilters && !__instance.DoesItemMatchPlayerFilters(item))
            {
                __result = 0;
                return;
            }
            if (__instance.ItemInstance == null || __instance.ItemInstance.CanStackWith(item, false))
            {
                __result = Mod.settings.GetStackLimit(item) - __instance.Quantity;
                return;
            }
            __result = 0;
            return;
        }


        public static new void RestoreDefaults()
        {
            try
            {
                foreach (var itemDef in Mod.processedItemDefs)
                {
                    if (itemDef.Name.ToLower() != "cash")
                    {
                        if (!Mod.originalStackLimits.ContainsKey(itemDef.Category.ToString()))
                        {
                            itemDef.StackLimit = new ModSettings().GetStackLimit(itemDef);
                        }
                        else
                        {
                            itemDef.StackLimit = Mod.originalStackLimits[itemDef.Category.ToString()];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Warn($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
                Warn($"Source: {e.Source}");
                Warn($"{e.StackTrace}");
                return;
            }
        }

    }

    // This patch requires its own class since we need a TargetMethod method to select
    // the correct overload of GetItem. Regular Harmony annotations don't support this.
    [HarmonyPatch]
    public class RegistryPatches: Sched1PatchesBase
    {
        // GetItem has two definitions: ItemDefinition GetItem(string), and T GetItem<T>(string)
        // TargetMethod required to select non-generic overload of GetItem
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            // no convenience method to select by return type, so iterate instead
            var methods = AccessTools.GetDeclaredMethods(typeof(Registry));
            foreach (var method in methods)
            {
                if (method.Name == "GetItem")
                {
                    if (method.ReturnType == typeof(ItemDefinition))
                    {
                        return method;
                    }
                }
            }

            return null;
        }

        [HarmonyPatch]
        [HarmonyPostfix]
        public static void GetItemPostfix(ref ItemDefinition __result)
        {
            if (__result == null)
            {
                return;
            }
            if (!Mod.processedItemDefs.Contains(__result) && __result.Name.ToLower() != "cash")
            {
                // Speed Grow is classified as product for some reason
                EItemCategory category;
                if (__result.Name == "Speed Grow")
                {
                    category = EItemCategory.Agriculture;
                }
                else
                {
                    category = __result.Category;
                }

                if (!Mod.originalStackLimits.ContainsKey(category.ToString()))
                {
                    Mod.originalStackLimits[category.ToString()] = __result.StackLimit; 
                }

                int stackLimit = Mod.settings.GetStackLimit(__result);
                // if lookup fails, just use stacklimit of 10
                if (stackLimit == 0)
                {
                    stackLimit = 10;
                }
                __result.StackLimit = stackLimit;
                Mod.processedItemDefs.Add(__result);
            }
        }

        public static new void RestoreDefaults()
        {
            // modifications reverted by ItemCapacityPatches.RestoreDefaults
        }
    }


    [HarmonyPatch]
    public class ShopPatches : Sched1PatchesBase
    {

        // allow user to enter values up to 999999
        [HarmonyPatch(typeof(ShopAmountSelector), "OnValueChanged")]
        [HarmonyPrefix]
        public static bool OnValueChangedPrefix(ShopAmountSelector __instance, string value)
        {
            int value2;
            if (int.TryParse(value, out value2))
            {
                SetProperty(typeof(ShopAmountSelector), "SelectedAmount", __instance, Mathf.Clamp(value2, 1, 999999));
                __instance.InputField.SetTextWithoutNotify(__instance.SelectedAmount.ToString());
                return false;
            }
            SetProperty(typeof(ShopAmountSelector), "SelectedAmount", __instance, 1);
            __instance.InputField.SetTextWithoutNotify(string.Empty);

            return false;
        }

        // Call to OnValueChanged probably optimized out
        [HarmonyPatch(typeof(ShopAmountSelector), "OnSubmitted")]
        [HarmonyPrefix]
        public static bool OnSubmittedPrefix(ShopAmountSelector __instance, string value)
        {
            if (!__instance.IsOpen)
            {
                return false;
            }
            CallMethod(typeof(ShopAmountSelector), "OnValueChanged", __instance, [value]);
            if (__instance.onSubmitted != null)
            {
                __instance.onSubmitted.Invoke(__instance.SelectedAmount);
            }
            __instance.Close();

            return false;
        }

        // Modify shop amount selector size so user can enter large numbers
        [HarmonyPatch(typeof(ShopAmountSelector), "Open")]
        [HarmonyPrefix]
        public static void OpenPrefix(ShopAmountSelector __instance)
        {
            if (__instance.InputField.characterLimit != 6)
            {
                __instance.InputField.characterLimit = 6;
                __instance.InputField.pointSize -= 2;
                float width = __instance.Container.rect.width * 1.5f;
                __instance.Container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }


        // make cart entry red X actually remove the entire stack
        [HarmonyPatch(typeof(CartEntry), "Initialize")]
        [HarmonyPrefix]
        public static bool CartEntryInitializePrefix(CartEntry __instance, Cart cart, ShopListing listing, int quantity)
        {
            SetProperty(typeof(CartEntry), "Cart", __instance, cart);
            SetProperty(typeof(CartEntry), "Listing", __instance, listing);
            SetProperty(typeof(CartEntry), "Quantity", __instance, quantity);

#if MONO_BUILD
            __instance.IncrementButton.onClick.AddListener(() =>
            {
                CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [1]);
            });
            __instance.DecrementButton.onClick.AddListener(() =>
            { 
                CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [-1]);
            });
            __instance.RemoveButton.onClick.AddListener(() =>
            {
                CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [-999999]);
            });
#else
            __instance.IncrementButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(() =>
            {
                CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [1]);
            }));
            __instance.DecrementButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(() =>
            { 
                CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [-1]);
            }));
            __instance.RemoveButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(() =>
            {
                CallMethod(typeof(CartEntry), "ChangeAmount", __instance, [-999999]);
            }));
#endif

            CallMethod(typeof(CartEntry), "UpdateTitle", __instance, []);
            CallMethod(typeof(CartEntry), "UpdatePrice", __instance, []);

            return false;
        }


        // Enable user to input more than 999 in delivery app
        // call to OnQuantityInputSubmitted was inlined as well
        [HarmonyPatch(typeof(ListingEntry), "Initialize")]
        [HarmonyPrefix]
        public static bool ListingEntryInitializePrefix(ListingEntry __instance, ShopListing match)
        {
            SetProperty(typeof(ListingEntry), "MatchingListing", __instance, match);
            __instance.Icon.sprite = __instance.MatchingListing.Item.Icon;
            __instance.ItemNameLabel.text = __instance.MatchingListing.Item.Name;
            __instance.ItemPriceLabel.text = MoneyManager.FormatAmount(__instance.MatchingListing.Price, false, false);
#if MONO_BUILD
            __instance.QuantityInput.onSubmit.AddListener( (string value) =>
            {
                CallMethod(typeof(ListingEntry), "OnQuantityInputSubmitted", __instance, [value]);
            });
            __instance.QuantityInput.onEndEdit.AddListener( (string value) =>
            {
                CallMethod(typeof(ListingEntry), "ValidateInput", __instance, []);
            });
            __instance.IncrementButton.onClick.AddListener( () =>
            {
                CallMethod(typeof(ListingEntry), "ChangeQuantity", __instance, [1]);
            });
            __instance.DecrementButton.onClick.AddListener( () =>
            {
                CallMethod(typeof(ListingEntry), "ChangeQuantity", __instance, [-1]);
            });
#else

            __instance.QuantityInput.onSubmit.AddListener(DelegateSupport.ConvertDelegate<UnityAction<string>>( (string value) =>
            {
                CallMethod(typeof(ListingEntry), "OnQuantityInputSubmitted", __instance, [value]);
            }));
            __instance.QuantityInput.onEndEdit.AddListener(DelegateSupport.ConvertDelegate<UnityAction<string>>( (string value) =>
            {
                CallMethod(typeof(ListingEntry), "ValidateInput", __instance, []);
            }));
            __instance.IncrementButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>( () =>
            {
                CallMethod(typeof(ListingEntry), "ChangeQuantity", __instance, [1]);
            }));
            __instance.DecrementButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>( () =>
            {
                CallMethod(typeof(ListingEntry), "ChangeQuantity", __instance, [-1]);
            }));

#endif
            __instance.QuantityInput.SetTextWithoutNotify(__instance.SelectedQuantity.ToString());
            __instance.RefreshLocked();

            if (__instance.QuantityInput.characterLimit != 6)
            {
                __instance.QuantityInput.characterLimit = 6;
                __instance.QuantityInput.textComponent.fontSize = 16;
                CastTo<RectTransform>(__instance.QuantityInput.transform).sizeDelta = new Vector2(80, 40);
            }

            return false;
        }

        // Allow user to purchase more than 999 items at a time from phone app
        [HarmonyPatch(typeof(ListingEntry), "SetQuantity")]
        [HarmonyPrefix]
        public static bool SetQuantityPrefix(ListingEntry __instance, int quant, bool notify)
        {
            if (!__instance.MatchingListing.Item.IsPurchasable)
            {
                quant = 0;
            }
            SetProperty(typeof(ListingEntry), "SelectedQuantity", __instance, Mathf.Clamp(quant, 0, 999999));
            __instance.QuantityInput.SetTextWithoutNotify(__instance.SelectedQuantity.ToString());
            if (notify && __instance.onQuantityChanged != null)
            {
                __instance.onQuantityChanged.Invoke();
            }

            return false;
        }


        // Call to SetQuantity probably optimized out
        [HarmonyPatch(typeof(ListingEntry), "OnQuantityInputSubmitted")]
        [HarmonyPrefix]
        public static bool OnQuantityInputSubmittedPrefix(ListingEntry __instance, string value)
        {
            int quant;
            if (int.TryParse(value, out quant))
            {
                __instance.SetQuantity(quant, true);
                return false;
            }
            __instance.SetQuantity(0, true);

            return false;
        }


        // Call to SetQuantity probably optimized out
        [HarmonyPatch(typeof(ListingEntry), "ChangeQuantity")]
        [HarmonyPrefix]
        public static bool ChangeQuantityPrefix(ListingEntry __instance, int change)
        {
            __instance.SetQuantity(__instance.SelectedQuantity + change, true);

            return false;
        }

        // call to OnQuantityInputSubmitted probably optimized out
        [HarmonyPatch(typeof(ListingEntry), "ValidateInput")]
        [HarmonyPrefix]
        public static bool ValidateInputPrefix(ListingEntry __instance)
        {
            CallMethod(typeof(ListingEntry), "OnQuantityInputSubmitted", __instance, [__instance.QuantityInput.text]);
            return false;
        }

        public static new void RestoreDefaults()
        {
            // no game objects to restore
        }
    }




    // Patch drying rack capacity and speed
    [HarmonyPatch]
    public class DryingRackPatches : Sched1PatchesBase
    {
        // Modify DryingRack.ItemCapacity
        [HarmonyPatch(typeof(StartDryingRackBehaviour), "IsRackReady")]
        [HarmonyPrefix]
        public static void IsRackReadyPrefix(DryingRack rack, ref bool __result)
        {
            if (!Mod.processedStationCapacities.Contains(rack))
            {
                if (!Mod.originalStationCapacities.ContainsKey("DryingRack"))
                {
                    Mod.originalStationCapacities["DryingRack"] = rack.ItemCapacity;
                }
                rack.ItemCapacity = Mod.settings.GetStationCapacity("DryingRack");
                Mod.processedStationCapacities.Add(rack);
            }
        }


        // Modify DryingRack.ItemCapacity
        // canstartoperation runs every time a player or npc tries to interact
        // may have optimized away real access to ItemCapacity; replace method body
        [HarmonyPatch(typeof(DryingRack), "CanStartOperation")]
        [HarmonyPrefix]
        public static bool CanStartOperationPrefix(DryingRack __instance, ref bool __result)
        {
            if (!Mod.processedStationCapacities.Contains(__instance))
            {
                if (!Mod.originalStationCapacities.ContainsKey("DryingRack"))
                {
                    Mod.originalStationCapacities["DryingRack"] = __instance.ItemCapacity;
                }
                __instance.ItemCapacity = Mod.settings.GetStationCapacity("DryingRack");
                Mod.processedStationCapacities.Add(__instance);
            }

            __result = __instance.GetTotalDryingItems() < __instance.ItemCapacity &&
                __instance.InputSlot.Quantity != 0 && 
                !__instance.InputSlot.IsLocked && 
                !__instance.InputSlot.IsRemovalLocked;

            return false;
        }


        // fix drying operation progress meter
        [HarmonyPatch(typeof(DryingOperationUI), "UpdatePosition")]
        [HarmonyPrefix]
        public static bool UpdatePositionPrefix(DryingOperationUI __instance)
        {
            if (!Mod.originalStationTimes.ContainsKey("DryingRack"))
            {
                Mod.originalStationTimes["DryingRack"] = DryingRack.DRY_MINS_PER_TIER;
               
            }

            float stationSpeed =  Mod.settings.enableStationAnimationAcceleration ? (float)Mod.originalStationTimes["DryingRack"] / Mod.settings.GetStationSpeed("DryingRack") : Mod.originalStationTimes["DryingRack"];
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
            if (!Mod.originalStationTimes.ContainsKey("DryingRack"))
            {
                Mod.originalStationTimes["DryingRack"] = DryingRack.DRY_MINS_PER_TIER;
            }
            int dryingTime = (int)((float)Mod.originalStationTimes["DryingRack"] / Mod.settings.GetStationSpeed("DryingRack"));

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
            if (!Mod.originalStationTimes.ContainsKey("DryingRack"))
            {
                Mod.originalStationTimes["DryingRack"] = DryingRack.DRY_MINS_PER_TIER;
            }
            if (__instance == null)
            {
                return false;
            }
            foreach (DryingOperation dryingOperation in __instance.DryingOperations.ToArray())
            {
                dryingOperation.Time++;
                if (dryingOperation.Time >= ((float)Mod.originalStationTimes["DryingRack"] / Mod.settings.GetStationSpeed("DryingRack")))
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


        // speed up employees hanging up leaves
        [HarmonyPatch(typeof(StartDryingRackBehaviour), "RpcLogic___BeginAction_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_BeginActionPrefix(StartDryingRackBehaviour __instance)
        {
            if (__instance.WorkInProgress)
            {
                return false;
            }
            if (__instance.Rack == null)
            {
                return false;
            }

            SetProperty(typeof(StartDryingRackBehaviour), "WorkInProgress", __instance, true);
            __instance.Npc.Movement.FacePoint(__instance.Rack.uiPoint.position, 0.5f);
            object workCoroutine = MelonCoroutines.Start(BeginActionCoroutine(__instance));
            SetField(typeof(StartDryingRackBehaviour), "workRoutine", __instance, (Coroutine)workCoroutine);
            return false;
        }

        // Replacement coroutine for BeginAction
        private static IEnumerator BeginActionCoroutine(StartDryingRackBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("DryingRack") : 1f;
            yield return new WaitForEndOfFrame();
            behaviour.Rack.InputSlot.ItemInstance.GetCopy(1);
            int itemCount = 0;
            while (behaviour.Rack != null && behaviour.Rack.InputSlot.Quantity > itemCount && behaviour.Rack.GetTotalDryingItems() + itemCount < behaviour.Rack.ItemCapacity)
            {
                behaviour.Npc.Avatar.Anim.SetTrigger("GrabItem");
                yield return new WaitForSeconds(1f / stationSpeed);
                int num = itemCount;
                itemCount = num + 1;
            }
            if (InstanceFinder.IsServer)
            {
                behaviour.Rack.StartOperation();
            }
            SetProperty(typeof(StartDryingRackBehaviour), "WorkInProgress", behaviour, false);
            SetField(typeof(StartDryingRackBehaviour), "workRoutine", behaviour, null);
            yield break;
        }

        [HarmonyPatch(typeof(StartDryingRackBehaviour), "StopCauldron")]
        [HarmonyPrefix]
        public static bool StopCauldronPrefix(StartDryingRackBehaviour __instance)
        {
            object workRoutine = GetField(typeof(StartDryingRackBehaviour), "workRoutine", __instance);
            if (workRoutine != null)
            {
                MelonCoroutines.Stop(workRoutine);
                SetField(typeof(StartDryingRackBehaviour), "workRoutine", __instance, null);
            }
            SetProperty(typeof(StartDryingRackBehaviour), "WorkInProgress", __instance, false);

            return false;
        }

        public static new void RestoreDefaults()
        {
            try
            {
                foreach (var station in Mod.processedStationCapacities)
                {
                    DryingRack rack = CastTo<DryingRack>(station);
                    if (rack != null)
                    {
                        if (!Mod.originalStationCapacities.ContainsKey("DryingRack"))
                        {
                            rack.ItemCapacity = 20;
                        }
                        else
                        {
                            rack.ItemCapacity = Mod.originalStationCapacities["DryingRack"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Warn($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
                Warn($"Source: {e.Source}");
                Warn($"{e.StackTrace}");
                return;
            }
        }
    }


    // Patch lab oven capacity and speed
    [HarmonyPatch]
    public class LabOvenPatches : Sched1PatchesBase
    {
        // speed
        [HarmonyPatch(typeof(OvenCookOperation), "GetCookDuration")]
        [HarmonyPostfix]
        public static void GetCookDurationPostfix(OvenCookOperation __instance, ref int __result)
        {
            if (!Mod.originalStationTimes.ContainsKey("LabOven"))
            {
                Mod.originalStationTimes["LabOven"] = __instance.Ingredient.StationItem.GetModule<CookableModule>().CookTime;
            }
            __result = (int)((float)Mod.originalStationTimes["LabOven"] / Mod.settings.GetStationSpeed("LabOven"));
        }

        // call to GetCookDuration seems to have been optimized out.
        [HarmonyPatch(typeof(OvenCookOperation), "IsReady")]
        [HarmonyPostfix]
        public static void IsReadyPostfix(OvenCookOperation __instance, ref bool __result)
        {
            // Re-insert original method body.
            __result = __instance.CookProgress >= __instance.GetCookDuration();
        }

        // allow player to start new cookoperation as long as output has space
        [HarmonyPatch(typeof(LabOvenCanvas), "DoesOvenOutputHaveSpace")]
        [HarmonyPostfix]
        public static void DoesOvenOutputHaveSpacePostfix(LabOvenCanvas __instance, ref bool __result)
        {
            ItemInstance productInstance = __instance.Oven.CurrentOperation.Product.GetDefaultInstance(1);
            ItemInstance outputInstance = __instance.Oven.OutputSlot.ItemInstance;
            int capacity = 0;

            // for some reason calling getcapacity here doesn't result in our postfix running
            // parent function might be overridden in child classes maybe?
            // for now just inline body of getcapacity
            if (!__instance.Oven.OutputSlot.DoesItemMatchHardFilters(productInstance))
            {
                capacity = 0;
            }
            else if (outputInstance == null)
            {
                capacity = Mod.settings.GetStackLimit(productInstance);
            }
            else if (productInstance.CanStackWith(outputInstance.Definition.GetDefaultInstance(1), false))
            {
                capacity = Mod.settings.GetStackLimit(productInstance) - outputInstance.Quantity;
            }

            int quantityProduced = __instance.Oven.CurrentOperation.Cookable.ProductQuantity;
            __result = capacity >= quantityProduced;

        }
        
        // Call our own startcook coroutine with accelerated animations
        [HarmonyPatch(typeof(StartLabOvenBehaviour), "RpcLogic___StartCook_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_StartCookPrefix(StartLabOvenBehaviour __instance)
        {
            try
            {
                if (GetField(typeof(StartLabOvenBehaviour), "cookRoutine", __instance) != null || __instance.targetOven == null)
                {
                    return false;
                }
                object workRoutine = MelonCoroutines.Start(StartCookCoroutine(__instance));
                SetField(typeof(StartLabOvenBehaviour), "cookRoutine", __instance, (Coroutine)workRoutine);
            }
            catch (Exception e)
            {
                Warn($"Failed to set cookroutine: {e.GetType().Name} - {e.Message}");
                Warn($"Source: {e.Source}");
                Warn($"{e.StackTrace}");
            }

            return false;
        }

        // Startcook coroutine with accelerated animations
        private static IEnumerator StartCookCoroutine(StartLabOvenBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("LabOven") : 1f;
            behaviour.targetOven.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.targetOven.transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f / stationSpeed);

            if (!(bool)CallMethod(typeof(StartLabOvenBehaviour), "CanCookStart", behaviour, []))
            {
                CallMethod(typeof(StartLabOvenBehaviour), "StopCook", behaviour, []);
                behaviour.End_Networked(null);
                yield break;
            }

            behaviour.targetOven.Door.SetPosition(1f / stationSpeed);
            yield return new WaitForSeconds(0.5f / stationSpeed);

            behaviour.targetOven.WireTray.SetPosition(1f / stationSpeed);
            yield return new WaitForSeconds(5f / stationSpeed);

            behaviour.targetOven.Door.SetPosition(0f);
            yield return new WaitForSeconds(1f / stationSpeed);

            ItemInstance itemInstance = behaviour.targetOven.IngredientSlot.ItemInstance;
            if (itemInstance == null)
            {
                CallMethod(typeof(StartLabOvenBehaviour), "StopCook", behaviour, []);
                behaviour.End_Networked(null);
                yield break;
            }

            int num = 1;
            if ((CastTo<StorableItemDefinition>(itemInstance.Definition)).StationItem.GetModule<CookableModule>().CookType == CookableModule.ECookableType.Solid)
            {
                num = Mathf.Min(behaviour.targetOven.IngredientSlot.Quantity, 10);
            }
            itemInstance.ChangeQuantity(-num);
            string id = (CastTo<StorableItemDefinition>(itemInstance.Definition)).StationItem.GetModule<CookableModule>().Product.ID;
            EQuality ingredientQuality = EQuality.Standard;
            if (Is<QualityItemInstance>(itemInstance))
            {
                ingredientQuality = (CastTo<QualityItemInstance>(itemInstance)).Quality;
            }
            behaviour.targetOven.SendCookOperation(new OvenCookOperation(itemInstance.ID, ingredientQuality, num, id));
            CallMethod(typeof(StartLabOvenBehaviour), "StopCook", behaviour, []);
            behaviour.End_Networked(null);
            yield break;
        }


        [HarmonyPatch(typeof(StartLabOvenBehaviour), "StopCook")]
        [HarmonyPrefix]
        public static bool StopCookPrefix(StartLabOvenBehaviour __instance)
        {
            if (__instance.targetOven != null)
            {
                __instance.targetOven.SetNPCUser(null);
            }
            object workRoutine = GetField(typeof(StartLabOvenBehaviour), "cookRoutine", __instance);
            if (workRoutine != null)
            {
                MelonCoroutines.Stop(workRoutine);
                SetField(typeof(StartLabOvenBehaviour), "cookRoutine", __instance, null);
            }

            return false;
        }



        // Call our own finishcook coroutine with accelerated animations
        [HarmonyPatch(typeof(FinishLabOvenBehaviour), "RpcLogic___StartAction_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_StartFinishCookPrefix(FinishLabOvenBehaviour __instance)
        {
            if (GetField(typeof(FinishLabOvenBehaviour), "actionRoutine", __instance) != null)
            {
                return false;
            }
            if (__instance.targetOven == null)
            {
                return false;
            }
            object workRoutine = MelonCoroutines.Start(FinishCookCoroutine(__instance));
            SetField(typeof(FinishLabOvenBehaviour), "actionRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        // FinishCook coroutine with accelerated animations
        private static IEnumerator FinishCookCoroutine(FinishLabOvenBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("LabOven") : 1f;
            behaviour.targetOven.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.targetOven.transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f);

            if (!(bool)CallMethod(typeof(FinishLabOvenBehaviour), "CanActionStart", behaviour, []))
            {
                CallMethod(typeof(FinishLabOvenBehaviour), "StopAction", behaviour, []);
                behaviour.End_Networked(null);
                yield break;
            }

            behaviour.Npc.SetEquippable_Networked(null, "Avatar/Equippables/Hammer");
            behaviour.targetOven.Door.SetPosition(1f / stationSpeed);
            behaviour.targetOven.WireTray.SetPosition(1f / stationSpeed);
            yield return new WaitForSeconds(0.5f / stationSpeed);

            behaviour.targetOven.SquareTray.SetParent(behaviour.targetOven.transform);
            behaviour.targetOven.RemoveTrayAnimation.Play();
            yield return new WaitForSeconds(0.1f);

            behaviour.targetOven.Door.SetPosition(0f);
            yield return new WaitForSeconds(1f / stationSpeed);

            behaviour.Npc.SetAnimationBool_Networked(null, "UseHammer", true);
            yield return new WaitForSeconds(10f / stationSpeed);

            behaviour.Npc.SetAnimationBool_Networked(null, "UseHammer", false);
            behaviour.targetOven.Shatter(behaviour.targetOven.CurrentOperation.Cookable.ProductQuantity, behaviour.targetOven.CurrentOperation.Cookable.ProductShardPrefab.gameObject);
            yield return new WaitForSeconds(1f / stationSpeed);
            
            ItemInstance productItem = behaviour.targetOven.CurrentOperation.GetProductItem(behaviour.targetOven.CurrentOperation.Cookable.ProductQuantity * behaviour.targetOven.CurrentOperation.IngredientQuantity);
            behaviour.targetOven.OutputSlot.AddItem(productItem, false);
            
            behaviour.targetOven.SendCookOperation(null);
            CallMethod(typeof(FinishLabOvenBehaviour), "StopAction", behaviour, []);
            behaviour.End_Networked(null);
            yield break;
        }

        [HarmonyPatch(typeof(FinishLabOvenBehaviour), "StopAction")]
        [HarmonyPrefix]
        public static bool StopActionPrefix(FinishLabOvenBehaviour __instance)
        {
            __instance.targetOven.SetNPCUser(null);
            __instance.Npc.SetEquippable_Networked(null, string.Empty);
            __instance.Npc.SetAnimationBool_Networked(null, "UseHammer", false);

            object workRoutine = GetField(typeof(FinishLabOvenBehaviour), "actionRoutine", __instance);
            if (workRoutine != null)
            {
                MelonCoroutines.Stop(workRoutine);
                SetField(typeof(FinishLabOvenBehaviour), "actionRoutine", __instance, null);
            }

            return false;
        }

        public static new void RestoreDefaults()
        {
            // not needed, because we don't change any object data
        }

    }


    // Patch mixing station capacity and speed
    [HarmonyPatch]
    public class MixingStationPatches : Sched1PatchesBase
    {
        // capacity
        [HarmonyPatch(typeof(MixingStation), "GetMixQuantity")]
        [HarmonyPrefix]
        public static bool GetMixQuantityPrefix(MixingStation __instance, ref int __result)
        {
            if (!Mod.processedStationCapacities.Contains(__instance))
            {
                if (Is<MixingStationMk2>(__instance))
                {
                    if (!Mod.originalStationCapacities.ContainsKey("MixingStationMk2"))
                    {
                        Mod.originalStationCapacities["MixingStationMk2"] = __instance.MaxMixQuantity;
                    }
                    __instance.MaxMixQuantity = Mod.settings.GetStationCapacity("MixingStationMk2");

                }
                else
                {
                    if (!Mod.originalStationCapacities.ContainsKey("MixingStation"))
                    {
                        Mod.originalStationCapacities["MixingStation"] = __instance.MaxMixQuantity;
                    }
                    __instance.MaxMixQuantity = Mod.settings.GetStationCapacity("MixingStation");

                }
                Mod.processedStationCapacities.Add(__instance);
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

        // actual call to GetMixQuantity seems to have been optimized out.
        [HarmonyPatch(typeof(MixingStation), "IsMixingDone", MethodType.Getter)]
        [HarmonyPostfix]
        public static void IsMixingDonePostfix(MixingStation __instance, ref bool __result)
        {
            // re-insert original method body.
            __result = __instance.CurrentMixOperation != null && __instance.CurrentMixTime >= __instance.GetMixTimeForCurrentOperation();
        }

        // speed
        [HarmonyPatch(typeof(MixingStation), "GetMixTimeForCurrentOperation")]
        [HarmonyPrefix]
        public static bool GetMixTimePrefix(MixingStation __instance, ref int __result)
        {
            if (!Mod.processedStationSpeeds.Contains(__instance))
            {
                if (Is<MixingStationMk2>(__instance))
                {
                    if (!Mod.originalStationTimes.ContainsKey("MixingStationMk2"))
                    {
                        // Use a dirty magic number for now; relevant constant is not accessible in il2cpp
                        Mod.originalStationTimes["MixingStationMk2"] = 3;
                    }
                    __instance.MixTimePerItem = (int)Mathf.Max((float)Mod.originalStationTimes["MixingStationMk2"] / Mod.settings.GetStationSpeed("MixingStationMk2"), 1f);
                }
                else
                {
                    if (!Mod.originalStationTimes.ContainsKey("MixingStation"))
                    {
                        // Use a dirty magic number for now; relevant constant is not accessible in il2cpp
                        Mod.originalStationTimes["MixingStation"] = 15;
                    }
                    __instance.MixTimePerItem = (int)Mathf.Max((float)Mod.originalStationTimes["MixingStation"] / Mod.settings.GetStationSpeed("MixingStation"), 1f);
                }

                // TODO: check if it's even necessary to modify this field anymore
                Mod.processedStationSpeeds.Add(__instance);
            }

            int originalStationTime;
            float stationSpeed;
            if (Is<MixingStationMk2>(__instance))
            {
                stationSpeed = Mod.settings.GetStationSpeed("MixingStationMk2");
                originalStationTime = Mod.originalStationTimes["MixingStationMk2"];
            }
            else
            {
                stationSpeed = Mod.settings.GetStationSpeed("MixingStation");
                originalStationTime = Mod.originalStationTimes["MixingStation"];
            }
            float mixTimePerItem = (float)originalStationTime / stationSpeed;

            if (__instance.CurrentMixOperation == null)
            {
                __result = 0;
                return false;
            }
            __result = (int)Mathf.Max((mixTimePerItem * (float)__instance.CurrentMixOperation.Quantity), 1f);

            return false;
        }

        [HarmonyPatch(typeof(Chemist), "GetMixStationsReadyToMove")]
        [HarmonyPrefix]
#if MONO_BUILD
        public static bool GetMixStationsReadyToMovePrefix(Chemist __instance, ref List<MixingStation> __result)
        {
            var list = new List<MixingStation>();
#else
        public static bool GetMixStationsReadyToMovePrefix(Chemist __instance, ref Il2CppSystem.Collections.Generic.List<MixingStation> __result)
        { 
            var list = new Il2CppSystem.Collections.Generic.List<MixingStation>();
#endif

            foreach (MixingStation mixingStation in CastTo<ChemistConfiguration>(GetProperty(typeof(Chemist), "configuration", __instance)).MixStations)
            {
                ItemSlot outputSlot = mixingStation.OutputSlot;
                BuildableItem destination = CastTo<MixingStationConfiguration>(mixingStation.Configuration).Destination.SelectedObject;
                //TODO: move transitroutevalid check after packaging station check
                if (outputSlot.Quantity != 0 && __instance.MoveItemBehaviour.IsTransitRouteValid(CastTo<MixingStationConfiguration>(mixingStation.Configuration).DestinationRoute, outputSlot.ItemInstance.ID))
                {
                    // Only deliver to packaging stations with at least half a stack of space in input slot.
                    if (Is<PackagingStation>(destination))
                    {
                        PackagingStation station = CastTo<PackagingStation>(destination);
                        int inputStackLimit = Mod.settings.GetStackLimit(station.InputSlots[0].ItemInstance);
                        if (inputStackLimit - station.InputSlots[0].Quantity > inputStackLimit / 2)
                        {
                            list.Add(mixingStation);
                        }
                    }
                    else
                    {
                        list.Add(mixingStation);

                    }
                }
            }

            __result = list;

            return false;
        }


        [HarmonyPatch(typeof(StartMixingStationBehaviour), "StartCook")]
        [HarmonyPrefix]
        public static void StartCookPrefix(StartMixingStationBehaviour __instance)
        {
            if (!Mod.processedStationCapacities.Contains(__instance.targetStation))
            {
                if (Is<MixingStationMk2>(__instance))
                {
                    if (!Mod.originalStationCapacities.ContainsKey("MixingStationMk2"))
                    {
                        Mod.originalStationCapacities["MixingStationMk2"] = __instance.targetStation.MaxMixQuantity;
                    }
                    __instance.targetStation.MaxMixQuantity = Mod.settings.GetStationCapacity("MixingStationMk2");

                }
                else
                {
                    if (!Mod.originalStationCapacities.ContainsKey("MixingStation"))
                    {
                        Mod.originalStationCapacities["MixingStation"] = __instance.targetStation.MaxMixQuantity;
                    }
                    __instance.targetStation.MaxMixQuantity = Mod.settings.GetStationCapacity("MixingStation");

                }
                Mod.processedStationCapacities.Add(__instance.targetStation);
            }
        }

        // GetMixTimeForCurrentOperation seems to have been optimized out.
        [HarmonyPatch(typeof(MixingStation), "MinPass")]
        [HarmonyPrefix]
        public static bool MinPassPrefix(MixingStation __instance)
        {
            if (__instance.CurrentMixOperation != null || __instance.OutputSlot.Quantity > 0)
            {
                int num = 0;
                if (__instance.CurrentMixOperation != null)
                {
                    int currentMixTime = __instance.CurrentMixTime;
                    int currentMixTime2 = __instance.CurrentMixTime;
                    SetProperty(typeof(MixingStation), "CurrentMixTime", __instance, currentMixTime2 + 1);
                    num = __instance.GetMixTimeForCurrentOperation();
                    if (__instance.CurrentMixTime >= num && currentMixTime < num && InstanceFinder.IsServer)
                    {
                        NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Mixing_Operations_Completed", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Mixing_Operations_Completed") + 1f).ToString(), true);
                        __instance.MixingDone_Networked();
                    }
                }
                if (__instance.Clock != null)
                {
                    __instance.Clock.SetScreenLit(true);
                    __instance.Clock.DisplayMinutes(Mathf.Max(num - __instance.CurrentMixTime, 0));
                }
                if (__instance.Light != null)
                {
                    if (__instance.IsMixingDone)
                    {
                        __instance.Light.isOn = (NetworkSingleton<TimeManager>.Instance.DailyMinTotal % 2 == 0);
                        return false;
                    }
                    __instance.Light.isOn = true;
                    return false;
                }
            }
            else
            {
                if (__instance.Clock != null)
                {
                    __instance.Clock.SetScreenLit(false);
                    __instance.Clock.DisplayText(string.Empty);
                }
                if (__instance.Light != null && __instance.IsMixingDone)
                {
                    __instance.Light.isOn = false;
                }
            }
            return false;
        }


        [HarmonyPatch(typeof(StartMixingStationBehaviour), "RpcLogic___StartCook_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_StartCookPrefix(StartMixingStationBehaviour __instance)
        {
            if (GetField(typeof(StartMixingStationBehaviour), "startRoutine", __instance) != null)
            {
                return false;
            }
            if (__instance.targetStation == null)
            {
                return false;
            }
            object workRoutine = MelonCoroutines.Start(StartMixCoroutine(__instance));
            SetField(typeof(StartMixingStationBehaviour), "startRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        private static IEnumerator StartMixCoroutine(StartMixingStationBehaviour behaviour)
        {
            float stationSpeed;
            if (!Mod.settings.enableStationAnimationAcceleration)
            {
                stationSpeed = 1f;
            }
            else if (Is<MixingStationMk2>(behaviour.targetStation))
            {
                stationSpeed = Mod.settings.GetStationSpeed("MixingStationMk2");
            }
            else
            {
                stationSpeed = Mod.settings.GetStationSpeed("MixingStation");
            }

            behaviour.Npc.Movement.FacePoint(behaviour.targetStation.transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f);

            if (!(bool)CallMethod(typeof(StartMixingStationBehaviour), "CanCookStart", behaviour, []))
            {
                CallMethod(typeof(StartMixingStationBehaviour), "StopCook", behaviour, []);
                behaviour.End_Networked(null);
                yield break;
            }

            behaviour.targetStation.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", true);
            QualityItemInstance product = CastTo<QualityItemInstance>(behaviour.targetStation.ProductSlot.ItemInstance);
            ItemInstance mixer = behaviour.targetStation.MixerSlot.ItemInstance;
            int mixQuantity = behaviour.targetStation.GetMixQuantity();
            float mixTime = (float)mixQuantity / stationSpeed;
            // waiting for more than a second or two at a time is a bad idea.
            // waiting for less than 20ms is also a bad idea.
            // just yield every second for a happy medium.
            for (int i = 0; i < mixTime; ++i)
            {
                yield return new WaitForSeconds(1f);
            }

            if (InstanceFinder.IsServer)
            {
                behaviour.targetStation.ProductSlot.ChangeQuantity(-mixQuantity, false);
                behaviour.targetStation.MixerSlot.ChangeQuantity(-mixQuantity, false);
                MixOperation operation = new MixOperation(product.ID, product.Quality, mixer.ID, mixQuantity);
                behaviour.targetStation.SendMixingOperation(operation, 0);
            }

            CallMethod(typeof(StartMixingStationBehaviour), "StopCook", behaviour, []);
            behaviour.End_Networked(null);
            yield break;
        }


        [HarmonyPatch(typeof(StartMixingStationBehaviour), "StopCook")]
        [HarmonyPrefix]
        public static bool StopCookPrefix(StartMixingStationBehaviour __instance)
        {
            if (__instance.targetStation != null)
            {
                __instance.targetStation.SetNPCUser(null);
            }
            __instance.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", false);

            object workRoutine = GetField(typeof(StartMixingStationBehaviour), "startRoutine", __instance);
            if (workRoutine != null)
            {
                MelonCoroutines.Stop(workRoutine);
                SetField(typeof(StartMixingStationBehaviour), "startRoutine", __instance, null);
            }

            return false;

        }

        public static new void RestoreDefaults()
        {
            try
            {
                foreach (var station in Mod.processedStationCapacities)
                {
                    MixingStation mixingStation = CastTo<MixingStation>(station);
                    if (mixingStation != null)
                    {
                        if (Is<MixingStationMk2>(mixingStation))
                        {
                            if (!Mod.originalStationCapacities.ContainsKey("MixingStationMk2"))
                            {
                                mixingStation.MaxMixQuantity = 20;
                            }
                            else
                            {
                                mixingStation.MaxMixQuantity = Mod.originalStationCapacities["MixingStationMk2"];
                            }
                            
                        }
                        else
                        {
                            if (!Mod.originalStationCapacities.ContainsKey("MixingStation"))
                            {
                                mixingStation.MaxMixQuantity = 10;
                            }
                            else
                            {
                                mixingStation.MaxMixQuantity = Mod.originalStationCapacities["MixingStation"];
                            }
                        }
                    }
                }

                foreach(var station in Mod.processedStationTimes)
                {
                    MixingStation mixingStation = CastTo<MixingStation>(station);
                    if (Mod.processedStationTimes.Contains(station))
                    {
                        if (Is<MixingStationMk2>(mixingStation))
                        {
                            if (!Mod.originalStationTimes.ContainsKey("MixingStationMk2"))
                            {
                                mixingStation.MixTimePerItem = 3;
                            }
                            else
                            {
                                mixingStation.MixTimePerItem = Mod.originalStationTimes["MixingStationMk2"];
                            }

                        }
                        else
                        {
                            if (!Mod.originalStationTimes.ContainsKey("MixingStation"))
                            {
                                mixingStation.MixTimePerItem = 15;
                            }
                            else
                            {
                                mixingStation.MixTimePerItem = Mod.originalStationTimes["MixingStation"];
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Warn($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
                Warn($"Source: {e.Source}");
                Warn($"{e.StackTrace}");
                return;
            }
        }


        // increase threshold
        [HarmonyPatch(typeof(MixingStationUIElement), "Initialize")]
        [HarmonyPrefix]
        public static void InitializeUIPrefix(MixingStation station)
        {
            int stationCapacity = Is<MixingStationMk2>(station) ? Mod.settings.GetStationCapacity("MixingStationMk2") : Mod.settings.GetStationCapacity("MixingStation");
            CastTo<MixingStationConfiguration>(station.Configuration).StartThrehold.Configure(1f, stationCapacity, true);
        }

    }


    // Brick press patches
    [HarmonyPatch]
    public class BrickPressPatches : Sched1PatchesBase
    {
        // call our own coroutine with accelerated animations
        [HarmonyPatch(typeof(BrickPressBehaviour), "RpcLogic___BeginPackaging_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_BeginPackagingPrefix(BrickPressBehaviour __instance)
        {
            if (__instance.PackagingInProgress)
            {
                return false;
            }
            if (__instance.Press == null)
            {
                return false;
            }
            SetProperty(typeof(BrickPressBehaviour), "PackagingInProgress", __instance, true);
            __instance.Npc.Movement.FaceDirection(__instance.Press.StandPoint.forward, 0.5f);
            object workRoutine = MelonCoroutines.Start(PackagingCoroutine(__instance));
            SetField(typeof(BrickPressBehaviour), "packagingRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        private static IEnumerator PackagingCoroutine(BrickPressBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("BrickPress") : 1f;
            yield return new WaitForEndOfFrame();

            behaviour.Npc.Avatar.Anim.SetBool("UsePackagingStation", true);
            float packageTime = 15f / (CastTo<Packager>(behaviour.Npc).PackagingSpeedMultiplier * stationSpeed);
            for (float i = 0f; i < packageTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Press.uiPoint.position, 0, false);
                yield return new WaitForEndOfFrame();
            }

            behaviour.Npc.Avatar.Anim.SetBool("UsePackagingStation", false);
            yield return new WaitForSeconds(0.2f);

            behaviour.Npc.Avatar.Anim.SetTrigger("GrabItem");
            behaviour.Press.PlayPressAnim();
            yield return new WaitForSeconds(1f);

            ProductItemInstance product;
            if (InstanceFinder.IsServer && behaviour.Press.HasSufficientProduct(out product))
            {
                behaviour.Press.CompletePress(product);
            }
            SetProperty(typeof(BrickPressBehaviour), "PackagingInProgress", behaviour, false);
            SetField(typeof(BrickPressBehaviour), "packagingRoutine", behaviour, null);
            yield break;
        }


        public static new void RestoreDefaults()
        {
            // We don't modify any game objects, so there's nothing to restore
        }
    }


    // cauldron patches
    [HarmonyPatch]
    public class CauldronPatches : Sched1PatchesBase
    {
        // patch visuals for capacity
        [HarmonyPatch(typeof(Cauldron), "UpdateIngredientVisuals")]
        [HarmonyPrefix]
        public static bool UpdateIngredientVisualsPatch(Cauldron __instance)
        {
            int cauldronCapacity = Mod.settings.GetStackLimit("Coca Leaf", EItemCategory.Agriculture);
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
        [HarmonyPatch(typeof(Cauldron), "StartCookOperation")]
        [HarmonyPrefix]
        public static void StartCookOperationPrefix(Cauldron __instance, ref int remainingCookTime)
        {
            if (!Mod.processedStationTimes.Contains(__instance))
            {
                if (!Mod.originalStationTimes.ContainsKey("Cauldron"))
                {
                    Mod.originalStationTimes["Cauldron"] = __instance.CookTime;
                }
                int newCookTime = (int)((float)Mod.originalStationTimes["Cauldron"] / Mod.settings.GetStationSpeed("Cauldron"));
                __instance.CookTime = newCookTime;
                Mod.processedStationTimes.Add(__instance);
            }
            remainingCookTime = (int)((float)Mod.originalStationTimes["Cauldron"] / Mod.settings.GetStationSpeed("Cauldron"));
        }

        // accurately calculate output space instead of assuming limit of 10
        [HarmonyPatch(typeof(Cauldron), "HasOutputSpace")]
        [HarmonyPostfix]
        public static void HasOutputSpacePostfix(Cauldron __instance, ref bool __result)
        {
            __result = Mod.settings.GetStackLimit(__instance.CocaineBaseDefinition) >= 10;
        }

        // call to HasOutputSpace seems to have been optimized out.
        [HarmonyPatch(typeof(Cauldron), "GetState")]
        [HarmonyPostfix]
        public static void GetStatePostfix(Cauldron __instance, ref Cauldron.EState __result)
        {
            // re-insert original method body.
            if ((bool)GetProperty(typeof(Cauldron), "isCooking", __instance))
            {
                __result = Cauldron.EState.Cooking;
            }
            else if (!__instance.HasIngredients())
            {
                __result = Cauldron.EState.MissingIngredients;
            }
            else if (!__instance.HasOutputSpace())
            {
                __result = Cauldron.EState.OutputFull;
            }
            else
            {
                __result = Cauldron.EState.Ready;
            }
        }

        // Override cauldron work routine to accelerate animation
        [HarmonyPatch(typeof(StartCauldronBehaviour), "RpcLogic___BeginCauldron_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_BeginCauldronPrefix(StartCauldronBehaviour __instance)
        {
            if (__instance.WorkInProgress)
            {
                return false;
            }
            if (__instance.Station == null)
            {
                return false;
            }

            SetProperty(typeof(StartCauldronBehaviour), "WorkInProgress", __instance, true);
            __instance.Npc.Movement.FaceDirection(__instance.Station.StandPoint.forward, 0.5f);
            object workCoroutine = MelonCoroutines.Start(BeginCauldronCoroutine(__instance));
            SetField(typeof(StartCauldronBehaviour), "workRoutine", __instance, (Coroutine)workCoroutine);
            return false;
        }


        // coroutine with reduced animation times
        private static IEnumerator BeginCauldronCoroutine(StartCauldronBehaviour behaviour)
        {
            yield return new WaitForEndOfFrame();
            behaviour.Npc.Avatar.Anim.SetBool("UseChemistryStation", true);
            float packageTime = 15f / Mod.settings.GetStationSpeed("Cauldron");
            for (float i = 0f; i < packageTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Station.LinkOrigin.position, 0, false);
                yield return new WaitForEndOfFrame();
            }
            behaviour.Npc.Avatar.Anim.SetBool("UseChemistryStation", false);
            if (InstanceFinder.IsServer)
            {
                EQuality quality = behaviour.Station.RemoveIngredients();
                behaviour.Station.StartCookOperation(null, behaviour.Station.CookTime, quality);
            }
            
            SetProperty(typeof(StartCauldronBehaviour), "WorkInProgress", behaviour, false);
            SetField(typeof(StartCauldronBehaviour), "workRoutine", behaviour, null);
            yield break;
        }


        [HarmonyPatch(typeof(StartCauldronBehaviour), "StopCauldron")]
        [HarmonyPrefix]
        public static bool StopCauldronPrefix(StartCauldronBehaviour __instance)
        {
            object workRoutine = GetField(typeof(StartCauldronBehaviour), "workRoutine", __instance);
            if (workRoutine != null)
            {
                MelonCoroutines.Stop(workRoutine);
                SetField(typeof(StartCauldronBehaviour), "workRoutine", __instance, null);
            }
            if (InstanceFinder.IsServer && __instance.Station != null && __instance.Station.NPCUserObject == __instance.Npc.NetworkObject)
            {
                __instance.Station.SetNPCUser(null);
            }
            SetProperty(typeof(StartCauldronBehaviour), "WorkInProgress", __instance, false);
            return false;
        }




        public static new void RestoreDefaults()
        {
            try
            {
                foreach (var station in Mod.processedStationTimes)
                {
                    Cauldron cauldron = CastTo<Cauldron>(station);
                    if (cauldron != null)
                    {
                        if (!Mod.originalStationTimes.ContainsKey("Cauldron"))
                        {
                            cauldron.CookTime = 360;
                            cauldron.RemainingCookTime = Mathf.Min(360, cauldron.RemainingCookTime);
                        }
                        else
                        {
                            cauldron.CookTime = Mathf.Min(Mod.originalStationTimes["Cauldron"], cauldron.CookTime);
                            cauldron.RemainingCookTime = Mathf.Min(Mod.originalStationTimes["Cauldron"], cauldron.RemainingCookTime);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Warn($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
                Warn($"Source: {e.Source}");
                Warn($"{e.StackTrace}");
                return;
            }
        }
    }


    // packaging station patches
    [HarmonyPatch]
    public class PackagingStationPatches : Sched1PatchesBase
    {
        // call our own packaging coroutine to reduce animation time
        [HarmonyPatch(typeof(PackagingStationBehaviour), "RpcLogic___BeginPackaging_2166136261")]
        [HarmonyPrefix]
        public static bool RpcLogic_BeginPackagingPrefix(PackagingStationBehaviour __instance)
        {
            if (__instance.PackagingInProgress || __instance.Station == null)
            {
                return false;
            }

            SetProperty(typeof(PackagingStationBehaviour), "PackagingInProgress", __instance, true);
            __instance.Npc.Movement.FaceDirection(__instance.Station.StandPoint.forward, 0.5f);
            object packagingCoroutine = MelonCoroutines.Start(BeginPackagingCoroutine(__instance));
            SetField(typeof(PackagingStationBehaviour), "packagingRoutine", __instance, (Coroutine)packagingCoroutine);

            return false;
        }

        // replacement coroutine to accelerate animation
        private static IEnumerator BeginPackagingCoroutine(PackagingStationBehaviour behaviour)
        {
            yield return new WaitForEndOfFrame();
            behaviour.Npc.Avatar.Anim.SetBool("UsePackagingStation", true);

            ProductItemInstance packagedInstance = CastTo<ProductItemInstance>(behaviour.Station.ProductSlot.ItemInstance.GetCopy(1));
            packagedInstance.SetPackaging(CastTo<PackagingDefinition>(behaviour.Station.PackagingSlot.ItemInstance.Definition));
            int outputSpace = behaviour.Station.OutputSlot.GetCapacityForItem(packagedInstance);
            int productPerPackage = CastTo<PackagingDefinition>(behaviour.Station.PackagingSlot.ItemInstance.Definition).Quantity;
            int availableToPackage = Mathf.Min(behaviour.Station.ProductSlot.Quantity, behaviour.Station.PackagingSlot.Quantity);
            int numPackages = Mathf.Min(availableToPackage / productPerPackage, outputSpace);
            float packageTime = (float)numPackages * 5f / (CastTo<Packager>(behaviour.Npc).PackagingSpeedMultiplier * behaviour.Station.PackagerEmployeeSpeedMultiplier * Mod.settings.stationSpeeds["PackagingStation"]);

            //Log($"Have {behaviour.Station.ProductSlot.Quantity} product in input slot, {behaviour.Station.PackagingSlot.Quantity} {behaviour.Station.PackagingSlot.ItemInstance.Definition.Name} in packaging slot, and {behaviour.Station.OutputSlot.Quantity} packaged product in the output.");
            //Log($"Have {outputSpace} output space, {availableToPackage} product to package at {productPerPackage} product per package. Will make {numPackages} packages in {packageTime} seconds.");

            for (float i = 0f; i < packageTime; i++)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Station.Container.position, 0, false);
                yield return new WaitForSeconds(1f);
            }

            if (InstanceFinder.IsServer)
            {
                float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("PackagedProductCount");
                NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("PackagedProductCount", (value + numPackages * productPerPackage).ToString(), true);
                if (behaviour.Station.OutputSlot.ItemInstance == null)
                {
                    behaviour.Station.OutputSlot.SetStoredItem(packagedInstance, false);
                    behaviour.Station.OutputSlot.SetQuantity(numPackages);
                }
                else
                {
                    behaviour.Station.OutputSlot.ChangeQuantity(numPackages, false);
                    packagedInstance.SetQuantity(0);
                }
                behaviour.Station.PackagingSlot.ChangeQuantity(-numPackages, false);
                behaviour.Station.ProductSlot.ChangeQuantity(-numPackages * productPerPackage, false);
            }

            behaviour.Npc.Avatar.Anim.SetBool("UsePackagingStation", false);
            SetProperty(typeof(PackagingStationBehaviour), "PackagingInProgress", behaviour, false);
            SetField(typeof(PackagingStationBehaviour), "packagingRoutine", behaviour, null);
            yield break;
        }

        // stop our coroutine cleanly
        [HarmonyPatch(typeof(PackagingStationBehaviour), "StopPackaging")]
        [HarmonyPrefix]
        public static bool StopPackagingPrefix(PackagingStationBehaviour __instance)
        {
            object workRoutine = GetField(typeof(PackagingStationBehaviour), "packagingRoutine", __instance);
            if (workRoutine != null)
            {
                MelonCoroutines.Stop(workRoutine);
                SetField(typeof(PackagingStationBehaviour), "packagingRoutine", __instance, null);
            }
            __instance.Npc.Avatar.Anim.SetBool("UsePackagingStation", false);
            if (InstanceFinder.IsServer && __instance.Station != null && __instance.Station.NPCUserObject == __instance.Npc.NetworkObject)
            {
                __instance.Station.SetNPCUser(null);
            }
            SetProperty(typeof(PackagingStationBehaviour), "PackagingInProgress", __instance, false);
            return false;
        }

        public static new void RestoreDefaults()
        {
            // no game objects modified
        }

    }


    // pot patches
    [HarmonyPatch]
    public class PotPatches : Sched1PatchesBase
    {
        // speed
        [HarmonyPatch(typeof(Pot), "GetAdditiveGrowthMultiplier")]
        [HarmonyPostfix]
        public static void GetAdditiveGrowthMultiplierPostfix(ref float __result)
        {
            __result = __result * Mod.settings.GetStationSpeed("Pot");
        }

        // use our own coroutine to speed up employee animation
        [HarmonyPatch(typeof(PotActionBehaviour), "PerformAction")]
        [HarmonyPrefix]
        public static bool PerformActionPrefix(PotActionBehaviour __instance)
        {
            if (CastTo<Botanist>(GetField(typeof(PotActionBehaviour), "botanist", __instance)).DEBUG)
            {
                string str = "PotActionBehaviour.PerformAction: Performing action ";
                string str2 = __instance.CurrentActionType.ToString();
                string str3 = " on pot ";
                Pot assignedPot = __instance.AssignedPot;
                Debug.Log(str + str2 + str3 + ((assignedPot != null) ? assignedPot.ToString() : null));
            }
            
            SetProperty(typeof(PotActionBehaviour), "CurrentState", __instance, PotActionBehaviour.EState.PerformingAction);
            object workRoutine = MelonCoroutines.Start(PerformActionCoroutine(__instance));
            SetField(typeof(PotActionBehaviour), "performActionRoutine", __instance, (Coroutine)workRoutine);
            return false;
        }

        // coroutine with accelerated animations
        private static IEnumerator PerformActionCoroutine(PotActionBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("Pot") : 1f;

            behaviour.AssignedPot.SetNPCUser(CastTo<Botanist>(GetField(typeof(PotActionBehaviour), "botanist", behaviour)).NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.AssignedPot.transform.position, 0.5f);
            
            string actionAnimation = (string)CallMethod(typeof(PotActionBehaviour), "GetActionAnimation", behaviour, [behaviour.CurrentActionType]);
            if (actionAnimation != string.Empty)
            {
                SetField(typeof(PotActionBehaviour), "currentActionAnimation", behaviour, actionAnimation);
                behaviour.Npc.SetAnimationBool_Networked(null, actionAnimation, true);
            }
            if (behaviour.CurrentActionType == PotActionBehaviour.EActionType.SowSeed && !behaviour.Npc.Avatar.Anim.IsCrouched)
            {
                behaviour.Npc.SetCrouched_Networked(true);
            }

            AvatarEquippable actionEquippable = CastTo<AvatarEquippable>(CallMethod(typeof(PotActionBehaviour), "GetActionEquippable", behaviour, [behaviour.CurrentActionType]));
            if (actionEquippable != null)
            {
                SetField(typeof(PotActionBehaviour), "currentActionEquippable", behaviour, behaviour.Npc.SetEquippable_Networked_Return(null, actionEquippable.AssetPath));
            }
            
            float waitTime = behaviour.GetWaitTime(behaviour.CurrentActionType) / stationSpeed;
            for (float i = 0f; i < waitTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.AssignedPot.transform.position, 0, false);
                yield return new WaitForEndOfFrame();
            }
            
            CallMethod(typeof(PotActionBehaviour), "StopPerformAction", behaviour, []);
            CallMethod(typeof(PotActionBehaviour), "CompleteAction", behaviour, []);
            yield break;
        }

        [HarmonyPatch(typeof(PotActionBehaviour), "StopPerformAction")]
        [HarmonyPrefix]
        public static bool StopPerformActionPrefix(PotActionBehaviour __instance)
        {
            if (__instance.CurrentActionType == PotActionBehaviour.EActionType.SowSeed && __instance.Npc.Avatar.Anim.IsCrouched)
            {
                __instance.Npc.SetCrouched_Networked(false);
            }
            SetProperty(typeof(PotActionBehaviour), "CurrentState", __instance, PotActionBehaviour.EState.Idle);

            object workRoutine = GetField(typeof(PotActionBehaviour), "performActionRoutine", __instance);
            if (workRoutine != null)
            {
                MelonCoroutines.Stop(workRoutine);
                SetField(typeof(PotActionBehaviour), "performActionRoutine", __instance, null);
            }

            AvatarEquippable currentActionEquippable = CastTo<AvatarEquippable>(GetField(typeof(PotActionBehaviour), "currentActionEquippable", __instance));
            if (currentActionEquippable != null)
            {
                __instance.Npc.SetEquippable_Networked(null, string.Empty);
                SetField(typeof(PotActionBehaviour), "currentActionEquippable", __instance, null);
            }

            string currentActionAnimation = CastTo<string>(GetField(typeof(PotActionBehaviour), "currentActionAnimation", __instance));
            if (currentActionAnimation != string.Empty)
            {
                __instance.Npc.SetAnimationBool_Networked(null, currentActionAnimation, false);
                SetField(typeof(PotActionBehaviour), "currentActionAnimation", __instance, string.Empty);
            }

            Botanist botanist = CastTo<Botanist>(GetField(typeof(PotActionBehaviour), "botanist", __instance));
            if (__instance.AssignedPot != null && __instance.AssignedPot.NPCUserObject == botanist.NetworkObject)
            {
                __instance.AssignedPot.SetNPCUser(null);
            }
            return false;
        }


        [HarmonyPatch(typeof(PotActionBehaviour), "ActiveMinPass")]
        [HarmonyPrefix]
        public static bool ActiveMinPassPrefix(PotActionBehaviour __instance)
        {
            if (!InstanceFinder.IsServer)
                {
                    return false;
                }
                if (__instance.Npc.behaviour.DEBUG_MODE)
                {
                    Debug.Log("Current state: " + __instance.CurrentState.ToString(), null);
                    Debug.Log("Is walking: " + __instance.Npc.Movement.IsMoving.ToString(), null);
                }
            if (__instance.CurrentState == PotActionBehaviour.EState.Idle)
            {

#if MONO_BUILD
                string[] requiredItemIDs = CastTo<string[]>(CallMethod(typeof(PotActionBehaviour), "GetRequiredItemIDs", __instance, []));
#else
                Il2CppStringArray requiredItemIDs = CastTo<Il2CppStringArray>(CallMethod(typeof(PotActionBehaviour), "GetRequiredItemIDs", __instance, []));
#endif
                if (!__instance.DoesTaskTypeRequireSupplies(__instance.CurrentActionType) || __instance.Npc.Inventory.GetMaxItemCount(requiredItemIDs) > 0)
                {
                    if ((bool)CallMethod(typeof(PotActionBehaviour), "IsAtPot", __instance, []))
                    {
                        __instance.PerformAction();
                        return false;
                    }

                    __instance.WalkToPot();
                    return false;
                }
                else
                {
                    if (__instance.AssignedPot == null)
                    {
                        string str = "PotActionBehaviour.ActiveMinPass: No pot assigned for botanist ";
                        Botanist botanist = CastTo<Botanist>(GetField(typeof(PotActionBehaviour), "botanist", __instance));
                        Debug.LogWarning(str + ((botanist != null) ? botanist.ToString() : null), null);
                        __instance.Disable_Networked(null);
                        return false;
                    }
                    if ((bool)CallMethod(typeof(PotActionBehaviour), "IsAtSupplies", __instance, []))
                    {
                        Botanist botanist = CastTo<Botanist>(GetField(typeof(PotActionBehaviour), "botanist", __instance));
                        if (__instance.DoesBotanistHaveMaterialsForTask(botanist, __instance.AssignedPot, __instance.CurrentActionType, __instance.AdditiveNumber))
                        {
                            __instance.GrabItem();
                            return false;
                        }

                        CallMethod(typeof(PotActionBehaviour), "StopPerformAction", __instance, []);
                        __instance.Disable_Networked(null);
                        return false;
                    }
                    else
                    {
                        __instance.WalkToSupplies();
                    }
                }
            }
            return false;
        }

        public static new void RestoreDefaults()
        {
            // no need to restore anything, since we never modified any game objects
        }
    }


    // chemistry station patches
    [HarmonyPatch]
    public class ChemistryStationPatches : Sched1PatchesBase
    {
        [HarmonyPatch(typeof(StationRecipeEntry), "AssignRecipe")]
        [HarmonyPostfix]
        public static void AssignRecipePostfix(StationRecipeEntry __instance, ref StationRecipe recipe)
        {
            if (!Mod.processedRecipes.Contains(recipe))
            {
                if (!Mod.originalRecipeTimes.ContainsKey(recipe))
                {
                    Mod.originalRecipeTimes[recipe] = __instance.Recipe.CookTime_Mins;
                }
                __instance.Recipe.CookTime_Mins = (int)((float)Mod.originalRecipeTimes[recipe] / Mod.settings.GetStationSpeed("ChemistryStation"));
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


        // use our own work coroutine to speed up animations
        [HarmonyPatch(typeof(StartChemistryStationBehaviour), "RpcLogic___StartCook_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_StartCookPrefix(StartChemistryStationBehaviour __instance)
        {
            if (GetField(typeof(StartChemistryStationBehaviour), "cookRoutine", __instance) != null)
            {
                return false;
            }
            if (__instance.targetStation == null)
            {
                return false;
            }
            object workRoutine = MelonCoroutines.Start(StartCookRoutine(__instance));
            SetField(typeof(StartChemistryStationBehaviour), "cookRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        // Coroutine with accelerated animations
        private static IEnumerator StartCookRoutine(StartChemistryStationBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("ChemistryStation") : 1f;
            behaviour.Npc.Movement.FacePoint(behaviour.targetStation.transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f);

            behaviour.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", true);
            if (!(bool)CallMethod(typeof(StartChemistryStationBehaviour), "CanCookStart", behaviour, []))
            {
                CallMethod(typeof(StartChemistryStationBehaviour), "StopCook", behaviour, []);
                behaviour.End_Networked(null);
                yield break;
            }

            behaviour.targetStation.SetNPCUser(behaviour.Npc.NetworkObject);
            StationRecipe recipe = (CastTo<ChemistryStationConfiguration>(behaviour.targetStation.Configuration)).Recipe.SelectedRecipe;
            CallMethod(typeof(StartChemistryStationBehaviour), "SetupBeaker", behaviour, []);
            yield return new WaitForSeconds(1f / stationSpeed);
            
            Beaker beaker = CastTo<Beaker>(GetField(typeof(StartChemistryStationBehaviour), "beaker", behaviour));
            CallMethod(typeof(StartChemistryStationBehaviour), "FillBeaker", behaviour, [recipe, beaker]);
            yield return new WaitForSeconds(20f / stationSpeed);

#if MONO_BUILD
            var list = new List<ItemInstance>();
#else
            var list = new Il2CppSystem.Collections.Generic.List<ItemInstance>();
#endif
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                foreach (ItemDefinition itemDefinition in recipe.Ingredients[i].Items)
                {
                    StorableItemDefinition storableItemDefinition = CastTo<StorableItemDefinition>(itemDefinition);
                    for (int j = 0; j < behaviour.targetStation.IngredientSlots.Length; j++)
                    {
                        if (behaviour.targetStation.IngredientSlots[j].ItemInstance != null && behaviour.targetStation.IngredientSlots[j].ItemInstance.Definition.ID == storableItemDefinition.ID)
                        {
                            list.Add(behaviour.targetStation.IngredientSlots[j].ItemInstance.GetCopy(recipe.Ingredients[i].Quantity));
                            behaviour.targetStation.IngredientSlots[j].ChangeQuantity(-recipe.Ingredients[i].Quantity, false);
                            break;
                        }
                    }
                }
            }
            EQuality productQuality = recipe.CalculateQuality(list);
            behaviour.targetStation.SendCookOperation(new ChemistryCookOperation(recipe, productQuality, beaker.Container.LiquidColor, beaker.Fillable.LiquidContainer.CurrentLiquidLevel, 0));

            beaker.Destroy();
            SetField(typeof(StartChemistryStationBehaviour), "beaker", behaviour, null);
            CallMethod(typeof(StartChemistryStationBehaviour), "StopCook", behaviour, []);
            behaviour.End_Networked(null);
            yield break;

        }


        [HarmonyPatch(typeof(StartChemistryStationBehaviour), "StopCook")]
        [HarmonyPrefix]
        public static bool StopCookPrefix(StartChemistryStationBehaviour __instance)
        {
            __instance.targetStation.SetNPCUser(null);
            __instance.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", false);
            object workRoutine = GetField(typeof(StartChemistryStationBehaviour), "cookRoutine", __instance);
            if (workRoutine != null)
            {
                MelonCoroutines.Stop(workRoutine);
                SetField(typeof(StartChemistryStationBehaviour), "cookRoutine", __instance, null);
            }

            return false;
        }

        public static new void RestoreDefaults()
        {
            try
            {
                foreach (var recipe in Mod.processedRecipes)
                {
                    if (!Mod.originalRecipeTimes.ContainsKey(recipe))
                    {
                        recipe.CookTime_Mins = 480;
                    }
                    else
                    {
                        recipe.CookTime_Mins = Mod.originalRecipeTimes[recipe];
                    }
                }
            }
            catch (Exception e)
            {
                Warn($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
                Warn($"Source: {e.Source}");
                Warn($"{e.StackTrace}");
                return;
            }
        }
    }


    // cash patches
    [HarmonyPatch]
    public class CashPatches : Sched1PatchesBase
    {
#if MONO_BUILD

        public static float GetCashStackLimit()
        {
            return Mod.settings.GetStackLimit(EItemCategory.Cash);
        }

        // Specify what methods the transpiler patch should apply to
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.DeclaredMethod(typeof(ItemUIManager), "UpdateCashDragAmount");
            yield return AccessTools.DeclaredMethod(typeof(ItemUIManager), "StartDragCash");
            yield return AccessTools.DeclaredMethod(typeof(ItemUIManager), "EndCashDrag");
        }

        // Replace all instances of "1000" with our custom stacklimit
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Mod field isn't set yet by the time this executes, so use MelonLogger instead of Mod.LoggerInstance
            //MelonLogger.Msg($"Transpiler for CashPatches started");
            //MelonLogger.Msg("Instruction dump:");
            //foreach (var instruction in instructions) { MelonLogger.Msg($"{instruction.opcode} {instruction.operand}"); }

            // Get GetCashStackLimit's methodinfo so we can call it with the "call" opcode
            MethodInfo getCashStackLimitInfo = AccessTools.Method(typeof(CashPatches), nameof(GetCashStackLimit));

            // Locate instances of "ldc.r4 1000" and replace them with calls to GetCashStackLimit
            CodeMatcher matcher = new(instructions, generator);
            try
            {
                while (true)
                {
                    matcher.MatchEndForward(
                        new CodeMatch(OpCodes.Ldc_R4, 1000f)
                    ).ThrowIfNotMatch("Couldn't find ldc.r4 1000f") 
                    .RemoveInstruction()
                    .InsertAndAdvance(
                        // since GetCashStackLimit is static with no params, we can just call it
                        // otherwise, we'd have to push this and/or params onto the stack first
                        new CodeInstruction(OpCodes.Call, getCashStackLimitInfo)
                        // return value is stored on the stack, in the exact location that "ldc.r4 1000"
                        // would have left it. no need to move it or balance the stack.
                    );
                    //MelonLogger.Msg("replaced instance of ldc.r4 1000f\n");
                } 
            }
            catch (InvalidOperationException e)
            {
                // this is the expected way to exit the loop
                //MelonLogger.Msg("Replaced all \"Ldc.r4 1000\" with calls to GetCashStackLimit()");
            }
            catch (Exception e)
            {
                Warn($"Failed to patch method: {e.GetType().Name} - {e.Message}");
                Warn($"Source: {e.Source}");
                Warn($"{e.StackTrace}");
            }

            IEnumerable<CodeInstruction> modifiedIL = matcher.InstructionEnumeration();

            //MelonLogger.Msg("\nModified instruction dump:");
            //foreach (var instruction in modifiedIL) { MelonLogger.Msg($"{instruction.opcode} {instruction.operand}"); }

            return modifiedIL;
        }

#else

        // harmony transpilers only deal with IL, so we can't apply one to IL2CPP binary
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

            CashInstance cashInstance = CastTo<CashInstance>(__instance.draggedSlot.assignedSlot.ItemInstance);
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
                            CashInstance cashInstance2 = CastTo<CashInstance>(itemSlot.ItemInstance);
                            if (cashInstance2 != null)
                            {
                                float num3;
                                if (Is<CashSlot>(itemSlot))
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
                            CashInstance cashInstance3 = CastTo<CashInstance>(Registry.GetItem("cash").GetDefaultInstance(1));
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
            CastTo<ItemUI_Cash>(__instance.draggedSlot.ItemUI).SetDisplayedBalance(cashInstance.Balance - __instance.draggedCashAmount);
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
            if (__instance.CanDragFromSlot(__instance.draggedSlot) && __instance.HoveredSlot != null && __instance.CanCashBeDraggedIntoSlot(__instance.HoveredSlot) && !__instance.HoveredSlot.assignedSlot.IsLocked && !__instance.HoveredSlot.assignedSlot.IsAddLocked && __instance.HoveredSlot.assignedSlot.DoesItemMatchHardFilters(__instance.draggedSlot.assignedSlot.ItemInstance))
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
                        CashInstance cashInstance2 = __instance.HoveredSlot.assignedSlot.ItemInstance.TryCast<CashInstance>();
                        if (Is<CashSlot>(__instance.HoveredSlot.assignedSlot))
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
        public static new void RestoreDefaults()
        {
            // nothing to restore, no objects were modified
        }
    }

    
    [HarmonyPatch]
    public class NPCMovementPatches : Sched1PatchesBase
    {
        // apply speed multiplier if NPC is an employee
        [HarmonyPatch(typeof(NPCMovement), "UpdateSpeed")]
        [HarmonyPrefix]
        public static bool UpdateSpeedPrefix(NPCMovement __instance)
        {
            float walkAcceleration = 1f;
            NPC npc = CastTo<NPC>(GetField(typeof(NPCMovement), "npc", __instance));
            if (Is<Employee>(npc))
            {
                walkAcceleration = Mod.settings.employeeWalkAcceleration;
            }
            if ((double)__instance.MovementSpeedScale >= 0.0)
            {
                __instance.Agent.speed = Mathf.Lerp(__instance.WalkSpeed * walkAcceleration, __instance.RunSpeed, __instance.MovementSpeedScale) * __instance.MoveSpeedMultiplier;
                return false;
            }
            __instance.Agent.speed = 0f;

            return false;
        }

#if !MONO_BUILD
        // call to updatespeed seems to have been optimized out.
        [HarmonyPatch(typeof(NPCMovement), "FixedUpdate")]
        [HarmonyPrefix]
        public static bool FixedUpdatePrefix(NPCMovement __instance)
        {
            // re-insert original method body.
            if (!InstanceFinder.IsServer)
            {
                return false;
            }
            if (__instance.IsPaused)
            {
                __instance.Agent.isStopped = true;
            }
            float timeSinceHitByCar = (float)GetProperty(typeof(NPCMovement), "timeSinceHitByCar", __instance);
            SetProperty(typeof(NPCMovement), "timeSinceHitByCar", __instance, timeSinceHitByCar + Time.fixedDeltaTime);
            __instance.capsuleCollider.transform.position = CastTo<Rigidbody>(GetField(typeof(NPCMovement), "ragdollCentralRB", __instance)).transform.position;
            CallMethod(typeof(NPCMovement), "UpdateSpeed", __instance, []);
            CallMethod(typeof(NPCMovement), "UpdateStumble", __instance, []);
            CallMethod(typeof(NPCMovement), "UpdateRagdoll", __instance, []);
            CallMethod(typeof(NPCMovement), "UpdateDestination", __instance, []);
            CallMethod(typeof(NPCMovement), "RecordVelocity", __instance, []);
            CallMethod(typeof(NPCMovement), "UpdateSlippery", __instance, []);
            CallMethod(typeof(NPCMovement), "UpdateCache", __instance, []);

            if (!(CastTo<NPCAnimation>(GetProperty(typeof(NPCMovement), "anim", __instance)).Avatar.Ragdolled || !__instance.CanRecoverFromRagdoll()))
            {
                SetProperty(typeof(NPCMovement), "ragdollStaticTime", __instance, 0f);
                return false;
            }
            float ragdollTime = (float)GetProperty(typeof(NPCMovement), "ragdollTime", __instance);
            SetProperty(typeof(NPCMovement), "ragdollTime", __instance, ragdollTime + Time.fixedDeltaTime);
            float ragdollStaticTime = (float)GetProperty(typeof(NPCMovement), "ragdollStaticTime", __instance);
            if (CastTo<Rigidbody>(GetProperty(typeof(NPCMovement), "ragdollCentralRB", __instance)).velocity.magnitude < 0.25f)
            {
                SetProperty(typeof(NPCMovement), "ragdollStaticTime", __instance, ragdollStaticTime + Time.fixedDeltaTime);
                return false;
            }
            SetProperty(typeof(NPCMovement), "ragdollStaticTime", __instance, 0f);

            return false;
        }
#endif

        public static new void RestoreDefaults()
        {
            // no game objects were changed, so we don't need to do anything
        }

    }
}
