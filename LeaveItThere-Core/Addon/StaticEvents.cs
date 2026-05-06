using LeaveItThere.Components;

namespace LeaveItThere.Addon;

internal class StaticEvents
{
    public delegate void FakeItemInitializedHandler(FakeItem fakeItem);
    /// <summary>
    /// Invoked when a FakeItem is initialized, just before it is placed.
    /// </summary>
    public static event FakeItemInitializedHandler FakeItemInitialized;
    internal static void InvokeFakeItemInitialized(FakeItem fakeItem)
    {
        FakeItemInitialized?.Invoke(fakeItem);
    }

    public delegate void ItemReclaimedHandler(FakeItem fakeItem);
    public static event ItemReclaimedHandler ItemReclaimed;
    internal static void InvokeItemReclaimed(FakeItem fakeItem)
    {
        ItemReclaimed?.Invoke(fakeItem);
    }

    /// <summary>
    /// Invoked every time a FakeItem is moved, including when it initially placed as well as every time it is moved via Move Mode.
    /// </summary>
    public delegate void FakeItemMovedHandler(FakeItem fakeItem);
    public static event FakeItemMovedHandler FakeItemMoved;
    internal static void InvokeFakeItemMoved(FakeItem fakeItem)
    {
        FakeItemMoved?.Invoke(fakeItem);
    }

    public delegate void ItemSpawnedHandler(FakeItem fakeItem);
    /// <summary>
    /// Invoked after LeaveItThere spawns a placed item and creates / initializes its FakeItem component on raid start.
    /// </summary>
    public static event ItemSpawnedHandler ItemSpawned;
    internal static void InvokeItemSpawned(FakeItem fakeItem)
    {
        ItemSpawned?.Invoke(fakeItem);
    }

    public delegate void AllItemsSpawnedHandler();
    /// <summary>
    /// Invoked after LeaveItThere spawns the last placed item, ideal time for checking if items exist or fetching their AddonData
    /// </summary>
    public static event AllItemsSpawnedHandler AllItemsSpawned;
    internal static void InvokeAllItemsSpawned()
    {
        AllItemsSpawned?.Invoke();
    }

    public delegate void GameStartedHandler();
    public static event GameStartedHandler GameStarted;
    internal static void InvokeGameStarted()
    {
        GameStarted?.Invoke();
    }

    public delegate void GameEndedHandler();
    public static event GameEndedHandler GameEnded;
    internal static void InvokeGameEnded()
    {
        GameEnded?.Invoke();
    }
}
