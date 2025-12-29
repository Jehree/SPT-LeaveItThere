using LeaveItThere.Fika;
using LeaveItThere.FikaModule.Common;
using LeaveItThere.FikaModule.Helpers;

namespace LeaveItThere.FikaModule;

internal class Main
{
    // called by the core dll via reflection
    public static void Init()
    {
        FikaBridge.PluginEnableEmitted += EventSubscriber.InitOnPluginEnabled;

        FikaBridge.IAmHostEmitted += FikaTools.IAmServer;
        FikaBridge.GetRaidIdEmitted += FikaTools.GetRaidId;

        FikaBridge.SendPlacedStateChangedPacketEmitted += PlacementPacketHandler.SendPlacedStateChangedPacket;
    }
}