using Comfort.Common;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;

namespace LeaveItThere.FikaModule.Helpers;

internal class FikaTools
{
    public static FikaServer Server => Singleton<FikaServer>.Instance;
    public static FikaClient Client => Singleton<FikaClient>.Instance;

    public static bool IAmServer()
    {
        return Singleton<FikaServer>.Instantiated;
    }

    public static bool IAmClient()
    {
        return Singleton<FikaClient>.Instantiated;
    }

    public static string GetRaidId()
    {
        return FikaBackendUtils.GroupId;
    }
}
