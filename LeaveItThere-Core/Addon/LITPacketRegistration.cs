using LeaveItThere.Addon;
using SPT.Reflection.Utils;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace LeaveItThere.Addon
{
    public enum EPacketDestination
    {
        /// <summary>
        /// NOTE: The host will send the sender's packet back to them. This is usually recommended.
        /// </summary>
        Everyone,
        HostOnly,
        /// <summary>
        /// WARNING: Avoid using this if you don't know what you're doing. Incorrect use of it can cause desync.
        /// </summary>
        EveryoneExceptSender
    }

    /// <summary>
    /// Derivatives are singletons. Get them with LITPacketRegistration.Get&lt;T&gt;(). Do not instantiate them.
    /// <para>To use, create your own class for your packet and inherit the base: MyCustomPacket : LITPacketRegistration.</para>
    /// <para>Register it in your plugin's Awake() function with LITPacketRegistration.Get&lt;MyCustomPacket&gt;().Register();</para>
    /// <para>Send it with  LITPacketRegistration.Get&lt;MyCustomPacket&gt;().SendBool(), .SendString(), .SentStringAndBool(), or .SendByteArray()</para>
    /// <para>Make sure that your derived class is in a defined namespace to avoid potential ambiguous GUID generations</para>
    /// </summary>
    public abstract class LITPacketRegistration
    {
        public struct Packet
        {
            public Packet() { }

            // both of these values are set by, and gettable from, the registration itself.
            internal string PacketGUID;
            internal EPacketDestination Destination;

            // should be gettable so that receivers can know who sent the packet, but is internally set.
            public string SenderProfileId { get; internal set; }

            // all data fields can be set as desired.
            public string StringData = "";
            public bool BoolData = false;
            public byte[] ByteArrayData = [0];
        }

        // all derivatives must be singletons
        private static Dictionary<Type, LITPacketRegistration> _instances = [];

        protected LITPacketRegistration()
        {
            Plugin.DebugLog("1");
            var type = GetType();
            Plugin.DebugLog("2");
            if (_instances.ContainsKey(type))
            {
                Plugin.DebugLog("3");
                throw new InvalidOperationException($"{type.Name} is a singleton and an instance already exists! Do not instantiate LITPacketRegistration derivatives. Get them with LITPacketRegistration.Get<T>().");
            }
            Plugin.DebugLog("4");
            _instances[type] = this;
            Plugin.DebugLog("5");
        }

        /// <summary>
        /// Gets the singleton instance of a packet registration.
        /// </summary>
        /// <typeparam name="T">The type of your derived class. MyCustomPacket : LITPacketRegistration.</typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : LITPacketRegistration, new()
        {
            var type = typeof(T);
            Plugin.DebugLog($"Get<{type.Name}> called.");
            Plugin.DebugLog("11");
            if (!_instances.ContainsKey(type))
            {
                Plugin.DebugLog("22");
                _instances[type] = new T();
            }
            Plugin.DebugLog("33");
            
            return (T)_instances[type];
        }

        internal string PacketGUID { get => $"{GetType().Namespace}.{GetType().Name}"; }

        /// <summary>
        /// Invoked when the packet is received. NOTE: This will NEVER be called if Fika is not installed.
        /// </summary>
        /// <param name="packet">Get needed data from packet with: packet.BoolData, packet.StringData, or packet.ByteArrayData</param>
        public abstract void OnPacketReceived(Packet packet);

        /// <summary>
        /// Who the packet will be sent to.
        /// </summary>
        public virtual EPacketDestination Destination { get => EPacketDestination.Everyone; }

        /// <summary>
        /// StringData in packets sent will = this value if not otherwise set.
        /// </summary>
        public virtual string DefaultStringData { get => ""; }

        /// <summary>
        /// ByteArrayData in packets sent will = this value if not otherwise set.
        /// </summary>
        public virtual byte[] DefaultByteArrayData { get => [0]; }

        /// <summary>
        /// Invoked on the sender client every time the packet is sent. NOTE: This is STILL called even if Fika is not installed.
        /// </summary>
        public virtual void OnPacketSent(Packet packet) { }

        /// <summary>
        /// Registers packet. Highly recommended to register packet in plugin's Awake() function.
        /// </summary>
        public void Register()
        {
            LITPacketTools.RegisterPacket(this);
        }

        public void Unregister()
        {
            LITPacketTools.UnregisterPacket(PacketGUID);
        }

        /// <summary>
        /// Sends a packet via LITPacketRegistration. Consider using SendBool(), SendString(), SendStringAndBool(), or SendByteArray() instead for simplicity.
        /// </summary>
        public void Send(Packet packet)
        {
            packet.SenderProfileId = ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId;
            packet.PacketGUID = PacketGUID;
            packet.Destination = Destination;
            packet.StringData = packet.StringData.IsNullOrEmpty() ? DefaultStringData : packet.StringData;
            packet.ByteArrayData = packet.ByteArrayData.IsNullOrEmpty() ? DefaultByteArrayData : packet.ByteArrayData;

            OnPacketSent(packet);
            LITPacketTools.SendPacket(packet);
        }

        /// <param name="str">StringData</param>
        /// <param name="bl">BoolData</param>
        public void SendStringAndBool(string str, bool bl)
        {
            Packet packet = new Packet()
            {
                StringData = str,
                BoolData = bl
            };
            Send(packet);
        }

        /// <param name="value">BoolData</param>
        public void SendBool(bool value)
        {
            Packet packet = new()
            {
                BoolData = value
            };
            Send(packet);
        }

        /// <param name="value">StringData</param>
        public void SendString(string value)
        {
            Packet packet = new()
            {
                StringData = value
            };
            Send(packet);
        }

        /// <param name="value">ByteArrayData</param>
        public void SendByteArray(byte[] value)
        {
            Packet packet = new()
            {
                ByteArrayData = value
            };
            Send(packet);
        }
    }
}