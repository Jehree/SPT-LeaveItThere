using LeaveItThere.Components;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LeaveItThere.Addon;

public class StateSynchronizerDatabase
{
    internal Dictionary<string, string> RawDatabase { get; set; } = [];

    [JsonIgnore]
    public Dictionary<string, BaseStateSynchronizer> Database { get; set; } = [];

    [JsonIgnore]
    public FakeItem FakeItem { get; set; }

    internal void Init(FakeItem fakeItem = null)
    {
        FakeItem = fakeItem;
    }

    /// <summary>
    /// Register a synchronizer to be used to sync states across all clients if Fika is installed, as well as to the server to allow them to persist between raids.
    /// </summary>
    /// <typeparam name="TStateSynchronizer">must inherit StateSynchronizer<TData>, TData being your own class created to hold state data.</typeparam>
    /// <param name="databaseKey">Key used to store and fetch synchronizer. Recommended to make it something like "YourName.YourAddonName"</param>
    /// <returns></returns>
    public TStateSynchronizer RegisterSynchronizer<TStateSynchronizer>(string databaseKey) where TStateSynchronizer : BaseStateSynchronizer, new()
    {
        TStateSynchronizer synchronizer;

        if (RawDatabase.TryGetValue(databaseKey, out string value))
        {

            try
            {
                TStateSynchronizer s = JsonConvert.DeserializeObject<TStateSynchronizer>(value);
                synchronizer = s;
            }
            catch
            {
                // failing to deserialize means there was a type mismatch and we need to reset this synchronizer
                synchronizer = new();
            }
        }
        else
        {
            synchronizer = new();
        }

        synchronizer.Init(FakeItem, databaseKey);
        synchronizer.OnRegisteredInternal();
        synchronizer.OnRegistered();
        Database[databaseKey] = synchronizer;

        return synchronizer;
    }

    /// <summary>
    /// Fetch a StateSynchronizer from the database.
    /// </summary>
    /// <typeparam name="TStateSynchronizer">Your class that inherits StateSynchronizer<TData>.</typeparam>
    /// <returns></returns>
    public TStateSynchronizer GetSynchronizer<TStateSynchronizer>() where TStateSynchronizer : BaseStateSynchronizer
    {
        foreach (BaseStateSynchronizer synchronizer in Database.Values)
        {
            if (synchronizer is TStateSynchronizer) return synchronizer as TStateSynchronizer;
        }

        return null;
    }

    internal BaseStateSynchronizer GetSynchronizer(string databaseKey)
    {
        if (Database.TryGetValue(databaseKey, out BaseStateSynchronizer synchronizer))
        {
            return synchronizer;
        }
        else
        {
            return null;
        }
    }

    internal void UpdateRawDatabase()
    {
        RawDatabase.Clear();

        foreach (var (key, value) in Database)
        {
            RawDatabase[key] = JsonConvert.SerializeObject(value);
        }
    }
}
