using System.Collections.Generic;

namespace LeaveItThere.Common
{
    internal class ItemFilter
    {
        public bool WhitelistEnabled = false;
        public bool BlacklistEnabled = false;
        public List<string> Whitelist = new();
        public List<string> Blacklist = new();
    }
}
