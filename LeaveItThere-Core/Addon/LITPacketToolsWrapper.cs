using Comfort.Common;
using EFT.UI;
using Fika.Core.Networking;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using LeaveItThere_Packets;
using LiteNetLib;
using System;
using System.Collections.Generic;

namespace LeaveItThere.Addon
{
    internal static class LITPacketToolsWrapper
    {
        public static Dictionary<string, LITPacketRegistration> Registrations = [];

        public static void RegisterPacket(LITPacketRegistration registration)
        {
            if (Registrations.ContainsKey(registration.PacketGUID))
            {
                throw new InvalidOperationException($"[Leave It There]: Packet registration with the GUID: {registration.PacketGUID} was attempted when it was already registered!");
            }

            Registrations[registration.PacketGUID] = registration;
        }

        public static void UnregisterPacket(string packetGUID)
        {
            if (Registrations.ContainsKey(packetGUID))
            {
                Registrations.Remove(packetGUID);
            }
        }

        public static LITPacketRegistration.Packet GetAbstractedPacket(LITGenericPacket packet)
        {
            return new LITPacketRegistration.Packet
            {
                PacketGUID = packet.PacketGUID,
                SenderProfileId = packet.SenderProfileId,
                Destination = (EPacketDestination)packet.Destination,
                StringData = packet.StringData,
                BoolData = (bool)packet.BoolData,
                ByteArrayData = packet.ByteArrayData,
            };
        }

        public static void SendPacket(LITPacketRegistration.Packet abstractedPacket)
        {
            if (!Registrations.ContainsKey(abstractedPacket.PacketGUID))
            {
                string msg = $"Attempted to send LITGenericPacket (GUID: {abstractedPacket.PacketGUID}) with no registration! Make sure to call LITPackegRegistration.Get<YourPacketClass>().Register() in your plugin's Awake() function!";
                ConsoleScreen.LogError(msg);
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
                InteractionHelper.NotificationLongWarning("Problem with LITGenericPacket! Press ~ for more info!");
                throw new Exception(msg);
            }

            Plugin.LogSource.LogError("send1");
            LITGenericPacket packet = new()
            {
                PacketGUID = abstractedPacket.PacketGUID,
                SenderProfileId = abstractedPacket.SenderProfileId,
                Destination = (int)abstractedPacket.Destination,
                StringData = abstractedPacket.StringData,
                BoolData = abstractedPacket.BoolData,
                ByteArrayData = abstractedPacket.ByteArrayData,
            };
            Plugin.LogSource.LogError("send2");
            if (FikaWrapper.IAmHost())
            {
                Plugin.LogSource.LogError("send3");
                if (abstractedPacket.Destination == EPacketDestination.HostOnly) return;
                Plugin.LogSource.LogError("send4");
                // if we are the host, we won't get a return packet anyway so we don't care if Destination is Everyone or EveryoneExceptSender
                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
                Plugin.LogSource.LogError("send5");
            }
            else
            {
                Plugin.LogSource.LogError("send6");
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered);
            }
        }

        public static void OnGenericPacketReceived(LITGenericPacket packet, NetPeer peer)
        {
            Plugin.LogSource.LogError("receive1");
            if (!Registrations.ContainsKey(packet.PacketGUID))
            {
                string msg = $"Received LITGenericPacket (GUID: {packet.PacketGUID}) with no registration! Make sure to call LITPackegRegistration.Get<YourPacketClass>().Register() in your plugin's Awake() function!";
                ConsoleScreen.LogError(msg);
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
                InteractionHelper.NotificationLongWarning("Problem with LITGenericPacket! Press ~ for more info!");
                throw new Exception(msg);
            }
            Plugin.LogSource.LogError("receive2");
            LITPacketRegistration.Packet abstractedPacket = GetAbstractedPacket(packet);
            Plugin.LogSource.LogError("receive3");
            Registrations[packet.PacketGUID].OnPacketReceived(abstractedPacket);
            Plugin.LogSource.LogError("receive4");
            // if we are not the host, or the Destination is set to HostOnly, we don't need to do any more sending
            if (!FikaWrapper.IAmHost()) return;
            Plugin.LogSource.LogError("receive5");
            if (abstractedPacket.Destination == EPacketDestination.HostOnly) return;
            Plugin.LogSource.LogError("receive6");
            FikaServer fikaServer = Singleton<FikaServer>.Instance;
            NetManager netServer = fikaServer.NetServer;

            if (abstractedPacket.Destination == EPacketDestination.Everyone)
            {
                Plugin.LogSource.LogError("receive7");
                fikaServer.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
            }
            // everyone except sender gets the packet
            else if (abstractedPacket.Destination == EPacketDestination.EveryoneExceptSender)
            {
                Plugin.LogSource.LogError("receive8");
                foreach (NetPeer p in netServer.ConnectedPeerList)
                {
                    if (p == peer) continue;

                    fikaServer.SendDataToPeer(p, ref packet, DeliveryMethod.ReliableOrdered);
                }
            }
        }
    }
}
