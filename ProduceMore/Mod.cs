using MelonLoader;
using HarmonyLib;

#if MONO_BUILD
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.StationFramework;
#else
using Il2CppScheduleOne.EntityFramework;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.StationFramework;
#endif



[assembly: MelonInfo(typeof(ProduceMore.ProduceMoreMod), "ProduceMore", "1.1.6", "lasersquid", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ProduceMore
{
    public class ProduceMoreMod : MelonMod
    {
        public ProduceMoreMod()
        {
            unityComparer = new Utils.UnityObjectComparer();
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
            runningCoroutines = new List<object>();
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
        public List<object> runningCoroutines;
        public bool plantsAlwaysGrowPresent = false;
        public bool checkedMelons = false;

        public HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.lasersquid.producemore");

        public override void OnInitializeMelon()
        {
            InitializeMelonPreferences();
            Utils.Initialize(this);
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            if (sceneName.ToLower().Contains("main") || sceneName.ToLower().Contains("tutorial"))
            {
                if (!checkedMelons)
                {
                    plantsAlwaysGrowPresent = Utils.OtherModIsLoaded("PlantsAlwaysGrowMod");
                    checkedMelons = true;
                }
                Utils.LateInitialize();
            }
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasUnloaded(buildIndex, sceneName);
            if (sceneName.ToLower().Contains("main") || sceneName.ToLower().Contains("tutorial"))
            {
                ResetState();
            }
        }

        public void StopCoroutines()
        {
            foreach (object coroutine in runningCoroutines)
            {
                if (coroutine != null)
                {
                    MelonCoroutines.Stop(coroutine);
                }
            }
            runningCoroutines.Clear();
        }

        public override void OnPreferencesSaved()
        {
            base.OnPreferencesSaved();
            ResetState();
        }

        private void ResetState()
        {
            processedStationCapacities = new HashSet<GridItem>(unityComparer);
            processedStationSpeeds = new HashSet<GridItem>(unityComparer);
            processedStationTimes = new HashSet<GridItem>(unityComparer);
            processedItemDefs = new HashSet<ItemDefinition>(unityComparer);
            processedRecipes = new HashSet<StationRecipe>(unityComparer);
        }

        private void InitializeMelonPreferences()
        {
            stationSpeeds = MelonPreferences.CreateCategory("ProduceMore_01_station_speeds", "Station Speeds (1=normal, 2=double, 0.5=half)");
            employeeAnimation = MelonPreferences.CreateCategory("ProduceMore_02_employee_animation", "Employee Speed (1=normal, 2=double, 0.5=half)");
            stationCapacities = MelonPreferences.CreateCategory("ProduceMore_03_station_capacities", "Station Capacities");
            stackSizes = MelonPreferences.CreateCategory("ProduceMore_04_stack_sizes", "Stack Limits (by category)");
            stackOverrides = MelonPreferences.CreateCategory("ProduceMore_05_stack_overrides", "Stack Limit Overrides");

            stationSpeeds.SetFilePath("UserData/ProduceMore.cfg", true, false);
            stationCapacities.SetFilePath("UserData/ProduceMore.cfg", true, false);
            employeeAnimation.SetFilePath("UserData/ProduceMore.cfg", true, false);
            stackSizes.SetFilePath("UserData/ProduceMore.cfg", true, false);
            stackOverrides.SetFilePath("UserData/ProduceMore.cfg", true, false);

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
            stationSpeeds.CreateEntry<float>("MushroomBed", 1f, "Mushroom Bed", false);

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
            employeeAnimation.CreateEntry<float>("MushroomBedAcceleration", 1f, "Mushroom Bed animation speed modifier", false);
            employeeAnimation.CreateEntry<float>("PackagingStationAcceleration", 1f, "Packaging Station animation speed modifier", false);
            employeeAnimation.CreateEntry<float>("PackagingStationMk2Acceleration", 1f, "Packaging Station Mk2 animation speed modifier", false);
            employeeAnimation.CreateEntry<float>("PotAcceleration", 1f, "Pot animation speed modifier", false);
            employeeAnimation.CreateEntry<float>("SpawnStationAcceleration", 1f, "Spawn Station animation speed modifier", false);

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

            stationSpeeds.SaveToFile(false);
            stationCapacities.SaveToFile(false);
            employeeAnimation.SaveToFile(false);
            stackSizes.SaveToFile(false);
            stackOverrides.SaveToFile(false);
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
}

// increase stack limits by category, with individual overrides - done
// increase processing limit and speed of lab stations
//    - chem station - speed done; won't do capacity/batchsize
//    - lab oven - speed done; won't do capacity/batchsize
//    - drying rack - done
//    - mixing station - done
//    - brick press - won't do
//    - packaging station - done
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
//    - mixing station - done
//    - packaging station - done
//  - pot - done
//  - chem station - done
//    - brick press - done
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
// really, *actually* cleanly shutdown coroutines on quit to menu - done
// use packaging station mk2 speed for packaging station mk2 - done
// rework employee walk speed multiplier, again - done
// make utils properly generic - done
// shrooms update -- rework PotPatches into GrowContainerPatches - done
// shrooms update -- mushroom bed acceleration - done
// shrooms update -- spawn station acceleration - done
// fix melonpreferences bug where changes are not picked up until reload - done (v1.1.0)
// fix using getfield instead of getproperty for growcontainerbehaviour._botanist - done (v1.1.1)
// fix (or at least improve) employees getting stuck - improved
// fix cauldron output capacity check - done
// find earlier hook to stop coroutines than OnSceneChanged - done (v1.1.2)
// fix cauldrons completing instantly - done
// fix mixing stations completing instantly - done
// fix drying rack dry time acceleration not working - done
// fix chemist starting new chemistry station operation without checking space in output - not my bug, but done
// fix NPC item capacity checks - done
// convert as many destructive patches as possible to non-destructive ones - done (v1.1.3)
// fix mixing station dinging infinitely on single mix at 2x multiplier - done
// fix not able to harvest weed (UE shows growth percentage as -0.999993764) - done
// fix dryingoperationui.updateposition postfix to not assume rack will be non-null - done
// fix plants to use potacceleration instead of mushroombedacceleration - done
// fix broken plant/mushroom acceleration - done
// fix broken chem station acceleration - done
// fix mixing station mk2 display time - done
// improve lag spike on game tick - done (v1.1.4)
// fix chemistrystationcanvas not loading properly - done
// fix chemistrystation cook time labels not always updating - done
// fix divide-by-zero when mushroombed acceleration is 1 - done
// fix chemists getting stuck at mixing stations - mostly fixed; still vulnerable to proximity bugs
// fix plant/shroom growth getting stuck at 99% - done (v1.1.5)
// update for 0.4.3 - done (v1.1.6)


// Bugs:
