using System.Collections.Generic;

namespace LeaveItThere.Common
{
    internal class ItemFilter
    {
        public bool WhitelistEnabled = false;
        public bool BlacklistEnabled = false;
        public List<string> Whitelist;
        public List<string> Blacklist;
    }
}
