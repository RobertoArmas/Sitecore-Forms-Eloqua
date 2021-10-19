using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SitecoreModules.Feature.EloquaForms.Models.Forms
{
    /// <summary>
    /// Model for a Form Post Body.
    /// </summary>
    public class PostFormBody
    {
        [JsonProperty("fieldValues")]
        public List<FieldValue> FieldValues { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        public PostFormBody()
        {
            FieldValues = new List<FieldValue>();
        }
    }
}
