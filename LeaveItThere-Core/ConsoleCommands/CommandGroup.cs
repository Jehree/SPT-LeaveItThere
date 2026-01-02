using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.UI;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System.Linq;
using UnityEngine;

namespace LeaveItThere.ConsoleCommands
{
    public class CommandGroup
    {
        /*
        [ConsoleCommand("lit_unplace_all_items_below_cost", "", null, "Un-Place all items on the map below a cost amount. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void ClearPlacedItemsUnderCost([ConsoleArgument(0, "Cost Amount")] int costAmount, [ConsoleArgument("", "type 'IAMSURE' to confirm")] string iAmSure)
        {
            if (iAmSure != "IAMSURE") return;

            ItemHelper.ForAllItemsUnderCost(
                costAmount,
                (FakeItem fakeItem) =>
                {
                    FikaBridge.SendPlacedStateChangedPacket(fakeItem, false);
                    fakeItem.Reclaim();
                }
            );
        }
        */

        [ConsoleCommand("LeaveItThere.tp_all_items_to_player", "", null, "Teleport all items to the player. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void TPAllItemsUnderCostToPlayer([ConsoleArgument("", "type 'IAMSURE' to confirm")] string iAmSure)
        {
            if (iAmSure != "IAMSURE") return;

            foreach (FakeItem fakeItem in RaidSession.Instance.FakeItems.Values)
            {
                fakeItem.Place(LITUtils.PlayerFront, RaidSession.Instance.Player.Transform.rotation);
                FikaBridge.SendPlacedStateChangedPacket(fakeItem, true);
            }
        }

        [ConsoleCommand("LeaveItThere.tp_item_to_player", "", null, "Teleport item to the player. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void TPItemToPlayer([ConsoleArgument(0, "Item number via LeaveItThere.list command")] int itemNum)
        {
            if (itemNum > RaidSession.Instance.FakeItems.Count - 1)
            {
                ConsoleScreen.LogError("No placed item found!");
                return;
            }

            FakeItem fakeItem = RaidSession.Instance.FakeItems.Values.ToList()[itemNum];

            ConsoleScreen.Log("Teleporting item!");
            fakeItem.Place(LITUtils.PlayerFront, RaidSession.Instance.Player.Transform.rotation);
            FikaBridge.SendPlacedStateChangedPacket(fakeItem, true);
        }

        [ConsoleCommand("LeaveItThere.list", "", null, "List information about all placed items on the map.")]
        public static void ListPlacedItems()
        {
            int index = 0;

            ConsoleScreen.Log("---------------------------------------");
            foreach (FakeItem fakeItem in RaidSession.Instance.FakeItems.Values)
            {
                Vector3 playerPosition = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
                string itemName = string.Format("({0})".Localized(null), fakeItem.LootItem.Name.Localized(null));
                string direction = LITUtils.GetCardinalDirection(playerPosition, fakeItem.gameObject.transform.position);
                string distance = Vector3.Distance(playerPosition, fakeItem.gameObject.transform.position).ToString();

                ConsoleScreen.Log($"{itemName} (item number: {index}) placed {distance} units away from player ({direction})");

                index++;
            }
            ConsoleScreen.Log("---------------------------------------");
        }
    }
}
