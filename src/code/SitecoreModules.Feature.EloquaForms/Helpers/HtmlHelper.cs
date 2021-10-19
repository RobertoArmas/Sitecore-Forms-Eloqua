using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Mvc.Helpers;

namespace SitecoreModules.Feature.EloquaForms.Helpers
{
    public static class HtmlHelper
    {
        public static bool IsExperienceFormsEditMode(this SitecoreHelper helper) => Context.Request.QueryString["sc_formmode"] != null;
    }
}
