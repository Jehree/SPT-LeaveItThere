using LeaveItThere.Components;
using SPT.Reflection.Utils;

namespace LeaveItThere.Fika
{
    internal class FikaInterface
    {
        internal static void InitOnPluginEnabled()
        {
            if (!Plugin.FikaInstalled) return;
            FikaWrapper.InitOnPluginEnabled();
        }

        internal static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false)
        {
            if (!Plugin.FikaInstalled) return;
            FikaWrapper.SendPlacedStateChangedPacket(fakeItem, isPlaced, physicsEnableRequested);
        }

        public static bool IAmHost()
        {
            if (!Plugin.FikaInstalled) return true;
            return FikaWrapper.IAmHost();
        }

        public static string GetRaidId()
        {
            if (!Plugin.FikaInstalled) return ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId;
            return FikaWrapper.GetRaidId();
        }
    }
}
