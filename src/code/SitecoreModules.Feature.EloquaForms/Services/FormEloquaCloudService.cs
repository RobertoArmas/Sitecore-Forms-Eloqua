using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SitecoreModules.Feature.EloquaForms.Models.Forms;

namespace SitecoreModules.Feature.EloquaForms.Services
{
    internal class FormEloquaCloudService : IEloquaCloudService
    {
        private string _siteName;

        private string _siteId;

        private string _userName;

        private string _password;

        public FormEloquaCloudService()
        {
            // Load sitename
            _siteName = Sitecore.Configuration.Settings.GetSetting("Eloqua.SiteName");

            if (string.IsNullOrWhiteSpace(_siteName))
            {
                throw new ApplicationException("Sitecore setting is missing: 'Eloqua.SiteName'");
            }

            // Load siteId
            _siteId = Sitecore.Configuration.Settings.GetSetting("Eloqua.SiteId");

            if (string.IsNullOrWhiteSpace(_siteId))
            {
                throw new ApplicationException("Sitecore setting is missing: 'Eloqua.SiteId'");
            }

            // Load username
            _userName = Sitecore.Configuration.Settings.GetSetting("Eloqua.UserName");

            if (string.IsNullOrWhiteSpace(_userName))
            {
                throw new ApplicationException("Sitecore setting is missing: 'Eloqua.UserName'");
            }

            // Load password
            _password = Sitecore.Configuration.Settings.GetSetting("Eloqua.Password");

            if (string.IsNullOrWhiteSpace(_password))
            {
                throw new ApplicationException("Sitecore setting is missing: 'Eloqua.Password'");
            }
        }

        public PostFormBody PostForm(string formId, params FormFieldValue[] fields)
        {
            // Validate the fields array is not null.
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            // Validate there is at least one field to post.
            if (fields.Length == 0)
            {
                throw new ArgumentException("Parameter " + nameof(fields) + "cannot have a length of 0.");
            }

            // ELOQUA API NOTE:
            // Do not call the /id endpoint each time you make an API call. 
            // There will be throttling or rate-limiting imposed on the /id endpoint to prevent this behavior.
            // Instead, call it once per session (or after a reasonable period of time) and cache the results.
            string baseUrl = SolveBaseUrlBasedOnSiteId();

            // Create the PostFormBody object to be send to Eloqua.
            var postFormBody = new PostFormBody { Id = formId };
            List<KeyValuePair<string, string>> fieldsAsList = new List<KeyValuePair<string, string>>();

            // Add the field value to the form
            foreach (FormFieldValue f in fields)
            {
                postFormBody.FieldValues.Add(new FieldValue
                {
                    Name = f.Name,
                    Value = f.Value,
                    Type = "FieldValue"
                });

                if (f?.Name != null) fieldsAsList.Add((new KeyValuePair<string, string>(f.Name, f.Value)));
            }
            // Build the POST Url
            string relativePath = string.Concat("e/f2", "?");
            string fullUrl = string.Concat(baseUrl.TrimEnd(new[] { '/' }), "/", relativePath.TrimStart(new[] { '/' }));

            // Build the request object.
            var request = (HttpWebRequest)WebRequest.Create(fullUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            // Add the basic Authorization header.
            // request.Headers["Authorization"] = BuildAuthorizationHeader();

            // Set the JSON as the body.
            var content = new FormUrlEncodedContent(fieldsAsList.AsEnumerable<KeyValuePair<string, string>>());
            var urlEncodedString = content.ReadAsStringAsync().Result;
            byte[] bodyBytes = Encoding.ASCII.GetBytes(urlEncodedString);
            request.ContentLength = bodyBytes.Length;

            // Send the request.
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(bodyBytes, 0, bodyBytes.Length);
            }

            // Return the response
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                if (responseString.StartsWith("<table"))
                {
                    throw new Exception("Bad Request");
                }


                return postFormBody;
            }
            catch (WebException webException)
            {
                var resp = new StreamReader(webException.Response.GetResponseStream()).ReadToEnd();

                throw webException;
            }
        }

        #region infrastructure

        private string SolveBaseUrlBasedOnSiteId()
        {

            return "https://s" + _siteId + ".t.eloqua.com/";
        }

        /// <summary>
        /// Determines the BaseUrl.
        /// </summary>
        private string DetermineBaseUrl()
        {
            // Is the HttpRuntime cache available?
            if (HttpRuntime.Cache == null)
            {
                // Default to a live / non-cached lookup.
                return LookupBaseUrl();
            }

            // Lazy load the value from HttpRuntime cache.
            const string cacheKey = "Eloqua";

            // Is the value already cached?
            if (HttpRuntime.Cache[cacheKey] is string cachedBaseUrl)
            {
                return cachedBaseUrl;
            }

            // Lookup in web service
            string baseUrl = LookupBaseUrl();

            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                int cacheDuration = Sitecore.Configuration.Settings.GetIntSetting("Eloqua.BaseUrlCacheDurationInMinutes", 15);

                HttpRuntime.Cache.Insert(
                    cacheKey,
                    baseUrl,
                    null,
                    DateTime.UtcNow.AddMinutes(cacheDuration),
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Default,
                    null
                );
            }

            return baseUrl;
        }

        ///
        /// Determines the base url for Eloqua form post
        /// Documentation: https://docs.oracle.com/cloud/latest/marketingcs_gs/OMCAC/DeterminingBaseURL.html
        ///
        private string LookupBaseUrl()
        {
            string uri = "https://login.eloqua.com/id";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            // Add the Authorization header.
            request.Headers["Authorization"] = BuildAuthorizationHeader();

            // Issue the HTTP Request
            string responseBody = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        responseBody = reader.ReadToEnd();
                    }
                }
            }

            // De-serialize the JSON.
            var baseUrlResponse = JsonConvert.DeserializeObject<DetermineBaseUrlResponse>(responseBody);

            return baseUrlResponse?.UrlInfo?.BaseUrl;
        }

        ///
        /// Builds the Authorization Header.
        /// Documentation: https://docs.oracle.com/cloud/latest/marketingcs_gs/OMCAC/Authentication_Basic.html
        ///
        private string BuildAuthorizationHeader()
        {
            // siteName + '\' + username + ':' + password

            string auth = string.Format("{0}\\{1}:{2}", _siteName, _userName, _password);

            // encode to base 64
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(auth);
            string base64Auth = System.Convert.ToBase64String(plainTextBytes); ;

            return string.Concat("Basic", " ", base64Auth);
        }

        #endregion
    }
}
