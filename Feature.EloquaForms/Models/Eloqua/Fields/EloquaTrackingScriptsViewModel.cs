using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.ExperienceForms.Mvc.Models.Fields;

namespace Feature.EloquaForms.Models.Eloqua.Fields
{
    [Serializable]
    public class EloquaTrackingScriptsViewModel : ListViewModel
    {
        public string EloquaSiteId { get; set; }

        public string EloquaFormName { get; set; }

        public string CustomerGUID { get; set; }

        protected override void InitItemProperties(Item item)
        {
            // on load of the form
            base.InitItemProperties(item);
            this.EloquaSiteId = StringUtil.GetString(item.Fields["Eloqua Site Id"]);
            this.EloquaFormName = StringUtil.GetString(item.Fields["Eloqua Form Name"]);

        }

        protected override void UpdateItemFields(Item item)
        {
            base.UpdateItemFields(item);
            item.Fields["Eloqua Site Id"]?.SetValue(this.EloquaSiteId, true);
            item.Fields["Eloqua Form Name"]?.SetValue(this.EloquaFormName, true);
        }
    }
}
