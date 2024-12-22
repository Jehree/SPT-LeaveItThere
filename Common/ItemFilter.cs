using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
