using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Fika;
using System;
using UnityEngine;

namespace LeaveItThere.Helpers
{
    /// <summary>
    /// Exposed external tools to be used primarily by Leave It There addons
    /// </summary>
    public static class LITFikaTools
    {
        /// <summary>
        /// Returns true if Fika is not installed, or if it is and this client is the host of the raid.
        /// </summary>
        public static bool IAmHost()
        {
            return FikaInterface.IAmHost();
        }

        /// <summary>
        /// Returns profile id if Fika is not installed, or the id of the raid if it is (which will be the host's profile id).
        /// </summary>
        public static string GetRaidId()
        {
            return FikaInterface.GetRaidId();
        }

        /// <summary>
        /// Spawns an item and syncs it with all clients. If you explicitly DON'T need syncing, use ItemHelper.SpawnItem instead.
        /// </summary>
        /// <param name="item">Item to spawn, create one via ItemFactory.</param>
        /// <param name="callback">Gets called once item is finished spawning.</param>
        public static void SpawnItem(Item item, Vector3 position = default, Quaternion rotation = default, Action<LootItem> callback = null)
        {
            if (!Plugin.FikaInstalled) ItemHelper.SpawnItem(item, position, rotation, callback);
            FikaWrapper.SendSpawnItemPacket(item, position, rotation, callback);
        }

        /// <summary>
        /// Removes an Item from CompoundItem' grids. Returns 'Succeeded' property. If you explicitly DON'T need syncing, use ItemHelper.RemoveItemFromGrid instead
        /// </summary>
        public static bool RemoveItemFromGrid(CompoundItem container, Item itemToRemove)
        {
            if (!Plugin.FikaInstalled) return ItemHelper.RemoveItemFromContainer(container, itemToRemove);
            return FikaWrapper.SendRemoveItemFromContainerPacket(container, itemToRemove);
        }
    }
}
