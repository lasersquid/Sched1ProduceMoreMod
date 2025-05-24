using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using HarmonyLib;


#if MONO_BUILD
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.StationFramework;
#else
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.StationFramework;
#endif



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
			LoggerInstance.Msg("Initialized.");
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			if (sceneName.ToLower().Contains("menu"))
			{
				LoggerInstance.Msg("Menu loaded, resetting state.");
				ResetState();
			}
		}

		private void ResetState()
		{
			processedStationCapacities = new HashSet<GridItem>();
			processedStationSpeeds = new HashSet<GridItem>();
			processedDefs = new HashSet<ItemDefinition>();
			processedRecipes = new HashSet<StationRecipe>();
		}

		private void LoadSettings()
		{
			LoggerInstance.Msg($"Loading settings from {settingsFilePath}");
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
			// this was so elegant. alas, AccessToolsExtensions is not available in HarmonyX
			// (and I don't know how to select harmonylib 2.3.6 over harmonyx 2.10.12 at runtime in a class library)
			/*
			Type[] types =
                (Type[])System.Reflection.Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(t => t.Namespace.StartsWith("ProduceMore") && t.Name.EndsWith("Patches"));

            foreach (var t in types)
            {
				LoggerInstance.Msg($"Setting Mod for {t.Name}");
				t.StaticFieldRefAccess<ProduceMoreMod>("Mod") = this;
            }
			*/

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
                ModSettings fromFile = JsonConvert.DeserializeObject<ModSettings>(json);
				ModSettings defaultSettings = new ModSettings();

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
			File.WriteAllText(jsonPath, this.ToString());
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public void PrintSettings()
		{
			MelonLogger.Msg("Settings:");
			MelonLogger.Msg($"{this}");
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

			// Default station processing capacities
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
//	- chem station - speed done; won't do capacity/batchsize
//	- lab oven - speed done; won't do capacity/batchsize
//	- drying rack - done
//	- mixing station - done
//	- brick press - won't do
//	- packaging station - done
//  - cauldron - speed done; won't do capacity/batchsize
// add plant growth multiplier - done
// cash stack size - done
// detect malformed settings - done
// keep dict of default settings - done
// refresh processed<stations/recipes> when returning to title screen/on settings update - done
// phone app for configuration - next ver
// support for changing settings on the fly - backend logic ready; still needs interface to test (next ver)
// injecting into savefile - next ver


// Testing:
// IL2CPP:
//		ItemInstancePatches - working
//		ChemistryStationPatches - working
//		DryingRackPatches - working
//		LabOvenPatches - working
//		MixingStationPatches - working
//		BrickPressPatches - empty
//		CauldronPatches - working
//		PackagingStationPatches - working
//		PotPatches - working
//		CashPatches - working
// Mono:
//		ItemInstancePatches - working
//		ChemistryStationPatches - working
//		DryingRackPatches - working
//		LabOvenPatches - working
//		MixingStationPatches - working
//		BrickPressPatches - empty
//		CauldronPatches - working
//		PackagingStationPatches - working
//		PotPatches - working
//		CashPatches - working

// IL2CPP build after Mono modifications:
//		CauldronPatches - needs testing
//		CashPatches - needs testing


// Bugs:
// - can't enter quantity in shops -- not my bug
// - if you shift-click a stack of cash, your inventory cash sometimes deposits itself into the slot -- FIXED
