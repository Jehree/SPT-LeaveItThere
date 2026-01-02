using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using LeaveItThere.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Helpers;

public static class ItemHelper
{
    public static LootItem GetLootItemInWorld(string itemId)
    {
        foreach (LootItem lootItem in RaidSession.Instance.GameWorld.LootItems.GetValuesEnumerator())
        {
            if (lootItem.ItemId == itemId) return lootItem;
        }

        return null;
    }

    public static void SetItemColor(Color color, GameObject gameObject)
    {
        MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
            if (!material.HasProperty("_Color")) continue;
            renderer.material.color = color;
        }
    }

    public static bool ItemCanBePickedUp(Item item)
    {
        InventoryController playerInventoryController = RaidSession.Instance.Player.InventoryController;
        InventoryEquipment playerEquipment = playerInventoryController.Inventory.Equipment;
        var pickedUpResult = InteractionsHandlerClass.QuickFindAppropriatePlace(item, playerInventoryController, playerEquipment.ToEnumerable(), InteractionsHandlerClass.EMoveItemOrder.PickUp, true);
        return pickedUpResult.Succeeded;
    }

    public static void MakeSearchableItemFullySearched(SearchableItemItemClass searchableItem)
    {
        IPlayerSearchController controller = Singleton<GameWorld>.Instance.MainPlayer.SearchController;
        controller.SetItemAsSearched(searchableItem);

        ForAllChildrenInItem(
            searchableItem,
            (Item item) =>
            {
                controller.SetItemAsKnown(item, false);
                if (item is SearchableItemItemClass)
                {
                    controller.SetItemAsSearched(item as SearchableItemItemClass);
                }
            }
        );
    }

    public static void ForAllChildrenInItem(Item parent, Action<Item> callable)
    {
        if (parent is not CompoundItem) return;
        CompoundItem compoundParent = parent as CompoundItem;
        foreach (StashGridClass grid in compoundParent.Grids)
        {
            IEnumerable<Item> children = grid.Items;

            foreach (Item child in children)
            {
                callable(child);
                ForAllChildrenInItem(child, callable);
            }
        }
        foreach (Slot slot in compoundParent.Slots)
        {
            IEnumerable<Item> children = slot.Items;

            foreach (Item child in children)
            {
                callable(child);
                ForAllChildrenInItem(child, callable);
            }
        }
    }

    public static Item StringToItem(string base64String)
    {
        byte[] bytes = Convert.FromBase64String(base64String);
        return BytesToItem(bytes);
    }

    public static Item BytesToItem(byte[] bytes)
    {
        try
        {
            EFTReaderClass eftReader = new(new ArraySegment<byte>(bytes));
            return EFTItemSerializerClass.DeserializeItem(eftReader.ReadEFTItemDescriptor(), Singleton<ItemFactoryClass>.Instance, []);
        }
        catch (Exception e)
        {
            string msg1 = "Failed to deserialize item from LeaveItThere-ItemData!";
            string msg2 = "This is usually caused by placing a modded item, updating the mod to a new version with changes made to that item,";
            string msg3 = "then trying to load into the map where it was placed. The item (and any contents if any) will be lost.";
            string msg4 = "Alt F4 and downgrade to an older version of culprit mod and unplace items before re-updating.";
            string msg5 = "Auto backups can also be used if needed, located in user/profiles/LeaveItThere-ItemData/[your_profile_id]/backups";
            string fullMsg = msg1 + msg2 + msg3 + msg4 + msg5;

            Plugin.LogSource.LogWarning(fullMsg);
            ConsoleScreen.LogWarning(msg5);
            ConsoleScreen.LogWarning(msg4);
            ConsoleScreen.LogWarning(msg3);
            ConsoleScreen.LogWarning(msg2);
            ConsoleScreen.LogWarning(msg1);
            InteractionHelper.NotificationLongWarning("Problem spawning item! Press ~ for more info!");
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);

            Plugin.LogSource.LogError(e);
            return null;
        }
    }

    public static byte[] ItemToBytes(Item item)
    {
        EFTWriterClass eftWriter = new();
        var descriptor = EFTItemSerializerClass.SerializeItem(item, Singleton<GameWorld>.Instance.MainPlayer.SearchController);
        eftWriter.WriteEFTItemDescriptor(descriptor);
        return eftWriter.ToArray();
    }

    public static string ItemToString(Item item)
    {
        byte[] bytes = ItemToBytes(item);
        return Convert.ToBase64String(bytes);
    }

    public static FlatItemsDataClass[] RemoveLostInsuredItemsByIds(FlatItemsDataClass[] lostInsuredItems, List<string> idsToRemove)
    {
        List<FlatItemsDataClass> editedLostInsuredItems = [];
        foreach (FlatItemsDataClass item in lostInsuredItems)
        {
            if (idsToRemove.Contains(item._id)) continue;
            editedLostInsuredItems.Add(item);
        }

        return [.. editedLostInsuredItems];
    }
}
