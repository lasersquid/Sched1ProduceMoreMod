using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using HarmonyLib;
using System.Reflection;




#if MONO_BUILD
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.StationFramework;
#else

using Il2CppScheduleOne;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.StationFramework;
#endif



[assembly: MelonInfo(typeof(ProduceMore.ProduceMoreMod), "ProduceMore", "1.0.8", "lasersquid", null)]
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

			originalStackLimits = new Dictionary<string, int>();
			originalStationCapacities = new Dictionary<string, int>();
			originalStationTimes = new Dictionary<string, int>();
			originalRecipeTimes = new Dictionary<StationRecipe, int>(unityComparer);

			registeredEmployees = new HashSet<NPC>(unityComparer);
		}

		public MelonPreferences_Category stationSpeeds;
		public MelonPreferences_Category stationCapacities;
		public MelonPreferences_Category employeeAnimation;
		public MelonPreferences_Category stackSizes;
		public MelonPreferences_Category stackOverrides;

		public IEqualityComparer<UnityEngine.Object> unityComparer;
		public HashSet<GridItem> processedStationCapacities;
		public HashSet<GridItem> processedStationSpeeds;
		public HashSet<GridItem> processedStationTimes;
		public HashSet<ItemDefinition> processedItemDefs;
		public HashSet<StationRecipe> processedRecipes;
		public Dictionary<string, int> originalStackLimits;
		public Dictionary<string, int> originalStationCapacities;
		public Dictionary<string, int> originalStationTimes;
		public Dictionary<StationRecipe, int> originalRecipeTimes;
		public HashSet<NPC> registeredEmployees;

		private bool needsReset = false;

		public HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.lasersquid.producemore");

		public override void OnInitializeMelon()
		{
			InitializeMelonPreferences();
			SetMod();
			LoggerInstance.Msg("Initialized.");
		}

		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			base.OnSceneWasLoaded(buildIndex, sceneName);
			if (sceneName.ToLower().Contains("main") || sceneName.ToLower().Contains("tutorial"))
			{
				//needsReset = true;
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

		public override void OnPreferencesSaved()
		{
			base.OnPreferencesSaved();
			ResetState();
		}

		private void ResetState()
		{
			//RestoreDefaults(); // needed??
			processedStationCapacities = new HashSet<GridItem>(unityComparer);
			processedStationSpeeds = new HashSet<GridItem>(unityComparer);
			processedStationTimes = new HashSet<GridItem>(unityComparer);
			processedItemDefs = new HashSet<ItemDefinition>(unityComparer);
			processedRecipes = new HashSet<StationRecipe>(unityComparer);
			needsReset = false;
		}


		private void InitializeMelonPreferences()
		{
			stationSpeeds = MelonPreferences.CreateCategory("ProduceMore_01_station_speeds", "Station Speeds (1=normal, 2=double, 0.5=half)");
			employeeAnimation = MelonPreferences.CreateCategory("ProduceMore_02_employee_animation", "Employee Work Speeds (1=normal, 2=double, 0.5=half)");
			stationCapacities = MelonPreferences.CreateCategory("ProduceMore_03_station_capacities", "Station Capacities");
			stackSizes = MelonPreferences.CreateCategory("ProduceMore_04_stack_sizes", "Stack Limits (by category)");
			stackOverrides = MelonPreferences.CreateCategory("ProduceMore_05_stack_overrides", "Stack Limit Overrides");

			stationSpeeds.SetFilePath("UserData/ProduceMore.cfg");
			stationCapacities.SetFilePath("UserData/ProduceMore.cfg");
			employeeAnimation.SetFilePath("UserData/ProduceMore.cfg");
			stackSizes.SetFilePath("UserData/ProduceMore.cfg");
			stackOverrides.SetFilePath("UserData/ProduceMore.cfg");

			stationSpeeds.CreateEntry<float>("LabOven", 1f, "Lab Oven", false);
			stationSpeeds.CreateEntry<float>("Cauldron", 1f, "Cauldron", false);
			stationSpeeds.CreateEntry<float>("BrickPress", 1f, "Brick Press", false);
			stationSpeeds.CreateEntry<float>("ChemistryStation", 1f, "Chemistry Station", false);
			stationSpeeds.CreateEntry<float>("DryingRack", 1f, "Drying Rack", false);
			stationSpeeds.CreateEntry<float>("MixingStation", 1f, "Mixing Station", false);
			stationSpeeds.CreateEntry<float>("MixingStationMk2", 1f, "Mixing Station Mk2", false);
			stationSpeeds.CreateEntry<float>("PackagingStation", 1f, "Packaging Station", false);
			stationSpeeds.CreateEntry<float>("PackagingStationMk2", 1f, "Packaging Station Mk2", false);
			stationSpeeds.CreateEntry<float>("Pot", 1f, "Pot", false);

			stationCapacities.CreateEntry<int>("DryingRack", 20, "Drying Rack", false);
			stationCapacities.CreateEntry<int>("MixingStation", 10, "Mixing Station", false);
			stationCapacities.CreateEntry<int>("MixingStationMk2", 20, "Mixing Station Mk2", false);
			stationCapacities.CreateEntry<int>("PackagingStation", 20, "Packaging Station", false);
			stationCapacities.CreateEntry<int>("PackagingStationMk2", 20, "Packaging Station Mk2", false);

			employeeAnimation.CreateEntry<float>("employeeWalkAcceleration", 1f, "Employee walk speed modifier", false);
			employeeAnimation.CreateEntry<float>("LabOvenAcceleration", 1f, "Lab Oven animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("CauldronAcceleration", 1f, "Cauldron animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("BrickPressAcceleration", 1f, "Brick Press animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("ChemistryStationAcceleration", 1f, "Chemistry Station animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("DryingRackAcceleration", 1f, "Drying Rack animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("MixingStationAcceleration", 1f, "Mixing Station animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("MixingStationMk2Acceleration", 1f, "Mixing Station Mk2 animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("PackagingStationAcceleration", 1f, "Packaging Station animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("PackagingStationMk2Acceleration", 1f, "Packaging Station Mk2 animation speed modifier", false);
			employeeAnimation.CreateEntry<float>("PotAcceleration", 1f, "Pot animation speed modifier", false);

			stackSizes.CreateEntry<int>("Agriculture", 10, "Agriculture", false);
			stackSizes.CreateEntry<int>("Cash", 1000, "Cash", false);
			stackSizes.CreateEntry<int>("Clothing", 1, "Clothing", false);
			stackSizes.CreateEntry<int>("Consumable", 20, "Consumable", false);
			stackSizes.CreateEntry<int>("Decoration", 1, "Decoration", false);
			stackSizes.CreateEntry<int>("Equipment", 10, "Equipment", false);
			stackSizes.CreateEntry<int>("Furniture", 10, "Furniture", false);
			stackSizes.CreateEntry<int>("Ingredient", 20, "Ingredient", false);
			stackSizes.CreateEntry<int>("Lighting", 10, "Lighting", false);
			stackSizes.CreateEntry<int>("Packaging", 20, "Packaging", false);
			stackSizes.CreateEntry<int>("Product", 20, "Product", false);
			stackSizes.CreateEntry<int>("Storage", 10, "Storage", false);
			stackSizes.CreateEntry<int>("Tools", 1, "Tools", false);

			stackOverrides.CreateEntry<int>("Acid", 10, "Acid", false);
			stackOverrides.CreateEntry<int>("Phosphorus", 10, "Phosphorus", false);
			stackOverrides.CreateEntry<int>("Low-Quality Pseudo", 10, "Low-Quality Pseudo", false);
			stackOverrides.CreateEntry<int>("Pseudo", 10, "Pseudo", false);
			stackOverrides.CreateEntry<int>("High-Quality Pseudo", 10, "High-Quality Pseudo", false);
			stackOverrides.CreateEntry<int>("Shotgun Shell", 10, "Shotgun Shell", false);
			stackOverrides.CreateEntry<int>("M1911 Magazine", 10, "M1911 Magazine", false);
			stackOverrides.CreateEntry<int>("Revolver Cylinder", 10, "Revolver Cylinder", false);
			stackOverrides.CreateEntry<int>("Spray Paint", 10, "Spray Paint", false);
			stackOverrides.CreateEntry<int>("Graffiti Cleaner", 10, "Graffiti Cleaner", false);

			MelonPreferences.Save();
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
		public int GetStackLimit(ItemInstance item)
		{
			int stackLimit = 10;
			if (item == null)
			{
				return 0;
			}
			if (stackOverrides.GetEntry<int>(item.Name) != null)
			{
				stackLimit = stackOverrides.GetEntry<int>(item.Name).Value;
			}
			else
			{
				EItemCategory category;
				if (item.Definition.Name == "Speed Grow")
				{
					category = EItemCategory.Agriculture;
				}
				else
				{
					category = item.Category;
				}

				if (stackSizes.GetEntry<int>(category.ToString()) != null)
				{
					stackLimit = stackSizes.GetEntry<int>(category.ToString()).Value;
				}
				else
				{
					MelonLogger.Msg($"Couldn't find stack size for item {item.Name} with category {category}, assuming 10");
				}
			}

			return stackLimit;
		}
		public int GetStackLimit(ItemDefinition itemDef)
		{
			int stackLimit = 10;
            if (stackOverrides.GetEntry<int>(itemDef.Name) != null)
			{
				stackLimit = stackOverrides.GetEntry<int>(itemDef.Name).Value;
			}
			else
			{
				EItemCategory category;
				if (itemDef.Name == "Speed Grow")
				{
					category = EItemCategory.Agriculture;
				}
				else
				{
					category = itemDef.Category;
				}
				if (stackSizes.GetEntry<int>(category.ToString()) != null)
				{
					stackLimit = stackSizes.GetEntry<int>(category.ToString()).Value;
				}
				else
				{
					MelonLogger.Msg($"Couldn't find stack size for item {itemDef.Name} with category {category}");
				}
			}

            return stackLimit;
        }

		public int GetStackLimit(string itemName, EItemCategory category)
		{
			int stackLimit = 10;

			if (stackOverrides.GetEntry<int>(itemName) != null)
			{
				stackLimit = stackOverrides.GetEntry<int>(itemName).Value;
			}
			else
			{
				EItemCategory actualCategory;
				if (itemName == "Speed Grow")
				{
					actualCategory = EItemCategory.Agriculture;
				}
				else
				{
					actualCategory = category;
				}
				if (stackSizes.GetEntry<int>(actualCategory.ToString()) != null)
				{
					stackLimit = stackSizes.GetEntry<int>(actualCategory.ToString()).Value;
				}
				else
				{
					MelonLogger.Msg($"Couldn't find stack size for item {itemName} with category {actualCategory}");
				}
			}
			return stackLimit;
		}

		public int GetStackLimit(EItemCategory category)
		{
			int stackLimit = 10;

			if (stackSizes.GetEntry<int>(category.ToString()) != null)
			{
				stackLimit = stackSizes.GetEntry<int>(category.ToString()).Value;
			}
			else
			{
				MelonLogger.Msg($"Couldn't find stack size for category {category}");
			}
			return stackLimit;
		}


        public int GetStationCapacity(string station)
        {
            int capacity = 10;
            if (stationCapacities.GetEntry<int>(station) != null)
			{
				capacity = stationCapacities.GetEntry<int>(station).Value;
			}
			else
			{
				MelonLogger.Msg($"Couldn't find station capacity for {station}");
			}

            return capacity;
        }

        public float GetStationSpeed(string station)
        {
            float speed = 1f;

            if (stationSpeeds.GetEntry<float>(station) != null)
			{
				speed = stationSpeeds.GetEntry<float>(station).Value;
			}
			else
			{
				MelonLogger.Msg($"Couldn't find station speed modifier for {station}");
			}

            return speed;
        }

        public float GetStationWorkSpeed(string station)
        {
            float speed = 1f;

            if (employeeAnimation.GetEntry<float>($"{station}Acceleration") != null)
			{
				speed = employeeAnimation.GetEntry<float>($"{station}Acceleration").Value;
			}
			else
			{
				MelonLogger.Msg($"Couldn't find employee work speed modifier for {station}");
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
// configurable via mod manager app - maaaaaaybe
// separate mixingstation mk1 and mk2 - done
// speed up station animations:
//  - drying rack - done
//  - cauldron - done
//	- mixing station - done
//	- packaging station - done
//  - pot - done
//  - chem station - done
//	- brick press - done
// speed up cleaners - maybe
// increased batch size for cauldron, laboven, and chemistry station - maybe
// automatically migrate settings between version updates - done
// employee walk speed multiplier - done
// v0.3.6 update - done
// increase size of shop item quantity text box - done
// increase purchase limit in shops to 999999 - done
// move bedless and worklate features to new mod - done
// move increased purchase limit to new mod - done
// fix bug where employees got stuck next to their destination - done
// v0.4.0 update - done
// rework employee walk speed multiplier - done
// figure out why cleaners keep getting stuck - done; moved to own mod
// separate mk1 and mk2 packaging stations - done
// separate station processing speed and employee work speed into own settings categories - done
// use melonpreferences for settings - done

// Testing:
// IL2CPP:
//		ItemInstancePatches - working
//		RegistryPatches - working
//		ChemistryStationPatches - working
//		DryingRackPatches - working
//		LabOvenPatches - working
//		MixingStationPatches - working
//		BrickPressPatches - working
//		CauldronPatches - working
//		PackagingStationPatches - working
//		PotPatches - working
//		CashPatches - working
//		NPCMovementPatches - needs testing
// Mono:
//		ItemInstancePatches - working
//		RegistryPatches - working
//		ChemistryStationPatches - working
//		DryingRackPatches - working
//		LabOvenPatches - working
//		MixingStationPatches - working
//		BrickPressPatches - working
//		CauldronPatches - working
//		PackagingStationPatches - working
//		PotPatches - working
//		CashPatches - working
//		NPCMovementPatches - working


// Bugs:
//	- Employees get stuck stopped by their destination, but won't proceed until interacted with -- fixed
//	- Employees get stuck oscillating at narrow gaps when walk speed is turned up -- fixed

