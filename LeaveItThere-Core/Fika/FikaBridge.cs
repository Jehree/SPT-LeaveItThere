using LeaveItThere.Addon;
using LeaveItThere.Components;
using SPT.Reflection.Utils;
using System;
using System.Reflection;

namespace LeaveItThere.Fika;

internal static class FikaBridge
{
    public delegate void SimpleEventHandler();
    public delegate bool SimpleBoolReturnEventHandler();
    public delegate string SimpleStringReturnEventHandler();

    public static event SimpleEventHandler PluginEnableEmitted;
    public static void PluginEnable()
    {
        PluginEnableEmitted?.Invoke();
    }

    public static event SimpleBoolReturnEventHandler IAmHostEmitted;
    public static bool IAmHost()
    {
        bool? eventResponse = IAmHostEmitted?.Invoke();
        return eventResponse == null || eventResponse.Value;
    }

    public static event SimpleStringReturnEventHandler GetRaidIdEmitted;
    public static string GetRaidId()
    {
        string eventResponse = GetRaidIdEmitted?.Invoke();
        return eventResponse ?? ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId;
    }

    public delegate void SendPlacedStateChangedPacketHandler(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false, bool moveOnly = false);
    public static event SendPlacedStateChangedPacketHandler SendPlacedStateChangedPacketEmitted;
    public static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false, bool moveOnly = false)
    {
        SendPlacedStateChangedPacketEmitted?.Invoke(fakeItem, isPlaced, physicsEnableRequested, moveOnly);
    }

    public delegate void SendStateSynchronizerUpdatePacketHandler(string data, string databaseKey, FakeItem fakeItem);
    public static event SendStateSynchronizerUpdatePacketHandler SendStateSynchronizerUpdatePacketEmitted;
    public static void SendStateSynchronizerUpdatePacket(string data, string databaseKey, FakeItem fakeItem = null)
    {
        SendStateSynchronizerUpdatePacketEmitted?.Invoke(data, databaseKey, fakeItem);
    }

    public delegate void RegisterGenericPacketHandler(PacketRegistration registration);
    public static event RegisterGenericPacketHandler RegisterGenericPacketEmitted;
    public static void RegisterGenericPacket(PacketRegistration registration)
    {
        RegisterGenericPacketEmitted?.Invoke(registration);
    }

    public delegate void UnregisterGenericPacketHandler(string packetGUID);
    public static event UnregisterGenericPacketHandler UnregisterGenericPacketEmitted;
    public static void UnregisterGenericPacket(string packetGUID)
    {
        UnregisterGenericPacketEmitted?.Invoke(packetGUID);
    }

    public delegate void SendGenericPacketEventHandler(PacketRegistration.Packet abstractedPacket);
    public static event SendGenericPacketEventHandler SendGenericPacketEmitted;
    public static void SendGenericPacket(PacketRegistration.Packet abstractedPacket)
    {
        SendGenericPacketEmitted?.Invoke(abstractedPacket);
    }

    public static void LoadFikaModuleAssembly()
    {
        Assembly fikaModuleAssembly = Assembly.Load("LeaveItThere-FikaModule");
        Type main = fikaModuleAssembly.GetType("LeaveItThere.FikaModule.Main");
        MethodInfo init = main.GetMethod("Init");

        init.Invoke(main, null);
    }
}