﻿using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using PersistentCaches.Components;
using PersistentCaches.Helpers;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PersistentCaches.Patches
{
    internal class GameEndedPatch : ModulePatch
    {
        private static Type _targetClassType;
        private static JsonConverter[] _defaultJsonConverters;

        protected override MethodBase GetTargetMethod()
        {
            var converterClass = typeof(AbstractGame).Assembly.GetTypes().First(t => t.GetField("Converters", BindingFlags.Static | BindingFlags.Public) != null);
            _defaultJsonConverters = Traverse.Create(converterClass).Field<JsonConverter[]>("Converters").Value;

            _targetClassType = PatchConstants.EftTypes.Single(targetClass =>
                !targetClass.IsInterface &&
                !targetClass.IsNested &&
                targetClass.GetMethods().Any(method => method.Name == "LocalRaidEnded") &&
                targetClass.GetMethods().Any(method => method.Name == "ReceiveInsurancePrices")
            );

            return AccessTools.Method(_targetClassType.GetTypeInfo(), "LocalRaidEnded");
        }

        // LocalRaidSettings settings, GClass1924 results, GClass1301[] lostInsuredItems, Dictionary<string, GClass1301[]> transferItems
        [PatchPostfix]
        static void Postfix(object[] __args)
        {
            var pair = PersistentCachesSession.GetSession().CacheObjectPairs.First();
            string dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/test.json";
            ItemHelper.WriteItemToFile(dirName, pair.LootItem.Item);
        }
    }
}
