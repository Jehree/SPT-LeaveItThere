using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.ModSettings;

internal class Settings
{
    private static string _devSectionName = "0: DEVELOPER";
    public static ConfigEntry<float> RigidbodySleepThreshold;
    public static ConfigEntry<int> FramesToWakeUpPhysicsObject;
    public static ConfigEntry<Color> PlacedItemTint;

    private static string _costSystemSectionName = "1: Cost System";
    public static ConfigEntry<int> MinimumPlacementCost;
    public static ConfigEntry<bool> MinimumCostItemsArePlaceable;
    public static ConfigEntry<bool> CostSystemEnabled;

    private static string _moveModeSectionName = "2: Move Mode";
    public static ConfigEntry<bool> MoveModeRequiresInventorySpace;
    public static ConfigEntry<bool> MoveModeCancelsSprinting;
    public static ConfigEntry<Color> PositionTabColor;
    public static ConfigEntry<Color> RotationTabColor;
    public static ConfigEntry<Color> PhysicsTabColor;
    public static ConfigEntry<Color> HighlightColor;
    public static ConfigEntry<Color> ClickColor;
    public static ConfigEntry<Color> BackgroundColor;

    private static string _moveModeKeybindsSectionName = "3: Move Mode (keybinds)";
    public static ConfigEntry<KeyboardShortcut> SaveHotkey;
    public static ConfigEntry<KeyboardShortcut> CancelHotkey;
    public static ConfigEntry<KeyboardShortcut> RepositionTabHotkey;
    public static ConfigEntry<KeyboardShortcut> RotationTabHotkey;
    public static ConfigEntry<KeyboardShortcut> PhysicsTabHotkey;
    public static ConfigEntry<KeyboardShortcut> PrecisionKey;

    private static string _moveModeSpeedsSectionName = "4: Move Mode (input speeds)";
    public static ConfigEntry<float> RotationSpeed;
    public static ConfigEntry<float> RotationScrollSpeed;
    public static ConfigEntry<float> RepositionSpeed;
    public static ConfigEntry<float> RepositionScrollSpeed;
    public static ConfigEntry<bool> InvertHorizontalRotation;
    public static ConfigEntry<bool> InvertVerticalRotation;
    public static ConfigEntry<float> PrecisionMultiplier;

    private static string _physicsCollisionSectionName = "5: Collision / Physics";
    public static ConfigEntry<bool> PlacedItemsHaveCollision;
    public static ConfigEntry<int> MinimumSizeItemToGetCollision;
    public static ConfigEntry<bool> ImmersivePhysics;

    private static string _mapPointAllotmentSectionName = "6: Map Point Allotments";
    private static string _mapPointAllotmentDescription = "Maximum number of placement points that can be used on this map. An items costs the amount of inventory cells it holds if it is a container, or its external size if it is not.";
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


    private static Dictionary<string, ConfigEntry<int>> _allottedPointsLookup = [];
    public static int AllottedPoints => _allottedPointsLookup[Singleton<GameWorld>.Instance.LocationId.ToLower()].Value;

