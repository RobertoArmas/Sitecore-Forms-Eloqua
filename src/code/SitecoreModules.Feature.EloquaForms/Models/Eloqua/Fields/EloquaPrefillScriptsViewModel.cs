using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.ExperienceForms.Mvc.Models.Fields;

namespace SitecoreModules.Feature.EloquaForms.Models.Eloqua.Fields
{
    [Serializable]
    public class EloquaPrefillScriptsViewModel : FieldViewModel
    {
        public string EloquaFormName { get; set; }

        public string EloquaDataLookupId { get; set; }

        protected override void InitItemProperties(Item item)
        {
            base.InitItemProperties(item);
            this.EloquaFormName = StringUtil.GetString(item.Fields["Eloqua Form Name"]);
            this.EloquaDataLookupId = StringUtil.GetString(item.Fields["Eloqua Data Lookup Id"]);

        }

        protected override void UpdateItemFields(Item item)
        {
            base.UpdateItemFields(item);
            item.Fields["Eloqua Form Name"]?.SetValue(this.EloquaFormName, true);
            item.Fields["Eloqua Data Lookup Id"]?.SetValue(this.EloquaDataLookupId, true);
        }
    }
}
