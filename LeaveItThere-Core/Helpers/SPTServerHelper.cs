using Newtonsoft.Json;
using SPT.Common.Http;
using System.Collections.Generic;

namespace LeaveItThere.Helpers
{
    internal class SPTServerHelper
    {
        public static T ServerRoute<T>(string url, T data = default)
        {
            string json = JsonConvert.SerializeObject(data);
            string req = RequestHandler.PostJson(url, json);
            return JsonConvert.DeserializeObject<T>(req);
        }
    }
}
