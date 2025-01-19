using LeaveItThere.Components;

namespace LeaveItThere.Fika
{
    internal class FikaInterface
    {
        public static bool IAmHost()
        {
            if (!Plugin.FikaInstalled) return true;
            return FikaWrapper.IAmHost();
        }

        public static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false)
        {
            if (!Plugin.FikaInstalled) return;
            FikaWrapper.SendPlacedStateChangedPacket(fakeItem, isPlaced, physicsEnableRequested);
        }

        public static string GetRaidId()
        {
            if (!Plugin.FikaInstalled) return LITSession.Instance.Player.ProfileId;
            return FikaWrapper.GetRaidId();
        }

        public static void InitOnPluginEnabled()
        {
            if (!Plugin.FikaInstalled) return;
            FikaWrapper.InitOnPluginEnabled();
        }
    }
}
