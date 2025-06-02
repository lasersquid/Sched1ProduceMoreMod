using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using HarmonyLib;
using System.Reflection;





#if MONO_BUILD
using ScheduleOne;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.StationFramework;
#else
using Il2CppScheduleOne;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.StationFramework;
#endif



[assembly: MelonInfo(typeof(ProduceMore.ProduceMoreMod), "ProduceMore", "1.0.2", "lasersquid", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ProduceMore
{
	
	public class ProduceMoreMod : MelonMod
	{
		public ProduceMoreMod()
		{
			unityComparer = new UnityObjectComparer();
			processedStationCapacities = new HashSet<GridItem>(unityComparer);
			processedStationSpeeds = new HashSet<GridItem>(unityComparer);
			processedStationTimes = new HashSet<GridItem>(unityComparer);
			processedItemDefs = new HashSet<ItemDefinition>(unityComparer);
			processedRecipes = new HashSet<StationRecipe>(unityComparer);

			originalStackLimits = new Dictionary<EItemCategory, int>();
			originalStationCapacities = new Dictionary<string, int>();
			originalStationTimes = new Dictionary<string, int>();
			originalRecipeTimes = new Dictionary<StationRecipe, int>(unityComparer);
		}

		public IEqualityComparer<UnityEngine.Object> unityComparer;
		public HashSet<GridItem> processedStationCapacities;
		public HashSet<GridItem> processedStationSpeeds;
		public HashSet<GridItem> processedStationTimes;
		public HashSet<ItemDefinition> processedItemDefs;
		public HashSet<StationRecipe> processedRecipes;
		public Dictionary<EItemCategory, int> originalStackLimits;
		public Dictionary<string, int> originalStationCapacities;
		public Dictionary<string, int> originalStationTimes;
		public Dictionary<StationRecipe, int> originalRecipeTimes;
		private bool needsReset = false;

		public ModSettings settings;
		public const string settingsFileName = "ProduceMoreSettings.json";
		public string settingsFilePath = Path.Combine(MelonEnvironment.UserDataDirectory, settingsFileName);

		public HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.lasersquid.producemore");

		public override void OnInitializeMelon()
		{
			LoadSettings();
			SaveSettings();
			SetMod();
			LoggerInstance.Msg("Initialized.");
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			if (sceneName.ToLower().Contains("main") || sceneName.ToLower().Contains("tutorial"))
			{
				needsReset = true;
			}

			if (sceneName.ToLower().Contains("menu"))
			{
				if (needsReset)
				{
					LoggerInstance.Msg("Menu loaded, resetting state.");
					ResetState();
				}
			}
		}

		private void ResetState()
		{
			RestoreDefaults();
			processedStationCapacities = new HashSet<GridItem>(unityComparer);
			processedStationSpeeds = new HashSet<GridItem>(unityComparer);
			processedStationTimes = new HashSet<GridItem>(unityComparer);
			processedItemDefs = new HashSet<ItemDefinition>(unityComparer);
			processedRecipes = new HashSet<StationRecipe>(unityComparer);
			needsReset = false;
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
		
		private List<Type> GetPatchTypes()
		{
			return  System.Reflection.Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(t => t.Name.EndsWith("Patches"))
				.ToList<Type>();
		}

		private void SetMod()
		{
            foreach (var t in GetPatchTypes())
            {
				MethodInfo method = t.GetMethod("SetMod", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
				method.Invoke(null, [this]);
            }
        }

		public void RestoreDefaults()
		{
            foreach (var t in GetPatchTypes())
            {
				try
				{
					MethodInfo method = t.GetMethod("RestoreDefaults", BindingFlags.Public | BindingFlags.Static);
					method.Invoke(null, null);
				}
				catch (Exception e)
				{
					LoggerInstance.Warning($"Couldn't restore defaults for class {t.Name}: {e.GetType().Name} - {e.Message}");
					LoggerInstance.Warning($"Source: {e.Source}");
					LoggerInstance.Warning($"{e.StackTrace}");
				}
            }
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

                foreach(KeyValuePair<string, int> entry in defaultSettings.stackOverrides)

                {
                    if (!fromFile.stackOverrides.ContainsKey(entry.Key))
                    {
                        fromFile.stackOverrides.Add(entry.Key, entry.Value);
                    }
                }


                //Trim malformed entries and do bounds checking
                var categoriesToRemove = new List<EItemCategory>();
				foreach (KeyValuePair<EItemCategory, int> entry in fromFile.stackSizes)
				{
					if (!defaultSettings.stackSizes.ContainsKey(entry.Key))
					{
						// can't change the entries of a list you're iterating over
						//fromFile.stackSizes.Remove(entry.Key);
						categoriesToRemove.Add(entry.Key);
					}

					if (entry.Value <= 0)
					{
						MelonLogger.Msg($"Settings file had stacklimit <= 0 for {entry.Key}, resetting to 1");
						fromFile.stackSizes[entry.Key] = 1;
					}
				}
				// there really should be a builtin for set subtraction, but whatevs.
				foreach (EItemCategory category in categoriesToRemove)
				{
					fromFile.stackSizes.Remove(category);
				}


				var stringsToRemove = new List<string>();
				foreach (KeyValuePair<string, float> entry in fromFile.stationSpeeds)
				{
					if (!defaultSettings.stationSpeeds.ContainsKey(entry.Key))
					{
						//fromFile.stationSpeeds.Remove(entry.Key);
						stringsToRemove.Add(entry.Key);
					}

					if (entry.Value < float.MinValue)
					{
						MelonLogger.Msg($"Settings file had speed <= 0 for {entry.Key}, resetting to 0.0001");
						fromFile.stationSpeeds[entry.Key] = 0.0001f;
					}
				}
				foreach(string key in stringsToRemove)
				{
					fromFile.stationSpeeds.Remove(key);
				}

				stringsToRemove.Clear();
				foreach (KeyValuePair<string, int> entry in fromFile.stationCapacities)
				{
					if (!defaultSettings.stationCapacities.ContainsKey(entry.Key))
					{
						//fromFile.stationCapacities.Remove(entry.Key);
						stringsToRemove.Add(entry.Key);
					}

					if (entry.Value <= 0)
					{
						MelonLogger.Msg($"Settings file had capacity <= 0 for {entry.Key}, resetting to 1");
						fromFile.stationSpeeds[entry.Key] = 1;
					}
				}
				foreach (string key in stringsToRemove)
				{
					fromFile.stationCapacities.Remove(key);
				}

				stringsToRemove.Clear();
				foreach (KeyValuePair<string, int> entry in fromFile.stackOverrides)
				{
					// have to do this validation after scene loads, since registry is not loaded at settings load time
					//if (!Registry.ItemExists(entry.Key) && !Registry.ItemExists(entry.Key.ToLower()))
					//{
					//	stringsToRemove.Add(entry.Key);
					//	MelonLogger.Msg($"Ignoring stack override that does not correspond to any known item: {entry.Key}");
					//}
					if (entry.Value <= 0)
					{
						fromFile.stackOverrides[entry.Key] = 1;
						MelonLogger.Msg($"Stack override for {entry.Key} <= 0; setting to 1");
					}
				}
				foreach (string key in stringsToRemove)
				{
					fromFile.stackOverrides.Remove(key);
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

			// Default stack overrides
			stackOverrides.Add("Acid", 10);
			stackOverrides.Add("Phosphorus", 10);
			stackOverrides.Add("Low-Quality Pseudo", 10);
			stackOverrides.Add("Pseudo", 10);
			stackOverrides.Add("High-Quality Pseudo", 10);
		}

		public int GetStackLimit(ItemInstance item)
		{
			int stackLimit = 10;

			if (!stackOverrides.TryGetValue(item.Name, out stackLimit))
			{
				EItemCategory category;
				if (item.Definition.Name == "Speed Grow")
				{
					category = EItemCategory.Growing;
				}
                else
                {
					category = item.Category;
                }

                if (!stackSizes.TryGetValue(category, out stackLimit))
				{
					MelonLogger.Msg($"Couldn't find stack size for item {item.Name} with category {category}");
				}
			}

			return stackLimit;
		}


		public int GetStackLimit(ItemDefinition itemDef)
		{
			int stackLimit = 10;
            if (!stackOverrides.TryGetValue(itemDef.Name, out stackLimit))
            {
				EItemCategory category;
				if (itemDef.Name == "Speed Grow")
				{
					category = EItemCategory.Growing;
				}
                else
                {
					category = itemDef.Category;
                }
                if (!stackSizes.TryGetValue(category, out stackLimit))
                {
                    MelonLogger.Msg($"Couldn't find stack size for item {itemDef.Name} with category {category}");
                }
            }

            return stackLimit;
        }

		public int GetStackLimit(string itemName, EItemCategory category)
		{
			int stackLimit = 10;

			if (!stackOverrides.TryGetValue(itemName, out stackLimit))
			{
				EItemCategory actualCategory;
				if (itemName == "Speed Grow")
				{
					actualCategory = EItemCategory.Growing;
				}
				else
				{
					actualCategory = category;
				}
				if (!stackSizes.TryGetValue(actualCategory, out stackLimit))
				{
					MelonLogger.Msg($"Couldn't find stack size for item {itemName} with category {actualCategory}");
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
// configurable via mod manager app
// separate mixingstation mk1 and mk2 - done
// speed up station animations:
//  - drying rack - done
//  - cauldron - done
//	- mixing station - done
//	- packaging station - done
//  - pot - done
//  - chem station 


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


// Bugs:
// - mixers have hard cap of 1s per item -- fixed
// - in multiplayer, guests can't start cauldrons until full original timer has elapsed -- needs testing
// - no tag for storage -- beta problem
// - mixers don't finish when time hits zero on high multiplier -- fixed
// - packagers still limited by animation time -- fixed, mostly. they're a lot faster now.
// - mixing station mk2 limited to mk1 speeds -- fixed
// - acid, pseudo, and phosphorous have default stacklimit of 20 -- fixed; should probably overhaul original item limit registration
// - laboven doesn't let you start a batch if any product is in the output -- fixed
// - cauldron doesn't let you start a batch if any product is in the output -- fixed


