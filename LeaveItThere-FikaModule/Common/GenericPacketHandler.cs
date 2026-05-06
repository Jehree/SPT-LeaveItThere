using Comfort.Common;
using EFT.UI;
using Fika.Core.Networking.LiteNetLib;
using LeaveItThere.Addon;
using LeaveItThere.FikaModule.Helpers;
using LeaveItThere.FikaModule.Packets;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;

namespace LeaveItThere.FikaModule.Common;

internal class GenericPacketHandler
{
    public static Dictionary<string, PacketRegistration> Registrations = [];

    public static void RegisterPacket(PacketRegistration registration)
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

    private static PacketRegistration.Packet GetAbstractedPacket(LITGenericPacket packet)
    {
        PacketRegistration.Packet abstractedPacket = new()
        {
            PacketGUID = packet.PacketGUID,
            SenderProfileId = packet.SenderProfileId,
            Destination = (EPacketDestination)packet.Destination,
            JsonData = packet.Data
        };

        return abstractedPacket;
    }

    public static void SendPacket(PacketRegistration.Packet abstractedPacket)
    {
        if (!Registrations.ContainsKey(abstractedPacket.PacketGUID))
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);

            string msg = $"Attempted to send LITGenericPacket (GUID: {abstractedPacket.PacketGUID}) with no registration! Make sure to call LITPackegRegistration.Get<YourPacketClass>().Register() in your plugin's Awake() function!";

            ConsoleScreen.LogError(msg);
            InteractionHelper.NotificationLongWarning("Problem with LITGenericPacket! Press ~ for more info!");

            throw new Exception(msg);
        }

        if (FikaTools.IAmHost() && abstractedPacket.Destination == EPacketDestination.HostOnly) return;

        LITGenericPacket packet = new()
        {
            PacketGUID = abstractedPacket.PacketGUID,
            SenderProfileId = abstractedPacket.SenderProfileId,
            Destination = (int)abstractedPacket.Destination,
            Data = abstractedPacket.JsonData,
        };

        if (FikaTools.IAmHost())
        {
            // if we are the host, we won't get a return packet anyway so we don't care if Destination is Everyone or EveryoneExceptSender
            FikaTools.Server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }
        else
        {
            FikaTools.Client.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }
    }

    public static void OnGenericPacketReceived(LITGenericPacket packet, NetPeer peer)
    {
        if (!Registrations.ContainsKey(packet.PacketGUID))
        {
            string msg = $"Received LITGenericPacket (GUID: {packet.PacketGUID}) with no registration! Make sure to call LITPackegRegistration.Get<YourPacketClass>().Register() in your plugin's Awake() function!";
            ConsoleScreen.LogError(msg);
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
            InteractionHelper.NotificationLongWarning("Problem with LITGenericPacket! Press ~ for more info!");
            throw new Exception(msg);
        }

        PacketRegistration.Packet abstractedPacket = GetAbstractedPacket(packet);
        Registrations[packet.PacketGUID].OnPacketReceived(abstractedPacket);

        // if we are not the host, or the Destination is set to HostOnly, we don't need to do any more sending
        if (!FikaTools.IAmHost()) return;
        if (abstractedPacket.Destination == EPacketDestination.HostOnly) return;


        if (abstractedPacket.Destination == EPacketDestination.Everyone)
        {
            FikaTools.Server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }
        // everyone except sender gets the packet
        else if (abstractedPacket.Destination == EPacketDestination.EveryoneExceptSender)
        {
            foreach (NetPeer p in FikaTools.Server.NetServer.ConnectedPeerList)
            {
                if (p == peer) continue;

                FikaTools.Server.SendDataToPeer(ref packet, DeliveryMethod.ReliableOrdered, p);
            }
        }
    }
}
