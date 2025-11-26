using ApiCore.Common;
using ApiCore.Common.Interfaces;

namespace ApiCore.DocAnalysis.Services
{
    public class DocAnalysisService : ApiServiceBase, IApiModule
    {
        private EndpointInfo[]? _apiCalls;

        /// <inheritdoc/>
        public EndpointInfo[] GetApiEndpoints()
        {
            var controllers = GetControllers(GetModuleAssembly);

            if (_apiCalls == null)
                _apiCalls = controllers.SelectMany(GetEndpoints).ToArray();

            return _apiCalls;
        }
    }
}
