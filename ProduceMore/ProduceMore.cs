using HarmonyLib;
using MelonLoader;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Events;

#if MONO_BUILD
using FishNet;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.EntityFramework;
using ScheduleOne.GameTime;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Lighting;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.NPCs;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Packaging;
using ScheduleOne.Persistence;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Product;
using ScheduleOne.StationFramework;
using ScheduleOne.Trash;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Management;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.UI.Shop;
using ScheduleOne.UI.Stations.Drying_rack;
using ScheduleOne.UI.Stations;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using ScheduleOne;
using StringArray = string[];
using ItemInstanceList = System.Collections.Generic.List<ScheduleOne.ItemFramework.ItemInstance>;
using MixingStationList = System.Collections.Generic.List<ScheduleOne.ObjectScripts.MixingStation>;
using ChemStationList = System.Collections.Generic.List<ScheduleOne.ObjectScripts.ChemistryStation>;
#else
using Il2CppFishNet;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Growing;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Lighting;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Packaging;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Product.Packaging;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.StationFramework;
using Il2CppScheduleOne.Trash;
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
using StringArray = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStringArray;
using ItemInstanceList = Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.ItemFramework.ItemInstance>;
using MixingStationList = Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.ObjectScripts.MixingStation>;
using ChemStationList = Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.ObjectScripts.ChemistryStation>;
#endif

namespace ProduceMore
{
    public class Utils
    {
        public static ProduceMoreMod Mod;
        private static Assembly S1Assembly;

        public static void Initialize(ProduceMoreMod mod)
        {
            Mod = mod;
#if !MONO_BUILD
            S1Assembly = AppDomain.CurrentDomain.GetAssemblies().First((Assembly a) => a.GetName().Name == "Assembly-CSharp");
#endif
        }

        // Must wait to run until LoadManager has been loaded in scene.
        public static void LateInitialize()
        {
            LoadManager.Instance.onPreSceneChange.AddListener(Utils.ToUnityAction(OnPreSceneChange));
        }

        // Stop coroutines when a scene change is initiated.
        private static void OnPreSceneChange()
        {
            Mod.StopCoroutines();
        }

        // Reflection convenience methods.
        // Needed to access private members in mono.
        // Also handles the property-fying of fields in IL2CPP.
        // Treturn cannot be an interface type in IL2CPP; use ToInterface for that.

        public static Treturn GetField<Ttarget, Treturn>(string fieldName, object target) where Treturn : class
        {
            return (Treturn)GetField<Ttarget>(fieldName, target);
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

        public static Treturn GetProperty<Ttarget, Treturn>(string fieldName, object target)
        {
            return (Treturn)GetProperty<Ttarget>(fieldName, target);
        }

        public static object GetProperty<Ttarget>(string fieldName, object target)
        {
            return AccessTools.Property(typeof(Ttarget), fieldName).GetValue(target);
        }

        public static void SetProperty<Ttarget>(string fieldName, object target, object value)
        {
            AccessTools.Property(typeof(Ttarget), fieldName).SetValue(target, value);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target)
        {
            return (Treturn)CallMethod<Ttarget>(methodName, target, []);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target, object[] args)
        {
            return (Treturn)CallMethod<Ttarget>(methodName, target, args);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, Type[] argTypes, object target, object[] args)
        {
            return (Treturn)CallMethod<Ttarget>(methodName, argTypes, target, args);
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


        // Type checking and conversion methods

        // In IL2CPP, do a type check before performing a forced cast, returning default (usually null) on failure.
        // In Mono, do a type check before a regular cast, returning default on type check failure.
        // You can't use CastTo with T as an Il2Cpp interface; use ToInterface for that.
#if MONO_BUILD
        public static T CastTo<T>(object o)
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                return default(T);
            }
        }
#else
        public static T CastTo<T>(Il2CppObjectBase o) where T : Il2CppObjectBase
        {
            if (typeof(T).IsAssignableFrom(GetType(o)))
            {
                return (T)System.Activator.CreateInstance(typeof(T), [o.Pointer]);
            }
            return default(T);
        }
#endif

        // Under Il2Cpp, "is" operator only looks at local scope for type info,
        // instead of checking object identity. 
        // Check against actual object type obtained via GetType.
        // In Mono, use standard "is" operator.
        // Will always return false for Il2Cpp interfaces.
#if MONO_BUILD
        public static bool Is<T>(object o)
        {
            return o is T;
        }
#else
        public static bool Is<T>(Il2CppObjectBase o) where T : Il2CppObjectBase
        {
            return typeof(T).IsAssignableFrom(GetType(o));
        }
#endif

        // You can't cast to an interface type in IL2CPP, since interface info is stripped.
        // Use this method to perform a blind cast without type checking.
        // In Mono, just do a regular cast.
#if MONO_BUILD
        public static T ToInterface<T>(object o)
        {
            return (T)o;
        }
#else
        public static T ToInterface<T>(Il2CppObjectBase o) where T : Il2CppObjectBase
        {
            return (T)System.Activator.CreateInstance(typeof(T), [o.Pointer]);
        }
#endif

        // Get actual identity of Il2Cpp objects based on their ObjectClass, and
        // convert between Il2CppScheduleOne and ScheduleOne namespaces.
        // In Mono, return object.GetType or null.
#if MONO_BUILD
        public static Type GetType(object o)
        {
            if (o == null)
            {
                return null;
            }
            return o.GetType();
        }
#else
        public static Type GetType(Il2CppObjectBase o)
        {
            if (o == null)
            {
                return null;
            }

            string typeName = Il2CppType.TypeFromPointer(o.ObjectClass).FullName;
            return S1Assembly.GetType($"Il2Cpp{typeName}");
        }
#endif

