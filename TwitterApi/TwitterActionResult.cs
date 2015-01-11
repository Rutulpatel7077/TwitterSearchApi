using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WZWVDLL.Bekende_Nederlanders;

namespace TwitterApi
{
    public class TwitterActionResult
    {
        public string RequestedName { get; private set; }
        public TwitterSearch Results { get; private set; }
        public bool FromCache { get; private set; }
        public string DateTime { get; private set; }

        internal TwitterActionResult(string RequestedName, TwitterSearch Results, string DateTime)
        {
            this.RequestedName = RequestedName;
            this.Results = Results;
            this.FromCache = FromCache;
            this.DateTime = DateTime;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
        
    }
}