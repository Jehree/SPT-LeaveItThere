using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveItThere.Helpers
{
    internal class SPTServerHelper
    {
        public static T ServerRoute<T>(string url, object data = default(object))
        {
            string json = JsonConvert.SerializeObject(data);
            var req = RequestHandler.PostJson(url, json);
            return JsonConvert.DeserializeObject<T>(req);
        }
        public static string ServerRoute(string url, object data = default(object))
        {
            string json;
            if (data is string)
            {
                Dictionary<string, string> dataDict = new Dictionary<string, string>();
                dataDict.Add("data", (string)data);
                json = JsonConvert.SerializeObject(dataDict);
            }
            else
            {
                json = JsonConvert.SerializeObject(data);
            }

            return RequestHandler.PutJson(url, json);
        }
    }
}
