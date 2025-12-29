using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Components;
using SPT.Reflection.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace LeaveItThere.Fika;

internal static class FikaBridge
{
    public delegate void SimpleEvent();
    public delegate bool SimpleBoolReturnEvent();
    public delegate string SimpleStringReturnEvent();

    public static event SimpleEvent PluginEnableEmitted;
    public static void PluginEnable()
    {
        PluginEnableEmitted?.Invoke();
    }

    public static event SimpleBoolReturnEvent IAmHostEmitted;
    public static bool IAmHost()
    {
        bool? eventResponse = IAmHostEmitted?.Invoke();
        return eventResponse == null || eventResponse.Value;
    }

    public static event SimpleStringReturnEvent GetRaidIdEmitted;
    public static string GetRaidId()
    {
        string eventResponse = GetRaidIdEmitted?.Invoke();
        return eventResponse ?? ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId;
    }

    public delegate void SendPlacedStateChangedPacketEvent(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false);
    public static event SendPlacedStateChangedPacketEvent SendPlacedStateChangedPacketEmitted;
    public static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false)
    {
        SendPlacedStateChangedPacketEmitted?.Invoke(fakeItem, isPlaced, physicsEnableRequested);
    }

    public static void LoadFikaModuleAssembly()
    {
        Assembly fikaModuleAssembly = Assembly.Load("LeaveItThere-FikaModule");
        Type main = fikaModuleAssembly.GetType("LeaveItThere.FikaModule.Main");
        MethodInfo init = main.GetMethod("Init");

        init.Invoke(main, null);
    }
}