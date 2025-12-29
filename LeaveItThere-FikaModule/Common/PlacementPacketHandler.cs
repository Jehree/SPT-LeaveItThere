using Fika.Core.Networking.LiteNetLib;
using LeaveItThere.Components;
using LeaveItThere.FikaModule.Helpers;
using LeaveItThere.FikaModule.Packets;

namespace LeaveItThere.FikaModule.Common;

internal class PlacementPacketHandler
{
    public static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false)
    {
        PlacedItemStateChangedPacket packet = new()
        {
            ItemId = fakeItem.ItemId,
            Position = fakeItem.gameObject.transform.position,
            Rotation = fakeItem.gameObject.transform.rotation,
            IsPlaced = isPlaced,
            PhysicsEnableRequested = physicsEnableRequested
        };

        if (FikaTools.IAmServer())
        {
            FikaTools.Server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }

        if (FikaTools.IAmClient())
        {
            FikaTools.Client.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }
    }

    public static void OnPlacedItemStateChangedPacketReceived(PlacedItemStateChangedPacket packet)
    {
        if (packet.IsPlaced)
        {
            PlaceItemPacketReceived(packet);
        }
        else
        {
            ReclaimItemPackedReceived(packet);
        }

        if (FikaTools.IAmServer())
        {
            FikaTools.Server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }
    }

    public static void PlaceItemPacketReceived(PlacedItemStateChangedPacket packet)
    {
        FakeItem fakeItem = RaidSession.Instance.GetOrCreateFakeItem(packet.ItemId);

        fakeItem.Place(packet.Position, packet.Rotation);

        /*
        if (packet.PhysicsEnableRequested)
        {
            fakeItem.gameObject.GetComponent<Moveable>().EnablePhysics(true);
        }

        ItemMover mover = ItemMover.Instance;
        if (mover.enabled && mover.Target.gameObject == fakeItem.gameObject)
        {
            mover.Disable(false);
        }
        */
    }

    public static void ReclaimItemPackedReceived(PlacedItemStateChangedPacket packet)
    {
        FakeItem fakeItem = RaidSession.Instance.GetFakeItemOrNull(packet.ItemId);
        if (fakeItem == null) return;

        /*
        ItemMover mover = ItemMover.Instance;
        if (mover.enabled && mover.Target.gameObject == fakeItem.gameObject)
        {
            mover.Disable(false);
        }
        */

        fakeItem.Reclaim();
    }
}
