using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.Interactive;
using EFT.UI;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System;
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
                (ItemRemotePair pair) =>
                {
                    pair.RemoteInteractable.DemolishItem();
                    FikaInterface.SendPlacedStateChangedPacket(pair);
                } 
            );
        }

        [ConsoleCommand("lit_teleport_all_placed_items_to_player", "", null, "Teleport items below cost to the player. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void TPAllItemsUnderCostToPlayer([ConsoleArgument(0, "Cost Amount")] int costAmount, [ConsoleArgument("", "type 'IAMSURE' to confirm")] string iAmSure)
        {
            if (iAmSure != "IAMSURE") return;

            ItemHelper.ForAllItemsUnderCost(
                costAmount,
                (ItemRemotePair pair) =>
                {
                    var session = ModSession.GetSession();
                    ItemPlacer.PlaceItem(pair.LootItem, session.Player.Transform.position, session.Player.Transform.rotation);
                    FikaInterface.SendPlacedStateChangedPacket(pair);
                }
            );
        }

        [ConsoleCommand("lit_teleport_item_to_player", "", null, "Teleport item to the player. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void TPItemToPlayer([ConsoleArgument(0, "Item number via lit_list_placed_items command")] int itemNum, [ConsoleArgument("", "type 'IAMSURE' to confirm")] string iAmSure)
        {
            if (iAmSure != "IAMSURE") return;

            var session = ModSession.GetSession();

            if (itemNum > session.ItemRemotePairs.Count - 1)
            {
                ConsoleScreen.LogError("No placed item found!");
                return;
            }

            ItemRemotePair selectedPair = session.ItemRemotePairs[itemNum];

            if (!selectedPair.Placed)
            {
                ConsoleScreen.LogError("No placed item found!");
                return;
            }

            ConsoleScreen.Log("Teleporting item!");
            ItemPlacer.PlaceItem(selectedPair.LootItem, session.Player.Transform.position, session.Player.Transform.rotation);
            ItemPlacer.PlacedPlayerFeedback(selectedPair.LootItem.Item);
            FikaInterface.SendPlacedStateChangedPacket(selectedPair);
        }

        [ConsoleCommand("lit_list_placed_items", "", null, "List information about all placed items on the map.")]
        public static void ListPlacedItems()
        {
            var session = ModSession.GetSession();
            int index = 0;

            ConsoleScreen.Log("---------------------------------------");
            foreach (ItemRemotePair pair in session.ItemRemotePairs)
            {
                if (pair.Placed != false)
                {
                    Vector3 playerPosition = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
                    string itemName = string.Format("({0})".Localized(null), pair.LootItem.Name.Localized(null));
                    string direction = Utils.GetCardinalDirection(playerPosition, pair.RemoteInteractable.gameObject.transform.position);
                    string distance = Vector3.Distance(playerPosition, pair.RemoteInteractable.gameObject.transform.position).ToString();
                    ConsoleScreen.Log($"{itemName} (item number: {index}) placed {distance} units away from player ({direction})");
                }
                index++;
            }
            ConsoleScreen.Log("---------------------------------------"); 
        }
    }
}
