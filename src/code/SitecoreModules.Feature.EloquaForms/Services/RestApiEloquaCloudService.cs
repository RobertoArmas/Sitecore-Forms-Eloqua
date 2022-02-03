using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SitecoreModules.Feature.EloquaForms.Models.Forms;

namespace SitecoreModules.Feature.EloquaForms.Services
{
    /// <summary>
    /// Connects to the Eloqua cloud service via the REST API. SEE: <seealso cref="https://docs.oracle.com/cloud/latest/marketingcs_gs/OMCAC/index.html"/>
    /// </summary>
    internal class RestApiEloquaCloudService : IEloquaCloudService
    {
        private string _siteName;

        private string _userName;

        private string _password;
        /// <summary>
        /// Creates a new instance of the <see cref="RestApiEloquaCloudService"/> class.
        /// </summary>
        public RestApiEloquaCloudService()
        {
            // Load sitename
            _siteName = Sitecore.Configuration.Settings.GetSetting("Eloqua.SiteName");

            if (string.IsNullOrWhiteSpace(_siteName))
            {
                throw new ApplicationException("Sitecore setting is missing: 'Eloqua.SiteName'");
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

        /// <summary>
        /// Creates a new instance of the <see cref="RestApiEloquaCloudService"/> class.
        /// </summary>
        public RestApiEloquaCloudService(string siteName, string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(siteName))
            {
                throw new ArgumentNullException(nameof(siteName));
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            _siteName = siteName;
            _userName = userName;
            _password = password;
        }

        /// <summary>
        /// Posts Form data to Eloqua.
        /// </summary>
        public PostFormBody PostForm(string formId, params FormFieldValue[] fields)
        {
            // Validate the formID is present.
            if (string.IsNullOrWhiteSpace(formId))
            {
                throw new ArgumentNullException(nameof(formId));
            }

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
            string baseUrl = DetermineBaseUrl();

            // Create the PostFormBody object to be send to Eloqua.
            var postFormBody = new PostFormBody { Id = formId };

            // Add the field value to the form
            foreach (FormFieldValue f in fields)
            {
                postFormBody.FieldValues.Add(new FieldValue
                {
                    Id = f.Id,
                    Value = f.Value,
                    Type = "FieldValue"
                });
            }

            // Build the POST Url
            string relativePath = string.Concat("/api/REST/2.0/data/form/", formId);
            string fullUrl = string.Concat(baseUrl.TrimEnd(new[] { '/' }), "/", relativePath.TrimStart(new[] { '/' }));

            // Build the request object.
            var request = (HttpWebRequest)WebRequest.Create(fullUrl);
            request.Method = "POST";
            request.ContentType = "application/json";

            // Add the basic Authorization header.
            request.Headers["Authorization"] = BuildAuthorizationHeader();

            // Set the JSON as the body.
            string jsonBody = JsonConvert.SerializeObject(postFormBody);
            byte[] bodyBytes = Encoding.ASCII.GetBytes(jsonBody);
            request.ContentLength = bodyBytes.Length;

            // Send the request.
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(bodyBytes, 0, bodyBytes.Length);
            }

            // Return the response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            // Convert the response string to strongly-typed class.
            PostFormBody responseData = JsonConvert.DeserializeObject<PostFormBody>(responseString);

            return responseData;
        }

        #region infrastructure

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
