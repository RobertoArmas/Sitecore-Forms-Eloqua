using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Feature.EloquaForms.Models.Forms;

namespace Feature.EloquaForms.Services
{
    /// <summary>
    /// Connects to the Eloqua cloud service.
    /// </summary>
    public interface IEloquaCloudService
    {
        /// <summary>
        /// Posts Form data to Eloqua.
        /// </summary>
        PostFormBody PostForm(string formId, params FormFieldValue[] fields);
    }
}
