﻿using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.Helpers
{
    internal class Settings
    {
        public static ConfigEntry<float> RigidbodySleepThreshold;
        public static ConfigEntry<int> FramesToWakeUpPhysicsObject;

        public static ConfigEntry<int> MinimumPlacementCost;
        public static ConfigEntry<bool> MinimumCostItemsArePlaceable;
        public static ConfigEntry<bool> CostSystemEnabled;
        public static ConfigEntry<Color> PlacedItemTint;
        public static ConfigEntry<bool> MoveModeRequiresInventorySpace;
        public static ConfigEntry<bool> MoveModeCancelsSprinting;
        public static ConfigEntry<bool> PlacedItemsHaveCollision;
        public static ConfigEntry<int> MinimumSizeItemToGetCollision;
        public static ConfigEntry<bool> ImmersivePhysics;

        public static ConfigEntry<int> CustomsAllottedPoints;
        public static ConfigEntry<int> FactoryAllottedPoints;
        public static ConfigEntry<int> InterchangeAllottedPoints;
        public static ConfigEntry<int> LabAllottedPoints;
        public static ConfigEntry<int> LighthouseAllottedPoints;
        public static ConfigEntry<int> ReserveAllottedPoints;
        public static ConfigEntry<int> GroundZeroAllottedPoints;
        public static ConfigEntry<int> ShorelineAllottedPoints;
        public static ConfigEntry<int> StreetsAllottedPoints;
        public static ConfigEntry<int> WoodsAllottedPoints;

        private const string _section1Name = "9: Allotted Point Limits Per Map";
        private const string _section1Description = "Maximum number of placement points that can be used on this map. An items costs the amount of inventory cells it holds if it is a container, or it's size if it is not.";
        private static Dictionary<string, ConfigEntry<int>> _itemCountLookup = new();

        public static void Init(ConfigFile config)
        {
            RigidbodySleepThreshold = config.Bind(
                "0: Debug",
                "Rigidbody Sleep Threshold",
                0.1f,
                new ConfigDescription("When object velocity is less than this, the object stops interacting with physics systems.", null, new ConfigurationManagerAttributes { IsAdvanced = true })
            );
            FramesToWakeUpPhysicsObject = config.Bind(
                "0: Debug",
                "Number Of Frames To Wake Up Physics Object",
                10,
                new ConfigDescription("Number of frames to enable physics before Rigidbody Sleep Threshold checks start happening.", null, new ConfigurationManagerAttributes { IsAdvanced = true })
            );
            
            CostSystemEnabled = config.Bind(
                "1: Cost System",
                "Cost System Enabled",
                true,
                "It is highly reccomended to leave this enabled. Disabling it will allow infinite placement of items on all maps."
            );
            MinimumPlacementCost = config.Bind(
                "1: Cost System",
                "Minimum Placement Cost",
                3,
                "Minimum cost for placing an item. Any items that would otherwise cost less than this will cost this amount instead."
            );
            MinimumCostItemsArePlaceable = config.Bind(
                "1: Cost System",
                "Minimum Placement Cost Items Can Be Placed",
                true,
                "Set to false to prevent mininum cost or less items from being placeable entirely."
            );
            PlacedItemTint = config.Bind(
                "1: General",
                "Placed Item Color Tint",
                new Color(1, 0.7667f, 0.8667f, 1),
                "Color tint that will be applied to items when they are placed"
            );

            MoveModeRequiresInventorySpace = config.Bind(
                "2: Move Mode",
                "Move Mode Requires Inventory Space",
                true,
                "When set to true, you can only use 'MOVE' on placed items when you have the inventory space to pick them up."
            );
            MoveModeCancelsSprinting = config.Bind(
                "2: Move Mode",
                "Move Mode Cancels Sprinting",
                true,
                "If true, sprinting will cancel 'MOVE' mode."
            );

            PlacedItemsHaveCollision = config.Bind(
                "3: Collision / Physics",
                "Placed Items Collide With Player And Bots",
                true,
                "This setting requires a raid restart to fully take affect! Items at or larger than the minimum physical item size will collide with the player and block AI pathing. If you are using Fika, it's recommended to sync this setting with all clients."
            );
            MinimumSizeItemToGetCollision = config.Bind(
                "3: Collision / Physics",
                "Minimum Physical Item Size",
                12,
                "Items at or larger than this size will be considered physical to the player and bots when collision is enabled. It is HIGHLY recommended to keep this number above 10 to avoid having tons of small items that the player and AI cannot pass through. Size = the number of inventory spaces the item takes up. If you are using Fika, it's recommended to sync this setting with all clients."
            );
            ImmersivePhysics = config.Bind(
                "3: Collision / Physics",
                "Immersive Physics (no floating items)",
                true,
                "If you want to be able to make items float wherever you want, set to false."
            );

            CustomsAllottedPoints = config.Bind(
                _section1Name,
                "Customs",
                280,
                _section1Description
            );
            FactoryAllottedPoints = config.Bind(
                _section1Name,
                "Factory",
                160,
                _section1Description
            );
            InterchangeAllottedPoints = config.Bind(
                _section1Name,
                "Interchange",
                280,
                _section1Description
            );
            LabAllottedPoints = config.Bind(
                _section1Name,
                "Lab",
                160,
                _section1Description
            );
            LighthouseAllottedPoints = config.Bind(
                _section1Name,
                "Lighthouse",
                320,
                _section1Description
            );
            ReserveAllottedPoints = config.Bind(
                _section1Name,
                "Reserve",
                280,
                _section1Description
            );
            GroundZeroAllottedPoints = config.Bind(
                _section1Name,
                "Ground Zero",
                200,
                _section1Description
            );
            ShorelineAllottedPoints = config.Bind(
                _section1Name,
                "Shoreline",
                280,
                _section1Description
            );
            StreetsAllottedPoints = config.Bind(
                _section1Name,
                "Streets",
                320,
                _section1Description
            );
            WoodsAllottedPoints = config.Bind(
                _section1Name,
                "Woods",
                320,
                _section1Description
            );

            _itemCountLookup.Add("bigmap", CustomsAllottedPoints);
            _itemCountLookup.Add("factory4_day", FactoryAllottedPoints);
            _itemCountLookup.Add("factory4_night", FactoryAllottedPoints);
            _itemCountLookup.Add("interchange", InterchangeAllottedPoints);
            _itemCountLookup.Add("laboratory", LabAllottedPoints);
            _itemCountLookup.Add("lighthouse", LighthouseAllottedPoints);
            _itemCountLookup.Add("rezervbase", ReserveAllottedPoints);
            _itemCountLookup.Add("sandbox", GroundZeroAllottedPoints);
            _itemCountLookup.Add("sandbox_high", GroundZeroAllottedPoints);
            _itemCountLookup.Add("shoreline", ShorelineAllottedPoints);
            _itemCountLookup.Add("tarkovstreets", StreetsAllottedPoints);
            _itemCountLookup.Add("woods", WoodsAllottedPoints);
        }

        public static int GetAllottedPoints()
        {
            return _itemCountLookup[Singleton<GameWorld>.Instance.LocationId.ToLower()].Value;
        }
    }
}
