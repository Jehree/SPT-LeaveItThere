using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.StateSync;

public class StateSynchronizerDatabase
{
    public StateSynchronizerDatabase() { }

    public Dictionary<string, string> RawDatabase { get; set; } = [];

    [JsonIgnore]
    public Dictionary<string, StateSynchronizer> Database { get; set; } = [];

    public T RegisterSynchronizer<T>(string databaseKey) where T : StateSynchronizer, new()
    {
        T synchronizer;

        if (RawDatabase.TryGetValue(databaseKey, out string value))
        {

            try
            {
                T s = JsonConvert.DeserializeObject<T>(value);
                synchronizer = s;
            }
            catch
            {
                // failing to deserialize means there was a type mismatch and we need to reset this synchronizer
                synchronizer = new T();
            }
        }
        else
        {
            synchronizer = new T();
        }

        Database[databaseKey] = synchronizer;
        return synchronizer;
    }

    public T GetSynchronizer<T>() where T : StateSynchronizer
    {
        foreach (StateSynchronizer synchronizer in Database.Values)
        {
            if (synchronizer is T) return synchronizer as T;
        }

        return null;
    }

    public void ConvertDatabaseToRaw()
    {
        RawDatabase.Clear();

        foreach (var (key, value)in Database)
        {
            RawDatabase[key] = JsonConvert.SerializeObject(value);
        }
    }
}
