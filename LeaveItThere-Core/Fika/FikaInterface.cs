using LeaveItThere.Components;
using SPT.Reflection.Utils;

namespace LeaveItThere.Fika
{
    internal class FikaInterface
    {
        internal static void InitOnPluginEnabled()
        {
#if FIKA_COMPATIBLE
            FikaWrapper.InitOnPluginEnabled();
#endif
        }

        internal static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false)
        {
#if FIKA_COMPATIBLE
            FikaWrapper.SendPlacedStateChangedPacket(fakeItem, isPlaced, physicsEnableRequested);
#endif
        }

        public static bool IAmHost()
        {
#if FIKA_COMPATIBLE
            return FikaWrapper.IAmHost();
#else
            return true;
#endif
        }

        public static string GetRaidId()
        {
#if FIKA_COMPATIBLE
            return FikaWrapper.GetRaidId();
#else
            return ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId;
#endif
        }
    }
}
