using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentCaches.Helpers
{
    internal class Settings
    {
        public static ConfigEntry<int> CustomsMaxCacheCount;
        public static ConfigEntry<int> FactoryMaxCacheCount;
        public static ConfigEntry<int> InterchangeMaxCacheCount;
        public static ConfigEntry<int> LabMaxCacheCount;
        public static ConfigEntry<int> LighthouseMaxCacheCount;
        public static ConfigEntry<int> ReserveMaxCacheCount;
        public static ConfigEntry<int> GroundZeroMaxCacheCount;
        public static ConfigEntry<int> ShorelineMaxCacheCount;
        public static ConfigEntry<int> StreetsMaxCacheCount;
        public static ConfigEntry<int> WoodsMaxCacheCount;

        private const string _section1Name = "1: Cache Limit Per Map";
        private const string _section1Description = "Number of caches allowed on map";
        private static Dictionary<string, ConfigEntry<int>> _cacheCountLookup = new();

        public static void Init(ConfigFile config)
        {
            CustomsMaxCacheCount = config.Bind(
                _section1Name,
                "Customs",
                3,
                _section1Description
            );
            FactoryMaxCacheCount = config.Bind(
                _section1Name,
                "Factory",
                3,
                _section1Description
            );
            InterchangeMaxCacheCount = config.Bind(
                _section1Name,
                "Interchange",
                3,
                _section1Description
            );
            LabMaxCacheCount = config.Bind(
                _section1Name,
                "Lab",
                3,
                _section1Description
            );
            LighthouseMaxCacheCount = config.Bind(
                _section1Name,
                "Lighthouse",
                3,
                _section1Description
            );
            ReserveMaxCacheCount = config.Bind(
                _section1Name,
                "Reserve",
                3,
                _section1Description
            );
            GroundZeroMaxCacheCount = config.Bind(
                _section1Name,
                "Ground Zero",
                3,
                _section1Description
            );
            ShorelineMaxCacheCount = config.Bind(
                _section1Name,
                "Shoreline",
                3,
                _section1Description
            );
            StreetsMaxCacheCount = config.Bind(
                _section1Name,
                "Streets",
                3,
                _section1Description
            );
            WoodsMaxCacheCount = config.Bind(
                _section1Name,
                "Woods",
                3,
                _section1Description
            );

            _cacheCountLookup.Add("bigmap", CustomsMaxCacheCount);
            _cacheCountLookup.Add("factory4_day", FactoryMaxCacheCount);
            _cacheCountLookup.Add("factory4_night", FactoryMaxCacheCount);
            _cacheCountLookup.Add("interchange", InterchangeMaxCacheCount);
            _cacheCountLookup.Add("laboratory", LabMaxCacheCount);
            _cacheCountLookup.Add("lighthouse", LighthouseMaxCacheCount);
            _cacheCountLookup.Add("rezervbase", ReserveMaxCacheCount);
            _cacheCountLookup.Add("sandbox", GroundZeroMaxCacheCount);
            _cacheCountLookup.Add("sandbox_high", GroundZeroMaxCacheCount);
            _cacheCountLookup.Add("shoreline", ShorelineMaxCacheCount);
            _cacheCountLookup.Add("tarkovstreets", StreetsMaxCacheCount);
            _cacheCountLookup.Add("woods", WoodsMaxCacheCount);
        }

        public static int GetMaxCacheCount()
        {
            return _cacheCountLookup[Singleton<GameWorld>.Instance.LocationId].Value;
        }
    }
}
