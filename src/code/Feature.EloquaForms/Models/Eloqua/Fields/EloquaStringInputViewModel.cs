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
    public class EloquaStringInputViewModel : StringInputViewModel
    {
        public string EloquaFieldId { get; set; }

        public bool DoPrefill { get; set; }

        public string EloquaPrefillId { get; set; }

        protected override void InitItemProperties(Item item)
        {
            base.InitItemProperties(item);
            this.EloquaFieldId = StringUtil.GetString(item.Fields["Eloqua Field Id"]?.Value);
            this.DoPrefill = MainUtil.GetBool(item.Fields["Do Prefill"]?.Value, false);
            this.EloquaPrefillId = StringUtil.GetString(item.Fields["Eloqua Prefill Id"]?.Value);
        }

        protected override void UpdateItemFields(Item item)
        {
            base.UpdateItemFields(item);
            item.Fields["Eloqua Field Id"]?.SetValue(this.EloquaFieldId, true);
            item.Fields["Do Prefill"]?.SetValue(this.DoPrefill ? "1" : string.Empty, true);
            item.Fields["Eloqua Prefill Id"]?.SetValue(this.EloquaPrefillId, true);
        }
    }
}
