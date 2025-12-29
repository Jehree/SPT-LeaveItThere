using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.StateSynchronizer;

public class StateSynchronizerDatabase : MonoBehaviour
{
    private static StateSynchronizerDatabase _instance = null;
    public static StateSynchronizerDatabase Instance
    {
        get
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                throw new Exception("Tried to get StateSynchronizerDatabase when game world was not instantiated!");
            }

            _instance ??= Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetOrAddComponent<StateSynchronizerDatabase>();

            return _instance;
        }
    }

    public Dictionary<string, StateSynchronizer> Database { get; private set; } = [];

    public void RegisterSynchronizer(StateSynchronizer synchronizer)
    {
        Database[synchronizer.DatabaseKey] = synchronizer;
    }
}
