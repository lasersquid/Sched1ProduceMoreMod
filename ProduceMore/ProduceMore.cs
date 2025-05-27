
#if MONO_BUILD
using FishNet;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Phone.Delivery;
using ScheduleOne.UI.Shop;
using ScheduleOne.UI.Stations.Drying_rack;
using ScheduleOne.UI.Stations;
#else
using Il2CppFishNet;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Startup;
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
    public class Sched1PatchesBase
    {
        protected static ProduceMoreMod Mod;

        public static void SetMod(ProduceMoreMod mod)
        {
            Mod = mod;
        }

        public static T CastTo<T>(object o)
        {
            return (T)o;
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
                if (!Mod.originalStackLimits.ContainsKey(__instance.Definition.Category))
                {
                    EItemCategory category;
                    if (__instance.Definition.Name == "Speed Grow")
                    {
                        category = EItemCategory.Growing;
                    }
                    else
                    {
                        category = __instance.Definition.Category;
                    }
                    Mod.LoggerInstance.Msg($"Captured original stacklimit of {category} as {__instance.Definition.StackLimit}");
                    Mod.originalStackLimits[category] = __instance.Definition.StackLimit; 
                }

                int stackLimit = Mod.settings.GetStackLimit(__instance);
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
                    if (!Mod.originalStackLimits.ContainsKey(match.Item.Category))
                    {
                        EItemCategory category;
                        if (match.Item.Name == "Speed Grow")
                        {
                            category = EItemCategory.Growing;
                        }
                        else
                        {
                            category = match.Item.Category;
                        }
                        Mod.LoggerInstance.Msg($"Captured original stacklimit of {category} as {match.Item.StackLimit}");
                        Mod.originalStackLimits[category] = match.Item.StackLimit;
                    }
                    int stackLimit = Mod.settings.GetStackLimit(match.Item);
                    match.Item.StackLimit = stackLimit;
                    Mod.processedItemDefs.Add(match.Item);
                }
            }
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
                        Mod.processedItemDefs.Remove(itemDef);
                    }
                }
            }
            catch (Exception e)
            {
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
				if (e.InnerException != null)
				{
					Mod.LoggerInstance.Warning($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
					Mod.LoggerInstance.Warning($"Source: {e.InnerException.Source}");
				}

                return;
            }

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
            //if (__instance.ItemCapacity != Mod.settings.GetStationCapacity("DryingRack"))
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

            float stationSpeed =  (float)Mod.originalStationTimes["DryingRack"] / Mod.settings.GetStationSpeed("DryingRack");
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
                        Mod.processedStationCapacities.Remove(rack);
                        Mod.LoggerInstance.Msg($"Reset capacity for {rack.GetType().Name} {rack.GetInstanceID()}");
                    }
                }
            }
            catch (Exception e)
            {
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
				if (e.InnerException != null)
				{
					Mod.LoggerInstance.Warning($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
					Mod.LoggerInstance.Warning($"Source: {e.InnerException.Source}");
				}
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
        public static void GetCookDurationPostfix(ref int __result)
        {
            if (!Mod.originalStationTimes.ContainsKey("LabOven"))
            {
                Mod.originalStationTimes["LabOven"] = __result;
            }
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
                if (!Mod.originalStationCapacities.ContainsKey("MixingStation"))
                {
                    Mod.originalStationCapacities["MixingStation"] = __instance.MaxMixQuantity;
                }
                __instance.MaxMixQuantity = Mod.settings.GetStationCapacity("MixingStation");
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

        // speed
        [HarmonyPatch(typeof(MixingStation), "GetMixTimeForCurrentOperation")]
        [HarmonyPrefix]
        public static bool GetMixTimePrefix(MixingStation __instance, ref int __result)
        {
            if (!Mod.processedStationSpeeds.Contains(__instance))
            {
                if (!Mod.originalStationTimes.ContainsKey("MixingStation"))
                {
                    // we can't just capture MixTimePerItem because that data is saved. this makes the divider compound over successive save/loads
                    //Mod.originalStationTimes["MixingStation"] = __instance.MixTimePerItem;
                    // Use a dirty magic number for now; relevant constant is not accessible in il2cpp
                    Mod.originalStationTimes["MixingStation"] = 15;
                }

                __instance.MixTimePerItem = (int)Mathf.Max((float)Mod.originalStationTimes["MixingStation"] / Mod.settings.GetStationSpeed("MixingStation"), 1f);
            }
            float mixTimePerItem = Mathf.Max((float)Mod.originalStationTimes["MixingStation"] / Mod.settings.GetStationSpeed("MixingStation"), 1f);

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
            if (!Mod.processedStationCapacities.Contains(__instance.targetStation))
            {
                if (!Mod.originalStationCapacities.ContainsKey("MixingStation"))
                {
                    Mod.originalStationCapacities["MixingStation"] = __instance.targetStation.MaxMixQuantity;
                }
                __instance.targetStation.MaxMixQuantity = Mod.settings.GetStationCapacity("MixingStation");
                Mod.processedStationCapacities.Add(__instance.targetStation);
            }
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
                        if (!Mod.originalStationCapacities.ContainsKey("MixingStation"))
                        {
                            mixingStation.MaxMixQuantity = 20;
                        }
                        else
                        {
                            mixingStation.MaxMixQuantity = Mod.originalStationCapacities["MixingStation"];
                        }
                        Mod.processedStationCapacities.Remove(station);
                    }
                }

                foreach(var station in Mod.processedStationTimes)
                {
                    MixingStation mixingStation = CastTo<MixingStation>(station);
                    if (Mod.processedStationTimes.Contains(station))
                    {
                        if (!Mod.originalStationTimes.ContainsKey("MixingStation"))
                        {
                            mixingStation.MixTimePerItem = 15;
                        }
                        else
                        {
                            mixingStation.MixTimePerItem = Mod.originalStationTimes["MixingStation"];
                        }
                        Mod.processedStationTimes.Remove(mixingStation);
                    }
                }
            }
            catch (Exception e)
            {
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
				if (e.InnerException != null)
				{
					Mod.LoggerInstance.Warning($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
					Mod.LoggerInstance.Warning($"Source: {e.InnerException.Source}");
				}
                return;
            }
        }
    }


    // Brick press patches
    // currently empty
    [HarmonyPatch]
    public class BrickPressPatches : Sched1PatchesBase
    {
        // currently empty
        public static new void RestoreDefaults()
        {
            // currently empty
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
        [HarmonyPatch(typeof(StartCauldronBehaviour), "BeginCauldron")]
        [HarmonyPostfix]
        public static void BeginCauldronPostfix(StartCauldronBehaviour __instance)
        {
            if (!Mod.originalStationTimes.ContainsKey("Cauldron"))
            {
                Mod.originalStationTimes["Cauldron"] = __instance.Station.CookTime;
            }

            int newCookTime = (int)((float)Mod.originalStationTimes["Cauldron"] / Mod.settings.GetStationSpeed("Cauldron"));
            if (__instance.Station.RemainingCookTime > newCookTime)
            {
                __instance.Station.RemainingCookTime = newCookTime;
            }
        }


        [HarmonyPatch(typeof(Cauldron), "StartCookOperation")]
        [HarmonyPostfix]
        public static void StartCookOperationPostfix(Cauldron __instance)
        {
            if (!Mod.originalStationTimes.ContainsKey("Cauldron"))
            {
                Mod.originalStationTimes["Cauldron"] = __instance.CookTime;
            }

            int newCookTime = (int)((float)Mod.originalStationTimes["Cauldron"] / Mod.settings.GetStationSpeed("Cauldron"));
            __instance.RemainingCookTime = newCookTime;
        }

#if !MONO_BUILD

        [HarmonyPatch(typeof(StartCauldronBehaviour), "BeginCauldron")]
        [HarmonyPrefix]
        public static void BeginCauldronPrefix(StartCauldronBehaviour __instance)
        {
            if (!Mod.originalStationTimes.ContainsKey("Cauldron"))
            {
                Mod.originalStationTimes["Cauldron"] = __instance.Station.CookTime;
            }
            if (!Mod.processedStationTimes.Contains(__instance.Station))
            {
                int newCookTime = (int)((float)Mod.originalStationTimes["Cauldron"] / Mod.settings.GetStationSpeed("Cauldron"));
                __instance.Station.CookTime = newCookTime;
                Mod.processedStationTimes.Add(__instance.Station);
            }
        }


        [HarmonyPatch(typeof(Cauldron), "StartCookOperation")]
        [HarmonyPrefix]
        public static void StartCookOperationPrefix(Cauldron __instance)
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
        }
#endif



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
                        Mod.processedStationTimes.Remove(station);
                    }
                }
            }
            catch (Exception e)
            {
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
				if (e.InnerException != null)
				{
					Mod.LoggerInstance.Warning($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
					Mod.LoggerInstance.Warning($"Source: {e.InnerException.Source}");
				}
                return;
            }
        }
        // capacity takes care of itself
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
                        Mod.processedStationSpeeds.Remove(packagingStation);
                    }
                }
            }
            catch (Exception e)
            {
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
				Mod.LoggerInstance.Warning($"Source: {e.Source}");
				if (e.InnerException != null)
				{
					Mod.LoggerInstance.Warning($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
					Mod.LoggerInstance.Warning($"Source: {e.InnerException.Source}");
				}
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
                    Mod.processedRecipes.Remove(recipe);
                }
            }
            catch (Exception e)
            {
                Mod.LoggerInstance.Warning($"Couldn't restore defaults for {MethodBase.GetCurrentMethod().DeclaringType.Name}: {e.GetType().Name} - {e.Message}");
                Mod.LoggerInstance.Warning($"Source: {e.Source}");
                if (e.InnerException != null)
                {
                    Mod.LoggerInstance.Warning($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
                    Mod.LoggerInstance.Warning($"Source: {e.InnerException.Source}");
                }
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


        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
			yield return AccessTools.DeclaredMethod(typeof(ItemUIManager), "UpdateCashDragAmount");
			yield return AccessTools.DeclaredMethod(typeof(ItemUIManager), "StartDragCash");
			yield return AccessTools.DeclaredMethod(typeof(ItemUIManager), "EndCashDrag");
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MelonLogger.Msg($"Transpiler for CashPatches started");
            //MelonLogger.Msg("Instruction dump:");
            //foreach (var instruction in instructions) { MelonLogger.Msg($"{instruction.opcode} {instruction.operand}"); }

            MethodInfo getCashStackLimitInfo = AccessTools.Method(typeof(CashPatches), nameof(GetCashStackLimit));

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
                        new CodeInstruction(OpCodes.Call, getCashStackLimitInfo)
                    );
                } 
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Replaced all \"Ldc.r4 1000f\" with calls to GetCashStackLimit()");
            }

            IEnumerable<CodeInstruction> modifiedIL = matcher.InstructionEnumeration();

            //MelonLogger.Msg("\nModified instruction dump:");
            //foreach (var instruction in modifiedIL) { MelonLogger.Msg($"{instruction.opcode} {instruction.operand}"); }

            return modifiedIL;
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

            CashInstance cashInstance = CastTo<CashInstance>(__instance.draggedSlot.assignedSlot.ItemInstance);
            if (cashInstance == null)
            {
                Mod.LoggerInstance.Warning($"Couldn't cast ItemInstance to CashInstance???");
            }
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
}
