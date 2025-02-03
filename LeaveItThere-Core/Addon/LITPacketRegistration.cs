using LeaveItThere.Addon;
using SPT.Reflection.Utils;
using System.Collections.Generic;
using System;

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
    /// Derivatives are singletons. Get them with LITPacketRegistration.Get<T>(). Do not instantiate them.
    /// </summary>
    public abstract class LITPacketRegistration
    {
        public class Packet
        {
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
        private static readonly Dictionary<Type, LITPacketRegistration> _instances = new();

        protected LITPacketRegistration()
        {
            var type = GetType();
            if (_instances.ContainsKey(type))
            {
                throw new InvalidOperationException($"{type.Name} is a singleton and an instance already exists! Do not instantiate LITPacketRegistration derivatives. Get them with LITPacketRegistration.Get<T>().");
            }
            _instances[type] = this;
        }

        /// <summary>
        /// Gets the singleton instance of a packet registration.
        /// </summary>
        /// <typeparam name="T">The type of your derived class. IAmTheTType : LITPacketRegistration.</typeparam>
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

        public string SenderProfileId { get; } = ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId;

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

        public void Send(Packet packet)
        {
            packet.SenderProfileId = SenderProfileId;
            packet.PacketGUID = PacketGUID;
            packet.Destination = Destination;
            packet.StringData = packet.StringData.IsNullOrEmpty() ? DefaultStringData : packet.StringData;
            packet.ByteArrayData = packet.ByteArrayData.IsNullOrEmpty() ? DefaultByteArrayData : packet.ByteArrayData;

            OnPacketSent(packet);
            LITPacketTools.SendPacket(packet);
        }

        public void SendBool(bool value)
        {
            Packet packet = new()
            {
                BoolData = value
            };
            Send(packet);
        }

        public void SendString(string value)
        {
            Packet packet = new()
            {
                StringData = value
            };
            Send(packet);
        }

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