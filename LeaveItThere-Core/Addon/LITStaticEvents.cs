using EFT;
using LeaveItThere.Components;

namespace LeaveItThere.Addon
{
    /// <summary>
    /// All these events are static! Make sure to clean up subscriptions if needed!
    /// </summary>
    public static class LITStaticEvents
    {
        public delegate void ItemPlacedStateChangedHandler(FakeItem fakeItem, bool isPlaced);
        /// <summary>
        /// Invoked every time an item is placed or reclaimed, including if it is re-placed in cases like being moved via move mode, or a packet received telling with instructions to place at X position, etc.
        /// </summary>
        public static event ItemPlacedStateChangedHandler OnItemPlacedStateChanged;
        internal static void InvokeOnItemPlacedStateChanged(FakeItem fakeItem, bool isPlaced)
        {
            OnItemPlacedStateChanged?.Invoke(fakeItem, isPlaced);
        }

        public delegate void FakeItemInitializedHandler(FakeItem fakeItem);
        /// <summary>
        /// Invoked once every time the item is placed, when it was not already placed. Will not be invoked if item is moved.
        /// </summary>
        public static event FakeItemInitializedHandler OnFakeItemInitialized;
        internal static void InvokeOnFakeItemInitialized(FakeItem fakeItem)
        {
            OnFakeItemInitialized?.Invoke(fakeItem);
        }

        public delegate void PlacedItemSpawnedHandler(FakeItem fakeItem);
        /// <summary>
        /// Invoked after LeaveItThere spawns a placed item and creates / initializes its FakeItem component on raid start.
        /// </summary>
        public static event PlacedItemSpawnedHandler OnPlacedItemSpawned;
        internal static void InvokeOnPlacedItemSpawned(FakeItem fakeItem)
        {
            OnPlacedItemSpawned?.Invoke(fakeItem);
        }

        public delegate void OnRaidEndHandler(LocalRaidSettings settings, object results, object lostInsuredItems, object transferItems, string exitName);
        /// <summary>
        /// Invoked just before LeaveItThere ModSession sends data to the server. This is the latest possible point where AddonData should be added.
        /// </summary>
        public static event OnRaidEndHandler OnRaidEnd;
        internal static void InvokeOnRaidEnd(LocalRaidSettings settings, object results, object lostInsuredItems, object transferItems, string exitName)
        {
            OnRaidEnd?.Invoke(settings, results, lostInsuredItems, transferItems, exitName);
        }

        public delegate void LastPlacedItemSpawnedHandler(FakeItem fakeItem);
        /// <summary>
        /// Invoked after LeaveItThere spawns the last placed item, ideal time for checking if items exist or fetching their AddonData
        /// </summary>
        public static event LastPlacedItemSpawnedHandler OnLastPlacedItemSpawned;
        internal static void InvokeOnLastPlacedItemSpawned(FakeItem fakeItem)
        {
            OnLastPlacedItemSpawned?.Invoke(fakeItem);
        }
    }
}
