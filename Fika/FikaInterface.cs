using Comfort.Common;
using EFT;
using LeaveItThere.Common;
using LeaveItThere.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveItThere.Fika
{
    internal class FikaInterface
    {
        public static bool IAmHost()
        {
            if (!Plugin.FikaInstalled) return true;
            return FikaWrapper.IAmHost();
        }

        public static void SendPlacedStateChangedPacket(ItemRemotePair pair)
        {
            if (!Plugin.FikaInstalled) return;
            FikaWrapper.SendPlacedStateChangedPacket(pair);
        }

        public static string GetRaidId()
        {
            if (!Plugin.FikaInstalled) return ModSession.GetSession().Player.ProfileId;
            return FikaWrapper.GetRaidId();
        }

        public static void InitOnPluginEnabled()
        {
            if (!Plugin.FikaInstalled) return;
            FikaWrapper.InitOnPluginEnabled();
        }
    }
}
