using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitecoreModules.Feature.EloquaForms.Services;

namespace SitecoreModules.Feature.EloquaForms.Factories
{
    /// <summary>
    /// Type
    /// Form: It sends data usting a normal url-encoded post call. It will create a Eloqua Cookie for Prefill Feature.
    /// RestApi: It sends data in json format using Reset API. The limitation is not compatible with Eloqua Prefill feature.
    /// </summary>
    public enum ImplementationType
    {
        Form,
        RestApi
    }
    /// <summary>
    /// Creates instances of <see cref="IEloquaCloudService"/>.
    /// </summary>
    public static class EloquaCloudServiceFactory
    {
        /// <summary>
        /// Builds instances of IEloquaCloudService();
        /// </summary>
        public static IEloquaCloudService Build(ImplementationType type = ImplementationType.Form)
        {
            switch (type)
            {
                case ImplementationType.Form:
                    return new FormEloquaCloudService();
                case ImplementationType.RestApi:
                    return new RestApiEloquaCloudService();
                default:
                    return new FormEloquaCloudService();
            }
            
        }

    }
}
