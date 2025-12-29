using Comfort.Common;
using EFT;

namespace LeaveItThere.StateSynchronizer;

public abstract class StateSynchronizer
{
    private object _data;
    public string DatabaseKey;

    public TData GetData<TData>() where TData : class
    {
        return _data as TData;
    }

    public void Register()
    {
        Singleton<GameWorld>.Instance.MainPlayer.GetComponent<StateSynchronizerDatabase>().RegisterSynchronizer(this);
    }
}