        // Convert a System.Action to a unity action.
        public static UnityAction ToUnityAction(Action action)
        {
#if MONO_BUILD
            return new UnityAction(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction>(action);
#endif
        }

        public static UnityAction<T> ToUnityAction<T>(Action<T> action)
        {
#if MONO_BUILD
            return new UnityAction<T>(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction<T>>(action);
#endif
        }

        // Convert a delegate to a predicate that IL2CPP ienumerable functions can actually use.
#if MONO_BUILD
        public static Predicate<T> ToPredicate<T>(Func<T, bool> func)
        {
            return new Predicate<T>(func);
        }
#else
        public static Il2CppSystem.Predicate<T> ToPredicate<T>(Func<T, bool> func)
        {
            return DelegateSupport.ConvertDelegate<Il2CppSystem.Predicate<T>>(func);
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

        public static void PrintException(Exception e)
        {
            Warn($"Exception: {e.GetType().Name} - {e.Message}");
            Warn($"Source: {e.Source}");
            Warn($"{e.StackTrace}");
            if (e.InnerException != null)
            {
                Warn($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
                Warn($"Source: {e.InnerException.Source}");
                Warn($"{e.InnerException.StackTrace}");
                if (e.InnerException.InnerException != null)
                {
                    Warn($"Inner inner exception: {e.InnerException.InnerException.GetType().Name} - {e.InnerException.InnerException.Message}");
                    Warn($"Source: {e.InnerException.InnerException.Source}");
                    Warn($"{e.InnerException.InnerException.StackTrace}");
                }
            }
        }

        // Check if a particular MelonMod is loaded.
        public static bool OtherModIsLoaded(string modName)
        {
            List<MelonBase> registeredMelons = new List<MelonBase>(MelonBase.RegisteredMelons);
            MelonBase melon = registeredMelons.Find(new Predicate<MelonBase>(m => m.Info.Name == modName));
            return (melon != null);
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

        // Mod-specific helper functions

        // Start coroutine and add it to the list.
        public static object StartCoroutine(IEnumerator func)
        {
            if (func == null)
            {
                Utils.Warn($"Tried to start null coroutine.");
                return null;
            }
            object coroutine = MelonCoroutines.Start(func);
            Mod.runningCoroutines.Add(coroutine);
            return coroutine;
        }

        // Shut down a coroutine and try to remove it from the list.
        public static void StopCoroutine(object coroutine)
        {
            if (coroutine == null)
            {
                Warn($"Tried to stop null coroutine; is there a dead entry in the list? (length {Mod.runningCoroutines.Count})");
                return;
            }

            Mod.runningCoroutines.Remove(coroutine);

            try
            {
                MelonCoroutines.Stop(coroutine);
            }
            catch (Exception e)
            {
                Utils.PrintException(e);
            }
        }

        public static Dictionary<Type, string> typeStrings = new Dictionary<Type, string>() {
                { typeof(DryingRack), "DryingRack" },
                { typeof(LabOven), "LabOven" },
                { typeof(ChemistryStation), "ChemistryStation" },
                { typeof(MixingStationMk2), "MixingStationMk2" },
                { typeof(MixingStation), "MixingStation" },
                { typeof(BrickPress), "BrickPress" },
                { typeof(Cauldron), "Cauldron" },
                { typeof(PackagingStationMk2), "PackagingStationMk2" },
                { typeof(PackagingStation), "PackagingStation" },
                { typeof(Pot), "Pot" },
                { typeof(MushroomBed), "MushroomBed" },
                { typeof(MushroomSpawnStation), "SpawnStation" }
        };

        // Does NOT modify station capacity; handle that in patches
        public static void AddStationCapacity(GridItem station, int capacity)
        {
            if (station == null)
            {
                return;
            }
            string stationString = typeStrings.GetValueOrDefault(GetType(station));
            if (!Mod.processedStationCapacities.Contains(station))
            {
                if (!Mod.originalStationCapacities.ContainsKey(stationString))
                {
                    Mod.originalStationCapacities.Add(stationString, capacity);
                }
                Mod.processedStationCapacities.Add(station);
            }
        }

        // Does NOT modify station time; handle that in patches
        public static void AddOriginalStationTime(GridItem station, int time)
        {
            if (station == null)
            {
                return;
            }
            string stationString = typeStrings.GetValueOrDefault(GetType(station));
            if (!Mod.processedStationTimes.Contains(station))
            {
                if (!Mod.originalStationTimes.ContainsKey(stationString))
                {
                    Mod.originalStationTimes.Add(stationString, time);
                }
                Mod.processedStationTimes.Add(station);
            }
        }

        public static void AddOriginalStationTime(string stationString, int time)
        {
            if (!Mod.originalStationTimes.ContainsKey(stationString))
            {
                Mod.originalStationTimes.TryAdd(stationString, time);
            }
        }

        // Modifies item definition
        public static void ModItemStackLimit(ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return;
            }
            if (!Mod.processedItemDefs.Contains(itemDefinition) && itemDefinition.Name.ToLower() != "cash")
            {
                // Speed Grow is classified as product for some reason
                EItemCategory category;
                if (itemDefinition.Name == "Speed Grow")
                {
                    category = EItemCategory.Agriculture;
                }
                else
                {
                    category = itemDefinition.Category;
                }
                if (!Mod.originalStackLimits.ContainsKey(category.ToString()))
                {
                    Mod.originalStackLimits[category.ToString()] = itemDefinition.StackLimit;
                }

                itemDefinition.StackLimit = Mod.GetStackLimit(itemDefinition.GetDefaultInstance(1));
                Mod.processedItemDefs.Add(itemDefinition);
            }
        }

        // Modifies recipe definition
        public static void ModRecipeTime(StationRecipe recipe, float stationSpeed)
        {
            if (recipe == null)
            {
                return;
            }
            if (!Mod.processedRecipes.Contains(recipe))
            {
                if (!Mod.originalRecipeTimes.ContainsKey(recipe))
                {
                    Mod.originalRecipeTimes[recipe] = recipe.CookTime_Mins;
                }

                int originalTime = Mod.originalRecipeTimes[recipe];
                int newTime = Mathf.Max((int)((float)originalTime / stationSpeed), 1);
                recipe.CookTime_Mins = newTime;
                Mod.processedRecipes.Add(recipe);
            }
        }

        public static int GetStationCapacity(GridItem station)
        {
            if (station == null)
            {
                return 10;
            }
            return Mod.GetStationCapacity(typeStrings[GetType(station)]);
        }

        public static int GetStationCapacity(string stationString)
        {
            return Mod.GetStationCapacity(stationString);
        }

        public static float GetStationSpeed(GridItem station)
        {
            if (station == null)
            {
                return 1f;
            }
            return Mod.GetStationSpeed(typeStrings[GetType(station)]);
        }

        public static float GetStationSpeed(string stationString)
        {
            return Mod.GetStationSpeed(stationString);
        }

        public static float GetStationWorkSpeed(GridItem station)
        {
            if (station == null)
            {
                return 1f;
            }
            return Mod.GetStationWorkSpeed(typeStrings[GetType(station)]);
        }

        public static float GetStationWorkSpeed(string stationString)
        {
            return Mod.GetStationWorkSpeed(stationString);
        }

        public static int GetOriginalStationTime(GridItem station)
        {
            int time = 10;
            if (station != null)
            {
                string typeString = typeStrings[GetType(station)];
                if (Mod.originalStationTimes.ContainsKey(typeString))
                {
                    time = Mod.originalStationTimes[typeString];
                }
                else
                {
                    Utils.Warn($"Tried to get original station time for {typeString}, but there wasn't one in the index?");
                }
            }
            return time;
        }

        public static int GetOriginalStationTime(string stationString)
        {
            int time = 10;
            if (Mod.originalStationTimes.ContainsKey(stationString))
            {
                time = Mod.originalStationTimes[stationString];
            }
            else
            {
                Utils.Warn($"Tried to get original station time for {stationString}, but there wasn't one in the index?");
            }
                return time;
        }
    }

    // Set stack sizes
    [HarmonyPatch]
    public class ItemCapacityPatches
    {
        // Modify stack limit on item access
        // Inlined almost always.
        [HarmonyPatch(typeof(ItemInstance), "StackLimit", MethodType.Getter)]
        [HarmonyPrefix]
        public static void StackLimitPrefix(ItemInstance __instance)
        {
            Utils.ModItemStackLimit(__instance.Definition);
        }

        // For when we first encounter an itemdef via the phone delivery app
        [HarmonyPatch(typeof(ListingEntry), "Initialize")]
        [HarmonyPrefix]
        public static void InitializePrefix(ShopListing match)
        {
            if (match != null && match.Item != null)
            {
                Utils.ModItemStackLimit(match.Item);
            }
        }

        [HarmonyPatch(typeof(ItemSlotUI), "UpdateUI")]
        [HarmonyPostfix]
        public static void UpdateUIPostfix(ItemSlotUI __instance)
        {
            if (__instance.assignedSlot != null && __instance.assignedSlot.ItemInstance != null)
            {
                Utils.ModItemStackLimit(__instance.assignedSlot.ItemInstance.Definition);
            }
        }

        // update item slots to report accurate capacity.
        // also, don't return a negative value when stacks larger than the current stacklimit are at the destination.
        [HarmonyPatch(typeof(ItemSlot), "GetCapacityForItem")]
        [HarmonyPostfix]
        public static void GetCapacityForItemPostfix(ItemSlot __instance, ref int __result, ItemInstance item, bool checkPlayerFilters)
        {
            if (item != null)
            {
                Utils.ModItemStackLimit(item.Definition);
            }

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
                __result = Mathf.Max(item.StackLimit - __instance.Quantity, 0);
                return;
            }
            __result = 0;
            return;
        }

        [HarmonyPatch(typeof(NPCInventory), "GetCapacityForItem")]
        [HarmonyPrefix]
        public static void NPCGetCapacityForItemPrefix(NPCInventory __instance, ItemInstance item, ref int __result)
        {
            if (item != null)
            {
                Utils.ModItemStackLimit(item.Definition);
            }
        }

        // Don't return a negative value if a stack larger than the stacklimit is in NPC inventory.
        [HarmonyPatch(typeof(NPCInventory), "GetCapacityForItem")]
        [HarmonyPostfix]
        public static void NPCGetCapacityForItemPostfix(NPCInventory __instance, ItemInstance item, ref int __result)
        {
            if (item == null)
            {
                return;
            }

            int num = 0;
            for (int i = 0; i < __instance.ItemSlots.Count; i++)
            {
                if (__instance.ItemSlots[i] != null && !__instance.ItemSlots[i].IsLocked && !__instance.ItemSlots[i].IsAddLocked)
                {
                    if (__instance.ItemSlots[i].ItemInstance == null)
                    {
                        num += item.StackLimit;
                    }
                    else if (__instance.ItemSlots[i].ItemInstance.CanStackWith(item, true))
                    {
                        num += Mathf.Max(0, item.StackLimit - __instance.ItemSlots[i].ItemInstance.Quantity);
                    }
                }
            }
            __result = num;
        }

        // Call to GetCapacityForItem seems to have been optimized out.
        // Replace with original method body.
        [HarmonyPatch(typeof(NPCInventory), "CanItemFit")]
        [HarmonyPostfix]
        public static void NPCCanItemFitPostfix(NPCInventory __instance, ItemInstance item, ref bool __result)
        {
            __result = item != null && __instance.GetCapacityForItem(item) >= item.Quantity;
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
            if (__result != null)
            {
                Utils.ModItemStackLimit(__result);
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
            // Default capacity of drying rack not available as a constant.
            Utils.AddStationCapacity(rack, 20);
            rack.ItemCapacity = Utils.GetStationCapacity(rack);
        }

        // Modify DryingRack.ItemCapacity
        // canstartoperation runs every time a player or npc tries to interact
        [HarmonyPatch(typeof(DryingRack), "CanStartOperation")]
        [HarmonyPostfix]
        public static void CanStartOperationPostfix(DryingRack __instance, ref bool __result)
        {
            // Default capacity of drying rack not available as a constant.
            Utils.AddStationCapacity(__instance, 20);
            __instance.ItemCapacity = Utils.GetStationCapacity(__instance);

            __result = __instance.GetTotalDryingItems() < __instance.ItemCapacity &&
                __instance.InputSlot.Quantity != 0 &&
                !__instance.InputSlot.IsLocked &&
                !__instance.InputSlot.IsRemovalLocked;
        }

        // fix drying operation progress meter
        [HarmonyPatch(typeof(DryingOperationUI), "UpdatePosition")]
        [HarmonyPostfix]
        public static void UpdatePositionPostfix(DryingOperationUI __instance)
        {
            DryingRack rack = Singleton<DryingRackCanvas>.Instance.Rack;
            if (rack != null)
            {
                Utils.AddOriginalStationTime(rack, DryingRack.DRY_MINS_PER_TIER);
            }

            int originalDryingTime = DryingRack.DRY_MINS_PER_TIER;
            float stationSpeed = Utils.GetStationSpeed("DryingRack");
            float dryingTime = (float)originalDryingTime / stationSpeed;
            float t = Mathf.Clamp01((float)__instance.AssignedOperation.Time / dryingTime);
            int num = Mathf.Clamp((int)dryingTime - __instance.AssignedOperation.Time, 0, (int)dryingTime);
            int num2 = num / 60;
            int num3 = num % 60;
            __instance.Tooltip.text = num2.ToString() + "h " + num3.ToString() + "m until next tier";
            float num4 = -62.5f;
            float b = -num4;
            __instance.Rect.anchoredPosition = new Vector2(Mathf.Lerp(num4, b, t), 0f);
        }

        // speed
        [HarmonyPatch(typeof(DryingOperation), "GetQuality")]
        [HarmonyPostfix]
        public static void GetQualityPostfix(DryingOperation __instance, ref EQuality __result)
        {
            Utils.AddOriginalStationTime("DryingRack", DryingRack.DRY_MINS_PER_TIER);

            int originalTime = DryingRack.DRY_MINS_PER_TIER;
            float stationSpeed = Utils.Mod.GetStationSpeed("DryingRack");
            int dryingTime = (int)((float)originalTime / stationSpeed);
            if (__instance.Time >= dryingTime)
            {
                __result = __instance.StartQuality + 1;
                return;
            }
            __result = __instance.StartQuality;
        }

        [HarmonyPatch(typeof(DryingRack), "MinPass")]
        [HarmonyPostfix]
        public static void MinPassPostfix(DryingRack __instance)
        {
            int originalStationTime = DryingRack.DRY_MINS_PER_TIER;
            float stationSpeed = Utils.GetStationSpeed(__instance);
            int dryingTime = (int)((float)originalStationTime / stationSpeed);

            // TODO: fix logic for work speed < 1
            foreach (DryingOperation dryingOperation in __instance.DryingOperations.ToArray())
            {
                // Don't increment dryingOperation.Time; it's already been incremented by original fn
                if (dryingOperation.Time >= dryingTime)
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
            return;
        }

        // call our own coroutine to speed up employees hanging up leaves
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

            object workRoutine = Utils.StartCoroutine(BeginActionCoroutine(__instance));
            Utils.SetField<StartDryingRackBehaviour>("workRoutine", __instance, (Coroutine)workRoutine);
            return false;
        }

        // Replacement coroutine for BeginAction
        private static IEnumerator BeginActionCoroutine(StartDryingRackBehaviour behaviour)
        {
            yield return new WaitForEndOfFrame();

            float workSpeed = Utils.GetStationWorkSpeed(behaviour.Rack);
            float originalTimePerItem = StartDryingRackBehaviour.TIME_PER_ITEM;
            float waitTimePerItem = originalTimePerItem / workSpeed;
            int inputQuantity = behaviour.Rack.InputSlot.Quantity;
            int outputCapacity = behaviour.Rack.ItemCapacity - behaviour.Rack.GetTotalDryingItems();
            int quantityToDry = Mathf.Min(inputQuantity, outputCapacity);
            float waitTime = Mathf.Max(0.1f, waitTimePerItem * quantityToDry);

            // So for the grabitem animation, there's not a simple way to make it repeat like other animations.
            // Instead, we have to trigger the animation every second.
            int lastWholeSecond = 0;
            behaviour.Npc.Avatar.Animation.SetTrigger("GrabItem");
            for (float i = 0f; i < waitTime; i += Time.deltaTime)
            {
                // Look at the rack.
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Rack.uiPoint.position, 0, false);

                // if a second passed, trigger grabitem again.
                if (i - (float)lastWholeSecond >= 1f)
                {
                    behaviour.Npc.Avatar.Animation.SetTrigger("GrabItem");
                    lastWholeSecond = (int)i;
                }
                yield return null;
            }
            behaviour.Npc.Avatar.Animation.ResetTrigger("GrabItem");

            if (InstanceFinder.IsServer)
            {
                behaviour.Rack.StartOperation();
            }
            Utils.SetProperty<StartDryingRackBehaviour>("WorkInProgress", behaviour, false);
            object workRoutine = Utils.GetField<StartDryingRackBehaviour>("workRoutine", behaviour);
            Utils.StopCoroutine(workRoutine);
            Utils.SetField<StartDryingRackBehaviour>("workRoutine", behaviour, null);
            yield break;
        }

        // Kill our coroutine and remove it from the list
        [HarmonyPatch(typeof(StartDryingRackBehaviour), "StopCauldron")]
        [HarmonyPrefix]
        public static void StopCauldronPrefix(StartDryingRackBehaviour __instance)
        {
            object workRoutine = Utils.GetField<StartDryingRackBehaviour>("workRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartDryingRackBehaviour>("workRoutine", __instance, null);
            }
            Utils.SetProperty<StartDryingRackBehaviour>("WorkInProgress", __instance, false);

            return;
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
            StorableItemDefinition ingredient = __instance.Ingredient;
            int originalCookTime = ingredient.StationItem.GetModule<CookableModule>().CookTime;
            float stationSpeed = Utils.Mod.GetStationSpeed("LabOven");
            int newCookTime = (int)((float)originalCookTime / stationSpeed);
            __result = newCookTime;
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
            ItemInstance productInstance = __instance.Oven.CurrentOperation.GetProductItem(1);
            int quantityProduced = __instance.Oven.CurrentOperation.Cookable.ProductQuantity;
            int capacity = __instance.Oven.OutputSlot.GetCapacityForItem(productInstance, false);
            __result = capacity >= quantityProduced;
        }

        // Call our own startcook coroutine with accelerated animations
        [HarmonyPatch(typeof(StartLabOvenBehaviour), "RpcLogic___StartCook_2166136261")]
        [HarmonyPrefix]
        public static bool Rpc_StartCookPrefix(StartLabOvenBehaviour __instance)
        {
            if (Utils.GetField<StartLabOvenBehaviour>("cookRoutine", __instance) != null || __instance.targetOven == null)
            {
                return false;
            }
            object workRoutine = Utils.StartCoroutine(StartCookCoroutine(__instance));
            Utils.SetField<StartLabOvenBehaviour>("cookRoutine", __instance, (Coroutine)workRoutine);
            return false;
        }

        // Startcook coroutine with accelerated animations
        private static IEnumerator StartCookCoroutine(StartLabOvenBehaviour behaviour)
        {
            float workSpeed = Utils.GetStationWorkSpeed(behaviour.targetOven);
            behaviour.targetOven.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.targetOven.transform.position, Mathf.Max(0.1f, 0.5f / workSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / workSpeed));

            if (!(bool)Utils.CallMethod<StartLabOvenBehaviour>("CanCookStart", behaviour))
            {
                Utils.CallMethod<StartLabOvenBehaviour>("StopCook", behaviour);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            behaviour.targetOven.Door.SetPosition(1f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / workSpeed));

            behaviour.targetOven.WireTray.SetPosition(1f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 5f / workSpeed));

            behaviour.targetOven.Door.SetPosition(0f);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / workSpeed));

