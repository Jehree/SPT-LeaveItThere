using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Addon;
using LeaveItThere.Components;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Fika;

internal class FikaBridge
{
    public delegate void SimpleEvent();
    public delegate bool SimpleBoolReturnEvent();
    public delegate string SimpleStringReturnEvent();

    public static event SimpleEvent PluginEnableEmitted;
    public static void PluginEnable() { PluginEnableEmitted?.Invoke(); }


    public static event SimpleBoolReturnEvent IAmHostEmitted;
    public static bool IAmHost()
    {
        bool? eventResponse = IAmHostEmitted?.Invoke();

        if (eventResponse == null)
        {
            return true;
        }
        else
        {
            return eventResponse.Value;
        }
    }


    public static event SimpleStringReturnEvent GetRaidIdEmitted;
    public static string GetRaidId()
    {
        string eventResponse = GetRaidIdEmitted?.Invoke();

        if (eventResponse == null)
        {
            return ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId;
        }
        else
        {
            return eventResponse;
        }
    }


    public delegate void SendPlacedStateChangedPacketEvent(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false);
    public static event SendPlacedStateChangedPacketEvent SendPlacedStateChangedPacketEmitted;
    public static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false) 
        { SendPlacedStateChangedPacketEmitted?.Invoke(fakeItem, isPlaced, physicsEnableRequested); }


    public delegate void SendSpawnItemPacketEvent(Item item, Vector3 position, Quaternion rotation, Action<LootItem> senderCallback);
    public static event SendSpawnItemPacketEvent SendSpawnItemPacketEmitted;
    public static void SendSpawnItemPacket(Item item, Vector3 position, Quaternion rotation, Action<LootItem> senderCallback)
        { SendSpawnItemPacketEmitted?.Invoke(item, position, rotation, senderCallback); }


    public delegate void RegisterPacketEvent(LITPacketRegistration registration);
    public static event RegisterPacketEvent RegisterPacketEmitted;
    public static void RegisterPacket(LITPacketRegistration registration) { RegisterPacketEmitted?.Invoke(registration); }


    public delegate void UnregisterPacketEvent(string packetGUID);
    public static event UnregisterPacketEvent UnregisterPacketEmitted;
    public static void UnregisterPacket(string packetGUID) { UnregisterPacketEmitted?.Invoke(packetGUID); }


    public delegate void SendPacketEvent(LITPacketRegistration.Packet abstractedPacket);
    public static event SendPacketEvent SendPacketEmitted;
    public static void SendPacket(LITPacketRegistration.Packet abstractedPacket) {  SendPacketEmitted?.Invoke(abstractedPacket); }
}
