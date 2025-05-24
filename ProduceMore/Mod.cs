using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.StationFramework;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.UI.Phone.Delivery;
using Il2CppSystem;
using MelonLoader;
using MelonLoader.Utils;
using Microsoft.Diagnostics.Runtime;
using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using static Il2CppScheduleOne.DevUtilities.ValueTracker;
using static Il2CppSystem.Threading.Timer;
using static System.Net.Mime.MediaTypeNames;


[assembly: MelonInfo(typeof(ProduceMore.ProduceMoreMod), "ProduceMore", "1.0.0", "lasersquid", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ProduceMore
{
	public class ProduceMoreMod : MelonMod
	{
		public HashSet<GridItem> processedStationCapacities = new HashSet<GridItem>();
		public HashSet<GridItem> processedStationSpeeds = new HashSet<GridItem>();
		public HashSet<ItemDefinition> processedDefs = new HashSet<ItemDefinition>();
		public HashSet<StationRecipe> processedRecipes = new HashSet<StationRecipe>();
		public ModSettings settings;
		public const string settingsFileName = "ProduceMoreSettings.json";
		public string settingsFilePath = Path.Combine(MelonEnvironment.UserDataDirectory, settingsFileName);

		public HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.lasersquid.producemore");

		public override void OnInitializeMelon()
		{
			LoadSettings();
			SetMod();
			LoggerInstance.Msg($"ProduceMore mod initialized.");
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			LoggerInstance.Msg($"Scene loaded: {sceneName}");
			if (sceneName.ToLower().Contains("menu"))
			{
				LoggerInstance.Msg("Menu loaded, resetting state.");
				processedStationCapacities = new HashSet<GridItem>();
				processedStationSpeeds = new HashSet<GridItem>();
				processedDefs = new HashSet<ItemDefinition>();
				processedRecipes = new HashSet<StationRecipe>();

				//LoggerInstance.Msg("Attempting late patching.");
				//ApplyTranspilerPatches();
				//LoggerInstance.Msg("Dump of ItemUiManager.UpdateCashDragAmount");
				//System.IntPtr handle = IL2CPP.GetIl2CppMethodByToken(Il2CppClassPointerStore<ItemUIManager>.NativeClassPtr, 100683405);
                //DumpBinary(handle, 1024);
			}
		}

        private void LoadSettings()
		{
			MelonLogger.Msg($"Settings loaded from {settingsFilePath}");
			settings = ModSettings.LoadSettings(settingsFilePath);
			if (!File.Exists(settingsFilePath))
			{
				SaveSettings();
			}
		}

		private void SaveSettings()
		{
			if (settings != null)
			{
				settings.SaveSettings(settingsFilePath);
			}
		}
		
		private void SetMod()
		{
			ItemCapacityPatches.Mod = this;
			DryingRackPatches.Mod = this;
			LabOvenPatches.Mod = this;
            MixingStationPatches.Mod = this;
            BrickPressPatches.Mod = this;
			CauldronPatches.Mod = this;
			PackagingStationPatches.Mod = this;
			PotPatches.Mod = this;
			ChemistryStationPatches.Mod = this;
			CashPatches.Mod = this;
        }
	
		/**
		private unsafe void DumpBinary(System.IntPtr handle, int bytes)
		{
			if (handle == System.IntPtr.Zero)
			{
				return;
			}

			// Read bytes into an array
			System.ReadOnlySpan<byte> byteSpan = new System.ReadOnlySpan<byte>((void *)handle, bytes);
			byte[] byteArray = byteSpan.ToArray();

            //int value = Marshal.ReadInt32(ptr);

            // Create the disassembler
            SharpDisasm.ArchitectureMode mode = SharpDisasm.ArchitectureMode.x86_32;
            SharpDisasm.Disassembler.Translator.IncludeAddress = true;
            SharpDisasm.Disassembler.Translator.IncludeBinary = true;

			SharpDisasm.Disassembler disasm = new SharpDisasm.Disassembler(byteArray, mode, 0, true);

            // Disassemble each instruction and output to console
            foreach (SharpDisasm.Instruction instruction in disasm.Disassemble())
                MelonLogger.Msg(instruction.ToString());
        }
		*/
	}

	public class ModSettings
	{
		// Stack size settings by category
		public Dictionary<EItemCategory, int> stackSizes = new Dictionary<EItemCategory, int>();

		// Stack size settings by item name
		public Dictionary<string, int> stackOverrides = new Dictionary<string, int>();

		// Station acceleration settings
		public Dictionary<string, float> stationSpeeds = new Dictionary<string, float>();

		// Station capacity settings
		public Dictionary<string, int> stationCapacities = new Dictionary<string, int>();

		public static ModSettings LoadSettings(string jsonPath)
		{
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                var fromFile = JsonConvert.DeserializeObject<ModSettings>(json);
				var defaultSettings = new ModSettings();

				foreach (KeyValuePair<EItemCategory, int> entry in defaultSettings.stackSizes)
				{
					if (!fromFile.stackSizes.ContainsKey(entry.Key))
					{
						fromFile.stackSizes.Add(entry.Key, entry.Value);
					}
				}

				foreach (KeyValuePair<string, float> entry in defaultSettings.stationSpeeds)
				{
					if (!fromFile.stationSpeeds.ContainsKey(entry.Key))
					{
						fromFile.stationSpeeds.Add(entry.Key, entry.Value);
					}
				}

				foreach (KeyValuePair<string, int> entry in defaultSettings.stationCapacities)
				{
					if (!fromFile.stationCapacities.ContainsKey(entry.Key))
					{
						fromFile.stationCapacities.Add(entry.Key, entry.Value);
					}
				}

				//Trim malformed entries
				foreach (KeyValuePair<EItemCategory, int> entry in fromFile.stackSizes)
				{
					if (!defaultSettings.stackSizes.ContainsKey(entry.Key))
					{
						fromFile.stackSizes.Remove(entry.Key);
					}
				}

				foreach (KeyValuePair<string, float> entry in fromFile.stationSpeeds)
				{
					if (!defaultSettings.stationSpeeds.ContainsKey(entry.Key))
					{
						fromFile.stationSpeeds.Remove(entry.Key);
					}
				}

				foreach (KeyValuePair<string, int> entry in fromFile.stationCapacities)
				{
					if (!defaultSettings.stationCapacities.ContainsKey(entry.Key))
					{
						fromFile.stationCapacities.Remove(entry.Key);
					}
				}

				return fromFile;
            }

			return new ModSettings();
		}


		public void SaveSettings(string jsonPath)
		{
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(jsonPath, json);
		}

		public void PrintSettings()
		{
			MelonLogger.Msg("Settings:");
			MelonLogger.Msg(JsonConvert.SerializeObject(this, Formatting.Indented));
		}
		public ModSettings()
		{
			// Default stack sizes
			stackSizes.Add(EItemCategory.Cash, 1000);
			stackSizes.Add(EItemCategory.Clothing, 1);
			stackSizes.Add(EItemCategory.Consumable, 20);
			stackSizes.Add(EItemCategory.Decoration, 1);
			stackSizes.Add(EItemCategory.Equipment, 10);
			stackSizes.Add(EItemCategory.Furniture, 10);
			stackSizes.Add(EItemCategory.Growing, 10);
			stackSizes.Add(EItemCategory.Ingredient, 20);
			stackSizes.Add(EItemCategory.Lighting, 10);
			stackSizes.Add(EItemCategory.Packaging, 20);
			stackSizes.Add(EItemCategory.Product, 20);
			stackSizes.Add(EItemCategory.Tools, 1);

			// Default station speed multipliers
			stationSpeeds.Add("LabOven", 1);
			stationSpeeds.Add("Cauldron", 1);
			stationSpeeds.Add("ChemistryStation", 1);
			stationSpeeds.Add("DryingRack", 1);
			stationSpeeds.Add("MixingStation", 1);
			stationSpeeds.Add("PackagingStation", 1);
			stationSpeeds.Add("Pot", 1);

			// Station processing capacities
			stationCapacities.Add("DryingRack", 20);
			stationCapacities.Add("MixingStation", 20);
			stationCapacities.Add("PackagingStation", 20);
		}
		public int GetStackLimit(ItemInstance item)
		{
			int stackLimit = 10;

			if (!stackOverrides.TryGetValue(item.Name, out stackLimit))
			{
				if (!stackSizes.TryGetValue(item.Category, out stackLimit))
				{
					MelonLogger.Msg($"Couldn't find stack size for item {item.Name} with category {item.Category}");
				}
			}

			return stackLimit;
		}


		public int GetStackLimit(ItemDefinition itemDef)
		{
			int stackLimit = 10;
            if (!stackOverrides.TryGetValue(itemDef.Name, out stackLimit))
            {
                if (!stackSizes.TryGetValue(itemDef.Category, out stackLimit))
                {
                    MelonLogger.Msg($"Couldn't find stack size for item {itemDef.Name} with category {itemDef.Category}");
                }
            }

            return stackLimit;
        }

		public int GetStackLimit(string itemName, EItemCategory category)
		{
			int stackLimit = 10;

			if (!stackOverrides.TryGetValue(itemName, out stackLimit))
			{
				if (!stackSizes.TryGetValue(category, out stackLimit))
				{
					MelonLogger.Msg($"Couldn't find stack size for item {itemName} with category {category}");
				}
			}
			return stackLimit;
		}
		public int GetStackLimit(EItemCategory category)
		{
			int stackLimit = 10;

			if (!stackSizes.TryGetValue(category, out stackLimit))
			{ 
				MelonLogger.Msg($"Couldn't find stack size for category {category}");
			}
			return stackLimit;
		}

		public int GetStationCapacity(string station)
		{
			int capacity = 10;

			if (!stationCapacities.TryGetValue(station, out capacity))
			{
				MelonLogger.Msg($"Couldn't find station capacity for {station}");
			}

			return capacity;
		}

		public float GetStationSpeed(string station)
		{
			float speed = 1f;

			if (!stationSpeeds.TryGetValue(station, out speed))
			{
				MelonLogger.Msg($"Couldn't find station speed for {station}");
			}

			return speed;

		}
	}
}

// increase stack limits by category, with individual overrides - done
// increase processing limit and speed of lab stations
//	- chem station (possible/sensical?) - might do
//	- lab oven - speed done; won't do capacity
//	- drying rack - done - speed & capacity
//	- mixing station - done - speed & capacity
//	- brick press - might do
//	- packaging station - done
//  - cauldron - done - speed; capacity does itself
// add plant growth multiplier - done
// cash stack size - testing
// detect malformed settings - done
// keep dict of default settings - done
// refresh processed<stations/recipes> when returning to title screen/on settings update - done
// phone app for configuration - next ver
// support for changing settings on the fly - backend logic ready; still needs interface to test (next ver)
// injecting into savefile - next ver


// Testing:
// ItemInstancePatches - testing listingentry patch for shops
// ChemistryStationPatches - speed working; UI needs update
// DryingRackPatches - working
// LabOvenPatches - working
// MixingStationPatches - working, except worker sometimes gets stuck? might not be my fault
// BrickPressPatches - working
// CauldronPatches - working
// PackagingStationPatches - working
// PotPatches - working
// CashPatches - in testing

// Bugs:
// - can't enter quantity in shops
// - if you shift-click a stack of cash, your inventory cash sometimes deposits itself into the slot -- FIXED
