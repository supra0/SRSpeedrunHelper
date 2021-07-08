using System.Collections.Generic;
using UnityEngine;
using MonomiPark.SlimeRancher.Regions;

namespace SRSpeedrunHelper
{
    public class WarpData
    {
        #region Predefined Warps
        // Ranch
        public static readonly WarpData RANCH_HOUSE = new WarpData(new Vector3(89.3f, 14.7f, -144.5f), new Vector3(0.0f, 24.9f, 0.0f), "House", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData RANCH_GROTTO = new WarpData(new Vector3(204.9f, 12.8f, -127.4f), new Vector3(0.0f, 94.3f, 0.0f), "Grotto", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData RANCH_OVERGROWTH = new WarpData(new Vector3(-13.4f, 14.9f, -142.4f), new Vector3(0.0f, 270.5f, 0.0f), "Overgrowth", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData RANCH_DOCKS = new WarpData(new Vector3(-87.5f, -1.1f, -257.9f), new Vector3(0.0f, 217.0f, 0.0f), "Docks", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData RANCH_LAB = new WarpData(new Vector3(195.5f, 15.1f, -303.8f), new Vector3(0.0f, 180.0f, 0.0f), "Lab", RegionRegistry.RegionSetId.HOME);

        // Dry Reef
        public static readonly WarpData DRY_REEF_MAIN = new WarpData(new Vector3(-5.7f, 11.9f, 42.1f), new Vector3(0.0f, 54.1f, 0.0f), "Main", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData DRY_REEF_RING_ISLAND = new WarpData(new Vector3(-523.4f, -2.0f, -134.7f), new Vector3(0.0f, 240.9f, 0.0f), "Ring Island", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData DRY_REEF_BRIDGE = new WarpData(new Vector3(-266.8f, -1.7f, 167.7f), new Vector3(0.0f, 359.8f, 0.0f), "Bridge", RegionRegistry.RegionSetId.HOME);

        // Moss Blanket
        public static readonly WarpData MOSS_BLANKET_ENTRANCE = new WarpData(new Vector3(-150.5f, 15.7f, 367.6f), new Vector3(0.0f, 0.0f, 0.0f), "Entrance", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData MOSS_BLANKET_CLEARING = new WarpData(new Vector3(-295.1f, 0.4f, 395.5f), new Vector3(0.0f, 0.0f, 0.0f), "Clearing", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData MOSS_BLANKET_HONEY_SLIMES = new WarpData(new Vector3(-270.5f, 0.0f, 552.6f), new Vector3(0.0f, 339.0f, 0.0f), "Honey Slimes", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData MOSS_BLANKET_LAKE = new WarpData(new Vector3(-432.3f, 6.4f, 619.7f), new Vector3(0.0f, 195.0f, 0.0f), "Lake", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData MOSS_BLANKET_HUNTER_GORDO = new WarpData(new Vector3(-389.8f, 9.3f, 848.8f), new Vector3(0.0f, 322.6f, 0.0f), "Hunter Gordo", RegionRegistry.RegionSetId.HOME);

        // Indigo Quarry
        public static readonly WarpData INDIGO_QUARRY_ENTRANCE = new WarpData(new Vector3(85.0f, 8.2f, 189.8f), new Vector3(0.0f, 90.0f, 0.0f), "Entrance", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData INDIGO_QUARRY_ROCK_GORDO_CAVE = new WarpData(new Vector3(207.3f, 5.6f, 178.4f), new Vector3(0.0f, 90.0f, 0.0f), "Rock Gordo Cave", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData INDIGO_QUARRY_BRIDGE = new WarpData(new Vector3(378.3f, 1.0f, 259.5f), new Vector3(0.0f, 6.3f, 0.0f), "Bridge", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData INDIGO_QUARRY_RAD_POND = new WarpData(new Vector3(239.8f, 12.1f, 300.6f), new Vector3(0.0f, 273.6f, 0.0f), "Rad Pond", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData INDIGO_QUARRY_ASH_ISLE = new WarpData(new Vector3(380.5f, -4.2f, 636.7f), new Vector3(0.0f, 28.6f, 0.0f), "Ash Isle", RegionRegistry.RegionSetId.HOME);

        // Ancient Ruins
        public static readonly WarpData ANCIENT_RUINS_COURTYARD = new WarpData(new Vector3(10.0f, 4.4f, 471.9f), new Vector3(0.0f, 25.9f, 0.0f), "Courtyard", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData ANCIENT_RUINS_CENTER = new WarpData(new Vector3(69.8f, 20.7f, 679.9f), new Vector3(0.0f, 0.0f, 0.0f), "Center", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData ANCIENT_RUINS_ROOFTOP = new WarpData(new Vector3(100.1f, 42.1f, 764.4f), new Vector3(0.0f, 229.3f, 0.0f), "Rooftops", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData ANCIENT_RUINS_QUANTUM_GORDO = new WarpData(new Vector3(194.7f, 11.2f, 865.0f), new Vector3(0.0f, 316.2f, 0.0f), "Quantum Gordo", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData ANCIENT_RUINS_PORTAL = new WarpData(new Vector3(60.0f, -4.5f, 1047.0f), new Vector3(0.0f, 0.0f, 0.0f), "Portal", RegionRegistry.RegionSetId.HOME);

        // Glass Desert
        public static readonly WarpData GLASS_DESERT_ENTRANCE = new WarpData(new Vector3(118.8f, 1065.9f, 780.7f), new Vector3(0.0f, 180.0f, 0.0f), "Entrance", RegionRegistry.RegionSetId.DESERT);
        public static readonly WarpData GLASS_DESERT_DERVISH_GORDO = new WarpData(new Vector3(13.6f, 1041.3f, 586.6f), new Vector3(0.0f, 277.5f, 0.0f), "Dervish Gordo", RegionRegistry.RegionSetId.DESERT);
        public static readonly WarpData GLASS_DESERT_TANGLE_GORDO = new WarpData(new Vector3(-16.0f, 1041.3f, 421.5f), new Vector3(0.0f, 321.8f, 0.0f), "Tangle Gordo", RegionRegistry.RegionSetId.DESERT);
        public static readonly WarpData GLASS_DESERT_MOSAIC_GORDO = new WarpData(new Vector3(-45.2f, 1042.4f, 283.9f), new Vector3(0.0f, 0.0f, 0.0f), "Mosaic Gordo", RegionRegistry.RegionSetId.DESERT);
        public static readonly WarpData GLASS_DESERT_HOBSON_NOTE = new WarpData(new Vector3(38.3f, 1003.8f, -157.3f), new Vector3(0.0f, 180.0f, 0.0f), "Hobson's Note", RegionRegistry.RegionSetId.DESERT);

        // NPC expansions
        public static readonly WarpData EXPANSION_OGDEN = new WarpData(new Vector3(899.4f, 1.7f, 482.7f), new Vector3(0.0f, 75.1f, 0.0f), "Ogden's Retreat", RegionRegistry.RegionSetId.HOME);
        public static readonly WarpData EXPANSION_MOCHI = new WarpData(new Vector3(242.2f, 13.0f, -949.5f), new Vector3(0.0f, 121.0f, 0.0f), "Mochi's Manor", RegionRegistry.RegionSetId.VALLEY);
        public static readonly WarpData EXPANSION_VIKTOR = new WarpData(new Vector3(982.8f, 20.9f, -236.0f), new Vector3(0.0f, 90.0f, 0.0f), "Viktor's Lab", RegionRegistry.RegionSetId.VIKTOR_LAB);

        // Area warp lists
        public static readonly WarpData[] RANCH_WARPS = { RANCH_HOUSE, RANCH_OVERGROWTH, RANCH_GROTTO, RANCH_DOCKS, RANCH_LAB };
        public static readonly WarpData[] DRY_REEF_WARPS = { DRY_REEF_MAIN, DRY_REEF_BRIDGE, DRY_REEF_RING_ISLAND };
        public static readonly WarpData[] MOSS_BLANKET_WARPS = { MOSS_BLANKET_ENTRANCE, MOSS_BLANKET_CLEARING, MOSS_BLANKET_HONEY_SLIMES, MOSS_BLANKET_LAKE, MOSS_BLANKET_HUNTER_GORDO };
        public static readonly WarpData[] INDIGO_QUARRY_WARPS = { INDIGO_QUARRY_ENTRANCE, INDIGO_QUARRY_ROCK_GORDO_CAVE, INDIGO_QUARRY_BRIDGE, INDIGO_QUARRY_RAD_POND, INDIGO_QUARRY_ASH_ISLE };
        public static readonly WarpData[] ANCIENT_RUINS_WARPS = { ANCIENT_RUINS_COURTYARD, ANCIENT_RUINS_CENTER, ANCIENT_RUINS_ROOFTOP, ANCIENT_RUINS_QUANTUM_GORDO, ANCIENT_RUINS_PORTAL };
        public static readonly WarpData[] GLASS_DESERT_WARPS = { GLASS_DESERT_ENTRANCE, GLASS_DESERT_DERVISH_GORDO, GLASS_DESERT_TANGLE_GORDO, GLASS_DESERT_MOSAIC_GORDO, GLASS_DESERT_HOBSON_NOTE };
        public static readonly WarpData[] EXPANSION_WARPS = { EXPANSION_OGDEN, EXPANSION_MOCHI, EXPANSION_VIKTOR };

        // Dictionary of all area warp lists (key = list, value = name of area)
        public static readonly Dictionary<WarpData[], string> ALL_AREA_WARPS = new Dictionary<WarpData[], string>()
        {
            { RANCH_WARPS, "Ranch" },
            { DRY_REEF_WARPS, "Dry Reef" },
            { MOSS_BLANKET_WARPS, "Moss Blanket" },
            { INDIGO_QUARRY_WARPS, "Indigo Quarry" },
            { ANCIENT_RUINS_WARPS, "Ancient Ruins" },
            { GLASS_DESERT_WARPS, "Glass Desert" },
            { EXPANSION_WARPS, "Expansions" }
        };
        #endregion

        // Properties
        public Vector3 Position { get; set; }

        public Vector3 RotEuler { get; set; }

        public string Name { get; set;  }

        public RegionRegistry.RegionSetId RegionSetId { get; set; }

        public InventoryData InventoryData { get; set; }

        public float? PlayerHealth { get; set; } = null;
        public float? PlayerEnergy { get; set; } = null;
        public int? PlayerNewbucks { get; set; } = null;

        // Need parameterless constructor for serialization
        public WarpData()
        {
            Position = Vector3.zero;
            RotEuler = Vector3.zero;
            Name = null;
            RegionSetId = RegionRegistry.RegionSetId.UNSET;
            InventoryData = null;
        }

        public WarpData(Vector3 position, Vector3 rotEuler, string name, RegionRegistry.RegionSetId regionSetId)
        {
            Position = position;
            RotEuler = rotEuler;
            Name = name;
            RegionSetId = regionSetId;
        }

        public WarpData(Vector3 position, Vector3 rotEuler, string name, RegionRegistry.RegionSetId regionSetId, InventoryData inventoryData)
        {
            Position = position;
            RotEuler = rotEuler;
            Name = name;
            RegionSetId = regionSetId;
            InventoryData = inventoryData;
        }

        public bool HasInventoryData()
        {
            return InventoryData != null;
        }

    }
}