            ItemInstance itemInstance = behaviour.targetOven.IngredientSlot.ItemInstance;
            if (itemInstance == null)
            {
                Utils.CallMethod<StartLabOvenBehaviour>("StopCook", behaviour);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            int num = 1;
            StorableItemDefinition storableItemDef = Utils.CastTo<StorableItemDefinition>(itemInstance.Definition);
            CookableModule cookable = storableItemDef.StationItem.GetModule<CookableModule>();
            if (cookable.CookType == CookableModule.ECookableType.Solid)
            {
                num = Mathf.Min(behaviour.targetOven.IngredientSlot.Quantity, LabOven.SOLID_INGREDIENT_COOK_LIMIT);
            }
            itemInstance.ChangeQuantity(-num);
            string productId = cookable.Product.ID;
            EQuality ingredientQuality = EQuality.Standard;
            if (Utils.Is<QualityItemInstance>(itemInstance))
            {
                ingredientQuality = Utils.CastTo<QualityItemInstance>(itemInstance).Quality;
            }
            behaviour.targetOven.SendCookOperation(new OvenCookOperation(itemInstance.ID, ingredientQuality, num, productId));
            Utils.CallMethod<StartLabOvenBehaviour>("StopCook", behaviour);
            behaviour.Deactivate_Networked(null);
            yield break;
        }

        [HarmonyPatch(typeof(StartLabOvenBehaviour), "StopCook")]
        [HarmonyPrefix]
        public static void StopCookPrefix(StartLabOvenBehaviour __instance)
        {
            object workRoutine = Utils.GetField<StartLabOvenBehaviour>("cookRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartLabOvenBehaviour>("cookRoutine", __instance, null);
            }
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
            float stationSpeed = Utils.GetStationWorkSpeed(behaviour.targetOven);
            behaviour.targetOven.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.Movement.FacePoint(behaviour.targetOven.transform.position, Mathf.Max(0.1f, 0.5f / stationSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / stationSpeed));

            if (!(bool)Utils.CallMethod<FinishLabOvenBehaviour>("CanActionStart", behaviour))
            {
                Utils.CallMethod<FinishLabOvenBehaviour>("StopAction", behaviour);
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
            Utils.CallMethod<FinishLabOvenBehaviour>("StopAction", behaviour);
            behaviour.Deactivate_Networked(null);
            yield break;
        }

        // Kill our coroutine and remove it from the list
        [HarmonyPatch(typeof(FinishLabOvenBehaviour), "StopAction")]
        [HarmonyPrefix]
        public static void StopActionPrefix(FinishLabOvenBehaviour __instance)
        {
            object workRoutine = Utils.GetField<FinishLabOvenBehaviour>("actionRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<FinishLabOvenBehaviour>("actionRoutine", __instance, null);
            }
        }
    }

    // Patch mixing station capacity and speed
    [HarmonyPatch]
    public class MixingStationPatches
    {
        // capacity
        [HarmonyPatch(typeof(MixingStation), "GetMixQuantity")]
        [HarmonyPostfix]
        public static void GetMixQuantityPostfix(MixingStation __instance, ref int __result)
        {
            Utils.AddStationCapacity(__instance, __instance.MaxMixQuantity);
            __instance.MaxMixQuantity = Utils.GetStationCapacity(__instance);

            if (__instance.GetProduct() == null || __instance.GetMixer() == null)
            {
                __result = 0;
                return;
            }
            __result = Mathf.Min([__instance.ProductSlot.Quantity, __instance.MixerSlot.Quantity, __instance.MaxMixQuantity]);
        }

        // actual call to GetMixQuantity seems to have been optimized out.
        [HarmonyPatch(typeof(MixingStation), "CanStartMix")]
        [HarmonyPostfix]
        public static void CanStartMixPostfix(MixingStation __instance, ref bool __result)
        {
            // re-insert original method body.
            __result = __instance.GetMixQuantity() > 0 && __instance.OutputSlot.Quantity == 0;
        }

        // actual call to GetMixTime seems to have been optimized out.
        [HarmonyPatch(typeof(MixingStation), "IsMixingDone", MethodType.Getter)]
        [HarmonyPostfix]
        public static void IsMixingDonePostfix(MixingStation __instance, ref bool __result)
        {
            // re-insert original method body.
            __result = __instance.CurrentMixOperation != null && __instance.CurrentMixTime >= __instance.GetMixTimeForCurrentOperation();
        }

        // speed
        [HarmonyPatch(typeof(MixingStation), "GetMixTimeForCurrentOperation")]
        [HarmonyPostfix]
        public static void GetMixTimePostfix(MixingStation __instance, ref int __result)
        {
            if (__instance.CurrentMixOperation == null)
            {
                return;
            }

            float stationSpeed = Utils.GetStationSpeed(__instance);
            int originalTimePerItem = __instance.MixTimePerItem;
            int quantity = __instance.CurrentMixOperation.Quantity;
            float mixTimePerItem = (float)originalTimePerItem / stationSpeed;
            float totalMixTime = mixTimePerItem * (float)quantity;

            // Returning < 1 breaks mix completion logic.
            __result = (int)Mathf.Max(1f, totalMixTime);
            return;
        }

        [HarmonyPatch(typeof(Chemist), "GetMixStationsReadyToMove")]
        [HarmonyPostfix]
        public static void GetMixStationsReadyToMovePostfix(Chemist __instance, ref MixingStationList __result)
        {
            var list = new MixingStationList();

            foreach (MixingStation mixingStation in Utils.GetProperty<Chemist, ChemistConfiguration>("configuration", __instance).MixStations)
            {
                ItemSlot outputSlot = mixingStation.OutputSlot;
                MixingStationConfiguration configuration = Utils.GetProperty<MixingStation, MixingStationConfiguration>("stationConfiguration", mixingStation);
                BuildableItem destination = configuration.Destination.SelectedObject;
                if (outputSlot.Quantity != 0 && __instance.MoveItemBehaviour.IsTransitRouteValid(configuration.DestinationRoute, outputSlot.ItemInstance.ID))
                {
                    // Only deliver to packaging stations with at least half a stack of space in input slot.
                    // Prevents chemists from running back and forth constantly to only carry a few items.
                    if (Utils.Is<PackagingStation>(destination) || Utils.Is<PackagingStationMk2>(destination))
                    {
                        PackagingStation station = Utils.CastTo<PackagingStation>(destination);
                        if (station.ProductSlot.ItemInstance == null)
                        {
                            list.Add(mixingStation);
                        }
                        else if (station.ProductSlot.ItemInstance.CanStackWith(outputSlot.ItemInstance))
                        {
                            int inputStackLimit = station.ProductSlot.ItemInstance.StackLimit;
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
        }

        [HarmonyPatch(typeof(StartMixingStationBehaviour), "StartCook")]
        [HarmonyPrefix]
        public static void StartCookPrefix(StartMixingStationBehaviour __instance)
        {
            // Default station batch size is not available as a constant.
            if (Utils.Is<MixingStationMk2>(__instance.targetStation))
            {
                Utils.AddStationCapacity(__instance.targetStation, 20);
            }
            else if (Utils.Is<MixingStation>(__instance.targetStation))
            {
                Utils.AddStationCapacity(__instance.targetStation, 10);
            }
            __instance.targetStation.MaxMixQuantity = Utils.GetStationCapacity(__instance.targetStation);
        }

        // GetMixTimeForCurrentOperation seems to have been optimized out.
        [HarmonyPatch(typeof(MixingStation), "MinPass")]
        [HarmonyPostfix]
        public static void MinPassPostfix(MixingStation __instance)
        {
            if (__instance.CurrentMixOperation != null)
            {
                // Complete mixing operation if needed
                int totalMixTime = __instance.GetMixTimeForCurrentOperation();
                if (__instance.CurrentMixTime == totalMixTime && InstanceFinder.IsServer)
                {
                    NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Mixing_Operations_Completed", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Mixing_Operations_Completed") + 1f).ToString(), true);
                    __instance.MixingDone_Networked();
                }

                // Display correct time remaining
                if (__instance.Clock != null)
                {
                    __instance.Clock.SetScreenLit(true);
                    __instance.Clock.DisplayMinutes(Mathf.Max(totalMixTime - __instance.CurrentMixTime, 0));
                }
            }

            // Fix the blinky light
            if (__instance.Light != null)
            {
                if (__instance.OutputSlot.Quantity > 0)
                {
                    __instance.Light.isOn = (int)Time.timeSinceLevelLoad % 2 == 0;
                    return;
                }
                if (__instance.CurrentMixOperation != null)
                {
                    __instance.Light.isOn = true;
                    return;
                }
                __instance.Light.isOn = false;
            }
        }

        [HarmonyPatch(typeof(MixingStationMk2), "UpdateScreen")]
        [HarmonyPostfix]
        public static void UpdateScreenPostfix(MixingStationMk2 __instance)
        {
            int totalMixTime = __instance.GetMixTimeForCurrentOperation();
            __instance.ProgressLabel.text = $"{totalMixTime - __instance.CurrentMixTime} mins remaining";
        }

        // start our accelerated mix coroutine
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
            float workSpeed = Utils.GetStationWorkSpeed(behaviour.targetStation);

            behaviour.Npc.Movement.FacePoint(behaviour.targetStation.transform.position, Mathf.Max(0.1f, 0.5f / workSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / workSpeed));

            if (!(bool)Utils.CallMethod<StartMixingStationBehaviour>("CanCookStart", behaviour))
            {
                Utils.CallMethod<StartMixingStationBehaviour>("StopCook", behaviour);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            behaviour.targetStation.SetNPCUser(behaviour.Npc.NetworkObject);
            behaviour.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", true);

            int originalMixTimePerItem = behaviour.targetStation.MixTimePerItem;
            float mixTimePerItem = (float)originalMixTimePerItem / workSpeed;
            int mixQuantity = behaviour.targetStation.GetMixQuantity();
            float totalMixTime = Mathf.Max(0.1f, mixTimePerItem * (float)mixQuantity);
            for (float i = 0f; i < totalMixTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.targetStation.uiPoint.position, 0, false);
                yield return null;
            }

            QualityItemInstance product = Utils.CastTo<QualityItemInstance>(behaviour.targetStation.ProductSlot.ItemInstance);
            ItemInstance mixer = behaviour.targetStation.MixerSlot.ItemInstance;
            if (InstanceFinder.IsServer)
            {
                behaviour.targetStation.ProductSlot.ChangeQuantity(-mixQuantity, false);
                behaviour.targetStation.MixerSlot.ChangeQuantity(-mixQuantity, false);
                MixOperation operation = new MixOperation(product.ID, product.Quality, mixer.ID, mixQuantity);
                behaviour.targetStation.SendMixingOperation(operation, 0);
            }

            Utils.CallMethod<StartMixingStationBehaviour>("StopCook", behaviour);
            behaviour.Deactivate_Networked(null);
            yield break;
        }
        
