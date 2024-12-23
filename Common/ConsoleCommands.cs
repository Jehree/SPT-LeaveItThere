using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.Interactive;
using EFT.UI;
using LeaveItThere.Components;
using LeaveItThere.Helpers;
using System;
using UnityEngine;

namespace LeaveItThere.Common
{
    internal class ConsoleCommands
    {
        [ConsoleCommand("lit_unplace_all_items_below_cost", "", null, "Un-Place all items on the map below a cost amount.")]
        public static void ClearPlacedItems([ConsoleArgument(0, "Cost Amount")] int costAmount)
        {
            ItemHelper.ForAllItemsUnderCost(
                costAmount,
                (LootItem lootItem, RemoteInteractable interactable) =>
                {
                    interactable.DemolishItem();
                } 
            );
        }

        [ConsoleCommand("lit_teleport_all_placed_items_to_player", "", null, "Un-Place all items on the map below a cost amount and teleport them to the player. If you run this command in error somehow, ALT F4 to avoid the changes being saved.")]
        public static void ClearPlacedItems([ConsoleArgument(0, "Cost Amount")] int costAmount, [ConsoleArgument("", "type 'IAMSURE' to confirm")] string iAmSure)
        {
            if (iAmSure != "IAMSURE") return;

            ItemHelper.ForAllItemsUnderCost(
                costAmount,
                (LootItem lootItem, RemoteInteractable interactable) =>
                {
                    interactable.DemolishItem();
                    lootItem.gameObject.transform.position = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
                }
            );
        }

        [ConsoleCommand("lit_list_placed_items", "", null, "List information about all placed items on the map.")]
        public static void ListPlacedItems()
        {
            ConsoleScreen.Log("---------------------------------------");
            ItemHelper.ForAllItemsUnderCost(
                999999999,
                (LootItem lootItem, RemoteInteractable interactable) =>
                {
                    Vector3 playerPosition = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
                    string itemName = string.Format("({0})".Localized(null), lootItem.Name.Localized(null));
                    string direction = Utils.GetCardinalDirection(playerPosition, interactable.gameObject.transform.position);
                    string distance = Vector3.Distance(playerPosition, interactable.gameObject.transform.position).ToString();
                    ConsoleScreen.Log($"{itemName} placed {distance} units away from player ({direction})");
                }
            );
            ConsoleScreen.Log("---------------------------------------");
        }
    }
}
