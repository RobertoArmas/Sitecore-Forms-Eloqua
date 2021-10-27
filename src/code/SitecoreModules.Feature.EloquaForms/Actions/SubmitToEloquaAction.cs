using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;
using SitecoreModules.Feature.EloquaForms.Factories;
using SitecoreModules.Feature.EloquaForms.Models.Eloqua.Fields;
using SitecoreModules.Feature.EloquaForms.Models.Forms;
using SitecoreModules.Feature.EloquaForms.Services;
using static System.FormattableString;

namespace SitecoreModules.Feature.EloquaForms.Actions
{
    /// <summary>
    /// Executes a submit action for logging the form submit status.
    /// </summary>
    /// <seealso cref="Sitecore.ExperienceForms.Processing.Actions.SubmitActionBase{TParametersData}" />
    public class SubmitToEloquaAction : SubmitActionBase<string>
    {

        protected IEloquaCloudService EloquaCloudService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitToEloquaAction"/> class.
        /// </summary>
        /// <param name="submitActionData">The submit action data.</param>
        public SubmitToEloquaAction(ISubmitActionData submitActionData) : base(submitActionData)
        {
            EloquaCloudService = EloquaCloudServiceFactory.Build();
        }

        /// <summary>
        /// Tries to convert the specified <paramref name="value" /> to an instance of the specified target type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="target">The target object.</param>
        /// <returns>
        /// true if <paramref name="value" /> was converted successfully; otherwise, false.
        /// </returns>
        protected override bool TryParse(string value, out string target)
        {
            target = string.Empty;
            return true;
        }

        /// <summary>
        /// Executes the action with the specified <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="formSubmitContext">The form submit context.</param>
        /// <returns>
        ///   <c>true</c> if the action is executed correctly; otherwise <c>false</c>
        /// </returns>
        protected override bool Execute(string data, FormSubmitContext formSubmitContext)
        {
            Assert.ArgumentNotNull(formSubmitContext, nameof(formSubmitContext));

            if (!formSubmitContext.HasErrors)
            {
                return SubmitFormToEloqua(formSubmitContext);
            }

            Logger.Warn(Invariant($"Form {formSubmitContext.FormId} submitted with errors: {string.Join(", ", formSubmitContext.Errors.Select(t => t.ErrorMessage))}."), this);

            return true;
        }

        /// <summary>
        /// This method goes through all fields in the form, retrieves the EloquaFieldId from the Sitecore field Item, and sends them to Eloqua
        /// </summary>
        /// <param name="formSubmitContext"></param>
        /// <returns></returns>
        public bool SubmitFormToEloqua(FormSubmitContext formSubmitContext)
        {
            Logger.Info(Invariant($"Form {formSubmitContext.FormId} submitted successfully."), this);



            // Prepare fields for Eloqua API
            var eloquaParameters = new List<FormFieldValue>();

            foreach (IViewModel field in formSubmitContext.Fields)
            {
                //Text field type
                if (field.TemplateId == Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.InputTemplateId") ||
                    field.TemplateId == Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.EmailTemplateId") ||
                    field.TemplateId == Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.MultilineTemplateId") ||
                    field.TemplateId == Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.CheckboxTemplateId")
                    )
                {
                    string fieldValue = GetValue(field);

                    eloquaParameters.Add(new FormFieldValue { Name = field.Name, Value = fieldValue });
                }

                //Dropdown field type
                if (field.TemplateId ==
                    Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.DropdownTemplateId"))
                {
                    string fieldValue = GetSelectedValue(field);

                    eloquaParameters.Add(new FormFieldValue { Name = field.Name, Value = fieldValue });
                }
                // Multicheckbox field type
                if (field.TemplateId ==
                    Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.RadioButtonsTemplateId") ||
                    field.TemplateId == Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.MultiCheckboxesTemplateId"))
                {
                    string fieldValue = GetSelectedValues(field);

                    eloquaParameters.Add(new FormFieldValue { Name = field.Name, Value = fieldValue });
                }

                //Hidden field type
                if (field.TemplateId == Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.HiddenTemplateId"))
                {
                    //if the field is called "submissionurl", send the page url to Eloqua
                    if (field.Name.ToLower().Equals("submissionurl"))
                    {
                        string pageUrl = System.Web.HttpContext.Current.Request.Headers.GetValues("Referer").FirstOrDefault();
                        eloquaParameters.Add(new FormFieldValue { Name = field.Name, Value = pageUrl });
                    }
                    else
                    {
                        string fieldValue = GetValue(field);
                        eloquaParameters.Add(new FormFieldValue { Name = field.Name, Value = fieldValue });
                    }
                }

                // Tracking field type
                if (field.TemplateId == Sitecore.Configuration.Settings.GetSetting("Eloqua.SitecoreFormsField.EloquaTrackingScriptsTemplateId"))
                {
                    if (field is EloquaTrackingScriptsViewModel)
                    {
                        EloquaTrackingScriptsViewModel trackingScriptsViewModel = field as EloquaTrackingScriptsViewModel;
                        eloquaParameters.Add(new FormFieldValue() { Name = "elqFormName", Value = trackingScriptsViewModel.EloquaFormName });
                        eloquaParameters.Add(new FormFieldValue() { Name = "elqSiteId", Value = trackingScriptsViewModel.EloquaSiteId });
                        eloquaParameters.Add(new FormFieldValue() { Name = "elqCustomerGUID", Value = trackingScriptsViewModel.CustomerGUID });
                        eloquaParameters.Add(new FormFieldValue() { Name = "elqCookieWrite", Value = "0" });
                    }
                }
            }

            var response = EloquaCloudService.PostForm(null, eloquaParameters.ToArray());

            return true;
        }

        private static string GetValue(object field)
        {
            return field?.GetType().GetProperty("Value")?.GetValue(field, null)?.ToString() ?? string.Empty;
        }

        private static string GetSelectedValue(object field)
        {
            string selectedValue = string.Empty;
            if (field is DropDownListViewModel)
            {
                DropDownListViewModel dropdownField = field as DropDownListViewModel;
                selectedValue = dropdownField.Items.FirstOrDefault(a => a.Selected)?.Value;
            }

            return selectedValue;
        }

        private static string GetSelectedValues(object field)
        {
            string selectedValue = string.Empty;
            if (field is Sitecore.ExperienceForms.Mvc.Models.Fields.ListViewModel)
            {
                ListViewModel listField = field as ListViewModel;
                selectedValue = string.Join("|", listField.Items.Where(a => a.Selected).Select(a => a.Value).ToList());
            }

            return selectedValue;
        }

    }
}