        // Kill our coroutine and remove it from the list
        [HarmonyPatch(typeof(StartMixingStationBehaviour), "StopCook")]
        [HarmonyPrefix]
        public static void StopCookPrefix(StartMixingStationBehaviour __instance)
        {
            object workRoutine = Utils.GetField<StartMixingStationBehaviour>("startRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartMixingStationBehaviour>("startRoutine", __instance, null);
            }
        }

        // increase max mixing start threshold and set station's max mix quantity
        [HarmonyPatch(typeof(MixingStationUIElement), "Initialize")]
        [HarmonyPrefix]
        public static void InitializeUIPrefix(MixingStation station)
        {
            int stationCapacity = Utils.GetStationCapacity(station);
            Utils.GetProperty<MixingStation, MixingStationConfiguration>("stationConfiguration", station).StartThrehold.Configure(1f, stationCapacity, true);
            station.MaxMixQuantity = stationCapacity;
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
            Utils.AddOriginalStationTime(behaviour.Press, (int)BrickPressBehaviour.BASE_PACKAGING_TIME);
            yield return new WaitForEndOfFrame();

            behaviour.Npc.Avatar.Animation.SetBool("UsePackagingStation", true);

            float workSpeed = Utils.GetStationWorkSpeed(behaviour.Press);
            float originalDuration = Utils.GetOriginalStationTime(behaviour.Press);
            float employeeMultiplier = Utils.CastTo<Packager>(behaviour.Npc).PackagingSpeedMultiplier;
            float duration = originalDuration / (employeeMultiplier * workSpeed);
            for (float i = 0f; i < duration; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Press.uiPoint.position, 0, false);
                yield return null;
            }

            behaviour.Npc.Avatar.Animation.SetBool("UsePackagingStation", false);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.2f / workSpeed));

