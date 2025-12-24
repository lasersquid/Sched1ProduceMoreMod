using HarmonyLib;
using MelonLoader;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Events;
using Il2CppSystem;





#if MONO_BUILD
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Packaging;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Product;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.UI.Shop;
using ScheduleOne.UI.Stations.Drying_rack;
using ScheduleOne.UI.Stations;
using ScheduleOne.Variables;
using ScheduleOne;
using ScheduleOne.Growing;
using ScheduleOne.Trash;
using StringArray = string[];
using ItemInstanceList = System.Collections.Generic.List<ScheduleOne.ItemFramework.ItemInstance>;
using MixingStationList = System.Collections.Generic.List<ScheduleOne.ObjectScripts.MixingStation>;
using Type = System.Type;
using Action = System.Action;
#else
using Il2CppFishNet;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Packaging;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Product.Packaging;
using Il2CppScheduleOne.Product;
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
using Il2CppScheduleOne.Growing;
using Il2CppScheduleOne.Trash;
using Il2CppTMPro;
using StringArray = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray;
using ItemInstanceList = Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.ItemFramework.ItemInstance>;
using MixingStationList = Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.ObjectScripts.MixingStation>;
using Type = System.Type;
using Action = System.Action;
using Exception = System.Exception;
#endif

namespace ProduceMore
{ 
    public class Utils
    {
        public static ProduceMoreMod Mod;

        public static Treturn GetField<Ttarget, Treturn>(string fieldName, object target) where Treturn : class
        {
            return CastTo<Treturn>(GetField<Ttarget>(fieldName, target));
        }

        public static object GetField<Ttarget>(string fieldName, object target)
        {
#if MONO_BUILD
            return AccessTools.Field(typeof(Ttarget), fieldName).GetValue(target);
#else
            return AccessTools.Property(typeof(Ttarget), fieldName).GetValue(target);
#endif
        }

        public static void SetField<Ttarget>(string fieldName, object target, object value)
        {
#if MONO_BUILD
            AccessTools.Field(typeof(Ttarget), fieldName).SetValue(target, value);
#else
            AccessTools.Property(typeof(Ttarget), fieldName).SetValue(target, value);
#endif
        }

        public static Treturn GetProperty<Ttarget, Treturn>(string fieldName, object target) where Treturn : class
        {
            return CastTo<Treturn>(GetProperty<Ttarget>(fieldName, target));
        }

        public static object GetProperty<Ttarget>(string fieldName, object target)
        {
            return AccessTools.Property(typeof(Ttarget), fieldName).GetValue(target);
        }

        public static void SetProperty<Ttarget>(string fieldName, object target, object value)
        {
            AccessTools.Property(typeof(Ttarget), fieldName).SetValue(target, value);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target) where Treturn : class
        {
            return CastTo<Treturn>(CallMethod<Ttarget>(methodName, target, []));
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target, object[] args) where Treturn : class
        {
            return CastTo<Treturn>(CallMethod<Ttarget>(methodName, target, args));
        }
        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, Type[] argTypes, object target, object[] args) where Treturn : class
        {
            return CastTo<Treturn>(CallMethod<Ttarget>(methodName, argTypes, target, args));
        }
        public static object CallMethod<Ttarget>(string methodName, object target)
        {
            return AccessTools.Method(typeof(Ttarget), methodName).Invoke(target, []);
        }

        public static object CallMethod<Ttarget>(string methodName, object target, object[] args)
        {
            return AccessTools.Method(typeof(Ttarget), methodName).Invoke(target, args);
        }

        public static object CallMethod<Ttarget>(string methodName, Type[] argTypes, object target, object[] args)
        {
            return AccessTools.Method(typeof(Ttarget), methodName, argTypes).Invoke(target, args);
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

        public static UnityAction ToUnityAction(Action action)
        {
#if MONO_BUILD
            return new UnityAction(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction>(action);
#endif
        }

        public static UnityAction<T> ToUnityAction<T>(System.Action<T> action)
        {
#if MONO_BUILD
            return new UnityAction<T>(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction<T>>(action);
#endif
        }

        public static object StartCoroutine(IEnumerator func)
        {
            object coroutine = MelonCoroutines.Start(func);
            Mod.runningCoroutines.Add(coroutine);
            return coroutine;
        }

        public static void StopCoroutine(object coroutine)
        {
            MelonCoroutines.Stop(coroutine);
            Mod.runningCoroutines.Remove(coroutine);
        }

        public static T ToInterface<T>(object o)
        {
            return (T)o;
        }

#if MONO_BUILD
        public static T ToInterface<T>(object o)
        {
            return (T)o;
        }
#else
        public static T ToInterface<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        {
            return CastTo<T>(System.Activator.CreateInstance(typeof(T), [o.Pointer]));
        }
#endif

        public static void Log(string message)
        {
            Utils.Mod.LoggerInstance.Msg(message);
        }

        public static void Warn(string message)
        {
            Utils.Mod.LoggerInstance.Warning(message);
        }

        // Compare unity objects by their instance ID
        public class UnityObjectComparer : IEqualityComparer<UnityEngine.Object>
        {
            public bool Equals(UnityEngine.Object a, UnityEngine.Object b)
            {
                return a.GetInstanceID() == b.GetInstanceID();
            }

            public int GetHashCode(UnityEngine.Object item)
            {
                return item.GetInstanceID();
            }
        }
    }

    // Set stack sizes
    [HarmonyPatch]
    public class ItemCapacityPatches
    {
        // Increase stack limit on item access
        [HarmonyPatch(typeof(ItemInstance), "StackLimit", MethodType.Getter)]
        [HarmonyPrefix]
        public static void StackLimitPrefix(ItemInstance __instance)
        {
            if (!Utils.Mod.processedItemDefs.Contains(__instance.Definition) && __instance.Definition.Name.ToLower() != "cash")
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
                if (!Utils.Mod.originalStackLimits.ContainsKey(category.ToString()))
                {
                    Utils.Mod.originalStackLimits[category.ToString()] = __instance.Definition.StackLimit;
                }

                __instance.Definition.StackLimit = Utils.Mod.GetStackLimit(__instance);
                Utils.Mod.processedItemDefs.Add(__instance.Definition);
            }
        }

        // For phone delivery app
        [HarmonyPatch(typeof(ListingEntry), "Initialize")]
        [HarmonyPrefix]
        public static void InitializePrefix(ShopListing match)
        {
            if (match != null)
            {
                if (!Utils.Mod.processedItemDefs.Contains(match.Item) && match.Item.Name.ToLower() != "cash")
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

                    if (!Utils.Mod.originalStackLimits.ContainsKey(category.ToString()))
                    {
                        Utils.Mod.originalStackLimits[category.ToString()] = match.Item.StackLimit;
                    }
                    int stackLimit = Utils.Mod.GetStackLimit(match.Item);


                    match.Item.StackLimit = stackLimit;
                    Utils.Mod.processedItemDefs.Add(match.Item);
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
                __result = Utils.Mod.GetStackLimit(item) - __instance.Quantity;
                return;
            }
            __result = 0;
            return;
        }

        [HarmonyPatch(typeof(NPCInventory), "GetCapacityForItem")]
        [HarmonyPostfix]
        public static void NPCGetCapacityForItemPostfix(NPCInventory __instance, ItemInstance item, ref int __result)
        {
            if (item == null)
            {
                __result = 0;
                return;
            }
            int num = 0;
            for (int i = 0; i < __instance.ItemSlots.Count; i++)
            {
                if (__instance.ItemSlots[i] != null && !__instance.ItemSlots[i].IsLocked && !__instance.ItemSlots[i].IsAddLocked)
                {
                    if (__instance.ItemSlots[i].ItemInstance == null)
                    {
                        num += Utils.Mod.GetStackLimit(item);
                    }
                    else if (__instance.ItemSlots[i].ItemInstance.CanStackWith(item, true))
                    {
                        num += Utils.Mod.GetStackLimit(item) - __instance.ItemSlots[i].ItemInstance.Quantity;
                    }
                }
            }
            __result = num;
            return;
        }
    }

