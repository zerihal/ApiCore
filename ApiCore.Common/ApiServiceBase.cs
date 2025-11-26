using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace ApiCore.Common
{
    public abstract class ApiServiceBase
    {
        /// <summary>
        /// Gets API module assembly.
        /// </summary>
        /// <returns>API module assembly.</returns>
        public Assembly GetModuleAssembly() => GetType().Assembly;

        /// <summary>
        /// Gets all controllers within this API module.
        /// </summary>
        /// <param name="assembly">API module assembly.</param>
        /// <returns>Collection of controllers (types).</returns>
        protected virtual IEnumerable<Type> GetControllers(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && (t.Name.EndsWith("Controller") ||
                     t.GetCustomAttributes(typeof(ApiControllerAttribute), true).Any()));
        }

        /// <summary>
        /// Gets endpoints info for the specified controller.
        /// </summary>
        /// <param name="controller">Controller (type).</param>
        /// <returns>Collection of <see cref="EndpointInfo"/> for the controller.</returns>
        protected virtual IEnumerable<EndpointInfo> GetEndpoints(Type controller)
        {
            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            foreach (var method in methods)
            {
                // Find attributes like [HttpGet], [HttpPost], etc
                var httpMethodAttr = method
                    .GetCustomAttributes()
                    .FirstOrDefault(attr => attr is HttpMethodAttribute) as HttpMethodAttribute;

                if (httpMethodAttr == null)
                    continue; // skip non-API methods

                var httpMethod = httpMethodAttr.HttpMethods.FirstOrDefault() ?? "UNKNOWN";
                var route = httpMethodAttr.Template ?? ""; // e.g. "searchdocs"

                yield return new EndpointInfo
                {
                    Controller = controller.Name,
                    Method = method.Name,
                    HttpMethod = httpMethod,
                    Route = route,
                    Parameters = method.GetParameters()
                        .Select(p => $"{p.ParameterType.Name} {p.Name}")
                        .ToList(),
                    ReturnType = method.ReturnType.Name
                };
            }
        }
    }
}
