using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Feature.EloquaForms.Models.Forms
{
    /// <summary>
    /// Represents an Eloqua form field value.
    /// </summary>
    public class FieldValue
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }


        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
