using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking.LiteNetLib;
using LeaveItThere.FikaModule.Packets;

namespace LeaveItThere.FikaModule.Common;

internal class EventSubscriber
{
    public static void OnFikaNetManagerCreated(FikaNetworkManagerCreatedEvent managerCreatedEvent)
    {
        managerCreatedEvent.Manager.RegisterPacket<PlacedItemStateChangedPacket>(PlacementPacketHandler.OnPlacedItemStateChangedPacketReceived);
        managerCreatedEvent.Manager.RegisterPacket<StateSynchronizerPacket>(StateSynchronizerPacketHandler.OnPacketReceived);
        managerCreatedEvent.Manager.RegisterPacket<LITGenericPacket, NetPeer>(GenericPacketHandler.OnGenericPacketReceived);
    }

    public static void InitOnPluginEnabled()
    {
        FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetManagerCreated);
    }
}
