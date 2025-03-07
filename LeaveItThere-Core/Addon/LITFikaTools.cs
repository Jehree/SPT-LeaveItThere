using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System;
using UnityEngine;

namespace LeaveItThere.Addon
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
        /// Spawns an item and syncs it with all clients. If you explicitly DON'T want syncing, use ItemHelper.SpawnItem instead.
        /// </summary>
        /// <param name="item">Item to spawn, create one via ItemFactory.</param>
        /// <param name="senderCallback">Gets called once item is finished spawning. ONLY CALLED BY THE SENDER.</param>
        public static void SpawnItem(Item item, Vector3 position = default, Quaternion rotation = default, Action<LootItem> senderCallback = null)
        {
            if (!Plugin.FikaInstalled) ItemHelper.SpawnItem(item, position, rotation, senderCallback);
            FikaWrapper.SendSpawnItemPacket(item, position, rotation, senderCallback);
        }

        /// <summary>
        /// Spawns an item inside a container and syncs it with all clients. Returns whether the sender's container had space for the item. If you explicitly DON'T want syncing, use ItemHelper.SpawnItemInContainer instead.
        /// </summary>
        /// <param name="item">Item to spawn, create one via ItemFactory</param>
        /// <param name="senderCallback">Gets called once item is finished spawning. ONLY CALLED BY THE SENDER.</param>
        public static bool TrySpawnItemInContainer(CompoundItem container, Item item, Action<LootItem> senderCallback)
        {
            if (!ItemHelper.ContainerHasSpaceForItem(container, item)) return false;

            if (!Plugin.FikaInstalled) ItemHelper.SpawnItemInContainer(container, item, senderCallback);
            FikaWrapper.SendSpawnItemInContainerPacket(container, item, senderCallback);

            return true;
        }

        /// <summary>
        /// Removes an Item from CompoundItem' grids. Returns 'Succeeded' property. If you explicitly DON'T want syncing, use ItemHelper.RemoveItemFromContainer instead
        /// </summary>
        public static bool RemoveItemFromContainer(CompoundItem container, Item itemToRemove)
        {
            if (!Plugin.FikaInstalled) return ItemHelper.RemoveItemFromContainer(container, itemToRemove);
            return FikaWrapper.SendRemoveItemFromContainerPacket(container, itemToRemove);
        }
    }
}
