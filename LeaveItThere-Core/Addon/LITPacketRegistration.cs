using LeaveItThere.Addon;
using SPT.Reflection.Utils;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using LeaveItThere.Fika;

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

            public string JsonData;

            public T GetData<T>()
            {
                return JsonConvert.DeserializeObject<T>(JsonData);
            }
        }

        // all derivatives must be singletons
        private static Dictionary<Type, LITPacketRegistration> _instances = [];

        protected LITPacketRegistration()
        {
            var type = GetType();
            if (_instances.ContainsKey(type))
            {
                throw new InvalidOperationException($"{type.Name} is a singleton and an instance already exists! Do not instantiate LITPacketRegistration derivatives. Get them with LITPacketRegistration.Get<YourPacketClassName>().");
            }
            _instances[type] = this;
        }

        /// <summary>
        /// Gets or creates the singleton instance of a packet registration.
        /// </summary>
        /// <typeparam name="T">The type of your derived class. MyCustomPacket : LITPacketRegistration.</typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : LITPacketRegistration, new()
        {
            var type = typeof(T);
            if (!_instances.ContainsKey(type))
            {
                _instances[type] = new T();
            }
            return (T)_instances[type];
        }

        internal string PacketGUID { get => $"{GetType().Namespace}.{GetType().Name}"; }

        /// <summary>
        /// Invoked when the packet is received. NOTE: This is NEVER called unless if Fika is installed.
        /// </summary>
        /// <param name="packet">Get needed data from packet with: packet.BoolData, packet.StringData, or packet.ByteArrayData</param>
        public abstract void OnPacketReceived(Packet packet);

        /// <summary>
        /// Who the packet will be sent to.
        /// </summary>
        public virtual EPacketDestination Destination { get => EPacketDestination.Everyone; }

        /// <summary>
        /// Invoked on the sender client every time the packet is sent. NOTE: This is never called unless Fika is installed.
        /// </summary>
        protected virtual void OnPacketSent(Packet packet) { }

        /// <summary>
        /// Registers packet. Highly recommended to register packet in plugin's Awake() function.
        /// </summary>
        public void Register()
        {
            FikaBridge.RegisterPacket(this);
        }

        public void Unregister()
        {
            FikaBridge.UnregisterPacket(PacketGUID);
        }

        internal void SendPacket(Packet packet)
        {
            packet.SenderProfileId = ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId;
            packet.PacketGUID = PacketGUID;
            packet.Destination = Destination;

            OnPacketSent(packet);
            FikaBridge.SendPacket(packet);
        }

        /// <summary>
        /// Sends a packet containing data given. This function does nothing if Fika is not installed.
        /// </summary>
        /// <param name="data">Data to send</param>
        public void SendData(object data)
        {
            if (!Plugin.FikaInstalled) return;
            if (FikaBridge.IAmHost() && Destination == EPacketDestination.HostOnly) return;

            Packet packet = new()
            {
                JsonData = JsonConvert.SerializeObject(data),
            };
            SendPacket(packet);
        }
    }
}