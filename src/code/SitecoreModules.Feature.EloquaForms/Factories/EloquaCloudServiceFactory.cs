using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitecoreModules.Feature.EloquaForms.Services;

namespace SitecoreModules.Feature.EloquaForms.Factories
{
    /// <summary>
    /// Creates instances of <see cref="IEloquaCloudService"/>.
    /// </summary>
    public static class EloquaCloudServiceFactory
    {
        /// <summary>
        /// Builds instances of IEloquaCloudService();
        /// </summary>
        public static IEloquaCloudService Build()
        {
            return new FormEloquaCloudService();
        }

    }
}