    public static void Init(ConfigFile config)
    {
        #region DEVELOPER
        RigidbodySleepThreshold = config.Bind(
            _devSectionName,
            "Rigidbody Sleep Threshold",
            0.1f,
            new ConfigDescription("When object velocity is less than this, the object stops interacting with physics systems.", null, new ConfigurationManagerAttributes { IsAdvanced = true })
        );
        FramesToWakeUpPhysicsObject = config.Bind(
            _devSectionName,
            "Number Of Frames To Wake Up Physics Object",
            10,
            new ConfigDescription("Number of frames to enable physics before Rigidbody Sleep Threshold checks start happening.", null, new ConfigurationManagerAttributes { IsAdvanced = true })
        );
        PlacedItemTint = config.Bind(
            _devSectionName,
            "Placed Item Color Tint",
            new Color(1, 0.7667f, 0.8667f, 1),
            new ConfigDescription("Color tint that will be applied to items when they are placed", null, new ConfigurationManagerAttributes { IsAdvanced = true })
        );
        #endregion

        #region COST SYSTEM
        CostSystemEnabled = config.Bind(
            _costSystemSectionName,
            "Cost System Enabled",
            true,
            "It is highly reccomended to leave this enabled. Disabling it will allow infinite placement of items on all maps."
        );
        MinimumPlacementCost = config.Bind(
            _costSystemSectionName,
            "Minimum Placement Cost",
            3,
            "Minimum cost for placing an item. Any items that would otherwise cost less than this will cost this amount instead."
        );
        MinimumCostItemsArePlaceable = config.Bind(
            _costSystemSectionName,
            "Minimum Placement Cost Items Can Be Placed",
            true,
            "Set to false to prevent mininum cost or less items from being placeable entirely."
        );
        #endregion

        #region MOVE MODE
        // OPTIONS
        MoveModeRequiresInventorySpace = config.Bind(
            _moveModeSectionName,
            "Edit Move Mode Requires Inventory Space",
            true,
            "When set to true, you can only use 'MOVE' on placed items when you have the inventory space to pick them up."
        );
        MoveModeCancelsSprinting = config.Bind(
            _moveModeSectionName,
            "Sprinting Cancels Edit Move Mode",
            true,
            "If true, sprinting will cancel 'MOVE' mode."
        );
        PositionTabColor = config.Bind(
            _moveModeSectionName,
            "1: Position Tab Color",
            new Color(0.6497419f, 0.8773585f, 0.649954f, 1),
            new ConfigDescription("", null, new ConfigurationManagerAttributes() { IsAdvanced = true })
        );
        RotationTabColor = config.Bind(
            _moveModeSectionName,
            "2: Rotation Tab Color",
            new Color(1, 0.5330188f, 0.5330188f, 1),
            new ConfigDescription("", null, new ConfigurationManagerAttributes() { IsAdvanced = true })
        );
        PhysicsTabColor = config.Bind(
            _moveModeSectionName,
            "3: Physics Tab Color",
            new Color(1, 0.5330188f, 0.907985f, 1),
            new ConfigDescription("", null, new ConfigurationManagerAttributes() { IsAdvanced = true })
        );
        HighlightColor = config.Bind(
            _moveModeSectionName,
            "7: Highlight Color",
            Color.white,
            new ConfigDescription("", null, new ConfigurationManagerAttributes() { IsAdvanced = true })
        );
        ClickColor = config.Bind(
            _moveModeSectionName,
            "8: Click Color",
            Color.gray,
            new ConfigDescription("", null, new ConfigurationManagerAttributes() { IsAdvanced = true })
        );
        BackgroundColor = config.Bind(
            _moveModeSectionName,
            "8: Background Color",
            new Color(0.6037736f, 0.5685925f, 0.504094f, 1),
            new ConfigDescription("", null, new ConfigurationManagerAttributes() { IsAdvanced = true })
        );

        // KEYBINDS
        PrecisionKey = config.Bind(
            _moveModeKeybindsSectionName,
            "Precision Key",
            new KeyboardShortcut(KeyCode.X),
            "Hold down to slow down Move mode mouse and scroll speed by the Precision Move Multiplier amount."
        );
        RepositionTabHotkey = config.Bind(
            _moveModeKeybindsSectionName,
            "Switch To Position Tab",
            new KeyboardShortcut(KeyCode.Alpha1),
            ""
        );
        RotationTabHotkey = config.Bind(
            _moveModeKeybindsSectionName,
            "Switch To Rotation Tab",
            new KeyboardShortcut(KeyCode.Alpha2),
            ""
        );
        PhysicsTabHotkey = config.Bind(
            _moveModeKeybindsSectionName,
            "Switch To Physics Tab",
            new KeyboardShortcut(KeyCode.Alpha3),
            ""
        );
        SaveHotkey = config.Bind(
            _moveModeKeybindsSectionName,
            "Close Move Mode (save)",
            new KeyboardShortcut(KeyCode.F),
            ""
        );
        CancelHotkey = config.Bind(
            _moveModeKeybindsSectionName,
            "Close Move Mode (cancel)",
            new KeyboardShortcut(KeyCode.Escape),
            ""
        );

        // SPEEDS
        RotationSpeed = config.Bind(
            _moveModeSpeedsSectionName,
            "Rotation Mouse Speed",
            4.5f,
            "Speed items will rotate in rotation mode with mouse movement."
        );
        RotationScrollSpeed = config.Bind(
            _moveModeSpeedsSectionName,
            "Rotation Scroll Step Size",
            100f,
            "Step size items will rotate in when scrolling mouse wheel."
        );
        RepositionSpeed = config.Bind(
            _moveModeSpeedsSectionName,
            "Reposition Mouse Speed",
            0.07f,
            "Speed items will move in position mode with mouse movement."
        );
        RepositionScrollSpeed = config.Bind(
            _moveModeSpeedsSectionName,
            "Reposition Scroll Step Size",
            4f,
            "Step size items will move in when scrolling mouse wheel."
        );
        InvertHorizontalRotation = config.Bind(
            _moveModeSpeedsSectionName,
            "Invert Horizontal Rotation Direction",
            false,
            ""
        );
        InvertVerticalRotation = config.Bind(
            _moveModeSpeedsSectionName,
            "Invert Vertical Rotation Direction",
            false,
            ""
        );
        PrecisionMultiplier = config.Bind(
            _moveModeSpeedsSectionName,
            "Precision Multiplier",
            0.2f,
            new ConfigDescription("Mouse and scroll speed in Move mode will slow down by this amount when Precision Move Key is held", new AcceptableValueRange<float>(0f, 1f))
        );
        #endregion

        #region COLLISION
        PlacedItemsHaveCollision = config.Bind(
            _physicsCollisionSectionName,
            "Placed Items Collide With Player And Bots",
            true,
            "This setting requires a raid restart to fully take affect! Items at or larger than the minimum physical item size will collide with the player and block AI pathing. If you are using Fika, it's recommended to sync this setting with all clients."
        );
        MinimumSizeItemToGetCollision = config.Bind(
            _physicsCollisionSectionName,
            "Minimum Physical Item Size",
            12,
            "Items at or larger than this size will be considered physical to the player and bots when collision is enabled. It is HIGHLY recommended to keep this number above 10 to avoid having tons of small items that the player and AI cannot pass through. Size = the number of inventory spaces the item takes up. If you are using Fika, it's recommended to sync this setting with all clients."
        );
        ImmersivePhysics = config.Bind(
            _physicsCollisionSectionName,
            "Items Fall After Moved",
            true,
            "When toggled off, items will float when moved by default. Can be manually changed per-item in the Edit Placement UI pop-up Physics tab."
        );
        #endregion

        #region POINT ALLOTMENTS
        CustomsAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Customs",
            280,
            _mapPointAllotmentDescription
        );
        FactoryAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Factory",
            160,
            _mapPointAllotmentDescription
        );
        InterchangeAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Interchange",
            280,
            _mapPointAllotmentDescription
        );
        LabAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Lab",
            160,
            _mapPointAllotmentDescription
        );
        LighthouseAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Lighthouse",
            320,
            _mapPointAllotmentDescription
        );
        ReserveAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Reserve",
            280,
            _mapPointAllotmentDescription
        );
        GroundZeroAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Ground Zero",
            200,
            _mapPointAllotmentDescription
        );
        ShorelineAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Shoreline",
            280,
            _mapPointAllotmentDescription
        );
        StreetsAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Streets",
            320,
            _mapPointAllotmentDescription
        );
        WoodsAllottedPoints = config.Bind(
            _mapPointAllotmentSectionName,
            "Woods",
            320,
            _mapPointAllotmentDescription
        );
        #endregion

        _allottedPointsLookup.Add("bigmap", CustomsAllottedPoints);
        _allottedPointsLookup.Add("factory4_day", FactoryAllottedPoints);
        _allottedPointsLookup.Add("factory4_night", FactoryAllottedPoints);
        _allottedPointsLookup.Add("interchange", InterchangeAllottedPoints);
        _allottedPointsLookup.Add("laboratory", LabAllottedPoints);
        _allottedPointsLookup.Add("lighthouse", LighthouseAllottedPoints);
        _allottedPointsLookup.Add("rezervbase", ReserveAllottedPoints);
        _allottedPointsLookup.Add("sandbox", GroundZeroAllottedPoints);
        _allottedPointsLookup.Add("sandbox_high", GroundZeroAllottedPoints);
        _allottedPointsLookup.Add("shoreline", ShorelineAllottedPoints);
        _allottedPointsLookup.Add("tarkovstreets", StreetsAllottedPoints);
        _allottedPointsLookup.Add("woods", WoodsAllottedPoints);
    }
}