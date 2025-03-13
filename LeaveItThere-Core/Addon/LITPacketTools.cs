using LeaveItThere.Fika;
using SPT.Reflection.Utils;

namespace LeaveItThere.Addon
{
    public static class LITPacketTools
    {
        /// <summary>
        /// Calling this function directly is not recommended. Call .Register() on your packet registration.
        /// </summary>
        public static void RegisterPacket(LITPacketRegistration registration)
        {
#if FIKA_COMPATIBLE
            LITPacketToolsWrapper.RegisterPacket(registration);
#endif
        }

        /// <summary>
        /// Calling this function directly is not recommended. Call .Unregister() on your packet registration.
        /// </summary>
        public static void UnregisterPacket(string packetGUID)
        {
#if FIKA_COMPATIBLE
            LITPacketToolsWrapper.UnregisterPacket(packetGUID);
#endif
        }

        internal static void SendPacket(LITPacketRegistration.Packet packet)
        {
#if FIKA_COMPATIBLE
            LITPacketToolsWrapper.SendPacket(packet);
#endif
        }
    }
}
