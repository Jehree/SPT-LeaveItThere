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
    }
}
