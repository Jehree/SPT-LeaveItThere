using Fika.Core.Networking.LiteNetLib;
using LeaveItThere.Components;
using LeaveItThere.FikaModule.Helpers;
using LeaveItThere.FikaModule.Packets;

namespace LeaveItThere.FikaModule.Common;

internal class PlacementPacketHandler
{
    public static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false, bool moveOnly = false)
    {
        PlacedItemStateChangedPacket packet = new()
        {
            ItemId = fakeItem.ItemId,
            Position = fakeItem.gameObject.transform.position,
            Rotation = fakeItem.gameObject.transform.rotation,
            IsPlaced = isPlaced,
            PhysicsEnableRequested = physicsEnableRequested,
            MoveOnly = moveOnly,
        };

        if (FikaTools.IAmHost())
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

        if (FikaTools.IAmHost())
        {
            FikaTools.Server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }
    }

    private static void PlaceItemPacketReceived(PlacedItemStateChangedPacket packet)
    {
        FakeItem fakeItem = RaidSession.Instance.GetOrCreateFakeItem(packet.ItemId);

        if (packet.MoveOnly)
        {
            fakeItem.SetFakeItemLocation(packet.Position, packet.Rotation);
        }
        else
        {
            fakeItem.Place(packet.Position, packet.Rotation);
        }

        if (packet.PhysicsEnableRequested)
        {
            fakeItem.Moveable.EnablePhysics(true);
        }

        if (ItemMover.Instance.enabled && ItemMover.Instance.Target == fakeItem)
        {
            ItemMover.Instance.Disable(false);
        }
    }

    private static void ReclaimItemPackedReceived(PlacedItemStateChangedPacket packet)
    {
        FakeItem fakeItem = RaidSession.Instance.GetFakeItemOrNull(packet.ItemId);

        if (fakeItem == null) return;

        if (ItemMover.Instance.enabled && ItemMover.Instance.Target.gameObject == fakeItem.gameObject)
        {
            ItemMover.Instance.Disable(false);
        }

        fakeItem.Reclaim();
    }
}