    // This patch requires its own class since we need a TargetMethod method to select
    // the correct overload of GetItem. Regular Harmony annotations don't support this.
    [HarmonyPatch]
    public class RegistryPatches
    {
        // GetItem has two definitions: ItemDefinition GetItem(string), and T GetItem<T>(string)
        // TargetMethod required to select non-generic overload of GetItem
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return AccessTools.FirstMethod(typeof(Registry), (method) => 
                method.Name == "GetItem" && method.ReturnType == typeof(ItemDefinition)
            );
        }

        [HarmonyPatch]
        [HarmonyPostfix]
        public static void GetItemPostfix(ref ItemDefinition __result)
        {
            if (__result == null)
            {
                return;
            }
            if (!Utils.Mod.processedItemDefs.Contains(__result) && __result.Name.ToLower() != "cash")
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

                if (!Utils.Mod.originalStackLimits.ContainsKey(category.ToString()))
                {
                    Utils.Mod.originalStackLimits[category.ToString()] = __result.StackLimit; 
                }

                __result.StackLimit = Utils.Mod.GetStackLimit(__result);
                Utils.Mod.processedItemDefs.Add(__result);
            }
        }
    }


    // Patch drying rack capacity and speed
    [HarmonyPatch]
    public class DryingRackPatches
    {
        // Modify DryingRack.ItemCapacity
        [HarmonyPatch(typeof(StartDryingRackBehaviour), "IsRackReady")]
        [HarmonyPrefix]
        public static void IsRackReadyPrefix(DryingRack rack, ref bool __result)
        {
            if (!Utils.Mod.processedStationCapacities.Contains(rack))
            {
                if (!Utils.Mod.originalStationCapacities.ContainsKey("DryingRack"))
                {
                    Utils.Mod.originalStationCapacities["DryingRack"] = rack.ItemCapacity;
                }
                rack.ItemCapacity = Utils.Mod.GetStationCapacity("DryingRack");
                Utils.Mod.processedStationCapacities.Add(rack);
            }
        }


        // Modify DryingRack.ItemCapacity
        // canstartoperation runs every time a player or npc tries to interact
        // may have optimized away real access to ItemCapacity; replace method body
        [HarmonyPatch(typeof(DryingRack), "CanStartOperation")]
        [HarmonyPrefix]
        public static bool CanStartOperationPrefix(DryingRack __instance, ref bool __result)
        {
            if (!Utils.Mod.processedStationCapacities.Contains(__instance))
            {
                if (!Utils.Mod.originalStationCapacities.ContainsKey("DryingRack"))
                {
                    Utils.Mod.originalStationCapacities["DryingRack"] = __instance.ItemCapacity;
                }
                __instance.ItemCapacity = Utils.Mod.GetStationCapacity("DryingRack");
                Utils.Mod.processedStationCapacities.Add(__instance);
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
            if (!Utils.Mod.originalStationTimes.ContainsKey("DryingRack"))
            {
                Utils.Mod.originalStationTimes["DryingRack"] = DryingRack.DRY_MINS_PER_TIER;
               
            }

            float stationSpeed = (float)Utils.Mod.originalStationTimes["DryingRack"] / Utils.Mod.GetStationSpeed("DryingRack");
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
            if (!Utils.Mod.originalStationTimes.ContainsKey("DryingRack"))
            {
                Utils.Mod.originalStationTimes["DryingRack"] = DryingRack.DRY_MINS_PER_TIER;
            }
            int dryingTime = (int)((float)Utils.Mod.originalStationTimes["DryingRack"] / Utils.Mod.GetStationSpeed("DryingRack"));

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
            if (!Utils.Mod.originalStationTimes.ContainsKey("DryingRack"))
            {
                Utils.Mod.originalStationTimes["DryingRack"] = DryingRack.DRY_MINS_PER_TIER;
            }
            if (__instance == null)
            {
                return false;
            }
            foreach (DryingOperation dryingOperation in __instance.DryingOperations)
            {
                dryingOperation.Time++;
                if (dryingOperation.Time >= ((float)Utils.Mod.originalStationTimes["DryingRack"] / Utils.Mod.GetStationSpeed("DryingRack")))
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

            Utils.SetProperty<StartDryingRackBehaviour>("WorkInProgress", __instance, true);
            __instance.Npc.Movement.FacePoint(__instance.Rack.uiPoint.position, 0.5f);

            object workCoroutine = Utils.StartCoroutine(BeginActionCoroutine(__instance));
            Utils.SetField<StartDryingRackBehaviour>("workRoutine", __instance, (Coroutine)workCoroutine);
            return false;
        }

        // Replacement coroutine for BeginAction
        private static IEnumerator BeginActionCoroutine(StartDryingRackBehaviour behaviour)
        {
            float stationSpeed = Utils.Mod.GetStationWorkSpeed("DryingRack");
            yield return new WaitForEndOfFrame();
            behaviour.Rack.InputSlot.ItemInstance.GetCopy(1);
            int itemCount = 0;
            while (behaviour.Rack != null && behaviour.Rack.InputSlot.Quantity > itemCount && behaviour.Rack.GetTotalDryingItems() + itemCount < behaviour.Rack.ItemCapacity)
            {
                behaviour.Npc.Avatar.Animation.SetTrigger("GrabItem");
                yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / stationSpeed));
                int num = itemCount;
                itemCount = num + 1;
            }
            if (InstanceFinder.IsServer)
            {
                behaviour.Rack.StartOperation();
            }
            Utils.SetProperty<StartDryingRackBehaviour>("WorkInProgress", behaviour, false);
            Utils.SetField<StartDryingRackBehaviour>("workRoutine", behaviour, null);
            yield break;
        }

        [HarmonyPatch(typeof(StartDryingRackBehaviour), "StopCauldron")]
        [HarmonyPrefix]
        public static bool StopCauldronPrefix(StartDryingRackBehaviour __instance)
        {
            object workRoutine = Utils.GetField<StartDryingRackBehaviour>("workRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartDryingRackBehaviour>("workRoutine", __instance, null);
            }
            Utils.SetProperty<StartDryingRackBehaviour>("WorkInProgress", __instance, false);

            return false;
        }
    }


    // Patch lab oven capacity and speed
    [HarmonyPatch]
    public class LabOvenPatches
    {
        // speed
        [HarmonyPatch(typeof(OvenCookOperation), "GetCookDuration")]
        [HarmonyPostfix]
        public static void GetCookDurationPostfix(OvenCookOperation __instance, ref int __result)
        {
            if (!Utils.Mod.originalStationTimes.ContainsKey("LabOven"))
            {
                Utils.Mod.originalStationTimes["LabOven"] = __instance.Ingredient.StationItem.GetModule<CookableModule>().CookTime;
            }
            __result = (int)((float)Utils.Mod.originalStationTimes["LabOven"] / Utils.Mod.GetStationSpeed("LabOven"));
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
                capacity = Utils.Mod.GetStackLimit(productInstance);
            }
            else if (productInstance.CanStackWith(outputInstance.Definition.GetDefaultInstance(1), false))
            {
                capacity = Utils.Mod.GetStackLimit(productInstance) - outputInstance.Quantity;
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
                if (Utils.GetField<StartLabOvenBehaviour>("cookRoutine", __instance) != null || __instance.targetOven == null)
                {
                    return false;
                }
                object workRoutine = Utils.StartCoroutine(StartCookCoroutine(__instance));
                Utils.SetField<StartLabOvenBehaviour>("cookRoutine", __instance, (Coroutine)workRoutine);
            }
            catch (Exception e)
            {
                Utils.Warn($"Failed to set cookroutine: {e.GetType().Name} - {e.Message}");
                Utils.Warn($"Source: {e.Source}");
                Utils.Warn($"{e.StackTrace}");
            }

            return false;
        }

        // Startcook coroutine with accelerated animations
        private static IEnumerator StartCookCoroutine(StartLabOvenBehaviour behaviour)
        {
            float stationSpeed = Utils.Mod.GetStationWorkSpeed("LabOven");
            behaviour.targetOven.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.targetOven.transform.position, Mathf.Max(0.1f, 0.5f / stationSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / stationSpeed));

            if (!(bool)Utils.CallMethod<StartLabOvenBehaviour>("CanCookStart", behaviour, []))
            {
                Utils.CallMethod<StartLabOvenBehaviour>("StopCook", behaviour, []);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            // halp
            behaviour.targetOven.Door.SetPosition(1f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / stationSpeed));

            behaviour.targetOven.WireTray.SetPosition(1f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 5f / stationSpeed));

            behaviour.targetOven.Door.SetPosition(0f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / stationSpeed));

            ItemInstance itemInstance = behaviour.targetOven.IngredientSlot.ItemInstance;
            if (itemInstance == null)
            {
                Utils.CallMethod<StartLabOvenBehaviour>("StopCook", behaviour);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            int num = 1;
            if ((Utils.CastTo<StorableItemDefinition>(itemInstance.Definition)).StationItem.GetModule<CookableModule>().CookType == CookableModule.ECookableType.Solid)
            {
                num = Mathf.Min(behaviour.targetOven.IngredientSlot.Quantity, 10);
            }
            itemInstance.ChangeQuantity(-num);
            string id = (Utils.CastTo<StorableItemDefinition>(itemInstance.Definition)).StationItem.GetModule<CookableModule>().Product.ID;
            EQuality ingredientQuality = EQuality.Standard;
            if (Utils.Is<QualityItemInstance>(itemInstance))
            {
                ingredientQuality = (Utils.CastTo<QualityItemInstance>(itemInstance)).Quality;
            }
            behaviour.targetOven.SendCookOperation(new OvenCookOperation(itemInstance.ID, ingredientQuality, num, id));
            Utils.CallMethod<StartLabOvenBehaviour>("StopCook", behaviour, []);
            behaviour.Deactivate_Networked(null);
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
            object workRoutine = Utils.GetField<StartLabOvenBehaviour>("cookRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartLabOvenBehaviour>("cookRoutine", __instance, null);
            }

            return false;
        }

        // Call our own finishcook coroutine with accelerated animations
        [HarmonyPatch(typeof(FinishLabOvenBehaviour), "RpcLogic___StartAction_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_StartFinishCookPrefix(FinishLabOvenBehaviour __instance)
        {
            if (Utils.GetField<FinishLabOvenBehaviour>("actionRoutine", __instance) != null)
            {
                return false;
            }
            if (__instance.targetOven == null)
            {
                return false;
            }
            object workRoutine = Utils.StartCoroutine(FinishCookCoroutine(__instance));
            Utils.SetField<FinishLabOvenBehaviour>("actionRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        // FinishCook coroutine with accelerated animations
        private static IEnumerator FinishCookCoroutine(FinishLabOvenBehaviour behaviour)
        {
            float stationSpeed = Utils.Mod.GetStationWorkSpeed("LabOven");
            behaviour.targetOven.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.targetOven.transform.position, Mathf.Max(0.1f, 0.5f / stationSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / stationSpeed));

            if (!(bool)Utils.CallMethod<FinishLabOvenBehaviour>("CanActionStart", behaviour, []))
            {
                Utils.CallMethod<FinishLabOvenBehaviour>("StopAction", behaviour, []);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            behaviour.Npc.SetEquippable_Networked(null, "Avatar/Equippables/Hammer");
            behaviour.targetOven.Door.SetPosition(1f);
            behaviour.targetOven.WireTray.SetPosition(1f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / stationSpeed));

            behaviour.targetOven.SquareTray.SetParent(behaviour.targetOven.transform);
            behaviour.targetOven.RemoveTrayAnimation.Play();
            yield return new WaitForSeconds(0.1f);

            behaviour.targetOven.Door.SetPosition(0f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / stationSpeed));

            behaviour.Npc.SetAnimationBool_Networked(null, "UseHammer", true);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 10f / stationSpeed));

            behaviour.Npc.SetAnimationBool_Networked(null, "UseHammer", false);
            behaviour.targetOven.Shatter(behaviour.targetOven.CurrentOperation.Cookable.ProductQuantity, behaviour.targetOven.CurrentOperation.Cookable.ProductShardPrefab.gameObject);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / stationSpeed));
            
            ItemInstance productItem = behaviour.targetOven.CurrentOperation.GetProductItem(behaviour.targetOven.CurrentOperation.Cookable.ProductQuantity * behaviour.targetOven.CurrentOperation.IngredientQuantity);
            behaviour.targetOven.OutputSlot.AddItem(productItem, false);
            
            behaviour.targetOven.SendCookOperation(null);
            Utils.CallMethod<FinishLabOvenBehaviour>("StopAction", behaviour, []);
            behaviour.Deactivate_Networked(null);
            yield break;
        }

        [HarmonyPatch(typeof(FinishLabOvenBehaviour), "StopAction")]
        [HarmonyPrefix]
        public static bool StopActionPrefix(FinishLabOvenBehaviour __instance)
        {
            __instance.targetOven.SetNPCUser(null);
            __instance.Npc.SetEquippable_Networked(null, string.Empty);
            __instance.Npc.SetAnimationBool_Networked(null, "UseHammer", false);

            object workRoutine = Utils.GetField<FinishLabOvenBehaviour>("actionRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<FinishLabOvenBehaviour>("actionRoutine", __instance, null);
            }

            return false;
        }
    }


    // Patch mixing station capacity and speed
    [HarmonyPatch]
    public class MixingStationPatches
    {
        // capacity
        [HarmonyPatch(typeof(MixingStation), "GetMixQuantity")]
        [HarmonyPrefix]
        public static bool GetMixQuantityPrefix(MixingStation __instance, ref int __result)
        {
            if (!Utils.Mod.processedStationCapacities.Contains(__instance))
            {
                if (Utils.Is<MixingStationMk2>(__instance))
                {
                    if (!Utils.Mod.originalStationCapacities.ContainsKey("MixingStationMk2"))
                    {
                        Utils.Mod.originalStationCapacities["MixingStationMk2"] = __instance.MaxMixQuantity;
                    }
                    __instance.MaxMixQuantity = Utils.Mod.GetStationCapacity("MixingStationMk2");

                }
                else
                {
                    if (!Utils.Mod.originalStationCapacities.ContainsKey("MixingStation"))
                    {
                        Utils.Mod.originalStationCapacities["MixingStation"] = __instance.MaxMixQuantity;
                    }
                    __instance.MaxMixQuantity = Utils.Mod.GetStationCapacity("MixingStation");

                }
                Utils.Mod.processedStationCapacities.Add(__instance);
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
            if (!Utils.Mod.processedStationSpeeds.Contains(__instance))
            {
                if (Utils.Is<MixingStationMk2>(__instance))
                {
                    if (!Utils.Mod.originalStationTimes.ContainsKey("MixingStationMk2"))
                    {
                        // Use a dirty magic number for now; relevant constant is not accessible in il2cpp
                        Utils.Mod.originalStationTimes["MixingStationMk2"] = 3;
                    }
                    __instance.MixTimePerItem = (int)Mathf.Max((float)Utils.Mod.originalStationTimes["MixingStationMk2"] / Utils.Mod.GetStationSpeed("MixingStationMk2"), 1f);
                }
                else
                {
                    if (!Utils.Mod.originalStationTimes.ContainsKey("MixingStation"))
                    {
                        // Use a dirty magic number for now; relevant constant is not accessible in il2cpp
                        Utils.Mod.originalStationTimes["MixingStation"] = 15;
                    }
                    __instance.MixTimePerItem = (int)Mathf.Max((float)Utils.Mod.originalStationTimes["MixingStation"] / Utils.Mod.GetStationSpeed("MixingStation"), 1f);
                }

                // TODO: check if it's even necessary to modify this field anymore
                Utils.Mod.processedStationSpeeds.Add(__instance);
            }

            int originalStationTime;
            float stationSpeed;
            if (Utils.Is<MixingStationMk2>(__instance))
            {
                stationSpeed = Utils.Mod.GetStationSpeed("MixingStationMk2");
                originalStationTime = Utils.Mod.originalStationTimes["MixingStationMk2"];
            }
            else
            {
                stationSpeed = Utils.Mod.GetStationSpeed("MixingStation");
                originalStationTime = Utils.Mod.originalStationTimes["MixingStation"];
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
        public static bool GetMixStationsReadyToMovePrefix(Chemist __instance, ref MixingStationList __result)
        {
            var list = new MixingStationList();

            foreach (MixingStation mixingStation in Utils.CastTo<ChemistConfiguration>(Utils.GetProperty<Chemist>("configuration", __instance)).MixStations)
            {
                ItemSlot outputSlot = mixingStation.OutputSlot;
                MixingStationConfiguration configuration = Utils.CastTo<MixingStationConfiguration>(mixingStation.Configuration);
                BuildableItem destination = configuration.Destination.SelectedObject;
                //TODO: move transitroutevalid check after packaging station check
                if (outputSlot.Quantity != 0 && __instance.MoveItemBehaviour.IsTransitRouteValid(configuration.DestinationRoute, outputSlot.ItemInstance.ID))
                {
                    // Only deliver to packaging stations with at least half a stack of space in input slot.
                    if (Utils.Is<PackagingStation>(destination) || Utils.Is<PackagingStationMk2>(destination))
                    {
                        PackagingStation station = Utils.CastTo<PackagingStation>(destination);
                        if (station.ProductSlot.ItemInstance == null)
                        {
                            list.Add(mixingStation);
                        }
                        else if (station.ProductSlot.ItemInstance.CanStackWith(outputSlot.ItemInstance))
                        {
                            int inputStackLimit = Utils.Mod.GetStackLimit(station.ProductSlot.ItemInstance);
                            if (inputStackLimit - station.ProductSlot.Quantity > inputStackLimit / 2)
                            {
                                list.Add(mixingStation);
                            }
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
            if (!Utils.Mod.processedStationCapacities.Contains(__instance.targetStation))
            {
                if (Utils.Is<MixingStationMk2>(__instance))
                {
                    if (!Utils.Mod.originalStationCapacities.ContainsKey("MixingStationMk2"))
                    {
                        Utils.Mod.originalStationCapacities["MixingStationMk2"] = __instance.targetStation.MaxMixQuantity;
                    }
                    __instance.targetStation.MaxMixQuantity = Utils.Mod.GetStationCapacity("MixingStationMk2");

                }
                else
                {
                    if (!Utils.Mod.originalStationCapacities.ContainsKey("MixingStation"))
                    {
                        Utils.Mod.originalStationCapacities["MixingStation"] = __instance.targetStation.MaxMixQuantity;
                    }
                    __instance.targetStation.MaxMixQuantity = Utils.Mod.GetStationCapacity("MixingStation");

                }
                Utils.Mod.processedStationCapacities.Add(__instance.targetStation);
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
                    Utils.SetProperty<MixingStation>("CurrentMixTime", __instance, currentMixTime2 + 1);
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
            if (Utils.GetField<StartMixingStationBehaviour>("startRoutine", __instance) != null)
            {
                return false;
            }
            if (__instance.targetStation == null)
            {
                return false;
            }
            object workRoutine = Utils.StartCoroutine(StartMixCoroutine(__instance));
            Utils.SetField<StartMixingStationBehaviour>("startRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        private static IEnumerator StartMixCoroutine(StartMixingStationBehaviour behaviour)
        {
            float stationSpeed;
            if (Utils.Is<MixingStationMk2>(behaviour.targetStation))
            {
                stationSpeed = Utils.Mod.GetStationWorkSpeed("MixingStationMk2");
            }
            else
            {
                stationSpeed = Utils.Mod.GetStationWorkSpeed("MixingStation");
            }

            behaviour.Npc.Movement.FacePoint(behaviour.targetStation.transform.position, Mathf.Max(0.1f, 0.5f / stationSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / stationSpeed));

            if (!(bool)Utils.CallMethod<StartMixingStationBehaviour>("CanCookStart", behaviour, []))
            {
                Utils.CallMethod<StartMixingStationBehaviour>("StopCook", behaviour, []);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            behaviour.targetStation.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", true);
            QualityItemInstance product = Utils.CastTo<QualityItemInstance>(behaviour.targetStation.ProductSlot.ItemInstance);
            ItemInstance mixer = behaviour.targetStation.MixerSlot.ItemInstance;
            int mixQuantity = behaviour.targetStation.GetMixQuantity();
            float mixTime = Mathf.Max(0.1f, (float)mixQuantity / stationSpeed);
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

            Utils.CallMethod<StartMixingStationBehaviour>("StopCook", behaviour, []);
            behaviour.Deactivate_Networked(null);
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

            object workRoutine = Utils.GetField<StartMixingStationBehaviour>("startRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartMixingStationBehaviour>("startRoutine", __instance, null);
            }

            return false;

        }

        // increase threshold
        [HarmonyPatch(typeof(MixingStationUIElement), "Initialize")]
        [HarmonyPrefix]
        public static void InitializeUIPrefix(MixingStation station)
        {
            int stationCapacity = Utils.Is<MixingStationMk2>(station) ? Utils.Mod.GetStationCapacity("MixingStationMk2") : Utils.Mod.GetStationCapacity("MixingStation");
            Utils.CastTo<MixingStationConfiguration>(station.Configuration).StartThrehold.Configure(1f, stationCapacity, true);
        }
    }


    // Brick press patches
    [HarmonyPatch]
    public class BrickPressPatches
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
            Utils.SetProperty<BrickPressBehaviour>("PackagingInProgress", __instance, true);
            __instance.Npc.Movement.FaceDirection(__instance.Press.StandPoint.forward, 0.5f);
            object workRoutine = Utils.StartCoroutine(PackagingCoroutine(__instance));
            Utils.SetField<BrickPressBehaviour>("packagingRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        private static IEnumerator PackagingCoroutine(BrickPressBehaviour behaviour)
        {
            float stationSpeed = Utils.Mod.GetStationWorkSpeed("BrickPress");
            yield return new WaitForEndOfFrame();

            behaviour.Npc.Avatar.Animation.SetBool("UsePackagingStation", true);
            float packageTime = 15f / (Utils.CastTo<Packager>(behaviour.Npc).PackagingSpeedMultiplier * stationSpeed);
            for (float i = 0f; i < packageTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Press.uiPoint.position, 0, false);
                yield return new WaitForEndOfFrame();
            }

            behaviour.Npc.Avatar.Animation.SetBool("UsePackagingStation", false);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.2f / stationSpeed));

            behaviour.Npc.Avatar.Animation.SetTrigger("GrabItem");
            behaviour.Press.PlayPressAnim();
            yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / stationSpeed));

            ProductItemInstance product;
            if (InstanceFinder.IsServer && behaviour.Press.HasSufficientProduct(out product))
            {
                behaviour.Press.CompletePress(product);
            }
            Utils.SetProperty<BrickPressBehaviour>("PackagingInProgress", behaviour, false);
            Utils.SetField<BrickPressBehaviour>("packagingRoutine", behaviour, null);
            yield break;
        }

        // gracefully stop meloncoroutine
        [HarmonyPatch(typeof(BrickPressBehaviour), "StopPackaging")]
        [HarmonyPrefix]
        public static bool StopPackagingPrefix(BrickPressBehaviour __instance)
        {
            object workRoutine = Utils.GetField<BrickPressBehaviour>("packagingRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<BrickPressBehaviour>("packagingRoutine", __instance, null);
            }
            Utils.SetField<BrickPressBehaviour>("PackagingInProgress", __instance, false);
            return false;
        }
    }


    // cauldron patches
    [HarmonyPatch]
    public class CauldronPatches
    {
        // patch visuals for capacity
        [HarmonyPatch(typeof(Cauldron), "UpdateIngredientVisuals")]
        [HarmonyPrefix]
        public static bool UpdateIngredientVisualsPatch(Cauldron __instance)
        {
            int cauldronCapacity = Utils.Mod.GetStackLimit("Coca Leaf", EItemCategory.Agriculture);
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
            if (!Utils.Mod.processedStationTimes.Contains(__instance))
            {
                if (!Utils.Mod.originalStationTimes.ContainsKey("Cauldron"))
                {
                    Utils.Mod.originalStationTimes["Cauldron"] = __instance.CookTime;
                }
                int newCookTime = (int)((float)Utils.Mod.originalStationTimes["Cauldron"] / Utils.Mod.GetStationSpeed("Cauldron"));
                __instance.CookTime = newCookTime;
                Utils.Mod.processedStationTimes.Add(__instance);
            }
            remainingCookTime = (int)((float)Utils.Mod.originalStationTimes["Cauldron"] / Utils.Mod.GetStationSpeed("Cauldron"));
        }

        // accurately calculate output space instead of assuming limit of 10
        [HarmonyPatch(typeof(Cauldron), "HasOutputSpace")]
        [HarmonyPostfix]
        public static void HasOutputSpacePostfix(Cauldron __instance, ref bool __result)
        {
            __result = Utils.Mod.GetStackLimit(__instance.CocaineBaseDefinition) >= 10;
        }

        // call to HasOutputSpace seems to have been optimized out.
        [HarmonyPatch(typeof(Cauldron), "GetState")]
        [HarmonyPostfix]
        public static void GetStatePostfix(Cauldron __instance, ref Cauldron.EState __result)
        {
            // re-insert original method body.
            if ((bool)Utils.GetProperty<Cauldron>("isCooking", __instance))
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

            Utils.SetProperty<StartCauldronBehaviour>("WorkInProgress", __instance, true);
            __instance.Npc.Movement.FaceDirection(__instance.Station.StandPoint.forward, 0.5f);
            object workCoroutine = Utils.StartCoroutine(BeginCauldronCoroutine(__instance));
            Utils.SetField<StartCauldronBehaviour>("workRoutine", __instance, (Coroutine)workCoroutine);

            return false;
        }


        // coroutine with reduced animation times
        private static IEnumerator BeginCauldronCoroutine(StartCauldronBehaviour behaviour)
        {
            yield return new WaitForEndOfFrame();
            behaviour.Npc.Avatar.Animation.SetBool("UseChemistryStation", true);
            float packageTime = Mathf.Max(0.1f, 15f / Utils.Mod.GetStationWorkSpeed("Cauldron"));
            for (float i = 0f; i < packageTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Station.LinkOrigin.position, 0, false);
                yield return new WaitForEndOfFrame();
            }
            behaviour.Npc.Avatar.Animation.SetBool("UseChemistryStation", false);
            if (InstanceFinder.IsServer)
            {
                EQuality quality = behaviour.Station.RemoveIngredients();
                behaviour.Station.StartCookOperation(null, behaviour.Station.CookTime, quality);
            }
            
            Utils.SetProperty<StartCauldronBehaviour>("WorkInProgress", behaviour, false);
            Utils.SetField<StartCauldronBehaviour>("workRoutine", behaviour, null);
            yield break;
        }


        [HarmonyPatch(typeof(StartCauldronBehaviour), "StopCauldron")]
        [HarmonyPrefix]
        public static bool StopCauldronPrefix(StartCauldronBehaviour __instance)
        {
            object workRoutine = Utils.GetField<StartCauldronBehaviour>("workRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartCauldronBehaviour>("workRoutine", __instance, null);
            }
            if (InstanceFinder.IsServer && __instance.Station != null && __instance.Station.NPCUserObject == __instance.Npc.NetworkObject)
            {
                __instance.Station.SetNPCUser(null);
            }
            Utils.SetProperty<StartCauldronBehaviour>("WorkInProgress", __instance, false);
            return false;
        }
    }


    // packaging station patches
    [HarmonyPatch]
    public class PackagingStationPatches
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

            Utils.SetProperty<PackagingStationBehaviour>("PackagingInProgress", __instance, true);
            __instance.Npc.Movement.FaceDirection(__instance.Station.StandPoint.forward, 0.5f);
            object packagingCoroutine = Utils.StartCoroutine(BeginPackagingCoroutine(__instance));
            Utils.SetField<PackagingStationBehaviour>("packagingRoutine", __instance, (Coroutine)packagingCoroutine);

            return false;
        }


        // replacement coroutine to accelerate animation
        private static IEnumerator BeginPackagingCoroutine(PackagingStationBehaviour behaviour)
        {
            yield return new WaitForEndOfFrame();
            behaviour.Npc.Avatar.Animation.SetBool("UsePackagingStation", true);

            float stationWorkSpeed;
            if (Utils.Is<PackagingStationMk2>(behaviour.Station))
            {
                stationWorkSpeed = Utils.Mod.GetStationWorkSpeed("PackagingStationMk2");
            }
            else
            {
                stationWorkSpeed = Utils.Mod.GetStationWorkSpeed("PackagingStation");
            }

            ProductItemInstance packagedInstance = Utils.CastTo<ProductItemInstance>(behaviour.Station.ProductSlot.ItemInstance.GetCopy(1));
            PackagingDefinition packagingDefinition = Utils.CastTo<PackagingDefinition>(behaviour.Station.PackagingSlot.ItemInstance.Definition);
            packagedInstance.SetPackaging(packagingDefinition);
            int outputSpace = behaviour.Station.OutputSlot.GetCapacityForItem(packagedInstance);
            int productPerPackage = packagingDefinition.Quantity;
            int availableToPackage = Mathf.Min(behaviour.Station.ProductSlot.Quantity, behaviour.Station.PackagingSlot.Quantity);
            // leave the last package to PackSingleInstance
            int numPackages = Mathf.Max(Mathf.Min(availableToPackage / productPerPackage, outputSpace) - 1, 0);
            float packageTime = Mathf.Max(0.1f, (float)numPackages * 5f / (Utils.CastTo<Packager>(behaviour.Npc).PackagingSpeedMultiplier * behaviour.Station.PackagerEmployeeSpeedMultiplier * stationWorkSpeed));

            //Log($"Have {behaviour.Station.ProductSlot.Quantity} product in input slot, {behaviour.Station.PackagingSlot.Quantity} {behaviour.Station.PackagingSlot.ItemInstance.Definition.Name} in packaging slot, and {behaviour.Station.OutputSlot.Quantity} packaged product in the output.");
            //Log($"Have {outputSpace} output space, {availableToPackage} product to package at {productPerPackage} product per package. Will make {numPackages} packages in {packageTime} seconds.");

            for (float i = 0f; i < packageTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Station.Container.position, 0, false);
                yield return new WaitForEndOfFrame();
            }

            if (InstanceFinder.IsServer && numPackages >= 0)
            {
                float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("PackagedProductCount");
                NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("PackagedProductCount", (value + (numPackages + 1) * productPerPackage).ToString(), true);
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
                // Make sure PackSingleInstance is called--AutoRestock hooks it
                behaviour.Station.PackSingleInstance();
            }

            behaviour.Npc.Avatar.Animation.SetBool("UsePackagingStation", false);
            Utils.SetProperty<PackagingStationBehaviour>("PackagingInProgress", behaviour, false);
            Utils.SetField<PackagingStationBehaviour>("packagingRoutine", behaviour, null);
            yield break;
        }

        // stop our coroutine cleanly
        [HarmonyPatch(typeof(PackagingStationBehaviour), "StopPackaging")]
        [HarmonyPrefix]
        public static bool StopPackagingPrefix(PackagingStationBehaviour __instance)
        {
            object workRoutine = Utils.GetField<PackagingStationBehaviour>("packagingRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<PackagingStationBehaviour>("packagingRoutine", __instance, null);
            }
            __instance.Npc.Avatar.Animation.SetBool("UsePackagingStation", false);
            if (InstanceFinder.IsServer && __instance.Station != null && __instance.Station.NPCUserObject == __instance.Npc.NetworkObject)
            {
                __instance.Station.SetNPCUser(null);
            }
            Utils.SetProperty<PackagingStationBehaviour>("PackagingInProgress", __instance, false);
            return false;
        }
    }


    // pot patches
    [HarmonyPatch]
    public class PotPatches
    {
        // speed
        [HarmonyPatch(typeof(GrowContainer), "GetAverageLightExposure")]
        [HarmonyPostfix]
        public static void GetAverageLightExposurePostfix(ref float __result)
        {
            __result = __result * Utils.Mod.GetStationSpeed("Pot");
        }

        // use our own coroutine to speed up employee animation
        [HarmonyPatch(typeof(GrowContainerBehaviour), "PerformAction")]
        [HarmonyPrefix]
        public static bool PerformActionPrefix(GrowContainerBehaviour __instance)
        {
            if (!__instance.AreTaskConditionsMetForContainer(Utils.GetProperty<GrowContainerBehaviour, GrowContainer>("_growContainer", __instance)))
            {
                __instance.Disable_Networked(null);
                return false;
            }
            // EState.PerformingAction
            Utils.SetProperty<GrowContainerBehaviour>("CurrentState", __instance, 3);
            object workRoutine = Utils.StartCoroutine(PerformActionCoroutine(__instance));
            Utils.SetField<GrowContainerBehaviour>("performActionRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        // coroutine with accelerated animations
        private static IEnumerator PerformActionCoroutine(GrowContainerBehaviour behaviour)
        {
            float stationSpeed = Utils.Mod.GetStationWorkSpeed("Pot");

            Utils.CallMethod<GrowContainerBehaviour>("OnStartPerformAction", behaviour, []);
            float waitTime = (float)Utils.CallMethod<GrowContainerBehaviour>("GetActionDuration", behaviour, []) / stationSpeed;
            for (float i = 0f; i < waitTime; i += Time.deltaTime)
            {
                Vector3 targetPosition = (Vector3)Utils.CallMethod<GrowContainerBehaviour>("GetGrowContainerLookPoint", behaviour, []);
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(targetPosition, 0, false);
                yield return new WaitForEndOfFrame();
            }
            GrowContainer growContainer = Utils.GetProperty<GrowContainerBehaviour, GrowContainer>("_growContainer", behaviour);
            if (!behaviour.AreTaskConditionsMetForContainer(growContainer))
            {
                behaviour.Disable_Networked(null);
                yield break;
            }
            Utils.CallMethod<GrowContainerBehaviour>("OnStopPerformAction", behaviour, []);
            ItemSlot itemSlot = null;
            StringArray suitableItemIDs = null;
            object[] args = new object[1] { suitableItemIDs };
            bool taskRequiresItem = (bool)Utils.CallMethod<GrowContainerBehaviour>("DoesTaskRequireItem", behaviour, args);
            suitableItemIDs = Utils.CastTo<StringArray>(args[1]);
            if (taskRequiresItem)
            {
                Botanist botanist = Utils.GetProperty<GrowContainerBehaviour, Botanist>("_botanist", behaviour);
                IItemSlotOwner slotOwner = Utils.ToInterface<IItemSlotOwner>(botanist.Inventory);
                itemSlot = Utils.CallMethod<GrowContainerBehaviour, ItemSlot>("GetItemSlotContainingRequiredItem", behaviour, [slotOwner, suitableItemIDs]);
            }
            ItemInstance usedItem = (itemSlot != null) ? itemSlot.ItemInstance.GetCopy(1) : null;
            if ((bool)Utils.CallMethod<GrowContainerBehaviour>("CheckSuccess", behaviour, [usedItem]))
            {
                Utils.CallMethod<GrowContainerBehaviour>("OnActionSuccess", behaviour, [usedItem]);
                if (itemSlot != null && itemSlot.Quantity > 0)
                {
                    itemSlot.ChangeQuantity(-1, false);
                }
                TrashItem trashPrefab = Utils.CallMethod<GrowContainerBehaviour, TrashItem>("GetTrashPrefab", behaviour, [usedItem]);
                if (trashPrefab != null)
                {
                    NetworkSingleton<TrashManager>.Instance.CreateTrashItem(trashPrefab.ID, behaviour.Npc.transform.position + Vector3.up * 0.3f, UnityEngine.Random.rotation, default(Vector3), "", false);
                }
            }
            behaviour.Disable_Networked(null);
            yield break;
        }

        // properly stop our coroutines
        [HarmonyPatch(typeof(GrowContainerBehaviour), "StopAllRoutines")]
        [HarmonyPrefix]
        public static bool StopAllRoutinesPrefix(GrowContainerBehaviour __instance)
        {
            Coroutine walkRoutine = Utils.GetField<GrowContainerBehaviour, Coroutine>("_walkRoutine", __instance);
            Coroutine grabRoutine = Utils.GetField<GrowContainerBehaviour, Coroutine>("_grabRoutine", __instance);
            object performActionRoutine = Utils.GetField<GrowContainerBehaviour>("_performActionRoutine", __instance);

            if (walkRoutine != null)
            {
                __instance.StopCoroutine(walkRoutine);
                Utils.SetField<GrowContainerBehaviour>("_walkRoutine", __instance, null);
            }
            if (grabRoutine != null)
            {
                __instance.StopCoroutine(grabRoutine);
                Utils.SetField<GrowContainerBehaviour>("_grabRoutine", __instance, null);
            }
            if (performActionRoutine != null)
            {
                Utils.CallMethod<GrowContainerBehaviour>("OnStopPerformAction", __instance, []);
                Utils.StopCoroutine(performActionRoutine);
                Utils.SetField<GrowContainerBehaviour>("_performActionRoutine", __instance, null);
            }
            return false;
        }

        // PerformAction is inlined. Replace with original method body.
        [HarmonyPatch(typeof(GrowContainerBehaviour), "OnActiveTick")]
        [HarmonyPrefix]
        public static bool OnActiveTickPrefix(GrowContainerBehaviour __instance)
        {
            if (!InstanceFinder.IsServer)
            {
                return false;
            }

            // The EState enum is protected. Just use integers.
            int currentState = (int)Utils.GetProperty<GrowContainerBehaviour>("_currentState", __instance);
            GrowContainer growContainer = Utils.GetProperty<GrowContainerBehaviour, GrowContainer>("_growContainer", __instance);

            // EState.Idle
            if (currentState == 0)
            {
                if (growContainer == null)
                {
                    __instance.Disable_Networked(null);
                    return false;
                }
                if ((bool)Utils.CallMethod<GrowContainerBehaviour>("IsRequiredItemInInventory", __instance, [growContainer]))
                {
                    if ((bool)Utils.CallMethod<GrowContainerBehaviour>("IsAtGrowContainer", __instance, []))
                    {
                        // inlined
                        Utils.CallMethod<GrowContainerBehaviour>("PerformAction", __instance, []);
                        return false;
                    }
                    Utils.CallMethod<GrowContainerBehaviour>("WalkTo", __instance, [growContainer]);
                    return false;
                }
                else if ((bool)Utils.CallMethod<GrowContainerBehaviour>("DoSuppliesContainRequiredItem", __instance, [growContainer]))
                {
                    if ((bool)Utils.CallMethod<GrowContainerBehaviour>("IsAtSupplies", __instance, []))
                    {
                        Utils.CallMethod<GrowContainerBehaviour>("GrabRequiredItemFromSupplies", __instance, [growContainer]);
                        return false;
                    }
                    Botanist botanist = Utils.GetProperty<GrowContainerBehaviour, Botanist>("_botanist", __instance);
                    Utils.CallMethod<GrowContainerBehaviour>("WalkTo", __instance, [botanist.GetSuppliesAsTransitEntity()]);
                    return false;
                }
                else
                {
                    __instance.Disable_Networked(null);
                }
            }
            return false;
        }
    }


    // chemistry station patches
    [HarmonyPatch]
    public class ChemistryStationPatches
    {
        [HarmonyPatch(typeof(StationRecipeEntry), "AssignRecipe")]
        [HarmonyPostfix]
        public static void AssignRecipePostfix(StationRecipeEntry __instance, ref StationRecipe recipe)
        {
            if (!Utils.Mod.processedRecipes.Contains(recipe))
            {
                if (!Utils.Mod.originalRecipeTimes.ContainsKey(recipe))
                {
                    Utils.Mod.originalRecipeTimes[recipe] = __instance.Recipe.CookTime_Mins;
                }
                __instance.Recipe.CookTime_Mins = (int)((float)Utils.Mod.originalRecipeTimes[recipe] / Utils.Mod.GetStationSpeed("ChemistryStation"));
                Utils.Mod.processedRecipes.Add(__instance.Recipe);
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
            if (Utils.GetField<StartChemistryStationBehaviour>("cookRoutine", __instance) != null)
            {
                return false;
            }
            if (__instance.targetStation == null)
            {
                return false;
            }
            object workRoutine = Utils.StartCoroutine(StartCookRoutine(__instance));
            Utils.SetField<StartChemistryStationBehaviour>("cookRoutine", __instance, (Coroutine)workRoutine);

            return false;
        }

        // Coroutine with accelerated animations
        private static IEnumerator StartCookRoutine(StartChemistryStationBehaviour behaviour)
        {
            float stationSpeed = Utils.Mod.GetStationWorkSpeed("ChemistryStation");
            behaviour.Npc.Movement.FacePoint(behaviour.targetStation.transform.position, Mathf.Max(0.1f, 0.5f / stationSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / stationSpeed));

            behaviour.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", true);
            if (!(bool)Utils.CallMethod<StartChemistryStationBehaviour>("CanCookStart", behaviour, []))
            {
                Utils.CallMethod<StartChemistryStationBehaviour>("StopCook", behaviour, []);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            behaviour.targetStation.SetNPCUser(behaviour.Npc.NetworkObject);
            StationRecipe recipe = (Utils.CastTo<ChemistryStationConfiguration>(behaviour.targetStation.Configuration)).Recipe.SelectedRecipe;
            Utils.CallMethod<StartChemistryStationBehaviour>("SetupBeaker", behaviour, []);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / stationSpeed));

            Beaker beaker = Utils.GetField<StartChemistryStationBehaviour, Beaker>("beaker", behaviour);
            Utils.CallMethod<StartChemistryStationBehaviour>("FillBeaker", behaviour, [recipe, beaker]);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 20f / stationSpeed));

            ItemInstanceList list = new ItemInstanceList();
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                foreach (ItemDefinition itemDefinition in recipe.Ingredients[i].Items)
                {
                    StorableItemDefinition storableItemDefinition = Utils.CastTo<StorableItemDefinition>(itemDefinition);
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
            Utils.SetField<StartChemistryStationBehaviour>("beaker", behaviour, null);
            Utils.CallMethod<StartChemistryStationBehaviour>("StopCook", behaviour, []);
            behaviour.Deactivate_Networked(null);
            yield break;

        }


        [HarmonyPatch(typeof(StartChemistryStationBehaviour), "StopCook")]
        [HarmonyPrefix]
        public static bool StopCookPrefix(StartChemistryStationBehaviour __instance)
        {
            __instance.targetStation.SetNPCUser(null);
            __instance.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", false);
            object workRoutine = Utils.GetField<StartChemistryStationBehaviour>("cookRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartChemistryStationBehaviour>("cookRoutine", __instance, null);
            }

            return false;
        }
    }


    // cash patches
    [HarmonyPatch]
    public class CashPatches
    {
#if MONO_BUILD

        public static float GetCashStackLimit()
        {
            return Utils.Mod.GetStackLimit(EItemCategory.Cash);
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
            // Mod field isn't set yet by the time this executes, so use MelonLogger instead of Utils.Mod.LoggerInstance
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
                Utils.Warn($"Failed to patch method: {e.GetType().Name} - {e.Message}");
                Utils.Warn($"Source: {e.Source}");
                Utils.Warn($"{e.StackTrace}");
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
            int stackLimit = Utils.Mod.GetStackLimit(EItemCategory.Cash);

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
            int stackLimit = Utils.Mod.GetStackLimit(EItemCategory.Cash);

            CashInstance cashInstance = Utils.CastTo<CashInstance>(__instance.draggedSlot.assignedSlot.ItemInstance);
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
                            CashInstance cashInstance2 = Utils.CastTo<CashInstance>(itemSlot.ItemInstance);
                            if (cashInstance2 != null)
                            {
                                float num3;
                                if (Utils.Is<CashSlot>(itemSlot))
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
                            CashInstance cashInstance3 = Utils.CastTo<CashInstance>(Registry.GetItem("cash").GetDefaultInstance(1));
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
            Utils.CastTo<ItemUI_Cash>(__instance.draggedSlot.ItemUI).SetDisplayedBalance(cashInstance.Balance - __instance.draggedCashAmount);
            return false;
        }


        // This method has hardcoded constants, so we need to replace it completely
        [HarmonyPatch(typeof(ItemUIManager), "EndCashDrag")]
        [HarmonyPrefix]
        public unsafe static bool EndCashDragPrefix(ItemUIManager __instance)
        {
            int stackLimit = Utils.Mod.GetStackLimit(EItemCategory.Cash);
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
                        if (Utils.Is<CashSlot>(__instance.HoveredSlot.assignedSlot))
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




    
    [HarmonyPatch]
    public class NPCMovementPatches
    {
        // whoops, we broke things setting stoppingdistance to 0.4. set it back to its default value.
        [HarmonyPatch(typeof(NPCMovement), "Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(NPCMovement __instance)
        {
            __instance.Agent.stoppingDistance = 0.2f;
        }
    }

    [HarmonyPatch]
    public class SetDestinationPatches
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return AccessTools.FirstMethod(typeof(NPCMovement), (method) => 
                method.Name == "SetDestination" && method.GetParameters().Length == 5
            );
        }

        [HarmonyPatch]
        [HarmonyPrefix]
        public static bool SetDestinationPrefix(NPCMovement __instance)
        {
            float walkAcceleration = 1f;
            NPC npc = Utils.GetField<NPCMovement, NPC>("npc", __instance);
            if (Utils.Is<Employee>(npc))
            {
                walkAcceleration = Utils.Mod.employeeAnimation.GetEntry<float>("employeeWalkAcceleration").Value;
            }
            // Setting acceleration and angularspeed is necessary for employees to navigate
            // paths at high speed, but setting them too high makes motions unnecessarily jerky.
            // Multiplying by walkAcceleration^2 seems like a good balance.
            __instance.MoveSpeedMultiplier = walkAcceleration;
            __instance.Agent.acceleration = 20f * walkAcceleration * walkAcceleration;
            __instance.Agent.angularSpeed = 720f * walkAcceleration * walkAcceleration;

            return true;
        }

    }
}
