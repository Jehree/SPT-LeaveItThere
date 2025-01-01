using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System.Linq;
using UnityEngine;

namespace LeaveItThere.Common
{
    internal class ConsoleCommands
    {
        [ConsoleCommand("lit_unplace_all_items_below_cost", "", null, "Un-Place all items on the map below a cost amount. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void ClearPlacedItemsUnderCost([ConsoleArgument(0, "Cost Amount")] int costAmount, [ConsoleArgument("", "type 'IAMSURE' to confirm")] string iAmSure)
        {
            if (iAmSure != "IAMSURE") return;

            ItemHelper.ForAllItemsUnderCost(
                costAmount,
                (FakeItem fakeItem) =>
                {
                    fakeItem.Reclaim();
                    FikaInterface.SendPlacedStateChangedPacket(fakeItem);
                }
            );
        }

        [ConsoleCommand("lit_teleport_all_placed_items_to_player", "", null, "Teleport items below cost to the player. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void TPAllItemsUnderCostToPlayer([ConsoleArgument(0, "Cost Amount")] int costAmount, [ConsoleArgument("", "type 'IAMSURE' to confirm")] string iAmSure)
        {
            if (iAmSure != "IAMSURE") return;

            ItemHelper.ForAllItemsUnderCost(
                costAmount,
                (FakeItem fakeItem) =>
                {
                    var session = ModSession.Instance;
                    fakeItem.PlaceAtLocation(Utils.PlayerFront, session.Player.Transform.rotation);
                    FikaInterface.SendPlacedStateChangedPacket(fakeItem);
                }
            );
        }

        [ConsoleCommand("lit_teleport_item_to_player", "", null, "Teleport item to the player. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void TPItemToPlayer([ConsoleArgument(0, "Item number via lit_list_placed_items command")] int itemNum, [ConsoleArgument("", "type 'IAMSURE' to confirm")] string iAmSure)
        {
            if (iAmSure != "IAMSURE") return;
            var session = ModSession.Instance;

            if (itemNum > session.FakeItems.Count - 1)
            {
                ConsoleScreen.LogError("No placed item found!");
                return;
            }

            FakeItem fakeItem = session.FakeItems.Values.ToList()[itemNum];

            if (!fakeItem.Placed)
            {
                ConsoleScreen.LogError("No placed item found!");
                return;
            }

            ConsoleScreen.Log("Teleporting item!");
            fakeItem.PlaceAtLocation(Utils.PlayerFront, session.Player.Transform.rotation);
            FikaInterface.SendPlacedStateChangedPacket(fakeItem);
        }

        [ConsoleCommand("lit_list_placed_items", "", null, "List information about all placed items on the map.")]
        public static void ListPlacedItems()
        {
            int index = 0;

            ConsoleScreen.Log("---------------------------------------");
            foreach (var kvp in ModSession.Instance.FakeItems)
            {
                FakeItem fakeItem = kvp.Value;
                if (fakeItem.Placed != false)
                {
                    Vector3 playerPosition = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
                    string itemName = string.Format("({0})".Localized(null), fakeItem.LootItem.Name.Localized(null));
                    string direction = Utils.GetCardinalDirection(playerPosition, fakeItem.gameObject.transform.position);
                    string distance = Vector3.Distance(playerPosition, fakeItem.gameObject.transform.position).ToString();
                    ConsoleScreen.Log($"{itemName} (item number: {index}) placed {distance} units away from player ({direction})");
                }
                index++;
            }
            ConsoleScreen.Log("---------------------------------------");
        }
    }
}