            behaviour.Npc.Avatar.Animation.SetTrigger("GrabItem");
            behaviour.Press.PlayPressAnim();
            yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / workSpeed));

            ProductItemInstance product;
            if (InstanceFinder.IsServer && behaviour.Press.HasSufficientProduct(out product))
            {
                behaviour.Press.CompletePress(product);
            }
            Utils.SetProperty<BrickPressBehaviour>("PackagingInProgress", behaviour, false);
            object workRoutine = Utils.GetField<BrickPressBehaviour>("packagingRoutine", behaviour);
            Utils.StopCoroutine(workRoutine);
            Utils.SetField<BrickPressBehaviour>("packagingRoutine", behaviour, null);
            yield break;
        }

        // gracefully stop meloncoroutine
        [HarmonyPatch(typeof(BrickPressBehaviour), "StopPackaging")]
        [HarmonyPrefix]
        public static void StopPackagingPrefix(BrickPressBehaviour __instance)
        {
            object workRoutine = Utils.GetField<BrickPressBehaviour>("packagingRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<BrickPressBehaviour>("packagingRoutine", __instance, null);
            }
            Utils.SetField<BrickPressBehaviour>("PackagingInProgress", __instance, false);
        }
    }

    // cauldron patches
    [HarmonyPatch]
    public class CauldronPatches
    {
        // speed
        [HarmonyPatch(typeof(Cauldron), "StartCookOperation")]
        [HarmonyPrefix]
        public static void StartCookOperationPrefix(Cauldron __instance, ref int remainingCookTime)
        {
            // Base cauldron time unfortunately not available as a constant
            Utils.AddOriginalStationTime(__instance, 360);
            int cookTime = Utils.GetOriginalStationTime(__instance);
            float stationSpeed = Utils.GetStationSpeed(__instance);

            // Cook time must be at least 1 second, otherwise cauldrons break entirely.
            int newCookTime = (int)Mathf.Max((float)cookTime / stationSpeed, 1f);
            __instance.CookTime = newCookTime;
            remainingCookTime = newCookTime;
        }

        // accurately calculate output space instead of assuming limit of 10
        [HarmonyPatch(typeof(Cauldron), "HasOutputSpace")]
        [HarmonyPostfix]
        public static void HasOutputSpacePostfix(Cauldron __instance, ref bool __result)
        {
            QualityItemInstance outputInstance = Utils.CastTo<QualityItemInstance>(__instance.CocaineBaseDefinition.GetDefaultInstance());
            outputInstance.Quality = __instance.InputQuality;

            // 10 is batch size, unfortunately not available as a constant.
            __result = __instance.OutputSlot.GetCapacityForItem(outputInstance, false) >= 10;
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
            float stationSpeed = Utils.GetStationWorkSpeed(behaviour.Station);
            float originalWorkTime = StartCauldronBehaviour.START_CAULDRON_TIME;
            float workSpeed = Utils.GetStationWorkSpeed(behaviour.Station);
            float workTime = Mathf.Max(0.1f, originalWorkTime / workSpeed);
            for (float i = 0f; i < workTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Station.LinkOrigin.position, 0, false);
                yield return null;
            }
            behaviour.Npc.Avatar.Animation.SetBool("UseChemistryStation", false);
            if (InstanceFinder.IsServer)
            {
                EQuality quality = behaviour.Station.RemoveIngredients();
                behaviour.Station.StartCookOperation(null, behaviour.Station.CookTime, quality);
            }

            Utils.SetProperty<StartCauldronBehaviour>("WorkInProgress", behaviour, false);
            object workRoutine = Utils.GetField<StartCauldronBehaviour>("workRoutine", behaviour);
            Utils.StopCoroutine(workRoutine);
            Utils.SetField<StartCauldronBehaviour>("workRoutine", behaviour, null);
            yield break;
        }

        // Kill our coroutine and remove it from the list
        [HarmonyPatch(typeof(StartCauldronBehaviour), "StopCauldron")]
        [HarmonyPrefix]
        public static void StopCauldronPrefix(StartCauldronBehaviour __instance)
        {
            object workRoutine = Utils.GetField<StartCauldronBehaviour>("workRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartCauldronBehaviour>("workRoutine", __instance, null);
            }
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

            ProductItemInstance packagedInstance = Utils.CastTo<ProductItemInstance>(behaviour.Station.ProductSlot.ItemInstance.GetCopy());
            PackagingDefinition packagingDefinition = Utils.CastTo<PackagingDefinition>(behaviour.Station.PackagingSlot.ItemInstance.Definition);
            packagedInstance.SetPackaging(packagingDefinition);

            float workSpeed = Utils.GetStationWorkSpeed(behaviour.Station);
            int outputSpace = behaviour.Station.OutputSlot.GetCapacityForItem(packagedInstance);
            int productPerPackage = packagingDefinition.Quantity;
            int productQuantity = behaviour.Station.ProductSlot.Quantity;
            int packagingQuantity = behaviour.Station.PackagingSlot.Quantity;
            int numPackages = Mathf.Min([packagingQuantity, productQuantity / productPerPackage, outputSpace]);

            // leave the last package to PackSingleInstance
            int numBatchPackages = numPackages - 1;

            if (numPackages > 0)
            {
                float basePackagingTime = PackagingStationBehaviour.BASE_PACKAGING_TIME;
                float employeeMultiplier = Utils.CastTo<Packager>(behaviour.Npc).PackagingSpeedMultiplier;
                float stationMultiplier = behaviour.Station.PackagerEmployeeSpeedMultiplier;
                float packageTime = Mathf.Max(0.1f, (float)numPackages * basePackagingTime / (stationMultiplier * employeeMultiplier * workSpeed));
                for (float i = 0f; i < packageTime; i += Time.deltaTime)
                {
                    behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Station.Container.position, 0, false);
                    yield return null;
                }

                if (InstanceFinder.IsServer)
                {
                    if (numBatchPackages > 0)
                    {
                        // Add to packagedproductcount. Minus one package, because packsingleinstance also increments this variable.
                        float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("PackagedProductCount");
                        NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("PackagedProductCount", (value + numBatchPackages * productPerPackage).ToString(), true);

                        if (behaviour.Station.OutputSlot.ItemInstance == null)
                        {
                            behaviour.Station.OutputSlot.SetStoredItem(packagedInstance, false);
                            behaviour.Station.OutputSlot.SetQuantity(numBatchPackages);
                        }
                        else
                        {
                            behaviour.Station.OutputSlot.ChangeQuantity(numBatchPackages, false);
                        }
                        behaviour.Station.PackagingSlot.ChangeQuantity(-numBatchPackages, false);
                        behaviour.Station.ProductSlot.ChangeQuantity(-numBatchPackages * productPerPackage, false);
                    }

                    // Make sure PackSingleInstance is called at least once--AutoRestock hooks it
                    behaviour.Station.PackSingleInstance();
                }
            }

            behaviour.Npc.Avatar.Animation.SetBool("UsePackagingStation", false);
            Utils.SetProperty<PackagingStationBehaviour>("PackagingInProgress", behaviour, false);
            object workRoutine = Utils.GetField<PackagingStationBehaviour>("packagingRoutine", behaviour);
            Utils.StopCoroutine(workRoutine);
            Utils.SetField<PackagingStationBehaviour>("packagingRoutine", behaviour, null);
            yield break;
        }

        // stop our coroutine cleanly and remove it from the list
        [HarmonyPatch(typeof(PackagingStationBehaviour), "StopPackaging")]
        [HarmonyPrefix]
        public static void StopPackagingPrefix(PackagingStationBehaviour __instance)
        {
            object workRoutine = Utils.GetField<PackagingStationBehaviour>("packagingRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<PackagingStationBehaviour>("packagingRoutine", __instance, null);
            }
        }
    }

    [HarmonyPatch]
    public class GrowablePatches
    {
        // plant speed
        // due to inlining, this is really the only place we can hook.
        // unfortunately, GetAverageLightExposure is expensive, and we don't want to call it more than once per frame.
        // use a destructive prefix for performance reasons.
        [HarmonyPatch(typeof(Plant), "MinPass")]
        [HarmonyPrefix]
        public static bool MinPassPrefix(Plant __instance, int mins)
        {
            if (NetworkSingleton<TimeManager>.Instance.IsEndOfDay && !Utils.Mod.plantsAlwaysGrowPresent)
            {
                return false;
            }

            float num = 1f / ((float)__instance.GrowthTime * 60f) * (float)mins;
            num *= __instance.Pot.GetTemperatureGrowthMultiplier();
            float num2;
            num *= GetAverageLightExposure(__instance.Pot, out num2);
            num *= __instance.Pot.GrowSpeedMultiplier;
            num *= num2;
            if (GameManager.IS_TUTORIAL)
            {
                num *= 0.3f;
            }
            if (__instance.Pot.NormalizedMoistureAmount <= 0f)
            {
                num *= 0f;
            }

            float growthSpeed = Utils.Mod.GetStationSpeed("Pot");
            float lastProgress = __instance.NormalizedGrowthProgress;
            float percentage = Mathf.Clamp01(lastProgress + (num * growthSpeed));
            __instance.SetNormalizedGrowthProgress(percentage);

            return false;
        }

        // This function has been optimized out completely--vtable entry is blank??
        // Call a local copy
        // TODO: this function is expensive. cache light exposure per tile, and add listeners to Tile.onTileChanged.
        private static float GetAverageLightExposure(GrowContainer container, out float growSpeedMultiplier)
        {
            growSpeedMultiplier = 1f;
            UsableLightSource lightSourceOverride = Utils.GetField<GrowContainer, UsableLightSource>("_lightSourceOverride", container);
            if (lightSourceOverride != null)
            {
                return lightSourceOverride.GrowSpeedMultiplier;
            }
            float num = 0f;
            for (int i = 0; i < container.CoordinatePairs.Count; i++)
            {
                float num2;
                num += container.OwnerGrid.GetTile(container.CoordinatePairs[i].coord2).LightExposureNode.GetTotalExposure(out num2);
                growSpeedMultiplier += num2;
            }
            growSpeedMultiplier /= (float)container.CoordinatePairs.Count;
            return num / (float)container.CoordinatePairs.Count;
        }

        // shroom speed
        // for performance reasons, do a destructive prefix.
        [HarmonyPatch(typeof(ShroomColony), "OnMinPass")]
        [HarmonyPrefix]
        public static bool OnMinPassPostfix(ShroomColony __instance)
        {
            if (NetworkSingleton<TimeManager>.Instance.IsEndOfDay && !Utils.Mod.plantsAlwaysGrowPresent)
            {
                return false;
            }
            float currentGrowthRate = (float)Utils.CallMethod<ShroomColony>("GetCurrentGrowthRate", __instance);
            float stationSpeed = Utils.Mod.GetStationSpeed("MushroomBed");
            int originalGrowTime = (int)Utils.GetField<ShroomColony>("_growTime", __instance);

            // Subtract 1 from stationSpeed to account for growth added by OnMinPass prior to this postfix
            float newGrowTime = (float)originalGrowTime / (stationSpeed - 1f);
            float growthDelta = Mathf.Clamp01(currentGrowthRate / ((float)newGrowTime * 60f));

            // Keep growthprogress between 0 and 1
            float percentageDelta = Mathf.Clamp01(__instance.GrowthProgress + growthDelta) - __instance.GrowthProgress;
            Utils.CallMethod<ShroomColony>("ChangeGrowthPercentage", __instance, [percentageDelta]);

            return false;
        }
    }

    [HarmonyPatch]
    public class SpawnStationPatches
    {
        // Call our own coroutine with accelerated animations
        [HarmonyPatch(typeof(UseSpawnStationBehaviour), "RpcLogic___BeginWork_2166136261")]
        [HarmonyPrefix]
        public static bool RpcLogicBeginWorkPrefix(UseSpawnStationBehaviour __instance)
        {
            if ((bool)Utils.GetField<UseSpawnStationBehaviour>("_currentlyUsingStation", __instance))
            {
                return false;
            }
            if (!__instance.IsStationReady(__instance.Station))
            {
                return false;
            }
            Utils.SetField<UseSpawnStationBehaviour>("_currentlyUsingStation", __instance, true);
            if (InstanceFinder.IsServer)
            {
                __instance.Station.SetNPCUser(__instance.Npc.NetworkObject);
            }
            object workRoutine = Utils.StartCoroutine(SpawnStationCoroutine(__instance));
            Utils.SetField<UseSpawnStationBehaviour>("_workRoutine", __instance, workRoutine);
            return false;
        }

        // Coroutine with accelerated animation
        public static IEnumerator SpawnStationCoroutine(UseSpawnStationBehaviour behaviour)
        {
            float originalDuration = (float)Utils.GetField<UseSpawnStationBehaviour>("TaskDuration", behaviour);
            float workSpeed = Utils.GetStationWorkSpeed(behaviour.Station);
            float progress = 0f;
            float duration = Mathf.Max(0.1f, originalDuration / workSpeed);

            // shouldn't this be SetAnimationBool_Networked?
            behaviour.Npc.SetAnimationBool("UsePackagingStation", true);
            while (progress < duration)
            {
                progress += Time.deltaTime;
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(behaviour.Station.UIPoint.position, 0, true);
                yield return null;
            }
            if (InstanceFinder.IsServer && behaviour.Station != null && behaviour.Station.DoesStationContainRequiredItems() && behaviour.Station.DoesStationHaveOutputSpace())
            {
                SporeSyringeDefinition sporeSyringeDefinition = Utils.CastTo<SporeSyringeDefinition>(behaviour.Station.SyringeSlot.ItemInstance.Definition);
                behaviour.Station.SyringeSlot.ChangeQuantity(-1, false);
                behaviour.Station.GrainBagSlot.ChangeQuantity(-1, false);
                behaviour.Station.OutputSlot.AddItem(sporeSyringeDefinition.SpawnDefinition.GetDefaultInstance(1), false);
            }

            Utils.CallMethod<UseSpawnStationBehaviour>("StopWork", behaviour);
            behaviour.Disable_Networked(null);
            yield break;
        }

        [HarmonyPatch(typeof(UseSpawnStationBehaviour), "StopWork")]
        [HarmonyPrefix]
        public static void StopWorkPrefix(UseSpawnStationBehaviour __instance)
        {
            object workRoutine = Utils.GetField<UseSpawnStationBehaviour>("_workRoutine", __instance);
            Utils.StopCoroutine(workRoutine);
            Utils.SetField<UseSpawnStationBehaviour>("_workRoutine", __instance, null);
        }
    }

    // GrowContainer patches
    [HarmonyPatch]
    public class GrowContainerPatches
    {
        // GrowContainerBehaviour has a protected enum that I can't figure out how to access via reflection.
        // Just keep a copy here, and hope Tyler doesn't change it too often.
        private enum EState
        {
            Idle = 0,
            Walking = 1,
            GrabbingSupplies = 2,
            PerformingAction = 3
        }

        private static Dictionary<Type, float> behaviourDurations = new Dictionary<Type, float>()
        {
            { typeof(AddSoilToGrowContainerBehaviour), 15f },
            { typeof(ApplyAdditiveToGrowContainerBehaviour), 10f },
            { typeof(HarvestPotBehaviour), 10f },
            { typeof(SowSeedInPotBehaviour), 10f },
            { typeof(WaterPotBehaviour), 10f },
            { typeof(ApplySpawnToMushroomBedBehaviour), 15f },
            { typeof(HarvestMushroomBedBehaviour), 10f },
            { typeof(MistMushroomBedBehaviour), 10f }
        };

        private static List<Type> harvestBehaviours = new List<Type>()
        {
            typeof(HarvestPotBehaviour),
            typeof(HarvestMushroomBedBehaviour)
        };

        // call our own coroutine to speed up employee animation
        private static void PerformAction(GrowContainerBehaviour __instance)
        {
            if (!__instance.AreTaskConditionsMetForContainer(Utils.GetProperty<GrowContainerBehaviour, GrowContainer>("_growContainer", __instance)))
            {
                __instance.Disable_Networked(null);
                return;
            }
            Utils.SetProperty<GrowContainerBehaviour>("_currentState", __instance, (int)EState.PerformingAction);
            object workRoutine = Utils.StartCoroutine(PerformActionCoroutine(__instance));
            Utils.SetField<GrowContainerBehaviour>("_performActionRoutine", __instance, (Coroutine)workRoutine);

            return;
        }

        // Directly patching GetActionDuration results in a stack overflow for some reason.
        // Just call this instead of GrowContainer.GetActionDuration.
        private static float GetActionDuration(GrowContainerBehaviour behaviour)
        {
            Type behaviourType = Utils.GetType(behaviour);
            float duration;

            if (harvestBehaviours.Contains(behaviourType))
            {
                duration = Botanist.IndividualHarvestTime * GetQuantityToHarvest(behaviour);
            }
            else if (behaviourDurations.ContainsKey(behaviourType))
            {
                duration = behaviourDurations[behaviourType];
            }
            else
            {
                Utils.Warn($"Couldn't find action duration for behaviour \"{behaviour.name}\" ({behaviourType.Name})");
                duration = 10f;
            }
            return duration;
        }

        // GetQuantityToHarvest is defined for some descendants of GrowContainerBehaviour, but not others, so
        // getting it via reflection is wordy. Abstract to its own function.
        private static int GetQuantityToHarvest(GrowContainerBehaviour behaviour)
        {
            if (Utils.Is<HarvestPotBehaviour>(behaviour))
            {
                HarvestPotBehaviour potBehaviour = Utils.CastTo<HarvestPotBehaviour>(behaviour);
                return (int)Utils.CallMethod<HarvestPotBehaviour>("GetQuantityToHarvest", potBehaviour);
            }
            else if (Utils.Is<HarvestMushroomBedBehaviour>(behaviour))
            {
                HarvestMushroomBedBehaviour shroomBehaviour = Utils.CastTo<HarvestMushroomBedBehaviour>(behaviour);
                return (int)Utils.CallMethod<HarvestMushroomBedBehaviour>("GetQuantityToHarvest", shroomBehaviour);
            }
            else
            {
                Utils.Warn($"Couldn't get quantity to harvest from behaviour {Utils.GetType(behaviour).Name}");
                return 0;
            }
        }

        // coroutine with accelerated animations
        // patching GetActionDuration directly resulted in a stack overflow for some reason?
        // call local replacement function instead.
        private static IEnumerator PerformActionCoroutine(GrowContainerBehaviour behaviour)
        {
            Utils.CallMethod<GrowContainerBehaviour>("OnStartPerformAction", behaviour);
            GrowContainer growContainer = Utils.GetProperty<GrowContainerBehaviour, GrowContainer>("_growContainer", behaviour);
            float workSpeed = Utils.GetStationWorkSpeed(growContainer);
            float waitTime = GetActionDuration(behaviour) / workSpeed;
            Vector3 targetPosition = (Vector3)Utils.CallMethod<GrowContainerBehaviour>("GetGrowContainerLookPoint", behaviour);
            for (float i = 0f; i < waitTime; i += Time.deltaTime)
            {
                behaviour.Npc.Avatar.LookController.OverrideLookTarget(targetPosition, 0, false);
                yield return null;
            }
            if (!behaviour.AreTaskConditionsMetForContainer(growContainer))
            {
                behaviour.Disable_Networked(null);
                yield break;
            }
            Utils.CallMethod<GrowContainerBehaviour>("OnStopPerformAction", behaviour);

            // Second parameter of DoesTaskRequireItem is out string[]. Keep a handle to the args array so we can retrieve the value.
            object[] args = new object[2] { growContainer, null };
            bool taskRequiresItem = (bool)Utils.CallMethod<GrowContainerBehaviour>("DoesTaskRequireItem", behaviour, args);
            StringArray suitableItemIDs = (StringArray)args[1];

            ItemSlot itemSlot = null;
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

            object workRoutine = Utils.GetField<GrowContainerBehaviour>("_performActionRoutine", behaviour);
            Utils.StopCoroutine(workRoutine);
            Utils.SetField<GrowContainerBehaviour>("_performActionRoutine", behaviour, null);

            behaviour.Disable_Networked(null);
            yield break;
        }

        // properly stop our coroutines
        [HarmonyPatch(typeof(GrowContainerBehaviour), "StopAllRoutines")]
        [HarmonyPrefix]
        public static void StopAllRoutinesPrefix(GrowContainerBehaviour __instance)
        {
            object performActionRoutine = Utils.GetField<GrowContainerBehaviour>("_performActionRoutine", __instance);
            if (performActionRoutine != null)
            {
                Utils.CallMethod<GrowContainerBehaviour>("OnStopPerformAction", __instance);
                Utils.StopCoroutine(performActionRoutine);
                Utils.SetField<GrowContainerBehaviour>("_performActionRoutine", __instance, null);
            }
        }

        // PerformAction is inlined. Replace OnActiveTick with original method body.
        [HarmonyPatch(typeof(GrowContainerBehaviour), "OnActiveTick")]
        [HarmonyPrefix]
        public static bool OnActiveTickPrefix(GrowContainerBehaviour __instance)
        {
            // base.OnActiveTick is originally called here.
            // However, Behaviour.OnActiveTick is empty.
            // Just skip the whole call.

            if (!InstanceFinder.IsServer)
            {
                return false;
            }

            int currentState = (int)Utils.GetProperty<GrowContainerBehaviour>("_currentState", __instance);
            GrowContainer growContainer = Utils.GetProperty<GrowContainerBehaviour, GrowContainer>("_growContainer", __instance);

            if (currentState == (int)EState.Idle)
            {
                if (growContainer == null)
                {
                    __instance.Disable_Networked(null);
                    return false;
                }
                if ((bool)Utils.CallMethod<GrowContainerBehaviour>("IsRequiredItemInInventory", __instance, [growContainer]))
                {
                    if ((bool)Utils.CallMethod<GrowContainerBehaviour>("IsAtGrowContainer", __instance))
                    {
                        // PerformAction seems to be inlined. just call our replacement version.
                        PerformAction(__instance);
                        return false;
                    }
                    ITransitEntity transitEntity = Utils.ToInterface<ITransitEntity>(growContainer);
                    Utils.CallMethod<GrowContainerBehaviour>("WalkTo", __instance, [transitEntity]);
                    return false;
                }
                else if ((bool)Utils.CallMethod<GrowContainerBehaviour>("DoSuppliesContainRequiredItem", __instance, [growContainer]))
                {
                    if ((bool)Utils.CallMethod<GrowContainerBehaviour>("IsAtSupplies", __instance))
                    {
                        Utils.CallMethod<GrowContainerBehaviour>("GrabRequiredItemFromSupplies", __instance);
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
        // Adjust recipe time when player begins interacting with station
        [HarmonyPatch(typeof(ChemistryStationCanvas), "Open")]
        [HarmonyPrefix]
        public static void CanvasOpenPrefix(ChemistryStationCanvas __instance, ChemistryStation station)
        {
            if (station != null) 
            {
                float stationSpeed = Utils.GetStationSpeed(station);
                StationRecipe recipe = Utils.GetProperty<ChemistryStation, ChemistryStationConfiguration>("stationConfiguration", station).Recipe.SelectedRecipe;
                Utils.ModRecipeTime(recipe, stationSpeed);
            }
        }

        // Fix cook time label
        [HarmonyPatch(typeof(StationRecipeEntry), "AssignRecipe")]
        [HarmonyPostfix]
        public static void AssignRecipePostfix(StationRecipeEntry __instance, ref StationRecipe recipe)
        {
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
            float stationSpeed = Utils.GetStationSpeed(__instance.targetStation);
            StationRecipe recipe = Utils.GetProperty<ChemistryStation, ChemistryStationConfiguration>("stationConfiguration", __instance.targetStation).Recipe.SelectedRecipe;
            Utils.ModRecipeTime(recipe, stationSpeed);

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
            float workSpeed = Utils.GetStationWorkSpeed(behaviour.targetStation);

            behaviour.Npc.Movement.FacePoint(behaviour.targetStation.transform.position, Mathf.Max(0.1f, 0.5f / workSpeed));
            yield return new WaitForSeconds(Mathf.Max(0.1f, 0.5f / workSpeed));

            behaviour.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", true);
            if (!(bool)Utils.CallMethod<StartChemistryStationBehaviour>("CanCookStart", behaviour))
            {
                Utils.CallMethod<StartChemistryStationBehaviour>("StopCook", behaviour);
                behaviour.Deactivate_Networked(null);
                yield break;
            }

            behaviour.targetStation.SetNPCUser(behaviour.Npc.NetworkObject);
            Utils.CallMethod<StartChemistryStationBehaviour>("SetupBeaker", behaviour);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 1f / workSpeed));

            StationRecipe recipe = Utils.GetProperty<ChemistryStation, ChemistryStationConfiguration>("stationConfiguration", behaviour.targetStation).Recipe.SelectedRecipe;
            float stationSpeed = Utils.GetStationSpeed(behaviour.targetStation);
            Utils.ModRecipeTime(recipe, stationSpeed);

            Beaker beaker = Utils.GetField<StartChemistryStationBehaviour, Beaker>("beaker", behaviour);
            Utils.CallMethod<StartChemistryStationBehaviour>("FillBeaker", behaviour, [recipe, beaker]);
            yield return new WaitForSeconds(Mathf.Max(0.1f, 20f / workSpeed));

            ItemInstanceList list = new ItemInstanceList();
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                foreach (ItemDefinition itemDefinition in recipe.Ingredients[i].Items)
                {
                    StorableItemDefinition storableIngredientDef = Utils.CastTo<StorableItemDefinition>(itemDefinition);
                    for (int j = 0; j < behaviour.targetStation.IngredientSlots.Length; j++)
                    {
                        if (behaviour.targetStation.IngredientSlots[j].ItemInstance != null && behaviour.targetStation.IngredientSlots[j].ItemInstance.Definition.ID == storableIngredientDef.ID)
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
            Utils.CallMethod<StartChemistryStationBehaviour>("StopCook", behaviour);
            behaviour.Deactivate_Networked(null);
            yield break;
        }

        [HarmonyPatch(typeof(StartChemistryStationBehaviour), "StopCook")]
        [HarmonyPrefix]
        public static void StopCookPrefix(StartChemistryStationBehaviour __instance)
        {
            object workRoutine = Utils.GetField<StartChemistryStationBehaviour>("cookRoutine", __instance);
            if (workRoutine != null)
            {
                Utils.StopCoroutine(workRoutine);
                Utils.SetField<StartChemistryStationBehaviour>("cookRoutine", __instance, null);
            }
        }

        // Base game bug: only go to stations that have enough space at the output.
        [HarmonyPatch(typeof(Chemist), "GetChemistryStationsReadyToStart")]
        [HarmonyPostfix]
        public static void GetChemistryStationsReadyToStartPostfix(Chemist __instance, ref ChemStationList __result)
        {
            ChemStationList readyStations = __result.FindAll(Utils.ToPredicate<ChemistryStation>((station) => 
            {
                StationRecipe recipe = Utils.GetProperty<ChemistryStation, ChemistryStationConfiguration>("stationConfiguration", station).Recipe.SelectedRecipe;
                return (bool)Utils.CallMethod<ChemistryStation>("DoesOutputHaveSpace", station, [recipe]);
            }));
            __result = readyStations;
        }

        // Base game bug: check that there's space in the output before performing cook operation.
        [HarmonyPatch(typeof(StartChemistryStationBehaviour), "CanCookStart")]
        [HarmonyPostfix]
        public static void CanCookStartPostfix(StartChemistryStationBehaviour __instance, ref bool __result)
        {
            StationRecipe recipe = Utils.GetProperty<ChemistryStation, ChemistryStationConfiguration>("stationConfiguration", __instance.targetStation).Recipe.SelectedRecipe;
            __result = __result && (bool)Utils.CallMethod<ChemistryStation>("DoesOutputHaveSpace", __instance.targetStation, [recipe]);
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
        // Shorten stopping distance for NPCs.
        // This should decrease the amount of overshoot at high walk acceleration settings.
        // This field is serialized, so this change will persist in the save.
        // If someone disables this mod, this field will be permanently altered.
        // Fortunately, decreasing stopping distance barely has a noticeable effect at default walk speed.
        [HarmonyPatch(typeof(NPCMovement), "Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(NPCMovement __instance)
        {
            __instance.Agent.stoppingDistance = 0.1f;
        }

        // Sometimes, when entity is spawn station, employees will fail to path even when station is
        // perfectly accessible.
        // Relax proximity requirement for spawn station.
        [HarmonyPatch(typeof(NPCMovement), "CanGetTo", [typeof(ITransitEntity), typeof(float)])]
        [HarmonyPrefix]
        public static void CanGetToPrefix(NPCMovement __instance, ITransitEntity entity, ref float proximityReq)
        {
            if (entity == null)
            {
                return;
            }
            if (Utils.Is<MushroomSpawnStation>(entity) && proximityReq < 0.8f)
            {
                proximityReq = 0.8f;
            }
            return;
        }
    }

    // NPCMovement.SetDestination has a generic in its signature, which is unfriendly to annotations.
    // Just move it to its own class and use HarmonyTargetMethod instead.
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
        public static void SetDestinationPrefix(NPCMovement __instance, ref float successThreshold)
        {
            // Adjust walk speed/acceleration/angularspeed
            float walkAcceleration = 1f;
            NPC npc = Utils.GetField<NPCMovement, NPC>("npc", __instance);
            if (Utils.Is<Employee>(npc))
            {
                walkAcceleration = Utils.Mod.employeeAnimation.GetEntry<float>("employeeWalkAcceleration").Value;
            }
            // Setting acceleration and angularspeed is necessary for employees to navigate
            // paths at high speed, but setting them too high makes motions unnecessarily jerky.
            __instance.MoveSpeedMultiplier = walkAcceleration;
            if (walkAcceleration > 1f)
            {
                __instance.Agent.acceleration = 20f * Mathf.Pow(walkAcceleration, 5f);
                __instance.Agent.angularSpeed = 720f * Mathf.Pow(walkAcceleration, 5f);
            }

            // Sometimes, employees get stuck at a station and won't move.
            // At least some of the time, this is due to most walk routines having a proximity threshold of 1f,
            // but various proximity checking functions (IsAtStation, CanGetTo, etc) check within a smaller radius (0.4-0.6).
            // Meaning an employee can get stuck between 0.6f and 1f from the station access point,
            // where IsAtStation returns false, but SetDestination does not bring the NPC any closer.
            // In other words, get closer to the destination before ending walk routine.
            if (successThreshold > 0.4f)
            {
                successThreshold = 0.4f;
            }
        }
    }
}
