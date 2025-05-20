using HarmonyLib;
using UnityEngine;
using MelonLoader;
using System.Collections;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.Persistence.Loaders;
using System.Reflection.Emit;
using Il2CppScheduleOne.Tiles;
using Il2CppScheduleOne.DevUtilities;

using Unity.Jobs.LowLevel.Unsafe;
using Il2CppScheduleOne.GameTime;
using System.Runtime.CompilerServices;
using System.Reflection;
using Il2CppScheduleOne.Map;
using Il2Cpp;
using Il2CppScheduleOne.UI.Stations;
using Il2CppScheduleOne.UI.Stations.Drying_rack;
using static UnityEngine.ExpressionEvaluator;
using Il2CppFishNet.Managing;

using Il2CppFishNet.Serializing;










#if MONO_BUILD
using FishNet;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.StationFramework;
using System.Runtime.InteropServices;
using ScheduleOne.ObjectScripts;
using ScheduleOne.ItemFramework;
using ScheduleOne;
#else
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.StationFramework;
using Il2CppSystem.Runtime.InteropServices;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.ItemFramework;
using Il2CppSystem.Collections;
using Il2CppScheduleOne;
using Il2CppScheduleOne.EntityFramework;
using Il2CppFishNet;
using Il2CppFishNet.Object;
using Il2CppFishNet.Transporting;
#endif

namespace ProduceMore
{
    //Set stack sizes
    [HarmonyPatch]
    public class ItemInstancePatches
    {
        public static ProduceMoreMod Mod = null;

