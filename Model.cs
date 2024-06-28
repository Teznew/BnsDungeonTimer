using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BnsDungeonTimer
{
    public class Stage
    {
        [JsonProperty("stage_name")]
        public string StageName { get; set; }

        [JsonProperty("details")]
        public List<Detail> Details { get; set; }
    }

    public class Detail
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("timestamp")]
        public string TimestampStr { get; set; }
    }
}
