using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Reflection.Emit;
using MelonLoader;

using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using System.Collections;


#if MONO_BUILD
using FishNet;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.Product;
using ScheduleOne.UI.Shop;
using ScheduleOne.UI.Stations.Drying_rack;
using ScheduleOne.UI.Stations;
using ScheduleOne.Variables;
using ScheduleOne;
#else
using Il2CppFishNet;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppScheduleOne.AvatarFramework.Equipping;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.StationFramework;
using Il2CppScheduleOne.UI.Items;
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
        public static void StackLimitPostfix(ItemInstance __instance)
        {
            if (!Mod.processedItemDefs.Contains(__instance.Definition) && __instance.Definition.Name.ToLower() != "cash")
            {
                // Speed Grow is classified as product for some reason
                EItemCategory category;
                if (__instance.Definition.Name == "Speed Grow")
                {
                    category = EItemCategory.Growing;
                }
                else
                {
                    category = __instance.Definition.Category;
                }

                if (!Mod.originalStackLimits.ContainsKey(category))
                {
                    Mod.originalStackLimits[category] = __instance.Definition.StackLimit; 
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
                        category = EItemCategory.Growing;
                    }
                    else
                    {
                        category = match.Item.Category;
                    }

                    if (!Mod.originalStackLimits.ContainsKey(category))
                    {
                        Mod.originalStackLimits[category] = match.Item.StackLimit;
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
        public static void GetCapacityForItemPostfix(ItemSlot __instance, ref int __result, ItemInstance item)
        {
            if (!__instance.DoesItemMatchFilters(item))
            {
                __result = 0;
                return;
            }
            if (__instance.ItemInstance == null || __instance.ItemInstance.CanStackWith(item, false))
            {
                //__result = item.StackLimit - __instance.Quantity;
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
                        if (!Mod.originalStackLimits.ContainsKey(itemDef.Category))
                        {
                            itemDef.StackLimit = new ModSettings().GetStackLimit(itemDef);
                        }
                        else
                        {
                            itemDef.StackLimit = (int)Mod.originalStackLimits[itemDef.Category];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
                Mod.LoggerInstance.Warning($"{e.StackTrace}");
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
                    category = EItemCategory.Growing;
                }
                else
                {
                    category = __result.Category;
                }

                if (!Mod.originalStackLimits.ContainsKey(category))
                {
                    Mod.originalStackLimits[category] = __result.StackLimit; 
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

            __result = __instance.GetTotalDryingItems() < __instance.ItemCapacity && __instance.InputSlot.Quantity != 0 && !__instance.InputSlot.IsLocked && !__instance.InputSlot.IsRemovalLocked;

            return false;
        }


        // fix drying operation progress meter
        [HarmonyPatch(typeof(DryingOperationUI), "UpdatePosition")]
        [HarmonyPrefix]
        public static bool UpdatePositionPrefix(DryingOperationUI __instance)
        {
            if (!Mod.originalStationTimes.ContainsKey("DryingRack"))
            {
                Mod.originalStationTimes["DryingRack"] = 720; //replace with constant from class when there is one accessible
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
                Mod.originalStationTimes["DryingRack"] = 720; //replace with constant from class once one is available
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
                Mod.originalStationTimes["DryingRack"] = 720; //replace with constant from class once one is available
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

            AccessTools.PropertySetter(typeof(StartDryingRackBehaviour), "WorkInProgress").Invoke(__instance, [true]);
            __instance.Npc.Movement.FacePoint(__instance.Rack.uiPoint.position, 0.5f);
            object workCoroutine = MelonCoroutines.Start(BeginActionCoroutine(__instance));
            AccessTools.Field(typeof(StartDryingRackBehaviour), "workRoutine").SetValue(__instance, (Coroutine)workCoroutine);
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
            AccessTools.PropertySetter(typeof(StartDryingRackBehaviour), "WorkInProgress").Invoke(behaviour, [false]);
            AccessTools.Field(typeof(StartDryingRackBehaviour), "workRoutine").SetValue(behaviour, null);
            yield break;
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
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
                Mod.LoggerInstance.Warning($"{e.StackTrace}");
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
            // TODO: clean this up
            if (!__instance.Oven.OutputSlot.DoesItemMatchFilters(productInstance))
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
                FieldInfo cookRoutine = AccessTools.Field(typeof(StartLabOvenBehaviour), "cookRoutine");
                if (cookRoutine.GetValue(__instance) != null || __instance.targetOven == null)
                {
                    return false;
                }
                object workRoutine = MelonCoroutines.Start(StartCookCoroutine(__instance));
                cookRoutine.SetValue(__instance, (Coroutine)workRoutine);

            }
            catch (Exception e)
            {
                MelonLogger.Warning($"Failed to set cookroutine: {e.GetType().Name} - {e.Message}");
                MelonLogger.Warning($"Source: {e.Source}");
                MelonLogger.Warning($"{e.StackTrace}");
            }

            return false;
        }

        // Startcook coroutine with accelerated animations
        private static IEnumerator StartCookCoroutine(StartLabOvenBehaviour behaviour)
        {
            Mod.LoggerInstance.Msg($"In StartCookRoutine");
            Mod.LoggerInstance.Msg($"look at oven");
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("LabOven") : 1f;
            behaviour.targetOven.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.targetOven.transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f / stationSpeed);

            if (!(bool)AccessTools.Method(typeof(StartLabOvenBehaviour), "CanCookStart").Invoke(behaviour, []))
            {
                AccessTools.Method(typeof(StartLabOvenBehaviour), "StopCook").Invoke(behaviour, []);
                behaviour.End_Networked(null);
                Mod.LoggerInstance.Msg($"can't start cook; breaking");
                yield break;
            }

            Mod.LoggerInstance.Msg($"door closed");
            behaviour.targetOven.Door.SetPosition(1f / stationSpeed);
            yield return new WaitForSeconds(0.5f / stationSpeed);

            Mod.LoggerInstance.Msg($"get out tray");
            behaviour.targetOven.WireTray.SetPosition(1f / stationSpeed);
            yield return new WaitForSeconds(5f / stationSpeed);

            Mod.LoggerInstance.Msg($"opening door");
            behaviour.targetOven.Door.SetPosition(0f);
            yield return new WaitForSeconds(1f / stationSpeed);

            ItemInstance itemInstance = behaviour.targetOven.IngredientSlot.ItemInstance;
            if (itemInstance == null)
            {
                Mod.LoggerInstance.Msg($"no item in slot; stopping");
                AccessTools.Method(typeof(StartLabOvenBehaviour), "StopCook").Invoke(behaviour, []);
                behaviour.End_Networked(null);
                yield break;
            }

            Mod.LoggerInstance.Msg($"moving ingredients to tray");
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
            AccessTools.Method(typeof(StartLabOvenBehaviour), "StopCook").Invoke(behaviour, []);
            behaviour.End_Networked(null);
            yield break;
        }



        // Call our own finishcook coroutine with accelerated animations
        [HarmonyPatch(typeof(FinishLabOvenBehaviour), "RpcLogic___StartAction_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_StartFinishCookPrefix(FinishLabOvenBehaviour __instance)
        {
            if (AccessTools.Field(typeof(FinishLabOvenBehaviour), "actionRoutine").GetValue(__instance) != null)
            {
                return false;
            }
            if (__instance.targetOven == null)
            {
                return false;
            }
            object workRoutine = MelonCoroutines.Start(FinishCookCoroutine(__instance));
            AccessTools.Field(typeof(FinishLabOvenBehaviour), "actionRoutine").SetValue(__instance, (Coroutine)workRoutine);

            return false;
        }

        // FinishCook coroutine with accelerated animations
        private static IEnumerator FinishCookCoroutine(FinishLabOvenBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("LabOven") : 1f;
            behaviour.targetOven.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.targetOven.transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f);

            if (!(bool)AccessTools.Method(typeof(FinishLabOvenBehaviour), "CanActionStart").Invoke(behaviour, []))
            {
                AccessTools.Method(typeof(FinishLabOvenBehaviour), "StopAction").Invoke(behaviour, []);
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
            AccessTools.Method(typeof(FinishLabOvenBehaviour), "StopAction").Invoke(behaviour, []);
            behaviour.End_Networked(null);
            yield break;
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
                    AccessTools.PropertySetter(typeof(MixingStation), nameof(MixingStation.CurrentMixTime)).Invoke(__instance, [currentMixTime2 + 1]);
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
            if (AccessTools.Field(typeof(StartMixingStationBehaviour), "startRoutine").GetValue(__instance) != null)
            {
                return false;
            }
            if (__instance.targetStation == null)
            {
                return false;
            }
            object workRoutine = MelonCoroutines.Start(StartMixCoroutine(__instance));
            AccessTools.Field(typeof(StartMixingStationBehaviour), "startRoutine").SetValue(__instance, (Coroutine)workRoutine);

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

            if (!(bool)AccessTools.Method(typeof(StartMixingStationBehaviour), "CanCookStart").Invoke(behaviour, []))
            {
                AccessTools.Method(typeof(StartMixingStationBehaviour), "StopCook").Invoke(behaviour, []);
                behaviour.End_Networked(null);
                yield break;
            }

            behaviour.targetStation.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", true);
            QualityItemInstance product = CastTo<QualityItemInstance>(behaviour.targetStation.ProductSlot.ItemInstance);
            ItemInstance mixer = behaviour.targetStation.MixerSlot.ItemInstance;
            int mixQuantity = behaviour.targetStation.GetMixQuantity();
            int num;
            for (int i = 0; i < mixQuantity; i = num + 1)
            {
                yield return new WaitForSeconds(1f / stationSpeed);
                num = i;
            }

            if (InstanceFinder.IsServer)
            {
                behaviour.targetStation.ProductSlot.ChangeQuantity(-mixQuantity, false);
                behaviour.targetStation.MixerSlot.ChangeQuantity(-mixQuantity, false);
                MixOperation operation = new MixOperation(product.ID, product.Quality, mixer.ID, mixQuantity);
                behaviour.targetStation.SendMixingOperation(operation, 0);
            }
            AccessTools.Method(typeof(StartMixingStationBehaviour), "StopCook").Invoke(behaviour, []);
            behaviour.End_Networked(null);
            yield break;
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
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
                Mod.LoggerInstance.Warning($"{e.StackTrace}");
                return;
            }
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
            AccessTools.PropertySetter(typeof(BrickPressBehaviour), "PackagingInProgress").Invoke(__instance, [true]);
            __instance.Npc.Movement.FaceDirection(__instance.Press.StandPoint.forward, 0.5f);
            object workRoutine = MelonCoroutines.Start(PackagingCoroutine(__instance));
            AccessTools.Field(typeof(BrickPressBehaviour), "packagingRoutine").SetValue(__instance, (Coroutine)workRoutine);

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
            AccessTools.PropertySetter(typeof(BrickPressBehaviour), "PackagingInProgress").Invoke(behaviour, [false]);
            AccessTools.Field(typeof(BrickPressBehaviour), "packagingRoutine").SetValue(behaviour, null);
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
            if ((bool)AccessTools.Property(typeof(Cauldron), "isCooking").GetValue(__instance))
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

            AccessTools.PropertySetter(typeof(StartCauldronBehaviour), "WorkInProgress").Invoke(__instance, [true]);
            __instance.Npc.Movement.FaceDirection(__instance.Station.StandPoint.forward, 0.5f);
            object workCoroutine = MelonCoroutines.Start(BeginCauldronCoroutine(__instance));
            AccessTools.Field(typeof(StartCauldronBehaviour), "workRoutine").SetValue(__instance, (Coroutine)workCoroutine);
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
            
            AccessTools.PropertySetter(typeof(StartCauldronBehaviour), "WorkInProgress").Invoke(behaviour, [false]);
            AccessTools.Field(typeof(StartCauldronBehaviour), "workRoutine").SetValue(behaviour, null);
            yield break;
        }

        // capacity takes care of itself

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
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
                Mod.LoggerInstance.Warning($"{e.StackTrace}");
                return;
            }
        }
    }


    // packaging station patches
    [HarmonyPatch]
    public class PackagingStationPatches : Sched1PatchesBase
    {
        // speed
        [HarmonyPatch(typeof(PackagingStationBehaviour), "BeginPackaging")]
        [HarmonyPrefix]
        public static void BeginPackagingPrefix(PackagingStationBehaviour __instance)
        {
            if (!Mod.processedStationSpeeds.Contains(__instance.Station))
            {
                float stationSpeed = Mod.settings.GetStationSpeed("PackagingStation");
                __instance.Station.PackagerEmployeeSpeedMultiplier = stationSpeed;
                Mod.processedStationSpeeds.Add(__instance.Station);
            }
        }

        // patch packaging coroutine to reduce animation time
        [HarmonyPatch(typeof(PackagingStationBehaviour), "RpcLogic___BeginPackaging_2166136261")]
        [HarmonyPrefix]
        public static bool RpcLogic_BeginPackagingPrefix(PackagingStationBehaviour __instance)
        {
            if (__instance.PackagingInProgress || __instance.Station == null)
            {
                return false;
            }
            
            AccessTools.PropertySetter(typeof(PackagingStationBehaviour), "PackagingInProgress").Invoke(__instance, [true]);
            __instance.Npc.Movement.FaceDirection(__instance.Station.StandPoint.forward, 0.5f / Mod.settings.GetStationSpeed("PackagingStation"));
            object packagingCoroutine = MelonCoroutines.Start(BeginPackagingCoroutine(__instance));
            AccessTools.Field(typeof(PackagingStationBehaviour), "packagingRoutine").SetValue(__instance, (Coroutine)packagingCoroutine);

            return false;
        }

        // replacement coroutine to accelerate animation
        private static IEnumerator BeginPackagingCoroutine(PackagingStationBehaviour behaviour)
        {
            yield return new WaitForEndOfFrame();
            behaviour.Npc.Avatar.Anim.SetBool("UsePackagingStation", true);
            float packageTime = 5f / (CastTo<Packager>(behaviour.Npc).PackagingSpeedMultiplier * behaviour.Station.PackagerEmployeeSpeedMultiplier);
            for (float i = 0f; i < packageTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Station.Container.position, 0, false);
                yield return new WaitForEndOfFrame();
            }
            behaviour.Npc.Avatar.Anim.SetBool("UsePackagingStation", false);
            if (InstanceFinder.IsServer)
            {
                behaviour.Station.PackSingleInstance();
            }

            AccessTools.PropertySetter(typeof(PackagingStationBehaviour), "PackagingInProgress").Invoke(behaviour, [false]);
            AccessTools.Field(typeof(PackagingStationBehaviour), "packagingRoutine").SetValue(behaviour, null);
            yield break;

        }

        // capacity takes care of itself

        public static new void RestoreDefaults()
        {
            try
            {
                foreach (var station in Mod.processedStationSpeeds)
                {
                    PackagingStation packagingStation = CastTo<PackagingStation>(station);
                    if (packagingStation != null)
                    {
                        packagingStation.PackagerEmployeeSpeedMultiplier = 1;
                    }
                }
            }
            catch (Exception e)
            {
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
                Mod.LoggerInstance.Warning($"{e.StackTrace}");
                return;
            }
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
            if (CastTo<Botanist>(AccessTools.Field(typeof(PotActionBehaviour), "botanist").GetValue(__instance)).DEBUG)
            {
                string str = "PotActionBehaviour.PerformAction: Performing action ";
                string str2 = __instance.CurrentActionType.ToString();
                string str3 = " on pot ";
                Pot assignedPot = __instance.AssignedPot;
                Debug.Log(str + str2 + str3 + ((assignedPot != null) ? assignedPot.ToString() : null));
            }
            
            AccessTools.PropertySetter(typeof(PotActionBehaviour), "CurrentState").Invoke(__instance, [PotActionBehaviour.EState.PerformingAction]);
            object workRoutine = MelonCoroutines.Start(PerformActionCoroutine(__instance));
            AccessTools.Field(typeof(PotActionBehaviour), "performActionRoutine").SetValue(__instance, (Coroutine)workRoutine);
            return false;
        }

        // coroutine with accelerated animations
        private static IEnumerator PerformActionCoroutine(PotActionBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("Pot") : 1f;

            behaviour.AssignedPot.SetNPCUser(CastTo<Botanist>(AccessTools.Field(typeof(PotActionBehaviour), "botanist").GetValue(behaviour)).NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.AssignedPot.transform.position, 0.5f);

            string actionAnimation = (string)AccessTools.Method(typeof(PotActionBehaviour), "GetActionAnimation").Invoke(behaviour, [behaviour.CurrentActionType]);
            if (actionAnimation != string.Empty)
            {
                AccessTools.Field(typeof(PotActionBehaviour), "currentActionAnimation").SetValue(behaviour, actionAnimation);
                behaviour.Npc.SetAnimationBool_Networked(null, actionAnimation, true);
            }
            if (behaviour.CurrentActionType == PotActionBehaviour.EActionType.SowSeed && !behaviour.Npc.Avatar.Anim.IsCrouched)
            {
                behaviour.Npc.SetCrouched_Networked(true);
            }

            AvatarEquippable actionEquippable = CastTo<AvatarEquippable>(AccessTools.Method(typeof(PotActionBehaviour), "GetActionEquippable").Invoke(behaviour, [behaviour.CurrentActionType]));
            if (actionEquippable != null)
            {
                AccessTools.Field(typeof(PotActionBehaviour), "currentActionEquippable").SetValue(behaviour, behaviour.Npc.SetEquippable_Networked_Return(null, actionEquippable.AssetPath));
            }
            
            float waitTime = behaviour.GetWaitTime(behaviour.CurrentActionType) / stationSpeed;
            for (float i = 0f; i < waitTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.AssignedPot.transform.position, 0, false);
                yield return new WaitForEndOfFrame();
            }
            
            AccessTools.Method(typeof(PotActionBehaviour), "StopPerformAction").Invoke(behaviour, []);
            AccessTools.Method(typeof(PotActionBehaviour), "CompleteAction").Invoke(behaviour, []);
            yield break;
        }


        [HarmonyPatch(typeof(PotActionBehaviour), "ActiveMinPass")]
        [HarmonyPrefix]
        public static bool ActiveMinPassPrefix(PotActionBehaviour __instance)
        {
//#if MONO_BUILD
//            AccessTools.Method(typeof(ScheduleOne.NPCs.Behaviour.Behaviour), "ActiveMinPass").Invoke(__instance, []);
//#else
//            AccessTools.Method(typeof(Il2CppScheduleOne.NPCs.Behaviour.Behaviour), "ActiveMinPass").Invoke(__instance, []);
//#endif

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
                string[] requiredItemIDs = CastTo<string[]>(AccessTools.Method(typeof(PotActionBehaviour), "GetRequiredItemIDs").Invoke(__instance, []));
#else
                Il2CppStringArray requiredItemIDs = CastTo<Il2CppStringArray>(AccessTools.Method(typeof(PotActionBehaviour), "GetRequiredItemIDs").Invoke(__instance, []));
#endif
                if (!__instance.DoesTaskTypeRequireSupplies(__instance.CurrentActionType) || __instance.Npc.Inventory.GetMaxItemCount(requiredItemIDs) > 0)
                {
                    if ((bool)AccessTools.Method(typeof(PotActionBehaviour), "IsAtPot").Invoke(__instance, []))
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
                        Botanist botanist = CastTo<Botanist>(AccessTools.Field(typeof(PotActionBehaviour), "botanist").GetValue(__instance));
                        Debug.LogWarning(str + ((botanist != null) ? botanist.ToString() : null), null);
                        __instance.Disable_Networked(null);
                        return false;
                    }
                    if ((bool)AccessTools.Method(typeof(PotActionBehaviour), "IsAtSupplies").Invoke(__instance, []))
                    {
                        Botanist botanist = CastTo<Botanist>(AccessTools.Field(typeof(PotActionBehaviour), "botanist").GetValue(__instance));
                        if (__instance.DoesBotanistHaveMaterialsForTask(botanist, __instance.AssignedPot, __instance.CurrentActionType, __instance.AdditiveNumber))
                        {
                            __instance.GrabItem();
                            return false;
                        }

                        AccessTools.Method(typeof(PotActionBehaviour), "StopPerformAction").Invoke(__instance, []);
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
            // no need to restore anything, since we never modified any objects
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
        public static bool Rpg_StartCookPrefix(StartChemistryStationBehaviour __instance)
        {
            if (AccessTools.Field(typeof(StartChemistryStationBehaviour), "cookRoutine").GetValue(__instance) != null)
            {
                return false;
            }
            if (__instance.targetStation == null)
            {
                return false;
            }
            object workRoutine = MelonCoroutines.Start(StartCookRoutine(__instance));
            AccessTools.Field(typeof(StartChemistryStationBehaviour), "cookRoutine").SetValue(__instance, (Coroutine)workRoutine);

            return false;
        }

        private static IEnumerator StartCookRoutine(StartChemistryStationBehaviour behaviour)
        {
            float stationSpeed = Mod.settings.enableStationAnimationAcceleration ? Mod.settings.GetStationSpeed("ChemistryStation") : 1f;
            behaviour.Npc.Movement.FacePoint(behaviour.targetStation.transform.position, 0.5f);
            yield return new WaitForSeconds(0.5f);

            behaviour.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", true);
            if (!(bool)AccessTools.Method(typeof(StartChemistryStationBehaviour), "CanCookStart").Invoke(behaviour, []))
            {
                AccessTools.Method(typeof(StartChemistryStationBehaviour), "StopCook").Invoke(behaviour, []);
                behaviour.End_Networked(null);
                yield break;
            }

            behaviour.targetStation.SetNPCUser(behaviour.Npc.NetworkObject);
            StationRecipe recipe = (CastTo<ChemistryStationConfiguration>(behaviour.targetStation.Configuration)).Recipe.SelectedRecipe;
            AccessTools.Method(typeof(StartChemistryStationBehaviour), "SetupBeaker").Invoke(behaviour, []);
            yield return new WaitForSeconds(1f / stationSpeed);
            
            Beaker beaker = CastTo<Beaker>(AccessTools.Field(typeof(StartChemistryStationBehaviour), "beaker").GetValue(behaviour));
            AccessTools.Method(typeof(StartChemistryStationBehaviour), "FillBeaker").Invoke(behaviour, [recipe, beaker]);
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
            AccessTools.Field(typeof(StartChemistryStationBehaviour), "beaker").SetValue(behaviour, null);
            AccessTools.Method(typeof(StartChemistryStationBehaviour), "StopCook").Invoke(behaviour, []);
            behaviour.End_Networked(null);
            yield break;

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
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
                Mod.LoggerInstance.Warning($"Source: {e.Source}");
                Mod.LoggerInstance.Warning($"{e.StackTrace}");
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
                MelonLogger.Warning($"Failed to patch method: {e.GetType().Name} - {e.Message}");
                MelonLogger.Warning($"Source: {e.Source}");
                MelonLogger.Warning($"{e.StackTrace}");
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
    public class NpcMovementPatches : Sched1PatchesBase
    {
        // using accesstools every fixedupdate is too much of a performance hit on mono
        [HarmonyPatch(typeof(NPCMovement), "UpdateSpeed")]
        [HarmonyPrefix]
        public static bool UpdateSpeedPrefix(NPCMovement __instance)
        {
            float walkAcceleration = 1f;
            NPC npc = CastTo<NPC>(AccessTools.Field(typeof(NPCMovement), "npc").GetValue(__instance));
            if (npc == null)
            {
                Mod.LoggerInstance.Msg($"updatespeed: npc was null?");
            }
            else if (Is<Employee>(npc))
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
            PropertyInfo timeSinceHitByCar = AccessTools.Property(typeof(NPCMovement), "timeSinceHitByCar");
            timeSinceHitByCar.SetValue(__instance, (float)timeSinceHitByCar.GetValue(__instance) + Time.fixedDeltaTime);
            __instance.capsuleCollider.transform.position = ((Rigidbody)AccessTools.Field(typeof(NPCMovement), "ragdollCentralRB").GetValue(__instance)).transform.position;
            AccessTools.Method(typeof(NPCMovement), "UpdateSpeed").Invoke(__instance, []);
            AccessTools.Method(typeof(NPCMovement), "UpdateStumble").Invoke(__instance, []);
            AccessTools.Method(typeof(NPCMovement), "UpdateRagdoll").Invoke(__instance, []);
            AccessTools.Method(typeof(NPCMovement), "UpdateDestination").Invoke(__instance, []);
            AccessTools.Method(typeof(NPCMovement), "RecordVelocity").Invoke(__instance, []);
            AccessTools.Method(typeof(NPCMovement), "UpdateSlippery").Invoke(__instance, []);
            AccessTools.Method(typeof(NPCMovement), "UpdateCache").Invoke(__instance, []);

            if (!((NPCAnimation)AccessTools.Property(typeof(NPCMovement), "anim").GetValue(__instance)).Avatar.Ragdolled || !__instance.CanRecoverFromRagdoll())
            {
                AccessTools.Field(typeof(NPCMovement), "ragdollStaticTime").SetValue(__instance, 0f);
                return false;
            }
            FieldInfo ragdollTime = AccessTools.Field(typeof(NPCMovement), "ragdollTime");
            ragdollTime.SetValue(__instance, (float)ragdollTime.GetValue(__instance) + Time.fixedDeltaTime);
            PropertyInfo ragdollStaticTime = AccessTools.Property(typeof(NPCMovement), "ragdollStaticTime");
            if (((Rigidbody)AccessTools.Property(typeof(NPCMovement), "ragdollCentralRB").GetValue(__instance)).velocity.magnitude < 0.25f)
            {
                ragdollStaticTime.SetValue(__instance, (float)ragdollStaticTime.GetValue(__instance) + Time.fixedDeltaTime);
                return false;
            }
            ragdollStaticTime.SetValue(__instance, 0f);

            return false;
        }
#endif
    }
}
