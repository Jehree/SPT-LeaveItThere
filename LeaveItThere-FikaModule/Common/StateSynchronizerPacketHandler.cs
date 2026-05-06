using Fika.Core.Networking.LiteNetLib;
using LeaveItThere.Addon;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.FikaModule.Helpers;
using LeaveItThere.FikaModule.Packets;

namespace LeaveItThere.FikaModule.Common;

internal class StateSynchronizerPacketHandler
{
    public static void SendPacket(string data, string databaseKey, FakeItem fakeItem = null)
    {
        StateSynchronizerPacket packet = new()
        {
            IsFakeItem = fakeItem != null,
            ItemId = fakeItem == null
                        ? "none"
                        : fakeItem.ItemId,
            DatabaseKey = databaseKey,
            Data = data
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

    public static void OnPacketReceived(StateSynchronizerPacket packet)
    {
        Plugin.LogSource.LogError("Packet received!");
        Plugin.LogSource.LogError(packet.IsFakeItem);
        if (packet.IsFakeItem)
        {
            if (RaidSession.Instance.FakeItems.TryGetValue(packet.ItemId, out FakeItem fakeItem))
            {
                Plugin.LogSource.LogError("TryGetValue success");
                BaseStateSynchronizer synchronizer = fakeItem.StateSynchronizerDatabase.GetSynchronizer(packet.DatabaseKey);
                synchronizer.RawData = packet.Data;
                synchronizer.OnSynchronized();
            }
        }
        else
        {
            // handle packet being in RaidSession's database
        }

        if (FikaBridge.IAmHost())
        {
            FikaTools.Server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        }
    }
}
