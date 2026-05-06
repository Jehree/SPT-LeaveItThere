using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using Newtonsoft.Json;

namespace LeaveItThere.Addon;

public abstract class BaseStateSynchronizer
{
    [JsonIgnore]
    public FakeItem FakeItem { get; private set; }
    [JsonIgnore]
    public string DatabaseKey { get; private set; }

    [JsonProperty("_rawData")]
    private string _rawData;
    internal string RawData
    {
        get
        {
            return _rawData;
        }
        set
        {
            _rawData = value;
            UpdateData();
        }
    }

    internal protected void ForceSetRawData(string rawData)
    {
        _rawData = rawData;
    }

    internal void Init(FakeItem fakeItem, string databaseKey)
    {
        FakeItem = fakeItem;
        DatabaseKey = databaseKey;
    }

    internal protected abstract void UpdateData();
    internal abstract void OnRegisteredInternal();

    /// <summary>
    /// Runs every time any client calls .Synchronize()
    /// </summary>
    public abstract void OnSynchronized();
    /// <summary>
    /// Runs when synchronizer is registered
    /// </summary>
    public virtual void OnRegistered() { }
}

public abstract class StateSynchronizer<TData> : BaseStateSynchronizer where TData : class, new()
{
    /// <summary>
    /// Your inputed TData class
    /// </summary>
    [JsonIgnore]
    public TData Data { get; private set; }

    internal protected override void UpdateData()
    {
        Data = JsonConvert.DeserializeObject<TData>(RawData);
    }

    internal override void OnRegisteredInternal()
    {
        if (RawData == null)
        {
            Data = new();
            ForceSetRawData(JsonConvert.SerializeObject(Data));
        }
        else
        {
            UpdateData();
        }
    }

    /// <summary>
    /// Synchronize the Data property across all clients.
    /// <para>Use wisely, this sends packets via Fika and should not be called too often. Do not call every frame.</para>
    /// <para>If Fika is <b>NOT</b> installed, OnSynchronized() is called locally</para>
    /// <para>If Fika <b>IS</b> installed and caller is the host of the raid, OnSynchronized() is called locally and then a packet is sent to all other clients</para>
    /// <para>If Fika <b>IS</b> installed and the caller is a non-host client, a packet is sent to the host client, which will return a packet back to the caller (and all other clients) that calls OnSynchronized()</para>
    /// </summary>
    public void Synchronize()
    {
        string json = JsonConvert.SerializeObject(Data);

        // non-host clients will be synchronized via return packet triggering StateSynchronizerPacketHandler.OnPacketReceived()
        // this does nothing if Fika is not installed
        FikaBridge.SendStateSynchronizerUpdatePacket(json, DatabaseKey, FakeItem);

        // this will also be true if Fika is not installed
        if (FikaBridge.IAmHost())
        {
            // host won't receive a return packet, must synchronize manually
            RawData = json;
            OnSynchronized();
        }
    }
}
