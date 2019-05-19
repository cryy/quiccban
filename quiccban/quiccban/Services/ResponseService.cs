using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quiccban.Services
{
    public class ResponseService
    {
        private Dictionary<string, string> Responses = new Dictionary<string, string>()
        {
            {"", ""}
        };



        public bool TryGet(string key, out string response)
            => Responses.TryGetValue(key, out response);


    }
}
