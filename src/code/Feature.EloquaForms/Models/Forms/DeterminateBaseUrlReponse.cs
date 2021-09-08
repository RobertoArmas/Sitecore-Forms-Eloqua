using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Feature.EloquaForms.Models.Forms
{
    /// <summary>
    /// Response object that contains data for which base URL to use when using the Eloqua API.
    /// </summary>
    public class DetermineBaseUrlResponse
    {
        [JsonProperty("site")]
        public Site SiteInfo { get; set; }

        [JsonProperty("user")]
        public User UserInfo { get; set; }

        [JsonProperty("urls")]
        public Urls UrlInfo { get; set; }

        public class Site
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class User
        {
            [JsonProperty("id")]
            public int id { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("firstName")]
            public string FirstName { get; set; }

            [JsonProperty("lastName")]
            public string LastName { get; set; }

            [JsonProperty("emailAddress")]
            public string EmailAddress { get; set; }
        }

        public class Urls
        {
            [JsonProperty("base")]
            public string BaseUrl { get; set; }

            [JsonProperty("apis")]
            public Apis APIs { get; set; }
        }

        public class Apis
        {
            [JsonProperty("soap")]
            public Soap Soap { get; set; }

            [JsonProperty("rest")]
            public Rest Rest { get; set; }
        }

        public class Soap
        {
            [JsonProperty("standard")]
            public string Standard { get; set; }

            [JsonProperty("dataTransfer")]
            public string DataTransfer { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("externalAction")]
            public string ExternalAction { get; set; }
        }

        public class Rest
        {
            [JsonProperty("standard")]
            public string Standard { get; set; }

            [JsonProperty("bulk")]
            public string Bulk { get; set; }
        }

    }
}
