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
        private string _siteId;

        public FormEloquaCloudService()
        {
            // Load siteId
            _siteId = Sitecore.Configuration.Settings.GetSetting("Eloqua.SiteId");

            if (string.IsNullOrWhiteSpace(_siteId))
            {
                throw new ApplicationException("Sitecore setting is missing: 'Eloqua.SiteId'");
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

        #endregion
    }
}
