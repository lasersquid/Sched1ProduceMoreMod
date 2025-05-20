using System.Collections;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.ItemFramework;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;
using MelonLoader.Utils;
using Newtonsoft.Json;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using Mono.Cecil;
using System;
using System.Linq;
using System.IO;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime;
using System.Linq;
using Il2CppSystem;


[assembly: MelonInfo(typeof(ProduceMore.ProduceMoreMod), "ProduceMore", "1.0.0", "lasersquid", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ProduceMore
{
    public class ProduceMoreMod : MelonMod
    {
		public HashSet<GridItem> processedStationCapacities = new HashSet<GridItem>();
		public HashSet<GridItem> processedStationSpeeds = new HashSet<GridItem>();
		public HashSet<ItemDefinition> processedDefs = new HashSet<ItemDefinition>();
		public ModSettings settings;
		public const string settingsFileName = "ProduceMoreSettings.json";

		public HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.lasersquid.producemore");

        public override void OnInitializeMelon()
        {
			LoadSettings();
			SetMod();
		//	ApplyTranspilerPatches();
            LoggerInstance.Msg($"ProduceMore mod initialized.");
        }

		public unsafe void ApplyTranspilerPatches()
		{
            // can I just wholesale jump to my own code while preserving context?
            // yes. https://stardewmodding.wiki.gg/wiki/Tutorial:_Harmony_Patching
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
		{
			if (!string.IsNullOrEmpty(sceneName) && !sceneName.ToLower().Contains("menu"))
			{
				LoggerInstance.Msg($"Scene {sceneName} was initialized.");
				//MelonCoroutines.Start(WaitForSceneInitialization());
			}
		}

		private void LoadSettings()
		{
			settings = ModSettings.LoadSettings(Path.Join(MelonEnvironment.ModsDirectory, settingsFileName));
			if (!File.Exists(Path.Join(MelonEnvironment.ModsDirectory, settingsFileName))) 
			{
				SaveSettings();
			}
		}

		private void SaveSettings()
		{
			if (settings == null)
			{
				LoadSettings();
			}
			if (settings != null)
			{
				settings.SaveSettings(Path.Join(MelonEnvironment.ModsDirectory, settingsFileName));
			}
		}
		
		private void SetMod()
		{
			ItemInstancePatches.Mod = this;
			DryingRackPatches.Mod = this;
			//LabOvenPatches.Mod = this;
            //MixingStationPatches.Mod = this;
            //BrickPressPatches.Mod = this;
			//CauldronPatches.Mod = this;
			//PackagingStationPatches.Mod = this;
			//PotPatches.Mod = this;
			
        }
	
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
                return JsonConvert.DeserializeObject<ModSettings>(json);
            }
            MelonLogger.Warning($"Couldn't find file {jsonPath}");

			return new ModSettings();
		}

		public void SaveSettings(string jsonPath)
		{
			string json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(jsonPath, json);
			MelonLogger.Msg($"Saved settings to file {jsonPath}");
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
			stationSpeeds.Add("MixingStationMk2", 1);
			stationSpeeds.Add("PackagingStation", 1);
			stationSpeeds.Add("BrickPress", 1);

			// Station processing capacities
			// Missing definition for chemistry station, since it doesn't have
			// a processing capacity per se.
			stationCapacities.Add("LabOven", 10);
			stationCapacities.Add("Cauldron", 20);
			stationCapacities.Add("DryingRack", 20);
			stationCapacities.Add("MixingStation", 20);
			stationCapacities.Add("MixingStationMk2", 20);
			stationCapacities.Add("PackagingStation", 20);
			stationCapacities.Add("BrickPress", 20);
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
//	- lab oven - done
//	- drying rack - done
//	- mixing station - done
//	- brick press - might do
//	- packaging station - done
//  - cauldron - done
// add plant growth multiplier - done
// increase stack limit in cauldron - done


// Testing:
// ItemInstancePatches - working
// DryingRackPatches - capacity working, speed working, wrote my first transpiler patch
// LabOvenPatches - 
// MixingStationPatches
// BrickPressPatches
// CauldronPatches
// PackagingStationPatches
// PotPatches
