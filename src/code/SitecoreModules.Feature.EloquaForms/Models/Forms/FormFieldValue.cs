using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreModules.Feature.EloquaForms.Models.Forms
{
    /// <summary>
    /// Represents an Eloqua form field value.
    /// </summary>
    public class FormFieldValue
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