        // Increase stack limit
        [HarmonyPatch(typeof(ItemInstance), "StackLimit", MethodType.Getter)]
        [HarmonyPrefix]
        public static void StackLimitPostfix(ItemInstance __instance)
        {
            if (!Mod.processedDefs.Contains(__instance.Definition))
            {
                int stackLimit = Mod.settings.GetStackLimit(__instance);
                __instance.Definition.StackLimit = stackLimit;
                Mod.processedDefs.Add(__instance.Definition);
                MelonLogger.Msg($"Set stack limit for {__instance.Definition.Name} to {stackLimit}");
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
        public static bool IsRackReadyPrefix(DryingRack rack, ref bool __result)
        {
            if (rack != null && rack.ItemCapacity != Mod.settings.GetStationCapacity("DryingRack"))
            {
                rack.ItemCapacity = Mod.settings.GetStationCapacity("DryingRack");
            }
            return true;
        }

        // Modify DryingRack.ItemCapacity
        // canstartoperation runs every time a player or npc tries to interact
        // may have optimized away real access to ItemCapacity??
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

        // maybe unnecessary
        [HarmonyPatch(typeof(DryingRack), "StartOperation")]
        [HarmonyPrefix]
        public static void StartOperationPrefix(DryingRack __instance)
        {
            if (__instance != null && __instance.ItemCapacity != Mod.settings.GetStationCapacity("DryingRack"))
            {
                __instance.ItemCapacity = Mod.settings.GetStationCapacity("DryingRack");
            }
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
        //public static void NewMinPass(DryingRack __instance)
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

        // baby's first transpiler patch
        /*
        // patch minpass to call into our NewMinPass routine, instead of jumping to IL2CPP ether
        [HarmonyPatch(typeof(DryingRack), "MinPass")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MinPassTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MelonLogger.Msg($"Transpiler for DryingRack.MinPass started");
            MethodInfo newMinPassInfo = AccessTools.Method(typeof(DryingRackPatches), nameof(NewMinPass));

            //MelonLogger.Msg("Instruction dump:");
            //foreach (var instruction in instructions)
            //{
            //    MelonLogger.Msg($"Opcode: {instruction.opcode}, Operand: {instruction.operand}");
            //}

            // We want to completely overwrite the target function with our own.
            CodeMatcher matcher = new(instructions, generator);

            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldarg_0)
            )
            .ThrowIfNotMatch($"Could not find entry point for {nameof(MinPassTranspiler)}");

            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),       // arg 0 is always this
                new CodeInstruction(OpCodes.Call, newMinPassInfo),
                new CodeInstruction(OpCodes.Ret)
            );

            // Navigate to end of injected method
            matcher.Start()
            .MatchEndForward(
                new CodeMatch(OpCodes.Ret)
            )
            .Advance(1);

            // get number of remaining instructions and remove
            int instructionsToRemove = matcher.InstructionEnumeration().ToList<CodeInstruction>().Count - matcher.Pos;
            matcher.RemoveInstructions(instructionsToRemove);

            IEnumerable<CodeInstruction> modifiedIL = matcher.InstructionEnumeration();

            //MelonLogger.Msg("\nModified instruction dump:");
            //foreach (var instruction in modifiedIL)
            //{
            //    MelonLogger.Msg($"Opcode: {instruction.opcode}, Operand: {instruction.operand}");
            //}

            return modifiedIL;
        }
        */
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
            if (Mod.settings.GetStationSpeed("LabOven") > 0)
            {
                __result = (int)((float)__result / Mod.settings.GetStationSpeed("LabOven"));
            }
        }

        [HarmonyPatch(typeof(OvenCookOperation), "IsReady")]
        [HarmonyPostfix]
        public static void IsReadyPostfix(OvenCookOperation __instance, ref bool __result)
        {
            // Re-insert original method body.
            __result = __instance.CookProgress >= __instance.GetCookDuration();
        }


        //TODO: patch laboven capacity.
        // or don't. too much effort for too little reward
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
            if (!Mod.processedStationCapacities.Contains(__instance))
            {
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

        // maybe unnecessary
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
            int mixTimePerItem = (int)(15f / Mod.settings.GetStationSpeed("MixingStation"));
            if (__instance.MixTimePerItem != mixTimePerItem)
            {
                __instance.MixTimePerItem = mixTimePerItem;
                Mod.processedStationSpeeds.Add(__instance);
            }

            if (__instance.CurrentMixOperation == null)
            {
                __result = 0;
            }
            __result = __instance.MixTimePerItem * __instance.CurrentMixOperation.Quantity;

            return false;
        }
    }


    // Brick press patches
    [HarmonyPatch]
    public static class BrickPressPatches
    {
        public static ProduceMoreMod Mod;
        // capacity
        //[HarmonyPatch(typeof(BrickPress), "GetMainInputs")]
        public static bool GetMainInputsPrefix(BrickPress __instance, ref ItemInstance primaryItem, ref int primaryItemQuantity, ref ItemInstance secondaryItem, ref int secondaryItemQuantity)
        {
            int batchLimit = Mod.settings.GetStationCapacity("BrickPress");
            List<ItemInstance> list = new List<ItemInstance>();
            Dictionary<ItemInstance, int> itemQuantities = new Dictionary<ItemInstance, int>();
            int i, k;
            for (i = 0; i < __instance.InputSlots.Count; i = k + 1)
            {
                if (__instance.InputSlots[i].ItemInstance != null)
                {
                    ItemInstance itemInstance = list.Find((ItemInstance x) => x.ID == __instance.InputSlots[i].ItemInstance.ID);
                    if (itemInstance == null || !itemInstance.CanStackWith(__instance.InputSlots[i].ItemInstance, false))
                    {
                        itemInstance = __instance.InputSlots[i].ItemInstance;
                        list.Add(itemInstance);
                        if (!itemQuantities.ContainsKey(__instance.InputSlots[i].ItemInstance))
                        {
                            itemQuantities.Add(__instance.InputSlots[i].ItemInstance, 0);
                        }
                    }
                    ItemInstance key = itemInstance;
                    itemQuantities[key] += __instance.InputSlots[i].Quantity;
                }
                k = i;
            }
            for (int j = 0; j < list.Count; j++)
            {
                if (itemQuantities[list[j]] > 20)
                {
                    //TODO: fix this logic
                    int numToPress = Mathf.Min((batchLimit), itemQuantities[list[j]]);
                    int num = itemQuantities[list[j]] - numToPress;
                    itemQuantities[list[j]] = numToPress;
                    ItemInstance copy = list[j].GetCopy(num);
                    list.Add(copy);
                    itemQuantities.Add(copy, num);
                }
            }
            list = (from x in list
                    orderby itemQuantities[x] descending
                    select x).ToList<ItemInstance>();
            if (list.Count > 0)
            {
                primaryItem = list[0];
                primaryItemQuantity = itemQuantities[list[0]];
            }
            else
            {
                primaryItem = null;
                primaryItemQuantity = 0;
            }
            if (list.Count > 1)
            {
                secondaryItem = list[1];
                secondaryItemQuantity = itemQuantities[list[1]];
                return false;
            }
            secondaryItem = null;
            secondaryItemQuantity = 0;

            return false;
        }
    }

    // cauldron patches
    [HarmonyPatch]
    public static class CauldronPatches
    {
        public static ProduceMoreMod Mod;
        // patch visuals for capacity
        // TODO: show it filled up if quantity > 20
        [HarmonyPatch(typeof(Cauldron), "UpdateIngredientVisuals")]
        [HarmonyPrefix]
        public static bool UpdateIngredientVisualsPatch(Cauldron __instance)
        {
            int cauldronCapacity = Mod.settings.GetStationCapacity("Cauldron");
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

        // Patch cauldron input stack size
        //[HarmonyPatch(typeof(Cauldron), "GetMainInputs")]
        //[HarmonyPrefix]
        public static bool GetMainInputsPatch(Cauldron __instance, out ItemInstance primaryItem, out int primaryItemQuantity, out ItemInstance secondaryItem, out int secondaryItemQuantity)
        {
            int cauldronCapacity = Mod.settings.GetStationCapacity("Cauldron");
            int stackSize = -1;
            List<ItemInstance> list = new List<ItemInstance>();
            Dictionary<ItemInstance, int> itemQuantities = new Dictionary<ItemInstance, int>();
            int i, k;
            for (i = 0; i < __instance.IngredientSlots.Length; i = k + 1)
            {
                if (__instance.IngredientSlots[i].ItemInstance != null)
                {
                    if (stackSize == -1)
                    {
                        stackSize = __instance.IngredientSlots[i].ItemInstance.StackLimit;
                    }
                    ItemInstance itemInstance = list.Find((ItemInstance x) => x.ID == __instance.IngredientSlots[i].ItemInstance.ID);
                    if (itemInstance == null || !itemInstance.CanStackWith(__instance.IngredientSlots[i].ItemInstance, false))
                    {
                        itemInstance = __instance.IngredientSlots[i].ItemInstance;
                        list.Add(itemInstance);
                        if (!itemQuantities.ContainsKey(__instance.IngredientSlots[i].ItemInstance))
                        {
                            itemQuantities.Add(__instance.IngredientSlots[i].ItemInstance, 0);
                        }
                    }
                    itemQuantities[itemInstance] += __instance.IngredientSlots[i].Quantity;
                }
                k = i;
            }
            for (int j = 0; j < list.Count; j++)
            {
                if (itemQuantities[list[j]] > stackSize)
                {
                    int num = itemQuantities[list[j]] - stackSize;
                    itemQuantities[list[j]] = stackSize;
                    ItemInstance copy = list[j].GetCopy(num);
                    list.Add(copy);
                    itemQuantities.Add(copy, num);
                }
            }
            list = (from x in list
                    orderby itemQuantities[x] descending
                    select x).ToList<ItemInstance>();
            if (list.Count > 0)
            {
                primaryItem = list[0];
                primaryItemQuantity = itemQuantities[list[0]];
            }
            else
            {
                primaryItem = null;
                primaryItemQuantity = 0;
            }
            if (list.Count > 1)
            {
                secondaryItem = list[1];
                secondaryItemQuantity = itemQuantities[list[1]];
                return false;
            }
            secondaryItem = null;
            secondaryItemQuantity = 0;

            return false;
        }

        // speed
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
    }
        
        /*

        // packaging station patches
        //[HarmonyPatch]
        public static class PackagingStationPatches
        {
            public static ProduceMoreMod Mod;
            // speed
            [HarmonyPatch(typeof(PackagingStationBehaviour), "BeginPackaging")]
            [HarmonyPrefix]
            public static void BeginPackagingPrefix(PackagingStationBehaviour __instance) 
            {
                // set PackagingStation.PackagerEmployeeSpeedMultiplier
                if (Mod.processedStationSpeeds.Contains(__instance.Station))
                {
                    __instance.Station.PackagerEmployeeSpeedMultiplier = 
                        (int)((float)__instance.Station.PackagerEmployeeSpeedMultiplier 
                         * Mod.settings.GetStationSpeed("PackagingStation"));
                    Mod.processedStationSpeeds.Add(__instance.Station);
                }

            }

            // capacity I think will like as itself?

        }

        /*
        // pot patches
        //[HarmonyPatch]
        public static class PotPatches
        {
            public static ProduceMoreMod Mod;

            // speed
            [HarmonyPatch(typeof(Pot), "GetAverageLightExposure")]
            public static void GetAverageLightExposurePostfix(Pot __instance, ref float growSpeedMultiplier, ref float __result)
            {
                growSpeedMultiplier = growSpeedMultiplier * Mod.settings.GetStationSpeed("Pot");
                __result = __result * Mod.settings.GetStationSpeed("Pot");
            }
        }
        */
}
