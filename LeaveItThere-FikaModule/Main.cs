using Comfort.Common;
using Fika.Core.Coop.Utils;
using Fika.Core.Networking;
using LeaveItThere.Fika;
using LeaveItThere.FikaModule.Common;

namespace LeaveItThere.FikaModule
{
    internal class Main
    {
        // called by the core dll via reflection
        public static void Init()
        {
            PluginAwake();
            FikaBridge.PluginEnableEmitted += PluginEnable;

            FikaBridge.IAmHostEmitted += IAmHost;
            FikaBridge.GetRaidIdEmitted += GetRaidId;

            FikaBridge.SendPlacedStateChangedPacketEmitted += FikaMethods.SendPlacedStateChangedPacket;
            FikaBridge.SendSpawnItemPacketEmitted += FikaMethods.SendSpawnItemPacket;

            FikaBridge.RegisterPacketEmitted += GenericPacketTools.RegisterPacket;
            FikaBridge.UnregisterPacketEmitted += GenericPacketTools.UnregisterPacket;
            FikaBridge.SendPacketEmitted += GenericPacketTools.SendPacket;
        }

        public static void PluginAwake()
        {

        }

        public static void PluginEnable()
        {
            FikaMethods.InitOnPluginEnabled();
        }

        public static bool IAmHost()
        {
            return Singleton<FikaServer>.Instantiated;
        }

        public static string GetRaidId()
        {
            return FikaBackendUtils.GroupId;
        }
    }
}