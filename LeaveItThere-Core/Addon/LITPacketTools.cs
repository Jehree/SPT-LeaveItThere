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
            if (!Plugin.FikaInstalled) return;
            LITPacketToolsWrapper.RegisterPacket(registration);
        }

        /// <summary>
        /// Calling this function directly is not recommended. Call .Unregister() on your packet registration.
        /// </summary>
        public static void UnregisterPacket(string packetGUID)
        {
            if (!Plugin.FikaInstalled) return;
            LITPacketToolsWrapper.UnregisterPacket(packetGUID);
        }

        internal static void SendPacket(LITPacketRegistration.Packet packet)
        {
            if (!Plugin.FikaInstalled) return;
            LITPacketToolsWrapper.SendPacket(packet);
        }
    }
}
