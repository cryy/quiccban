using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban
{
    public struct ConfigResult
    {
        public bool IsValid;
        public string Message;
        public Config ParsedConfig;
    }
}
